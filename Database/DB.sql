CREATE DATABASE IF NOT EXISTS gard;
USE gard;

CREATE TABLE IF NOT EXISTS subscribers (
    id INT PRIMARY KEY AUTO_INCREMENT,
    email VARCHAR(255) UNIQUE NOT NULL,
    name VARCHAR(255),
    status ENUM('active', 'inactive','bounced') DEFAULT 'active',
    subscribed_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);


-- Table: EmailCampaigns
CREATE TABLE IF NOT EXISTS EmailCampaigns (
    id INT PRIMARY KEY AUTO_INCREMENT,
    campaign_name VARCHAR(255) NOT NULL,
    content TEXT NOT NULL,
    sent_at DATETIME
);

-- Table: EmailLogs
CREATE TABLE IF NOT EXISTS EmailLogs (
    id INT PRIMARY KEY AUTO_INCREMENT,
    subscriber_id INT NOT NULL,
    campaign_id INT NOT NULL,
    status VARCHAR(10) DEFAULT 'sent',
    sent_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (subscriber_id) REFERENCES Subscribers(id) ON DELETE CASCADE,
    FOREIGN KEY (campaign_id) REFERENCES EmailCampaigns(id) ON DELETE CASCADE
);

-- Table: Admins
CREATE TABLE IF NOT EXISTS Admins (
    id INT PRIMARY KEY AUTO_INCREMENT,
    username VARCHAR(255) UNIQUE NOT NULL,
    password VARCHAR(255) NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Table: CampaignStats
CREATE TABLE IF NOT EXISTS CampaignStats (
    id INT PRIMARY KEY AUTO_INCREMENT,
    campaign_id INT NOT NULL,
    total_sent INT DEFAULT 0,
    total_opened INT DEFAULT 0,
    total_clicked INT DEFAULT 0,
    FOREIGN KEY (campaign_id) REFERENCES EmailCampaigns(id) ON DELETE CASCADE
);
