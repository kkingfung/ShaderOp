/**
 * ショップサービス
 * アイテム購入と経済システム
 */

const { query, transaction } = require('../config/database');
const { NotFoundError, InsufficientFundsError, VipOnlyError } = require('../utils/errors');
const logger = require('../utils/logger');

/**
 * ショップアイテム一覧取得
 */
async function getShopItems(category = null) {
  let sql = `SELECT * FROM shop_items WHERE is_active = true`;
  const params = [];

  if (category) {
    sql += ` AND category = $1`;
    params.push(category);
  }

  sql += ` ORDER BY category, item_name`;

  const result = await query(sql, params);
  return result.rows;
}

/**
 * アイテム詳細取得
 */
async function getItemById(itemId) {
  const result = await query(
    `SELECT * FROM shop_items WHERE item_id = $1 AND is_active = true`,
    [itemId]
  );

  if (result.rows.length === 0) {
    throw new NotFoundError('アイテムが見つかりません');
  }

  return result.rows[0];
}

/**
 * アイテム購入処理
 */
async function purchaseItem(userId, itemId, currencyType) {
  const result = await transaction(async (client) => {
    // アイテム情報取得
    const itemResult = await client.query(
      `SELECT * FROM shop_items WHERE item_id = $1 AND is_active = true`,
      [itemId]
    );

    if (itemResult.rows.length === 0) {
      throw new NotFoundError('アイテムが見つかりません');
    }

    const item = itemResult.rows[0];

    // ユーザー情報取得
    const userResult = await client.query(
      `SELECT coins, gems, vip_coins, is_vip FROM users WHERE user_id = $1`,
      [userId]
    );

    if (userResult.rows.length === 0) {
      throw new NotFoundError('ユーザーが見つかりません');
    }

    const user = userResult.rows[0];

    // VIP限定チェック
    if (item.is_vip_only && !user.is_vip) {
      throw new VipOnlyError();
    }

    // すでに所持しているかチェック
    const ownershipResult = await client.query(
      `SELECT 1 FROM user_items WHERE user_id = $1 AND item_id = $2`,
      [userId, itemId]
    );

    if (ownershipResult.rows.length > 0) {
      throw new Error('すでに所持しているアイテムです');
    }

    // 価格と通貨チェック
    let price = 0;
    let currencyField = '';

    if (currencyType === 'coins') {
      price = item.coin_price;
      currencyField = 'coins';
    } else if (currencyType === 'gems') {
      price = item.gem_price;
      currencyField = 'gems';
    } else {
      throw new Error('無効な通貨タイプです');
    }

    if (price === 0) {
      throw new Error('この通貨での購入はできません');
    }

    // 残高チェック
    if (user[currencyField] < price) {
      throw new InsufficientFundsError();
    }

    // 通貨を減算
    await client.query(
      `UPDATE users SET ${currencyField} = ${currencyField} - $1 WHERE user_id = $2`,
      [price, userId]
    );

    // トランザクション記録
    const txnResult = await client.query(
      `INSERT INTO transactions (user_id, item_id, amount_paid, currency, status)
       VALUES ($1, $2, $3, $4, 'completed')
       RETURNING transaction_id, purchase_date`,
      [userId, itemId, price, currencyType]
    );

    const transaction = txnResult.rows[0];

    // アイテムを所持アイテムに追加
    await client.query(
      `INSERT INTO user_items (user_id, item_id) VALUES ($1, $2)`,
      [userId, itemId]
    );

    // 新しい残高を取得
    const balanceResult = await client.query(
      `SELECT coins, gems, vip_coins FROM users WHERE user_id = $1`,
      [userId]
    );

    logger.info('アイテム購入完了:', { userId, itemId, price, currencyType });

    return {
      transaction: {
        transactionId: transaction.transaction_id,
        itemId,
        amountPaid: price,
        currency: currencyType,
        status: 'completed',
        purchaseDate: transaction.purchase_date,
      },
      newBalance: balanceResult.rows[0],
    };
  });

  return result;
}

/**
 * ユーザー所持アイテム一覧
 */
async function getUserItems(userId) {
  const result = await query(
    `SELECT si.*, ui.acquired_date
     FROM user_items ui
     JOIN shop_items si ON ui.item_id = si.item_id
     WHERE ui.user_id = $1
     ORDER BY ui.acquired_date DESC`,
    [userId]
  );

  return result.rows;
}

/**
 * トランザクション履歴取得
 */
async function getTransactionHistory(userId, limit = 50, offset = 0) {
  const result = await query(
    `SELECT
       t.transaction_id,
       t.item_id,
       si.item_name,
       t.amount_paid,
       t.currency,
       t.status,
       t.purchase_date
     FROM transactions t
     LEFT JOIN shop_items si ON t.item_id = si.item_id
     WHERE t.user_id = $1
     ORDER BY t.purchase_date DESC
     LIMIT $2 OFFSET $3`,
    [userId, limit, offset]
  );

  return result.rows;
}

module.exports = {
  getShopItems,
  getItemById,
  purchaseItem,
  getUserItems,
  getTransactionHistory,
};
