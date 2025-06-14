from flask import Blueprint, jsonify, request
from __init__db import connectDatabase
import smtplib
import re
import traceback
import ssl
from email.mime.text import MIMEText
from email.mime.multipart import MIMEMultipart
smtp_bp = Blueprint("smtp", __name__, url_prefix="/smtp")

@smtp_bp.route("/get/<int:admin_id>", methods=["GET"])
def get_smtp_settings(admin_id):
    conn = connectDatabase()
    cursor = conn.cursor(dictionary=True)

    cursor.execute("SELECT * FROM SmtpSettings WHERE admin_id = %s", (admin_id,))
    smtp_settings = cursor.fetchone()
    conn.close()

    if smtp_settings:
        smtp_settings["smtp_ssl"] = bool(smtp_settings["smtp_ssl"])
        return jsonify(smtp_settings), 200
    else:
        return jsonify({"Message": "Configure your SMTP Settings"}), 404
    
@smtp_bp.route("/save", methods=["POST"])
def save_smtp_settings():
    conn = connectDatabase()
    cursor = conn.cursor()
    data = request.json
    admin_id = data.get("admin_id")

    if admin_id == -1:
        conn.close()
        return jsonify({"error": "Please Login First"}), 401

    smtp_email = data.get("smtp_email")
    smtp_password = data.get("smtp_password")
    smtp_server = data.get("smtp_server")
    smtp_port = data.get("smtp_port")
    smtp_ssl = data.get("smtp_ssl", False)

    if (
        not smtp_email
        or not smtp_password
        or not smtp_server
        or not smtp_port
        or not admin_id
    ):
        conn.close()
        return jsonify({"error": "Missing required SMTP fields"}), 400

    try:
        cursor.execute("SELECT id FROM SmtpSettings WHERE admin_id = %s", (admin_id,))
        existing_record = cursor.fetchone()

        if existing_record:
            cursor.execute(
                """
                UPDATE SmtpSettings 
                SET smtp_email = %s, smtp_password = %s, smtp_server = %s, 
                    smtp_port = %s, smtp_ssl = %s 
                WHERE admin_id = %s
                """,
                (smtp_email, smtp_password, smtp_server, smtp_port, smtp_ssl, admin_id),
            )
            message = "SMTP settings updated successfully!"
        else:
            cursor.execute(
                """
                INSERT INTO SmtpSettings (admin_id, smtp_email, smtp_password, smtp_server, smtp_port, smtp_ssl)
                VALUES (%s, %s, %s, %s, %s, %s)
                """,
                (admin_id, smtp_email, smtp_password, smtp_server, smtp_port, smtp_ssl),
            )
            message = "SMTP settings saved successfully!"

        conn.commit()
        conn.close()
        return jsonify({"message": message}), 200

    except Exception as e:
        conn.rollback()
        conn.close()
        return jsonify({"error": str(e)}), 500

smtp_cache = {}

@smtp_bp.route("/send/<email>", methods=["POST"])
def send_email(email):
    try:
        data = request.get_json()
        if not data:
            return jsonify({"error": "No data provided"}), 400

        admin_id = data.get('admin_id')
        subscriber_id = data.get('subscriber_id')
        campaign_id = data.get('campaign_id')
        subject = data.get('subject')
        body = data.get('body')

        if not all([admin_id, subscriber_id, campaign_id, subject, body]):
            return jsonify({"error": "Missing required fields"}), 400

        email_pattern = r'^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$'
        if not re.match(email_pattern, email):
            return jsonify({"error": "Invalid email format"}), 400

        # Fetch SMTP settings from cache or database
        if admin_id not in smtp_cache:
            conn = connectDatabase()
            cursor = conn.cursor(dictionary=True)
            cursor.execute("SELECT * FROM SmtpSettings WHERE admin_id = %s", (admin_id,))
            smtp_settings = cursor.fetchone()
            conn.close()

            if not smtp_settings:
                return jsonify({"error": "SMTP settings not configured for this admin"}), 404

            smtp_cache[admin_id] = smtp_settings
        else:
            smtp_settings = smtp_cache[admin_id]

        smtp_email = smtp_settings['smtp_email']
        smtp_password = smtp_settings['smtp_password']
        smtp_server = smtp_settings['smtp_server']
        smtp_port = smtp_settings['smtp_port']
        smtp_ssl = bool(smtp_settings['smtp_ssl'])

        print(f"Sending email to: {email} using server: {smtp_server}:{smtp_port} (SSL: {smtp_ssl})")

        # Build the email
        message = MIMEMultipart()
        message["From"] = smtp_email
        message["To"] = email
        message["Subject"] = subject
        message.attach(MIMEText(body, "html"))

        # Connect and send
        if smtp_ssl and smtp_port == 465:
            context = ssl.create_default_context()
            server = smtplib.SMTP_SSL(smtp_server, smtp_port, context=context)
        elif not smtp_ssl and smtp_port == 587:
            server = smtplib.SMTP(smtp_server, smtp_port)
            server.ehlo()
            server.starttls()
        else:
            return jsonify({
                "error": "Invalid SMTP configuration.",
                "hint": "Use port 465 with SSL or port 587 without SSL (TLS). Please update your SMTP settings."
            }), 400

        server.login(smtp_email, smtp_password)
        server.sendmail(smtp_email, email, message.as_string())
        server.quit()

        # Log success in EmailLogs
        conn = connectDatabase()
        cursor = conn.cursor()
        cursor.execute("""
            INSERT INTO EmailLogs (admin_id, subscriber_id, campaign_id, status)
            VALUES (%s, %s, %s, %s)
        """, (admin_id, subscriber_id, campaign_id, 'sent'))
        conn.commit()
        conn.close()

        return jsonify({
            "message": "Email sent successfully",
            "recipient": email,
            "subject": subject
        }), 200

    except smtplib.SMTPAuthenticationError:
        return jsonify({"error": "SMTP Authentication failed. Check email and password."}), 401

    except smtplib.SMTPRecipientsRefused:
        return jsonify({"error": f"Recipient refused: {email}"}), 400

    except smtplib.SMTPServerDisconnected:
        return jsonify({"error": "SMTP server disconnected unexpectedly"}), 500

    except smtplib.SMTPException as e:
        return jsonify({"error": f"SMTP error occurred: {str(e)}"}), 500

    except Exception as e:
        # Log the full traceback to console for debugging
        print("Unexpected error occurred:")
        traceback.print_exc()
        return jsonify({"error": f"An error occurred: {str(e)}"}), 500




    
    
# email logs
@smtp_bp.route("/email-logs/<int:admin_id>", methods=["GET"])
def get_email_logs(admin_id):
    conn = connectDatabase()
    cursor = conn.cursor(dictionary=True)

    query = """
        SELECT 
            el.id,
            el.admin_id,
            el.subscriber_id,
            s.email AS subscriber_email,
            ec.campaign_name,
            el.status,
            el.sent_at
        FROM EmailLogs el
        JOIN Subscribers s ON el.subscriber_id = s.id
        JOIN EmailCampaigns ec ON el.campaign_id = ec.id
        WHERE el.admin_id = %s
        ORDER BY el.sent_at DESC
    """

    cursor.execute(query, (admin_id,))
    data = cursor.fetchall()

    conn.close()
    return jsonify(data if data else [])
