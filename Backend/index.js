import express from 'express';
import mysql from 'mysql2/promise';
import cors from 'cors';

const app = express();
const PORT = 5000;

// Database configuration with connection pool
const dbConfig = {
  host: 'localhost',
  user: 'root',
  password: 'Mine@7137',
  database: 'email_automation', // Change to your DB name
  waitForConnections: true,
  connectionLimit: 10,
};

// Create connection pool
const pool = mysql.createPool(dbConfig);

app.use(cors());
app.use(express.json());

// API endpoint to get subscribers
app.get('/api/subscribers', async (req, res) => {
    try {
      const connection = await pool.getConnection();
      const [rows] = await connection.query(`
        SELECT 
          id,
          name,
          email,
          subscribed_at as subscribedAt,
          status,
          updated_at as updatedAt
        FROM subscribers
      `);
      connection.release();
      res.json(rows);
    } catch (error) {
      console.error('Database error:', error);
      res.status(500).json({ error: error?.message ?? 'Database error' });
    }
  });
  

// Health check endpoint
app.get('/api/health', (req, res) => {
  res.json({ 
    status: 'OK', 
    timestamp: new Date().toISOString(),
    dbStatus: pool.pool?.connectionLimit ? 'Connected' : 'Disconnected'
  });
});

// Start server
app.listen(PORT, () => {
  console.log(`Server running on http://localhost:${PORT}`);
}).on('error', (error) => {
  console.error('Server failed:', error);
});

// Handle graceful shutdown
process.on('SIGTERM', async () => {
  await pool.end();
  process.exit(0);
});