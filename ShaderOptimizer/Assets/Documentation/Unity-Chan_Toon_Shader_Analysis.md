# Unity-Chan Toon Shader 2.0.6 解析レポート

**解析日**: 2026-02-22
**対象バージョン**: Unity-Chan Toon Shader ver.2.0.7.5
**参照元**: `Assets/SD Unity-Chan Haon Custom/Shader/Unity-chan Toon Shader Ver 2.0.6/`

---

## 📋 目次

1. [概要](#概要)
2. [シェーダー構造](#シェーダー構造)
3. [セルシェーディング実装](#セルシェーディング実装)
4. [アウトライン実装](#アウトライン実装)
5. [モバイル最適化テクニック](#モバイル最適化テクニック)
6. [Shader Graphへの移植ポイント](#shader-graphへの移植ポイント)

---

## 概要

Unity-Chan Toon Shader (UCTS) は、Unity Technologies Japan が開発した高品質トゥーンシェーダーです。
本プロジェクトでは、この実装を参考にしてモバイル最適化された Shader Graph テンプレートを構築します。

### 主要機能

- **2トーンセルシェーディング** (Base → 1st Shade → 2nd Shade)
- **フェザリング** (段階的グラデーション)
- **アウトライン** (距離ベース幅調整)
- **リムライト** (Antipodean RimLight対応)
- **マットキャップ** (カメラロール補正付き)
- **ハイカラー/スペキュラー**
- **エミッシブアニメーション**

---

## シェーダー構造

### ファイル構成

```
Unity-chan Toon Shader Ver 2.0.6/
├── UCTS_DoubleShadeWithFeather.cginc    # セルシェーディングコア（442行）
├── UCTS_Outline.cginc                   # アウトライン実装（約150行）
├── UCTS_ShadingGradeMap.cginc           # グラデーションマップ版
├── UCTS_ShadowCaster.cginc              # シャドウキャスター
├── Tess/                                # テッセレーション版（高品質）
└── [各種.shaderファイル]                # 30種類以上のバリエーション
```

### マルチコンパイル構成

```csharp
#pragma multi_compile _IS_CLIPPING_OFF _IS_CLIPPING_MODE _IS_CLIPPING_TRANSMODE
#pragma multi_compile _IS_PASS_FWDBASE _IS_PASS_FWDDELTA
#pragma multi_compile _OUTLINE_NML _OUTLINE_POS
#pragma multi_compile _EMISSIVE_SIMPLE _EMISSIVE_ANIMATION
```

---

## セルシェーディング実装

### 核心アルゴリズム: 2段階シャドウ

**UCTS_DoubleShadeWithFeather.cginc: 245-254行**

#### 1. **Half-Lambert計算** (ライティング基礎)

```hlsl
// Line 245: 基本ライティング計算
float _HalfLambert_var = 0.5 * dot(lerp(i.normalDir, normalDirection, _Is_NormalMapToBase), lightDirection) + 0.5;
```

**ポイント**:
- **Half-Lambert**: `0.5 * dot(N, L) + 0.5` で範囲を [0, 1] に正規化
- `_Is_NormalMapToBase`: ノーマルマップの適用ON/OFF切り替え
- `normalDirection`: ノーマルマップ適用後の法線
- `i.normalDir`: 頂点法線

#### 2. **システムシャドウ統合** (リアルタイムシャドウ対応)

```hlsl
// Line 250: システムシャドウレベル計算
float _SystemShadowsLevel_var = (attenuation * 0.5) + 0.5 + _Tweak_SystemShadowsLevel > 0.001
    ? (attenuation * 0.5) + 0.5 + _Tweak_SystemShadowsLevel
    : 0.0001;
```

**ポイント**:
- `attenuation`: Unity のライトアッテネーション（減衰）
- 最小値 `0.0001` で除算エラー防止
- `_Tweak_SystemShadowsLevel`: 手動調整パラメータ

#### 3. **Base → 1st Shade 境界計算** (Step + Feather)

```hlsl
// Line 251: 1段階目シャドウマスク
float Set_FinalShadowMask = saturate(
    (1.0 + (
        (lerp(_HalfLambert_var, _HalfLambert_var * saturate(_SystemShadowsLevel_var), _Set_SystemShadowsToBase)
         - (_BaseColor_Step - _BaseShade_Feather))
        * ((1.0 - _Set_1st_ShadePosition_var.rgb).r - 1.0)
    )) / (_BaseColor_Step - (_BaseColor_Step - _BaseShade_Feather))
);
```

**ポイント**:
- **`_BaseColor_Step`**: Base色の閾値 (0-1)
- **`_BaseShade_Feather`**: フェザリング幅（グラデーション）
- **`_Set_1st_ShadePosition_var`**: シャドウ位置マスクテクスチャ（白=影なし、黒=強制影）
- **計算式の意味**: `smoothstep` 的な補間（閾値±フェザリング幅でグラデーション）

#### 4. **1st Shade → 2nd Shade 境界計算**

```hlsl
// Line 254: 2段階目シャドウ計算（内部）
saturate(
    (1.0 + (
        (_HalfLambert_var - (_ShadeColor_Step - _1st2nd_Shades_Feather))
        * ((1.0 - _Set_2nd_ShadePosition_var.rgb).r - 1.0)
    )) / (_ShadeColor_Step - (_ShadeColor_Step - _1st2nd_Shades_Feather))
)
```

**ポイント**:
- `_ShadeColor_Step`: 2nd Shade の閾値
- `_1st2nd_Shades_Feather`: 1st→2nd のフェザリング幅
- `_Set_2nd_ShadePosition_var`: 2段階目シャドウ位置マスク

#### 5. **最終カラー合成**

```hlsl
// Line 254: 3色のブレンド
float3 Set_FinalBaseColor = lerp(
    Set_BaseColor,  // Base色
    lerp(
        Set_1st_ShadeColor,  // 1st Shade色
        Set_2nd_ShadeColor,  // 2nd Shade色
        [2nd Shade境界マスク]
    ),
    Set_FinalShadowMask  // 1st Shade境界マスク
);
```

**結果**: Base → 1st Shade → 2nd Shade の滑らかな3段階シェーディング

---

### カラー定義とライトカラー反映

```hlsl
// Line 238-244: ライトカラーの適用
float3 Set_LightColor = lightColor.rgb;

float3 Set_BaseColor = lerp(
    (_BaseColor.rgb * _MainTex_var.rgb),  // ライトカラーなし
    ((_BaseColor.rgb * _MainTex_var.rgb) * Set_LightColor),  // ライトカラーあり
    _Is_LightColor_Base  // 切り替えフラグ
);

float3 Set_1st_ShadeColor = lerp(
    (_1st_ShadeColor.rgb * _1st_ShadeMap_var.rgb),
    ((_1st_ShadeColor.rgb * _1st_ShadeMap_var.rgb) * Set_LightColor),
    _Is_LightColor_1st_Shade
);

float3 Set_2nd_ShadeColor = lerp(
    (_2nd_ShadeColor.rgb * _2nd_ShadeMap_var.rgb),
    ((_2nd_ShadeColor.rgb * _2nd_ShadeMap_var.rgb) * Set_LightColor),
    _Is_LightColor_2nd_Shade
);
```

**ポイント**:
- 各シェーディング段階で個別にライトカラー反映ON/OFF可能
- アンリット風（_Is_LightColor_Base = 0）とライティング対応を切り替え可能

---

### リムライト計算

```hlsl
// Line 264-270: リムライト
float _RimArea_var = (1.0 - dot(lerp(i.normalDir, normalDirection, _Is_NormalMapToRimLight), viewDirection));
float _RimLightPower_var = pow(_RimArea_var, exp2(lerp(3, 0, _RimLight_Power)));

// Antipodean RimLight（対向リムライト）
float _ApRimLightPower_var = pow(_RimArea_var, exp2(lerp(3, 0, _Ap_RimLight_Power)));

float3 Set_RimLight = (saturate(_Set_RimLightMask_var.g + _Tweak_RimLightMaskLevel))
    * [ライト方向マスク適用]
    + [Antipodean RimLight追加];
```

**ポイント**:
- `1.0 - dot(N, V)`: フレネル効果（視線と法線の角度）
- `exp2(lerp(3, 0, _RimLight_Power))`: 指数的なパワー調整（3→8倍、0→1倍）
- **Antipodean RimLight**: 逆光側にも別色のリムライトを追加可能

---

### マットキャップ実装

```hlsl
// Line 279-315: MatCap UV計算（カメラロール補正付き）

// カメラロール角度検出
float3 _Camera_Right = UNITY_MATRIX_V[0].xyz;
float3 _Camera_Front = UNITY_MATRIX_V[2].xyz;
float3 _Right_Axis = cross(_Camera_Front, float3(0, 1, 0));
float _Camera_Roll = acos(clamp(dot(_Right_Axis, _Camera_Right) / (length(_Right_Axis) * length(_Camera_Right)), -1, 1));

// MatCap UV計算（ビュー空間法線）
float3 viewNormal = (mul(UNITY_MATRIX_V, float4(normalDirection, 0))).rgb;
float2 _ViewNormalAsMatCapUV = (viewNormal.rg * 0.5) + 0.5;

// UV回転（カメラロール補正 + 手動回転）
float2 _Rot_MatCapUV_var = RotateUV(_ViewNormalAsMatCapUV, _Rot_MatCapUV_var_ang, float2(0.5, 0.5), 1.0);

// 鏡の中ならUV左右反転
if (_sign_Mirror < 0) {
    _Rot_MatCapUV_var.x = 1 - _Rot_MatCapUV_var.x;
}

// LOD対応サンプリング
float4 _MatCap_Sampler_var = tex2Dlod(_MatCap_Sampler, float4(TRANSFORM_TEX(_Rot_MatCapUV_var, _MatCap_Sampler), 0.0, _BlurLevelMatcap));
```

**ポイント**:
- **カメラロール補正**: カメラが傾いてもMatCapが回転しない
- **鏡対応**: VRChat等の鏡の中で正しく表示
- **LOD**: `_BlurLevelMatcap` でミップマップレベル指定（遠距離でぼかす）

---

## アウトライン実装

**UCTS_Outline.cginc 解析**

### 頂点シェーダー: 法線方向押し出し

```hlsl
// Line 73: 距離ベース幅調整
float Set_Outline_Width = (
    _Outline_Width * 0.001
    * smoothstep(_Farthest_Distance, _Nearest_Distance, distance(objPos.rgb, _WorldSpaceCameraPos))
    * _Outline_Sampler_var.rgb
).r;

// Line 85-92: 2つのアウトライン方式
#ifdef _OUTLINE_NML
    // 方式1: 法線方向押し出し（一般的）
    o.pos = UnityObjectToClipPos(lerp(
        float4(v.vertex.xyz + v.normal * Set_Outline_Width, 1),  // 頂点法線使用
        float4(v.vertex.xyz + _BakedNormalDir * Set_Outline_Width, 1),  // Baked法線使用
        _Is_BakedNormal
    ));
#elif _OUTLINE_POS
    // 方式2: 頂点位置方向押し出し（球体に強い）
    Set_Outline_Width = Set_Outline_Width * 2;
    float signVar = dot(normalize(v.vertex), normalize(v.normal)) < 0 ? -1 : 1;
    o.pos = UnityObjectToClipPos(float4(v.vertex.xyz + signVar * normalize(v.vertex) * Set_Outline_Width, 1));
#endif

// Line 94: Z-Offset（カメラ距離補正）
o.pos.z = o.pos.z + _Offset_Z * _ClipCameraPos.z;
```

**ポイント**:
- **距離フェード**: `smoothstep(_Farthest_Distance, _Nearest_Distance, ...)` で遠くなるとアウトライン細くなる
- **`_Outline_Sampler`**: テクスチャでアウトライン幅を部分的に調整可能（髪の毛は太く、顔は細く等）
- **Baked Normal**: ハードエッジで法線が分裂している箇所でもスムーズなアウトライン
- **Z-Offset**: アウトラインがモデルに埋もれないようにカメラ方向にオフセット

---

## モバイル最適化テクニック

### 1. **精度管理**

```hlsl
// 頻繁に使う変数
uniform fixed _Is_LightColor_Base;  // fixed: 1/256精度（フラグ向け）
uniform float _BaseColor_Step;      // float: 高精度（計算向け）
uniform half4 _LightColor0;         // half: 中精度（カラー向け）
```

**方針**:
- **fixed**: フラグ、マスク値（モバイルで最速）
- **half**: カラー、UV、一般的な計算（モバイル推奨）
- **float**: 高精度が必要な計算のみ（座標変換等）

### 2. **条件分岐の最小化**

```hlsl
// ❌ Bad: 動的分岐
if (_Is_LightColor_Base) {
    color = baseColor * lightColor;
} else {
    color = baseColor;
}

// ✅ Good: lerp使用（GPUフレンドリー）
color = lerp(baseColor, baseColor * lightColor, _Is_LightColor_Base);
```

### 3. **テクスチャサンプリング最適化**

```hlsl
// Line 240-243: テクスチャ再利用
float4 _1st_ShadeMap_var = lerp(
    tex2D(_1st_ShadeMap, TRANSFORM_TEX(Set_UV0, _1st_ShadeMap)),
    _MainTex_var,  // 既にサンプリング済みのテクスチャを再利用
    _Use_BaseAs1st  // フラグで切り替え
);
```

**ポイント**: 同じテクスチャを複数回サンプリングしない

### 4. **マルチコンパイルでの機能分離**

```hlsl
#ifdef _EMISSIVE_SIMPLE
    // シンプル版: 計算少ない
    emissive = _Emissive_Tex_var.rgb * _Emissive_Color.rgb * emissiveMask;
#elif _EMISSIVE_ANIMATION
    // アニメーション版: 計算多い（UV回転、スクロール等）
    [複雑な計算...]
#endif
```

**ポイント**: 不要な機能はコンパイル時に除外

### 5. **Vertex Shader軽量化**

```hlsl
// VertexOutput構造体: 最小限のデータ転送
struct VertexOutput {
    float4 pos : SV_POSITION;
    float2 uv0 : TEXCOORD0;
    float4 posWorld : TEXCOORD1;     // ワールド座標（ライティング用）
    float3 normalDir : TEXCOORD2;    // 法線（正規化済み）
    float3 tangentDir : TEXCOORD3;   // タンジェント
    float3 bitangentDir : TEXCOORD4; // バイタンジェント
    float mirrorFlag : TEXCOORD5;    // VRChat鏡対応
    LIGHTING_COORDS(6,7)             // ライトマップ/シャドウ
    UNITY_FOG_COORDS(8)              // フォグ
};
```

**ポイント**: 必要最小限のデータのみ頂点→フラグメント間で転送

---

## Shader Graphへの移植ポイント

### Phase 1: 基本セルシェーディング（Week 1目標）

#### SG_Character_Base.shadergraph に実装すべき機能

**必須機能**:
1. **Half-Lambert計算**
   - Custom Function: `float HalfLambert(float3 Normal, float3 LightDir)`
   - 戻り値: `0.5 * dot(Normal, LightDir) + 0.5`

2. **2段階シャドウ（Base → 1st Shade）**
   - Propertyノード: `BaseColor_Step` (Range 0-1, default 0.5)
   - Propertyノード: `BaseShade_Feather` (Range 0-1, default 0.05)
   - Smoothstepノード使用: `smoothstep(Step - Feather, Step, HalfLambert)`

3. **カラー合成**
   - Lerpノード: `lerp(BaseColor, ShadeColor, ShadowMask)`

**プレースホルダーCustom Function**:
```hlsl
// ToonLighting.hlsl
void CalculateToonShading_float(
    float3 Normal,
    float3 LightDirection,
    float BaseStep,
    float Feather,
    out float ShadowMask
) {
    float halfLambert = 0.5 * dot(Normal, LightDirection) + 0.5;
    ShadowMask = smoothstep(BaseStep - Feather, BaseStep, halfLambert);
}
```

#### SG_Character_Hair.shadergraph に追加すべき機能

**プレースホルダー**:
```hlsl
// AnisotropicHighlight.hlsl
void CalculateAnisotropicHighlight_float(
    float3 Tangent,
    float3 ViewDirection,
    float3 LightDirection,
    float Shift,
    float Exponent,
    out float Highlight
) {
    // Week 4で実装予定
    Highlight = 0.0;
}
```

#### SG_Character_Cloth.shadergraph に追加すべき機能

**プレースホルダー**:
```hlsl
// ColorCustomization.hlsl
void ApplyColorMask_float(
    float4 ColorMask,
    float4 BaseColor,
    float4 PatternColor,
    float4 TrimColor,
    float4 AccentColor,
    out float4 FinalColor
) {
    // Week 3で実装予定
    FinalColor = BaseColor;
}
```

---

### モバイル最適化設定（Shader Graph）

**Graph Settings で設定**:
```
Target: URP
Precision: Half (モバイル最適化)
Workflow Mode: Metallic
Surface Type: Opaque
Render Face: Front (両面描画不要)
Alpha Clipping: Off（Clipping版は別シェーダー）
```

**Material Properties で公開**:
```
_BaseColor: Color (default: White)
_ShadeColor: Color (default: Gray)
_BaseColor_Step: Float Range(0, 1) default: 0.5
_BaseShade_Feather: Float Range(0, 0.5) default: 0.05
_OutlineWidth: Float Range(0, 0.01) default: 0.003
_QualityLevel: Enum (High, Low) default: High  // メインゲーム/ミニゲーム切り替え
```

---

### アウトライン実装方法（Shader Graph）

**方法1: URP Renderer Feature使用**
- Custom Render Pass でアウトライン専用パス追加
- Shader Graph側はアウトラインなし版を作成

**方法2: マルチパスシェーダー**
- Shader Graph では1パス目（メインシェーディング）のみ作成
- アウトラインは別途 .shader ファイルで実装してマージ

**推奨**: 方法2（Week 2で対応）

---

## まとめ

### Unity-Chan Toon Shaderの核心技術

1. **2段階セルシェーディング**: Half-Lambert + Step + Feather
2. **柔軟なカスタマイズ**: シャドウ位置マスク、ライトカラー反映ON/OFF
3. **リッチなエフェクト**: リムライト、マットキャップ、ハイカラー
4. **モバイル対応**: 精度管理、マルチコンパイル、条件分岐最小化
5. **高品質アウトライン**: 距離フェード、Baked Normal対応

### Week 1実装スコープ

**実装する**:
- ✅ 基本Half-Lambert
- ✅ 2トーンセルシェーディング（Base → 1st Shade）
- ✅ Step + Feather制御
- ✅ モバイル最適化設定

**Week 2以降に延期**:
- ⏸️ 3トーン（2nd Shade追加）
- ⏸️ リムライト
- ⏸️ マットキャップ
- ⏸️ アウトライン
- ⏸️ エミッシブ

---

**作成者**: Claude Code
**最終更新**: 2026-02-22
