/**
 * レート制限ミドルウェア
 * APIへの過剰なリクエストを防ぐ
 */

const rateLimit = require('express-rate-limit');

// 一般的なAPIエンドポイント用レート制限
const generalLimiter = rateLimit({
  windowMs: parseInt(process.env.RATE_LIMIT_WINDOW_MS) || 15 * 60 * 1000, // 15分
  max: parseInt(process.env.RATE_LIMIT_MAX_REQUESTS) || 100, // 100リクエスト
  message: {
    success: false,
    error: 'RATE_LIMIT_EXCEEDED',
    message: 'リクエストが多すぎます。しばらく待ってから再試行してください。',
  },
  standardHeaders: true,
  legacyHeaders: false,
});

// 認証エンドポイント用の厳しいレート制限
const authLimiter = rateLimit({
  windowMs: 15 * 60 * 1000, // 15分
  max: 5, // 5リクエストまで
  message: {
    success: false,
    error: 'AUTH_RATE_LIMIT_EXCEEDED',
    message: '認証リクエストが多すぎます。15分後に再試行してください。',
  },
  standardHeaders: true,
  legacyHeaders: false,
});

// ショップ購入用のレート制限
const purchaseLimiter = rateLimit({
  windowMs: 60 * 1000, // 1分
  max: 10, // 10リクエスト
  message: {
    success: false,
    error: 'PURCHASE_RATE_LIMIT_EXCEEDED',
    message: '購入リクエストが多すぎます。しばらく待ってください。',
  },
  standardHeaders: true,
  legacyHeaders: false,
});

module.exports = {
  generalLimiter,
  authLimiter,
  purchaseLimiter,
};
