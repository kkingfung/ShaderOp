/**
 * マッチサービス
 * マッチング・対戦記録管理
 */

const { query, transaction } = require('../config/database');
const { NotFoundError } = require('../utils/errors');
const logger = require('../utils/logger');

/**
 * マッチセッション作成
 */
async function createMatch(sessionId, gameType, players, roomName = null, inviteCode = null) {
  const result = await transaction(async (client) => {
    // マッチセッション作成
    const matchResult = await client.query(
      `INSERT INTO match_sessions (session_id, game_type, room_name, max_players, invite_code)
       VALUES ($1, $2, $3, $4, $5)
       RETURNING match_id, session_id, created_date`,
      [sessionId, gameType, roomName, players.length, inviteCode]
    );

    const match = matchResult.rows[0];

    // プレイヤーを追加
    for (const player of players) {
      await client.query(
        `INSERT INTO match_players (match_id, user_id)
         VALUES ($1, $2)`,
        [match.match_id, player.userId]
      );
    }

    logger.info('マッチセッション作成:', { sessionId, gameType });
    return match;
  });

  return result;
}

/**
 * マッチ結果更新
 */
async function updateMatchResult(sessionId, winnerId, loserId, gameType, duration) {
  const result = await transaction(async (client) => {
    // マッチセッション更新
    const matchResult = await client.query(
      `UPDATE match_sessions
       SET status = 'finished', winner_id = $1, loser_id = $2, duration_seconds = $3, end_time = NOW()
       WHERE session_id = $4
       RETURNING match_id`,
      [winnerId, loserId, duration, sessionId]
    );

    if (matchResult.rows.length === 0) {
      throw new NotFoundError('マッチセッションが見つかりません');
    }

    // 勝者の統計更新
    await client.query(
      `UPDATE player_stats
       SET total_matches = total_matches + 1,
           wins = wins + 1,
           total_play_time_seconds = total_play_time_seconds + $1
       WHERE user_id = $2`,
      [duration, winnerId]
    );

    // 敗者の統計更新
    if (loserId) {
      await client.query(
        `UPDATE player_stats
         SET total_matches = total_matches + 1,
             losses = losses + 1,
             total_play_time_seconds = total_play_time_seconds + $1
         WHERE user_id = $2`,
        [duration, loserId]
      );
    }

    // 報酬計算（勝者）
    const coinReward = 50;
    const expReward = 100;

    await client.query(
      `UPDATE users
       SET coins = coins + $1,
           experience = experience + $2
       WHERE user_id = $3`,
      [coinReward, expReward, winnerId]
    );

    // 更新後の統計取得
    const statsResult = await client.query(
      `SELECT total_matches, wins, losses, draws FROM player_stats WHERE user_id = $1`,
      [winnerId]
    );

    const stats = statsResult.rows[0];

    logger.info('マッチ結果更新:', { sessionId, winnerId, loserId });

    return {
      rewards: {
        coins: coinReward,
        experience: expReward,
      },
      newStats: {
        totalMatches: stats.total_matches,
        wins: stats.wins,
        losses: stats.losses,
        draws: stats.draws,
        winRate: ((stats.wins / stats.total_matches) * 100).toFixed(2),
      },
    };
  });

  return result;
}

/**
 * マッチ履歴取得
 */
async function getMatchHistory(userId, limit = 20, offset = 0) {
  const result = await query(
    `SELECT
       ms.match_id,
       ms.session_id,
       ms.game_type,
       ms.winner_id,
       ms.loser_id,
       ms.duration_seconds,
       ms.start_time,
       CASE
         WHEN ms.winner_id = $1 THEN 'win'
         WHEN ms.loser_id = $1 THEN 'loss'
         ELSE 'draw'
       END as result,
       u.user_id as opponent_id,
       u.display_name as opponent_name
     FROM match_sessions ms
     JOIN match_players mp ON ms.match_id = mp.match_id
     LEFT JOIN users u ON (u.user_id = ms.winner_id OR u.user_id = ms.loser_id) AND u.user_id != $1
     WHERE mp.user_id = $1 AND ms.status = 'finished'
     ORDER BY ms.start_time DESC
     LIMIT $2 OFFSET $3`,
    [userId, limit, offset]
  );

  return result.rows.map((row) => ({
    matchId: row.match_id,
    gameType: row.game_type,
    opponent: {
      userId: row.opponent_id,
      displayName: row.opponent_name,
    },
    result: row.result,
    duration: row.duration_seconds,
    playedDate: row.start_time,
  }));
}

/**
 * マッチ総数取得
 */
async function getMatchCount(userId) {
  const result = await query(
    `SELECT COUNT(*) as count
     FROM match_sessions ms
     JOIN match_players mp ON ms.match_id = mp.match_id
     WHERE mp.user_id = $1 AND ms.status = 'finished'`,
    [userId]
  );

  return parseInt(result.rows[0].count);
}

module.exports = {
  createMatch,
  updateMatchResult,
  getMatchHistory,
  getMatchCount,
};
