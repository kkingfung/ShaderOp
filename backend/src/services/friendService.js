/**
 * フレンドサービス
 * フレンド管理とリクエスト処理
 */

const { query, transaction } = require('../config/database');
const { isUserOnline } = require('../config/redis');
const { NotFoundError, ValidationError } = require('../utils/errors');
const logger = require('../utils/logger');

/**
 * フレンドリスト取得
 */
async function getFriends(userId) {
  const result = await query(
    `SELECT
       u.user_id,
       u.display_name,
       u.level,
       u.is_vip,
       u.last_login_date,
       f.friend_since
     FROM friendships f
     JOIN users u ON (u.user_id = f.user_id_2 OR u.user_id = f.user_id_1)
     WHERE (f.user_id_1 = $1 OR f.user_id_2 = $1) AND u.user_id != $1
     ORDER BY u.display_name`,
    [userId]
  );

  // オンラインステータスを付与
  const friends = await Promise.all(
    result.rows.map(async (friend) => ({
      userId: friend.user_id,
      displayName: friend.display_name,
      level: friend.level,
      isOnline: await isUserOnline(friend.user_id),
      isVip: friend.is_vip,
      lastOnlineDate: friend.last_login_date,
      friendSince: friend.friend_since,
    }))
  );

  return friends;
}

/**
 * フレンドリクエスト送信
 */
async function sendFriendRequest(fromUserId, toUserId) {
  if (fromUserId === toUserId) {
    throw new ValidationError('自分自身にフレンドリクエストは送信できません');
  }

  // すでにフレンドかチェック
  const friendCheck = await query(
    `SELECT 1 FROM friendships
     WHERE (user_id_1 = LEAST($1, $2) AND user_id_2 = GREATEST($1, $2))`,
    [fromUserId, toUserId]
  );

  if (friendCheck.rows.length > 0) {
    throw new ValidationError('すでにフレンドです');
  }

  // リクエスト重複チェック
  const requestCheck = await query(
    `SELECT 1 FROM friend_requests
     WHERE from_user_id = $1 AND to_user_id = $2 AND status = 'pending'`,
    [fromUserId, toUserId]
  );

  if (requestCheck.rows.length > 0) {
    throw new ValidationError('すでにリクエストを送信済みです');
  }

  // リクエスト作成
  const result = await query(
    `INSERT INTO friend_requests (from_user_id, to_user_id)
     VALUES ($1, $2)
     RETURNING request_id, from_user_id, to_user_id, status, request_date`,
    [fromUserId, toUserId]
  );

  logger.info('フレンドリクエスト送信:', { from: fromUserId, to: toUserId });
  return result.rows[0];
}

/**
 * フレンドリクエスト承認
 */
async function acceptFriendRequest(requestId, userId) {
  const result = await transaction(async (client) => {
    // リクエスト取得
    const requestResult = await client.query(
      `SELECT * FROM friend_requests
       WHERE request_id = $1 AND to_user_id = $2 AND status = 'pending'`,
      [requestId, userId]
    );

    if (requestResult.rows.length === 0) {
      throw new NotFoundError('リクエストが見つかりません');
    }

    const request = requestResult.rows[0];

    // リクエストを承認済みに更新
    await client.query(
      `UPDATE friend_requests
       SET status = 'accepted', response_date = NOW()
       WHERE request_id = $1`,
      [requestId]
    );

    // フレンド関係を作成
    const userId1 = request.from_user_id < userId ? request.from_user_id : userId;
    const userId2 = request.from_user_id < userId ? userId : request.from_user_id;

    await client.query(
      `INSERT INTO friendships (user_id_1, user_id_2)
       VALUES ($1, $2)`,
      [userId1, userId2]
    );

    // フレンド情報を取得
    const friendResult = await client.query(
      `SELECT user_id, display_name, level FROM users WHERE user_id = $1`,
      [request.from_user_id]
    );

    logger.info('フレンドリクエスト承認:', { requestId });
    return friendResult.rows[0];
  });

  return result;
}

/**
 * フレンドリクエスト拒否
 */
async function rejectFriendRequest(requestId, userId) {
  const result = await query(
    `UPDATE friend_requests
     SET status = 'rejected', response_date = NOW()
     WHERE request_id = $1 AND to_user_id = $2 AND status = 'pending'
     RETURNING request_id`,
    [requestId, userId]
  );

  if (result.rows.length === 0) {
    throw new NotFoundError('リクエストが見つかりません');
  }

  logger.info('フレンドリクエスト拒否:', { requestId });
  return { success: true };
}

/**
 * フレンド削除
 */
async function removeFriend(userId, friendId) {
  const result = await query(
    `DELETE FROM friendships
     WHERE (user_id_1 = LEAST($1, $2) AND user_id_2 = GREATEST($1, $2))
     RETURNING friendship_id`,
    [userId, friendId]
  );

  if (result.rows.length === 0) {
    throw new NotFoundError('フレンド関係が見つかりません');
  }

  logger.info('フレンド削除:', { userId, friendId });
  return { success: true };
}

/**
 * 受信したフレンドリクエスト一覧
 */
async function getIncomingRequests(userId) {
  const result = await query(
    `SELECT
       fr.request_id,
       fr.from_user_id,
       u.display_name as from_display_name,
       u.level,
       u.is_vip,
       fr.request_date
     FROM friend_requests fr
     JOIN users u ON fr.from_user_id = u.user_id
     WHERE fr.to_user_id = $1 AND fr.status = 'pending'
     ORDER BY fr.request_date DESC`,
    [userId]
  );

  return result.rows;
}

module.exports = {
  getFriends,
  sendFriendRequest,
  acceptFriendRequest,
  rejectFriendRequest,
  removeFriend,
  getIncomingRequests,
};
