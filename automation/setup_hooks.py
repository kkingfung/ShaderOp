#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Git Hooks 自動セットアップスクリプト

Git Hooksを自動的にインストール・設定します:
- pre-commit hook: コミット前のコード品質チェック
- pre-push hook: プッシュ前の検証
- クロスプラットフォーム対応（Windows/Linux/macOS）
"""

import os
import sys
import io
import argparse
import subprocess
from pathlib import Path
from typing import Optional

# セキュリティユーティリティのインポート
from security_utils import (
    PathValidator,
    SecurityError
)

# Windows環境でUTF-8出力を強制
if sys.platform == 'win32':
    if sys.stdout.encoding != 'utf-8':
        sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
    if sys.stderr.encoding != 'utf-8':
        sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8')


class GitHooksInstaller:
    """Git Hooksインストーラークラス"""

    def __init__(self, repo_root: Path):
        # パス検証を使用してリポジトリルートを安全に取得
        try:
            validator = PathValidator()
            self.repo_root = validator.validate_path(str(repo_root), must_exist=True)
        except SecurityError as e:
            print(f"❌ エラー: {e}", file=sys.stderr)
            raise RuntimeError(f"パス検証エラー: {e}")

        self.hooks_dir = self.repo_root / ".git" / "hooks"
        self.automation_dir = self.repo_root / "automation"

        # Git リポジトリの確認
        if not (self.repo_root / ".git").exists():
            raise RuntimeError(f"Gitリポジトリが見つかりません: {self.repo_root}")

        # hooks ディレクトリの作成
        self.hooks_dir.mkdir(parents=True, exist_ok=True)

    def install_all(self):
        """すべてのフックをインストール"""
        print("=== Git Hooks セットアップ ===\n")

        self.install_pre_commit_hook()
        self.install_pre_push_hook()

        print("\n" + "=" * 60)
        print("✅ Git Hooks のインストールが完了しました")
        print("=" * 60)
        print("\n📝 使い方:")
        print("  - git commit 時に自動でコード品質チェックが実行されます")
        print("  - git push 時に自動でアセット検証が実行されます")
        print("  - フックをスキップする場合: git commit --no-verify")
        print("\n🔧 アンインストール:")
        print(f"  python {Path(__file__).name} --uninstall")

    def install_pre_commit_hook(self):
        """pre-commit フックをインストール"""
        print("[1/2] pre-commit フックをインストール中...")

        hook_path = self.hooks_dir / "pre-commit"

        # Windows と Unix で異なるシバンを使用
        if sys.platform == 'win32':
            shebang = "#!/usr/bin/env python\n"
            python_cmd = "python"
        else:
            shebang = "#!/usr/bin/env python3\n"
            python_cmd = "python3"

        hook_content = f'''{shebang}# -*- coding: utf-8 -*-
"""
Pre-commit hook - コミット前のコード品質チェック

このフックは以下のチェックを実行します:
- コーディング規約違反の検出
- TODO/FIXME コメントの検出
- 機密情報の検出
- ファイルサイズのチェック

フックをスキップする場合: git commit --no-verify
"""

import sys
import subprocess
from pathlib import Path

def main():
    """Pre-commit チェックを実行"""
    repo_root = Path(__file__).parent.parent.parent
    script_path = repo_root / "automation" / "pre_commit_check.py"

    if not script_path.exists():
        print(f"エラー: {{script_path}} が見つかりません")
        return 1

    print("🔍 Pre-commit チェック実行中...\\n")

    try:
        result = subprocess.run(
            ["{python_cmd}", str(script_path)],
            cwd=str(repo_root),
            check=False
        )

        if result.returncode != 0:
            print("\\n⚠️  コード品質チェックで問題が見つかりました")
            print("修正するか、--no-verify でスキップしてください")
            return 1

        print("\\n✅ Pre-commit チェック完了")
        return 0

    except Exception as e:
        print(f"エラー: {{e}}")
        return 1

if __name__ == "__main__":
    sys.exit(main())
'''

        # フックファイルの書き込み
        hook_path.write_text(hook_content, encoding='utf-8')

        # 実行権限の付与（Unix系のみ）
        if sys.platform != 'win32':
            hook_path.chmod(0o755)

        print(f"  ✓ pre-commit フックをインストールしました: {hook_path}")

    def install_pre_push_hook(self):
        """pre-push フックをインストール"""
        print("\n[2/2] pre-push フックをインストール中...")

        hook_path = self.hooks_dir / "pre-push"

        # Windows と Unix で異なるシバンを使用
        if sys.platform == 'win32':
            shebang = "#!/usr/bin/env python\n"
            python_cmd = "python"
        else:
            shebang = "#!/usr/bin/env python3\n"
            python_cmd = "python3"

        hook_content = f'''{shebang}# -*- coding: utf-8 -*-
"""
Pre-push hook - プッシュ前の検証

このフックは以下のチェックを実行します:
- アセット検証
- シェーダープロファイリング（警告のみ）

フックをスキップする場合: git push --no-verify
"""

import sys
import subprocess
from pathlib import Path

def main():
    """Pre-push チェックを実行"""
    repo_root = Path(__file__).parent.parent.parent
    validate_script = repo_root / "automation" / "validate_assets.py"
    shader_script = repo_root / "automation" / "shader_profiling.py"
    gitignore_script = repo_root / "automation" / "validate_gitignore.py"

    print("🔍 Pre-push チェック実行中...\\n")

    # .gitignore セキュリティ検証
    if gitignore_script.exists():
        print("🔒 .gitignore セキュリティ検証中...")
        result = subprocess.run(
            ["{python_cmd}", str(gitignore_script)],
            cwd=str(repo_root),
            check=False
        )

        if result.returncode != 0:
            print("\\n⚠️  .gitignore に必須セキュリティパターンが欠けています")
            print("修正するか、--no-verify でスキップしてください")
            print(f"\\nヒント: 自動修正するには以下を実行:")
            print(f"  {python_cmd} automation/validate_gitignore.py --add-missing")
            return 1

    # アセット検証
    if validate_script.exists():
        print("\\n📋 アセット検証中...")
        result = subprocess.run(
            ["{python_cmd}", str(validate_script), "--project", "ShaderOptimizer"],
            cwd=str(repo_root),
            check=False
        )

        if result.returncode != 0:
            print("\\n⚠️  アセット検証で問題が見つかりました")
            print("修正するか、--no-verify でスキップしてください")
            return 1

    # シェーダープロファイリング（警告のみ、エラーにしない）
    if shader_script.exists():
        print("\\n🔧 シェーダープロファイリング...")
        subprocess.run(
            ["{python_cmd}", str(shader_script), "--project", "ShaderOptimizer"],
            cwd=str(repo_root),
            check=False
        )

    print("\\n✅ Pre-push チェック完了")
    return 0

if __name__ == "__main__":
    sys.exit(main())
'''

        # フックファイルの書き込み
        hook_path.write_text(hook_content, encoding='utf-8')

        # 実行権限の付与（Unix系のみ）
        if sys.platform != 'win32':
            hook_path.chmod(0o755)

        print(f"  ✓ pre-push フックをインストールしました: {hook_path}")

    def uninstall_all(self):
        """すべてのフックをアンインストール"""
        print("=== Git Hooks アンインストール ===\n")

        hooks_to_remove = ["pre-commit", "pre-push"]
        removed_count = 0

        for hook_name in hooks_to_remove:
            hook_path = self.hooks_dir / hook_name

            if hook_path.exists():
                hook_path.unlink()
                print(f"  ✓ {hook_name} フックを削除しました")
                removed_count += 1

        if removed_count == 0:
            print("  ℹ 削除するフックが見つかりませんでした")
        else:
            print(f"\n✅ {removed_count} 個のフックを削除しました")

    def check_status(self):
        """フックのインストール状態を確認"""
        print("=== Git Hooks ステータス ===\n")

        hooks_to_check = {
            "pre-commit": "コミット前のコード品質チェック",
            "pre-push": "プッシュ前のアセット検証"
        }

        for hook_name, description in hooks_to_check.items():
            hook_path = self.hooks_dir / hook_name

            if hook_path.exists():
                # ファイルサイズを取得
                size = hook_path.stat().st_size
                print(f"✅ {hook_name}")
                print(f"   {description}")
                print(f"   場所: {hook_path}")
                print(f"   サイズ: {size} bytes\n")
            else:
                print(f"❌ {hook_name}")
                print(f"   {description}")
                print(f"   ステータス: インストールされていません\n")


def find_repo_root() -> Optional[Path]:
    """Gitリポジトリのルートディレクトリを検索"""
    current = Path.cwd()

    # 最大10階層まで遡る
    for _ in range(10):
        if (current / ".git").exists():
            return current

        parent = current.parent
        if parent == current:  # ルートディレクトリに到達
            break
        current = parent

    return None


def main():
    """メイン関数"""
    parser = argparse.ArgumentParser(description='Git Hooks 自動セットアップツール')
    parser.add_argument(
        '--install',
        action='store_true',
        help='Git Hooks をインストールする'
    )
    parser.add_argument(
        '--uninstall',
        action='store_true',
        help='Git Hooks をアンインストールする'
    )
    parser.add_argument(
        '--status',
        action='store_true',
        help='Git Hooks のインストール状態を確認する'
    )

    args = parser.parse_args()

    # リポジトリルートの検索
    repo_root = find_repo_root()

    if not repo_root:
        print("❌ エラー: Gitリポジトリが見つかりません")
        print("このスクリプトはGitリポジトリ内で実行してください")
        sys.exit(1)

    try:
        installer = GitHooksInstaller(repo_root)

        if args.uninstall:
            installer.uninstall_all()
        elif args.status:
            installer.check_status()
        elif args.install:
            installer.install_all()
        else:
            # デフォルトはインストール
            installer.install_all()

    except Exception as e:
        print(f"❌ エラー: {e}")
        sys.exit(1)


if __name__ == '__main__':
    main()
