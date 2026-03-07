/**
 * ユーザーサービス
 * ユーザー情報の管理とCRUD操作
 */

const { query, transaction } = require('../config/database');
const { NotFoundError } = require('../utils/errors');
const logger = require('../utils/logger');

/**
 * ユーザー登録
 */
async function registerUser(firebaseUid, displayName, email) {
  try {
    const result = await transaction(async (client) => {
      // ユーザー作成
      const userResult = await client.query(
        `INSERT INTO users (firebase_uid, display_name, email)
         VALUES ($1, $2, $3)
         RETURNING user_id, display_name, created_date`,
        [firebaseUid, displayName, email]
      );

      const user = userResult.rows[0];

      // アバターデータ作成
      await client.query(
        `INSERT INTO avatar_data (user_id) VALUES ($1)`,
        [user.user_id]
      );

      // プレイヤー統計作成
      await client.query(
        `INSERT INTO player_stats (user_id) VALUES ($1)`,
        [user.user_id]
      );

      return user;
    });

    logger.info('新規ユーザー登録完了:', { userId: result.user_id, displayName });
    return result;
  } catch (error) {
    logger.error('ユーザー登録エラー:', error);
    throw error;
  }
}

/**
 * Firebase UIDからユーザー情報取得
 */
async function getUserByFirebaseUid(firebaseUid) {
  const result = await query(
    `SELECT u.*, a.*, s.*
     FROM users u
     LEFT JOIN avatar_data a ON u.user_id = a.user_id
     LEFT JOIN player_stats s ON u.user_id = s.user_id
     WHERE u.firebase_uid = $1`,
    [firebaseUid]
  );

  if (result.rows.length === 0) {
    throw new NotFoundError('ユーザーが見つかりません');
  }

  return formatUserProfile(result.rows[0]);
}

/**
 * ユーザーIDからプロフィール取得
 */
async function getUserProfile(userId) {
  const result = await query(
    `SELECT u.*, a.*, s.*
     FROM users u
     LEFT JOIN avatar_data a ON u.user_id = a.user_id
     LEFT JOIN player_stats s ON u.user_id = s.user_id
     WHERE u.user_id = $1`,
    [userId]
  );

  if (result.rows.length === 0) {
    throw new NotFoundError('ユーザーが見つかりません');
  }

  return formatUserProfile(result.rows[0]);
}

/**
 * プロフィール更新
 */
async function updateProfile(userId, updates) {
  try {
    const result = await transaction(async (client) => {
      // ユーザー基本情報更新
      if (updates.displayName) {
        await client.query(
          `UPDATE users SET display_name = $1 WHERE user_id = $2`,
          [updates.displayName, userId]
        );
      }

      // アバターデータ更新
      if (updates.avatar) {
        const avatar = updates.avatar;
        const setClause = [];
        const values = [];
        let paramIndex = 1;

        if (avatar.gender) {
          setClause.push(`gender = $${paramIndex++}`);
          values.push(avatar.gender);
        }
        if (avatar.faceId) {
          setClause.push(`face_id = $${paramIndex++}`);
          values.push(avatar.faceId);
        }
        if (avatar.hairstyleId) {
          setClause.push(`hairstyle_id = $${paramIndex++}`);
          values.push(avatar.hairstyleId);
        }
        if (avatar.hairColor) {
          setClause.push(`hair_color = $${paramIndex++}`);
          values.push(avatar.hairColor);
        }
        if (avatar.skinToneId !== undefined) {
          setClause.push(`skin_tone_id = $${paramIndex++}`);
          values.push(avatar.skinToneId);
        }
        if (avatar.eyeColor) {
          setClause.push(`eye_color = $${paramIndex++}`);
          values.push(avatar.eyeColor);
        }
        if (avatar.outfitId) {
          setClause.push(`outfit_id = $${paramIndex++}`);
          values.push(avatar.outfitId);
        }

        if (setClause.length > 0) {
          values.push(userId);
          await client.query(
            `UPDATE avatar_data SET ${setClause.join(', ')} WHERE user_id = $${paramIndex}`,
            values
          );
        }
      }

      return { success: true };
    });

    logger.info('プロフィール更新完了:', { userId });
    return result;
  } catch (error) {
    logger.error('プロフィール更新エラー:', error);
    throw error;
  }
}

/**
 * ユーザー検索（表示名）
 */
async function searchUsers(searchQuery, limit = 20) {
  const result = await query(
    `SELECT user_id, display_name, level, is_vip
     FROM users
     WHERE display_name ILIKE $1
     ORDER BY level DESC
     LIMIT $2`,
    [`%${searchQuery}%`, limit]
  );

  return result.rows;
}

/**
 * 通貨残高取得
 */
async function getBalance(userId) {
  const result = await query(
    `SELECT coins, gems, vip_coins FROM users WHERE user_id = $1`,
    [userId]
  );

  if (result.rows.length === 0) {
    throw new NotFoundError('ユーザーが見つかりません');
  }

  return result.rows[0];
}

/**
 * 通貨更新
 */
async function updateBalance(userId, coins = 0, gems = 0, vipCoins = 0) {
  const result = await query(
    `UPDATE users
     SET coins = coins + $1, gems = gems + $2, vip_coins = vip_coins + $3
     WHERE user_id = $4
     RETURNING coins, gems, vip_coins`,
    [coins, gems, vipCoins, userId]
  );

  if (result.rows.length === 0) {
    throw new NotFoundError('ユーザーが見つかりません');
  }

  return result.rows[0];
}

/**
 * ユーザープロフィールフォーマット
 */
function formatUserProfile(row) {
  return {
    userId: row.user_id,
    displayName: row.display_name,
    level: row.level,
    experience: row.experience,
    avatar: {
      gender: row.gender,
      faceId: row.face_id,
      hairstyleId: row.hairstyle_id,
      hairColor: row.hair_color,
      skinToneId: row.skin_tone_id,
      eyeColor: row.eye_color,
      outfitId: row.outfit_id,
      accessoryIds: row.accessory_ids || [],
      heightScale: row.height_scale,
    },
    stats: {
      totalPlayTimeSeconds: row.total_play_time_seconds,
      totalMatches: row.total_matches,
      wins: row.wins,
      losses: row.losses,
      draws: row.draws,
      winRate: row.total_matches > 0 ? ((row.wins / row.total_matches) * 100).toFixed(2) : 0,
    },
    isVip: row.is_vip,
    vipExpiryDate: row.vip_expiry_date,
    lastLoginDate: row.last_login_date,
    createdDate: row.created_date,
  };
}

module.exports = {
  registerUser,
  getUserByFirebaseUid,
  getUserProfile,
  updateProfile,
  searchUsers,
  getBalance,
  updateBalance,
};
