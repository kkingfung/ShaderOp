-- ShaderOp Database Schema
-- 作成日: 2026-03-01

-- UUIDエクステンション有効化
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ユーザーテーブル
CREATE TABLE users (
    user_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    firebase_uid VARCHAR(128) UNIQUE NOT NULL,
    display_name VARCHAR(50) NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    level INTEGER DEFAULT 1 CHECK (level >= 1),
    experience INTEGER DEFAULT 0 CHECK (experience >= 0),
    coins INTEGER DEFAULT 1000 CHECK (coins >= 0),
    gems INTEGER DEFAULT 100 CHECK (gems >= 0),
    vip_coins INTEGER DEFAULT 0 CHECK (vip_coins >= 0),
    is_vip BOOLEAN DEFAULT FALSE,
    vip_expiry_date TIMESTAMPTZ,
    last_login_date TIMESTAMPTZ DEFAULT NOW(),
    created_date TIMESTAMPTZ DEFAULT NOW(),
    updated_date TIMESTAMPTZ DEFAULT NOW()
);

-- アバターデータテーブル
CREATE TABLE avatar_data (
    avatar_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID UNIQUE NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    gender VARCHAR(20) DEFAULT 'Female' CHECK (gender IN ('Male', 'Female', 'NonBinary')),
    face_id VARCHAR(50) DEFAULT 'face_01',
    hairstyle_id VARCHAR(50) DEFAULT 'hair_01',
    hair_color VARCHAR(7) DEFAULT '#FFD700',
    skin_tone_id INTEGER DEFAULT 1 CHECK (skin_tone_id BETWEEN 1 AND 10),
    eye_color VARCHAR(7) DEFAULT '#0000FF',
    outfit_id VARCHAR(50) DEFAULT 'outfit_01',
    accessory_ids JSONB DEFAULT '[]'::jsonb,
    height_scale REAL DEFAULT 1.0 CHECK (height_scale BETWEEN 0.8 AND 1.2),
    created_date TIMESTAMPTZ DEFAULT NOW(),
    updated_date TIMESTAMPTZ DEFAULT NOW()
);

-- プレイヤー統計テーブル
CREATE TABLE player_stats (
    stats_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID UNIQUE NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    total_play_time_seconds INTEGER DEFAULT 0 CHECK (total_play_time_seconds >= 0),
    total_matches INTEGER DEFAULT 0 CHECK (total_matches >= 0),
    wins INTEGER DEFAULT 0 CHECK (wins >= 0),
    losses INTEGER DEFAULT 0 CHECK (losses >= 0),
    draws INTEGER DEFAULT 0 CHECK (draws >= 0),
    created_date TIMESTAMPTZ DEFAULT NOW(),
    updated_date TIMESTAMPTZ DEFAULT NOW()
);

-- フレンド関係テーブル
CREATE TABLE friendships (
    friendship_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id_1 UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    user_id_2 UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    friend_since TIMESTAMPTZ DEFAULT NOW(),
    UNIQUE(user_id_1, user_id_2),
    CHECK (user_id_1 < user_id_2)
);

-- フレンドリクエストテーブル
CREATE TABLE friend_requests (
    request_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    from_user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    to_user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    status VARCHAR(20) DEFAULT 'pending' CHECK (status IN ('pending', 'accepted', 'rejected')),
    request_date TIMESTAMPTZ DEFAULT NOW(),
    response_date TIMESTAMPTZ,
    UNIQUE(from_user_id, to_user_id)
);

-- ショップアイテムテーブル
CREATE TABLE shop_items (
    item_id VARCHAR(100) PRIMARY KEY,
    item_name VARCHAR(100) NOT NULL,
    description TEXT,
    category VARCHAR(50) NOT NULL CHECK (category IN ('Avatar', 'BoardTheme', 'Stamp', 'Currency', 'VipSubscription')),
    coin_price INTEGER DEFAULT 0 CHECK (coin_price >= 0),
    gem_price INTEGER DEFAULT 0 CHECK (gem_price >= 0),
    is_vip_only BOOLEAN DEFAULT FALSE,
    is_limited_time BOOLEAN DEFAULT FALSE,
    expiry_date TIMESTAMPTZ,
    thumbnail_url TEXT,
    is_active BOOLEAN DEFAULT TRUE,
    created_date TIMESTAMPTZ DEFAULT NOW(),
    updated_date TIMESTAMPTZ DEFAULT NOW()
);

-- トランザクション（購入履歴）テーブル
CREATE TABLE transactions (
    transaction_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    item_id VARCHAR(100) REFERENCES shop_items(item_id) ON DELETE SET NULL,
    amount_paid INTEGER NOT NULL CHECK (amount_paid >= 0),
    currency VARCHAR(20) NOT NULL CHECK (currency IN ('coins', 'gems', 'real_money')),
    status VARCHAR(20) DEFAULT 'pending' CHECK (status IN ('pending', 'completed', 'failed', 'refunded')),
    purchase_date TIMESTAMPTZ DEFAULT NOW(),
    metadata JSONB DEFAULT '{}'::jsonb
);

-- 所持アイテムテーブル
CREATE TABLE user_items (
    user_item_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    item_id VARCHAR(100) NOT NULL REFERENCES shop_items(item_id) ON DELETE CASCADE,
    acquired_date TIMESTAMPTZ DEFAULT NOW(),
    UNIQUE(user_id, item_id)
);

-- マッチセッションテーブル
CREATE TABLE match_sessions (
    match_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    session_id VARCHAR(100) UNIQUE NOT NULL,
    game_type VARCHAR(50) NOT NULL,
    room_name VARCHAR(100),
    max_players INTEGER DEFAULT 2 CHECK (max_players BETWEEN 2 AND 10),
    invite_code VARCHAR(10),
    status VARCHAR(20) DEFAULT 'waiting' CHECK (status IN ('waiting', 'starting', 'in_progress', 'finished', 'cancelled')),
    winner_id UUID REFERENCES users(user_id) ON DELETE SET NULL,
    loser_id UUID REFERENCES users(user_id) ON DELETE SET NULL,
    duration_seconds INTEGER CHECK (duration_seconds >= 0),
    start_time TIMESTAMPTZ DEFAULT NOW(),
    end_time TIMESTAMPTZ,
    created_date TIMESTAMPTZ DEFAULT NOW()
);

-- マッチ参加者テーブル
CREATE TABLE match_players (
    match_player_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    match_id UUID NOT NULL REFERENCES match_sessions(match_id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    is_ready BOOLEAN DEFAULT FALSE,
    joined_date TIMESTAMPTZ DEFAULT NOW(),
    UNIQUE(match_id, user_id)
);

-- セーブデータテーブル
CREATE TABLE save_data (
    save_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID UNIQUE NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    profile_data JSONB NOT NULL,
    last_sync_date TIMESTAMPTZ DEFAULT NOW()
);

-- インデックス作成（パフォーマンス最適化）
CREATE INDEX idx_users_firebase_uid ON users(firebase_uid);
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_display_name ON users(display_name);
CREATE INDEX idx_friendships_user1 ON friendships(user_id_1);
CREATE INDEX idx_friendships_user2 ON friendships(user_id_2);
CREATE INDEX idx_friend_requests_from ON friend_requests(from_user_id);
CREATE INDEX idx_friend_requests_to ON friend_requests(to_user_id);
CREATE INDEX idx_friend_requests_status ON friend_requests(status);
CREATE INDEX idx_transactions_user ON transactions(user_id);
CREATE INDEX idx_transactions_status ON transactions(status);
CREATE INDEX idx_user_items_user ON user_items(user_id);
CREATE INDEX idx_match_sessions_status ON match_sessions(status);
CREATE INDEX idx_match_sessions_game_type ON match_sessions(game_type);
CREATE INDEX idx_match_players_match ON match_players(match_id);
CREATE INDEX idx_match_players_user ON match_players(user_id);

-- updated_date自動更新トリガー関数
CREATE OR REPLACE FUNCTION update_updated_date_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_date = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- トリガー設定
CREATE TRIGGER update_users_updated_date BEFORE UPDATE ON users
    FOR EACH ROW EXECUTE FUNCTION update_updated_date_column();

CREATE TRIGGER update_avatar_data_updated_date BEFORE UPDATE ON avatar_data
    FOR EACH ROW EXECUTE FUNCTION update_updated_date_column();

CREATE TRIGGER update_player_stats_updated_date BEFORE UPDATE ON player_stats
    FOR EACH ROW EXECUTE FUNCTION update_updated_date_column();

CREATE TRIGGER update_shop_items_updated_date BEFORE UPDATE ON shop_items
    FOR EACH ROW EXECUTE FUNCTION update_updated_date_column();

COMMENT ON TABLE users IS 'ユーザー基本情報';
COMMENT ON TABLE avatar_data IS 'アバターカスタマイズデータ';
COMMENT ON TABLE player_stats IS 'プレイヤー統計情報';
COMMENT ON TABLE friendships IS 'フレンド関係（双方向）';
COMMENT ON TABLE friend_requests IS 'フレンドリクエスト';
COMMENT ON TABLE shop_items IS 'ショップアイテムマスタ';
COMMENT ON TABLE transactions IS '購入トランザクション履歴';
COMMENT ON TABLE user_items IS 'ユーザー所持アイテム';
COMMENT ON TABLE match_sessions IS 'マッチセッション情報';
COMMENT ON TABLE match_players IS 'マッチ参加プレイヤー';
COMMENT ON TABLE save_data IS 'クラウドセーブデータ';
