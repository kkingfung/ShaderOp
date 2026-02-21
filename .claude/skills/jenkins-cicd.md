# Jenkins CI/CD パターン

## 概要
UnityプロジェクトのJenkinsパイプライン構築パターン。

## Jenkinsfile基本構造

```groovy
pipeline {
    agent any

    environment {
        UNITY_PATH = 'C:\\Program Files\\Unity\\Hub\\Editor\\2022.3.10f1\\Editor\\Unity.exe'
        PROJECT_PATH = "${WORKSPACE}"
        BUILD_OUTPUT = "${WORKSPACE}\\Builds"
    }

    stages {
        stage('アセット検証') {
            steps {
                script {
                    echo '=== アセット検証開始 ==='
                    bat """
                        python Scripts/Automation/validate_assets.py --path Assets
                    """
                }
            }
        }

        stage('Unityビルド') {
            steps {
                script {
                    echo '=== Unityビルド開始 ==='
                    bat """
                        "${UNITY_PATH}" -quit -batchmode -projectPath "${PROJECT_PATH}" ^
                        -buildTarget Android ^
                        -executeMethod BuildScript.BuildAndroid ^
                        -logFile build.log
                    """
                }
            }
        }

        stage('デプロイ') {
            steps {
                script {
                    echo '=== デプロイ開始 ==='
                    // Firebase App Distributionへアップロード
                    bat """
                        firebase appdistribution:distribute "${BUILD_OUTPUT}\\app.apk" ^
                        --app 1:1234567890:android:abcdef ^
                        --groups testers
                    """
                }
            }
        }
    }

    post {
        success {
            echo 'ビルド成功！'
        }
        failure {
            echo 'ビルド失敗'
        }
    }
}
```

## Unity Editor スクリプト（ビルド自動化）

```csharp
using UnityEditor;
using UnityEngine;
using System.IO;

/// <summary>
/// Jenkins連携用ビルドスクリプト
/// </summary>
public static class BuildScript
{
    /// <summary>ビルド出力先</summary>
    private static readonly string BuildPath = Path.Combine(Directory.GetCurrentDirectory(), "Builds");

    /// <summary>
    /// Android向けビルド実行
    /// </summary>
    [MenuItem("Build/Build Android")]
    public static void BuildAndroid()
    {
        // ビルド設定
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = GetScenePaths(),
            locationPathName = Path.Combine(BuildPath, "app.apk"),
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        // ビルド実行
        var report = BuildPipeline.BuildPlayer(buildOptions);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"ビルド成功: {report.summary.totalSize} bytes");
            EditorApplication.Exit(0); // 成功
        }
        else
        {
            Debug.LogError($"ビルド失敗: {report.summary.result}");
            EditorApplication.Exit(1); // 失敗
        }
    }

    private static string[] GetScenePaths()
    {
        return new[]
        {
            "Assets/Scenes/MainScene.unity"
        };
    }
}
```

## Python自動検証スクリプト

```python
import sys
from pathlib import Path
from typing import List, Dict

def validate_assets(asset_path: str) -> Dict[str, List[str]]:
    """
    アセットを検証してエラーを返す

    Args:
        asset_path: アセットフォルダパス

    Returns:
        検証結果（エラーリスト）
    """
    errors = {'naming': [], 'size': [], 'format': []}

    asset_dir = Path(asset_path)

    # テクスチャ検証
    for texture in asset_dir.glob('**/*.png'):
        # 命名規則チェック（例: tex_character_001.png）
        if not texture.stem.startswith('tex_'):
            errors['naming'].append(f"命名規則違反: {texture}")

        # サイズチェック（例: 10MB以上は警告）
        if texture.stat().st_size > 10 * 1024 * 1024:
            errors['size'].append(f"サイズ超過: {texture} ({texture.stat().st_size / 1024 / 1024:.2f}MB)")

    return errors

if __name__ == '__main__':
    import argparse

    parser = argparse.ArgumentParser()
    parser.add_argument('--path', required=True, help='アセットフォルダパス')
    args = parser.parse_args()

    results = validate_assets(args.path)

    # エラー出力
    has_errors = False
    for category, error_list in results.items():
        if error_list:
            has_errors = True
            print(f"\n[{category}エラー]")
            for error in error_list:
                print(f"  - {error}")

    if has_errors:
        sys.exit(1)  # エラーコード1で終了（Jenkinsがビルド失敗と判断）
    else:
        print("\n✅ すべてのアセット検証に合格しました")
        sys.exit(0)
```

## ワンクリックデプロイ

```python
from pathlib import Path
import subprocess

def deploy_to_device(apk_path: str, device_id: str) -> bool:
    """
    指定デバイスへAPKをインストール

    Args:
        apk_path: APKファイルパス
        device_id: デバイスID（adb devicesで確認）

    Returns:
        成功フラグ
    """
    try:
        # ADB経由でインストール
        result = subprocess.run(
            ['adb', '-s', device_id, 'install', '-r', apk_path],
            capture_output=True,
            text=True
        )

        if result.returncode == 0:
            print(f"✅ デプロイ成功: {device_id}")
            return True
        else:
            print(f"❌ デプロイ失敗: {result.stderr}")
            return False
    except Exception as e:
        print(f"❌ エラー: {e}")
        return False
```

## ベストプラクティス

✅ **DO**:
- ビルド前に必ずアセット検証を実行
- ビルドログを保存（`-logFile`）
- エラー時は適切なexit codeを返す（0=成功, 1=失敗）
- Pythonスクリプトで自動化を推進

❌ **DON'T**:
- 手動ビルドに依存
- エラーハンドリングの欠如
- ログを残さない
- 検証をスキップ

信頼度: 0.88
