/**
 * ショップ関連ルート
 */

const express = require('express');
const router = express.Router();
const { authenticateToken } = require('../middleware/auth');
const { purchaseValidation } = require('../middleware/validation');
const { purchaseLimiter } = require('../middleware/rateLimiter');
const shopService = require('../services/shopService');
const userService = require('../services/userService');

/**
 * GET /api/shop/items
 * ショップアイテム一覧
 */
router.get('/items', authenticateToken, async (req, res, next) => {
  try {
    const { category } = req.query;
    const items = await shopService.getShopItems(category);

    res.status(200).json({
      success: true,
      data: items,
    });
  } catch (error) {
    next(error);
  }
});

/**
 * GET /api/shop/items/:itemId
 * アイテム詳細
 */
router.get('/items/:itemId', authenticateToken, async (req, res, next) => {
  try {
    const item = await shopService.getItemById(req.params.itemId);

    res.status(200).json({
      success: true,
      data: item,
    });
  } catch (error) {
    next(error);
  }
});

/**
 * POST /api/shop/purchase
 * アイテム購入
 */
router.post('/purchase', authenticateToken, purchaseLimiter, purchaseValidation, async (req, res, next) => {
  try {
    const user = await userService.getUserByFirebaseUid(req.user.firebaseUid);
    const { itemId, currencyType } = req.body;

    const result = await shopService.purchaseItem(user.userId, itemId, currencyType);

    res.status(200).json({
      success: true,
      message: 'アイテムを購入しました',
      data: result,
    });
  } catch (error) {
    next(error);
  }
});

/**
 * GET /api/shop/my-items
 * 所持アイテム一覧
 */
router.get('/my-items', authenticateToken, async (req, res, next) => {
  try {
    const user = await userService.getUserByFirebaseUid(req.user.firebaseUid);
    const items = await shopService.getUserItems(user.userId);

    res.status(200).json({
      success: true,
      data: items,
    });
  } catch (error) {
    next(error);
  }
});

/**
 * GET /api/shop/transactions
 * トランザクション履歴
 */
router.get('/transactions', authenticateToken, async (req, res, next) => {
  try {
    const user = await userService.getUserByFirebaseUid(req.user.firebaseUid);
    const { limit, offset } = req.query;

    const transactions = await shopService.getTransactionHistory(
      user.userId,
      limit ? parseInt(limit) : 50,
      offset ? parseInt(offset) : 0
    );

    res.status(200).json({
      success: true,
      data: transactions,
    });
  } catch (error) {
    next(error);
  }
});

module.exports = router;
