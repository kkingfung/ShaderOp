# Unity Shader開発スキル

## 概要
Unity Shader GraphとHLSLを使用した質感表現の開発パターン。

## 基本ワークフロー

1. **Shader Graphでプロトタイプ作成**
2. **カスタムノード実装**（必要に応じて）
3. **HLSL最適化**
4. **モバイル対応チェック**

## Shader Graph ベストプラクティス

```
✅ DO:
- サブグラフで再利用可能な部分を抽出
- プロパティに分かりやすい名前をつける
- Preview を活用して視覚的に確認

❌ DON'T:
- 過度な計算をフラグメントシェーダーで実行
- 不要なテクスチャサンプリング
- マジックナンバーの使用
```

## HLSL パフォーマンス最適化

```hlsl
// ✅ Good: テクスチャサンプリングを最小化
half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

// ❌ Bad: 複数回サンプリング
half4 color1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
half4 color2 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + 0.01);
```

## モバイル対応

- half/fixed精度を使用（floatの代わりに）
- テクスチャ圧縮を活用（ASTC、ETC2）
- Draw Call削減のためマテリアルを統合

## 検証

- [ ] Frame Debuggerでテクスチャサンプリング回数を確認
- [ ] Profilerで処理時間を計測
- [ ] 実機テスト（Android/iOS）

信頼度: 0.95
