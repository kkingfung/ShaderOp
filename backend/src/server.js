/**
 * ShaderOp REST APIサーバー
 * メインエントリーポイント
 */

require('dotenv').config();
const express = require('express');
const cors = require('cors');
const helmet = require('helmet');
const compression = require('compression');
const morgan = require('morgan');

const logger = require('./utils/logger');
const { notFoundHandler, errorHandler } = require('./middleware/errorHandler');
const { generalLimiter } = require('./middleware/rateLimiter');

// ルート
const userRoutes = require('./routes/userRoutes');
const friendRoutes = require('./routes/friendRoutes');
const shopRoutes = require('./routes/shopRoutes');
const matchRoutes = require('./routes/matchRoutes');
const saveDataRoutes = require('./routes/saveDataRoutes');

const app = express();
const PORT = process.env.PORT || 3000;

// ミドルウェア設定
app.use(helmet()); // セキュリティヘッダー
app.use(cors()); // CORS許可
app.use(compression()); // レスポンス圧縮
app.use(express.json()); // JSONパーサー
app.use(express.urlencoded({ extended: true })); // URLエンコードパーサー

// HTTPリクエストログ
if (process.env.NODE_ENV !== 'test') {
  app.use(morgan('combined', { stream: { write: (message) => logger.info(message.trim()) } }));
}

// レート制限
app.use('/api/', generalLimiter);

// ヘルスチェック
app.get('/health', (req, res) => {
  res.status(200).json({
    status: 'OK',
    timestamp: new Date().toISOString(),
    uptime: process.uptime(),
  });
});

// APIルート
app.use('/api/users', userRoutes);
app.use('/api/friends', friendRoutes);
app.use('/api/shop', shopRoutes);
app.use('/api/matches', matchRoutes);
app.use('/api/savedata', saveDataRoutes);

// 404ハンドラ
app.use(notFoundHandler);

// エラーハンドラ
app.use(errorHandler);

// サーバー起動
if (process.env.NODE_ENV !== 'test') {
  app.listen(PORT, () => {
    logger.info(`ShaderOp APIサーバー起動: http://localhost:${PORT}`);
    logger.info(`環境: ${process.env.NODE_ENV || 'development'}`);
  });
}

module.exports = app;
