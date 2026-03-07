#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
プリコミットチェックスクリプト

コミット前に自動実行されるチェック:
- コーディング規約違反の検出
- TODO/FIXME コメントの検出
- 機密情報の検出
- ファイルサイズのチェック
"""

import os
import sys
import io
import re
from pathlib import Path
from typing import List, Tuple

# セキュリティユーティリティのインポート
from security_utils import (
    PathValidator,
    SafeFileReader,
    SafeRegex,
    SensitiveDataMasker,
    SecurityError
)

# Windows環境でUTF-8出力を強制
if sys.platform == 'win32':
    if sys.stdout.encoding != 'utf-8':
        sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
    if sys.stderr.encoding != 'utf-8':
        sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8')


class PreCommitChecker:
    """プリコミットチェッククラス"""

    def __init__(self, project_root: str):
        # パス検証を使用してプロジェクトルートを安全に取得
        try:
            validator = PathValidator()
            self.project_root = validator.validate_path(project_root, must_exist=True)
        except SecurityError as e:
            print(f"❌ エラー: {e}", file=sys.stderr)
            sys.exit(1)

        self.errors: List[str] = []
        self.warnings: List[str] = []

        # セキュリティユーティリティの初期化
        self.file_reader = SafeFileReader(max_size_mb=10)
        self.path_validator = validator

        # チェック対象の拡張子
        self.code_extensions = ['.cs', '.shader', '.py', '.js']

        # 機密情報パターン（SensitiveDataMaskerを活用）
        self.secret_patterns = [
            (r'password\s*=\s*["\'][^"\']+["\']', "パスワードが含まれている可能性"),
            (r'api[_-]?key\s*=\s*["\'][^"\']+["\']', "APIキーが含まれている可能性"),
            (r'secret\s*=\s*["\'][^"\']+["\']', "シークレットキーが含まれている可能性"),
            (r'Bearer\s+[A-Za-z0-9\-._~+/]+=*', "Bearer トークンが含まれている可能性"),
        ]

    def check_all(self, files: List[str]) -> bool:
        """すべてのチェックを実行"""
        print("=== Pre-commit Checks ===\n")

        if not files:
            print("チェック対象のファイルがありません")
            return True

        for file_path in files:
            self.check_file(file_path)

        self.print_results()
        return len(self.errors) == 0

    def check_file(self, file_path: str):
        """個別ファイルのチェック"""
        # パス検証
        try:
            file = self.path_validator.validate_path(file_path, must_exist=True)
        except SecurityError as e:
            self.errors.append(f"{file_path}: パス検証エラー - {e}")
            return

        if not file.exists():
            return

        # コードファイルのみチェック
        if file.suffix not in self.code_extensions:
            return

        print(f"チェック中: {file.name}")

        try:
            # 安全なファイル読み込み
            content = self.file_reader.read_text(file)
            lines = content.split('\n')

            self.check_coding_style(file, content, lines)
            self.check_secrets(file, content, lines)
            self.check_todos(file, content, lines)
            self.check_file_size(file)

        except SecurityError as e:
            self.errors.append(f"{file.name}: ファイル読み込みエラー - {e}")
        except Exception as e:
            self.warnings.append(f"{file.name}: ファイル読み込みエラー - {str(e)}")

    def check_coding_style(self, file: Path, content: str, lines: List[str]):
        """コーディング規約チェック"""

        # C# ファイルの場合
        if file.suffix == '.cs':
            # #nullable enable の確認
            if '#nullable enable' not in content:
                self.warnings.append(f"{file.name}: #nullable enable ディレクティブがありません")

            # Debug.Log の残存チェック（本番コードでは警告）
            if 'Debug.Log(' in content and 'Tests' not in str(file):
                debug_log_count = content.count('Debug.Log(')
                self.warnings.append(f"{file.name}: Debug.Log が {debug_log_count} 箇所あります（削除推奨）")

        # Python ファイルの場合
        elif file.suffix == '.py':
            # shebang の確認
            if not lines[0].startswith('#!'):
                self.warnings.append(f"{file.name}: shebang が見つかりません")

        # タブ文字のチェック（スペースインデント推奨）
        if '\t' in content and file.suffix in ['.py', '.js']:
            self.warnings.append(f"{file.name}: タブ文字が含まれています（スペースインデント推奨）")

        # 行末の空白チェック
        for i, line in enumerate(lines, 1):
            if line.endswith(' ') or line.endswith('\t'):
                self.warnings.append(f"{file.name}:{i}: 行末に空白があります")

    def check_secrets(self, file: Path, content: str, lines: List[str]):
        """機密情報チェック"""

        for pattern, message in self.secret_patterns:
            try:
                # 安全な正規表現実行（タイムアウト付き）
                matches = SafeRegex.finditer_with_timeout(pattern, content, timeout_seconds=2.0, flags=re.IGNORECASE)
                for match in matches:
                    line_num = content[:match.start()].count('\n') + 1
                    # マッチした内容をマスキングして表示
                    masked_match = SensitiveDataMasker.mask(match.group())
                    self.errors.append(f"{file.name}:{line_num}: {message} - {masked_match}")
            except SecurityError as e:
                self.warnings.append(f"{file.name}: 機密情報チェックタイムアウト - {e}")

    def check_todos(self, file: Path, content: str, lines: List[str]):
        """TODO/FIXMEコメントの検出"""

        for i, line in enumerate(lines, 1):
            # TODO コメント
            if 'TODO' in line:
                self.warnings.append(f"{file.name}:{i}: TODO コメントがあります: {line.strip()}")

            # FIXME コメント
            if 'FIXME' in line:
                self.errors.append(f"{file.name}:{i}: FIXME コメントがあります: {line.strip()}")

            # HACK コメント
            if 'HACK' in line:
                self.warnings.append(f"{file.name}:{i}: HACK コメントがあります: {line.strip()}")

    def check_file_size(self, file: Path):
        """ファイルサイズチェック"""

        size_mb = file.stat().st_size / (1024 * 1024)

        # 1MB 以上のコードファイルは警告
        if size_mb > 1:
            self.warnings.append(f"{file.name}: ファイルサイズが大きすぎます ({size_mb:.2f} MB)")

        # 5MB 以上はエラー
        if size_mb > 5:
            self.errors.append(f"{file.name}: ファイルサイズが大きすぎます ({size_mb:.2f} MB) - 分割を検討してください")

    def print_results(self):
        """チェック結果の表示"""
        print("\n" + "=" * 60)
        print("チェック結果")
        print("=" * 60)

        if self.errors:
            print(f"\n❌ エラー: {len(self.errors)} 件")
            for error in self.errors:
                print(f"  - {error}")

        if self.warnings:
            print(f"\n⚠️  警告: {len(self.warnings)} 件")
            for warning in self.warnings:
                print(f"  - {warning}")

        if not self.errors and not self.warnings:
            print("\n✅ 問題は見つかりませんでした")

        print("\n" + "=" * 60)


def get_staged_files() -> List[str]:
    """ステージングされたファイルのリストを取得"""
    import subprocess

    try:
        result = subprocess.run(
            ['git', 'diff', '--cached', '--name-only', '--diff-filter=ACM'],
            capture_output=True,
            text=True,
            check=True
        )
        return result.stdout.strip().split('\n') if result.stdout.strip() else []
    except subprocess.CalledProcessError:
        print("⚠️ Git コマンドの実行に失敗しました")
        return []


def main():
    """メイン関数"""
    import argparse

    parser = argparse.ArgumentParser(description='プリコミットチェック')
    parser.add_argument(
        '--files',
        nargs='*',
        help='チェックするファイル（指定しない場合はステージングされたファイル）'
    )
    parser.add_argument(
        '--fail-on-warning',
        action='store_true',
        help='警告がある場合も終了コード 1 で終了する'
    )

    args = parser.parse_args()

    # チェック対象ファイルの決定
    files = args.files if args.files else get_staged_files()

    if not files:
        print("チェック対象のファイルがありません")
        sys.exit(0)

    project_root = Path(__file__).parent.parent
    checker = PreCommitChecker(str(project_root))

    success = checker.check_all(files)

    # 終了コードの決定
    if not success:
        sys.exit(1)
    elif args.fail_on_warning and checker.warnings:
        sys.exit(1)
    else:
        sys.exit(0)


if __name__ == '__main__':
    main()
