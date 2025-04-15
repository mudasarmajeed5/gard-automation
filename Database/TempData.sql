-- 1. Create the database if it doesn't exist
CREATE DATABASE IF NOT EXISTS email_automation;
USE email_automation;

-- 2. Drop existing table if it exists (clean start)
DROP TABLE IF EXISTS subscribers;

-- 3. Create new table structure with simplified fields
CREATE TABLE subscribers (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    subscribed_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    status ENUM('active', 'inactive', 'pending') DEFAULT 'pending',
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- 4. Insert initial 5 records
INSERT INTO subscribers (name, email, subscribed_at, status) VALUES
('Alice Johnson', 'alice.j@example.com', '2023-11-01 09:00:00', 'active'),
('Bob Smith', 'bob.smith@example.com', '2023-11-02 10:30:00', 'active'),
('Charlie Brown', 'charlie@example.com', '2023-11-03 11:45:00', 'inactive'),
('Diana Prince', 'diana@example.com', '2023-11-04 14:15:00', 'pending'),
('Evan Wright', 'evan.w@example.com', '2023-11-05 16:30:00', 'active');

-- 5. Add 10 more records (15 total)
INSERT INTO subscribers (name, email, subscribed_at, status) VALUES
('Emma Watson', 'emma.w@example.com', '2023-11-06 08:15:00', 'active'),
('Michael Scott', 'm.scott@example.com', '2023-11-07 09:45:00', 'inactive'),
('Olivia Parker', 'olivia.p@example.com', '2023-11-08 11:20:00', 'active'),
('Peter Quill', 'starlord@example.com', '2023-11-09 13:10:00', 'pending'),
('Rachel Green', 'rachel.g@example.com', '2023-11-10 15:30:00', 'active'),
('Steve Rogers', 'captain@example.com', '2023-11-11 17:45:00', 'inactive'),
('Tony Stark', 'ironman@example.com', '2023-11-12 19:00:00', 'active'),
('Natasha Romanoff', 'blackwidow@example.com', '2023-11-13 20:15:00', 'pending'),
('Bruce Banner', 'hulk@example.com', '2023-11-14 21:30:00', 'active'),
('Wanda Maximoff', 'scarletwitch@example.com', '2023-11-15 22:45:00', 'inactive');

-- 6. Verify the data
SELECT COUNT(*) AS total_subscribers FROM subscribers;
SELECT * FROM subscribers ORDER BY subscribed_at DESC LIMIT 5;