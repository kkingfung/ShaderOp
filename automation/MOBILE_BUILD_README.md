# ShaderOp モバイルビルド自動化ガイド

**Version**: 1.0
**Date**: 2026-03-15
**Author**: automation-dev agent

---

## 概要

このドキュメントは、ShaderOp プロジェクトのAndroid/iOS モバイルビルドを自動化するための完全ガイドです。

**提供される機能**:
- ✅ Python自動化スクリプト（build_mobile.py）
- ✅ JSON設定ファイルによる柔軟なビルド設定
- ✅ Unity バッチモード実行
- ✅ ビルド成果物の自動検証
- ✅ 詳細なビルドレポート生成（JSON + Markdown）
- ✅ セキュリティ対策（パストラバーサル防止、コマンドインジェクション防止）

---

## クイックスタート

### ステップ1: 設定ファイルを生成

```bash
cd D:\PersonalGameDev\ShaderOp\automation
python build_mobile.py --generate-config build_config.json
```

生成されるファイル: `build_config.json`（編集可能なテンプレート）

### ステップ2: 設定ファイルを編集（任意）

```json
{
  "platform": "Android",
  "buildType": "Development",
  "version": "0.4.0",
  "scriptingBackend": "IL2CPP",
  "architecture": "ARM64",
  ...
}
```

### ステップ3: ビルドを実行

```bash
python build_mobile.py --config build_config.json
```

### ステップ4: 結果を確認

ビルド成果物:
- `builds/Android/ShaderOp_0.4.0_Development_YYYYMMDD.apk`
- `builds/build_report_Android_YYYYMMDD_HHMMSS.json`
- `builds/build_report_Android_YYYYMMDD_HHMMSS.md`

---

## 詳細ガイド

### 1. 必須環境

#### 1.1 Unity Editor

**必須バージョン**: Unity 2021.3 LTS 以上

**インストール方法**:
- Unity Hub経由でインストール
- Android Build Support モジュールを含める
- iOS Build Support（iOS ビルドの場合）

**自動検出パス**:
- Windows: `C:\Program Files\Unity\Hub\Editor\{version}\Editor\Unity.exe`
- macOS: `/Applications/Unity/Unity.app/Contents/MacOS/Unity`

**手動指定**:
```bash
python build_mobile.py --unity-path "C:\Program Files\Unity\Hub\Editor\2022.3.10f1\Editor\Unity.exe"
```

または環境変数:
```bash
set UNITY_PATH=C:\Program Files\Unity\Hub\Editor\2022.3.10f1\Editor\Unity.exe
```

#### 1.2 Android SDK（Android ビルドの場合）

**必須コンポーネント**:
- Android SDK Platform (API Level 29以上)
- Android SDK Build-Tools
- Android NDK
- JDK 11以上

**インストール方法**:
- Unity Hub → Installs → {Version} → Add Modules → Android Build Support
- または Android Studio経由でインストール

**Unity設定**:
1. Unity Editor → Edit → Preferences → External Tools
2. Android SDK/NDK/JDK パスを設定

#### 1.3 Python環境

**Python バージョン**: Python 3.8以上

**必須パッケージ**:
```bash
pip install -r automation/requirements.txt
```

`requirements.txt` の内容:
```
# セキュリティユーティリティの依存関係のみ
# build_mobile.pyは標準ライブラリのみ使用
```

---

### 2. ビルド設定

#### 2.1 設定ファイル構造

**build_config.json**:
```json
{
  "platform": "Android",           // "Android" | "iOS"
  "buildType": "Development",       // "Development" | "Release"
  "version": "0.4.0",               // セマンティックバージョン
  "scriptingBackend": "IL2CPP",     // "Mono" | "IL2CPP"
  "apiLevel": "Android10",          // Android API Level
  "architecture": "ARM64",          // "ARMv7" | "ARM64" | "x86"
  "compressionMethod": "LZ4",       // "None" | "LZ4" | "LZ4HC"
  "managedStripping": "Medium",     // "Disabled" | "Low" | "Medium" | "High"

  "optimizations": {
    "graphicsAPI": "OpenGLES3",     // "OpenGLES2" | "OpenGLES3" | "Vulkan"
    "textureCompression": "ASTC",   // "ETC2" | "ASTC"
    "scriptOptimization": "Speed"   // "Speed" | "Size"
  },

  "scenes": [
    "Assets/Scenes/MainMenu.unity",
    "Assets/Scenes/HexChess.unity"
  ],

  "playerSettings": {
    "companyName": "ShaderOp",
    "productName": "ShaderOp",
    "orientation": "Portrait"
  }
}
```

#### 2.2 プラットフォーム別設定

**Android 推奨設定**:
```json
{
  "platform": "Android",
  "buildType": "Development",
  "scriptingBackend": "IL2CPP",
  "architecture": "ARM64",
  "compressionMethod": "LZ4",
  "managedStripping": "Medium",
  "optimizations": {
    "graphicsAPI": "OpenGLES3",
    "textureCompression": "ASTC"
  }
}
```

**iOS 推奨設定**:
```json
{
  "platform": "iOS",
  "buildType": "Development",
  "scriptingBackend": "IL2CPP",
  "architecture": "ARM64",
  "managedStripping": "Medium",
  "optimizations": {
    "graphicsAPI": "Metal",
    "textureCompression": "ASTC"
  }
}
```

---

### 3. コマンドラインオプション

#### 3.1 基本オプション

```bash
python build_mobile.py [OPTIONS]
```

**オプション一覧**:

| オプション | 説明 | デフォルト値 | 例 |
|-----------|------|-------------|-----|
| `--project` | Unityプロジェクトパス | `ShaderOptimizer` | `--project ShaderOptimizer` |
| `--unity-path` | Unity.exeのパス | 自動検出 | `--unity-path "C:\...\Unity.exe"` |
| `--config` | ビルド設定JSONファイル | - | `--config build_config.json` |
| `--generate-config` | 設定ファイルを生成 | - | `--generate-config config.json` |
| `--dry-run` | ドライラン（実行しない） | false | `--dry-run` |

#### 3.2 使用例

**例1: 設定ファイル生成**
```bash
python build_mobile.py --generate-config android_dev.json
```

**例2: Android Development ビルド**
```bash
python build_mobile.py --config android_dev.json
```

**例3: ドライラン（実行せずに確認）**
```bash
python build_mobile.py --config android_dev.json --dry-run
```

**例4: Unityパスを手動指定**
```bash
python build_mobile.py --config android_dev.json --unity-path "C:\Program Files\Unity\Hub\Editor\2022.3.10f1\Editor\Unity.exe"
```

**例5: iOS Release ビルド**
```bash
python build_mobile.py --config ios_release.json
```

---

### 4. ビルドプロセス詳細

#### 4.1 ビルドフロー

```
1. 設定ファイル読み込み
   ↓
2. 設定検証（プラットフォーム、ビルドタイプ等）
   ↓
3. Unity Editor パス検出
   ↓
4. ビルドコマンド構築
   ↓
5. Unity バッチモード実行
   ↓
6. ビルド成果物の検証
   ↓
7. ビルドレポート生成（JSON + Markdown）
   ↓
8. 完了
```

#### 4.2 Unity バッチモードコマンド

自動生成されるコマンド例:
```bash
Unity.exe \
  -quit \
  -batchmode \
  -projectPath "D:\PersonalGameDev\ShaderOp\ShaderOptimizer" \
  -executeMethod ShaderOp.Editor.BuildScript.BuildAndroidDev \
  -buildPath "D:\PersonalGameDev\ShaderOp\builds\Android\ShaderOp_0.4.0_Development_20260315.apk" \
  -logFile "D:\PersonalGameDev\ShaderOp\builds\build_Android_20260315_143022.log"
```

**パラメータ説明**:
- `-quit`: ビルド完了後にUnityを終了
- `-batchmode`: バッチモード（UIなし）
- `-projectPath`: Unityプロジェクトのパス
- `-executeMethod`: 実行するC#メソッド
- `-buildPath`: ビルド成果物の出力先
- `-logFile`: ビルドログの出力先

#### 4.3 BuildScript.cs メソッド

**Android**:
- `BuildAndroidDev` - Development ビルド
- `BuildAndroid` - Release ビルド

**iOS**:
- `BuildiOSDev` - Development ビルド
- `BuildiOS` - Release ビルド

---

### 5. ビルド成果物

#### 5.1 ファイル構造

```
builds/
├── Android/
│   └── ShaderOp_0.4.0_Development_20260315.apk  (ビルド成果物)
├── build_Android_20260315_143022.log            (ビルドログ)
├── build_report_Android_20260315_143022.json    (JSONレポート)
└── build_report_Android_20260315_143022.md      (Markdownレポート)
```

#### 5.2 ビルドレポート（JSON）

**build_report_Android_YYYYMMDD_HHMMSS.json**:
```json
{
  "report": {
    "status": "success",
    "platform": "Android",
    "buildType": "Development",
    "timestamp": "2026-03-15T14:30:45",
    "buildTime": 456.78,
    "artifact": "D:\\...\\ShaderOp_0.4.0_Development_20260315.apk",
    "sizeMB": 145.23
  },
  "config": {
    "platform": "Android",
    "buildType": "Development",
    ...
  }
}
```

#### 5.3 ビルドレポート（Markdown）

**build_report_Android_YYYYMMDD_HHMMSS.md**:
- ビルド設定サマリー
- ビルド成果物情報（サイズ、パス）
- 警告・エラー（あれば）
- 最適化設定
- インストールテストチェックリスト

---

### 6. 検証とテスト

#### 6.1 APK サイズ検証

**自動チェック**:
- APKサイズが200MBを超えると警告
- Google Play配信制限に基づく推奨値

**APK Analyzer（手動確認）**:
```bash
# Android Studio に付属
$ANDROID_HOME/tools/bin/apkanalyzer apk summary builds/Android/ShaderOp.apk
```

#### 6.2 インストールテスト

**ADB経由でインストール**:
```bash
# デバイス接続確認
adb devices

# APKをデバイスに転送
adb push builds/Android/ShaderOp_0.4.0_Development_20260315.apk /sdcard/Download/

# インストール
adb install -r builds/Android/ShaderOp_0.4.0_Development_20260315.apk

# アプリ起動
adb shell am start -n com.shaderop.mobile/.MainActivity

# ログ確認
adb logcat -s Unity
```

#### 6.3 パフォーマンステスト

**Unity Remote Profiler**:
1. Development Buildでビルド
2. "Autoconnect Profiler" を有効化
3. デバイスをUSB接続
4. Unity Editor → Window → Analysis → Profiler
5. デバイスを選択して "Record" 開始

**測定項目**:
- Frame Time (目標: <16.67ms = 60fps)
- CPU Usage (目標: <50%)
- Memory Usage (目標: <300MB)
- GC Allocation (目標: <100KB/frame)

---

### 7. トラブルシューティング

#### 7.1 Unity Editorが見つからない

**エラー**:
```
❌ エラー: Unity Editorが見つかりません
```

**解決策**:
1. `--unity-path` オプションで手動指定
2. 環境変数 `UNITY_PATH` を設定
3. デフォルトパスにUnityをインストール

#### 7.2 ビルド失敗（エラーコード 1）

**エラー**:
```
❌ ビルド失敗 (終了コード: 1)
```

**解決策**:
1. ビルドログを確認: `builds/build_Android_YYYYMMDD_HHMMSS.log`
2. Unity Editorで手動ビルドしてエラー特定
3. Android SDK/NDKのパス設定を確認
4. Build Settings でシーンが追加されているか確認

#### 7.3 APKサイズが大きすぎる

**警告**:
```
⚠ 警告: APKサイズが200MBを超えています
```

**解決策**:
1. **Managed Stripping Level** を Medium → High に変更
2. **Texture Compression** を ASTC に設定
3. **Shader Stripping** を有効化
4. 不要なアセットを削除
5. Addressables でオンデマンド配信

#### 7.4 セキュリティエラー

**エラー**:
```
❌ エラー: パストラバーサルは許可されていません
```

**原因**: ビルドパスがプロジェクト外を指している

**解決策**:
- ビルドパスをプロジェクト内に設定
- 相対パスではなく絶対パスを使用

---

### 8. CI/CD統合

#### 8.1 Jenkins パイプライン

**Jenkinsfile 例**:
```groovy
pipeline {
    agent any

    environment {
        UNITY_PATH = 'C:\\Program Files\\Unity\\Hub\\Editor\\2022.3.10f1\\Editor\\Unity.exe'
    }

    stages {
        stage('Setup') {
            steps {
                bat 'pip install -r automation/requirements.txt'
            }
        }

        stage('Generate Config') {
            steps {
                bat 'python automation/build_mobile.py --generate-config build_config.json'
            }
        }

        stage('Build Android') {
            steps {
                bat 'python automation/build_mobile.py --config build_config.json'
            }
        }

        stage('Archive Artifacts') {
            steps {
                archiveArtifacts artifacts: 'builds/**/*.apk', fingerprint: true
                archiveArtifacts artifacts: 'builds/**/*.json', fingerprint: true
                archiveArtifacts artifacts: 'builds/**/*.md', fingerprint: true
            }
        }
    }

    post {
        always {
            archiveArtifacts artifacts: 'builds/**/*.log', fingerprint: true
        }
    }
}
```

#### 8.2 GitHub Actions

**例: .github/workflows/build-android.yml**:
```yaml
name: Build Android

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: windows-latest

    steps:
    - uses: actions/checkout@v3

    - name: Setup Python
      uses: actions/setup-python@v4
      with:
        python-version: '3.10'

    - name: Install dependencies
      run: pip install -r automation/requirements.txt

    - name: Setup Unity
      uses: game-ci/unity-builder@v2
      with:
        unityVersion: 2022.3.10f1

    - name: Generate build config
      run: python automation/build_mobile.py --generate-config build_config.json

    - name: Build Android APK
      run: python automation/build_mobile.py --config build_config.json

    - name: Upload APK
      uses: actions/upload-artifact@v3
      with:
        name: android-apk
        path: builds/**/*.apk

    - name: Upload Build Report
      uses: actions/upload-artifact@v3
      with:
        name: build-report
        path: builds/**/*.md
```

---

### 9. セキュリティ対策

#### 9.1 パストラバーサル防止

**実装内容**:
- `security_utils.PathValidator` による厳格なパス検証
- プロジェクト外へのビルド出力を禁止
- `..` を含むパスを拒否

#### 9.2 コマンドインジェクション防止

**実装内容**:
- `subprocess.run()` でリスト形式の引数を使用
- シェル展開を無効化（`shell=False`）
- 危険なコマンド（`rm`, `del`, `format`等）をブラックリスト化

#### 9.3 ファイルサイズ制限

**実装内容**:
- 設定ファイル読み込みは10MB以内に制限
- DoS攻撃を防止

---

### 10. ベストプラクティス

#### 10.1 ビルド前の準備

- [ ] Build Settings でシーンを追加
- [ ] Player Settings でバージョン設定
- [ ] Android SDK/NDK パス設定
- [ ] テクスチャ圧縮設定の確認
- [ ] Shader Stripping 有効化

#### 10.2 設定ファイル管理

- [ ] プラットフォーム別に設定ファイルを分離
  - `android_dev.json` - Android Development
  - `android_release.json` - Android Release
  - `ios_dev.json` - iOS Development
- [ ] Git管理（`.gitignore` で機密情報を除外）
- [ ] バージョン番号の統一

#### 10.3 ビルド最適化

- [ ] IL2CPP バックエンドを使用（パフォーマンス向上）
- [ ] Managed Stripping Level = Medium 以上
- [ ] Texture Compression = ASTC（高画質・低容量）
- [ ] Script Optimization = Speed
- [ ] LZ4 圧縮（ビルド速度重視）

---

### 11. 参考資料

#### 11.1 公式ドキュメント

- [Unity Build Pipeline](https://docs.unity3d.com/Manual/BuildPlayerPipeline.html)
- [Unity Command Line Arguments](https://docs.unity3d.com/Manual/CommandLineArguments.html)
- [Android Build Settings](https://docs.unity3d.com/Manual/android-BuildProcess.html)
- [iOS Build Settings](https://docs.unity3d.com/Manual/iphone-BuildProcess.html)

#### 11.2 プロジェクト内ドキュメント

- `PHASE4_WEEK4_PLAN.md` - Week 4 タスク計画
- `MOBILE_BUILD_REPORT.md` - ビルド検証テンプレート
- `automation/README.md` - 自動化ツール全体ガイド

---

### 12. まとめ

**提供される機能**:
✅ Python自動化スクリプト（500行）
✅ JSON設定テンプレート
✅ セキュリティ対策済み
✅ 詳細なビルドレポート生成
✅ CI/CD統合対応

**次のステップ**:
1. Unity Editor と Android SDK をインストール
2. `build_mobile.py --generate-config` で設定ファイル生成
3. 設定ファイルを編集
4. `build_mobile.py --config` でビルド実行
5. ビルドレポートで検証

**サポート**:
- 質問・バグ報告: GitHub Issues
- ドキュメント更新: Pull Request歓迎

---

**Document Version**: 1.0
**Last Updated**: 2026-03-15
**Author**: automation-dev agent

---

**END OF DOCUMENT**
