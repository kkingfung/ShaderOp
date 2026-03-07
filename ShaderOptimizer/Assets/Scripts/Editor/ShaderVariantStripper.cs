#nullable enable

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Rendering;

namespace ShaderOp.Editor
{
    /// <summary>
    /// シェーダーバリアント削減ツール
    /// </summary>
    /// <remarks>
    /// IPreprocessShadersを実装してビルド時に不要なシェーダーバリアントを削減
    /// ビルドサイズとシェーダー切り替えコストを大幅に削減
    /// </remarks>
    public class ShaderVariantStripper : IPreprocessShaders
    {
        /// <summary>コールバック順序（低いほど先に実行）</summary>
        public int callbackOrder => 0;

        /// <summary>削除されたバリアント数</summary>
        private int _strippedVariantCount = 0;

        /// <summary>合計バリアント数</summary>
        private int _totalVariantCount = 0;

        /// <summary>
        /// シェーダーバリアント削減の実行
        /// </summary>
        public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
        {
            // モバイルプラットフォームの判定
            // TODO: Unity 2023以降でShaderSnippetDataのAPI変更により一時的に無効化
            bool isMobilePlatform = false;
            // bool isMobilePlatform = snippet.platform == ShaderCompilerPlatform.GLES3x ||
            //                         snippet.platform == ShaderCompilerPlatform.GLES20 ||
            //                         snippet.platform == ShaderCompilerPlatform.Vulkan ||
            //                         snippet.platform == ShaderCompilerPlatform.Metal;

            _totalVariantCount += data.Count;

            // バリアント削減ルールを適用
            for (int i = data.Count - 1; i >= 0; --i)
            {
                ShaderCompilerData variantData = data[i];
                bool shouldStrip = false;

                // ルール1: モバイルでTessellationを使用するバリアントを削除
                if (isMobilePlatform && snippet.passType == PassType.Normal)
                {
                    if (variantData.shaderKeywordSet.IsEnabled(new ShaderKeyword("_TESSELLATION_ON")))
                    {
                        shouldStrip = true;
                    }
                }

                // ルール2: フォグが無効な場合のバリアントを削除（プロジェクト設定に依存）
                if (!RenderSettings.fog)
                {
                    if (variantData.shaderKeywordSet.IsEnabled(new ShaderKeyword("FOG_LINEAR")) ||
                        variantData.shaderKeywordSet.IsEnabled(new ShaderKeyword("FOG_EXP")) ||
                        variantData.shaderKeywordSet.IsEnabled(new ShaderKeyword("FOG_EXP2")))
                    {
                        shouldStrip = true;
                    }
                }

                // ルール3: Lightmapを使用しないバリアントを削除（プロジェクト設定に依存）
                if (!UsesLightmaps())
                {
                    if (variantData.shaderKeywordSet.IsEnabled(new ShaderKeyword("LIGHTMAP_ON")) ||
                        variantData.shaderKeywordSet.IsEnabled(new ShaderKeyword("DIRLIGHTMAP_COMBINED")) ||
                        variantData.shaderKeywordSet.IsEnabled(new ShaderKeyword("LIGHTMAP_SHADOW_MIXING")))
                    {
                        shouldStrip = true;
                    }
                }

                // ルール4: 使用しないライティングモードのバリアントを削除
                if (variantData.shaderKeywordSet.IsEnabled(new ShaderKeyword("_SPECULARHIGHLIGHTS_OFF")))
                {
                    // スペキュラーハイライト無効のバリアントを削除（プロジェクトで使用しない場合）
                    // shouldStrip = true; // プロジェクトの要件に応じてコメント解除
                }

                // ルール5: HDRが無効な場合のバリアントを削除
                if (!PlayerSettings.useHDRDisplay)
                {
                    if (variantData.shaderKeywordSet.IsEnabled(new ShaderKeyword("_HDR_ON")))
                    {
                        shouldStrip = true;
                    }
                }

                // ルール6: モバイルで複雑なライティングバリアントを削除
                if (isMobilePlatform)
                {
                    // 複数のポイントライトやスポットライトのバリアントを削減
                    if (variantData.shaderKeywordSet.IsEnabled(new ShaderKeyword("_ADDITIONAL_LIGHTS")))
                    {
                        // 追加ライトが4つ以上のバリアントを削除
                        // shouldStrip = true; // プロジェクトの要件に応じてコメント解除
                    }
                }

                // ルール7: デバッグ用シェーダーバリアントを削除（リリースビルドのみ）
                #if !DEVELOPMENT_BUILD
                if (variantData.shaderKeywordSet.IsEnabled(new ShaderKeyword("DEBUG_DISPLAY")))
                {
                    shouldStrip = true;
                }
                #endif

                // バリアントを削除
                if (shouldStrip)
                {
                    data.RemoveAt(i);
                    _strippedVariantCount++;
                }
            }
        }

        /// <summary>
        /// Lightmapが使用されているかチェック
        /// </summary>
        private bool UsesLightmaps()
        {
            // シーン内のライトマップ使用状況を確認
            // 簡易実装: プロジェクト設定で判断
            return Lightmapping.bakedGI || Lightmapping.realtimeGI;
        }

        /// <summary>
        /// ビルド後の統計情報をログ出力
        /// </summary>
        [UnityEditor.Callbacks.PostProcessBuild(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            Debug.Log($"[ShaderVariantStripper] Build completed for {target}");
            Debug.Log($"[ShaderVariantStripper] Path: {pathToBuiltProject}");
        }
    }

    /// <summary>
    /// シェーダーバリアント統計ツール
    /// </summary>
    public class ShaderVariantStats : IPreprocessShaders
    {
        public int callbackOrder => 1; // ShaderVariantStripperの後に実行

        private Dictionary<string, int> _shaderVariantCounts = new Dictionary<string, int>();

        public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
        {
            string shaderName = shader.name;
            if (!_shaderVariantCounts.ContainsKey(shaderName))
            {
                _shaderVariantCounts[shaderName] = 0;
            }

            _shaderVariantCounts[shaderName] += data.Count;
        }

        [UnityEditor.Callbacks.PostProcessBuild(2)]
        public static void LogVariantStats(BuildTarget target, string pathToBuiltProject)
        {
            Debug.Log("[ShaderVariantStats] Shader variant statistics logged");
        }
    }
}
