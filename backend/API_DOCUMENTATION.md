# ShaderOp REST API ドキュメント

## 概要

ShaderOpモバイルゲームのバックエンドREST APIドキュメント。

**ベースURL**: `http://localhost:3000/api`

**認証方式**: Firebase JWT Bearer Token

**レスポンス形式**: JSON

---

## 認証フロー

1. クライアント側でFirebase Authenticationを使用してログイン
2. Firebase IDトークンを取得
3. APIリクエストの`Authorization`ヘッダーにトークンを含める

```
Authorization: Bearer <FIREBASE_ID_TOKEN>
```

---

## エラーコード

| ステータスコード | エラーコード | 説明 |
|---|---|---|
| 400 | VALIDATION_ERROR | リクエストデータが無効 |
| 400 | INSUFFICIENT_FUNDS | 通貨が不足 |
| 401 | UNAUTHORIZED | 認証が必要 |
| 401 | TOKEN_EXPIRED | トークンの有効期限切れ |
| 403 | FORBIDDEN | アクセス権限なし |
| 403 | VIP_ONLY | VIP限定コンテンツ |
| 404 | NOT_FOUND | リソースが見つからない |
| 429 | RATE_LIMIT_EXCEEDED | レート制限超過 |
| 500 | INTERNAL_ERROR | サーバーエラー |

---

## レート制限

### 一般APIエンドポイント
- **制限**: 15分間に100リクエスト
- **ヘッダー**: `X-RateLimit-Limit`, `X-RateLimit-Remaining`, `X-RateLimit-Reset`

### 認証エンドポイント
- **制限**: 15分間に5リクエスト

### ショップ購入エンドポイント
- **制限**: 1分間に10リクエスト

---

## エンドポイント一覧

### 1. ユーザー関連 (`/api/users`)

#### **POST /api/users/register**
新規ユーザー登録

**認証**: 不要

**リクエストボディ**:
```json
{
  "firebaseUid": "firebase-uid-string",
  "displayName": "PlayerName",
  "email": "player@example.com"
}
```

**レスポンス (201)**:
```json
{
  "success": true,
  "message": "ユーザー登録完了",
  "data": {
    "user_id": "uuid",
    "display_name": "PlayerName",
    "created_date": "2026-03-06T10:00:00Z"
  }
}
```

---

#### **GET /api/users/me**
自分のプロフィール取得

**認証**: 必須

**レスポンス (200)**:
```json
{
  "success": true,
  "data": {
    "userId": "uuid",
    "displayName": "PlayerName",
    "level": 10,
    "experience": 2500,
    "avatar": {
      "gender": "Female",
      "faceId": "face_01",
      "hairstyleId": "hair_05",
      "hairColor": "#FFD700",
      "skinToneId": 3,
      "eyeColor": "#0000FF",
      "outfitId": "outfit_01",
      "accessoryIds": ["accessory_01"],
      "heightScale": 1.0
    },
    "stats": {
      "totalPlayTimeSeconds": 36000,
      "totalMatches": 50,
      "wins": 30,
      "losses": 15,
      "draws": 5,
      "winRate": "60.00"
    },
    "isVip": true,
    "vipExpiryDate": "2026-04-06T10:00:00Z",
    "lastLoginDate": "2026-03-06T10:00:00Z",
    "createdDate": "2026-01-01T10:00:00Z"
  }
}
```

---

#### **GET /api/users/:userId**
他ユーザーのプロフィール取得

**認証**: 必須

**パラメータ**:
- `userId` (path): ユーザーID (UUID)

**レスポンス (200)**: `/api/users/me`と同じ形式

---

#### **PUT /api/users/me**
プロフィール更新

**認証**: 必須

**リクエストボディ**:
```json
{
  "displayName": "NewPlayerName",
  "avatar": {
    "hairstyleId": "hair_10",
    "hairColor": "#FF0000",
    "outfitId": "outfit_vip_01"
  }
}
```

**レスポンス (200)**:
```json
{
  "success": true,
  "message": "プロフィール更新完了",
  "data": { /* 更新後のプロフィール */ }
}
```

---

#### **GET /api/users/search?q=PlayerName**
ユーザー検索

**認証**: 必須

**クエリパラメータ**:
- `q` (必須): 検索クエリ
- `limit` (任意): 結果数 (デフォルト: 20)

**レスポンス (200)**:
```json
{
  "success": true,
  "data": [
    {
      "user_id": "uuid",
      "display_name": "PlayerName",
      "level": 10,
      "is_vip": true
    }
  ]
}
```

---

#### **GET /api/users/me/balance**
通貨残高取得

**認証**: 必須

**レスポンス (200)**:
```json
{
  "success": true,
  "data": {
    "coins": 5000,
    "gems": 500,
    "vip_coins": 100
  }
}
```

---

### 2. フレンド関連 (`/api/friends`)

#### **GET /api/friends**
フレンドリスト取得

**認証**: 必須

**レスポンス (200)**:
```json
{
  "success": true,
  "data": [
    {
      "userId": "uuid",
      "displayName": "FriendName",
      "level": 15,
      "isOnline": true,
      "isVip": false,
      "lastOnlineDate": "2026-03-06T10:00:00Z",
      "friendSince": "2026-02-01T10:00:00Z"
    }
  ]
}
```

---

#### **POST /api/friends/requests**
フレンドリクエスト送信

**認証**: 必須

**リクエストボディ**:
```json
{
  "targetUserId": "uuid"
}
```

**レスポンス (201)**:
```json
{
  "success": true,
  "message": "フレンドリクエストを送信しました",
  "data": {
    "request_id": "uuid",
    "from_user_id": "uuid",
    "to_user_id": "uuid",
    "status": "pending",
    "request_date": "2026-03-06T10:00:00Z"
  }
}
```

---

#### **GET /api/friends/requests/incoming**
受信したフレンドリクエスト一覧

**認証**: 必須

**レスポンス (200)**:
```json
{
  "success": true,
  "data": [
    {
      "request_id": "uuid",
      "from_user_id": "uuid",
      "from_display_name": "SenderName",
      "level": 10,
      "is_vip": false,
      "request_date": "2026-03-06T10:00:00Z"
    }
  ]
}
```

---

#### **POST /api/friends/requests/:requestId/accept**
フレンドリクエスト承認

**認証**: 必須

**パラメータ**:
- `requestId` (path): リクエストID (UUID)

**レスポンス (200)**:
```json
{
  "success": true,
  "message": "フレンドリクエストを承認しました",
  "data": {
    "user_id": "uuid",
    "display_name": "NewFriend",
    "level": 10
  }
}
```

---

#### **POST /api/friends/requests/:requestId/reject**
フレンドリクエスト拒否

**認証**: 必須

**パラメータ**:
- `requestId` (path): リクエストID (UUID)

**レスポンス (200)**:
```json
{
  "success": true,
  "message": "フレンドリクエストを拒否しました"
}
```

---

#### **DELETE /api/friends/:friendId**
フレンド削除

**認証**: 必須

**パラメータ**:
- `friendId` (path): フレンドのユーザーID (UUID)

**レスポンス (200)**:
```json
{
  "success": true,
  "message": "フレンドを削除しました"
}
```

---

### 3. ショップ関連 (`/api/shop`)

#### **GET /api/shop/items**
ショップアイテム一覧

**認証**: 必須

**クエリパラメータ**:
- `category` (任意): カテゴリフィルタ (`Avatar`, `BoardTheme`, `Stamp`, `Currency`, `VipSubscription`)

**レスポンス (200)**:
```json
{
  "success": true,
  "data": [
    {
      "item_id": "outfit_vip_01",
      "item_name": "VIP Exclusive Outfit",
      "description": "VIP限定衣装",
      "category": "Avatar",
      "coin_price": 0,
      "gem_price": 500,
      "is_vip_only": true,
      "is_limited_time": false,
      "expiry_date": null,
      "thumbnail_url": "https://example.com/outfit.png",
      "is_active": true
    }
  ]
}
```

---

#### **GET /api/shop/items/:itemId**
アイテム詳細

**認証**: 必須

**パラメータ**:
- `itemId` (path): アイテムID

**レスポンス (200)**: アイテムオブジェクト

---

#### **POST /api/shop/purchase**
アイテム購入

**認証**: 必須

**レート制限**: 1分間に10リクエスト

**リクエストボディ**:
```json
{
  "itemId": "outfit_vip_01",
  "currencyType": "gems"
}
```

**レスポンス (200)**:
```json
{
  "success": true,
  "message": "アイテムを購入しました",
  "data": {
    "transaction": {
      "transactionId": "uuid",
      "itemId": "outfit_vip_01",
      "amountPaid": 500,
      "currency": "gems",
      "status": "completed",
      "purchaseDate": "2026-03-06T10:00:00Z"
    },
    "newBalance": {
      "coins": 5000,
      "gems": 0,
      "vip_coins": 0
    }
  }
}
```

---

#### **GET /api/shop/my-items**
所持アイテム一覧

**認証**: 必須

**レスポンス (200)**:
```json
{
  "success": true,
  "data": [
    {
      "item_id": "outfit_vip_01",
      "item_name": "VIP Exclusive Outfit",
      "category": "Avatar",
      "acquired_date": "2026-03-06T10:00:00Z"
    }
  ]
}
```

---

#### **GET /api/shop/transactions**
トランザクション履歴

**認証**: 必須

**クエリパラメータ**:
- `limit` (任意): 結果数 (デフォルト: 50)
- `offset` (任意): オフセット (デフォルト: 0)

**レスポンス (200)**:
```json
{
  "success": true,
  "data": [
    {
      "transaction_id": "uuid",
      "item_id": "outfit_vip_01",
      "item_name": "VIP Exclusive Outfit",
      "amount_paid": 500,
      "currency": "gems",
      "status": "completed",
      "purchase_date": "2026-03-06T10:00:00Z"
    }
  ]
}
```

---

### 4. マッチ関連 (`/api/matches`)

#### **POST /api/matches**
マッチセッション作成

**認証**: 必須

**リクエストボディ**:
```json
{
  "sessionId": "photon-session-id",
  "gameType": "HexChess",
  "players": [
    { "userId": "uuid" },
    { "userId": "uuid" }
  ],
  "roomName": "Room 1",
  "inviteCode": "ABC123"
}
```

**レスポンス (201)**:
```json
{
  "success": true,
  "message": "マッチセッションを作成しました",
  "data": {
    "match_id": "uuid",
    "session_id": "photon-session-id",
    "created_date": "2026-03-06T10:00:00Z"
  }
}
```

---

#### **PUT /api/matches/:sessionId/result**
マッチ結果更新

**認証**: 必須

**パラメータ**:
- `sessionId` (path): セッションID

**リクエストボディ**:
```json
{
  "winnerId": "uuid",
  "loserId": "uuid",
  "gameType": "HexChess",
  "duration": 300
}
```

**レスポンス (200)**:
```json
{
  "success": true,
  "message": "マッチ結果を記録しました",
  "data": {
    "rewards": {
      "coins": 50,
      "experience": 100
    },
    "newStats": {
      "totalMatches": 51,
      "wins": 31,
      "losses": 15,
      "draws": 5,
      "winRate": "60.78"
    }
  }
}
```

---

#### **GET /api/matches/history**
マッチ履歴取得

**認証**: 必須

**クエリパラメータ**:
- `limit` (任意): 結果数 (デフォルト: 20)
- `offset` (任意): オフセット (デフォルト: 0)

**レスポンス (200)**:
```json
{
  "success": true,
  "data": {
    "matches": [
      {
        "matchId": "uuid",
        "gameType": "HexChess",
        "opponent": {
          "userId": "uuid",
          "displayName": "OpponentName"
        },
        "result": "win",
        "duration": 300,
        "playedDate": "2026-03-06T10:00:00Z"
      }
    ],
    "total": 50
  }
}
```

---

### 5. セーブデータ関連 (`/api/savedata`)

#### **GET /api/savedata**
セーブデータ取得

**認証**: 必須

**レスポンス (200)**:
```json
{
  "success": true,
  "data": {
    "profile": {
      "customField1": "value",
      "customField2": 123
    },
    "lastSyncDate": "2026-03-06T10:00:00Z"
  }
}
```

---

#### **POST /api/savedata**
セーブデータ保存

**認証**: 必須

**リクエストボディ**:
```json
{
  "profileData": {
    "customField1": "new value",
    "customField2": 456
  }
}
```

**レスポンス (200)**:
```json
{
  "success": true,
  "message": "セーブデータを保存しました",
  "data": {
    "success": true,
    "lastSyncDate": "2026-03-06T10:30:00Z"
  }
}
```

---

#### **DELETE /api/savedata**
セーブデータ削除

**認証**: 必須

**レスポンス (200)**:
```json
{
  "success": true,
  "message": "セーブデータを削除しました"
}
```

---

## 開発用ヘルスチェック

#### **GET /health**
サーバーヘルスチェック

**認証**: 不要

**レスポンス (200)**:
```json
{
  "status": "OK",
  "timestamp": "2026-03-06T10:00:00Z",
  "uptime": 123456
}
```

---

## curlコマンド例

### ユーザー登録
```bash
curl -X POST http://localhost:3000/api/users/register \
  -H "Content-Type: application/json" \
  -d '{
    "firebaseUid": "test-firebase-uid",
    "displayName": "TestPlayer",
    "email": "test@example.com"
  }'
```

### プロフィール取得
```bash
curl -X GET http://localhost:3000/api/users/me \
  -H "Authorization: Bearer YOUR_FIREBASE_TOKEN"
```

### アイテム購入
```bash
curl -X POST http://localhost:3000/api/shop/purchase \
  -H "Authorization: Bearer YOUR_FIREBASE_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "itemId": "outfit_vip_01",
    "currencyType": "gems"
  }'
```

### フレンドリクエスト送信
```bash
curl -X POST http://localhost:3000/api/friends/requests \
  -H "Authorization: Bearer YOUR_FIREBASE_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "targetUserId": "target-user-uuid"
  }'
```

---

## セットアップ手順

### 1. 環境変数設定

`.env`ファイルを作成:
```bash
cp .env.example .env
```

Firebase Admin SDKの認証情報を設定してください。

### 2. Docker Compose起動

```bash
docker-compose up -d
```

PostgreSQL、Redis、Node.jsサーバーが起動します。

### 3. データベースマイグレーション

初回のみ自動で`init.sql`が実行されます。

### 4. シードデータ投入（任意）

```bash
npm run seed
```

テストユーザーとショップアイテムが登録されます。

### 5. サーバー起動確認

```bash
curl http://localhost:3000/health
```

---

## Unity側の実装例

```csharp
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class ShaderOpAPI
{
    private const string BASE_URL = "http://localhost:3000/api";
    private string _firebaseToken;

    public void SetToken(string token)
    {
        _firebaseToken = token;
    }

    public async Task<UserProfile> GetMyProfile()
    {
        using var request = UnityWebRequest.Get($"{BASE_URL}/users/me");
        request.SetRequestHeader("Authorization", $"Bearer {_firebaseToken}");

        await request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var response = JsonUtility.FromJson<APIResponse<UserProfile>>(request.downloadHandler.text);
            return response.data;
        }

        throw new System.Exception(request.error);
    }

    public async Task PurchaseItem(string itemId, string currencyType)
    {
        var body = JsonUtility.ToJson(new { itemId, currencyType });

        using var request = new UnityWebRequest($"{BASE_URL}/shop/purchase", "POST");
        request.SetRequestHeader("Authorization", $"Bearer {_firebaseToken}");
        request.SetRequestHeader("Content-Type", "application/json");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
        request.downloadHandler = new DownloadHandlerBuffer();

        await request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            throw new System.Exception(request.error);
        }
    }
}
```

---

## 今後の拡張

- [ ] WebSocket対応（リアルタイムチャット）
- [ ] ランキングシステム
- [ ] アチーブメント機能
- [ ] イベント管理
- [ ] 通知システム
- [ ] 管理者用API

---

**最終更新日**: 2026-03-06
