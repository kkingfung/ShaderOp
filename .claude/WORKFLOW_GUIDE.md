# ShaderOp 実践ワークフローガイド

## プロジェクト目標

キャラクターカスタマイズ系モバイルゲーム向けの**次世代質感表現**と**開発効率化**を実現する。

### 3つの柱

1. **次世代質感表現ライブラリ** - マルチレイヤー衣装シェーダー、ダイナミックカラー変更
2. **アセット入稿・デバッグ自動化** - Python/JS自動化ツール、ワンクリックビルド
3. **通信・ロード最適化フレームワーク** - UniTask非同期、通信バッチング

---

## ワークフロー1: 新しいシェーダー開発

### 目標
「サテン生地の質感シェーダーを作成し、実機でデモを確認する」

### ステップ1: 設計（architect）
```
プロンプト:
「サテン生地の異方性反射を表現するシェーダーを設計してください。
要件:
- モバイル向け最適化
- カラーバリエーション対応
- Shader Graphで実装
」

出力:
- シェーダーの技術要件
- プロパティ設計（_AnisotropyStrength, _SatinColor等）
- パフォーマンス目標（ドローコール、テクスチャサンプリング数）
- 実装タスクリスト
```

### ステップ2: シェーダー作成（shader-dev）
```
プロンプト:
「サテン生地シェーダーのShader Graphテンプレートを作成してください」

または コマンド:
/new-shader "SG_Cloth_Satin" "ShaderGraph"

出力:
- Shader Graphノード構成
- HLSLカスタムノード（異方性計算）
- サンプルマテリアル設定
```

### ステップ3: Unity実装（unity-developer）
```
プロンプト:
「サテンシェーダーを適用するマテリアルマネージャーを実装してください。
要件:
- Addressablesでマテリアルをロード
- カラーバリエーションをランタイムで変更
- UIからプレビュー可能
」

出力:
- MaterialManager.cs（Addressables対応）
- SatinMaterialController.cs（カラー変更）
- UI Toolkitプレビュー画面
```

### ステップ4: デモシーン作成（unity-developer）
```
プロンプト:
「サテンシェーダーのデモシーンを作成してください。
- キャラクターモデルに適用
- カラーピッカーUI
- ライティング設定
」

出力:
- DemoScene.unity
- DemoSceneController.cs
- UI（カラーピッカー、パラメータスライダー）
```

### ステップ5: シェーダーレビュー（shader-dev）
```
コマンド:
/review-shader "Assets/Shaders/SG_Cloth_Satin.shadergraph"

出力:
- パフォーマンス評価
- モバイル最適化提案
- ベストプラクティスチェック
- 改善コード例
```

### ステップ6: 最適化（shader-dev + performance-analyzer）
```
プロンプト（shader-dev）:
/optimize-shader "Assets/Shaders/SG_Cloth_Satin.shadergraph"

プロンプト（performance-analyzer）:
「デモシーンのパフォーマンスをプロファイリングしてください」

出力:
- 最適化されたシェーダー
- ドローコール削減提案
- メモリ使用量レポート
- FPS改善案
```

### ステップ7: 実機ビルド（automation-dev + cicd-helper）
```
プロンプト（automation-dev）:
「実機デプロイ用のビルドスクリプトを作成してください」

コマンド（cicd-helper）:
/build-jenkins "Android"

出力:
- ビルド済みAPK
- Firebase App Distribution へ自動配信
- テスター向け通知
```

**所要時間**: 設計30分 → 実装2時間 → レビュー30分 → 最適化1時間 → ビルド15分 = **約4時間**

---

## ワークフロー2: アセット検証ツール開発

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

このチームで実現できること:

✅ **高品質なシェーダー開発** - Shader Graph/HLSL、モバイル最適化
✅ **完全自動化** - アセット検証、ビルド、デプロイ
✅ **パフォーマンス保証** - 60fps維持、メモリ最適化
✅ **品質保証体制** - コードレビュー、テスト、セキュリティ監査
✅ **高速開発サイクル** - 設計からデプロイまで一貫サポート

**すべてのフェーズで専門家がサポート**し、**モバイルゲーム開発を劇的に効率化**します。
