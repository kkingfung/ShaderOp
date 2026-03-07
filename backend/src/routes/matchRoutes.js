/**
 * マッチ関連ルート
 */

const express = require('express');
const router = express.Router();
const { authenticateToken } = require('../middleware/auth');
const { createMatchValidation } = require('../middleware/validation');
const matchService = require('../services/matchService');
const userService = require('../services/userService');

/**
 * POST /api/matches
 * マッチセッション作成
 */
router.post('/', authenticateToken, createMatchValidation, async (req, res, next) => {
  try {
    const { sessionId, gameType, players, roomName, inviteCode } = req.body;

    const match = await matchService.createMatch(sessionId, gameType, players, roomName, inviteCode);

    res.status(201).json({
      success: true,
      message: 'マッチセッションを作成しました',
      data: match,
    });
  } catch (error) {
    next(error);
  }
});

/**
 * PUT /api/matches/:sessionId/result
 * マッチ結果更新
 */
router.put('/:sessionId/result', authenticateToken, async (req, res, next) => {
  try {
    const { winnerId, loserId, gameType, duration } = req.body;

    const result = await matchService.updateMatchResult(
      req.params.sessionId,
      winnerId,
      loserId,
      gameType,
      duration
    );

    res.status(200).json({
      success: true,
      message: 'マッチ結果を記録しました',
      data: result,
    });
  } catch (error) {
    next(error);
  }
});

/**
 * GET /api/matches/history
 * マッチ履歴取得
 */
router.get('/history', authenticateToken, async (req, res, next) => {
  try {
    const user = await userService.getUserByFirebaseUid(req.user.firebaseUid);
    const { limit, offset } = req.query;

    const history = await matchService.getMatchHistory(
      user.userId,
      limit ? parseInt(limit) : 20,
      offset ? parseInt(offset) : 0
    );

    const total = await matchService.getMatchCount(user.userId);

    res.status(200).json({
      success: true,
      data: {
        matches: history,
        total,
      },
    });
  } catch (error) {
    next(error);
  }
});

module.exports = router;
