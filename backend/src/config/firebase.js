/**
 * Firebase Admin SDK設定
 */

const admin = require('firebase-admin');
const logger = require('../utils/logger');

// Firebase Admin SDKの初期化
const initializeFirebase = () => {
  try {
    // 環境変数から設定を読み込み
    const serviceAccount = {
      type: 'service_account',
      project_id: process.env.FIREBASE_PROJECT_ID,
      private_key: process.env.FIREBASE_PRIVATE_KEY?.replace(/\\n/g, '\n'),
      client_email: process.env.FIREBASE_CLIENT_EMAIL,
    };

    admin.initializeApp({
      credential: admin.credential.cert(serviceAccount),
    });

    logger.info('Firebase Admin SDKを初期化しました');
  } catch (error) {
    logger.error('Firebase初期化エラー:', error);
    throw error;
  }
};

// アプリケーション起動時に初期化
initializeFirebase();

module.exports = admin;
