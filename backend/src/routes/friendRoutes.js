/**
 * フレンド関連ルート
 */

const express = require('express');
const router = express.Router();
const { authenticateToken } = require('../middleware/auth');
const { friendRequestValidation } = require('../middleware/validation');
const friendService = require('../services/friendService');
const userService = require('../services/userService');

/**
 * GET /api/friends
 * フレンドリスト取得
 */
router.get('/', authenticateToken, async (req, res, next) => {
  try {
    const user = await userService.getUserByFirebaseUid(req.user.firebaseUid);
    const friends = await friendService.getFriends(user.userId);

    res.status(200).json({
      success: true,
      data: friends,
    });
  } catch (error) {
    next(error);
  }
});

/**
 * POST /api/friends/requests
 * フレンドリクエスト送信
 */
router.post('/requests', authenticateToken, friendRequestValidation, async (req, res, next) => {
  try {
    const user = await userService.getUserByFirebaseUid(req.user.firebaseUid);
    const { targetUserId } = req.body;

    const request = await friendService.sendFriendRequest(user.userId, targetUserId);

    res.status(201).json({
      success: true,
      message: 'フレンドリクエストを送信しました',
      data: request,
    });
  } catch (error) {
    next(error);
  }
});

/**
 * GET /api/friends/requests/incoming
 * 受信したフレンドリクエスト一覧
 */
router.get('/requests/incoming', authenticateToken, async (req, res, next) => {
  try {
    const user = await userService.getUserByFirebaseUid(req.user.firebaseUid);
    const requests = await friendService.getIncomingRequests(user.userId);

    res.status(200).json({
      success: true,
      data: requests,
    });
  } catch (error) {
    next(error);
  }
});

/**
 * POST /api/friends/requests/:requestId/accept
 * フレンドリクエスト承認
 */
router.post('/requests/:requestId/accept', authenticateToken, async (req, res, next) => {
  try {
    const user = await userService.getUserByFirebaseUid(req.user.firebaseUid);
    const friend = await friendService.acceptFriendRequest(req.params.requestId, user.userId);

    res.status(200).json({
      success: true,
      message: 'フレンドリクエストを承認しました',
      data: friend,
    });
  } catch (error) {
    next(error);
  }
});

/**
 * POST /api/friends/requests/:requestId/reject
 * フレンドリクエスト拒否
 */
router.post('/requests/:requestId/reject', authenticateToken, async (req, res, next) => {
  try {
    const user = await userService.getUserByFirebaseUid(req.user.firebaseUid);
    await friendService.rejectFriendRequest(req.params.requestId, user.userId);

    res.status(200).json({
      success: true,
      message: 'フレンドリクエストを拒否しました',
    });
  } catch (error) {
    next(error);
  }
});

/**
 * DELETE /api/friends/:friendId
 * フレンド削除
 */
router.delete('/:friendId', authenticateToken, async (req, res, next) => {
  try {
    const user = await userService.getUserByFirebaseUid(req.user.firebaseUid);
    await friendService.removeFriend(user.userId, req.params.friendId);

    res.status(200).json({
      success: true,
      message: 'フレンドを削除しました',
    });
  } catch (error) {
    next(error);
  }
});

module.exports = router;
