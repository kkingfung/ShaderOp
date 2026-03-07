/**
 * セーブデータ関連ルート
 */

const express = require('express');
const router = express.Router();
const { authenticateToken } = require('../middleware/auth');
const saveDataService = require('../services/saveDataService');
const userService = require('../services/userService');

/**
 * GET /api/savedata
 * セーブデータ取得
 */
router.get('/', authenticateToken, async (req, res, next) => {
  try {
    const user = await userService.getUserByFirebaseUid(req.user.firebaseUid);
    const saveData = await saveDataService.getSaveData(user.userId);

    res.status(200).json({
      success: true,
      data: saveData,
    });
  } catch (error) {
    next(error);
  }
});

/**
 * POST /api/savedata
 * セーブデータ保存
 */
router.post('/', authenticateToken, async (req, res, next) => {
  try {
    const user = await userService.getUserByFirebaseUid(req.user.firebaseUid);
    const { profileData } = req.body;

    const result = await saveDataService.saveSaveData(user.userId, profileData);

    res.status(200).json({
      success: true,
      message: 'セーブデータを保存しました',
      data: result,
    });
  } catch (error) {
    next(error);
  }
});

/**
 * DELETE /api/savedata
 * セーブデータ削除
 */
router.delete('/', authenticateToken, async (req, res, next) => {
  try {
    const user = await userService.getUserByFirebaseUid(req.user.firebaseUid);
    await saveDataService.deleteSaveData(user.userId);

    res.status(200).json({
      success: true,
      message: 'セーブデータを削除しました',
    });
  } catch (error) {
    next(error);
  }
});

module.exports = router;
