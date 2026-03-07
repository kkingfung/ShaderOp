# ShaderOp

**統合スタイライズドシェーダーライブラリ for モバイルキャラクターカスタマイズゲーム**

[![Unity](https://img.shields.io/badge/Unity-6000.3.9f1-black.svg)](https://unity.com/)
[![URP](https://img.shields.io/badge/URP-17.3.0-blue.svg)](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@17.3/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

---

## 📋 目次

- [プロジェクト概要](#プロジェクト概要)
- [クイックスタート](#クイックスタート)
- [プロジェクト構造](#プロジェクト構造)
- [アーキテクチャ](#アーキテクチャ)
- [セットアップガイド](#セットアップガイド)
- [開発ワークフロー](#開発ワークフロー)
- [実装状況](#実装状況)
- [Claude Code 統合](#claude-code-統合)
- [技術スタック](#技術スタック)
- [セキュリティ](#セキュリティ)
- [トラブルシューティング](#トラブルシューティング)
- [参考リソース](#参考リソース)

---

## プロジェクト概要

ShaderOpは、**縦画面向けオンラインソーシャルモバイルゲーム**のUnityプロジェクトです。

### 🎮 ゲームコンセプト

#### ジャンル: カジュアル・ソーシャル（Cocone系）
- **参考作品**: ココネ「ポケコロ」「リヴリーアイランド」
- **画面向き**: 縦画面（Portrait）専用
- **操作**: 片手でプレイ可能なシンプルタッチ操作
- **プレイ時間**: 短時間セッション + 長期育成要素
- **ソーシャル性**:
  - プレイヤー間のテキストチャット
  - フレンド機能
  - ギフト交換
  - 訪問・いいね機能
  - マルチプレイ対戦

#### ゲーム内容: ヘックスボードゲーム集 + ソーシャル要素
- **メインコンテンツ**: リバーシ、チェッカー、チェス、Tic-Tac-Toe等（計10種予定）
- **ビジュアル**: スタイライズドで可愛い2Dグラフィック
- **UI**: 縦画面に最適化されたレスポンシブレイアウト
- **プレイモード**:
  - ソロプレイ（AI対戦、練習モード）
  - オンライン対戦（リアルタイム・ターン制）
  - フレンド対戦（招待・ランダムマッチ）

#### ソーシャル要素
- **アバターカスタマイズ**:
  - 顔パーツ、髪型、服装、アクセサリー
  - ガチャ・ショップでアイテム入手
  - 課金・無料コンテンツ両対応
- **コミュニケーション**:
  - リアルタイムチャット
  - スタンプ・エモート
  - プレイヤープロフィール訪問
  - フレンドリスト管理
- **コレクション要素**:
  - ボードテーマ・背景のアンロック
  - アチーブメント・称号システム
  - イベント限定アイテム

### 🎯 技術目標

- **ターゲットデバイス**: エントリー〜中級モバイル端末（iPhone SE 2 / Galaxy A シリーズ以上）
- **画面向き**: Portrait（縦画面）固定
- **パフォーマンス**: 60fps安定動作
- **容量**: 初回DL 100MB以下、アセットバンドル対応
- **オンライン機能**:
  - リアルタイムマルチプレイ（WebSocket/Photon等）
  - チャット・メッセージング
  - フレンド・ソーシャル機能
  - サーバー同期セーブデータ
- **シェーダー戦略**: 軽量なスタイライズドシェーダー、モバイル最適化優先
- **UI**: UI Toolkit使用、縦画面レスポンシブデザイン
- **収益化**:
  - ガチャシステム
  - アイテムショップ
  - 広告（リワード動画）
  - サブスクリプション（VIP機能）

---

## クイックスタート

### 前提条件

- **Unity**: Unity 6 (6000.3.9f1 推奨)
- **Git**: 2.30 以降
- **IDE**: Visual Studio Code / Visual Studio 2022 / JetBrains Rider
- **Python**: 3.11 以上（MCP統合用、オプション）
- **Node.js**: 18 以上（MCP統合用、オプション）

### 1️⃣ リポジトリのクローン

```bash
git clone <repository-url>
cd ShaderOp
```

### 2️⃣ Unity プロジェクトを開く

Unity Hubから `ShaderOp/ShaderOptimizer/` フォルダを開く

### 3️⃣ 自動セットアップ（初回のみ）

Unityエディタのメニューバーから:

```
ShaderOp > Setup > Create All Scenes
ShaderOp > Setup > Create All Prefabs
ShaderOp > Setup > Validate Project Setup
```

これで以下が自動生成されます:
- **MainMenu.unity** - メインメニューシーン
- **TicTacToeHex.unity** - Tic-Tac-Toeミニゲームシーン
- **RoomDecoration.unity** - 部屋デコレーションシーン
- **HexTile.prefab** - ヘックスタイルプレハブ

### 4️⃣ テストを実行

```
Window > General > Test Runner > Run All
```

✅ 40個のユニットテストが全てパスすることを確認

---

## プロジェクト構造

```
ShaderOp/
├── .claude/                      # Claude Code AI設定
│   ├── agents/                   # 10個のエージェント定義
│   ├── skills/                   # 10個のスキルパターン
│   ├── commands/                 # カスタムコマンド
│   ├── rules/                    # コーディング規約
│   └── hooks/                    # 自動化フック
│
├── ShaderOptimizer/              # Unity プロジェクト ルート
│   ├── Assets/
│   │   ├── Shaders/              # シェーダーライブラリ（開発中）
│   │   │   ├── ShaderGraphs/     # Shader Graphテンプレート
│   │   │   │   ├── Character/    # キャラクター用
│   │   │   │   └── Environment/  # 環境用
│   │   │   └── HLSL/             # 再利用可能HLSL関数
│   │   │
│   │   ├── Scripts/              # C#スクリプト
│   │   │   ├── Runtime/
│   │   │   │   ├── Core/         # GameManager等
│   │   │   │   ├── Customization/ # キャラクター・部屋カスタマイズ
│   │   │   │   └── Minigames/    # ヘックスボードゲーム
│   │   │   └── Editor/
│   │   │       ├── MCPBridge/    # MCP統合ブリッジ
│   │   │       └── GameSetupUtility.cs
│   │   │
│   │   ├── Materials/            # マテリアル管理
│   │   │   ├── Character/
│   │   │   ├── Environment/
│   │   │   └── Minigames/
│   │   │
│   │   ├── Tests/                # Unity Test Framework
│   │   │   └── Runtime/
│   │   │       ├── HexGridTests.cs
│   │   │       └── CharacterCustomizationTests.cs
│   │   │
│   │   ├── Scenes/               # Unityシーン
│   │   ├── Prefabs/              # プレハブ
│   │   │
│   │   └── [参考アセット]/        # Git除外
│   │       ├── SD Unity-Chan Haon Custom/  # トゥーンシェーダー参考
│   │       ├── SoStylized/                 # 環境アセット
│   │       ├── 8Set/                       # ヘックスタイル
│   │       └── TomatocolCharacterVarietyPackVol1DEMO/
│   │
│   ├── Packages/
│   │   └── manifest.json         # パッケージ依存関係
│   │
│   └── ProjectSettings/          # Unity設定
│
├── CLAUDE.md                     # Claude Code使用ガイド
├── README.md                     # このファイル
└── IMPLEMENTATION_STATUS.md      # 実装進捗レポート
```

---

## アーキテクチャ

### プロジェクトタイプ

**オンラインソーシャル縦画面モバイルゲーム（Cocone系）** - ヘックスボードゲーム集 + チャット・カスタマイズ

### 設計パターン: MVC (Model-View-Controller) + サービス層

#### 理由
- ✅ **明確な責任分離** - ゲームロジック、UI、入力処理、ネットワーク処理が独立
- ✅ **テスト可能** - ゲームルールをUIから分離してテスト
- ✅ **拡張性** - 10種のボードゲーム + ソーシャル機能に対応
- ✅ **Unity親和性** - MonoBehaviourと相性が良い
- ✅ **オンライン対応** - サービス層でネットワーク通信を抽象化

### サービスアーキテクチャ

```
クライアント層（Unity）
├── UI Layer (View)
│   ├── MainMenuUI
│   ├── GameBoardUI
│   ├── ChatUI
│   └── CustomizationUI
│
├── Controller Layer
│   ├── GameController
│   ├── InputController
│   └── SocialController
│
├── Model Layer
│   ├── GameLogic (ローカル)
│   └── PlayerData (同期)
│
└── Service Layer
    ├── NetworkService - WebSocket/REST通信
    ├── ChatService - チャット機能
    ├── FriendService - フレンド管理
    ├── MatchmakingService - マッチング
    ├── SaveDataService - セーブデータ同期
    └── IAPService - 課金処理

サーバー層（バックエンド）
├── Game Server - ゲームロジック・状態管理
├── Chat Server - リアルタイムメッセージング
├── Database - ユーザーデータ・フレンド・履歴
└── API Gateway - 認証・レート制限
```

### MVC実装例

#### Model（ゲームロジック・データ）

```csharp
namespace ShaderOp.Minigames
{
    /// <summary>
    /// ヘックスリバーシのゲームモデル
    /// </summary>
    public class HexReversiModel : HexBoardGameModel
    {
        private readonly HexGrid _grid;
        private Player _currentPlayer;

        /// <summary>
        /// 指定位置にピースを配置できるか判定
        /// </summary>
        public bool CanPlacePiece(HexCoordinate coord)
        {
            // ゲームルールロジック
            return IsValidMove(coord, _currentPlayer);
        }

        /// <summary>
        /// ピース配置と反転処理
        /// </summary>
        public void PlacePiece(HexCoordinate coord)
        {
            // ピース配置
            // 挟まれたピースを反転
            OnBoardChanged?.Invoke();
        }
    }
}
```

#### View（UI表示）

```csharp
namespace ShaderOp.Minigames
{
    /// <summary>
    /// ヘックスボードの視覚表示
    /// </summary>
    public class HexReversiView : MonoBehaviour
    {
        [SerializeField] private HexTileVisualizer _tilePrefab;

        /// <summary>
        /// ボード状態を視覚的に更新
        /// </summary>
        public void UpdateBoard(HexGrid grid)
        {
            // タイルの色・状態を更新
            foreach (var tile in grid.Tiles)
            {
                UpdateTileVisual(tile);
            }
        }

        /// <summary>
        /// 縦画面レイアウトに最適化
        /// </summary>
        private void AdjustForPortraitLayout()
        {
            // ボードを画面上部に配置
            // 操作UIを画面下部に配置
        }
    }
}
```

#### Controller（ViewとModelの仲介）

```csharp
namespace ShaderOp.Minigames
{
    /// <summary>
    /// ヘックスリバーシController
    /// </summary>
    public class HexReversiController : MonoBehaviour
    {
        private HexReversiModel _model;
        private HexReversiView _view;

        /// <summary>
        /// タッチ入力処理（縦画面最適化）
        /// </summary>
        private void HandleTouchInput()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                HexCoordinate coord = ScreenToHexCoord(touch.position);

                if (_model.CanPlacePiece(coord))
                {
                    _model.PlacePiece(coord);
                    _view.UpdateBoard(_model.Grid);
                }
            }
        }
    }
}
```

### Assembly Definition による分離

```
ShaderOp.Runtime
├── 依存: Unity.Mathematics, Unity.Burst, URP
├── 用途: ランタイムロジック、シェーダー制御
└── パターン: Utility Classes, MonoBehaviour Controllers, ScriptableObject Configs

ShaderOp.Editor
├── 依存: ShaderOp.Runtime
├── 用途: カスタムインスペクター、エディターツール、バリデーター
└── パターン: Editor Windows, Custom Inspectors, Asset Processors

ShaderOp.Tests.Runtime
├── 依存: ShaderOp.Runtime, UnityEngine.TestRunner
└── 用途: Play Modeテスト
```

### シェーダー設計方針

#### モバイル最適化・スタイライズドアプローチ

**基本方針:**
- **軽量第一**: エントリーレベル端末でも60fps維持
- **可愛いビジュアル**: Cocone系のポップでスタイライズドな表現
- **カスタマイズ対応**: アバター・ボード・背景のカラー変更に対応

**シェーダー構成:**

1. **アバター用シェーダー（2D Sprite）:**
   - フラットカラー + リムライト
   - 2-3色のカラーマスク（髪・肌・服）
   - シンプルなアウトライン
   - スプライトバッチング対応

2. **ボードゲーム用シェーダー（2D）:**
   - ヘックスタイルのハイライト表現
   - ホバー・選択状態のアニメーション
   - ピース配置のフィードバックエフェクト
   - バッチング最適化

3. **背景・UI用シェーダー:**
   - グラデーション背景
   - パーティクル（キラキラ、星など）
   - ボタンのホバーエフェクト

#### パフォーマンス目標

- **ドローコール**: 50以下/フレーム
- **バッチング**: Static/Dynamic Batching活用
- **テクスチャ**: アトラス化、最大1024x1024
- **シェーダーバリアント**: 最小限（10種類以内）

---

## セットアップガイド

### インポート済みアセット

**メインゲーム用:**
- **SD Unity-Chan Haon Custom** - Unity-Chan Toon Shader 2.0.6（トゥーンシェーディング参考実装）
- **SoStylized** - URP最適化スタイライズド環境アセット
- **AITranslator** - 多言語対応ツール

**ミニゲーム用:**
- **8Set Free 2D Hex Tiles** - 12枚のヘックスタイル
- **TomatocolCharacterVarietyPackVol1DEMO** - 20種の2Dキャラクタースプライト

### 必須Unityパッケージ

`Packages/manifest.json` に以下が含まれています:

```json
{
  "dependencies": {
    "com.unity.render-pipelines.universal": "17.3.0",
    "com.unity.shadergraph": "17.3.0",
    "com.unity.addressables": "2.4.1",
    "com.unity.burst": "1.8.21",
    "com.unity.mathematics": "1.3.2",
    "com.unity.inputsystem": "1.18.0",
    "com.unity.test-framework": "1.6.0",
    "com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
    "com.neuecc.unirx": "https://github.com/neuecc/UniRx.git?path=Assets/Plugins/UniRx/Scripts"
  }
}
```

### Git設定

**重要な.gitignore設定:**
- ✅ Library/ Temp/ Logs/ 除外済み
- ✅ 参考アセット（Unity-Chan, SoStylized等）除外済み
- ✅ IDE生成ファイル（.csproj, .sln等）除外済み
- ❌ Git LFS **使用しない**（ストレージコスト削減）

**Unity YAMLマージ設定:**
- ✅ .gitattributes で unityyamlmerge 設定済み
- ✅ シェーダーファイル（.shader, .hlsl）テキスト設定済み
- ✅ 改行コード LF 統一設定済み

---

## 開発ワークフロー

### 日常的な開発

```bash
# 最新の変更を取得
git pull

# Unityプロジェクトを開く
# Unity Hub から ShaderOptimizer/ を開く

# 開発作業...

# 変更をコミット
git add .
git commit -m "feat: 新しい機能を追加"
git push
```

### ブランチ戦略

```bash
# 新機能開発
git checkout -b feature/shader-toon-lighting

# バグ修正
git checkout -b fix/outline-thickness-issue

# ドキュメント更新
git checkout -b docs/update-readme
```

### Claude Code AI を使った開発

```bash
# Claude Code CLI を起動
cd ShaderOp
claude

# エージェントを使った開発例
> shader-devエージェントでSG_Character_Base.shadergraphを作成してください
> code-reviewerエージェントでシェーダーコードをレビューしてください
> architect エージェントで新機能の設計をしてください
```

詳細は [CLAUDE.md](./CLAUDE.md) を参照

---

## 🤖 自動化・CI/CD

ShaderOpプロジェクトは包括的な自動化システムを備えています。

### Makefileターゲット

**開発環境セットアップ**:
```bash
make setup-dev          # Git Hooks自動インストール
make setup-hooks        # Git Hooksのみインストール
make hooks-status       # インストール状態確認
```

**アセット検証**:
```bash
make validate           # アセット検証
make validate-strict    # 厳格検証（警告もエラー扱い）
make validate-all       # アセット + シェーダープロファイリング
```

**シェーダー分析**:
```bash
make shader-profile         # シェーダープロファイリング
make shader-profile-json    # JSONレポート出力
```

**バージョン管理**:
```bash
make version                # 現在のバージョン表示
make set-version VERSION=1.0.0   # バージョン設定
make increment PLATFORM=Android  # ビルド番号インクリメント
```

**ビルド管理**:
```bash
make build-info         # ビルド情報表示
make organize           # ビルド成果物整理
make report             # ビルドレポート生成
make clean-builds       # 古いビルド削除
```

### Python自動化スクリプト

| スクリプト | 機能 | ドキュメント |
|-----------|------|--------------|
| `validate_assets.py` | シェーダー/テクスチャ/メタファイル検証 | [automation/README.md](automation/README.md) |
| `shader_profiling.py` | シェーダーパフォーマンス分析 | [automation/README.md](automation/README.md) |
| `build_utils.py` | ビルド番号管理・バージョン設定 | [automation/README.md](automation/README.md) |
| `pre_commit_check.py` | コード品質チェック | [automation/README.md](automation/README.md) |
| `setup_hooks.py` | Git Hooks自動セットアップ | [automation/README.md](automation/README.md) |

**最新の改善レポート**: [AUTOMATION_IMPROVEMENTS.md](AUTOMATION_IMPROVEMENTS.md)
**自動化ロードマップ**: [AUTOMATION_ROADMAP.md](AUTOMATION_ROADMAP.md)

### Git Hooks

**自動実行される検証**:
- **pre-commit**: コード品質チェック、機密情報検出、TODO/FIXME検出
- **pre-push**: アセット検証、シェーダープロファイリング

**インストール**:
```bash
make setup-dev
# または
python automation/setup_hooks.py --install
```

**スキップする場合**:
```bash
git commit --no-verify
git push --no-verify
```

### GitHub Actions

自動実行されるワークフロー:

- **shader-analysis.yml**: シェーダー変更時の自動分析
  - PR作成時に自動コメント投稿
  - 警告/エラー数の統計表示
  - JSONレポート生成

- **asset-validation.yml**: アセット変更時の自動検証
  - アセット整合性チェック
  - テクスチャサイズ警告

### Jenkins CI/CD

Jenkins パイプラインステージ:

1. **Setup** - 環境準備
2. **Checkout** - Git LFS、サブモジュール
3. **Validate Assets** - アセット検証
4. **Shader Profiling** - シェーダー分析
5. **Restore Cache** - Unity Library キャッシュ
6. **Run Tests** - EditMode/PlayMode テスト
7. **Build** - マルチプラットフォームビルド (Android/iOS/WebGL/Windows/Linux)
8. **Package Artifacts** - ビルド成果物の整理

詳細: [Jenkinsfile](Jenkinsfile)

### 統計情報（最新実行結果）

**直近のシェーダープロファイリング** (2026-02-27):
- シェーダーファイル数: 120
- Shader Graph数: 16
- 総Pass数: 172
- 警告: 174件（要対応）
- 最も複雑なShader Graph: S_StylizedWater.shadergraph (1045 nodes)

**直近のアセット検証** (2026-02-27):
- 検証済みシェーダー: 120個
- 検証済みテクスチャ: 332個
- 検証済みアセット: 1556個
- エラー: 17件（Shader Graph JSONパースエラー等）
- 警告: 29件（大きなテクスチャ28個）

---

## 実装状況

### ✅ 完了した実装（Phase 1基盤）

#### 1. Hexグリッドシステム基盤
- `HexCoordinate.cs` - Axial座標系(q, r, s)の実装
- `HexTile.cs` - タイル状態管理
- `HexGrid.cs` - グリッド生成（Rectangle, Hexagon, Triangle, Parallelogram）

#### 2. MVCアーキテクチャ基底クラス
- `HexBoardGameModel.cs` - ゲームロジック基底クラス
- `HexBoardGameView.cs` - ビジュアル表示基底クラス
- `HexBoardGameController.cs` - 入力処理・MV接続基底クラス

#### 3. Tic-Tac-Toe Hex（完全実装）
- 3×3 Hexグリッド、3目並べロジック
- 6方向の勝利判定
- リセット機能

#### 4. 3Dキャラクターカスタマイズシステム
- `CharacterCustomizationData.cs` - データモデル
- `MaterialController.cs` - マテリアル管理
- `CharacterCustomizer.cs` - メインコントローラー
- `CharacterCustomizationUI.cs` - UIコントローラー

#### 5. 部屋デコレーションシステム
- `RoomDecorationData.cs` - 部屋データモデル
- `RoomDecorator.cs` - 部屋管理コントローラー

#### 6. ゲームフローシステム
- `GameManager.cs` - シングルトンゲームマネージャー
- `MainMenuUI.cs` - メインメニューUI

#### 7. Unity Test Framework
- **HexGridTests** (18 tests) - Hexグリッドシステムテスト
- **CharacterCustomizationTests** (22 tests) - カスタマイズシステムテスト
- **合計**: 40 unit tests

### 🚧 未実装（Phase 2-5）

#### Phase 2: アセット統合
- Unity-Chan Toon Shader統合
- Hexタイルスプライト作成
- 3Dキャラクターモデル配置
- 家具プレハブ作成

#### Phase 3: シーン構築
- MainMenu Scene完成
- MainCustomization Scene拡張
- RoomDecoration Scene完成
- TicTacToeHex Scene完成

#### Phase 4: 追加ミニゲーム実装
- Hex Reversi
- Hex Checkers
- その他（計10種）

#### Phase 5: 最適化・仕上げ
- UniTask統合
- Addressables統合
- パフォーマンステスト

詳細は [IMPLEMENTATION_STATUS.md](./IMPLEMENTATION_STATUS.md) を参照

---

## Claude Code 統合

このプロジェクトは**Claude Code AI**による開発支援に完全対応しています。

### 📊 統計

- **エージェント**: 10個（設計、開発、品質、インフラ）
- **スキル**: 10個（Unity、アーキテクチャ、自動化）
- **コマンド**: 6個
- **Hooks**: 4個（自動バックアップ、セキュリティチェック）

### 🤖 利用可能なエージェント

#### 開発・設計
- `architect` - アーキテクチャ設計・機能計画
- `unity-developer` - Unity C#開発・UI Toolkit
- `shader-dev` - シェーダー開発・最適化
- `automation-dev` - Python/JS自動化ツール
- `ui-ux-designer` - UI/UX技術設計

#### 品質・最適化
- `code-reviewer` - コードレビュー・リファクタリング
- `test-engineer` - Unity Test Framework・TDD
- `performance-analyzer` - パフォーマンス最適化
- `security-auditor` - セキュリティ監査

#### インフラ・ドキュメント
- `cicd-helper` - Jenkins/CI/CD構築
- `doc-writer` - 技術ドキュメント作成

### 使用例

```bash
# エージェント一覧を表示
/agents

# エージェントを呼び出す
「shader-devエージェントでサテンシェーダーを作成してください」
「code-reviewerエージェントでコードをレビューしてください」
```

詳細は [CLAUDE.md](./CLAUDE.md) を参照

---

## 技術スタック

### Unity
- **バージョン**: 6000.3.9f1 (Unity 6)
- **レンダーパイプライン**: URP 17.3.0
- **シェーダー**: Shader Graph + HLSL
- **UI**: UI Toolkit (推奨) / uGUI
- **アセット管理**: Addressables

### C#
- **フレームワーク**: .NET Standard 2.1
- **非同期処理**: UniTask
- **リアクティブ**: UniRx
- **テスト**: Unity Test Framework

### 開発ツール
- **Python**: アセット検証、自動化スクリプト
- **CI/CD**: Jenkins, GitHub Actions
- **AI支援**: Claude Code

---

## トラブルシューティング

### Unity が参考アセットを認識しない

1. Asset Store からアセットを再インポート
2. `Assets/` 直下に配置されているか確認
3. Unity Editor を再起動

### パッケージのインストールエラー

```bash
# Package Cache を削除
rm -rf Library/PackageCache

# Unity Editor を再起動
```

### Git merge 競合（Unity YAML）

```bash
# Unity YAML Merge ツールを使用
# .git/config に以下を追加

[merge]
    tool = unityyamlmerge

[mergetool "unityyamlmerge"]
    trustExitCode = false
    cmd = 'C:\\Program Files\\Unity\\Hub\\Editor\\<version>\\Editor\\Data\\Tools\\UnityYAMLMerge.exe' merge -p "$BASE" "$REMOTE" "$LOCAL" "$MERGED"
```

### テストが失敗する

- コンソールのエラーメッセージを確認
- Assembly Definitions が正しいか確認
- Unity を再起動

### マテリアルが変更されない

- CharacterCustomizer の `Character Model` が設定されているか確認
- 各ボディパーツに MeshRenderer がアタッチされているか確認
- MaterialController が自動生成されているか確認（Awake時）

---

## CI/CD パイプライン

このプロジェクトには**完全自動化された CI/CD パイプライン**が実装されています。

### 🚀 機能

- ✅ **自動テスト実行** - PR作成時に40+テストを自動実行
- ✅ **マルチプラットフォームビルド** - Android、iOS、WebGL、Windows、Linux
- ✅ **自動デプロイ** - Google Play、App Store、Itch.io、GitHub Pages
- ✅ **Python自動化ツール** - アセット検証、ビルド管理、プリコミットチェック

### 📋 クイックスタート

```bash
# アセット検証
python automation/validate_assets.py

# バージョン設定
python automation/build_utils.py set-version 1.0.0

# リリース
git tag -a v1.0.0 -m "Release 1.0.0"
git push origin v1.0.0
# → GitHub Actions が自動でビルド&デプロイ
```

### 📚 CI/CD ドキュメント

- [CI/CD セットアップガイド](docs/CICD_SETUP.md) - 詳細なセットアップ手順
- [CI/CD クイックリファレンス](docs/CICD_QUICK_REFERENCE.md) - よく使うコマンド集
- [自動化スクリプトREADME](automation/README.md) - Python スクリプト詳細
- [実装サマリー](CICD_IMPLEMENTATION_SUMMARY.md) - 実装完了レポート

### 🔧 ワークフロー一覧

| ワークフロー | トリガー | 所要時間 | 内容 |
|--------------|----------|----------|------|
| test.yml | PR/Push | 10-15分 | EditMode/PlayMode テスト |
| build.yml | mainマージ/タグ | 20-25分 | 全プラットフォームビルド |
| deploy.yml | リリース | 5-10分 | 各ストアへデプロイ |

---

## 🔒 セキュリティ

ShaderOpプロジェクトには包括的なセキュリティ対策が実装されています。

### セキュリティ監査レポート

最新のセキュリティ監査結果と詳細な修正内容:
- **[SECURITY_AUDIT_REPORT.md](SECURITY_AUDIT_REPORT.md)** - 脆弱性分析レポート
- **[SECURITY_IMPLEMENTATION_GUIDE.md](SECURITY_IMPLEMENTATION_GUIDE.md)** - 修正実装ガイド

### 🛡️ 実装済みセキュリティ対策

#### 1. コマンドインジェクション対策
```groovy
// Jenkinsfile - 安全なシェルコマンド実行
sh '''
    set -e  # Exit on error
    set -u  # Exit on undefined variables
    "${UNITY_PATH}" \
        -batchmode \
        -projectPath "${PROJECT_PATH}"
'''
```

#### 2. パストラバーサル対策
```python
# automation/security_utils.py
class PathValidator:
    def validate_path(self, user_input: str, must_exist: bool = True) -> Path:
        """
        パス検証でディレクトリトラバーサル攻撃を防止
        - Null byte チェック
        - 相対パス解決
        - ベースディレクトリ境界検証
        """
```

#### 3. ReDoS（正規表現DoS）対策
```python
# automation/security_utils.py
class SafeRegex:
    @staticmethod
    def search_with_timeout(pattern: str, text: str, timeout_seconds: float = 1.0):
        """
        タイムアウト付き正規表現実行でReDoS攻撃を防止
        """
```

#### 4. ファイルサイズDoS対策
```python
# automation/security_utils.py
class SafeFileReader:
    def read_text(self, file_path: Path, encoding: str = 'utf-8') -> str:
        """
        ファイルサイズ制限でDoS攻撃を防止
        - デフォルト: 10MB
        - カスタマイズ可能: max_size_mb パラメータ
        """
```

#### 5. 機密情報マスキング
```python
# automation/security_utils.py
class SensitiveDataMasker:
    @classmethod
    def mask(cls, text: str) -> str:
        """
        ログ出力時に機密情報を自動マスキング
        - パスワード
        - APIキー
        - トークン
        - クレデンシャル
        """
```

#### 6. Jenkins Credentials管理
```groovy
// Jenkinsfile - Android署名キー保護
withCredentials([
    file(credentialsId: 'android-keystore', variable: 'KEYSTORE_FILE'),
    string(credentialsId: 'android-keystore-pass', variable: 'KEYSTORE_PASS')
]) {
    sh """
        TEMP_KEYSTORE="/tmp/build_\$\$.keystore"
        cp "\${KEYSTORE_FILE}" "\${TEMP_KEYSTORE}"
        chmod 400 "\${TEMP_KEYSTORE}"
        # ... build commands ...
        rm -f "\${TEMP_KEYSTORE}"  # 必ずクリーンアップ
    """
}
```

### 🔍 自動セキュリティチェック

#### Pre-commit Hook
```bash
# Git Hooks経由で自動実行
python automation/pre_commit_check.py

# 以下をチェック:
# - 機密情報の検出（パスワード、APIキー、トークン）
# - コーディング規約違反
# - TODO/FIXME コメント
# - ファイルサイズチェック
```

#### .gitignore 検証
```bash
# セキュリティパターンの検証
python automation/validate_gitignore.py

# 不足パターンの自動追加
python automation/validate_gitignore.py --add-missing

# 必須パターン:
# - .env, *.keystore, *.jks
# - *_secret*, unity-license.ulf
# - credentials.json, *.pem
```

#### アセット検証
```bash
# セキュリティ強化された検証
python automation/validate_assets.py --project ShaderOptimizer

# 自動的に適用されるセキュリティ:
# - パストラバーサル検証
# - ファイルサイズ制限
# - タイムアウト付き正規表現
```

### 📋 .gitignore セキュリティパターン

**機密情報を含むファイル**（必須）:
```gitignore
.env
*.keystore
*.jks
*_secret*
unity-license.ulf
credentials.json
service-account*.json
*.pem
*.p12
```

**IDE設定ファイル**（推奨）:
```gitignore
.idea/
.vscode/settings.json
```

**ビルド成果物とレポート**:
```gitignore
validation_report.txt
shader_profile_report.json
build_report.json
```

### 🚨 セキュリティベストプラクティス

#### 開発者向けチェックリスト

✅ **機密情報管理**:
- [ ] `.env` ファイルをコミットしない
- [ ] パスワード・APIキーをハードコードしない
- [ ] Unity License ファイルを除外する
- [ ] Android/iOS署名キーを適切に保護する

✅ **コード品質**:
- [ ] Pre-commit フックを有効化する
- [ ] `--no-verify` を乱用しない
- [ ] FIXME コメントを残したままコミットしない

✅ **CI/CD**:
- [ ] Jenkins Credentials を使用する
- [ ] 一時ファイルを確実にクリーンアップする
- [ ] ビルドログに機密情報を出力しない

### 🔧 セキュリティツール使用方法

#### Git Hooks のインストール
```bash
# 自動インストール
make setup-dev

# または手動インストール
python automation/setup_hooks.py --install

# ステータス確認
python automation/setup_hooks.py --status

# アンインストール
python automation/setup_hooks.py --uninstall
```

#### .gitignore 検証の実行
```bash
# 検証のみ
python automation/validate_gitignore.py

# 不足パターンをプレビュー
python automation/validate_gitignore.py --add-missing --dry-run

# 不足パターンを追加
python automation/validate_gitignore.py --add-missing

# 警告でも終了コード1を返す
python automation/validate_gitignore.py --fail-on-warning
```

#### プリコミットチェックの手動実行
```bash
# ステージングされたファイルをチェック
python automation/pre_commit_check.py

# 特定のファイルをチェック
python automation/pre_commit_check.py --files path/to/file1.cs path/to/file2.py

# 警告でも終了コード1を返す
python automation/pre_commit_check.py --fail-on-warning
```

### 📊 セキュリティ統計

**監査実施日**: 2026-02-28

**検出された脆弱性**:
- 🔴 Critical: 2件（すべて修正済み）
- 🟠 High: 4件（すべて修正済み）
- 🟡 Medium: 6件（すべて修正済み）
- 🟢 Low: 3件（対応中）

**適用された対策**:
- ✅ コマンドインジェクション対策
- ✅ パストラバーサル対策
- ✅ ReDoS対策
- ✅ ファイルサイズDoS対策
- ✅ 機密情報保護
- ✅ 入力検証強化

**カバレッジ**:
- Jenkinsfile: 100% 修正完了
- BuildScript.cs: 100% 修正完了
- Python自動化スクリプト: 100% 修正完了

### 🆘 セキュリティインシデント対応

脆弱性を発見した場合:

1. **報告**: セキュリティ監査担当者に連絡
2. **評価**: SECURITY_AUDIT_REPORT.md のフォーマットで文書化
3. **修正**: SECURITY_IMPLEMENTATION_GUIDE.md に従って修正
4. **検証**: 修正後の動作確認
5. **ドキュメント更新**: README および監査レポート更新

### 📚 参考ドキュメント

- [OWASP Top 10](https://owasp.org/www-project-top-ten/) - Webアプリケーションセキュリティリスク
- [CWE - Common Weakness Enumeration](https://cwe.mitre.org/) - ソフトウェア脆弱性分類
- [NIST Cybersecurity Framework](https://www.nist.gov/cyberframework) - サイバーセキュリティフレームワーク

---

## 参考リソース

### Unity公式ドキュメント
- **Unity Manual**: https://docs.unity3d.com/Manual/
- **URP Documentation**: https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest
- **Shader Graph**: https://docs.unity3d.com/Packages/com.unity.shadergraph@latest

### 外部リソース
- **UniTask**: https://github.com/Cysharp/UniTask
- **UniRx**: https://github.com/neuecc/UniRx
- **Hex Grid Guide**: https://www.redblobgames.com/grids/hexagons/
- **Git Best Practices**: https://git-scm.com/book/en/v2
- **GitHub Actions**: https://docs.github.com/en/actions
- **Game CI**: https://game.ci/

### プロジェクト内ドキュメント

#### 基本ドキュメント
- [CLAUDE.md](./CLAUDE.md) - Claude Code使用方法
- [ROADMAP.md](./ROADMAP.md) - 開発ロードマップ
- [docs/GETTING_STARTED.md](docs/GETTING_STARTED.md) - クイックスタートガイド
- [docs/IMPLEMENTATION_STATUS.md](docs/IMPLEMENTATION_STATUS.md) - 実装進捗詳細
- [docs/BEST_PRACTICES.md](docs/BEST_PRACTICES.md) - コーディング規約

#### 技術ドキュメント
- [docs/PERFORMANCE.md](docs/PERFORMANCE.md) - パフォーマンス最適化ガイド
- [docs/shaders/CHARACTER_BASE.md](docs/shaders/CHARACTER_BASE.md) - キャラクターシェーダー実装

#### CI/CD & セキュリティ
- [docs/cicd/README.md](docs/cicd/README.md) - CI/CD概要
- [docs/cicd/SETUP.md](docs/cicd/SETUP.md) - CI/CDセットアップ
- [docs/cicd/REFERENCE.md](docs/cicd/REFERENCE.md) - コマンドリファレンス
- [docs/security/AUDIT_REPORT.md](docs/security/AUDIT_REPORT.md) - セキュリティ監査レポート
- [docs/security/IMPLEMENTATION_GUIDE.md](docs/security/IMPLEMENTATION_GUIDE.md) - セキュリティ実装ガイド

#### Claude Code設定
- [.claude/MINIGAME_DESIGNS.md](./.claude/MINIGAME_DESIGNS.md) - ミニゲーム仕様書
- [.claude/WORKFLOW_GUIDE.md](./.claude/WORKFLOW_GUIDE.md) - 開発ワークフロー
- [.claude/OVERVIEW.md](./.claude/OVERVIEW.md) - Claude Code構成概要

---

## ライセンス

MIT License

---

## 最終更新

**日付**: 2026-03-01
**更新者**: Doc Writer Specialist (Claude Code)
**バージョン**: 1.0.0

---

**Happy Coding!** 🎉
