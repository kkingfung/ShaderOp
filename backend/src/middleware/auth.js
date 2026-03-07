/**
 * Firebase認証ミドルウェア
 * Bearer Tokenを検証してユーザー情報を取得
 */

const admin = require('../config/firebase');
const logger = require('../utils/logger');

/**
 * Firebase IDトークンを検証するミドルウェア
 */
async function authenticateToken(req, res, next) {
  try {
    const authHeader = req.headers.authorization;

    if (!authHeader || !authHeader.startsWith('Bearer ')) {
      return res.status(401).json({
        success: false,
        error: 'UNAUTHORIZED',
        message: '認証トークンが必要です',
      });
    }

    const idToken = authHeader.split('Bearer ')[1];

    // Firebase IDトークンを検証
    const decodedToken = await admin.auth().verifyIdToken(idToken);

    // リクエストオブジェクトにユーザー情報を追加
    req.user = {
      firebaseUid: decodedToken.uid,
      email: decodedToken.email,
      emailVerified: decodedToken.email_verified,
    };

    next();
  } catch (error) {
    logger.error('認証エラー:', error);

    if (error.code === 'auth/id-token-expired') {
      return res.status(401).json({
        success: false,
        error: 'TOKEN_EXPIRED',
        message: 'トークンの有効期限が切れています',
      });
    }

    return res.status(401).json({
      success: false,
      error: 'UNAUTHORIZED',
      message: '認証に失敗しました',
    });
  }
}

/**
 * オプショナル認証（トークンがあれば検証、なくても続行）
 */
async function optionalAuth(req, res, next) {
  try {
    const authHeader = req.headers.authorization;

    if (authHeader && authHeader.startsWith('Bearer ')) {
      const idToken = authHeader.split('Bearer ')[1];
      const decodedToken = await admin.auth().verifyIdToken(idToken);

      req.user = {
        firebaseUid: decodedToken.uid,
        email: decodedToken.email,
        emailVerified: decodedToken.email_verified,
      };
    }

    next();
  } catch (error) {
    logger.warn('オプショナル認証エラー:', error);
    next();
  }
}

module.exports = {
  authenticateToken,
  optionalAuth,
};
