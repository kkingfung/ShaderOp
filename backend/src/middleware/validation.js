/**
 * リクエストバリデーションミドルウェア
 */

const { body, param, query, validationResult } = require('express-validator');

/**
 * バリデーション結果をチェックするミドルウェア
 */
function validate(req, res, next) {
  const errors = validationResult(req);

  if (!errors.isEmpty()) {
    return res.status(400).json({
      success: false,
      error: 'VALIDATION_ERROR',
      message: 'リクエストデータが無効です',
      details: errors.array(),
    });
  }

  next();
}

/**
 * ユーザー登録バリデーション
 */
const registerValidation = [
  body('firebaseUid').notEmpty().withMessage('firebaseUidは必須です'),
  body('displayName')
    .notEmpty()
    .withMessage('表示名は必須です')
    .isLength({ min: 2, max: 50 })
    .withMessage('表示名は2〜50文字である必要があります'),
  body('email').isEmail().withMessage('有効なメールアドレスを入力してください'),
  validate,
];

/**
 * プロフィール更新バリデーション
 */
const updateProfileValidation = [
  body('displayName')
    .optional()
    .isLength({ min: 2, max: 50 })
    .withMessage('表示名は2〜50文字である必要があります'),
  body('avatar').optional().isObject().withMessage('avatarはオブジェクトである必要があります'),
  validate,
];

/**
 * フレンドリクエストバリデーション
 */
const friendRequestValidation = [
  body('targetUserId').notEmpty().isUUID().withMessage('有効なユーザーIDを指定してください'),
  validate,
];

/**
 * 購入バリデーション
 */
const purchaseValidation = [
  body('itemId').notEmpty().withMessage('アイテムIDは必須です'),
  body('currencyType')
    .notEmpty()
    .isIn(['coins', 'gems', 'real_money'])
    .withMessage('有効な通貨タイプを指定してください'),
  validate,
];

/**
 * マッチ作成バリデーション
 */
const createMatchValidation = [
  body('sessionId').notEmpty().withMessage('セッションIDは必須です'),
  body('gameType').notEmpty().withMessage('ゲームタイプは必須です'),
  body('players').isArray({ min: 2 }).withMessage('プレイヤーは2人以上必要です'),
  validate,
];

/**
 * ページネーションバリデーション
 */
const paginationValidation = [
  query('limit')
    .optional()
    .isInt({ min: 1, max: 100 })
    .withMessage('limitは1〜100の整数である必要があります'),
  query('offset').optional().isInt({ min: 0 }).withMessage('offsetは0以上の整数である必要があります'),
  validate,
];

module.exports = {
  validate,
  registerValidation,
  updateProfileValidation,
  friendRequestValidation,
  purchaseValidation,
  createMatchValidation,
  paginationValidation,
};
