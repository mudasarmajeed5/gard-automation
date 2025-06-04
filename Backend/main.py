from flask import Flask, jsonify, request
import mysql.connector
from __init__db import initdb

app = Flask(__name__)

def connectDatabase():
    """Connect to the gard database"""
    conn = mysql.connector.connect(
        host="localhost",
        user="root",
        password="Mine@7137",
        database="gard"
    )
    return conn

@app.route('/subscribers', methods=['GET'])
def getSubscribers():
    conn = connectDatabase()
    cursor = conn.cursor(dictionary=True)
    cursor.execute("SELECT * FROM Subscribers")
    data = cursor.fetchall()
    conn.close()
    return jsonify(data if data else [])

@app.route('/subscribers/add', methods=['POST'])
def addSubscriber():
    conn = connectDatabase()
    cursor = conn.cursor()
    data = request.json
    status = data.get('status', 'active')
    name = data.get('name')
    email = data.get('email')

    if not email:
        return jsonify({"error": "Email is required"}), 400

    try:
        cursor.execute("""
        INSERT INTO Subscribers (name, email, status)
        VALUES (%s, %s, %s)
        """, (name, email, status))
        
        conn.commit()
        conn.close()
        return jsonify({"message": "Subscriber added successfully!"}), 201
    except mysql.connector.IntegrityError:
        conn.close()
        return jsonify({"error": "Email already exists"}), 409

@app.route('/subscribers/update', methods=['PUT'])
def updateSubscriber():
    data = request.json
    subscriber_id = data.get('id')
    name = data.get('name')
    email = data.get('email')
    status = data.get('status')

    if not subscriber_id:
        return jsonify({"error": "ID is required"}), 400

    conn = connectDatabase()
    cursor = conn.cursor()

    try:
        cursor.execute("""
            UPDATE Subscribers
            SET name = %s,
                email = %s,
                status = %s
            WHERE id = %s
        """, (name, email, status, subscriber_id))

        if cursor.rowcount == 0:
            conn.close()
            return jsonify({"error": "Subscriber not found"}), 404

        conn.commit()
        conn.close()
        return jsonify({"message": "Subscriber updated successfully"})
    except mysql.connector.IntegrityError:
        conn.close()
        return jsonify({"error": "Email already exists"}), 409

@app.route('/subscribers/delete/<int:id>', methods=['DELETE'])
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

# Initialize database when the app starts
print("Initializing database...")
success = initdb(host="localhost", user="root", password="Mine@7137")
if success:
    print("Database initialization completed!")
else:
    print("Database initialization failed!")

if __name__ == '__main__':
    app.run(port=5000, debug=True)