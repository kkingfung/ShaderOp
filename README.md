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
- [トラブルシューティング](#トラブルシューティング)
- [参考リソース](#参考リソース)

---

## プロジェクト概要

ShaderOpは、**ポケコロ風キャラクターカスタマイズゲーム + クラシックボードゲーム集**を実現するUnityプロジェクトです。

### 🎮 ゲーム構成

#### メインゲーム: 3Dキャラクターカスタマイズ & 部屋デコレーション
- **参考作品**: ココネ「ポケコロ」「リヴリーアイランド」
- **機能**: アバター作成、着せ替え、部屋デコレーション、ソーシャル要素
- **ビジュアル**: トゥーンシェーディングキャラクター × スタイライズド環境
- **カメラ**: 近距離視点、高品質シェーダー

#### サブゲーム: ヘックスボードゲーム集
- **ゲームタイトル**: リバーシ、チェッカー、Tic-Tac-Toe等（計10種予定）
- **機能**: ターン制ヘックスボードゲーム、1-4プレイヤー対応
- **ビジュアル**: 2Dトップダウンビュー、シンプルスタイライズド
- **プレイ時間**: 1-3分/ゲーム

### 🎯 技術目標

- **ターゲットデバイス**: 中級モバイル端末（iPhone 11 / Galaxy S10世代）
- **パフォーマンス**: 60fps維持
- **シェーダー戦略**: 統一ライブラリでメインゲーム＆ミニゲーム両対応
- **カスタマイズシステム**: カラー変更、マテリアルレイヤリング、テクスチャスワップ対応

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

キャラクターカスタマイズゲーム + ミニゲーム集

### 設計パターン: MVC (Model-View-Controller)

#### 理由
- ✅ **明確な責任分離** - ゲームロジック、UI、入力処理が独立
- ✅ **テスト可能** - ゲームルールをUIから分離してテスト
- ✅ **拡張性** - 10種のミニゲームに対応しやすい
- ✅ **Unity親和性** - MonoBehaviourと相性が良い

### MVC実装例

#### Model（ゲームロジック・データ）

```csharp
namespace ShaderOp.Customization
{
    /// <summary>
    /// キャラクターカスタマイズデータ
    /// </summary>
    public class CharacterCustomizationData
    {
        public Gender Gender { get; set; }
        public int SkinToneId { get; set; }
        public int HairstyleId { get; set; }
        public Color HairColor { get; set; }
        public Color EyeColor { get; set; }
        public float Height { get; set; }

        // イベント
        public event Action OnDataChanged;
    }
}
```

#### View（UI表示）

```csharp
namespace ShaderOp.Customization
{
    /// <summary>
    /// キャラクターカスタマイズUI
    /// </summary>
    public class CharacterCustomizationUI : MonoBehaviour
    {
        [SerializeField] private Slider _hairColorRSlider;
        [SerializeField] private Slider _skinToneSlider;

        /// <summary>
        /// UIイベント（Controllerへ通知）
        /// </summary>
        public event Action<Color> OnHairColorChanged;

        /// <summary>
        /// Modelデータを反映（Controllerから呼ばれる）
        /// </summary>
        public void UpdatePreview(CharacterCustomizationData data)
        {
            // UIを更新
        }
    }
}
```

#### Controller（ViewとModelの仲介）

```csharp
namespace ShaderOp.Customization
{
    /// <summary>
    /// キャラクターカスタマイズController
    /// </summary>
    public class CharacterCustomizer : MonoBehaviour
    {
        [SerializeField] private GameObject _characterModel;
        private CharacterCustomizationData _data;

        private void Start()
        {
            // Modelの変更をViewに反映
            _data.OnDataChanged += UpdateMaterials;
        }

        private void UpdateMaterials()
        {
            // マテリアルプロパティを更新
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

#### 統一ライブラリアプローチ

**共通機能（HLSL）:**
- 基本トゥーンライティング → 3D/2D両対応
- アウトライン計算 → 3D/2D両対応
- カラーカスタマイズ → 3D/2D両対応

**専用機能（Shader Graph）:**
- 3D高品質機能: 異方性ハイライト、マットキャップ
- 2D最適化機能: スプライトバッチング、タイルハイライト

#### パフォーマンス階層

**Tier 1 - メインゲーム（高品質）:**
- 複雑なライティング（マットキャップ、異方性）
- 4ゾーンカラーマスク
- テクスチャレイヤリング

**Tier 2 - ミニゲーム（軽量）:**
- 基本トゥーンライティング
- 2ゾーンカラーマスク
- シンプルアウトライン

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

### プロジェクト内ドキュメント
- [CLAUDE.md](./CLAUDE.md) - Claude Code使用方法
- [IMPLEMENTATION_STATUS.md](./IMPLEMENTATION_STATUS.md) - 実装進捗詳細
- [.claude/MINIGAME_DESIGNS.md](./.claude/MINIGAME_DESIGNS.md) - ミニゲーム仕様書
- [.claude/WORKFLOW_GUIDE.md](./.claude/WORKFLOW_GUIDE.md) - 開発ワークフロー

---

## ライセンス

MIT License

---

## 最終更新

**日付**: 2026-02-23
**更新者**: Claude Code
**バージョン**: 1.0.0

---

**Happy Coding!** 🎉
