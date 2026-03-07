// ShaderOp Unity プロジェクト Jenkins パイプライン
//
// 前提条件:
// - Jenkins に Unity がインストールされている
// - Jenkins に必要なプラグインがインストール済み:
//   - Pipeline
//   - Git
//   - Credentials Binding
//   - Unity3d plugin (または手動でUnity CLI使用)
// - Jenkins Credentials に以下を設定:
//   - unity-license: Unity ライセンスファイル (Secret file)
//   - android-keystore: Android キーストアファイル (Secret file)
//   - android-keystore-pass: キーストアパスワード (Secret text)
//   - android-keyalias-pass: キーエイリアスパスワード (Secret text)

@Library('shared-library') _

// Unity バージョンファイルから動的にバージョンを読み取る関数
def getUnityVersionFromProject() {
    def versionFile = "${WORKSPACE}/ShaderOptimizer/ProjectSettings/ProjectVersion.txt"

    if (fileExists(versionFile)) {
        def content = readFile(versionFile)
        def matcher = content =~ /m_EditorVersion:\s*(\S+)/
        if (matcher) {
            return matcher[0][1]
        }
    }

    // デフォルトバージョン（フォールバック）
    return '6000.0.30f1'
}

// バージョンから Unity パスを構築
def findUnityByVersion(String version) {
    def osName = System.getProperty('os.name').toLowerCase()

    if (osName.contains('mac')) {
        return "/Applications/Unity/Hub/Editor/${version}/Unity.app/Contents/MacOS/Unity"
    } else if (osName.contains('win')) {
        return "C:\\Program Files\\Unity\\Hub\\Editor\\${version}\\Editor\\Unity.exe"
    } else {
        return "/opt/unity/${version}/Editor/Unity"
    }
}

// Unity パスを取得する統合関数
def getUnityPath() {
    // 1. 環境変数 UNITY_PATH が設定されている場合は最優先
    def unityPath = env.UNITY_PATH
    if (unityPath && fileExists(unityPath)) {
        echo "Using Unity from environment variable: ${unityPath}"
        return unityPath
    }

    // 2. ProjectVersion.txt からバージョンを読み取り、対応するパスを探す
    def version = getUnityVersionFromProject()
    def pathByVersion = findUnityByVersion(version)

    if (fileExists(pathByVersion)) {
        echo "Using Unity version ${version}: ${pathByVersion}"
        return pathByVersion
    }

    // 3. すべて失敗した場合はエラー
    error("Unity installation not found! Please set UNITY_PATH environment variable or install Unity ${version}")
}

// プラットフォーム別のデフォルトUnityパスを返すヘルパー関数（後方互換性のため保持）
@Deprecated
def getDefaultUnityPath() {
    def version = getUnityVersionFromProject()
    return findUnityByVersion(version)
}

pipeline {
    agent any

    // パラメータ定義
    parameters {
        choice(
            name: 'BUILD_TARGET',
            choices: ['All', 'Android', 'iOS', 'WebGL', 'Windows', 'Linux'],
            description: 'ビルドするターゲットプラットフォーム'
        )
        choice(
            name: 'BUILD_CONFIGURATION',
            choices: ['Development', 'Release'],
            description: 'ビルド構成'
        )
        string(
            name: 'VERSION',
            defaultValue: '0.1.0',
            description: 'ビルドバージョン (例: 0.1.0)'
        )
        booleanParam(
            name: 'RUN_TESTS',
            defaultValue: true,
            description: 'ビルド前にテストを実行する'
        )
        booleanParam(
            name: 'SKIP_CACHE',
            defaultValue: false,
            description: 'Unity Library キャッシュをスキップする'
        )
        booleanParam(
            name: 'VALIDATE_ARTIFACTS',
            defaultValue: true,
            description: 'ビルド成果物の検証を実行する'
        )
        booleanParam(
            name: 'RUN_SECURITY_SCAN',
            defaultValue: true,
            description: 'セキュリティスキャンを実行する'
        )
        booleanParam(
            name: 'COLLECT_METRICS',
            defaultValue: true,
            description: 'ビルドメトリクスを収集する'
        )
    }

    // 環境変数
    environment {
        // Unity パスは Setup ステージで動的に設定
        // Jenkins の「グローバルプロパティ」で UNITY_PATH を設定することを推奨
        // 設定されていない場合は ProjectVersion.txt から自動検出

        PROJECT_PATH = "${WORKSPACE}/ShaderOptimizer"
        BUILD_PATH = "${WORKSPACE}/builds"

        // Unity ライセンス（必須）
        UNITY_LICENSE = credentials('unity-license')

        // Android 署名設定（Android ビルド時のみ必要）
        ANDROID_KEYSTORE = credentials('android-keystore')
        ANDROID_KEYSTORE_PASS = credentials('android-keystore-pass')
        ANDROID_KEYALIAS_PASS = credentials('android-keyalias-pass')

        // Unity ログファイル
        UNITY_LOG_FILE = "${WORKSPACE}/unity-build.log"
    }

    options {
        // ビルド履歴を30個まで保持
        buildDiscarder(logRotator(numToKeepStr: '30'))

        // タイムアウト設定（2時間）
        timeout(time: 120, unit: 'MINUTES')

        // 同時ビルドを1つまでに制限
        disableConcurrentBuilds()

        // タイムスタンプをログに表示
        timestamps()
    }

    stages {
        // ステージ1: 環境準備
        stage('Setup') {
            steps {
                script {
                    echo "=== ShaderOp Unity Build Pipeline ==="
                    echo "Build Target: ${params.BUILD_TARGET}"
                    echo "Configuration: ${params.BUILD_CONFIGURATION}"
                    echo "Version: ${params.VERSION}"
                    echo "Workspace: ${WORKSPACE}"

                    // Unity パスを動的に設定
                    env.UNITY_PATH = getUnityPath()
                    echo "Unity Path: ${env.UNITY_PATH}"

                    // ビルドディレクトリの作成（セキュア化）
                    sh '''
                        set -e
                        set -u
                        mkdir -p "${BUILD_PATH}"
                    '''

                    // Unity バージョン確認（セキュア化）
                    sh '''
                        set -e
                        set -u
                        if [ ! -f "${UNITY_PATH}" ]; then
                            echo "❌ Unity executable not found at: ${UNITY_PATH}"
                            exit 1
                        fi

                        echo "Unity version check:"
                        "${UNITY_PATH}" -version || echo "Unity version command failed"
                    '''
                }
            }
        }

        // ステージ2: Git リポジトリのクリーンアップ
        stage('Checkout') {
            steps {
                checkout scm

                script {
                    // Git LFS のプル
                    sh 'git lfs pull'

                    // サブモジュールの更新（もしあれば）
                    sh 'git submodule update --init --recursive'
                }
            }
        }

        // ステージ3: 依存関係検証
        stage('Validate Dependencies') {
            steps {
                script {
                    echo "=== Validating Package Dependencies ==="

                    sh '''
                        set -e
                        set -u

                        MANIFEST="${PROJECT_PATH}/Packages/manifest.json"

                        if [ ! -f "$MANIFEST" ]; then
                            echo "❌ Packages/manifest.json not found"
                            exit 1
                        fi

                        echo "Checking critical package dependencies..."

                        # UniTask の確認
                        if ! grep -q '"com.cysharp.unitask"' "$MANIFEST"; then
                            echo "⚠ UniTask not found in manifest.json"
                        else
                            echo "✓ UniTask found"
                        fi

                        # UniRx の確認
                        if ! grep -q '"com.neuecc.unirx"' "$MANIFEST"; then
                            echo "⚠ UniRx not found in manifest.json"
                        else
                            echo "✓ UniRx found"
                        fi

                        # Shader Graph の確認
                        if ! grep -q '"com.unity.shadergraph"' "$MANIFEST"; then
                            echo "⚠ Shader Graph not found in manifest.json"
                        else
                            echo "✓ Shader Graph found"
                        fi

                        # URP の確認
                        if ! grep -q '"com.unity.render-pipelines.universal"' "$MANIFEST"; then
                            echo "⚠ URP not found in manifest.json"
                        else
                            echo "✓ URP found"
                        fi

                        echo "✓ Dependency validation complete"
                    '''
                }
            }
        }

        // ステージ4: アセット検証
        stage('Validate Assets') {
            steps {
                script {
                    echo "=== Running Asset Validation ==="

                    sh '''
                        set -e
                        set -u
                        python3 automation/validate_assets.py \
                            --project "${PROJECT_PATH}" \
                            --fail-on-warning
                    '''
                }
            }
            post {
                always {
                    archiveArtifacts artifacts: 'validation_report.txt', allowEmptyArchive: true
                }
            }
        }

        // ステージ5: セキュリティスキャン
        stage('Security Scan') {
            when {
                expression { return params.RUN_SECURITY_SCAN }
            }
            steps {
                script {
                    echo "=== Running Security Scan ==="

                    sh '''
                        set -e

                        SCAN_FAILED=0

                        echo "1. Checking for secrets in code..."
                        # API キーのパターン検索
                        if grep -r "API_KEY\\s*=\\s*\\"[^\\"]*\\"" "${PROJECT_PATH}/Assets/Scripts" --include="*.cs" 2>/dev/null; then
                            echo "⚠ Potential API key found in code"
                            SCAN_FAILED=1
                        fi

                        # パスワードのパターン検索
                        if grep -r "password\\s*=\\s*\\"[^\\"]*\\"" "${PROJECT_PATH}/Assets/Scripts" --include="*.cs" -i 2>/dev/null; then
                            echo "⚠ Potential password found in code"
                            SCAN_FAILED=1
                        fi

                        # トークンのパターン検索
                        if grep -r "token\\s*=\\s*\\"[^\\"]*\\"" "${PROJECT_PATH}/Assets/Scripts" --include="*.cs" -i 2>/dev/null; then
                            echo "⚠ Potential token found in code"
                            SCAN_FAILED=1
                        fi

                        if [ $SCAN_FAILED -eq 0 ]; then
                            echo "✓ No secrets detected in code"
                        fi

                        echo ""
                        echo "2. Validating .gitignore..."
                        if [ ! -f "${WORKSPACE}/.gitignore" ]; then
                            echo "❌ .gitignore missing"
                            exit 1
                        fi

                        # 重要なエントリの確認
                        if ! grep -q "Library/" "${WORKSPACE}/.gitignore"; then
                            echo "❌ .gitignore missing Library/ entry"
                            exit 1
                        fi

                        if ! grep -q "Temp/" "${WORKSPACE}/.gitignore"; then
                            echo "❌ .gitignore missing Temp/ entry"
                            exit 1
                        fi

                        if ! grep -q "obj/" "${WORKSPACE}/.gitignore"; then
                            echo "❌ .gitignore missing obj/ entry"
                            exit 1
                        fi

                        echo "✓ .gitignore valid"

                        echo ""
                        echo "3. Checking for sensitive files..."
                        # .env ファイルの確認
                        if find "${PROJECT_PATH}" -name ".env" -type f 2>/dev/null | grep -q .; then
                            echo "⚠ .env files found - ensure they are in .gitignore"
                            find "${PROJECT_PATH}" -name ".env" -type f
                            SCAN_FAILED=1
                        fi

                        # credentials ファイルの確認
                        if find "${PROJECT_PATH}" -name "*credentials*.json" -type f 2>/dev/null | grep -q .; then
                            echo "⚠ Credentials files found - ensure they are in .gitignore"
                            find "${PROJECT_PATH}" -name "*credentials*.json" -type f
                            SCAN_FAILED=1
                        fi

                        if [ $SCAN_FAILED -eq 0 ]; then
                            echo "✓ No sensitive files detected"
                        fi

                        echo ""
                        echo "4. Checking file permissions..."
                        # 実行権限を持つべきでないファイルの確認
                        if find "${PROJECT_PATH}/Assets" -name "*.cs" -perm /111 -type f 2>/dev/null | grep -q .; then
                            echo "⚠ Executable C# files found"
                            find "${PROJECT_PATH}/Assets" -name "*.cs" -perm /111 -type f
                            SCAN_FAILED=1
                        fi

                        if [ $SCAN_FAILED -eq 0 ]; then
                            echo "✓ File permissions OK"
                        fi

                        echo ""
                        if [ $SCAN_FAILED -eq 1 ]; then
                            echo "❌ Security scan completed with warnings"
                            exit 1
                        else
                            echo "✅ Security scan passed"
                        fi
                    '''
                }
            }
        }

        // ステージ6: シェーダープロファイリング
        stage('Shader Profiling') {
            steps {
                script {
                    echo "=== Running Shader Profiling ==="

                    sh '''
                        set -e
                        set -u
                        python3 automation/shader_profiling.py \
                            --project "${PROJECT_PATH}" \
                            --export-json \
                            --output "${WORKSPACE}/shader_profile_report.json"
                    '''
                }
            }
            post {
                always {
                    archiveArtifacts artifacts: 'shader_profile_report.json', allowEmptyArchive: true
                }
            }
        }

        // ステージ7: Unity Library のキャッシュ復元
        stage('Restore Cache') {
            when {
                expression { return !params.SKIP_CACHE }
            }
            steps {
                script {
                    echo "Restoring Unity Library cache..."
                    // キャッシュプラグインを使用する場合
                    // cache(maxCacheSize: 10000, caches: [
                    //     arbitraryFileCache(path: "${PROJECT_PATH}/Library", cacheValidityDecidingFile: "${PROJECT_PATH}/Packages/manifest.json")
                    // ]) {
                    //     echo "Cache restored"
                    // }
                }
            }
        }

        // ステージ8: テスト実行
        stage('Run Tests') {
            when {
                expression { return params.RUN_TESTS }
            }
            steps {
                script {
                    echo "=== Running Unity Tests ==="

                    // EditMode テスト実行（セキュア化）
                    sh '''
                        set -e
                        set -u
                        "${UNITY_PATH}" \
                            -batchmode \
                            -nographics \
                            -projectPath "${PROJECT_PATH}" \
                            -runTests \
                            -testPlatform EditMode \
                            -testResults "${WORKSPACE}/test-results-editmode.xml" \
                            -logFile "${UNITY_LOG_FILE}"
                    '''

                    // PlayMode テスト実行（セキュア化）
                    sh '''
                        set -e
                        set -u
                        "${UNITY_PATH}" \
                            -batchmode \
                            -nographics \
                            -projectPath "${PROJECT_PATH}" \
                            -runTests \
                            -testPlatform PlayMode \
                            -testResults "${WORKSPACE}/test-results-playmode.xml" \
                            -logFile "${UNITY_LOG_FILE}"
                    '''
                }
            }
            post {
                always {
                    // テスト結果の公開
                    junit testResults: 'test-results-*.xml', allowEmptyResults: true

                    // Unity ログのアーカイブ
                    archiveArtifacts artifacts: 'unity-build.log', allowEmptyArchive: true
                }
            }
        }

        // ステージ9: Android ビルド
        stage('Build Android') {
            when {
                expression { return params.BUILD_TARGET == 'All' || params.BUILD_TARGET == 'Android' }
            }
            steps {
                // withCredentials で機密情報を安全に取り扱う
                withCredentials([
                    file(credentialsId: 'android-keystore', variable: 'KEYSTORE_FILE'),
                    string(credentialsId: 'android-keystore-pass', variable: 'KEYSTORE_PASS'),
                    string(credentialsId: 'android-keyalias-pass', variable: 'KEYALIAS_PASS')
                ]) {
                    script {
                        echo "=== Building for Android ==="

                        def buildMethod = params.BUILD_CONFIGURATION == 'Release' ? 'BuildScript.BuildAndroid' : 'BuildScript.BuildAndroidDev'

                        sh """
                            set -e
                            set -u

                            # 一時ファイルにキーストアをコピー（読み取り専用）
                            TEMP_KEYSTORE="/tmp/build_\$\$.keystore"
                            cp "\${KEYSTORE_FILE}" "\${TEMP_KEYSTORE}"
                            chmod 400 "\${TEMP_KEYSTORE}"

                            # ビルド実行
                            "\${UNITY_PATH}" \
                                -batchmode \
                                -nographics \
                                -quit \
                                -projectPath "\${PROJECT_PATH}" \
                                -executeMethod ${buildMethod} \
                                -buildTarget Android \
                                -buildPath "\${BUILD_PATH}/Android/ShaderOp.apk" \
                                -logFile "\${UNITY_LOG_FILE}"

                            # 一時ファイルを確実に削除
                            rm -f "\${TEMP_KEYSTORE}"
                        """
                    }
                }
            }
            post {
                success {
                    archiveArtifacts artifacts: 'builds/Android/**/*.apk', fingerprint: true
                }
            }
        }

        // ステージ10: Android ビルド検証
        stage('Validate Android Build') {
            when {
                allOf {
                    expression { return params.BUILD_TARGET == 'All' || params.BUILD_TARGET == 'Android' }
                    expression { return params.VALIDATE_ARTIFACTS }
                }
            }
            steps {
                script {
                    echo "=== Validating Android Build Artifacts ==="

                    sh '''
                        set -e
                        set -u

                        APK_PATH="${BUILD_PATH}/Android/ShaderOp.apk"

                        echo "1. File existence check..."
                        if [ ! -f "$APK_PATH" ]; then
                            echo "❌ APK file not found at: $APK_PATH"
                            exit 1
                        fi
                        echo "✓ APK file exists"

                        echo ""
                        echo "2. File size sanity check..."
                        # Windowsとmacで異なるstatコマンドに対応
                        if [ "$(uname)" = "Darwin" ]; then
                            APK_SIZE=$(stat -f%z "$APK_PATH")
                        else
                            APK_SIZE=$(stat -c%s "$APK_PATH")
                        fi

                        MIN_SIZE=10485760   # 10MB minimum
                        MAX_SIZE=524288000  # 500MB maximum

                        if [ "$APK_SIZE" -lt "$MIN_SIZE" ]; then
                            echo "❌ APK too small: ${APK_SIZE} bytes (min: ${MIN_SIZE})"
                            exit 1
                        fi

                        if [ "$APK_SIZE" -gt "$MAX_SIZE" ]; then
                            echo "❌ APK too large: ${APK_SIZE} bytes (max: ${MAX_SIZE})"
                            exit 1
                        fi

                        APK_SIZE_MB=$((APK_SIZE / 1024 / 1024))
                        echo "✓ APK size OK: ${APK_SIZE} bytes (${APK_SIZE_MB} MB)"

                        echo ""
                        echo "3. APK structure validation..."
                        # aapt が利用可能な場合のみ検証
                        if command -v aapt >/dev/null 2>&1; then
                            if aapt dump badging "$APK_PATH" > /dev/null 2>&1; then
                                echo "✓ APK structure valid"

                                # パッケージ情報の表示
                                PACKAGE_NAME=$(aapt dump badging "$APK_PATH" | grep package | awk \'{print $2}\' | sed s/name=//g | sed s/\\'//g)
                                VERSION_CODE=$(aapt dump badging "$APK_PATH" | grep package | awk \'{print $3}\' | sed s/versionCode=//g | sed s/\\'//g)
                                VERSION_NAME=$(aapt dump badging "$APK_PATH" | grep package | awk \'{print $4}\' | sed s/versionName=//g | sed s/\\'//g)

                                echo "  Package: $PACKAGE_NAME"
                                echo "  Version Code: $VERSION_CODE"
                                echo "  Version Name: $VERSION_NAME"
                            else
                                echo "⚠ APK structure validation failed"
                                exit 1
                            fi
                        else
                            echo "⚠ aapt not available - skipping APK structure validation"
                        fi

                        echo ""
                        echo "4. APK signing verification..."
                        # jarsigner が利用可能な場合のみ検証
                        if command -v jarsigner >/dev/null 2>&1; then
                            if jarsigner -verify -verbose -certs "$APK_PATH" > /dev/null 2>&1; then
                                echo "✓ APK is signed"
                            else
                                echo "⚠ APK signature verification failed"
                                # 開発ビルドの場合は警告のみ
                                if [ "''' + params.BUILD_CONFIGURATION + '''" = "Development" ]; then
                                    echo "  (Development build - continuing)"
                                else
                                    exit 1
                                fi
                            fi
                        else
                            echo "⚠ jarsigner not available - skipping signature verification"
                        fi

                        echo ""
                        echo "5. APK content check..."
                        # ZIP として解凍してみる
                        TEMP_DIR="/tmp/apk_validation_$$"
                        mkdir -p "$TEMP_DIR"

                        if unzip -q -d "$TEMP_DIR" "$APK_PATH" 2>/dev/null; then
                            # AndroidManifest.xml の存在確認
                            if [ ! -f "$TEMP_DIR/AndroidManifest.xml" ]; then
                                echo "❌ AndroidManifest.xml not found in APK"
                                rm -rf "$TEMP_DIR"
                                exit 1
                            fi

                            # classes.dex の存在確認
                            if ! ls "$TEMP_DIR"/*.dex >/dev/null 2>&1; then
                                echo "❌ No .dex files found in APK"
                                rm -rf "$TEMP_DIR"
                                exit 1
                            fi

                            # lib ディレクトリの確認
                            if [ -d "$TEMP_DIR/lib" ]; then
                                echo "✓ Native libraries found:"
                                ls -lh "$TEMP_DIR/lib"
                            fi

                            # assets ディレクトリの確認
                            if [ -d "$TEMP_DIR/assets" ]; then
                                ASSETS_SIZE=$(du -sh "$TEMP_DIR/assets" | awk \'{print $1}\')
                                echo "✓ Assets directory found (${ASSETS_SIZE})"
                            fi

                            rm -rf "$TEMP_DIR"
                            echo "✓ APK content validation passed"
                        else
                            echo "❌ Failed to extract APK"
                            rm -rf "$TEMP_DIR"
                            exit 1
                        fi

                        echo ""
                        echo "✅ Android build validation completed successfully"
                    '''
                }
            }
        }

        // ステージ11: iOS ビルド
        stage('Build iOS') {
            when {
                expression { return params.BUILD_TARGET == 'All' || params.BUILD_TARGET == 'iOS' }
            }
            steps {
                script {
                    echo "=== Building for iOS ==="

                    def buildMethod = params.BUILD_CONFIGURATION == 'Release' ? 'BuildScript.BuildiOS' : 'BuildScript.BuildiOSDev'

                    sh """
                        set -e
                        set -u
                        "\${UNITY_PATH}" \
                            -batchmode \
                            -nographics \
                            -quit \
                            -projectPath "\${PROJECT_PATH}" \
                            -executeMethod ${buildMethod} \
                            -buildTarget iOS \
                            -buildPath "\${BUILD_PATH}/iOS" \
                            -logFile "\${UNITY_LOG_FILE}"
                    """
                }
            }
            post {
                success {
                    archiveArtifacts artifacts: 'builds/iOS/**/*', fingerprint: true
                }
            }
        }

        // ステージ12: WebGL ビルド
        stage('Build WebGL') {
            when {
                expression { return params.BUILD_TARGET == 'All' || params.BUILD_TARGET == 'WebGL' }
            }
            steps {
                script {
                    echo "=== Building for WebGL ==="

                    def buildMethod = params.BUILD_CONFIGURATION == 'Release' ? 'BuildScript.BuildWebGL' : 'BuildScript.BuildWebGLDev'

                    sh """
                        set -e
                        set -u
                        "\${UNITY_PATH}" \
                            -batchmode \
                            -nographics \
                            -quit \
                            -projectPath "\${PROJECT_PATH}" \
                            -executeMethod ${buildMethod} \
                            -buildTarget WebGL \
                            -buildPath "\${BUILD_PATH}/WebGL" \
                            -logFile "\${UNITY_LOG_FILE}"
                    """
                }
            }
            post {
                success {
                    archiveArtifacts artifacts: 'builds/WebGL/**/*', fingerprint: true
                }
            }
        }

        // ステージ13: Windows ビルド
        stage('Build Windows') {
            when {
                expression { return params.BUILD_TARGET == 'All' || params.BUILD_TARGET == 'Windows' }
            }
            steps {
                script {
                    echo "=== Building for Windows ==="

                    def buildMethod = params.BUILD_CONFIGURATION == 'Release' ? 'BuildScript.BuildWindows' : 'BuildScript.BuildWindowsDev'

                    sh """
                        set -e
                        set -u
                        "\${UNITY_PATH}" \
                            -batchmode \
                            -nographics \
                            -quit \
                            -projectPath "\${PROJECT_PATH}" \
                            -executeMethod ${buildMethod} \
                            -buildTarget StandaloneWindows64 \
                            -buildPath "\${BUILD_PATH}/Windows" \
                            -logFile "\${UNITY_LOG_FILE}"
                    """
                }
            }
            post {
                success {
                    archiveArtifacts artifacts: 'builds/Windows/**/*', fingerprint: true
                }
            }
        }

        // ステージ14: Linux ビルド
        stage('Build Linux') {
            when {
                expression { return params.BUILD_TARGET == 'All' || params.BUILD_TARGET == 'Linux' }
            }
            steps {
                script {
                    echo "=== Building for Linux ==="

                    def buildMethod = params.BUILD_CONFIGURATION == 'Release' ? 'BuildScript.BuildLinux' : 'BuildScript.BuildLinuxDev'

                    sh """
                        set -e
                        set -u
                        "\${UNITY_PATH}" \
                            -batchmode \
                            -nographics \
                            -quit \
                            -projectPath "\${PROJECT_PATH}" \
                            -executeMethod ${buildMethod} \
                            -buildTarget StandaloneLinux64 \
                            -buildPath "\${BUILD_PATH}/Linux" \
                            -logFile "\${UNITY_LOG_FILE}"
                    """
                }
            }
            post {
                success {
                    archiveArtifacts artifacts: 'builds/Linux/**/*', fingerprint: true
                }
            }
        }

        // ステージ15: ビルドメトリクス収集
        stage('Collect Metrics') {
            when {
                expression { return params.COLLECT_METRICS }
            }
            steps {
                script {
                    echo "=== Collecting Build Metrics ==="

                    sh '''
                        set -e
                        set -u

                        METRICS_FILE="${WORKSPACE}/build-metrics.json"

                        # Unity バージョンの取得
                        UNITY_VERSION="unknown"
                        if [ -f "${PROJECT_PATH}/ProjectSettings/ProjectVersion.txt" ]; then
                            UNITY_VERSION=$(grep "m_EditorVersion:" "${PROJECT_PATH}/ProjectSettings/ProjectVersion.txt" | awk \'{print $2}\')
                        fi

                        # Git コミット情報の取得
                        GIT_COMMIT=$(git rev-parse HEAD 2>/dev/null || echo "unknown")
                        GIT_BRANCH=$(git rev-parse --abbrev-ref HEAD 2>/dev/null || echo "unknown")

                        # Android APK サイズの取得
                        ANDROID_APK_SIZE=0
                        if [ -f "${BUILD_PATH}/Android/ShaderOp.apk" ]; then
                            if [ "$(uname)" = "Darwin" ]; then
                                ANDROID_APK_SIZE=$(stat -f%z "${BUILD_PATH}/Android/ShaderOp.apk")
                            else
                                ANDROID_APK_SIZE=$(stat -c%s "${BUILD_PATH}/Android/ShaderOp.apk")
                            fi
                        fi

                        # メトリクス JSON の作成
                        cat > "$METRICS_FILE" <<EOF
{
  "build_info": {
    "build_number": "${BUILD_NUMBER}",
    "job_name": "${JOB_NAME}",
    "timestamp": "$(date -u +"%Y-%m-%dT%H:%M:%SZ")",
    "build_url": "${BUILD_URL}"
  },
  "git_info": {
    "commit": "$GIT_COMMIT",
    "branch": "$GIT_BRANCH"
  },
  "unity_info": {
    "version": "$UNITY_VERSION",
    "path": "${UNITY_PATH}"
  },
  "build_config": {
    "target": "${params.BUILD_TARGET}",
    "configuration": "${params.BUILD_CONFIGURATION}",
    "version": "${params.VERSION}",
    "run_tests": ${params.RUN_TESTS},
    "validate_artifacts": ${params.VALIDATE_ARTIFACTS},
    "run_security_scan": ${params.RUN_SECURITY_SCAN}
  },
  "artifacts": {
    "android": {
      "size_bytes": $ANDROID_APK_SIZE,
      "size_mb": $((ANDROID_APK_SIZE / 1024 / 1024)),
      "exists": $([ -f "${BUILD_PATH}/Android/ShaderOp.apk" ] && echo "true" || echo "false")
    },
    "ios": {
      "exists": $([ -d "${BUILD_PATH}/iOS" ] && echo "true" || echo "false")
    },
    "webgl": {
      "exists": $([ -d "${BUILD_PATH}/WebGL" ] && echo "true" || echo "false")
    },
    "windows": {
      "exists": $([ -d "${BUILD_PATH}/Windows" ] && echo "true" || echo "false")
    },
    "linux": {
      "exists": $([ -d "${BUILD_PATH}/Linux" ] && echo "true" || echo "false")
    }
  },
  "workspace": {
    "path": "${WORKSPACE}",
    "project_path": "${PROJECT_PATH}",
    "build_path": "${BUILD_PATH}"
  }
}
EOF

                        echo "✓ Build metrics collected"
                        echo ""
                        echo "Metrics summary:"
                        cat "$METRICS_FILE"
                    '''
                }
            }
            post {
                always {
                    archiveArtifacts artifacts: 'build-metrics.json', allowEmptyArchive: true
                }
            }
        }

        // ステージ16: ビルド成果物の整理とアップロード
        stage('Package Artifacts') {
            steps {
                script {
                    echo "=== Packaging Build Artifacts ==="

                    // ビルドをZIP圧縮（セキュア化）
                    sh '''
                        set -e
                        set -u
                        cd "${BUILD_PATH}"
                        for dir in */; do
                            if [ -d "$dir" ]; then
                                zip -r "${dir%/}-''' + params.VERSION + '''.zip" "$dir"
                            fi
                        done
                    '''
                }
            }
            post {
                success {
                    archiveArtifacts artifacts: 'builds/*.zip', fingerprint: true
                }
            }
        }
    }

    // パイプライン全体の後処理
    post {
        always {
            script {
                echo "=== Build Pipeline Completed ==="

                // Unity ログの表示（最後の100行）
                sh "tail -n 100 ${UNITY_LOG_FILE} || echo 'No Unity log file found'"

                // ワークスペースのクリーンアップ（オプション）
                // cleanWs()
            }
        }
        success {
            echo "✅ ビルドが正常に完了しました"
            // 成功時の通知（Slack、Email等）
            // slackSend color: 'good', message: "Build Successful: ${env.JOB_NAME} #${env.BUILD_NUMBER}"
        }
        failure {
            echo "❌ ビルドが失敗しました"
            // 失敗時の通知
            // slackSend color: 'danger', message: "Build Failed: ${env.JOB_NAME} #${env.BUILD_NUMBER}"
        }
    }
}
