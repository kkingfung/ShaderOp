/**
 * セーブデータサービス
 * クラウドセーブ管理
 */

const { query } = require('../config/database');
const logger = require('../utils/logger');

/**
 * セーブデータ取得
 */
async function getSaveData(userId) {
  const result = await query(
    `SELECT profile_data, last_sync_date FROM save_data WHERE user_id = $1`,
    [userId]
  );

  if (result.rows.length === 0) {
    return null;
  }

  return {
    profile: result.rows[0].profile_data,
    lastSyncDate: result.rows[0].last_sync_date,
  };
}

/**
 * セーブデータ保存
 */
async function saveSaveData(userId, profileData) {
  const result = await query(
    `INSERT INTO save_data (user_id, profile_data, last_sync_date)
     VALUES ($1, $2, NOW())
     ON CONFLICT (user_id)
     DO UPDATE SET profile_data = $2, last_sync_date = NOW()
     RETURNING last_sync_date`,
    [userId, JSON.stringify(profileData)]
  );

  logger.info('セーブデータ保存:', { userId });

  return {
    success: true,
    lastSyncDate: result.rows[0].last_sync_date,
  };
}

/**
 * セーブデータ削除
 */
async function deleteSaveData(userId) {
  await query(`DELETE FROM save_data WHERE user_id = $1`, [userId]);

  logger.info('セーブデータ削除:', { userId });
  return { success: true };
}

module.exports = {
  getSaveData,
  saveSaveData,
  deleteSaveData,
};
