import mysql.connector
from mysql.connector import Error

def initdb(host='localhost', user='root', password='Mine@7137', port=3306):
    """
    Initialize the gard database with all required tables.
    
    Args:
        host (str): MySQL host (default: localhost)
        user (str): MySQL username (default: root)
        password (str): MySQL password (default: empty)
        port (int): MySQL port (default: 3306)
    
    Returns:
        bool: True if successful, False otherwise
    """
    
    connection = None
    cursor = None
    
    try:
        # Connect to MySQL server
        connection = mysql.connector.connect(
            host=host,
            user=user,
            password=password,
            port=port
        )
        
        if connection.is_connected():
            cursor = connection.cursor()
            print("Connected to MySQL server")
            
            # Create database
            cursor.execute("CREATE DATABASE IF NOT EXISTS gard")
            print("Database 'gard' created successfully")
            
            # Use the database
            cursor.execute("USE gard")
            
            # SQL commands to create tables
            sql_commands = [
                """
                CREATE TABLE IF NOT EXISTS Subscribers (
                    id INT PRIMARY KEY AUTO_INCREMENT,
                    email VARCHAR(255) UNIQUE NOT NULL,
                    name VARCHAR(255),
                    status ENUM('active', 'inactive','bounced') DEFAULT 'active',
                    subscribed_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
                )
                """,
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
                CREATE TABLE IF NOT EXISTS EmailCampaigns (
                    id INT PRIMARY KEY AUTO_INCREMENT,
                    campaign_name VARCHAR(255) NOT NULL,
                    content TEXT NOT NULL,
                    sent_at DATETIME
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
                    campaign_id INT NOT NULL,
                    status VARCHAR(10) DEFAULT 'sent',
                    sent_at DATETIME DEFAULT CURRENT_TIMESTAMP,
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
                """
            ]
            
            # Execute each SQL command
            for i, command in enumerate(sql_commands):
                cursor.execute(command)
                table_names = ['Subscribers', 'Admins', 'EmailCampaigns', 'EmailConfig', 'EmailLogs', 'CampaignStats']
                print(f"Table '{table_names[i]}' created successfully")
            
            # Commit the changes
            connection.commit()
            print("All tables created successfully!")
            return True
            
    except Error as e:
        print(f"Error: {e}")
        return False
        
    finally:
        # Close cursor and connection
        if cursor:
            cursor.close()
        if connection and connection.is_connected():
            connection.close()
            print("MySQL connection closed")