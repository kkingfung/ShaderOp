# ShaderOp 自動化スクリプト

## 📋 概要

Unity プロジェクトのビルドとアセット管理を自動化するPythonスクリプト集です。

## 🚀 クイックスタート

```bash
# 1. Git Hooks をインストール（推奨）
python automation/setup_hooks.py --install

# 2. アセット検証を実行
python automation/validate_assets.py

# 3. シェーダープロファイリングを実行
python automation/shader_profiling.py

# 4. ビルドバージョンを設定
python automation/build_utils.py set-version 1.0.0
```

## 🛠️ スクリプト一覧

### 1. validate_assets.py - アセット検証 ✅ UTF-8対応

Unity プロジェクトのアセットを検証し、問題を早期に発見します。

#### 検証項目

- ✅ **シェーダーファイル検証**
  - Shader 定義の存在確認
  - SubShader の存在確認
  - 括弧の数の整合性チェック

- ✅ **テクスチャファイル検証**
  - ファイルサイズチェック（10MB以上で警告）
  - 命名規則チェック（T_ または tex_ プレフィックス推奨）

- ✅ **メタファイル検証**
  - メタファイルの存在確認
  - 孤立したメタファイルの検出

- ✅ **命名規則検証**
  - PascalCase チェック（C# スクリプト）
  - スペースを含むファイル名の検出

- ✅ **Shader Graph 検証**
  - JSON 構造の整合性確認

#### 使用方法

```bash
# 基本的な使用
python validate_assets.py

# プロジェクトパスを指定
python validate_assets.py --project /path/to/ShaderOptimizer

# 警告でも失敗とする（CI/CD用）
python validate_assets.py --fail-on-warning
```

#### 出力例

```
=== ShaderOp Asset Validation ===
Project: D:\PersonalGameDev\ShaderOp\ShaderOptimizer

[1/5] シェーダーファイル検証中...
  ✓ 8 個のシェーダーファイルをチェックしました

[2/5] テクスチャファイル検証中...
  ✓ 42 個のテクスチャファイルをチェックしました
  ⚠ 3 個の大きなテクスチャが見つかりました

[3/5] メタファイル検証中...
  ✓ 156 個のアセットのメタファイルをチェックしました

[4/5] 命名規則検証中...
  ✓ 35 個のスクリプトファイルをチェックしました

[5/5] Shader Graph 検証中...
  ✓ 1 個の Shader Graph をチェックしました

============================================================
検証結果
============================================================

⚠️  警告: 3 件
  - 大きなテクスチャ: Assets/Textures/background.png (12.5 MB)
  - テクスチャ命名規則違反: myTexture.png
  - スクリプト命名規則違反: myScript.cs

============================================================
```

---

### 2. build_utils.py - ビルドユーティリティ ✅ UTF-8対応

ビルド番号の管理、バージョン設定、ビルド成果物の整理を行います。

#### 機能

- 📦 **ビルド番号管理**: プラットフォームごとのビルド番号を自動インクリメント
- 🏷️ **バージョン管理**: セマンティックバージョニング対応
- 📂 **ビルド整理**: ZIP アーカイブの自動作成
- 📊 **レポート生成**: JSON/Markdown 形式のビルドレポート
- 🗑️ **クリーンアップ**: 古いビルドの自動削除

#### コマンド一覧

##### increment - ビルド番号をインクリメント

```bash
# すべてのプラットフォーム
python build_utils.py increment --platform all

# 特定のプラットフォーム
python build_utils.py increment --platform Android
```

##### set-version - バージョンを設定

```bash
python build_utils.py set-version 1.0.0
```

##### organize - ビルド成果物を整理

```bash
python build_utils.py organize \
  --builds-dir builds \
  --output-dir releases
```

出力例: `releases/ShaderOp_Android_1.0.0_20260227_143022.zip`

##### report - ビルドレポートを生成

```bash
python build_utils.py report \
  --builds-dir builds \
  --output build_report.json
```

生成されるファイル:
- `build_report.json`: 詳細なビルド情報（JSON形式）
- `build_report.md`: 人間が読みやすいレポート（Markdown形式）

レポート例（Markdown）:

```markdown
# ShaderOp Build Report

**Version:** 1.0.0
**Date:** 2026-02-27T14:30:22

## Build Artifacts

| Platform | Size (MB) | Files |
|----------|-----------|-------|
| Android | 45.2 | 123 |
| WebGL | 38.7 | 89 |
| Windows | 52.1 | 145 |

**Total Size:** 136.0 MB
```

##### clean - 古いビルドをクリーンアップ

```bash
# 最新5個を保持、それ以外を削除
python build_utils.py clean \
  --builds-dir releases \
  --keep 5
```

#### ビルド情報ファイル

`build_info.json` にビルド情報が保存されます:

```json
{
  "version": "1.0.0",
  "Android_build_number": 42,
  "iOS_build_number": 38,
  "Windows_build_number": 15,
  "WebGL_build_number": 12,
  "Linux_build_number": 8,
  "last_update": "2026-02-27T14:30:22"
}
```

---

### 3. pre_commit_check.py - プリコミットチェック ✅ UTF-8対応

コミット前に自動でコード品質をチェックし、問題を早期発見します。

#### チェック項目

- 🔒 **機密情報検出**
  - パスワード、APIキー、シークレットキーの検出
  - Bearer トークンの検出

- 📝 **コーディングスタイル**
  - `#nullable enable` の存在確認（C#）
  - Debug.Log の残存チェック
  - タブ文字の検出（Python/JS）
  - 行末の空白チェック

- 📌 **TODO/FIXME コメント**
  - TODO: 警告
  - FIXME: エラー
  - HACK: 警告

- 📏 **ファイルサイズ**
  - 1MB以上: 警告
  - 5MB以上: エラー

#### 使用方法

##### Git Hooks として設定（推奨）

```bash
# Linux/macOS
cat > .git/hooks/pre-commit << 'EOF'
#!/bin/bash
python automation/pre_commit_check.py
EOF

chmod +x .git/hooks/pre-commit

# Windows (PowerShell)
@"
#!/bin/bash
python automation/pre_commit_check.py
"@ | Out-File -FilePath .git/hooks/pre-commit -Encoding ASCII
```

##### 手動実行

```bash
# ステージングされたファイルを自動チェック
python pre_commit_check.py

# 特定のファイルをチェック
python pre_commit_check.py --files MyScript.cs MyShader.shader

# 警告でも失敗とする
python pre_commit_check.py --fail-on-warning
```

#### 出力例

```
=== Pre-commit Checks ===

チェック中: CharacterCustomizer.cs
チェック中: MaterialController.cs
チェック中: BuildScript.cs

============================================================
チェック結果
============================================================

❌ エラー: 2 件
  - CharacterCustomizer.cs:42: FIXME コメントがあります: // FIXME: Optimize this
  - MaterialController.cs:15: パスワードが含まれている可能性

⚠️  警告: 3 件
  - CharacterCustomizer.cs:128: TODO コメントがあります: // TODO: Add validation
  - BuildScript.cs: Debug.Log が 2 箇所あります（削除推奨）
  - MaterialController.cs:5: 行末に空白があります

============================================================
```

---

### 4. shader_profiling.py - シェーダープロファイリング 🆕

シェーダーのパフォーマンス分析とバリアント数の推定を行います。

#### 分析項目

- 🔍 **シェーダーバリアント推定**
  - multi_compile/shader_feature の検出
  - バリアント爆発の警告

- 📊 **複雑度分析**
  - Pass数のカウント
  - 行数による複雑度評価
  - Shader Graphノード数

- 📱 **モバイル最適化チェック**
  - clip()使用の検出
  - 高負荷演算の検出

#### 使用方法

```bash
# 基本的な使用
python shader_profiling.py

# プロジェクトパスを指定
python shader_profiling.py --project ShaderOptimizer

# JSONレポート出力
python shader_profiling.py --export-json --output shader_report.json
```

#### 出力例

```
=== ShaderOp Shader Profiling ===

[1/4] シェーダーファイル分析中...
  ✓ 120 個のシェーダーファイルを分析しました
  ℹ 総Pass数: 172
  ℹ 平均Pass数/シェーダー: 1.43

📊 統計情報
============================================================
シェーダーファイル数: 120
Shader Graph数: 16
総Pass数: 172
モバイル最適化懸念: 2 件

📌 最も複雑なシェーダー (Pass数順):
  - Toon_DoubleShadeWithFeather.shader: 4 passes
```

---

### 5. setup_hooks.py - Git Hooks 自動セットアップ 🆕

Git Hooksの自動インストール・管理を行います。

#### 機能

- 🔧 **pre-commit hook**: コミット前のコード品質チェック
- 🚀 **pre-push hook**: プッシュ前のアセット検証
- 🌐 **クロスプラットフォーム対応**: Windows/Linux/macOS

#### コマンド一覧

##### install - Git Hooks をインストール

```bash
python setup_hooks.py --install
```

##### status - インストール状態を確認

```bash
python setup_hooks.py --status
```

出力例:
```
=== Git Hooks ステータス ===

✅ pre-commit
   コミット前のコード品質チェック
   場所: .git/hooks/pre-commit
   サイズ: 1419 bytes

✅ pre-push
   プッシュ前のアセット検証
   場所: .git/hooks/pre-push
   サイズ: 1674 bytes
```

##### uninstall - Git Hooks をアンインストール

```bash
python setup_hooks.py --uninstall
```

#### フックをスキップする方法

```bash
# コミット時
git commit --no-verify

# プッシュ時
git push --no-verify
```

---

### 6. validate_scenes.py - Unity Scene検証 🆕 (Phase 2)

Unity Scene構築を検証し、Phase 2のシーンセットアップをサポートします。

#### 検証対象シーン

- **MainMenu** - Canvas/UI配置、Button配線、EventSystem
- **MainCustomization** - CharacterCustomizer、Camera、Lighting
- **RoomDecoration** - RoomDecorator、床・壁メッシュ、UI
- **TicTacToeHex** - MVC Components、HexTileプレハブ、UI
- **HexReversi** - MVC Components、スコア表示、UI

#### 検証項目

- ✅ **必須コンポーネント確認**
  - Canvas、EventSystem、Camera、Lighting
  - MVCコンポーネント（Model/View/Controller）

- ✅ **シーン構造確認**
  - UI配置の確認
  - ゲームロジックコンポーネントの配置確認

#### 使用方法

```bash
# 基本的な使用
python validate_scenes.py

# プロジェクトパスを指定
python validate_scenes.py --project ShaderOptimizer

# 警告でも失敗とする（CI/CD用）
python validate_scenes.py --fail-on-warning
```

#### 出力例

```
=== ShaderOp Scene Validation ===
Project: D:\PersonalGameDev\ShaderOp\ShaderOptimizer
Scenes: D:\PersonalGameDev\ShaderOp\ShaderOptimizer\Assets\Scenes

[1/5] MainMenu シーン検証中...
  ✓ MainMenu シーンをチェックしました

[2/5] MainCustomization シーン検証中...
  ✓ MainCustomization シーンをチェックしました

============================================================
検証結果
============================================================

❌ エラー: 1 件
  - MainMenu: Canvas が見つかりません

⚠️  警告: 6 件
  - MainCustomization: CharacterCustomizer が見つかりません
  - TicTacToeHex: TicTacToeHexModel が見つかりません
  - HexReversi: Canvas が見つかりません（スコア、ヒントボタン用）

============================================================
```

#### Makefileターゲット

```bash
# Scene検証のみ
make validate-scenes

# 全検証（シーン + アセット + シェーダー）
make validate-all
```

---

## 🚀 CI/CD での使用

### GitHub Actions

```yaml
# .github/workflows/validate.yml
- name: Validate Assets
  run: python automation/validate_assets.py --fail-on-warning

- name: Pre-commit Check
  run: python automation/pre_commit_check.py --fail-on-warning

- name: Shader Profiling
  run: |
    python automation/shader_profiling.py \
      --export-json \
      --output shader_report.json

- name: Upload Shader Profile Report
  uses: actions/upload-artifact@v4
  with:
    name: shader-profile-report
    path: shader_report.json
```

### Jenkins

Jenkinsfile には以下のステージが含まれています:

```groovy
// Jenkinsfile
stage('Validate Assets') {
    steps {
        sh 'python3 automation/validate_assets.py --project ShaderOptimizer --fail-on-warning'
    }
}

stage('Shader Profiling') {
    steps {
        sh '''
            python3 automation/shader_profiling.py \
                --project ShaderOptimizer \
                --export-json \
                --output shader_profile_report.json
        '''
    }
    post {
        always {
            archiveArtifacts artifacts: 'shader_profile_report.json', allowEmptyArchive: true
        }
    }
}
```

---

## 📦 依存関係

すべてのスクリプトは **Python 3.8+ の標準ライブラリのみ** を使用しています。
追加のパッケージインストールは不要です。

使用している標準ライブラリ:
- `os`, `sys`, `pathlib` - ファイルシステム操作
- `json` - JSON パース
- `re` - 正規表現
- `argparse` - コマンドライン引数解析
- `shutil` - ファイル操作
- `datetime` - 日時処理
- `subprocess` - Git コマンド実行（pre_commit_check.py のみ）

---

## 🧪 テスト

スクリプトのテストを実行:

```bash
# validate_assets.py のテスト
python validate_assets.py --project ShaderOptimizer

# build_utils.py のテスト
python build_utils.py set-version 0.0.1
python build_utils.py increment --platform Android

# pre_commit_check.py のテスト
python pre_commit_check.py --files automation/README.md
```

---

## 📝 ライセンス

MIT License

---

## 🤝 コントリビューション

改善提案や不具合報告は GitHub Issues でお願いします。

---

## 📚 関連ドキュメント

- [CI/CD セットアップガイド](../docs/CICD_SETUP.md)
- [CI/CD クイックリファレンス](../docs/CICD_QUICK_REFERENCE.md)
- [プロジェクト README](../README.md)
