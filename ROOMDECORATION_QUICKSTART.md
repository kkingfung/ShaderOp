# RoomDecoration Scene - Quick Start Guide

## 概要

**RoomDecoration**シーンは、4種類の布シェーダー（Cotton, Silk, Denim, Leather）を3D環境で展示する技術デモです。
このガイドでは、最速でシーンをセットアップして実行する方法を説明します。

## クイックセットアップ（5分）

### ステップ1: シーンを開く

```
Unity Editor で以下のシーンを開く:
Assets/Scenes/RoomDecoration.unity
```

### ステップ2: 自動セットアップ実行

Unity Editor メニューから:
```
ShaderOp → Setup → RoomDecoration Cloth Showcase
```

確認ダイアログで **"Yes"** をクリック

自動的に以下が作成されます:
- ✓ 3D部屋（床、壁）
- ✓ 布シェーダーデモオブジェクト
- ✓ ライティング
- ✓ OrbitCameraController
- ✓ UI Toolkit パネル

### ステップ3: 検証

セットアップ後、検証ツールを実行:
```
ShaderOp → Validate → RoomDecoration Scene
```

**すべてのチェックに合格すれば完了!**

### ステップ4: 実行

**Play** ボタンを押してシーンを実行

## 操作方法

### カメラ操作
- **右クリックドラッグ**: カメラ回転
- **スクロール**: ズームイン/アウト
- **R キー**: カメラリセット

### UI操作（画面下部）

#### Curtain Color (カーテン - Silkシェーダー)
- **< / > ボタン**: 8色のプリセットを循環
- 色: Red, Blue, Green, Purple, Gold, Gray, Brown, Pink

#### Rug Color (ラグ - Cottonシェーダー)
- **< / > ボタン**: 8色のプリセットを循環
- 色: Beige, Brown, Red, Navy, Green, Gray, Cream, Purple

#### Cushion Color (クッション - Denimシェーダー)
- **< / > ボタン**: 8色のプリセットを循環
- 色: Blue Denim, Dark Denim, Light Denim, Gray Denim, etc.

#### その他
- **Reset to Default**: すべてをデフォルト設定にリセット
- **Back to Menu**: メインメニューに戻る

## シェーダー展示内容

### 1. Curtains (カーテン) - Silk Shader
**特徴**:
- 異方性反射（Anisotropic Reflection）
- 高い滑らかさ（High Smoothness）
- サテン質感

**確認ポイント**:
- カメラを回転させると反射がシルクのように変化
- 光の当たり方で質感が変わる

### 2. Rug (ラグ) - Cotton Shader
**特徴**:
- マット質感（Matte）
- 拡散反射（Diffuse Reflection）
- 自然な布感

**確認ポイント**:
- 反射が少なく、柔らかい見た目
- 光が均一に拡散される

### 3. Cushions (クッション) - Denim Shader
**特徴**:
- 多層パターンブレンド（Multi-layer Blend）
- デニム特有の質感
- パターン重ね

**確認ポイント**:
- 複数のテクスチャ層が重なっている
- デニムのような複雑な質感

### 4. TableCloth (テーブルクロス) - Leather Shader
**特徴**:
- レザー質感
- 適度な反射
- リッチな見た目

**確認ポイント**:
- 革のような質感
- 光沢が控えめ

## トラブルシューティング

### 問題: UIが表示されない
**解決策**:
1. UIDocument が RoomDecorationUI GameObject に存在するか確認
2. RoomDecoration.uxml が設定されているか確認
3. シーンを再セットアップ（ステップ2）

### 問題: マテリアルが適用されていない
**解決策**:
1. 検証ツールを実行（ステップ3）
2. マテリアルが存在するか確認:
   - `Assets/Materials/Cloth/MAT_Cotton_New.mat`
   - `Assets/Materials/Cloth/MAT_Silk_New.mat`
   - `Assets/Materials/Cloth/MAT_Denim_New.mat`
   - `Assets/Materials/Cloth/MAT_Leather_New.mat`
3. シーンを再セットアップ

### 問題: カメラが動かない
**解決策**:
1. **右クリック**でドラッグしているか確認（左クリックではない）
2. InputManager が有効か確認
3. OrbitCameraController が Main Camera に存在するか確認

### 問題: パフォーマンスが悪い
**解決策**:
1. Unity Profiler を開く（Window → Analysis → Profiler）
2. 検証ツールでパフォーマンスチェック実行
3. ライトの影を無効化（Directional Light の Shadow Type を "No Shadows" に）

## 次のステップ

### シェーダーをカスタマイズ
1. Shader Graph を開く:
   - `Assets/Shaders/ShaderGraphs/Cloth/SG_FabricCotton.shadergraph`
   - `Assets/Shaders/ShaderGraphs/Cloth/SG_FabricSatin.shadergraph`
   - `Assets/Shaders/ShaderGraphs/Cloth/SG_ClothLayered.shadergraph`

2. パラメータを調整:
   - Base Color
   - Smoothness
   - Normal Map Intensity
   - Anisotropy（Satin shader）

### UIをカスタマイズ
1. UXML を編集:
   - `Assets/UI/RoomDecoration.uxml`

2. USS を編集:
   - `Assets/UI/RoomDecoration.uss`

3. RoomDecoratorController.cs を編集:
   - カラープリセット追加
   - 新しいコントロール追加

### 新しいオブジェクト追加
1. シーンに新しいオブジェクト配置
2. 布シェーダーマテリアルを割り当て
3. RoomDecoratorController.cs にコントロール追加
4. UI に新しいコントロール追加

## 参考リソース

### ドキュメント
- `ROOMDECORATION_IMPLEMENTATION.md` - 詳細実装ガイド
- `CLOTH_SHADERS_README.md` - 布シェーダー仕様
- `UI_TOOLKIT_PATTERNS.md` - UI Toolkitパターン

### スクリプト
- `RoomDecoratorController.cs` - メインUIコントローラー
- `OrbitCameraController.cs` - カメラ制御
- `RoomDecorator.cs` - 部屋デコレーションロジック

### Editor Tools
- `RoomDecorationSceneSetup.cs` - 自動セットアップ
- `RoomDecorationValidator.cs` - 検証ツール

## よくある質問（FAQ）

### Q: 他のシェーダーを追加できますか?
**A**: はい、新しいマテリアルを作成し、RoomDecoratorController.cs にカラープリセットとUI制御を追加してください。

### Q: モバイルでも動作しますか?
**A**: はい、Portrait向けに最適化されています。ただし、ライトの影を無効化することでパフォーマンスが向上します。

### Q: VR対応できますか?
**A**: OrbitCameraController を VR用に拡張する必要がありますが、シーン自体はVR対応可能です。

### Q: スクリーンショット機能はありますか?
**A**: InputManager に統合されています。**F12キー**でスクリーンショット撮影可能です。

## サポート

問題が発生した場合:
1. Consoleログを確認
2. 検証ツールを実行（`ShaderOp → Validate → RoomDecoration Scene`）
3. シーンを再セットアップ（`ShaderOp → Setup → RoomDecoration Cloth Showcase`）

---

**最終更新**: 2026-03-08
**バージョン**: 1.0.0

Happy Shader Development! 🎨
