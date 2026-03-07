/**
 * エラーハンドリングミドルウェア
 */

const logger = require('../utils/logger');

/**
 * 404エラーハンドラ
 */
function notFoundHandler(req, res, next) {
  res.status(404).json({
    success: false,
    error: 'NOT_FOUND',
    message: `エンドポイントが見つかりません: ${req.method} ${req.path}`,
  });
}

/**
 * グローバルエラーハンドラ
 */
function errorHandler(err, req, res, next) {
  logger.error('エラーが発生しました:', {
    error: err.message,
    stack: err.stack,
    path: req.path,
    method: req.method,
  });

  // バリデーションエラー
  if (err.name === 'ValidationError') {
    return res.status(400).json({
      success: false,
      error: 'VALIDATION_ERROR',
      message: 'リクエストデータが無効です',
      details: err.details || err.message,
    });
  }

  // データベースエラー
  if (err.code && err.code.startsWith('23')) {
    return res.status(400).json({
      success: false,
      error: 'DATABASE_ERROR',
      message: 'データベース操作に失敗しました',
    });
  }

  // デフォルトエラーレスポンス
  const statusCode = err.statusCode || 500;
  res.status(statusCode).json({
    success: false,
    error: err.code || 'INTERNAL_ERROR',
    message: err.message || 'サーバーエラーが発生しました',
    ...(process.env.NODE_ENV === 'development' && { stack: err.stack }),
  });
}

module.exports = {
  notFoundHandler,
  errorHandler,
};
