# CharacterBase シェーダー クイックリファレンス

## プロパティ早見表

### 必須設定

| プロパティ | C#名 | デフォルト | 説明 |
|-----------|------|-----------|------|
| Base Color | `_BaseColor` | 白 (1,1,1,1) | ベースとなる色 |
| Main Texture | `_MainTex` | - | キャラクターのメインテクスチャ |

### カスタマイズカラー（4ゾーン）

| プロパティ | C#名 | デフォルト | 用途 |
|-----------|------|-----------|------|
| Hair Color | `_HairColor` | 茶 (0.2,0.1,0.05) | 髪の毛の色 |
| Skin Color | `_SkinColor` | 肌色 (1.0,0.87,0.78) | 肌の色 |
| Eye Color | `_EyeColor` | 青 (0.3,0.5,1.0) | 瞳の色 |
| Cloth Color | `_ClothColor` | 赤 (0.8,0.2,0.2) | 衣装の色 |

### トゥーンシェーディング

| プロパティ | C#名 | 範囲 | 推奨値 | 説明 |
|-----------|------|------|--------|------|
| Shadow Color | `_ShadowColor` | - | (0.7,0.7,0.7) | 影の色 |
| Shadow Threshold | `_ShadowThreshold` | 0-1 | 0.5 | 影の境界（高=明るい） |
| Shadow Smoothness | `_ShadowSmoothness` | 0-1 | 0.05 | ぼかし幅（低=シャープ） |

### リムライト

| プロパティ | C#名 | 範囲 | 推奨値 | 説明 |
|-----------|------|------|--------|------|
| Rim Color | `_RimColor` | - | 白 (1,1,1) | リムライトの色 |
| Rim Power | `_RimPower` | 0-10 | 3.0 | 鋭さ（高=細い） |
| Rim Intensity | `_RimIntensity` | 0-1 | 0.5 | 強度（高=明るい） |

## プリセット設定

### アニメ風（Anime Style）

```
Shadow Threshold: 0.6
Shadow Smoothness: 0.02
Rim Power: 4.0
Rim Intensity: 0.7
```

シャープな影境界と強いリムライトでアニメ調の表現。

### ソフトトゥーン（Soft Toon）

```
Shadow Threshold: 0.5
Shadow Smoothness: 0.1
Rim Power: 2.0
Rim Intensity: 0.3
```

柔らかい影と控えめなリムライトで優しい印象。

### リアル寄り（Semi-Realistic）

```
Shadow Threshold: 0.4
Shadow Smoothness: 0.15
Rim Power: 5.0
Rim Intensity: 0.2
```

影が多めで自然な陰影感。

## C# コードスニペット

### 基本的なカラー変更

```csharp
// MaterialController経由（推奨）
materialController.SetHairColor(new Color(0.8f, 0.3f, 0.1f));
materialController.SetSkinColor(new Color(1.0f, 0.87f, 0.78f));
materialController.SetEyeColor(new Color(0.2f, 0.6f, 0.3f));
materialController.SetClothColor(new Color(0.3f, 0.3f, 0.9f));
```

### マテリアル直接操作

```csharp
// 髪色変更
material.SetColor("_HairColor", hairColor);

// トゥーンシェーディング調整
material.SetFloat("_ShadowThreshold", 0.6f);
material.SetFloat("_ShadowSmoothness", 0.05f);

// リムライト調整
material.SetFloat("_RimPower", 3.5f);
material.SetFloat("_RimIntensity", 0.7f);
```

## Unity エディターメニュー

### マテリアル作成

```
ShaderOp > Create Material > Character Base Material
ShaderOp > Create Material > Character Base (Anime Style)
ShaderOp > Create Material > Character Base (Soft Toon)
```

### プリセット適用（マテリアル選択時）

```
ShaderOp > Apply Preset > Anime Style
ShaderOp > Apply Preset > Soft Toon
```

## パフォーマンス目安

| デバイス | FPS | キャラクター数 |
|---------|-----|--------------|
| iPhone 12 Pro | 60 | 10+ |
| iPhone 11 | 60 | 8-10 |
| Galaxy S10 | 60 | 8-10 |
| iPhone SE (2nd) | 55-60 | 6-8 |

## トラブルシューティング

### 影が表示されない
- URP Asset → Shadows → Max Distance を確認（50以上推奨）
- Main Light → Shadow Type を Hard/Soft に設定

### リムライトが見えない
- Rim Intensity を上げる（0.5以上）
- Rim Power を下げる（2.0〜4.0）

### カラー変更が反映されない
- `material`プロパティを使用（`sharedMaterial`ではない）
- MaterialControllerを使用（推奨）

### パフォーマンスが悪い
- SRP Batcher を有効化（URP Asset）
- 不要なシャドウキャスターを無効化
- Additional Lightsを減らす

## 関連ファイル

- **シェーダー**: `Assets/Shaders/Character/SG_CharacterBase.shader`
- **詳細ガイド**: `Assets/Shaders/Character/SG_CharacterBase_USAGE.md`
- **MaterialController**: `Assets/Scripts/Runtime/Customization/MaterialController.cs`
- **エディターヘルパー**: `Assets/Scripts/Editor/CharacterBaseShaderHelper.cs`

---

**最終更新**: 2026-02-28
