# SG_CharacterBase Shader - 使用ガイド

## 概要

**ShaderOp/Character/CharacterBase** は、Pokecolo風3Dキャラクターカスタマイズシステム向けの基本キャラクターシェーダーです。

### 主な機能

- 2段階トゥーンシェーディング（Light/Shadow）
- URP Main Light対応
- アンビエントライティング（SH）
- 4ゾーンカラーカスタマイズ（Hair/Skin/Eye/Cloth）
- フレネルベースのリムライト
- モバイル最適化（half精度、最小テクスチャサンプリング）

## シェーダープロパティ一覧

### C# 統合用プロパティ名

MaterialController.cs との互換性を確保するため、以下のプロパティ名を使用しています：

```csharp
// ベース設定
"_BaseColor"          // Color - ベースカラー（デフォルト: 白）
"_MainTex"            // Texture2D - メインテクスチャ

// カスタマイズカラー（4ゾーン）
"_HairColor"          // Color - 髪色（デフォルト: 茶色）
"_SkinColor"          // Color - 肌色（デフォルト: 明るい肌色）
"_EyeColor"           // Color - 瞳色（デフォルト: 青）
"_ClothColor"         // Color - 衣装色（デフォルト: 赤）

// トゥーンシェーディング
"_ShadowColor"        // Color - 影の色（デフォルト: グレー）
"_ShadowThreshold"    // Float (0-1) - 影の境界位置（デフォルト: 0.5）
"_ShadowSmoothness"   // Float (0-1) - 影のぼかし幅（デフォルト: 0.05）

// リムライト
"_RimColor"           // Color - リムライトの色（デフォルト: 白）
"_RimPower"           // Float (0-10) - リムの鋭さ（デフォルト: 3.0）
"_RimIntensity"       // Float (0-1) - リムの強度（デフォルト: 0.5）
```

## C# からの使用例

### MaterialController との統合

```csharp
using ShaderOp.Customization;
using UnityEngine;

public class CharacterExample : MonoBehaviour
{
    [SerializeField] private MaterialController _materialController;

    void Start()
    {
        // 髪色を変更
        _materialController.SetHairColor(new Color(0.8f, 0.3f, 0.1f)); // オレンジ系

        // 肌色を変更
        _materialController.SetSkinColor(new Color(1.0f, 0.87f, 0.78f)); // 明るい肌

        // 瞳色を変更
        _materialController.SetEyeColor(new Color(0.2f, 0.6f, 0.3f)); // 緑色

        // 衣装色を変更
        _materialController.SetClothColor(new Color(0.3f, 0.3f, 0.9f)); // 青い服
    }

    // トゥーンシェーディング調整
    public void AdjustToonShading(float threshold, float smoothness)
    {
        _materialController.SetFloat("_ShadowThreshold", threshold);
        _materialController.SetFloat("_ShadowSmoothness", smoothness);
    }

    // リムライト調整
    public void AdjustRimLight(Color color, float power, float intensity)
    {
        _materialController.SetColor("_RimColor", color);
        _materialController.SetFloat("_RimPower", power);
        _materialController.SetFloat("_RimIntensity", intensity);
    }
}
```

### 直接マテリアル操作

```csharp
using UnityEngine;

public class DirectMaterialExample : MonoBehaviour
{
    [SerializeField] private Material _characterMaterial;

    void Start()
    {
        // 髪色を茶色に変更
        _characterMaterial.SetColor("_HairColor", new Color(0.3f, 0.15f, 0.05f));

        // 影の境界を調整（より暗めに）
        _characterMaterial.SetFloat("_ShadowThreshold", 0.6f);

        // リムライトを強調
        _characterMaterial.SetFloat("_RimIntensity", 0.8f);
    }
}
```

## パフォーマンス仕様

### 推奨設定値

| パラメータ | 推奨値 | 説明 |
|-----------|--------|------|
| Shadow Threshold | 0.5 - 0.7 | 影の境界位置（高いほど影が少ない） |
| Shadow Smoothness | 0.03 - 0.08 | 影のぼかし（低いほどシャープ） |
| Rim Power | 2.0 - 4.0 | リムの鋭さ（高いほど細いリム） |
| Rim Intensity | 0.3 - 0.6 | リムの強度 |

### パフォーマンス指標

- **シェーダーインストラクション数**: 約18-20命令（ForwardLitパス）
- **テクスチャサンプリング**: 1回（_MainTexのみ）
- **ターゲットFPS**: 60fps @ iPhone 11 / Galaxy S10
- **バッチング**: SRP Batcher対応（CBUFFER使用）

### 最適化ポイント

1. **half精度を使用**: モバイルGPUで効率的
2. **最小限のテクスチャサンプリング**: メインテクスチャのみ
3. **条件分岐なし**: smoothstep/lerpでGPUフレンドリー
4. **SRP Batcher互換**: 動的バッチングで高速レンダリング

## マテリアル設定例

### 基本キャラクター設定

```
Base Color: (1.0, 1.0, 1.0, 1.0)  # 白（テクスチャ色をそのまま使用）
Shadow Color: (0.7, 0.7, 0.7, 1.0)  # グレー
Shadow Threshold: 0.5
Shadow Smoothness: 0.05
Rim Color: (1.0, 1.0, 1.0, 1.0)
Rim Power: 3.0
Rim Intensity: 0.5
```

### アニメ風キャラクター設定

```
Shadow Threshold: 0.6  # 影を少なめに
Shadow Smoothness: 0.02  # シャープな影境界
Rim Power: 4.0  # 細いリムライト
Rim Intensity: 0.7  # 強いリム
```

### ソフト・トゥーン設定

```
Shadow Threshold: 0.5
Shadow Smoothness: 0.1  # 柔らかい影境界
Rim Power: 2.0  # 広いリムライト
Rim Intensity: 0.3  # 控えめなリム
```

## 技術仕様

### シェーダーターゲット

- **Shader Model**: 4.5
- **Render Pipeline**: URP (Universal Render Pipeline)
- **Render Queue**: Geometry (2000)
- **Render Type**: Opaque

### サポート機能

- Main Light Shadows（リアルタイムシャドウ）
- Soft Shadows（ソフトシャドウ）
- Additional Lights（追加ライト、オプション）
- Ambient Lighting（SH環境光）

### パス構成

1. **ForwardLit**: メインレンダリングパス
2. **ShadowCaster**: 影を落とすパス
3. **DepthOnly**: 深度書き込みパス

## 今後の拡張予定

### カラーマスクシステム（Phase 2）

現在は各カラープロパティを直接使用していますが、将来的にカラーマスクテクスチャを追加予定：

```hlsl
// 将来の実装例
TEXTURE2D(_ColorMaskTex);  // RGBAチャンネルでゾーン分け
// R: Hair
// G: Skin
// B: Eyes
// A: Cloth

half4 colorMask = SAMPLE_TEXTURE2D(_ColorMaskTex, sampler_ColorMaskTex, uv);
half3 finalColor = baseColor;
finalColor = lerp(finalColor, _HairColor.rgb, colorMask.r);
finalColor = lerp(finalColor, _SkinColor.rgb, colorMask.g);
finalColor = lerp(finalColor, _EyeColor.rgb, colorMask.b);
finalColor = lerp(finalColor, _ClothColor.rgb, colorMask.a);
```

### スペキュラーハイライト（オプション）

髪や目用の光沢ハイライト追加予定。

### アウトライン対応

別シェーダー（ToonOutline.shader）と組み合わせて使用。

## トラブルシューティング

### 影が表示されない

**原因**: URPアセットでシャドウが無効

**解決**:
1. URP Asset を選択
2. Shadows → Max Distance を 50 以上に設定
3. Main Light で Shadow Type = Soft/Hard を選択

### リムライトが見えない

**原因**: Rim Intensity が低すぎる、またはRim Powerが高すぎる

**解決**:
- Rim Intensity を 0.5 以上に設定
- Rim Power を 2.0〜4.0 の範囲に調整
- Rim Color を明るい色（白など）に設定

### カラー変更が反映されない

**原因**: マテリアルインスタンスを使用していない

**解決**:
```csharp
// NG: sharedMaterialを直接変更（全オブジェクトに影響）
renderer.sharedMaterial.SetColor("_HairColor", color);

// OK: materialプロパティでインスタンス作成
renderer.material.SetColor("_HairColor", color);

// またはMaterialControllerを使用（推奨）
materialController.SetHairColor(color);
```

### パフォーマンスが悪い

**チェック項目**:
1. SRP Batcher が有効か確認（URP Asset設定）
2. 不要なシャドウキャスターを無効化
3. Additional Lightsを最小限に抑える
4. LODシステムを導入

## 関連ファイル

- **シェーダー本体**: `Assets/Shaders/Character/SG_CharacterBase.shader`
- **MaterialController**: `Assets/Scripts/Runtime/Customization/MaterialController.cs`
- **CharacterCustomizer**: `Assets/Scripts/Runtime/Customization/CharacterCustomizer.cs`
- **HLSLインクルード**: `Assets/Shaders/Includes/ToonLightingCore.hlsl`

## 参考リソース

- Unity URP Shader Documentation: https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest
- Unity Shader Graph: https://docs.unity3d.com/Packages/com.unity.shadergraph@latest
- Unity-Chan Toon Shader: https://github.com/unity3d-jp/UnityChanToonShaderVer2_Project

---

**作成日**: 2026-02-28
**最終更新**: 2026-02-28
**作成者**: Claude Code (ShaderOp Project)
**バージョン**: 1.0.0
