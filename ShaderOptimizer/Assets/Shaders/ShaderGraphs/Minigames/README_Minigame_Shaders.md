# ミニゲーム用 Shader Graph ライブラリ

## 概要

モバイルゲーム向けに最適化された2Dヘックスボードゲーム用シェーダーコレクション。
60fps安定動作、低ドローコール、バッチング対応を最重要視。

---

## シェーダー一覧

### 1. SG_HexTile_Interactive.shadergraph
**パス**: `ShaderOp/Minigames/HexTile_Interactive`

**用途**: インタラクティブなヘックスタイル（ホバー・選択・無効状態対応）

**機能**:
- 4つの状態サポート（Normal/Hover/Selected/Disabled）
- スムーズな状態遷移アニメーション
- チームカラーティント（赤/青/ニュートラル）
- 有効手のグロー表現
- GPU Instancing対応（100+タイル同時バッチング）

**パラメータ**:
```
Base Color: タイルのベースカラー（デフォルト: 白）
Team Color: チーム識別色（デフォルト: 白）
Team Tint Strength: チームカラーの強度（0〜1、デフォルト: 0.3）
Hover Brightness: ホバー時の明るさ（1〜2、デフォルト: 1.2）
Glow Color: グロー色（デフォルト: 黄）
Glow Intensity: グロー強度（0〜2、デフォルト: 0.8）
Glow Speed: グロー点滅速度（0〜5、デフォルト: 2.0）
State: タイル状態（0=Normal, 1=Hover, 2=Selected, 3=Disabled）
```

**使用例**:
```csharp
// タイル状態を変更
_tileMaterial.SetFloat("_State", 1f); // Hover状態

// チームカラーを設定
_tileMaterial.SetColor("_TeamColor", Color.red);
_tileMaterial.SetFloat("_TeamTintStrength", 0.5f);

// グロー効果を有効化
_tileMaterial.SetFloat("_GlowIntensity", 1.0f);
```

---

### 2. SG_Avatar_2D.shadergraph
**パス**: `ShaderOp/Minigames/Avatar_2D`

**用途**: 2Dアバタースプライト（カスタマイズ対応）

**機能**:
- 2チャンネルカラーマスク（髪・服）
- フラットカラー + シンプルリムライト
- アルファクリッピング対応
- スプライトバッチング最適化
- 縦画面UI向け最適化

**パラメータ**:
```
Main Texture: アバタースプライト
Color Mask: カラーマスクテクスチャ（R=髪, G=服）
Hair Color: 髪の色
Clothing Color: 服の色
Rim Color: リムライトの色
Rim Power: リムライトの鋭さ（0.5〜5、デフォルト: 2.0）
Rim Intensity: リムライトの強度（0〜1、デフォルト: 0.3）
Alpha Cutoff: アルファクリッピング閾値（0〜1、デフォルト: 0.5）
```

**使用例**:
```csharp
// アバター色をカスタマイズ
_avatarMaterial.SetColor("_HairColor", playerHairColor);
_avatarMaterial.SetColor("_ClothingColor", playerClothingColor);

// リムライトでポップな表現
_avatarMaterial.SetColor("_RimColor", Color.white);
_avatarMaterial.SetFloat("_RimIntensity", 0.4f);
```

---

### 3. SG_GamePiece_2D.shadergraph
**パス**: `ShaderOp/Minigames/GamePiece_2D`

**用途**: ゲームピース（駒・コマ）スプライト

**機能**:
- プレイヤーカラーティント
- 選択時ハイライト
- 配置アニメーション対応（フェードイン・スケール）
- シンプルなドロップシャドウ
- バッチング対応

**パラメータ**:
```
Main Texture: ピーススプライト
Player Color: プレイヤー色
Tint Strength: 色ティント強度（0〜1、デフォルト: 0.5）
Highlight Color: 選択時ハイライト色
Highlight Intensity: ハイライト強度（0〜1、デフォルト: 0）
Fade: フェード値（0〜1、デフォルト: 1）
Shadow Offset: シャドウオフセット（Vector2、デフォルト: (0.02, -0.02)）
Shadow Opacity: シャドウ不透明度（0〜1、デフォルト: 0.3）
```

**使用例**:
```csharp
// ピース配置アニメーション
await FadePieceIn(_pieceMaterial);

async UniTask FadePieceIn(Material mat)
{
    float t = 0f;
    while (t < 1f)
    {
        t += Time.deltaTime * 2f;
        mat.SetFloat("_Fade", Mathf.SmoothStep(0, 1, t));
        await UniTask.Yield();
    }
}

// プレイヤーカラー設定
_pieceMaterial.SetColor("_PlayerColor", playerColor);
_pieceMaterial.SetFloat("_TintStrength", 0.6f);

// 選択状態
_pieceMaterial.SetFloat("_HighlightIntensity", 1.0f);
```

---

### 4. SG_UI_Button.shadergraph
**パス**: `ShaderOp/Minigames/UI_Button`

**用途**: UIボタンのホバー・プレスエフェクト

**機能**:
- ホバー時グラデーション
- プレス時ダークニング
- グロー境界線
- カラーカスタマイズ対応
- UI Canvas Renderer 最適化

**パラメータ**:
```
Base Color: ボタンベースカラー
Hover Color: ホバー時カラー
Press Color: プレス時カラー
Border Color: 境界線カラー
Border Width: 境界線幅（0〜0.1、デフォルト: 0.02）
Glow Intensity: グロー強度（0〜1、デフォルト: 0）
State: ボタン状態（0=Normal, 1=Hover, 2=Pressed, 3=Disabled）
```

**使用例**:
```csharp
// UI Button コンポーネントと連携
public class UIButtonShaderController : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Image _buttonImage;
    private Material _buttonMaterial;

    void Start()
    {
        _buttonMaterial = _buttonImage.material;
    }

    public void OnPointerEnter()
    {
        _buttonMaterial.SetFloat("_State", 1f); // Hover
    }

    public void OnPointerDown()
    {
        _buttonMaterial.SetFloat("_State", 2f); // Pressed
    }

    public void OnPointerUp()
    {
        _buttonMaterial.SetFloat("_State", 1f); // Hover
    }

    public void OnPointerExit()
    {
        _buttonMaterial.SetFloat("_State", 0f); // Normal
    }
}
```

---

### 5. SG_Background_Gradient.shadergraph
**パス**: `ShaderOp/Minigames/Background_Gradient`

**用途**: 縦画面向け背景グラデーション

**機能**:
- 2色〜4色グラデーション
- 縦方向・横方向切り替え
- オプショナルノイズテクスチャ
- 軽量（計算のみ、テクスチャサンプリング最小）

**パラメータ**:
```
Color Top: 上部カラー
Color Bottom: 下部カラー
Color Mid 1: 中間カラー1（オプション）
Color Mid 2: 中間カラー2（オプション）
Gradient Direction: グラデーション方向（0=縦, 1=横）
Noise Texture: ノイズテクスチャ（オプション）
Noise Intensity: ノイズ強度（0〜1、デフォルト: 0.1）
```

---

### 6. SG_Particle_Star.shadergraph
**パス**: `ShaderOp/Minigames/Particle_Star`

**用途**: キラキラエフェクト用パーティクルシェーダー

**機能**:
- ソフトパーティクル（深度フェード）
- カラーグラデーション（ライフタイム）
- アルファディゾルブ
- 加算ブレンド対応

**パラメータ**:
```
Particle Texture: パーティクルテクスチャ（スター形状）
Start Color: 開始色
End Color: 終了色
Fade Softness: フェード柔らかさ（0〜5、デフォルト: 1.0）
Lifetime: パーティクルライフタイム進行度（0〜1）
```

---

## パフォーマンス最適化

### モバイル最適化チェックリスト

#### ✅ シェーダー設計
- [x] 全シェーダーで Half 精度を使用
- [x] GPU Instancing 有効化（タイル・ピース）
- [x] SRP Batcher 互換性確保
- [x] テクスチャサンプリング最小化（最大2回/シェーダー）
- [x] 条件分岐を Lerp で実装（GPUフレンドリー）

#### ✅ バッチング戦略
- [x] タイルシェーダー: GPU Instancing で100+タイルをバッチ
- [x] アバター: Sprite Atlas + Static Batching
- [x] UI: Canvas Batch
- [x] パーティクル: Particle System Batching

#### ✅ ドローコール目標
- ヘックスタイル（100個）: 1 draw call
- アバター（10個）: 1 draw call
- UI（20ボタン）: 1 draw call
- 背景: 1 draw call
- パーティクル: 1-2 draw calls
**合計**: 5-6 draw calls/フレーム（目標: 50以下）

### ベンチマーク（想定）

| デバイス | FPS (10キャラ + 100タイル) | 備考 |
|---------|---------------------------|------|
| iPhone SE 2 | 60 FPS | ターゲットデバイス |
| Galaxy A52 | 60 FPS | ターゲットデバイス |
| Pixel 4a | 60 FPS | 軽量シェーダー |
| iPhone 12 | 60 FPS | 余裕あり |

---

## テクスチャ要件

### カラーマスクテクスチャ（SG_Avatar_2D）

**フォーマット**: PNG 512x512
**チャンネル構成**:
- R チャンネル: 髪の毛（白=適用、黒=非適用）
- G チャンネル: 服（白=適用、黒=非適用）
- B チャンネル: 未使用（拡張用）
- A チャンネル: 未使用

**Unity設定**:
```
Texture Type: Default
sRGB: OFF（重要！）
Alpha Source: None
Compression: Normal Quality
Max Size: 512
```

### パーティクルテクスチャ（SG_Particle_Star）

**フォーマット**: PNG 128x128
**要件**:
- アルファチャンネル付き
- グラデーション形状（中心明るく、外側透明）
- シンプルな星・キラキラ形状

**Unity設定**:
```
Texture Type: Default
Alpha Source: Input Texture Alpha
Alpha Is Transparency: ON
Compression: Normal Quality
Max Size: 128
```

---

## C# 統合パターン

### HexTile 状態管理

```csharp
using UnityEngine;

namespace ShaderOp.Minigames
{
    /// <summary>
    /// ヘックスタイルのシェーダー制御
    /// </summary>
    public class HexTileShaderController : MonoBehaviour
    {
        private Material _tileMaterial;
        private Renderer _renderer;

        public enum TileState
        {
            Normal = 0,
            Hover = 1,
            Selected = 2,
            Disabled = 3
        }

        void Awake()
        {
            _renderer = GetComponent<Renderer>();
            _tileMaterial = _renderer.material; // インスタンス化
        }

        /// <summary>
        /// タイル状態を設定
        /// </summary>
        public void SetState(TileState state)
        {
            _tileMaterial.SetFloat("_State", (float)state);
        }

        /// <summary>
        /// チームカラーを設定
        /// </summary>
        public void SetTeamColor(Color color, float strength = 0.5f)
        {
            _tileMaterial.SetColor("_TeamColor", color);
            _tileMaterial.SetFloat("_TeamTintStrength", strength);
        }

        /// <summary>
        /// 有効手グロー表示
        /// </summary>
        public void ShowValidMoveGlow(bool show)
        {
            _tileMaterial.SetFloat("_GlowIntensity", show ? 1.0f : 0f);
        }

        void OnDestroy()
        {
            // マテリアルインスタンスを破棄
            if (_tileMaterial != null)
            {
                Destroy(_tileMaterial);
            }
        }
    }
}
```

### UniTask によるアニメーション

```csharp
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ShaderOp.Minigames
{
    /// <summary>
    /// ゲームピースのアニメーション制御
    /// </summary>
    public class GamePieceAnimator : MonoBehaviour
    {
        private Material _pieceMaterial;

        /// <summary>
        /// フェードインアニメーション
        /// </summary>
        public async UniTask FadeIn(float duration = 0.5f)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0, 1, elapsed / duration);
                _pieceMaterial.SetFloat("_Fade", t);
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
            _pieceMaterial.SetFloat("_Fade", 1f);
        }

        /// <summary>
        /// ハイライト点滅
        /// </summary>
        public async UniTask BlinkHighlight(int blinkCount = 3, float interval = 0.3f)
        {
            for (int i = 0; i < blinkCount; i++)
            {
                _pieceMaterial.SetFloat("_HighlightIntensity", 1f);
                await UniTask.Delay((int)(interval * 500));

                _pieceMaterial.SetFloat("_HighlightIntensity", 0f);
                await UniTask.Delay((int)(interval * 500));
            }
        }
    }
}
```

---

## ディレクトリ構造

```
Assets/Shaders/ShaderGraphs/Minigames/
├── SG_HexTile_Interactive.shadergraph    # ヘックスタイルシェーダー
├── SG_Avatar_2D.shadergraph              # 2Dアバターシェーダー
├── SG_GamePiece_2D.shadergraph           # ゲームピースシェーダー
├── SG_UI_Button.shadergraph              # UIボタンシェーダー
├── SG_Background_Gradient.shadergraph    # 背景グラデーション
├── SG_Particle_Star.shadergraph          # パーティクルシェーダー
└── README_Minigame_Shaders.md            # このファイル
```

---

## トラブルシューティング

### タイルのバッチングが効かない

**原因**: マテリアルプロパティが異なる

**解決**:
```csharp
// MaterialPropertyBlock を使用（バッチングを維持）
MaterialPropertyBlock props = new MaterialPropertyBlock();
props.SetColor("_TeamColor", color);
_renderer.SetPropertyBlock(props);
```

### アバターの色が変わらない

**原因**: カラーマスクの sRGB 設定が ON

**解決**:
1. カラーマスクテクスチャを選択
2. Inspector で `sRGB (Color Texture)` を **OFF**
3. Apply をクリック

### UIボタンの状態変更が反映されない

**原因**: マテリアルが共有されている

**解決**:
```csharp
// Image コンポーネントのマテリアルをインスタンス化
_buttonImage.material = new Material(_buttonImage.material);
```

### パーティクルが表示されない

**原因**: パーティクルシェーダーのブレンドモード

**解決**:
1. Shader Graph を開く
2. Graph Settings → Blend Mode → **Additive**
3. Save Asset

---

## 参考リソース

- Unity Shader Graph: https://docs.unity3d.com/Packages/com.unity.shadergraph@latest
- URP Documentation: https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest
- モバイル最適化: https://docs.unity3d.com/Manual/MobileOptimizationGraphicsMethods.html
- Sprite Batching: https://docs.unity3d.com/Manual/SpritePacker.html

---

**作成日**: 2026-03-01
**作成者**: Shader Developer (Claude Code)
**バージョン**: 1.0.0
