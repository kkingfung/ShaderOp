#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
.gitignore 検証スクリプト

セキュリティ上重要な.gitignoreパターンが含まれているか検証します:
- 機密情報を含むファイル (.env, keystoreなど)
- ビルド成果物
- 一時ファイル
- プラットフォーム固有ファイル
"""

import os
import sys
import io
import argparse
from pathlib import Path
from typing import List, Set

# セキュリティユーティリティのインポート
from security_utils import (
    PathValidator,
    SafeFileReader,
    SecurityError
)

# Windows環境でUTF-8出力を強制
if sys.platform == 'win32':
    if sys.stdout.encoding != 'utf-8':
        sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
    if sys.stderr.encoding != 'utf-8':
        sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8')


class GitignoreValidator:
    """
    .gitignore検証クラス

    セキュリティ上重要なパターンが.gitignoreに含まれているか検証します。
    """

    # 必須のセキュリティパターン（これらは必ず含まれるべき）
    REQUIRED_SECURITY_PATTERNS = {
        # 機密情報
        '.env',
        '*.keystore',
        '*.jks',
        '*_secret*',
        'unity-license.ulf',

        # 認証情報
        'credentials.json',
        'service-account*.json',
        '*.pem',
        '*.p12',

        # IDE設定（個人情報を含む可能性）
        '.idea/',
        '.vscode/settings.json',
    }

    # 推奨パターン（警告のみ）
    RECOMMENDED_PATTERNS = {
        # ビルド成果物
        '[Bb]uild/',
        '[Bb]uilds/',

        # Unity一時ファイル
        '[Ll]ibrary/',
        '[Tt]emp/',
        '[Oo]bj/',
        '[Ll]ogs/',

        # レポートファイル
        'validation_report.txt',
        'shader_profile_report.json',
        'build_report.json',
    }

    def __init__(self, repo_root: str):
        # パス検証を使用してリポジトリルートを安全に取得
        try:
            validator = PathValidator()
            self.repo_root = validator.validate_path(repo_root, must_exist=True)
        except SecurityError as e:
            print(f"❌ エラー: {e}", file=sys.stderr)
            sys.exit(1)

        self.gitignore_path = self.repo_root / ".gitignore"
        self.errors: List[str] = []
        self.warnings: List[str] = []
        self.info: List[str] = []

        # セキュリティユーティリティの初期化
        self.file_reader = SafeFileReader(max_size_mb=1)

    def validate(self) -> bool:
        """
        .gitignoreを検証する

        Returns:
            bool: エラーがない場合True
        """
        print("=== .gitignore セキュリティ検証 ===")
        print(f"Repository: {self.repo_root}")
        print(f".gitignore: {self.gitignore_path}\n")

        if not self.gitignore_path.exists():
            self.errors.append(".gitignore ファイルが見つかりません")
            self.print_results()
            return False

        try:
            # 安全なファイル読み込み
            content = self.file_reader.read_text(self.gitignore_path)
        except SecurityError as e:
            self.errors.append(f".gitignore読み込みエラー: {e}")
            self.print_results()
            return False

        # パターンを行ごとに取得（コメントと空行を除外）
        existing_patterns = self._extract_patterns(content)

        # 必須パターンのチェック
        self._check_required_patterns(existing_patterns)

        # 推奨パターンのチェック
        self._check_recommended_patterns(existing_patterns)

        self.print_results()
        return len(self.errors) == 0

    def _extract_patterns(self, content: str) -> Set[str]:
        """
        .gitignoreからパターンを抽出する

        Args:
            content: .gitignoreの内容

        Returns:
            Set[str]: パターンのセット
        """
        patterns = set()

        for line in content.split('\n'):
            # 前後の空白を削除
            line = line.strip()

            # 空行とコメントをスキップ
            if not line or line.startswith('#'):
                continue

            patterns.add(line)

        return patterns

    def _check_required_patterns(self, existing_patterns: Set[str]):
        """
        必須パターンが存在するかチェック

        Args:
            existing_patterns: 既存のパターンセット
        """
        print("[1/2] 必須セキュリティパターンをチェック中...")

        missing_patterns = self.REQUIRED_SECURITY_PATTERNS - existing_patterns

        if missing_patterns:
            for pattern in sorted(missing_patterns):
                self.errors.append(f"必須パターンが欠けています: {pattern}")
        else:
            self.info.append("すべての必須セキュリティパターンが含まれています ✓")

        print(f"  必須パターン: {len(self.REQUIRED_SECURITY_PATTERNS)} 個")
        print(f"  不足: {len(missing_patterns)} 個")

    def _check_recommended_patterns(self, existing_patterns: Set[str]):
        """
        推奨パターンが存在するかチェック

        Args:
            existing_patterns: 既存のパターンセット
        """
        print("\n[2/2] 推奨パターンをチェック中...")

        missing_patterns = self.RECOMMENDED_PATTERNS - existing_patterns

        if missing_patterns:
            for pattern in sorted(missing_patterns):
                self.warnings.append(f"推奨パターンが欠けています: {pattern}")
        else:
            self.info.append("すべての推奨パターンが含まれています ✓")

        print(f"  推奨パターン: {len(self.RECOMMENDED_PATTERNS)} 個")
        print(f"  不足: {len(missing_patterns)} 個")

    def add_missing_patterns(self, dry_run: bool = False) -> bool:
        """
        不足しているパターンを.gitignoreに追加する

        Args:
            dry_run: Trueの場合、実際には書き込まない

        Returns:
            bool: 成功した場合True
        """
        if not self.gitignore_path.exists():
            print("❌ エラー: .gitignore ファイルが見つかりません")
            return False

        try:
            # 現在の内容を読み込み
            content = self.file_reader.read_text(self.gitignore_path)
            existing_patterns = self._extract_patterns(content)

            # 不足しているパターンを取得
            missing_required = self.REQUIRED_SECURITY_PATTERNS - existing_patterns

            if not missing_required:
                print("✅ 追加するパターンはありません")
                return True

            # 新しいセクションを作成
            new_section = "\n# === セキュリティ検証により自動追加 ===\n"
            new_section += "# 機密情報を含むファイル\n"

            for pattern in sorted(missing_required):
                new_section += f"{pattern}\n"

            if dry_run:
                print("=== Dry Run: 以下の内容を追加します ===")
                print(new_section)
                return True

            # ファイルに追記
            with open(self.gitignore_path, 'a', encoding='utf-8') as f:
                f.write(new_section)

            print(f"✅ {len(missing_required)} 個のパターンを .gitignore に追加しました")
            for pattern in sorted(missing_required):
                print(f"  + {pattern}")

            return True

        except Exception as e:
            print(f"❌ エラー: {e}", file=sys.stderr)
            return False

    def print_results(self):
        """検証結果の表示"""
        print("\n" + "=" * 60)
        print("検証結果")
        print("=" * 60)

        if self.errors:
            print(f"\n❌ エラー: {len(self.errors)} 件")
            for error in self.errors:
                print(f"  - {error}")

        if self.warnings:
            print(f"\n⚠️  警告: {len(self.warnings)} 件")
            for warning in self.warnings:
                print(f"  - {warning}")

        if self.info and not self.errors:
            print(f"\n✅ 情報: {len(self.info)} 件")
            for info_msg in self.info:
                print(f"  - {info_msg}")

        if not self.errors and not self.warnings:
            print("\n✅ .gitignore は適切に設定されています")

        print("\n" + "=" * 60)


def find_repo_root() -> Path:
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

    raise RuntimeError("Gitリポジトリが見つかりません")


def main():
    """メイン関数"""
    parser = argparse.ArgumentParser(description='.gitignore セキュリティ検証ツール')
    parser.add_argument(
        '--add-missing',
        action='store_true',
        help='不足しているパターンを自動的に追加する'
    )
    parser.add_argument(
        '--dry-run',
        action='store_true',
        help='追加する内容を表示するが、実際には書き込まない'
    )
    parser.add_argument(
        '--fail-on-warning',
        action='store_true',
        help='警告がある場合も終了コード 1 で終了する'
    )

    args = parser.parse_args()

    try:
        repo_root = find_repo_root()
    except RuntimeError as e:
        print(f"❌ エラー: {e}", file=sys.stderr)
        sys.exit(1)

    validator = GitignoreValidator(str(repo_root))

    if args.add_missing:
        # パターン追加モード
        success = validator.add_missing_patterns(dry_run=args.dry_run)
        sys.exit(0 if success else 1)
    else:
        # 検証モード
        success = validator.validate()

        if not success:
            sys.exit(1)
        elif args.fail_on_warning and validator.warnings:
            sys.exit(1)
        else:
            sys.exit(0)


if __name__ == '__main__':
    main()
