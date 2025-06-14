import mysql.connector
from mysql.connector import Error

def connectDatabase():
    return mysql.connector.connect(
        host="localhost", user="root", password="Mine@7137", database="gard"
    )

def initdb():
    """
    Initialize the gard database with all required tables.

    Args:
        host (str): MySQL host (default: localhost)
        user (str): MySQL username (default: root)
        password (str): MySQL password (default: empty)

    Returns:
        bool: True if successful, False otherwise
    """
    connection = None
    cursor = None

    try:
        # Connect to MySQL server
        connection = connectDatabase()

        if connection.is_connected():
            cursor = connection.cursor()
            print("Connected to MySQL server")

            # Create database
            cursor.execute("CREATE DATABASE IF NOT EXISTS gard")
            print("Database 'gard' created successfully")

            # Use the database
            cursor.execute("USE gard")

            # Create campaigns table
            cursor.execute("""
                CREATE TABLE IF NOT EXISTS campaigns (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    name VARCHAR(255) NOT NULL,
                    content TEXT,
                    status VARCHAR(100) DEFAULT 'Draft',
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                )
            """)
            print("Table 'campaigns' created successfully.")

            # SQL commands to create other tables
            sql_commands = [
                """
                CREATE TABLE IF NOT EXISTS Admins (
                    id INT PRIMARY KEY AUTO_INCREMENT,
                    username VARCHAR(255) UNIQUE NOT NULL,
                    password VARCHAR(255) NOT NULL,
                    email VARCHAR(255) UNIQUE NOT NULL,
                    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
                )
                """,
                """
                CREATE TABLE IF NOT EXISTS Subscribers (
                    id INT PRIMARY KEY AUTO_INCREMENT,
                    email VARCHAR(255) UNIQUE NOT NULL,
                    admin_id INT NOT NULL,
                    name VARCHAR(255),
                    status ENUM('active', 'inactive','bounced') DEFAULT 'active',
                    subscribed_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                    FOREIGN KEY (admin_id) REFERENCES Admins(id) ON DELETE CASCADE
                )
                """,
                """
                CREATE TABLE IF NOT EXISTS EmailCampaigns (
                    id INT PRIMARY KEY AUTO_INCREMENT,
                    admin_id INT NOT NULL,
                    campaign_name VARCHAR(255) NOT NULL,
                    content TEXT NOT NULL,
                    sent_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (admin_id) REFERENCES Admins(id) ON DELETE CASCADE
                )
                """,
                """
                CREATE TABLE IF NOT EXISTS EmailConfig (
                    id INT PRIMARY KEY AUTO_INCREMENT,
                    admin_id INT UNIQUE NOT NULL,
                    email VARCHAR(255) NOT NULL,
                    app_password VARCHAR(255) NOT NULL,
                    smtp_server VARCHAR(255) NOT NULL,
                    port INT NOT NULL,
                    tls BOOLEAN DEFAULT TRUE,
                    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                    FOREIGN KEY (admin_id) REFERENCES Admins(id) ON DELETE CASCADE
                )
                """,
                """
                CREATE TABLE IF NOT EXISTS EmailLogs (
                    id INT PRIMARY KEY AUTO_INCREMENT,
                    subscriber_id INT NOT NULL,
                    admin_id INT NOT NULL,
                    campaign_id INT NOT NULL,
                    status VARCHAR(10) DEFAULT 'sent',
                    sent_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (admin_id) REFERENCES Admins(id) ON DELETE CASCADE,
                    FOREIGN KEY (subscriber_id) REFERENCES Subscribers(id) ON DELETE CASCADE,
                    FOREIGN KEY (campaign_id) REFERENCES EmailCampaigns(id) ON DELETE CASCADE
                )
                """,
                """
                CREATE TABLE IF NOT EXISTS CampaignStats (
                    id INT PRIMARY KEY AUTO_INCREMENT,
                    campaign_id INT NOT NULL,
                    total_sent INT DEFAULT 0,
                    total_opened INT DEFAULT 0,
                    total_clicked INT DEFAULT 0,
                    FOREIGN KEY (campaign_id) REFERENCES EmailCampaigns(id) ON DELETE CASCADE
                )
                """,
                """
                CREATE TABLE IF NOT EXISTS SmtpSettings (
                    id INT PRIMARY KEY AUTO_INCREMENT,
                    admin_id INT UNIQUE NOT NULL,
                    smtp_email VARCHAR(255) NOT NULL,
                    smtp_password VARCHAR(255) NOT NULL,
                    smtp_server VARCHAR(255) NOT NULL,
                    smtp_port INT NOT NULL,
                    smtp_ssl BOOLEAN DEFAULT FALSE,
                    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                    FOREIGN KEY (admin_id) REFERENCES Admins(id) ON DELETE CASCADE
                )
                """
            ]

            table_names = [
                "Subscribers",
                "Admins",
                "EmailCampaigns",
                "EmailConfig",
                "EmailLogs",
                "CampaignStats",
                "SmtpSettings",
            ]

            for i, command in enumerate(sql_commands):
                cursor.execute(command)
                print(f"Table '{table_names[i]}' created successfully")

            connection.commit()
            print("All tables created successfully!")
            return True

    except Error as e:
        print(f"Error: {e}")
        return False

    finally:
        if cursor:
            cursor.close()
        if connection and connection.is_connected():
            connection.close()
            print("MySQL connection closed")
