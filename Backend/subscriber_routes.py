from flask import Blueprint, jsonify, request
from __init__db import connectDatabase
import mysql.connector
subscriber_bp = Blueprint("subscribers",__name__,url_prefix="/subscribers")



@subscriber_bp.route("/<int:admin_id>", methods=["GET"])
def getSubscribers(admin_id):
    conn = connectDatabase()
    cursor = conn.cursor(dictionary=True)
    cursor.execute("SELECT * FROM Subscribers WHERE admin_id = %s", (admin_id,))
    data = cursor.fetchall()
    conn.close()
    return jsonify(data if data else [])


@subscriber_bp.route("/add", methods=["POST"])
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


@subscriber_bp.route("/update", methods=["PUT"])
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


@subscriber_bp.route("/delete/<int:id>", methods=["DELETE"])
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


