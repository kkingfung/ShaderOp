# ShaderOp

Unified Stylized Shader Library for Mobile Character Customization Games

## 概要

キャラクターカスタマイズ系モバイルゲーム向けの**統合スタイライズドシェーダーライブラリ**を構築するプロジェクトです。

### プロジェクトビジョン

**トゥーンシェーディングキャラクター × スタイライズド環境** の美しい融合

- **アート方向性**: Unity-Chan Toon Shader風のアニメキャラクター + SoStylized風のローポリ環境
- **ターゲットプラットフォーム**: 中級モバイル端末（iPhone 11 / Galaxy S10世代）60fps維持
- **開発アプローチ**: Shader Graph + HLSL Custom Functions（ビルドファースト・反復開発）
- **カスタマイズシステム**: 包括的対応（カラー変更、マテリアルレイヤリング、テクスチャスワップ、プロシージャル生成）

### インポート済みアセット

- **SD Unity-Chan Haon Custom** - Unity-Chan Toon Shader 2.0.6（リファレンス実装）
- **SoStylized** - URP最適化スタイライズド環境アセット
- **TomatocolCharacterVarietyPackVol1DEMO** - キャラクターバリエーションデモ

### 第1マイルストーン: シェーダーテンプレートコレクション（3-4日）

#### 成果物
1. **SG_Character_Base.shadergraph** - 肌/ボディ用基礎シェーダー
2. **SG_Character_Hair.shadergraph** - 髪専用レンダリング
3. **SG_Character_Cloth.shadergraph** - 衣装/布表現
4. **SG_Environment_Stylized.shadergraph** - スタイライズド環境

#### 各テンプレートの特徴
- クリーンなノード構造（コメント付きグループ化）
- プレースホルダーCustom Functionノード（適切に配線済み）
- モバイル最適化設定（URP、高負荷機能なし）
- カスタマイズ用マテリアルプロパティ設定
- Shader Graph内ドキュメンテーションコメント

## アーキテクチャ戦略

### モジュラーCustom Functionライブラリ

```
ShaderOptimizer/Assets/
├── Shaders/
│   ├── ShaderGraphs/              # Shader Graphテンプレート
│   │   ├── Character/
│   │   │   ├── SG_Character_Base.shadergraph
│   │   │   ├── SG_Character_Hair.shadergraph
│   │   │   └── SG_Character_Cloth.shadergraph
│   │   └── Environment/
│   │       └── SG_Environment_Stylized.shadergraph
│   └── HLSL/                      # 再利用可能HLSL関数
│       ├── ToonLighting.hlsl      # セルシェーディング関数
│       ├── OutlineUtils.hlsl      # アウトライン計算
│       ├── ColorCustomization.hlsl # HSV、カラーマスク
│       ├── AnisotropicHighlight.hlsl # 髪/サテン効果
│       └── MatcapUtils.hlsl       # フェイクリフレクション
```

### 段階的機能追加計画

- **Week 1**: テンプレート + 基本トゥーンライティング
- **Week 2**: アウトライン + リムライティング
- **Week 3**: カラーカスタマイズシステム
- **Week 4**: 高度機能（マットキャップ、異方性等）

## 技術スタック

- **Unity**: Shader Graph, HLSL, URP, UI Toolkit, Addressables
- **C#**: UniTask, UniRx, async/await
- **Python**: アセット検証、自動化スクリプト
- **CI/CD**: Jenkins, GitHub Actions

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

#### 品質・最適化
- `code-reviewer` - コードレビュー・リファクタリング
- `test-engineer` - Unity Test Framework・TDD
- `performance-analyzer` - パフォーマンス最適化
- `security-auditor` - セキュリティ監査

#### インフラ・ドキュメント
- `cicd-helper` - Jenkins/CI/CD構築
- `doc-writer` - 技術ドキュメント作成

### 📚 詳細ドキュメント

- [CLAUDE.md](./CLAUDE.md) - Claude Code 使用方法
- [.claude/OVERVIEW.md](./.claude/OVERVIEW.md) - エージェント・スキル詳細
- [.claude/WORKFLOW_GUIDE.md](./.claude/WORKFLOW_GUIDE.md) - 実践ワークフロー
- [.claude/AGENTS_QUICK_REFERENCE.md](./.claude/AGENTS_QUICK_REFERENCE.md) - エージェント選択ガイド

## クイックスタート

### 開発の開始

```bash
# 新しいShader Graphテンプレートを作成
/new-shader "SG_Character_Base" "ShaderGraph"

# Unity-Chan Toon Shaderのコード解析
shader-devエージェントに「Unity-Chan Toon Shaderの基本構造を分析してください」

# テンプレートをレビュー
/review-shader "Assets/Shaders/ShaderGraphs/Character/SG_Character_Base.shadergraph"
```

### 学習リソース

- **Unity-Chan Toon Shader 2.0.6**: `ShaderOptimizer/Assets/SD Unity-Chan Haon Custom/Shader/`
- **SoStylized参考実装**: `ShaderOptimizer/Assets/SoStylized/`
- **ワークフローガイド**: `.claude/WORKFLOW_GUIDE.md`

## 開発ワークフロー

### マイルストーン1: シェーダーテンプレート作成（3-4日）

**Day 1-2: キャラクターシェーダーテンプレート**
1. `shader-dev`: Unity-Chan Toon Shaderの構造分析
2. `shader-dev`: SG_Character_Base.shadergraph 作成（基本セルシェーディング）
3. `shader-dev`: SG_Character_Hair.shadergraph 作成（髪専用機能）
4. `shader-dev`: SG_Character_Cloth.shadergraph 作成（布表現）

**Day 3: 環境シェーダーテンプレート**
1. `shader-dev`: SoStylizedシェーダー解析
2. `shader-dev`: SG_Environment_Stylized.shadergraph 作成

**Day 4: HLSL Custom Function基盤**
1. `shader-dev`: ToonLighting.hlsl（基本関数スタブ作成）
2. `shader-dev`: OutlineUtils.hlsl、ColorCustomization.hlsl等
3. `doc-writer`: テンプレート使用ガイド作成

## プロジェクト目標

### 技術目標
- **60fps維持**: 中級モバイル端末（iPhone 11/Galaxy S10）で安定動作
- **モジュラー設計**: 再利用可能なHLSL Custom Function ライブラリ
- **包括的カスタマイズ**: カラー変更、レイヤリング、テクスチャスワップ、プロシージャル対応
- **学習可能**: Shader Graph + コメント付きで理解しやすい構造

### マイルストーン成果物

**Milestone 1（Week 1）**:
- 4つのShader Graphテンプレート
- 5つのHLSL Custom Functionスタブ
- テンプレート使用ガイド

**Milestone 2（Week 2-4）**:
- 基本トゥーンライティング実装
- アウトライン・リムライト機能
- カラーカスタマイズシステム
- 高度機能（マットキャップ、異方性）

## プロジェクト構成

```
ShaderOp/
├── .claude/                 # Claude Code設定
│   ├── agents/             # 10個のエージェント
│   ├── skills/             # 10個のスキルパターン
│   ├── commands/           # 6個のコマンド
│   ├── rules/              # 3個のルール
│   ├── hooks/              # 4個のHooksスクリプト
│   ├── OVERVIEW.md         # 構成概要
│   ├── WORKFLOW_GUIDE.md   # 実践ワークフロー
│   └── AGENTS_QUICK_REFERENCE.md
├── .claude-plugin/         # プラグインマニフェスト
├── CLAUDE.md               # Claude Code使用方法
└── README.md               # このファイル
```

## ライセンス

MIT

## 最終更新

2026-02-21
