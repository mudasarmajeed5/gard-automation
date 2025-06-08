from flask import Flask, jsonify, request
import mysql.connector
from __init__db import initdb,connectDatabase
from smtp_routes import smtp_bp
app = Flask(__name__)

app.register_blueprint(smtp_bp)
@app.route("/signup", methods=["POST"])
def signup():
    data = request.get_json()
    username = data.get("user_sign")
    email = data.get("email_sign")
    password = data.get("pass_sign")

    if not all([username, email, password]):
        return jsonify({"error": "Missing required fields"}), 400

    try:
        conn = connectDatabase()
        cursor = conn.cursor()
        cursor.execute(
            """
            INSERT INTO admins (username, email, password)
            VALUES (%s, %s, %s)
        """,
            (username, email, password),
        )
        conn.commit()
        return jsonify({"message": "Signup successful"}), 201
    except mysql.connector.Error as e:
        print("SIGNUP ERROR:", e)
        return jsonify({"error": "Database error"}), 500
    finally:
        conn.close()


@app.route("/admins/login", methods=["POST"])
def admin_login():
    data = request.get_json()
    username = data.get("username", "").strip()
    password = data.get("password", "").strip()

    if not username or not password:
        return jsonify(
            {"success": False, "message": "Username and password are required"}
        ), 400

    try:
        conn = connectDatabase()
        cursor = conn.cursor(dictionary=True)
        cursor.execute("SELECT * FROM admins WHERE username = %s", (username,))
        admin = cursor.fetchone()
        conn.close()

        if admin:
            db_password = admin["password"].strip()
            admin_id = admin["id"]

            if password == db_password:
                return jsonify({"admin_id": admin_id}), 200

            else:
                return jsonify(
                    {"success": False, "message": "Invalid username or password"}
                ), 401
        else:
            return jsonify(
                {"success": False, "message": "Invalid username or password"}
            ), 401

    except Exception as e:
        print("LOGIN ERROR:", e)
        return jsonify({"success": False, "message": "Internal server error"}), 500


@app.route("/subscribers/<int:admin_id>", methods=["GET"])
def getSubscribers(admin_id):
    conn = connectDatabase()
    cursor = conn.cursor(dictionary=True)
    cursor.execute("SELECT * FROM Subscribers WHERE admin_id = %s", (admin_id,))
    data = cursor.fetchall()
    conn.close()
    return jsonify(data if data else [])


@app.route("/subscribers/add", methods=["POST"])
def addSubscriber():
    conn = connectDatabase()
    cursor = conn.cursor()
    data = request.json
    status = data.get("status", "active")
    name = data.get("name")
    email = data.get("email")
    admin_id = data.get("admin_id")

    if not email or not admin_id:
        return jsonify({"error": "Email,AdminID is required"}), 400

    try:
        cursor.execute(
            """
            INSERT INTO Subscribers (admin_id,name, email, status)
            VALUES (%s,%s, %s, %s)
        """,
            (admin_id,name, email, status),
        )
        conn.commit()
        conn.close()
        return jsonify({"message": "Subscriber added successfully!"}), 201
    except mysql.connector.IntegrityError:
        conn.close()
        return jsonify({"error": "Email already exists"}), 409


@app.route("/subscribers/update", methods=["PUT"])
def updateSubscriber():
    data = request.json
    subscriber_id = data.get("id")
    name = data.get("name")
    email = data.get("email")
    status = data.get("status")

    if not subscriber_id:
        return jsonify({"error": "ID is required"}), 400

    conn = connectDatabase()
    cursor = conn.cursor()

    try:
        cursor.execute(
            """
            UPDATE Subscribers
            SET name = %s, email = %s, status = %s
            WHERE id = %s
        """,
            (name, email, status, subscriber_id),
        )

        if cursor.rowcount == 0:
            conn.close()
            return jsonify({"error": "Subscriber not found"}), 404

        conn.commit()
        conn.close()
        return jsonify({"message": "Subscriber updated successfully"})
    except mysql.connector.IntegrityError:
        conn.close()
        return jsonify({"error": "Email already exists"}), 409


@app.route("/subscribers/delete/<int:id>", methods=["DELETE"])
def deleteSubscriber(id):
    conn = connectDatabase()
    cursor = conn.cursor()
    cursor.execute("DELETE FROM Subscribers WHERE id = %s", (id,))

    if cursor.rowcount == 0:
        conn.close()
        return jsonify({"error": "Subscriber not found"}), 404

    conn.commit()
    conn.close()
    return jsonify({"message": "Subscriber deleted successfully!"})



@app.route("/campaigns/<int:admin_id>", methods=["GET"])
def get_campaigns(admin_id):
    conn = connectDatabase()
    cursor = conn.cursor(dictionary=True)
    cursor.execute("SELECT * FROM EmailCampaigns WHERE admin_id = %s", (admin_id,))
    data = cursor.fetchall()
    conn.close()
    return jsonify(data if data else [])


@app.route("/campaigns/add", methods=["POST"])
def add_campaign():
    try:
        data = request.get_json()
        campaign_name = data.get("name")
        admin_id = data.get("admin_id")
        campaign_content = data.get("content")
        if not campaign_name or not campaign_content or not admin_id:
            return jsonify({"error": "Campaign name, content, and admin_id are required"}), 400

        conn = connectDatabase()
        cursor = conn.cursor()

        query = "INSERT INTO EmailCampaigns (admin_id, campaign_name, content) VALUES (%s, %s, %s)"
        values = (admin_id, campaign_name, campaign_content) 

        cursor.execute(query, values)
        conn.commit()
        conn.close()

        return jsonify({"message": "Campaign added successfully"}), 201

    except Exception as e:
        print("INSERT ERROR:", e)
        return jsonify({"error": "Failed to add campaign"}), 500


@app.route("/campaigns/delete/<int:id>", methods=["DELETE"])
def delete_campaign(id):
    conn = connectDatabase()
    cursor = conn.cursor()
    cursor.execute("DELETE FROM EmailCampaigns WHERE id = %s", (id,))

    if cursor.rowcount == 0:
        conn.close()
        return jsonify({"error": "Campaign not found"}), 404

    conn.commit()
    conn.close()
    return jsonify({"message": "Campaign deleted successfully!"})



print("Initializing database...")
success = initdb(host="localhost", user="root", password="Mine@7137")
print(
    "Database initialization completed!"
    if success
    else "Database initialization failed!"
)

if __name__ == "__main__":
    app.run(port=5000, debug=True)
