# RoomDecoration Scene - README

## 概要

**RoomDecoration.unity** は、Phase 1布シェーダー（Cotton, Silk, Denim, Leather）を3D環境で展示する技術デモシーンです。

## クイックスタート

### 自動セットアップ（推奨）

1. Unity Editor でこのシーンを開く
2. メニュー: `ShaderOp → Setup → RoomDecoration Cloth Showcase`
3. "Yes" をクリック
4. メニュー: `ShaderOp → Validate → RoomDecoration Scene` で検証
5. **Play** ボタンでシーン実行

### 操作方法

**カメラ**:
- 右クリックドラッグ: 回転
- スクロール: ズーム
- R キー: リセット

**UI（画面下部40%）**:
- Arrow Buttons (</>): カラープリセット変更
- Reset to Default: デフォルトに戻す
- Back to Menu: メインメニューへ

## シェーダー展示

### 1. Curtains (カーテン) - Silk Shader
- **マテリアル**: `MAT_Silk_New`
- **特徴**: 異方性反射、高い滑らかさ、サテン質感
- **カラープリセット**: 8色（Red, Blue, Green, Purple, Gold, Gray, Brown, Pink）

### 2. Rug (ラグ) - Cotton Shader
- **マテリアル**: `MAT_Cotton_New`
- **特徴**: マット質感、拡散反射
- **カラープリセット**: 8色（Beige, Brown, Red, Navy, Green, Gray, Cream, Purple）

### 3. Cushions (クッション) - Denim Shader
- **マテリアル**: `MAT_Denim_New`
- **特徴**: 多層パターンブレンド、デニム質感
- **カラープリセット**: 8色（Blue Denim, Dark Denim, Light Denim, etc.）

### 4. TableCloth (テーブルクロス) - Leather Shader
- **マテリアル**: `MAT_Leather_New`
- **特徴**: レザー質感、適度な反射
- **カラー**: 固定（Brown）

## シーン構成

### 3D Environment
- **Floor**: 50x50 Plane
- **Walls**: 4面（North, South, East, West）
- **Lighting**:
  - Directional Light x1（メインライト）
  - Point Light x2（環境光）

### Camera System
- **OrbitCameraController**:
  - InputManager統合
  - スムーズな回転・ズーム
  - 自動回転機能

### UI System
- **UI Toolkit**（Portrait向け）:
  - Bottom 40% Panel
  - Arrow Button Controls
  - リアルタイムカラー変更

## 技術仕様

### Scripts
- **RoomDecoratorController.cs**: メインUIコントローラー
- **OrbitCameraController.cs**: カメラ制御
- **RoomDecorationData.cs**: データ構造

### Editor Tools
- **RoomDecorationSceneSetup.cs**: 自動セットアップ
- **RoomDecorationValidator.cs**: 検証ツール

### UI Files
- **RoomDecoration.uxml**: UI構造
- **RoomDecoration.uss**: スタイル

### Performance
- **ポリゴン数**: ~10,000 tris
- **ライト数**: 3個
- **目標FPS**: 60 FPS（モバイル）

## トラブルシューティング

### UIが表示されない
1. UIDocument が設定されているか確認
2. RoomDecoration.uxml が割り当てられているか確認
3. 検証ツールを実行

### マテリアルが適用されていない
1. マテリアルが存在するか確認（`Assets/Materials/Cloth/`）
2. 検証ツールを実行
3. シーンを再セットアップ

### カメラが動かない
1. **右クリック**でドラッグしているか確認
2. OrbitCameraController が Main Camera に存在するか確認
3. InputManager が有効か確認

## ドキュメント

### 詳細ガイド
- **ROOMDECORATION_IMPLEMENTATION.md**: 詳細実装ガイド
- **ROOMDECORATION_QUICKSTART.md**: クイックスタート
- **ROOMDECORATION_COMPLETE.md**: 実装完了サマリー
- **ROOMDECORATION_SCENE_STRUCTURE.txt**: シーン構造図

### 関連資料
- **CLOTH_SHADERS_README.md**: 布シェーダー仕様
- **UI_TOOLKIT_PATTERNS.md**: UI Toolkitパターン

## 拡張性

将来的な拡張可能性:
- [ ] TableClothカラーカスタマイズ
- [ ] 家具配置機能
- [ ] プリセット保存/読み込み
- [ ] スクリーンショット機能
- [ ] VR対応

## サポート

問題が発生した場合:
1. Consoleログを確認
2. 検証ツールを実行: `ShaderOp → Validate → RoomDecoration Scene`
3. シーンを再セットアップ: `ShaderOp → Setup → RoomDecoration Cloth Showcase`

---

**最終更新**: 2026-03-08
**バージョン**: 1.0.0
**作成者**: Claude (Unity C# Developer Agent)
