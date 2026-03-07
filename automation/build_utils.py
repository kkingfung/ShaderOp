#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
ビルドユーティリティスクリプト

Unity ビルドに関連するユーティリティ機能を提供:
- ビルド番号の自動インクリメント
- バージョン管理
- ビルド成果物の整理
- ビルドレポートの生成
"""

import os
import sys
import io
import json
import shutil
import argparse
from pathlib import Path
from datetime import datetime
from typing import Dict, Any, Optional

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


class BuildUtils:
    """ビルドユーティリティクラス"""

    def __init__(self, project_path: str):
        # パス検証を使用してプロジェクトパスを安全に取得
        try:
            validator = PathValidator()
            self.project_path = validator.validate_path(project_path, must_exist=True)
        except SecurityError as e:
            print(f"❌ エラー: {e}", file=sys.stderr)
            sys.exit(1)

        self.project_settings = self.project_path / "ProjectSettings" / "ProjectSettings.asset"
        self.build_info_file = self.project_path.parent / "build_info.json"

        # セキュリティユーティリティの初期化
        self.file_reader = SafeFileReader(max_size_mb=10)
        self.path_validator = validator

    def increment_build_number(self, platform: str = "all") -> Dict[str, int]:
        """ビルド番号をインクリメント"""
        print(f"ビルド番号をインクリメント中（プラットフォーム: {platform}）...")

        build_info = self.load_build_info()

        if platform == "all":
            platforms = ["Android", "iOS", "Windows", "WebGL", "Linux"]
        else:
            platforms = [platform]

        for plt in platforms:
            current = build_info.get(f"{plt}_build_number", 0)
            build_info[f"{plt}_build_number"] = current + 1
            print(f"  {plt}: {current} -> {current + 1}")

        self.save_build_info(build_info)
        return build_info

    def set_version(self, version: str):
        """バージョンを設定"""
        print(f"バージョンを設定中: {version}")

        build_info = self.load_build_info()
        build_info["version"] = version
        build_info["last_update"] = datetime.now().isoformat()

        self.save_build_info(build_info)
        print(f"  ✓ バージョン {version} を設定しました")

    def get_version(self) -> str:
        """現在のバージョンを取得"""
        build_info = self.load_build_info()
        return build_info.get("version", "0.0.0")

    def organize_builds(self, builds_dir: str, output_dir: str):
        """ビルド成果物を整理"""
        print("ビルド成果物を整理中...")

        # パス検証
        try:
            builds_path = self.path_validator.validate_path(builds_dir, must_exist=True)
            output_path = self.path_validator.validate_path(output_dir, must_exist=False)
        except SecurityError as e:
            print(f"  ✗ パスエラー: {e}", file=sys.stderr)
            return

        if not builds_path.exists():
            print(f"  ✗ ビルドディレクトリが見つかりません: {builds_path}")
            return

        output_path.mkdir(parents=True, exist_ok=True)

        version = self.get_version()
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")

        for platform_dir in builds_path.iterdir():
            if platform_dir.is_dir():
                platform = platform_dir.name
                archive_name = f"ShaderOp_{platform}_{version}_{timestamp}"
                archive_path = output_path / archive_name

                print(f"  {platform} -> {archive_name}")

                # ZIP アーカイブを作成
                shutil.make_archive(str(archive_path), 'zip', platform_dir)

        print(f"  ✓ ビルド成果物を {output_path} に整理しました")

    def generate_build_report(self, builds_dir: str, output_file: str):
        """ビルドレポートを生成"""
        print("ビルドレポートを生成中...")

        # パス検証
        try:
            builds_path = self.path_validator.validate_path(builds_dir, must_exist=True)
            output_file_path = self.path_validator.validate_path(output_file, must_exist=False)
        except SecurityError as e:
            print(f"  ✗ パスエラー: {e}", file=sys.stderr)
            return

        report = {
            "version": self.get_version(),
            "timestamp": datetime.now().isoformat(),
            "platforms": []
        }

        if not builds_path.exists():
            print(f"  ✗ ビルドディレクトリが見つかりません: {builds_path}")
            return

        for platform_dir in builds_path.iterdir():
            if platform_dir.is_dir():
                platform = platform_dir.name
                size_mb = self.get_directory_size(platform_dir) / (1024 * 1024)

                platform_info = {
                    "name": platform,
                    "size_mb": round(size_mb, 2),
                    "files": len(list(platform_dir.rglob("*")))
                }

                report["platforms"].append(platform_info)
                print(f"  {platform}: {size_mb:.2f} MB ({platform_info['files']} ファイル)")

        # レポートをJSONで保存
        with open(output_file_path, 'w', encoding='utf-8') as f:
            json.dump(report, f, indent=2, ensure_ascii=False)

        print(f"  ✓ ビルドレポートを {output_file_path} に保存しました")

        # Markdown レポートも生成
        md_file = output_file_path.with_suffix('.md')
        self.generate_markdown_report(report, md_file)

    def generate_markdown_report(self, report: Dict[str, Any], output_file: Path):
        """Markdown形式のレポートを生成"""
        md_content = f"""# ShaderOp Build Report

**Version:** {report['version']}
**Date:** {report['timestamp']}

## Build Artifacts

| Platform | Size (MB) | Files |
|----------|-----------|-------|
"""

        for platform in report['platforms']:
            md_content += f"| {platform['name']} | {platform['size_mb']} | {platform['files']} |\n"

        total_size = sum(p['size_mb'] for p in report['platforms'])
        md_content += f"\n**Total Size:** {total_size:.2f} MB\n"

        with open(output_file, 'w', encoding='utf-8') as f:
            f.write(md_content)

        print(f"  ✓ Markdown レポートを {output_file} に保存しました")

    def load_build_info(self) -> Dict[str, Any]:
        """ビルド情報を読み込み"""
        if self.build_info_file.exists():
            with open(self.build_info_file, 'r', encoding='utf-8') as f:
                return json.load(f)
        else:
            return {
                "version": "0.0.0",
                "Android_build_number": 0,
                "iOS_build_number": 0,
                "Windows_build_number": 0,
                "WebGL_build_number": 0,
                "Linux_build_number": 0
            }

    def save_build_info(self, build_info: Dict[str, Any]):
        """ビルド情報を保存"""
        with open(self.build_info_file, 'w', encoding='utf-8') as f:
            json.dump(build_info, f, indent=2, ensure_ascii=False)

    @staticmethod
    def get_directory_size(path: Path) -> int:
        """ディレクトリのサイズを取得（バイト単位）"""
        total_size = 0
        for file in path.rglob("*"):
            if file.is_file():
                total_size += file.stat().st_size
        return total_size

    def clean_builds(self, builds_dir: str, keep_latest: int = 5):
        """古いビルドをクリーンアップ"""
        print(f"古いビルドをクリーンアップ中（最新 {keep_latest} 個を保持）...")

        # パス検証
        try:
            builds_path = self.path_validator.validate_path(builds_dir, must_exist=True)
        except SecurityError as e:
            print(f"  ✗ パスエラー: {e}", file=sys.stderr)
            return

        if not builds_path.exists():
            print(f"  ✗ ビルドディレクトリが見つかりません: {builds_path}")
            return

        # ZIPファイルを取得
        zip_files = sorted(
            builds_path.glob("*.zip"),
            key=lambda f: f.stat().st_mtime,
            reverse=True
        )

        removed_count = 0
        for zip_file in zip_files[keep_latest:]:
            print(f"  削除: {zip_file.name}")
            zip_file.unlink()
            removed_count += 1

        if removed_count == 0:
            print(f"  ✓ クリーンアップの必要はありません")
        else:
            print(f"  ✓ {removed_count} 個のビルドを削除しました")


def main():
    """メイン関数"""
    parser = argparse.ArgumentParser(description='ShaderOp ビルドユーティリティ')
    parser.add_argument(
        '--project',
        default='ShaderOptimizer',
        help='Unity プロジェクトのパス（デフォルト: ShaderOptimizer）'
    )

    subparsers = parser.add_subparsers(dest='command', help='実行するコマンド')

    # increment コマンド
    increment_parser = subparsers.add_parser('increment', help='ビルド番号をインクリメント')
    increment_parser.add_argument(
        '--platform',
        default='all',
        choices=['all', 'Android', 'iOS', 'Windows', 'WebGL', 'Linux'],
        help='対象プラットフォーム（デフォルト: all）'
    )

    # set-version コマンド
    version_parser = subparsers.add_parser('set-version', help='バージョンを設定')
    version_parser.add_argument('version', help='バージョン番号（例: 1.0.0）')

    # organize コマンド
    organize_parser = subparsers.add_parser('organize', help='ビルド成果物を整理')
    organize_parser.add_argument('--builds-dir', default='builds', help='ビルドディレクトリ')
    organize_parser.add_argument('--output-dir', default='releases', help='出力ディレクトリ')

    # report コマンド
    report_parser = subparsers.add_parser('report', help='ビルドレポートを生成')
    report_parser.add_argument('--builds-dir', default='builds', help='ビルドディレクトリ')
    report_parser.add_argument('--output', default='build_report.json', help='レポート出力ファイル')

    # clean コマンド
    clean_parser = subparsers.add_parser('clean', help='古いビルドをクリーンアップ')
    clean_parser.add_argument('--builds-dir', default='releases', help='ビルドディレクトリ')
    clean_parser.add_argument('--keep', type=int, default=5, help='保持するビルド数')

    args = parser.parse_args()

    if not args.command:
        parser.print_help()
        sys.exit(1)

    utils = BuildUtils(args.project)

    if args.command == 'increment':
        utils.increment_build_number(args.platform)
    elif args.command == 'set-version':
        utils.set_version(args.version)
    elif args.command == 'organize':
        utils.organize_builds(args.builds_dir, args.output_dir)
    elif args.command == 'report':
        utils.generate_build_report(args.builds_dir, args.output)
    elif args.command == 'clean':
        utils.clean_builds(args.builds_dir, args.keep)


if __name__ == '__main__':
    main()
