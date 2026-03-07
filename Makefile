# ShaderOp Makefile
#
# よく使うコマンドを簡単に実行できるようにするMakefile
# Windows では `make` コマンドが必要（Git Bash または WSL 推奨）

.PHONY: help validate test build clean setup deploy

# デフォルトターゲット
.DEFAULT_GOAL := help

# 変数定義
PYTHON := python
UNITY_PROJECT := ShaderOptimizer
BUILDS_DIR := builds
RELEASES_DIR := releases

# カラー出力
CYAN := \033[0;36m
GREEN := \033[0;32m
YELLOW := \033[0;33m
RED := \033[0;31m
NC := \033[0m # No Color

##@ ヘルプ

help: ## このヘルプメッセージを表示
	@echo "$(CYAN)ShaderOp Makefile$(NC)"
	@echo ""
	@echo "$(GREEN)利用可能なターゲット:$(NC)"
	@awk 'BEGIN {FS = ":.*##"; printf ""} /^[a-zA-Z_-]+:.*?##/ { printf "  $(CYAN)%-15s$(NC) %s\n", $$1, $$2 } /^##@/ { printf "\n$(YELLOW)%s$(NC)\n", substr($$0, 5) } ' $(MAKEFILE_LIST)

##@ 開発

setup: ## プロジェクトの初期セットアップ
	@echo "$(GREEN)プロジェクトをセットアップ中...$(NC)"
	@$(PYTHON) --version
	@echo "$(GREEN)✓ Python インストール確認完了$(NC)"
	@echo "$(YELLOW)Unity プロジェクトは Unity Hub から開いてください$(NC)"

validate: ## アセットを検証
	@echo "$(GREEN)アセットを検証中...$(NC)"
	@$(PYTHON) automation/validate_assets.py --project $(UNITY_PROJECT)

validate-strict: ## アセットを厳格に検証（警告もエラー扱い）
	@echo "$(GREEN)アセットを厳格に検証中...$(NC)"
	@$(PYTHON) automation/validate_assets.py --project $(UNITY_PROJECT) --fail-on-warning

precommit: ## プリコミットチェックを実行
	@echo "$(GREEN)プリコミットチェック実行中...$(NC)"
	@$(PYTHON) automation/pre_commit_check.py

test: ## Unity テストを実行（手動）
	@echo "$(YELLOW)Unity Editor から Window > General > Test Runner を開いてテストを実行してください$(NC)"

##@ バージョン管理

version: ## 現在のバージョンを表示
	@echo "$(GREEN)現在のバージョン:$(NC)"
	@$(PYTHON) automation/build_utils.py --project $(UNITY_PROJECT) set-version 0.0.0 2>/dev/null || true
	@if [ -f "build_info.json" ]; then \
		$(PYTHON) -c "import json; data=json.load(open('build_info.json')); print('  Version:', data.get('version', 'Not set'))"; \
	else \
		echo "  Version: Not set (build_info.json not found)"; \
	fi

set-version: ## バージョンを設定（使い方: make set-version VERSION=1.0.0）
	@if [ -z "$(VERSION)" ]; then \
		echo "$(RED)エラー: VERSION を指定してください$(NC)"; \
		echo "使用例: make set-version VERSION=1.0.0"; \
		exit 1; \
	fi
	@echo "$(GREEN)バージョンを $(VERSION) に設定中...$(NC)"
	@$(PYTHON) automation/build_utils.py --project $(UNITY_PROJECT) set-version $(VERSION)

increment: ## ビルド番号をインクリメント（使い方: make increment PLATFORM=Android）
	@if [ -z "$(PLATFORM)" ]; then \
		PLATFORM=all; \
	fi; \
	echo "$(GREEN)ビルド番号をインクリメント中 ($(PLATFORM))...$(NC)"; \
	$(PYTHON) automation/build_utils.py --project $(UNITY_PROJECT) increment --platform $(PLATFORM)

##@ ビルド

build-info: ## ビルド情報を表示
	@echo "$(GREEN)ビルド情報:$(NC)"
	@if [ -f "build_info.json" ]; then \
		cat build_info.json | $(PYTHON) -m json.tool; \
	else \
		echo "$(YELLOW)build_info.json が見つかりません$(NC)"; \
	fi

organize: ## ビルド成果物を整理
	@echo "$(GREEN)ビルド成果物を整理中...$(NC)"
	@$(PYTHON) automation/build_utils.py --project $(UNITY_PROJECT) organize \
		--builds-dir $(BUILDS_DIR) \
		--output-dir $(RELEASES_DIR)

report: ## ビルドレポートを生成
	@echo "$(GREEN)ビルドレポートを生成中...$(NC)"
	@$(PYTHON) automation/build_utils.py --project $(UNITY_PROJECT) report \
		--builds-dir $(BUILDS_DIR) \
		--output build_report.json
	@echo "$(GREEN)✓ build_report.json と build_report.md を生成しました$(NC)"

clean-builds: ## 古いビルドをクリーンアップ
	@echo "$(GREEN)古いビルドをクリーンアップ中...$(NC)"
	@$(PYTHON) automation/build_utils.py --project $(UNITY_PROJECT) clean \
		--builds-dir $(RELEASES_DIR) \
		--keep 5

##@ Git

status: ## Git ステータスを表示
	@git status

commit: ## 変更をコミット（使い方: make commit MSG="commit message"）
	@if [ -z "$(MSG)" ]; then \
		echo "$(RED)エラー: MSG を指定してください$(NC)"; \
		echo "使用例: make commit MSG=\"feat: 新機能を追加\""; \
		exit 1; \
	fi
	@echo "$(GREEN)プリコミットチェック実行中...$(NC)"
	@$(PYTHON) automation/pre_commit_check.py || true
	@echo "$(GREEN)変更をコミット中...$(NC)"
	@git add .
	@git commit -m "$(MSG)"

push: ## 変更をプッシュ
	@echo "$(GREEN)変更をプッシュ中...$(NC)"
	@git push

release: ## リリースタグを作成（使い方: make release VERSION=1.0.0）
	@if [ -z "$(VERSION)" ]; then \
		echo "$(RED)エラー: VERSION を指定してください$(NC)"; \
		echo "使用例: make release VERSION=1.0.0"; \
		exit 1; \
	fi
	@echo "$(GREEN)バージョンを設定中...$(NC)"
	@$(PYTHON) automation/build_utils.py --project $(UNITY_PROJECT) set-version $(VERSION)
	@echo "$(GREEN)変更をコミット中...$(NC)"
	@git add build_info.json
	@git commit -m "chore: Bump version to $(VERSION)" || true
	@git push
	@echo "$(GREEN)タグを作成中...$(NC)"
	@git tag -a v$(VERSION) -m "Release $(VERSION)"
	@git push origin v$(VERSION)
	@echo "$(GREEN)✓ リリース v$(VERSION) を作成しました$(NC)"
	@echo "$(YELLOW)GitHub Actions が自動でビルド&デプロイを開始します$(NC)"

##@ クリーンアップ

clean: ## ビルド成果物を削除
	@echo "$(GREEN)ビルド成果物を削除中...$(NC)"
	@rm -rf $(BUILDS_DIR)
	@rm -rf $(RELEASES_DIR)
	@echo "$(GREEN)✓ クリーンアップ完了$(NC)"

clean-unity: ## Unity の一時ファイルを削除（注意: Unity を閉じてから実行）
	@echo "$(RED)警告: Unity を閉じてから実行してください$(NC)"
	@read -p "続行しますか？ (y/N): " confirm; \
	if [ "$$confirm" = "y" ] || [ "$$confirm" = "Y" ]; then \
		echo "$(GREEN)Unity 一時ファイルを削除中...$(NC)"; \
		rm -rf $(UNITY_PROJECT)/Library; \
		rm -rf $(UNITY_PROJECT)/Temp; \
		rm -rf $(UNITY_PROJECT)/Logs; \
		echo "$(GREEN)✓ Unity 一時ファイルを削除しました$(NC)"; \
	else \
		echo "$(YELLOW)キャンセルしました$(NC)"; \
	fi

##@ CI/CD

ci-test: ## CI/CD テストをローカルで実行
	@echo "$(GREEN)CI/CD テストをローカルで実行中...$(NC)"
	@$(MAKE) validate-strict
	@$(MAKE) precommit

ci-info: ## CI/CD ワークフロー情報を表示
	@echo "$(GREEN)GitHub Actions ワークフロー:$(NC)"
	@echo "  - test.yml: 自動テスト実行"
	@echo "  - build.yml: マルチプラットフォームビルド"
	@echo "  - deploy.yml: 自動デプロイ"
	@echo "  - code-quality.yml: コード品質チェック"
	@echo ""
	@echo "$(GREEN)詳細は docs/CICD_SETUP.md を参照してください$(NC)"

##@ 自動化（新規追加）

validate-all: ## すべての検証を実行（シーン + アセット + シェーダープロファイリング）
	@echo "$(GREEN)🔍 シーン検証...$(NC)"
	@$(PYTHON) automation/validate_scenes.py --project $(UNITY_PROJECT) || true
	@echo ""
	@echo "$(GREEN)🔍 アセット検証...$(NC)"
	@$(PYTHON) automation/validate_assets.py --project $(UNITY_PROJECT)
	@echo ""
	@echo "$(GREEN)🔍 シェーダープロファイリング...$(NC)"
	@$(PYTHON) automation/shader_profiling.py --project $(UNITY_PROJECT)
	@echo ""
	@echo "$(GREEN)✅ すべての検証完了$(NC)"

validate-scenes: ## Unity Scene検証（Phase 2用）
	@echo "$(GREEN)Unity Scene検証中...$(NC)"
	@$(PYTHON) automation/validate_scenes.py --project $(UNITY_PROJECT)

shader-profile: ## シェーダープロファイリングを実行
	@echo "$(GREEN)シェーダープロファイリング実行中...$(NC)"
	@$(PYTHON) automation/shader_profiling.py --project $(UNITY_PROJECT)

shader-profile-json: ## シェーダープロファイリングを実行してJSONレポート出力
	@echo "$(GREEN)シェーダープロファイリング実行中（JSONレポート出力）...$(NC)"
	@$(PYTHON) automation/shader_profiling.py \
		--project $(UNITY_PROJECT) \
		--export-json \
		--output shader_profile_report.json
	@echo "$(GREEN)✓ shader_profile_report.json を生成しました$(NC)"

setup-hooks: ## Git Hooks を自動インストール
	@echo "$(GREEN)Git Hooks をインストール中...$(NC)"
	@$(PYTHON) automation/setup_hooks.py --install

hooks-status: ## Git Hooks のインストール状態を確認
	@$(PYTHON) automation/setup_hooks.py --status

hooks-uninstall: ## Git Hooks をアンインストール
	@echo "$(YELLOW)Git Hooks をアンインストール中...$(NC)"
	@$(PYTHON) automation/setup_hooks.py --uninstall

setup-dev: ## 開発環境セットアップ（Git Hooks インストール）
	@echo "$(GREEN)開発環境をセットアップ中...$(NC)"
	@$(PYTHON) automation/setup_hooks.py --install
	@echo "$(GREEN)✅ 開発環境セットアップ完了$(NC)"
	@echo "$(CYAN)ℹ  git commit 時に自動でコード品質チェックが実行されます$(NC)"
	@echo "$(CYAN)ℹ  git push 時に自動でアセット検証が実行されます$(NC)"

##@ ドキュメント

docs: ## ドキュメントを開く
	@echo "$(GREEN)ドキュメント一覧:$(NC)"
	@echo "  - README.md: プロジェクト概要"
	@echo "  - CLAUDE.md: Claude Code 使用ガイド"
	@echo "  - TROUBLESHOOTING.md: トラブルシューティングガイド"
	@echo "  - DOCUMENTATION_AUDIT.md: ドキュメント監査レポート"
	@echo "  - docs/GETTING_STARTED.md: クイックスタートガイド"
	@echo "  - docs/ARCHITECTURE.md: アーキテクチャドキュメント"
	@echo "  - docs/BEST_PRACTICES.md: ベストプラクティス"
	@echo "  - docs/tutorials/: チュートリアルシリーズ"
	@echo "  - automation/README.md: 自動化スクリプトドキュメント"

docs-generate: ## API リファレンスを生成（DocFX使用）
	@echo "$(GREEN)API リファレンスを生成中...$(NC)"
	@if command -v docfx > /dev/null; then \
		docfx .docfx/docfx.json; \
		echo "$(GREEN)✓ API リファレンスを生成しました: .docfx/_site/$(NC)"; \
	else \
		echo "$(RED)エラー: docfx がインストールされていません$(NC)"; \
		echo "$(YELLOW)インストール: dotnet tool install -g docfx$(NC)"; \
		exit 1; \
	fi

docs-serve: ## API リファレンスをローカルサーバーで表示
	@echo "$(GREEN)API リファレンスサーバーを起動中...$(NC)"
	@if command -v docfx > /dev/null; then \
		docfx .docfx/docfx.json --serve; \
	else \
		echo "$(RED)エラー: docfx がインストールされていません$(NC)"; \
		echo "$(YELLOW)インストール: dotnet tool install -g docfx$(NC)"; \
		exit 1; \
	fi

docs-validate: ## ドキュメントリンク切れをチェック
	@echo "$(GREEN)ドキュメントリンク切れをチェック中...$(NC)"
	@$(PYTHON) automation/validate_docs.py

##@ その他

all: validate precommit ## すべてのチェックを実行
	@echo "$(GREEN)✓ すべてのチェックが完了しました$(NC)"

quick-release: ## クイックリリース（バージョン設定、コミット、タグ作成を一度に実行）
	@if [ -z "$(VERSION)" ]; then \
		echo "$(RED)エラー: VERSION を指定してください$(NC)"; \
		echo "使用例: make quick-release VERSION=1.0.0"; \
		exit 1; \
	fi
	@$(MAKE) validate-strict
	@$(MAKE) set-version VERSION=$(VERSION)
	@$(MAKE) release VERSION=$(VERSION)
