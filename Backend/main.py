from flask import Flask, jsonify, request
import mysql.connector
from __init__db import initdb,connectDatabase
from smtp_routes import smtp_bp
from subscriber_routes import subscriber_bp
from campaign_routes import campaigns_bp


app = Flask(__name__)
app.register_blueprint(subscriber_bp)
app.register_blueprint(smtp_bp)
app.register_blueprint(campaigns_bp)


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


print("Initializing database...")
success = initdb(host="localhost", user="root", password="Mine@7137")
print(
    "Database initialization completed!"
    if success
    else "Database initialization failed!"
)

if __name__ == "__main__":
    app.run(port=5000, debug=True)
