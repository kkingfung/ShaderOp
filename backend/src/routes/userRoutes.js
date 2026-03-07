/**
 * ユーザー関連ルート
 */

const express = require('express');
const router = express.Router();
const { authenticateToken } = require('../middleware/auth');
const { registerValidation, updateProfileValidation } = require('../middleware/validation');
const userService = require('../services/userService');

/**
 * POST /api/users/register
 * ユーザー登録
 */
router.post('/register', registerValidation, async (req, res, next) => {
  try {
    const { firebaseUid, displayName, email } = req.body;
    const user = await userService.registerUser(firebaseUid, displayName, email);

    res.status(201).json({
      success: true,
      message: 'ユーザー登録完了',
      data: user,
    });
  } catch (error) {
    next(error);
  }
});

/**
 * GET /api/users/me
 * 自分のプロフィール取得
 */
router.get('/me', authenticateToken, async (req, res, next) => {
  try {
    const user = await userService.getUserByFirebaseUid(req.user.firebaseUid);

    res.status(200).json({
      success: true,
      data: user,
    });
  } catch (error) {
    next(error);
  }
});

/**
 * GET /api/users/:userId
 * 他ユーザーのプロフィール取得
 */
router.get('/:userId', authenticateToken, async (req, res, next) => {
  try {
    const user = await userService.getUserProfile(req.params.userId);

    res.status(200).json({
      success: true,
      data: user,
    });
  } catch (error) {
    next(error);
  }
});

/**
 * PUT /api/users/me
 * プロフィール更新
 */
router.put('/me', authenticateToken, updateProfileValidation, async (req, res, next) => {
  try {
    const user = await userService.getUserByFirebaseUid(req.user.firebaseUid);
    await userService.updateProfile(user.userId, req.body);

    const updatedUser = await userService.getUserProfile(user.userId);

    res.status(200).json({
      success: true,
      message: 'プロフィール更新完了',
      data: updatedUser,
    });
  } catch (error) {
    next(error);
  }
});

/**
 * GET /api/users/search
 * ユーザー検索
 */
router.get('/search', authenticateToken, async (req, res, next) => {
  try {
    const { q, limit } = req.query;
    const users = await userService.searchUsers(q, limit ? parseInt(limit) : 20);

    res.status(200).json({
      success: true,
      data: users,
    });
  } catch (error) {
    next(error);
  }
});

/**
 * GET /api/users/me/balance
 * 残高取得
 */
router.get('/me/balance', authenticateToken, async (req, res, next) => {
  try {
    const user = await userService.getUserByFirebaseUid(req.user.firebaseUid);
    const balance = await userService.getBalance(user.userId);

    res.status(200).json({
      success: true,
      data: balance,
    });
  } catch (error) {
    next(error);
  }
});

module.exports = router;
