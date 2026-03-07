# ShaderOp Backend Server

ShaderOpモバイルゲームのNode.js REST APIバックエンドサーバー

## 技術スタック

- **Node.js 18+** - JavaScriptランタイム
- **Express 4** - Webフレームワーク
- **PostgreSQL 16** - リレーショナルデータベース
- **Redis 7** - セッション管理・キャッシュ
- **Firebase Admin SDK** - JWT認証
- **Docker Compose** - コンテナオーケストレーション

## ディレクトリ構造

```
backend/
├── src/
│   ├── config/           # 設定ファイル (DB, Redis, Firebase)
│   ├── middleware/       # ミドルウェア (認証, バリデーション, レート制限)
│   ├── routes/           # APIルート定義
│   ├── services/         # ビジネスロジック層
│   ├── utils/            # ユーティリティ (ロガー, エラー)
│   └── server.js         # エントリーポイント
├── migrations/           # データベースマイグレーション
│   ├── init.sql          # 初期スキーマ
│   └── seed.js           # シードデータ
├── logs/                 # ログファイル
├── docker-compose.yml    # Docker設定
├── Dockerfile            # Node.jsイメージ
├── package.json          # npm依存関係
├── .env.example          # 環境変数テンプレート
└── API_DOCUMENTATION.md  # API仕様書
```

## セットアップ

### 1. 環境変数設定

```bash
cp .env.example .env
```

`.env`ファイルを編集してFirebase認証情報を設定:

```env
FIREBASE_PROJECT_ID=your-project-id
FIREBASE_PRIVATE_KEY="-----BEGIN PRIVATE KEY-----\n...\n-----END PRIVATE KEY-----\n"
FIREBASE_CLIENT_EMAIL=firebase-adminsdk@your-project.iam.gserviceaccount.com
```

### 2. Docker Composeで起動

```bash
docker-compose up -d
```

以下が自動的に起動します:
- PostgreSQL (ポート 5432)
- Redis (ポート 6379)
- Node.js APIサーバー (ポート 3000)

### 3. データベース初期化確認

初回起動時に`migrations/init.sql`が自動実行されます。

### 4. シードデータ投入（任意）

テストユーザーとショップアイテムを作成:

```bash
docker-compose exec api npm run seed
```

### 5. ヘルスチェック

```bash
curl http://localhost:3000/health
```

レスポンス:
```json
{
  "status": "OK",
  "timestamp": "2026-03-06T10:00:00Z",
  "uptime": 123
}
```

## 開発モード

### ローカル開発（Docker不使用）

```bash
# 依存関係インストール
npm install

# PostgreSQL・Redisを別途起動しておく必要があります

# 開発サーバー起動 (nodemon)
npm run dev
```

### ログ確認

```bash
# コンテナログ
docker-compose logs -f api

# ファイルログ
tail -f logs/app.log
```

## npm スクリプト

| コマンド | 説明 |
|---|---|
| `npm start` | 本番モード起動 |
| `npm run dev` | 開発モード起動 (nodemon) |
| `npm run seed` | シードデータ投入 |
| `npm test` | テスト実行 |
| `npm run lint` | ESLint実行 |
| `npm run format` | Prettier実行 |

## データベーススキーマ

### 主要テーブル

- **users** - ユーザー基本情報
- **avatar_data** - アバターカスタマイズデータ
- **player_stats** - プレイヤー統計
- **friendships** - フレンド関係
- **friend_requests** - フレンドリクエスト
- **shop_items** - ショップアイテムマスタ
- **transactions** - 購入トランザクション履歴
- **user_items** - ユーザー所持アイテム
- **match_sessions** - マッチセッション情報
- **match_players** - マッチ参加プレイヤー
- **save_data** - クラウドセーブデータ

詳細は`migrations/init.sql`を参照。

## API エンドポイント

詳細なAPI仕様は[API_DOCUMENTATION.md](./API_DOCUMENTATION.md)を参照。

### 主要エンドポイント

| エンドポイント | メソッド | 説明 | 認証 |
|---|---|---|---|
| `/api/users/register` | POST | ユーザー登録 | 不要 |
| `/api/users/me` | GET | 自分のプロフィール取得 | 必須 |
| `/api/friends` | GET | フレンドリスト取得 | 必須 |
| `/api/shop/items` | GET | ショップアイテム一覧 | 必須 |
| `/api/shop/purchase` | POST | アイテム購入 | 必須 |
| `/api/matches/history` | GET | マッチ履歴取得 | 必須 |
| `/api/savedata` | GET/POST | クラウドセーブ管理 | 必須 |

## セキュリティ

### 実装済み

- **Firebase JWT認証** - すべての保護されたエンドポイントで必須
- **Helmet.js** - セキュリティヘッダー設定
- **レート制限** - DDoS防止
- **バリデーション** - express-validatorによる入力検証
- **CORS** - クロスオリジン設定
- **SQL Injection対策** - パラメータ化クエリ使用

### 推奨事項

- 本番環境では必ずHTTPSを使用
- 環境変数は`.env`ファイルで管理（gitにコミットしない）
- Firebase Admin SDKのサービスアカウントキーは厳重に管理
- PostgreSQLのパスワードは強力なものを使用
- Redis認証を有効化（本番環境）

## トラブルシューティング

### PostgreSQL接続エラー

```bash
# コンテナ状態確認
docker-compose ps

# PostgreSQLログ確認
docker-compose logs postgres
```

### Redis接続エラー

```bash
# Redisログ確認
docker-compose logs redis

# Redis CLI接続テスト
docker-compose exec redis redis-cli ping
```

### Firebase認証エラー

- `.env`のFirebase認証情報が正しいか確認
- プライベートキーの改行文字(`\n`)が正しくエスケープされているか確認

## テスト

```bash
# ユニットテスト実行
npm test

# カバレッジレポート生成
npm test -- --coverage
```

## デプロイ

### 本番環境設定

1. 環境変数を本番用に設定
2. `NODE_ENV=production`に設定
3. ログレベルを`warn`または`error`に変更
4. SSL/TLS証明書設定
5. リバースプロキシ（Nginx等）の設定

### Dockerイメージビルド

```bash
docker build -t shaderop-backend:latest .
```

## ライセンス

MIT

## サポート

問題が発生した場合は、GitHubのIssueを作成してください。
