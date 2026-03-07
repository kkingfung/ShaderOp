/**
 * Redis接続設定
 * セッション管理とキャッシュ用
 */

const redis = require('redis');
const logger = require('../utils/logger');

const redisClient = redis.createClient({
  host: process.env.REDIS_HOST || 'localhost',
  port: parseInt(process.env.REDIS_PORT) || 6379,
  password: process.env.REDIS_PASSWORD || undefined,
  database: parseInt(process.env.REDIS_DB) || 0,
});

redisClient.on('connect', () => {
  logger.info('Redisに接続しました');
});

redisClient.on('error', (err) => {
  logger.error('Redis接続エラー:', err);
});

// Redis接続を開始
redisClient.connect();

/**
 * オンラインステータス管理
 */
async function setUserOnline(userId) {
  const key = `user:${userId}:online`;
  await redisClient.set(key, '1', {
    EX: 300, // 5分間有効
  });
}

async function setUserOffline(userId) {
  const key = `user:${userId}:online`;
  await redisClient.del(key);
}

async function isUserOnline(userId) {
  const key = `user:${userId}:online`;
  const result = await redisClient.get(key);
  return result === '1';
}

/**
 * セッション管理
 */
async function setSession(sessionId, data, expirySeconds = 86400) {
  const key = `session:${sessionId}`;
  await redisClient.set(key, JSON.stringify(data), {
    EX: expirySeconds,
  });
}

async function getSession(sessionId) {
  const key = `session:${sessionId}`;
  const data = await redisClient.get(key);
  return data ? JSON.parse(data) : null;
}

async function deleteSession(sessionId) {
  const key = `session:${sessionId}`;
  await redisClient.del(key);
}

/**
 * キャッシュ管理
 */
async function setCache(key, value, expirySeconds = 300) {
  await redisClient.set(`cache:${key}`, JSON.stringify(value), {
    EX: expirySeconds,
  });
}

async function getCache(key) {
  const data = await redisClient.get(`cache:${key}`);
  return data ? JSON.parse(data) : null;
}

async function deleteCache(key) {
  await redisClient.del(`cache:${key}`);
}

async function clearCachePattern(pattern) {
  const keys = await redisClient.keys(`cache:${pattern}`);
  if (keys.length > 0) {
    await redisClient.del(keys);
  }
}

module.exports = {
  redisClient,
  setUserOnline,
  setUserOffline,
  isUserOnline,
  setSession,
  getSession,
  deleteSession,
  setCache,
  getCache,
  deleteCache,
  clearCachePattern,
};
