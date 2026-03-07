using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

#nullable enable

namespace ShaderOp.Editor
{
    /// <summary>
    /// CI/CD用のビルドスクリプト
    /// </summary>
    /// <remarks>
    /// Jenkins、GitHub Actions等から呼び出されるビルドメソッドを提供します。
    /// コマンドライン引数でビルドパスやオプションを指定できます。
    /// </remarks>
    public static class BuildScript
    {
        private const string DefaultBuildPath = "builds";

        /// <summary>ビルドに含めるシーン一覧</summary>
        private static string[] GetScenes()
        {
            return EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();
        }

        /// <summary>
        /// コマンドライン引数からビルドパスを取得
        /// </summary>
        private static string GetBuildPath(string defaultFileName)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-buildPath" && i + 1 < args.Length)
                {
                    string userPath = args[i + 1];
                    return ValidateBuildPath(userPath);
                }
            }

            return Path.Combine(DefaultBuildPath, defaultFileName);
        }

        /// <summary>
        /// ビルドパスを検証してパストラバーサル攻撃を防止
        /// </summary>
        /// <param name="userPath">ユーザー指定のパス</param>
        /// <returns>検証済みパス</returns>
        /// <exception cref="ArgumentException">不正なパスの場合</exception>
        private static string ValidateBuildPath(string userPath)
        {
            // 空文字チェック
            if (string.IsNullOrWhiteSpace(userPath))
            {
                throw new ArgumentException("[BuildScript] ビルドパスが空です");
            }

            // Null文字チェック
            if (userPath.Contains('\0'))
            {
                throw new ArgumentException("[BuildScript] ビルドパスに不正な文字が含まれています");
            }

            // 絶対パスに変換
            string fullPath = Path.GetFullPath(userPath);

            // プロジェクトルートのパス
            string projectRoot = Path.GetDirectoryName(Application.dataPath) ?? "";
            string normalizedProjectRoot = Path.GetFullPath(projectRoot);

            // プロジェクト内かチェック
            if (!fullPath.StartsWith(normalizedProjectRoot, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    $"[BuildScript] ビルドパスはプロジェクト内である必要があります。\n" +
                    $"指定されたパス: {fullPath}\n" +
                    $"プロジェクトルート: {normalizedProjectRoot}"
                );
            }

            // Assetsディレクトリへの書き込みを禁止
            string assetsPath = Path.GetFullPath(Application.dataPath);
            if (fullPath.StartsWith(assetsPath, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("[BuildScript] Assetsディレクトリへのビルド出力は禁止されています");
            }

            return fullPath;
        }

        /// <summary>
        /// ビルドを実行する共通メソッド
        /// </summary>
        private static void PerformBuild(BuildTarget target, string defaultFileName, BuildOptions options)
        {
            string buildPath = GetBuildPath(defaultFileName);

            // ビルドディレクトリの作成
            string? directory = Path.GetDirectoryName(buildPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var scenes = GetScenes();
            if (scenes.Length == 0)
            {
                Debug.LogError("[BuildScript] ビルドに含めるシーンが見つかりません。Build Settingsでシーンを追加してください。");
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log($"[BuildScript] ビルド開始: {target}");
            Debug.Log($"[BuildScript] シーン数: {scenes.Length}");
            Debug.Log($"[BuildScript] 出力先: {buildPath}");

            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = target,
                options = options
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[BuildScript] ビルド成功: {summary.totalSize} bytes ({summary.totalTime})");
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError($"[BuildScript] ビルド失敗: {summary.result}");

                // エラーメッセージの出力
                foreach (var step in report.steps)
                {
                    foreach (var message in step.messages)
                    {
                        if (message.type == LogType.Error || message.type == LogType.Exception)
                        {
                            Debug.LogError($"[BuildScript] {message.content}");
                        }
                    }
                }

                EditorApplication.Exit(1);
            }
        }

        // ====================================
        // Android ビルド
        // ====================================

        [MenuItem("ShaderOp/Build/Android (Development)")]
        public static void BuildAndroidDev()
        {
            PlayerSettings.Android.bundleVersionCode++;
            PerformBuild(
                BuildTarget.Android,
                "Android/ShaderOp.apk",
                BuildOptions.Development | BuildOptions.AllowDebugging
            );
        }

        public static void BuildAndroid()
        {
            PlayerSettings.Android.bundleVersionCode++;
            PerformBuild(
                BuildTarget.Android,
                "Android/ShaderOp.apk",
                BuildOptions.None
            );
        }

        // ====================================
        // iOS ビルド
        // ====================================

        [MenuItem("ShaderOp/Build/iOS (Development)")]
        public static void BuildiOSDev()
        {
            IncrementIOSBuildNumber();
            PerformBuild(
                BuildTarget.iOS,
                "iOS",
                BuildOptions.Development | BuildOptions.AllowDebugging
            );
        }

        public static void BuildiOS()
        {
            IncrementIOSBuildNumber();
            PerformBuild(
                BuildTarget.iOS,
                "iOS",
                BuildOptions.None
            );
        }

        /// <summary>
        /// iOSビルド番号を安全にインクリメント
        /// </summary>
        /// <remarks>
        /// int.Parse()の例外を防ぎ、不正な値の場合は1から開始します
        /// </remarks>
        private static void IncrementIOSBuildNumber()
        {
            string currentBuildNumber = PlayerSettings.iOS.buildNumber;

            if (int.TryParse(currentBuildNumber, out int buildNumber))
            {
                PlayerSettings.iOS.buildNumber = (buildNumber + 1).ToString();
                Debug.Log($"[BuildScript] iOSビルド番号をインクリメント: {buildNumber} → {buildNumber + 1}");
            }
            else
            {
                // パースに失敗した場合は1から開始
                PlayerSettings.iOS.buildNumber = "1";
                Debug.LogWarning($"[BuildScript] iOSビルド番号が不正な値です: '{currentBuildNumber}'。1にリセットします。");
            }
        }

        // ====================================
        // WebGL ビルド
        // ====================================

        [MenuItem("ShaderOp/Build/WebGL (Development)")]
        public static void BuildWebGLDev()
        {
            PerformBuild(
                BuildTarget.WebGL,
                "WebGL",
                BuildOptions.Development | BuildOptions.AllowDebugging
            );
        }

        public static void BuildWebGL()
        {
            PerformBuild(
                BuildTarget.WebGL,
                "WebGL",
                BuildOptions.None
            );
        }

        // ====================================
        // Windows ビルド
        // ====================================

        [MenuItem("ShaderOp/Build/Windows (Development)")]
        public static void BuildWindowsDev()
        {
            PerformBuild(
                BuildTarget.StandaloneWindows64,
                "Windows/ShaderOp.exe",
                BuildOptions.Development | BuildOptions.AllowDebugging
            );
        }

        public static void BuildWindows()
        {
            PerformBuild(
                BuildTarget.StandaloneWindows64,
                "Windows/ShaderOp.exe",
                BuildOptions.None
            );
        }

        // ====================================
        // Linux ビルド
        // ====================================

        [MenuItem("ShaderOp/Build/Linux (Development)")]
        public static void BuildLinuxDev()
        {
            PerformBuild(
                BuildTarget.StandaloneLinux64,
                "Linux/ShaderOp.x86_64",
                BuildOptions.Development | BuildOptions.AllowDebugging
            );
        }

        public static void BuildLinux()
        {
            PerformBuild(
                BuildTarget.StandaloneLinux64,
                "Linux/ShaderOp.x86_64",
                BuildOptions.None
            );
        }

        // ====================================
        // 全プラットフォームビルド
        // ====================================

        [MenuItem("ShaderOp/Build/All Platforms")]
        public static void BuildAllPlatforms()
        {
            Debug.Log("[BuildScript] 全プラットフォームビルド開始");

            BuildAndroid();
            BuildWebGL();
            BuildWindows();
            BuildLinux();

            Debug.Log("[BuildScript] 全プラットフォームビルド完了");
        }

        // ====================================
        // ユーティリティメソッド
        // ====================================

        /// <summary>
        /// ビルドバージョンを設定
        /// </summary>
        /// <remarks>
        /// コマンドライン引数 -version で指定されたバージョンを適用します
        /// 例: -version 1.0.5
        /// </remarks>
        [MenuItem("ShaderOp/Build/Set Version From Args")]
        public static void SetVersionFromArgs()
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-version" && i + 1 < args.Length)
                {
                    string version = args[i + 1];
                    PlayerSettings.bundleVersion = version;
                    Debug.Log($"[BuildScript] バージョン設定: {version}");
                    return;
                }
            }

            Debug.LogWarning("[BuildScript] -version 引数が見つかりません");
        }

        /// <summary>
        /// ビルド設定の情報を出力
        /// </summary>
        [MenuItem("ShaderOp/Build/Print Build Info")]
        public static void PrintBuildInfo()
        {
            Debug.Log("=== Build Information ===");
            Debug.Log($"Product Name: {PlayerSettings.productName}");
            Debug.Log($"Bundle Version: {PlayerSettings.bundleVersion}");
            Debug.Log($"Android Bundle Version Code: {PlayerSettings.Android.bundleVersionCode}");
            Debug.Log($"iOS Build Number: {PlayerSettings.iOS.buildNumber}");
            Debug.Log($"Company Name: {PlayerSettings.companyName}");
            Debug.Log($"Scenes in Build: {GetScenes().Length}");

            foreach (var scene in GetScenes())
            {
                Debug.Log($"  - {scene}");
            }
        }
    }
}
