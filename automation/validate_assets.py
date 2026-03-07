#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
アセット検証スクリプト

Unity プロジェクトのアセットを検証し、問題を検出します:
- シェーダーコンパイルエラー
- テクスチャサイズ/フォーマット検証
- アセット命名規則チェック
- メタファイルの整合性確認
"""

import os
import sys
import io
import json
import re
import argparse
from pathlib import Path
from typing import List, Dict, Any, Tuple

# セキュリティユーティリティのインポート
from security_utils import (
    PathValidator,
    SafeFileReader,
    SafeRegex,
    SecurityError
)

# Windows環境でUTF-8出力を強制
if sys.platform == 'win32':
    if sys.stdout.encoding != 'utf-8':
        sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
    if sys.stderr.encoding != 'utf-8':
        sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8')


class AssetValidator:
    """アセット検証クラス"""

    def __init__(self, project_path: str):
        # パス検証を使用してプロジェクトパスを安全に取得
        try:
            validator = PathValidator()
            self.project_path = validator.validate_path(project_path, must_exist=True)
        except SecurityError as e:
            print(f"❌ エラー: {e}", file=sys.stderr)
            sys.exit(1)

        self.assets_path = self.project_path / "Assets"
        self.errors: List[str] = []
        self.warnings: List[str] = []

        # セキュリティユーティリティの初期化
        self.file_reader = SafeFileReader(max_size_mb=50)

    def validate_all(self) -> bool:
        """すべての検証を実行"""
        print("=== ShaderOp Asset Validation ===")
        print(f"Project: {self.project_path}")

        self.validate_shader_files()
        self.validate_texture_files()
        self.validate_meta_files()
        self.validate_naming_conventions()
        self.validate_shader_graphs()

        self.print_results()

        return len(self.errors) == 0

    def validate_shader_files(self):
        """シェーダーファイルの検証"""
        print("\n[1/5] シェーダーファイル検証中...")

        shader_files = list(self.assets_path.rglob("*.shader"))

        for shader_file in shader_files:
            try:
                # 安全なファイル読み込みを使用
                content = self.file_reader.read_text(shader_file)

                # Shader 定義の存在確認（安全な正規表現を使用）
                shader_pattern = r'Shader\s+"[^"]+"'
                try:
                    if not SafeRegex.search_with_timeout(shader_pattern, content, timeout_seconds=2.0):
                        self.errors.append(f"シェーダー定義が見つかりません: {shader_file.relative_to(self.project_path)}")
                except SecurityError as e:
                    self.warnings.append(f"シェーダー検証タイムアウト: {shader_file.relative_to(self.project_path)} - {e}")
                    continue

                # SubShader の存在確認
                if "SubShader" not in content:
                    self.errors.append(f"SubShader が見つかりません: {shader_file.relative_to(self.project_path)}")

                # HLSL 構文エラーの簡易チェック（閉じ括弧の数）
                open_braces = content.count('{')
                close_braces = content.count('}')
                if open_braces != close_braces:
                    self.errors.append(f"括弧の数が一致しません: {shader_file.relative_to(self.project_path)} ('{': {open_braces}, '}': {close_braces})")

            except SecurityError as e:
                self.errors.append(f"シェーダーファイル読み込みエラー: {shader_file.relative_to(self.project_path)} - {e}")

        print(f"  ✓ {len(shader_files)} 個のシェーダーファイルをチェックしました")

    def validate_texture_files(self):
        """テクスチャファイルの検証"""
        print("\n[2/5] テクスチャファイル検証中...")

        texture_extensions = ['.png', '.jpg', '.jpeg', '.tga', '.psd']
        texture_files = []

        for ext in texture_extensions:
            texture_files.extend(self.assets_path.rglob(f"*{ext}"))

        large_textures = []
        for texture_file in texture_files:
            size_mb = texture_file.stat().st_size / (1024 * 1024)

            # 10MB 以上のテクスチャに警告
            if size_mb > 10:
                large_textures.append((texture_file, size_mb))
                self.warnings.append(f"大きなテクスチャ: {texture_file.relative_to(self.project_path)} ({size_mb:.2f} MB)")

            # 命名規則チェック（T_ プレフィックス推奨）
            if not texture_file.stem.startswith('T_') and not texture_file.stem.startswith('tex_'):
                # Assets/Textures 配下のみ警告
                if 'Textures' in str(texture_file):
                    self.warnings.append(f"テクスチャ命名規則違反（T_ または tex_ で始めることを推奨）: {texture_file.name}")

        print(f"  ✓ {len(texture_files)} 個のテクスチャファイルをチェックしました")
        if large_textures:
            print(f"  ⚠ {len(large_textures)} 個の大きなテクスチャが見つかりました")

    def validate_meta_files(self):
        """メタファイルの整合性確認"""
        print("\n[3/5] メタファイル検証中...")

        asset_files = []
        for ext in ['.cs', '.shader', '.shadergraph', '.mat', '.prefab', '.unity', '.png', '.jpg']:
            asset_files.extend(self.assets_path.rglob(f"*{ext}"))

        missing_meta = []
        for asset_file in asset_files:
            meta_file = Path(str(asset_file) + ".meta")
            if not meta_file.exists():
                missing_meta.append(asset_file)
                self.errors.append(f"メタファイルが見つかりません: {asset_file.relative_to(self.project_path)}")

        # 孤立したメタファイルのチェック
        orphaned_meta = []
        meta_files = list(self.assets_path.rglob("*.meta"))
        for meta_file in meta_files:
            original_file = Path(str(meta_file)[:-5])  # .meta を削除
            if not original_file.exists():
                orphaned_meta.append(meta_file)
                self.warnings.append(f"孤立したメタファイル: {meta_file.relative_to(self.project_path)}")

        print(f"  ✓ {len(asset_files)} 個のアセットのメタファイルをチェックしました")
        if missing_meta:
            print(f"  ✗ {len(missing_meta)} 個のメタファイルが見つかりません")
        if orphaned_meta:
            print(f"  ⚠ {len(orphaned_meta)} 個の孤立したメタファイルが見つかりました")

    def validate_naming_conventions(self):
        """命名規則の検証"""
        print("\n[4/5] 命名規則検証中...")

        # スクリプトファイルの命名規則
        script_files = list(self.assets_path.rglob("*.cs"))

        for script_file in script_files:
            filename = script_file.stem

            # PascalCase チェック（簡易版）
            if not filename[0].isupper():
                self.warnings.append(f"スクリプト命名規則違反（PascalCase推奨）: {script_file.name}")

            # スペースを含むファイル名
            if ' ' in filename:
                self.errors.append(f"ファイル名にスペースが含まれています: {script_file.name}")

        print(f"  ✓ {len(script_files)} 個のスクリプトファイルをチェックしました")

    def validate_shader_graphs(self):
        """Shader Graph ファイルの検証"""
        print("\n[5/5] Shader Graph 検証中...")

        shadergraph_files = list(self.assets_path.rglob("*.shadergraph"))

        for sg_file in shadergraph_files:
            try:
                # 安全なJSON読み込みを使用（深さ制限あり）
                sg_data = self.file_reader.read_json(sg_file, max_depth=30)

                # 基本構造の確認
                content_str = json.dumps(sg_data)
                if "m_SerializedProperties" not in content_str and "m_Nodes" not in content_str:
                    self.warnings.append(f"Shader Graph の構造が不正な可能性: {sg_file.relative_to(self.project_path)}")

            except SecurityError as e:
                self.errors.append(f"Shader Graph 読み込みエラー: {sg_file.relative_to(self.project_path)} - {e}")
            except Exception as e:
                self.warnings.append(f"Shader Graph 読み込みエラー: {sg_file.relative_to(self.project_path)} - {str(e)}")

        print(f"  ✓ {len(shadergraph_files)} 個の Shader Graph をチェックしました")

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

        if not self.errors and not self.warnings:
            print("\n✅ 問題は見つかりませんでした")

        print("\n" + "=" * 60)


def main():
    """メイン関数"""
    parser = argparse.ArgumentParser(description='ShaderOp アセット検証ツール')
    parser.add_argument(
        '--project',
        default='ShaderOptimizer',
        help='Unity プロジェクトのパス（デフォルト: ShaderOptimizer）'
    )
    parser.add_argument(
        '--fail-on-warning',
        action='store_true',
        help='警告がある場合も終了コード 1 で終了する'
    )

    args = parser.parse_args()

    validator = AssetValidator(args.project)
    success = validator.validate_all()

    # 終了コードの決定
    if not success:
        sys.exit(1)
    elif args.fail_on_warning and validator.warnings:
        sys.exit(1)
    else:
        sys.exit(0)


if __name__ == '__main__':
    main()
