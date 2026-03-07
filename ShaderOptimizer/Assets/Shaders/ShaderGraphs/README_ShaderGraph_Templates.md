# Shader Graph テンプレート使用ガイド

## 概要

このディレクトリには、アーティスト向けのShader Graphテンプレートが含まれています。
Shader Graphはビジュアルノードベースのシェーダー作成ツールで、コードを書かずにシェーダーを作成・編集できます。

## Shader Graph テンプレート一覧

### SG_CustomizableToon.shadergraph
**用途**: キャラクターカスタマイズ用トゥーンシェーダー
**特徴**:
- カラーマスクベースの色変更（R/G/B チャンネル）
- 2段階トゥーンシェーディング
- リムライト
- シャドウフェザリング調整

**推奨用途**: キャラクター、衣装、装備品

### SG_EnvironmentToon.shadergraph（作成推奨）
**用途**: 環境オブジェクト用シンプルトゥーンシェーダー
**特徴**:
- 軽量な2段階シェーディング
- オプショナルなカラーマスク
- バッチング最適化

**推奨用途**: 背景、小道具、建築物

## Shader Graph での実装方法

### 1. 基本的なトゥーンシェーディングの作成

#### 必要なノード:
1. **Dot Product** - 法線とライト方向の内積計算
2. **Remap** - Half-Lambert変換（-1~1 → 0~1）
3. **Step** - シャドウ境界の作成
4. **Smoothstep** - フェザリング（柔らかい境界）
5. **Lerp** - ベースカラーとシャドウカラーのブレンド

#### ノード接続例:
```
Normal Vector → Dot Product ← Main Light Direction
                     ↓
                  Remap (0.5 * x + 0.5)
                     ↓
              Smoothstep (ShadowStep - Feather, ShadowStep, x)
                     ↓
               Lerp (ShadeColor, BaseColor, mask)
```

### 2. カラーマスクシステムの実装

#### 必要なノード:
1. **Sample Texture 2D** - カラーマスクテクスチャの読み込み
2. **Split** - RGBAチャンネルの分離
3. **Multiply** - 各チャンネルに対応する色を乗算
4. **Add** - すべての色を合成
5. **Lerp** - マスク強度による合成

#### ノード接続例:
```
ColorMaskTexture → Sample Texture 2D → Split
                                        ↓
                        R → Multiply ← PrimaryColor
                        G → Multiply ← SecondaryColor
                        B → Multiply ← AccentColor
                                        ↓
                                    Add (全チャンネル合計)
                                        ↓
                                Lerp (BaseColor, MaskedColor, TotalMask)
```

### 3. リムライトの実装

#### 必要なノード:
1. **Fresnel Effect** - ビューと法線の関係計算
2. **Power** - リムの鋭さ調整
3. **Multiply** - リムカラーと強度の適用
4. **Add** - 最終カラーに加算

#### ノード接続例:
```
Normal Vector → Fresnel Effect ← View Direction
                     ↓
                Power (RimPower)
                     ↓
              Multiply ← RimColor
                     ↓
              Multiply ← RimIntensity
                     ↓
            Add (FinalColor + RimContribution)
```

## Shader Graph での最適化テクニック

### モバイル最適化
1. **Precision設定**: グラフ全体を「Half」精度に設定
   - Graph Inspector → Precision → Half
2. **不要なパスの削除**: Shadow Caster、Depth Onlyは必要に応じて有効化
3. **テクスチャサンプリングの最小化**: 同じテクスチャを複数回サンプリングしない

### バッチング対応
1. **GPU Instancing有効化**: Graph Settings → Support GPU Instancing
2. **SRP Batcher互換性**: すべてのプロパティをCBufferに配置（自動）

## HLSLシェーダーとの使い分け

### Shader Graphを使うべき場合:
- アーティストが頻繁に調整する必要がある
- ビジュアルフィードバックが重要
- プロトタイピング段階
- 複雑な計算が不要

### HLSLコードを使うべき場合:
- 高度な数学計算が必要（Kajiya-Kay、SSSなど）
- パフォーマンスが最重要
- マルチパスレンダリング
- カスタムバリアント制御

## Shader Graph テンプレートの作成手順

### SG_CustomizableToon の再作成方法:

1. **新しいShader Graph作成**
   - Project ウィンドウで右クリック
   - Create → Shader Graph → URP → Lit Shader Graph
   - 名前を `SG_CustomizableToon` に変更

2. **プロパティの追加**
   - Blackboard で「+」をクリック
   - 以下のプロパティを追加:
     - `Base Color` (Color)
     - `Main Texture` (Texture2D)
     - `Color Mask` (Texture2D)
     - `Primary Color` (Color)
     - `Secondary Color` (Color)
     - `Accent Color` (Color)
     - `Shade Color` (Color)
     - `Shadow Step` (Float, Range 0-1)
     - `Shadow Feather` (Float, Range 0-0.5)
     - `Rim Color` (Color)
     - `Rim Power` (Float, Range 0.5-8)
     - `Rim Intensity` (Float, Range 0-1)

3. **ノードグラフの構築**
   - 上記の「実装方法」セクションに従ってノードを接続
   - Main Preview でリアルタイムプレビュー確認

4. **出力への接続**
   - 最終カラーを `Base Color` に接続
   - 必要に応じて `Smoothness`, `Normal` も設定

5. **グラフの保存**
   - Save Asset でShader Graphを保存

### SG_EnvironmentToon の作成方法:

SG_CustomizableToonと同様の手順で作成しますが、以下の点を簡略化:
- カラーマスクはオプション（Boolean プロパティで制御）
- リムライトもオプション
- より軽量な2段階シェーディングのみ

## C# からのマテリアル制御

Shader Graphで作成したシェーダーも、HLSLシェーダーと同じ方法でC#から制御できます:

```csharp
using UnityEngine;

public class ShaderGraphMaterialController : MonoBehaviour
{
    [SerializeField] private Material _material;

    /// <summary>
    /// カラーマスクの各チャンネル色を設定
    /// </summary>
    public void SetCustomColors(Color primary, Color secondary, Color accent)
    {
        _material.SetColor("_PrimaryColor", primary);
        _material.SetColor("_SecondaryColor", secondary);
        _material.SetColor("_AccentColor", accent);
    }

    /// <summary>
    /// シャドウ設定を調整
    /// </summary>
    public void SetShadowSettings(float step, float feather)
    {
        _material.SetFloat("_ShadowStep", step);
        _material.SetFloat("_ShadowFeather", feather);
    }

    /// <summary>
    /// リムライト設定
    /// </summary>
    public void SetRimLight(Color color, float power, float intensity)
    {
        _material.SetColor("_RimColor", color);
        _material.SetFloat("_RimPower", power);
        _material.SetFloat("_RimIntensity", intensity);
    }
}
```

## サブグラフの作成（再利用可能なノードグループ）

頻繁に使う計算はサブグラフとして保存できます:

### SubGraph_ToonShading.shadersubgraph
**入力**:
- Normal (Vector3)
- Light Direction (Vector3)
- Shadow Step (Float)
- Shadow Feather (Float)

**出力**:
- Shadow Mask (Float)

### SubGraph_ColorMask.shadersubgraph
**入力**:
- Color Mask Texture (Texture2D)
- UV (Vector2)
- Primary/Secondary/Accent Colors (Color x3)
- Base Color (Vector3)

**出力**:
- Masked Color (Vector3)

## トラブルシューティング

### シェーダーが正しく表示されない
- Graph Inspector → Precision を確認（Half推奨）
- Preview Mode を確認（Material推奨）
- エラーがないか Shader Graph ウィンドウ下部を確認

### パフォーマンスが悪い
- ノード数を削減（複雑な計算は1つのカスタム関数ノードに）
- テクスチャサンプリング回数を最小化
- Precision を Half に変更

### マテリアルプロパティが見つからない
- プロパティのReference名（アンダースコア付き）を確認
- Exposed トグルが有効か確認
- Shader Graph を再保存・再インポート

## 参考リソース

- Unity Shader Graph Documentation: https://docs.unity3d.com/Packages/com.unity.shadergraph@latest
- URP Shader Graph サンプル: https://github.com/Unity-Technologies/ShaderGraph_ExampleLibrary
- Shader Graph カスタム関数: https://docs.unity3d.com/Packages/com.unity.shadergraph@latest/manual/Custom-Function-Node.html

---
最終更新: 2026-02-26
