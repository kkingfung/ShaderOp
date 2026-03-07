/**
 * シードデータ投入スクリプト
 * 開発環境用のサンプルデータを生成
 */

const { Pool } = require('pg');
require('dotenv').config();

const pool = new Pool({
  host: process.env.DB_HOST || 'localhost',
  port: process.env.DB_PORT || 5432,
  database: process.env.DB_NAME || 'shaderop_db',
  user: process.env.DB_USER || 'shaderop_user',
  password: process.env.DB_PASSWORD || 'shaderop_password',
});

async function seedShopItems() {
  console.log('ショップアイテムをシード中...');

  const items = [
    {
      item_id: 'outfit_vip_01',
      item_name: 'VIP Exclusive Outfit',
      description: 'VIP限定衣装',
      category: 'Avatar',
      coin_price: 0,
      gem_price: 500,
      is_vip_only: true,
    },
    {
      item_id: 'hair_premium_01',
      item_name: 'Premium Hairstyle',
      description: 'プレミアムヘアスタイル',
      category: 'Avatar',
      coin_price: 1000,
      gem_price: 100,
      is_vip_only: false,
    },
    {
      item_id: 'board_theme_sunset',
      item_name: 'Sunset Board Theme',
      description: '夕焼けボードテーマ',
      category: 'BoardTheme',
      coin_price: 500,
      gem_price: 50,
      is_vip_only: false,
    },
    {
      item_id: 'stamp_pack_01',
      item_name: 'Cute Stamp Pack',
      description: 'かわいいスタンプパック',
      category: 'Stamp',
      coin_price: 300,
      gem_price: 30,
      is_vip_only: false,
    },
    {
      item_id: 'currency_gems_100',
      item_name: '100 Gems',
      description: 'ジェム100個パック',
      category: 'Currency',
      coin_price: 0,
      gem_price: 0,
      is_vip_only: false,
    },
    {
      item_id: 'vip_subscription_monthly',
      item_name: 'VIP Subscription (Monthly)',
      description: 'VIP会員権（1ヶ月）',
      category: 'VipSubscription',
      coin_price: 0,
      gem_price: 1000,
      is_vip_only: false,
    },
  ];

  for (const item of items) {
    await pool.query(
      `INSERT INTO shop_items (item_id, item_name, description, category, coin_price, gem_price, is_vip_only)
       VALUES ($1, $2, $3, $4, $5, $6, $7)
       ON CONFLICT (item_id) DO NOTHING`,
      [
        item.item_id,
        item.item_name,
        item.description,
        item.category,
        item.coin_price,
        item.gem_price,
        item.is_vip_only,
      ]
    );
  }

  console.log(`${items.length}個のショップアイテムをシードしました`);
}

async function seedTestUsers() {
  console.log('テストユーザーをシード中...');

  const testUsers = [
    {
      firebase_uid: 'test-user-001',
      display_name: 'TestPlayer1',
      email: 'test1@example.com',
      level: 10,
      experience: 2500,
      coins: 5000,
      gems: 500,
      is_vip: true,
    },
    {
      firebase_uid: 'test-user-002',
      display_name: 'TestPlayer2',
      email: 'test2@example.com',
      level: 5,
      experience: 800,
      coins: 2000,
      gems: 100,
      is_vip: false,
    },
    {
      firebase_uid: 'test-user-003',
      display_name: 'TestPlayer3',
      email: 'test3@example.com',
      level: 15,
      experience: 5000,
      coins: 10000,
      gems: 1000,
      is_vip: true,
    },
  ];

  for (const user of testUsers) {
    const result = await pool.query(
      `INSERT INTO users (firebase_uid, display_name, email, level, experience, coins, gems, is_vip)
       VALUES ($1, $2, $3, $4, $5, $6, $7, $8)
       ON CONFLICT (firebase_uid) DO NOTHING
       RETURNING user_id`,
      [
        user.firebase_uid,
        user.display_name,
        user.email,
        user.level,
        user.experience,
        user.coins,
        user.gems,
        user.is_vip,
      ]
    );

    if (result.rows.length > 0) {
      const userId = result.rows[0].user_id;

      // アバターデータ作成
      await pool.query(
        `INSERT INTO avatar_data (user_id)
         VALUES ($1)
         ON CONFLICT (user_id) DO NOTHING`,
        [userId]
      );

      // プレイヤー統計作成
      await pool.query(
        `INSERT INTO player_stats (user_id, total_matches, wins, losses, draws)
         VALUES ($1, $2, $3, $4, $5)
         ON CONFLICT (user_id) DO NOTHING`,
        [userId, Math.floor(Math.random() * 100), Math.floor(Math.random() * 60), Math.floor(Math.random() * 30), Math.floor(Math.random() * 10)]
      );
    }
  }

  console.log(`${testUsers.length}人のテストユーザーをシードしました`);
}

async function main() {
  try {
    console.log('シードデータ投入を開始...');

    await seedShopItems();
    await seedTestUsers();

    console.log('シードデータ投入が完了しました');
  } catch (error) {
    console.error('シードエラー:', error);
    process.exit(1);
  } finally {
    await pool.end();
  }
}

main();
