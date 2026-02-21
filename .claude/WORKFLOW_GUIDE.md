# ShaderOp 実践ワークフローガイド

## プロジェクトビジョン

**Unified Stylized Shader Library for Casual Mobile Game Suite**

カジュアルモバイルゲームスイート向けの**統合スタイライズドシェーダーライブラリ**を構築する。

### ゲーム構成

#### メインゲーム: ポケコロ風キャラクターカスタマイズ
- **参考作品**: ココネ「ポケコロ」「リヴリーアイランド」
- **コアループ**: アバター作成 → 着せ替え → 部屋デコレーション → ソーシャル交流
- **使用アセット**: SD Unity-Chan（キャラクター）、SoStylized（環境）、AITranslator（多言語）
- **シェーダー要件**: 高品質トゥーンシェーディング、詳細カスタマイズ、近距離カメラ対応

#### サブゲーム: クラシックボードゲーム集（ヘックスタイル版）
- **参考作品**: リバーシ、チェッカー、マンカラ等の古典ボードゲーム
- **コアループ**: ミニゲーム選択 → ターン制プレイ（1-3分） → リワード獲得 → メインゲームへ還元
- **使用アセット**: 8Set Hex Tiles（12タイル）、Tomatocol Characters（20スプライト、ゲーム駒）
- **シェーダー要件**: 軽量トゥーンシェーディング、複数キャラクター同時表示、俯瞰カメラ対応、ヘックスタイルハイライト

### 技術コンセプト

- **統一シェーダーライブラリ**: 両ゲームで共通基盤、品質レベルで切り替え
- **ターゲット**: 中級モバイル端末（iPhone 11 / Galaxy S10世代）で60fps維持
- **開発手法**: Shader Graph + HLSL Custom Functions（ビルドファースト・反復開発）
- **カスタマイズ**: カラー変更、マテリアルレイヤリング、テクスチャスワップ、プロシージャル生成

### インポート済みアセット分類

**メインゲーム用:**
1. **SD Unity-Chan Haon Custom** - Unity-Chan Toon Shader 2.0.6（高品質トゥーン参考実装）
2. **SoStylized** - URP最適化スタイライズド環境（部屋デコ、背景）
3. **AITranslator** - 多言語対応ツール

**ミニゲーム用:**
1. **8Set Free 2D Hex Tiles** - 12枚のヘックスタイル（4地形、2特殊、6装飾）
2. **TomatocolCharacterVarietyPackVol1DEMO** - 20種の2Dキャラクタースプライト（ゲーム駒として使用）

---

## マイルストーン1: シェーダーテンプレートコレクション構築（Week 1）

このマイルストーンでは、**メインゲームとミニゲーム両方で使える**統一シェーダーライブラリの基盤を構築します。

### 目標
「モバイル最適化されたShader Graphテンプレート群を作成し、品質レベル切り替えで両ゲームに対応できる土台を整える」

### シェーダー設計の重要ポイント

#### 両ゲーム対応のための設計原則

**1. 品質レベル階層化**
- **High Quality（メインゲーム）**: 全機能有効、近距離カメラ向け、1-2キャラクター
- **Low Quality（ミニゲーム）**: 基本機能のみ、俯瞰カメラ向け、複数キャラクター同時表示

**2. カメラアングル考慮**
- **近距離カメラ（メインゲーム）**: 太いアウトライン、詳細テクスチャ、リッチエフェクト
- **俯瞰カメラ（ミニゲーム）**: 細いアウトライン、明るいカラー、視認性優先

**3. パフォーマンス階層**
- **メインゲーム**: マットキャップ、異方性ハイライト、4ゾーンカラーマスク
- **ミニゲーム**: 基本トゥーンライティング、2ゾーンカラーマスク、シンプルアウトライン

---

### Day 1: Unity-Chan Toon Shader解析 + キャラクターベースシェーダー

#### ステップ1: Unity-Chan Toon Shaderの構造理解（shader-dev）

```
プロンプト:
「Unity-Chan Toon Shader 2.0.6の基本構造を分析してください。
パス: ShaderOptimizer/Assets/SD Unity-Chan Haon Custom/Shader/
重点:
- セルシェーディングの実装方法
- アウトラインレンダリング技法
- モバイル最適化のテクニック
- プロパティ構成
」

出力期待:
- 主要.shaderファイルの構造解説
- セルシェーディング計算ロジック（UCTS_DoubleShadeWithFeather.cginc等）
- モバイル向け最適化ポイント
- Shader Graphへの移植可能な部分の特定
```

#### ステップ2: SG_Character_Base.shadergraph 作成（shader-dev）

```
プロンプト:
「キャラクターベースシェーダーテンプレートを作成してください。
要件:
- Shader Graphで実装（URP）
- 基本セルシェーディング（2トーン: Base + Shadow）
- プレースホルダーCustom Functionノード配置
- モバイル最適化設定
- **品質レベル切り替え対応**（QualityLevel enum: High/Low）
  - High: メインゲーム用（マットキャップ、リムライト有効）
  - Low: ミニゲーム用（基本トゥーンのみ）
- **カメラ距離対応**（OutlineWidth: Near=0.01, Far=0.003）
- プロパティ: BaseColor, ShadowColor, ShadowThreshold, QualityLevel, OutlineWidth
」

使用シナリオ例:
- メインゲーム: SD Unity-Chanモデルに適用（近距離カメラ、High Quality）
- ミニゲーム: Tomatocolキャラクターに適用（俯瞰カメラ、Low Quality）

コマンド代替:
/new-shader "SG_Character_Base" "ShaderGraph"

出力:
- Assets/Shaders/ShaderGraphs/Character/SG_Character_Base.shadergraph
- サンプルマテリアル（2種類: MainGame_Material, MiniGame_Material）
- 使用方法ドキュメント
```

---

### Day 2: 髪・衣装シェーダーテンプレート

#### ステップ3: SG_Character_Hair.shadergraph 作成（shader-dev）

```
プロンプト:
「髪専用シェーダーテンプレートを作成してください。
要件:
- SG_Character_Baseを基礎とする
- 異方性ハイライト用プレースホルダー（AnisotropicHighlight.hlsl）
- Angel Ring（髪の天使の輪）用プレースホルダー
- プロパティ: HairColor, HighlightColor, AnisotropyStrength
」

出力:
- Assets/Shaders/ShaderGraphs/Character/SG_Character_Hair.shadergraph
- プレースホルダーCustom Function配置図
```

#### ステップ4: SG_Character_Cloth.shadergraph 作成（shader-dev）

```
プロンプト:
「衣装専用シェーダーテンプレートを作成してください。
要件:
- カラーマスク対応（Base, Pattern, Trim, Accent の4ゾーン）
- ColorCustomization.hlsl プレースホルダー
- サテン/レース用オプション設定
- プロパティ: ClothColor, PatternColor, SatinStrength
」

出力:
- Assets/Shaders/ShaderGraphs/Character/SG_Character_Cloth.shadergraph
- カラーマスク使用例
```

---

### Day 3: 環境シェーダーテンプレート

#### ステップ5: SoStylized解析（shader-dev）

```
プロンプト:
「SoStylizedの環境シェーダーを分析してください。
パス: ShaderOptimizer/Assets/SoStylized/
重点:
- URP最適化手法
- 頂点カラー活用
- シンプルなライティングモデル
」

出力:
- SoStylizedのシェーダー構造レポート
- モバイル最適化テクニック抽出
```

#### ステップ6: SG_Environment_Stylized.shadergraph 作成（shader-dev）

```
プロンプト:
「スタイライズド環境シェーダーテンプレートを作成してください。
要件:
- 頂点カラー対応
- シンプルライティング（Lambertベース）
- 風揺れアニメーション用プレースホルダー
- プロパティ: TintColor, ShadowStrength, WindStrength
」

出力:
- Assets/Shaders/ShaderGraphs/Environment/SG_Environment_Stylized.shadergraph
```

---

### Day 4: HLSL Custom Function基盤構築

#### ステップ7: Custom Function HLSLファイル作成（shader-dev）

```
プロンプト:
「再利用可能なHLSL Custom Functionファイルを作成してください。
各ファイルには基本的な関数スタブとコメントを含める。

ファイル構成:
1. ToonLighting.hlsl - セルシェーディング計算
2. OutlineUtils.hlsl - アウトライン幅計算
3. ColorCustomization.hlsl - HSV変換、カラーマスク処理
4. AnisotropicHighlight.hlsl - 異方性反射計算
5. MatcapUtils.hlsl - Matcapサンプリング
」

出力:
- Assets/Shaders/HLSL/*.hlsl（5ファイル）
- 各関数のインターフェース定義
- 使用例コメント
```

#### ステップ8: ドキュメント作成（doc-writer）

```
プロンプト:
「シェーダーテンプレート使用ガイドを作成してください。
内容:
- 各テンプレートの目的と使い分け
- Custom Functionの追加方法
- モバイル最適化チェックリスト
- 次週以降の機能追加ロードマップ
」

出力:
- ShaderOptimizer/Assets/Shaders/README.md
- ShaderTemplateGuide.md
```

---

**Week 1 完了時の成果物**:
- ✅ 4つのShader Graphテンプレート
- ✅ 5つのHLSL Custom Functionスタブ
- ✅ Unity-Chan/SoStylized解析レポート
- ✅ テンプレート使用ガイド

---

## Week 2-4: 段階的機能実装（参考）

Week 1でテンプレート基盤ができた後は、段階的に機能を追加していきます。

### Week 2: 基本トゥーンライティング実装
- ToonLighting.hlslの実装（2トーンセルシェーディング）
- OutlineUtils.hlslの実装（アウトライン計算）
- Unity-Chanモデルでのテスト

### Week 3: カラーカスタマイズシステム
- ColorCustomization.hlsl実装（HSV変換、カラーマスク）
- Unity C#側のマテリアル管理システム
- UI Toolkitカラーピッカー

### Week 4: 高度機能
- AnisotropicHighlight.hlsl実装（髪/サテン）
- MatcapUtils.hlsl実装（フェイクリフレクション）
- リムライト追加
- 最終最適化

---

## その他のワークフロー（将来的な展開）

### ワークフロー2: アセット検証ツール開発

### 目標
「デザイナーが作成したテクスチャを自動検証するツールを作成」

### ステップ1: 要件定義（architect）
```
プロンプト:
「テクスチャアセット検証ツールを設計してください。
検証項目:
- サイズ: 2048x2048以下
- 形式: PNG/JPGのみ
- 命名規則: tex_キャラクター名_部位_番号.png
- 2のべき乗チェック
」

出力:
- 検証ルール定義
- エラーレポート形式
- クラス設計
```

### ステップ2: Python実装（automation-dev）
```
プロンプト:
「テクスチャ検証ツールをPythonで実装してください」

出力:
- texture_validator.py
- validation_config.json
- エラーレポート生成機能
- Pillowを使用した画像解析
```

### ステップ3: Unity Editor拡張（unity-developer）
```
プロンプト:
「Unity Editor上でPython検証ツールを呼び出すメニューを作成してください」

出力:
- EditorメニューUI
- Python実行スクリプト
- 検証結果の表示UI
```

### ステップ4: 自動化（cicd-helper）
```
プロンプト:
「Git push時に自動でアセット検証を実行するCI/CDパイプラインを構築してください」

出力:
- Jenkinsfile（アセット検証ステージ）
- GitHub Actions workflow
- Slack通知設定
```

### ステップ5: テスト（test-engineer）
```
プロンプト:
「テクスチャ検証ツールのテストケースを作成してください」

出力:
- pytest テストスイート
- 正常系/異常系テストケース
- モックデータ
```

**所要時間**: 設計30分 → Python実装1時間 → Unity連携1時間 → CI/CD設定30分 → テスト30分 = **約3.5時間**

---

## ワークフロー3: 品質チェック（リリース前）

### 目標
「リリース前に全体的な品質を確認する」

### ステップ1: コードレビュー（code-reviewer）
```
プロンプト:
「Assets/Scripts/ 配下のすべてのC#コードをレビューしてください」

出力:
- コード品質スコア
- 改善提案リスト
- リファクタリングが必要な箇所
- 優先度付きアクションアイテム
```

### ステップ2: パフォーマンステスト（performance-analyzer）
```
プロンプト:
「メインシーンのパフォーマンスをプロファイリングしてください。
ターゲット:
- 60fps維持
- GC Alloc 100KB/frame以下
- メモリ使用量 500MB以下
」

出力:
- Profilerレポート
- ボトルネック特定
- 最適化提案（具体的なコード例付き）
```

### ステップ3: セキュリティ監査（security-auditor）
```
プロンプト:
「プロジェクト全体のセキュリティ監査を実施してください」

出力:
- 脆弱性リスト（重要度順）
- 機密情報のハードコード検出
- .gitignore設定確認
- OWASP Top 10チェック結果
```

### ステップ4: テスト実行（test-engineer）
```
プロンプト:
「すべてのテストを実行し、カバレッジレポートを生成してください」

出力:
- テスト成功/失敗レポート
- カバレッジ率（目標80%以上）
- 未テスト箇所のリスト
```

### ステップ5: 最終ビルド（cicd-helper）
```
コマンド:
/build-jenkins "Android"
/build-jenkins "iOS"

出力:
- リリースビルド（APK/IPA）
- ビルドレポート
- 自動配信完了通知
```

**所要時間**: レビュー1時間 → パフォーマンス1時間 → セキュリティ30分 → テスト30分 → ビルド30分 = **約3.5時間**

---

## ワークフロー4: 新機能実装（フルサイクル）

### 例: 「ダイナミックカラー変更システム」実装

### Phase 1: 計画・設計（1日目）

**architect**:
- 機能要件分析
- システム設計（MVVMパターン）
- タスク分解
- 技術選定（UniTask、ReactiveProperty）

### Phase 2: 実装（2-3日目）

**shader-dev**:
- カラーマスクシェーダー作成
- HSV色空間変換実装

**unity-developer**:
- ColorCustomizationService 実装
- UI Toolkit カラーピッカー作成
- Addressables カラープリセット管理

**automation-dev**:
- カラープリセット検証ツール（Python）
- 自動テクスチャ生成スクリプト

### Phase 3: 品質保証（4日目）

**code-reviewer**:
- 全コードレビュー
- リファクタリング提案

**test-engineer**:
- 単体テスト作成（Unity Test Framework）
- 統合テスト作成
- カバレッジ80%以上確保

**performance-analyzer**:
- カラー変更時のGC Alloc削減
- UniTaskによる非同期化
- メモリプロファイリング

**security-auditor**:
- カラーデータの安全性確認
- 入力検証実装

### Phase 4: デプロイ（5日目）

**cicd-helper**:
- Jenkinsパイプライン更新
- ステージング環境へ自動デプロイ
- 本番環境リリース

**doc-writer**:
- 機能仕様書作成
- API ドキュメント生成
- リリースノート作成

**所要時間**: 5日間（設計1日 + 実装2日 + QA1日 + デプロイ1日）

---

## エージェント組み合わせパターン

### パターンA: シェーダー開発特化
```
architect → shader-dev → performance-analyzer → cicd-helper
```
**用途**: 新しいシェーダーの開発と最適化

### パターンB: ツール開発特化
```
architect → automation-dev → test-engineer → cicd-helper
```
**用途**: Python/JS自動化ツールの開発

### パターンC: 品質改善特化
```
code-reviewer → test-engineer → performance-analyzer → security-auditor
```
**用途**: 既存コードの品質向上

### パターンD: フル開発サイクル
```
architect → [unity-dev, shader-dev, automation-dev] → code-reviewer →
test-engineer → performance-analyzer → security-auditor → cicd-helper → doc-writer
```
**用途**: 大規模機能の完全実装

---

## 実践例: モバイルゲーム企業での活用

### シナリオ: キャラクターカスタマイズシステム改善

#### Week 1: 質感表現強化
- **shader-dev**: サテン、レース、刺繍の専用シェーダー開発
- **performance-analyzer**: モバイル端末でのFPS最適化
- **結果**: アイテムの高級感向上 → ガチャ売上15%増加

#### Week 2: アセット量産効率化
- **automation-dev**: テクスチャ自動検証ツール作成
- **cicd-helper**: デザイナー向けワンクリックビルド構築
- **結果**: アセット入稿時間を80%削減

#### Week 3: パフォーマンス最適化
- **performance-analyzer**: UniTaskベース非同期ロード実装
- **unity-developer**: Addressables メモリ管理改善
- **結果**: ロード時間50%短縮、離脱率20%減少

#### Week 4: 品質保証体制構築
- **test-engineer**: 自動テストスイート構築
- **security-auditor**: セキュリティ監査実施
- **code-reviewer**: コードレビュー規約策定
- **結果**: バグ発生率60%減少

---

## 成果指標

### 開発効率
- シェーダー開発時間: **5日 → 4時間**（95%削減）
- アセット検証時間: **手動2時間 → 自動5分**（96%削減）
- ビルド時間: **手動30分 → 自動10分**（67%削減）

### コード品質
- テストカバレッジ: **0% → 80%以上**
- コードレビュー実施率: **100%**
- セキュリティ脆弱性: **0件維持**

### パフォーマンス
- FPS: **40fps → 60fps**（50%向上）
- GC Alloc: **500KB/frame → 50KB/frame**（90%削減）
- ロード時間: **10秒 → 5秒**（50%短縮）

### ビジネスインパクト
- ガチャ売上: **15%増加**（質感向上）
- 開発コスト: **30%削減**（自動化）
- ユーザー離脱率: **20%減少**（最適化）

---

## まとめ

### ShaderOpプロジェクトで実現すること

#### フェーズ1（現在）: 統合スタイライズドシェーダーライブラリ
✅ **トゥーン × スタイライズド融合** - Unity-Chan風キャラクター + SoStylized風環境
✅ **Shader Graph + HLSL** - 視覚的で保守性高い、かつパワフル
✅ **モバイル最適化** - 中級端末で60fps維持
✅ **包括的カスタマイズ** - カラー変更、レイヤリング、テクスチャスワップ、プロシージャル
✅ **学習可能な設計** - コメント付き、段階的実装

#### Week 1の重要性
**テンプレートコレクション**は、プロジェクト全体の成功を左右する基盤です:
- 将来の機能追加が容易になる
- コード再利用性が高まる
- チーム内での知識共有が簡単になる
- モバイル最適化の方針が明確になる

#### 次のステップ
Week 1完了後、以下の方向性が選択可能:

**フェーズ2（Week 2-4）: シェーダー機能拡張**
1. Week 2: 基本トゥーンライティング実装（ToonLighting.hlsl、OutlineUtils.hlsl）
2. Week 3: カラーカスタマイズシステム（ColorCustomization.hlsl、Unity C#連携）
3. Week 4: 高度機能（マットキャップ、異方性、リムライト）

**フェーズ3（Month 2+）: ゲーム開発本格化**

**メインゲーム（ポケコロ風）開発:**
- キャラクターカスタマイズUI（着せ替えシステム）
- 部屋デコレーションシステム（SoStylized環境活用）
- Addressablesアセット管理（衣装、家具、背景）
- ソーシャル機能（フレンド訪問、ギフト）

**ミニゲーム開発（クラシックボードゲーム集）:**
- Phase 1（MVP）: Tic-Tac-Toe Hex、Hex Reversi、Hex Checkers
- Phase 2: Hex Chinese Checkers、Hex Mancala
- Phase 3: Hex Catan（簡易版）、Hex Battleship、Hex Dominoes、Hex Peg Solitaire、Hex Shogi
- ヘックスグリッドシステム（Axial Coordinate）
- ターン制ゲームロジック
- マルチプレイヤーシステム（ローカル/オンライン、1-4プレイヤー）
- リワード連携（ミニゲーム報酬 → メインゲーム衣装アンロック）
- 詳細: `.claude/MINIGAME_DESIGNS.md` 参照

**共通インフラ:**
- 多言語対応（AITranslator統合）
- セーブ/ロードシステム
- ガチャ/課金システム
- パフォーマンス最適化（UniTask/UniRx、バッチング）

---

**このプロジェクトは、カジュアルモバイルゲーム開発における質感表現・カスタマイズシステム・ミニゲーム統合の新しい標準を目指します。**
