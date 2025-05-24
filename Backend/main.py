from flask import Flask, jsonify, request
import mysql.connector

app = Flask(__name__)


def createDatabaseIfNotExists():
    conn = mysql.connector.connect(
        host="localhost",
        user="root",
        password="Mine@7137"
    )
    cursor = conn.cursor()
    cursor.execute("CREATE DATABASE IF NOT EXISTS gard")
    conn.commit()
    conn.close()
    print("Database 'gard' checked/created")


def connectDatabase():
    conn = mysql.connector.connect(
        host="localhost",
        user="root",
        password="Mine@7137",
        database="gard"
    )
    print("Connected to 'gard' database")
    return conn


def createTableIfNotExists():
    conn = connectDatabase()
    cursor = conn.cursor()
    cursor.execute("""
    CREATE TABLE IF NOT EXISTS subscribers (
        id INT PRIMARY KEY AUTO_INCREMENT,
        email VARCHAR(255) UNIQUE NOT NULL,
        name VARCHAR(255),
        status ENUM('active', 'inactive', 'bounced') DEFAULT 'active',
        subscribed_at DATETIME DEFAULT CURRENT_TIMESTAMP,
        updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
    )
    """)
    conn.commit()
    conn.close()
    print("Table 'subscribers' checked/created")


@app.route('/subscribers', methods=['GET'])
def getSubscribers():
    conn = connectDatabase()
    cursor = conn.cursor(dictionary=True)
    cursor.execute("SELECT * FROM subscribers")
    data = cursor.fetchall()
    conn.close()
    return jsonify(data if data else [])


@app.route('/subscribers/add', methods=['POST'])
def addSubscriber():
    conn = connectDatabase()
    cursor = conn.cursor()
    data = request.json
    status = data['status']
    name = data['name']
    email = data['email']

    cursor.execute("""
    INSERT INTO subscribers (name, email, status)
    VALUES (%s, %s, %s)
    """, (name, email, status))

    conn.commit()
    conn.close()
    return jsonify({"message": "Subscriber added successfully!"}), 201

@app.route('/subscribers/update', methods=['PUT'])
def updateSubscriber():
    data = request.json
    subscriber_id = data['id']
    name = data['name']
    email = data['email']
    status = data['status']

    conn = connectDatabase()
    cursor = conn.cursor()

    cursor.execute("""
        UPDATE subscribers
        SET name = %s,
            email = %s,
            status = %s
        WHERE id = %s
    """, (name, email, status, subscriber_id))

    conn.commit()
    conn.close()

    return jsonify({"message": "Subscriber updated successfully"})


@app.route('/subscribers/delete/<int:id>', methods=['DELETE'])
def deleteSubscriber(id):
    conn = connectDatabase()
    cursor = conn.cursor()
    cursor.execute("DELETE FROM subscribers WHERE id = %s", (id,))
    conn.commit()
    conn.close()
    return jsonify({"message": "Subscriber deleted successfully!"})


createDatabaseIfNotExists()
createTableIfNotExists()

if __name__ == '__main__':
    app.run(port=5000, debug=True)
