# RoomDecoration Scene Implementation Guide

## 概要

**RoomDecoration**シーンは、Phase 1布シェーダー4種類（Cotton, Silk, Denim, Leather）を3D環境で展示する技術デモシーンです。

## 実装内容

### 1. シーン構成

#### A. 3D Room Environment
- **Floor**: 50x50 Plane（床）
- **Walls**: 4面の壁（North, South, East, West）
- **Lighting**:
  - Directional Light（メインライト、太陽光シミュレーション）
  - Point Light x2（環境光）

#### B. Cloth Shader Demonstration Objects

| オブジェクト | シェーダー | 用途 | 特徴 |
|------------|----------|------|------|
| **Curtains (Left/Right)** | Silk (Satin) | カーテン | 異方性反射、高い滑らかさ |
| **Rug** | Cotton | ラグ/カーペット | マット、拡散反射 |
| **Cushions (x3)** | Denim (Layered) | クッション | 多層パターンブレンド |
| **TableCloth** | Leather | テーブルクロス | レザー質感 |

#### C. Camera System
- **OrbitCameraController**:
  - 右クリックドラッグで回転
  - スクロールでズーム
  - スムーズなダンピング
  - 自動回転機能

#### D. UI Toolkit Panel (Bottom 40%)

Portrait向けUI（縦画面専用）:
- **Curtain Color Control**: カーテン色変更（8色プリセット）
- **Rug Color Control**: ラグ色変更（8色プリセット）
- **Cushion Color Control**: クッション色変更（8色プリセット）
- **Reset Button**: デフォルト設定にリセット
- **Back Button**: メインメニューに戻る

### 2. 技術スタック

#### A. シェーダー
- `SG_FabricCotton.shadergraph` - マット質感、拡散反射
- `SG_FabricSatin.shadergraph` - 異方性反射、高smoothness
- `SG_ClothLayered.shadergraph` - 多層ブレンド、パターン重ね
- `SG_FabricSatin.shadergraph`（Leather設定） - レザー質感

#### B. スクリプト
- **RoomDecoratorController.cs** - メインUIコントローラー
  - UI Toolkit統合
  - カラープリセット管理
  - マテリアルインスタンス生成・適用
  - Arrow Buttonイベント処理

- **OrbitCameraController.cs** - カメラ制御
  - InputManager統合
  - マウス/タッチ入力対応
  - スムーズな回転・ズーム

#### C. UI Toolkit
- **RoomDecoration.uxml** - UI構造定義
- **RoomDecoration.uss** - スタイル定義（Portrait最適化）

### 3. 自動セットアップツール

#### RoomDecorationSceneSetup.cs
**メニュー**: `ShaderOp → Setup → RoomDecoration Cloth Showcase`

自動生成内容:
1. 3D部屋ジオメトリ（床、壁）
2. 布シェーダーデモオブジェクト配置
3. マテリアル自動割り当て
4. ライティング設定
5. OrbitCameraController セットアップ
6. UI Toolkit パネル統合

**使用方法**:
```
1. Unity Editor で RoomDecoration.unity を開く
2. メニューから ShaderOp → Setup → RoomDecoration Cloth Showcase を選択
3. 確認ダイアログで "Yes" をクリック
4. 自動セットアップ完了
```

### 4. 検証ツール

#### RoomDecorationValidator.cs
**メニュー**: `ShaderOp → Validate → RoomDecoration Scene`

検証項目:
- ✓ 必須オブジェクト存在確認
- ✓ マテリアル割り当て確認
- ✓ ライティング設定確認
- ✓ UI Toolkit設定確認
- ✓ カメラ設定確認
- ✓ パフォーマンスチェック（ポリゴン数、ライト数、マテリアル数）

**推奨パフォーマンス目標**:
- ポリゴン数: 50,000 tris 以下
- ライト数: 3-5個
- マテリアル数: 20個以下
- 目標FPS: 60 FPS（モバイル）

### 5. カラープリセット

#### Curtain (Silk Shader)
```csharp
Red, Blue, Green, Purple, Gold, Gray, Brown, Pink
```

#### Rug (Cotton Shader)
```csharp
Beige, Brown, Red, Navy, Green, Gray, Cream, Purple
```

#### Cushion (Denim Shader)
```csharp
Blue Denim, Dark Denim, Light Denim, Gray Denim,
Red Denim, Green Denim, Brown Denim, Purple Denim
```

### 6. シーンフロー

```
MainMenu
    ↓ (Room Decoration Button)
RoomDecoration
    ↓ (Back Button)
MainMenu
```

### 7. ディレクトリ構造

```
ShaderOptimizer/
├── Assets/
│   ├── Scenes/
│   │   └── RoomDecoration.unity
│   ├── Scripts/
│   │   ├── Runtime/
│   │   │   ├── Core/
│   │   │   │   └── OrbitCameraController.cs
│   │   │   └── Customization/
│   │   │       ├── RoomDecorator.cs
│   │   │       ├── RoomDecoratorController.cs
│   │   │       └── RoomDecorationData.cs
│   │   └── Editor/
│   │       ├── RoomDecorationSceneSetup.cs
│   │       └── RoomDecorationValidator.cs
│   ├── Materials/
│   │   └── Cloth/
│   │       ├── MAT_Cotton_New.mat
│   │       ├── MAT_Silk_New.mat
│   │       ├── MAT_Denim_New.mat
│   │       └── MAT_Leather_New.mat
│   ├── Shaders/
│   │   └── ShaderGraphs/
│   │       └── Cloth/
│   │           ├── SG_FabricCotton.shadergraph
│   │           ├── SG_FabricSatin.shadergraph
│   │           └── SG_ClothLayered.shadergraph
│   └── UI/
│       ├── RoomDecoration.uxml
│       └── RoomDecoration.uss
```

### 8. 実装パターン

#### A. UI Toolkit Arrow Button Pattern
```csharp
private void OnCurtainColorNextClicked()
{
    _currentCurtainColorIndex = (_currentCurtainColorIndex + 1) % _curtainColors.Length;
    ApplyCurtainColor();
    UpdateCurtainColorLabel();
}
```

#### B. Material Instancing Pattern
```csharp
private void ApplyCurtainColor()
{
    Color color = _curtainColors[_currentCurtainColorIndex];

    if (_curtainLeft != null && _silkMaterial != null)
    {
        var renderer = _curtainLeft.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material instanceMat = new Material(_silkMaterial);
            instanceMat.SetColor("_BaseColor", color);
            renderer.material = instanceMat;
        }
    }
}
```

#### C. Camera Orbit Pattern
```csharp
// OrbitCameraController は InputManager と統合
InputManager.Instance.OnMouseDrag += OnMouseDrag;
InputManager.Instance.OnMouseScroll += OnMouseScroll;
```

### 9. トラブルシューティング

#### 問題: マテリアルが割り当てられない
**解決策**:
1. `Assets/Materials/Cloth/` にマテリアルが存在するか確認
2. RoomDecorationValidator.cs を実行して検証
3. RoomDecorationSceneSetup.cs で再セットアップ

#### 問題: UIが表示されない
**解決策**:
1. UIDocument に `RoomDecoration.uxml` が設定されているか確認
2. Canvas Scaler Mode を確認（Portrait: 9:16）
3. UI Panel の position が `absolute` で `bottom: 0` になっているか確認

#### 問題: カメラが動かない
**解決策**:
1. OrbitCameraController の `_target` が設定されているか確認
2. InputManager が有効化されているか確認
3. 右クリックドラッグ、スクロールが正しく動作するか確認

### 10. パフォーマンス最適化

#### A. マテリアルインスタンス管理
- 各色変更時に新しいマテリアルインスタンスを生成
- 古いインスタンスは自動的にGCで回収
- 頻繁な変更でもメモリリークなし

#### B. ライティング最適化
- Directional Light: 1個（メインライト）
- Point Light: 2個（環境光）
- Realtime Shadow: 必要に応じて無効化

#### C. ポリゴン削減
- プリミティブ使用（Cube, Plane, Quad）
- カスタムメッシュ不要
- 総ポリゴン数: 約10,000 tris

### 11. 拡張性

将来的な拡張可能性:
- [ ] テーブルクロスのカラーカスタマイズ追加
- [ ] 家具配置機能（RoomDecorator.cs統合）
- [ ] スクリーンショット機能
- [ ] プリセット保存/読み込み
- [ ] VR対応（OrbitCameraController拡張）

### 12. 関連ドキュメント

- `ROOMDECORATION_QUICKSTART.md` - クイックスタートガイド
- `CLOTH_SHADERS_README.md` - 布シェーダー詳細
- `UI_TOOLKIT_PATTERNS.md` - UI Toolkitパターン集

---

**最終更新**: 2026-03-08
**バージョン**: 1.0.0
**作成者**: Claude (Unity C# Developer Agent)
