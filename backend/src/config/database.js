/**
 * PostgreSQLデータベース接続設定
 */

const { Pool } = require('pg');
const logger = require('../utils/logger');

const pool = new Pool({
  host: process.env.DB_HOST || 'localhost',
  port: parseInt(process.env.DB_PORT) || 5432,
  database: process.env.DB_NAME || 'shaderop_db',
  user: process.env.DB_USER || 'shaderop_user',
  password: process.env.DB_PASSWORD,
  max: 20, // 最大接続数
  idleTimeoutMillis: 30000,
  connectionTimeoutMillis: 2000,
});

// 接続テスト
pool.on('connect', () => {
  logger.info('PostgreSQLに接続しました');
});

pool.on('error', (err) => {
  logger.error('PostgreSQL接続エラー:', err);
  process.exit(-1);
});

/**
 * クエリ実行ヘルパー
 */
async function query(text, params) {
  const start = Date.now();
  try {
    const result = await pool.query(text, params);
    const duration = Date.now() - start;

    logger.debug('クエリ実行', {
      query: text,
      duration: `${duration}ms`,
      rows: result.rowCount,
    });

    return result;
  } catch (error) {
    logger.error('クエリエラー:', {
      query: text,
      error: error.message,
    });
    throw error;
  }
}

/**
 * トランザクション実行ヘルパー
 */
async function transaction(callback) {
  const client = await pool.connect();

  try {
    await client.query('BEGIN');
    const result = await callback(client);
    await client.query('COMMIT');
    return result;
  } catch (error) {
    await client.query('ROLLBACK');
    throw error;
  } finally {
    client.release();
  }
}

module.exports = {
  pool,
  query,
  transaction,
};
