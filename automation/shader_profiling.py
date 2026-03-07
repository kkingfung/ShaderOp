#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
シェーダープロファイリングスクリプト

Unity プロジェクトのシェーダーを分析し、パフォーマンス問題を検出します:
- シェーダーバリアント数の推定
- Pass数のカウント（多すぎると警告）
- プラグマディレクティブの検証
- 複雑度の分析
- モバイル最適化チェック
"""

import os
import sys
import io
import json
import re
import argparse
from pathlib import Path
from typing import List, Dict, Any, Tuple
from collections import defaultdict

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


class ShaderProfiler:
    """シェーダープロファイリングクラス"""

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
        self.stats: Dict[str, Any] = {}

        # セキュリティユーティリティの初期化
        self.file_reader = SafeFileReader(max_size_mb=50)
        self.path_validator = validator

    def profile_all(self) -> bool:
        """すべてのシェーダー分析を実行"""
        print("=== ShaderOp Shader Profiling ===")
        print(f"Project: {self.project_path}\n")

        self.profile_shader_files()
        self.profile_shader_graphs()
        self.analyze_shader_complexity()
        self.check_mobile_optimizations()

        self.print_results()
        self.print_statistics()

        return len(self.errors) == 0

    def profile_shader_files(self):
        """シェーダーファイルのプロファイリング"""
        print("[1/4] シェーダーファイル分析中...")

        shader_files = list(self.assets_path.rglob("*.shader"))

        total_passes = 0
        total_pragmas = 0
        complex_shaders = []

        for shader_file in shader_files:
            try:
                # 安全なファイル読み込み
                content = self.file_reader.read_text(shader_file)

                # Pass数のカウント
                pass_count = content.count('Pass {') + content.count('Pass{')
                total_passes += pass_count
            except SecurityError as e:
                self.errors.append(f"シェーダーファイル読み込みエラー: {shader_file.relative_to(self.assets_path)} - {e}")
                continue

            # 多すぎるPass数に警告
            if pass_count > 3:
                self.warnings.append(
                    f"Pass数が多い: {shader_file.relative_to(self.assets_path)} ({pass_count} passes)"
                )
                complex_shaders.append({
                    'name': shader_file.name,
                    'passes': pass_count,
                    'path': str(shader_file.relative_to(self.assets_path))
                })

            # プラグマディレクティブのカウント（安全な正規表現使用）
            try:
                pragma_matches = SafeRegex.finditer_with_timeout(r'#pragma\s+', content, timeout_seconds=2.0)
                pragmas = len(pragma_matches)
                total_pragmas += pragmas

                # multi_compileの検出（バリアント爆発の可能性）
                multi_compile_matches = SafeRegex.finditer_with_timeout(r'#pragma\s+multi_compile\s+(\S+.*)', content, timeout_seconds=2.0)
                multi_compiles = [m.group() for m in multi_compile_matches]
            except SecurityError as e:
                self.warnings.append(f"シェーダー分析タイムアウト: {shader_file.relative_to(self.assets_path)} - {e}")
                continue
            if len(multi_compiles) > 5:
                variant_estimate = 2 ** len(multi_compiles)
                self.warnings.append(
                    f"大量のシェーダーバリアント: {shader_file.relative_to(self.assets_path)} "
                    f"(multi_compile: {len(multi_compiles)}, 推定バリアント数: ~{variant_estimate})"
                )

            # shader_feature vs multi_compile の推奨チェック（安全な正規表現使用）
            try:
                shader_feature_matches = SafeRegex.finditer_with_timeout(r'#pragma\s+shader_feature\s+', content, timeout_seconds=2.0)
                shader_features = len(shader_feature_matches)
            except SecurityError:
                shader_features = 0
            multi_compile_count = len(multi_compiles)

            if multi_compile_count > shader_features and multi_compile_count > 3:
                self.warnings.append(
                    f"multi_compile の過剰使用: {shader_file.relative_to(self.assets_path)} "
                    f"(shader_feature の使用を検討してください)"
                )

            # #include の深さチェック（安全な正規表現使用）
            try:
                include_matches = SafeRegex.finditer_with_timeout(r'#include\s+"([^"]+)"', content, timeout_seconds=2.0)
                includes = [m.group() for m in include_matches]
                if len(includes) > 10:
                    self.warnings.append(
                        f"インクルードファイルが多い: {shader_file.relative_to(self.assets_path)} ({len(includes)} includes)"
                    )
            except SecurityError:
                pass

        self.stats['shader_files'] = len(shader_files)
        self.stats['total_passes'] = total_passes
        self.stats['total_pragmas'] = total_pragmas
        self.stats['avg_passes_per_shader'] = round(total_passes / len(shader_files), 2) if shader_files else 0
        self.stats['complex_shaders'] = complex_shaders

        print(f"  ✓ {len(shader_files)} 個のシェーダーファイルを分析しました")
        print(f"  ℹ 総Pass数: {total_passes}")
        print(f"  ℹ 平均Pass数/シェーダー: {self.stats['avg_passes_per_shader']}")

    def profile_shader_graphs(self):
        """Shader Graph ファイルのプロファイリング"""
        print("\n[2/4] Shader Graph 分析中...")

        shadergraph_files = list(self.assets_path.rglob("*.shadergraph"))

        total_nodes = 0
        complex_graphs = []

        for sg_file in shadergraph_files:
            try:
                # 安全なファイル読み込み
                content = self.file_reader.read_text(sg_file)

                # ノード数の推定（m_Nodesの出現回数）
                node_count = content.count('"m_Type":')
                total_nodes += node_count
            except SecurityError as e:
                self.errors.append(f"Shader Graph 読み込みエラー: {sg_file.relative_to(self.assets_path)} - {e}")
                continue

            if node_count > 50:
                self.warnings.append(
                    f"複雑なShader Graph: {sg_file.relative_to(self.assets_path)} ({node_count} nodes)"
                )
                complex_graphs.append({
                    'name': sg_file.name,
                    'nodes': node_count,
                    'path': str(sg_file.relative_to(self.assets_path))
                })

            # サンプリング回数のチェック（パフォーマンス影響大）
            sample_count = content.count('"SampleTexture2D"')
            if sample_count > 8:
                self.warnings.append(
                    f"テクスチャサンプリングが多い: {sg_file.relative_to(self.assets_path)} ({sample_count} samples)"
                )

        self.stats['shader_graphs'] = len(shadergraph_files)
        self.stats['total_nodes'] = total_nodes
        self.stats['avg_nodes_per_graph'] = round(total_nodes / len(shadergraph_files), 2) if shadergraph_files else 0
        self.stats['complex_graphs'] = complex_graphs

        print(f"  ✓ {len(shadergraph_files)} 個のShader Graphを分析しました")
        print(f"  ℹ 総ノード数: {total_nodes}")
        print(f"  ℹ 平均ノード数/グラフ: {self.stats['avg_nodes_per_graph']}")

    def analyze_shader_complexity(self):
        """シェーダーの複雑度分析"""
        print("\n[3/4] 複雑度分析中...")

        shader_files = list(self.assets_path.rglob("*.shader"))

        complexity_stats = {
            'simple': 0,    # ~50行以下
            'moderate': 0,  # 51-200行
            'complex': 0,   # 201-500行
            'very_complex': 0  # 500行以上
        }

        for shader_file in shader_files:
            try:
                # 安全なファイル読み込み
                content = self.file_reader.read_text(shader_file)
                line_count = len(content.splitlines())
            except SecurityError:
                continue

            if line_count <= 50:
                complexity_stats['simple'] += 1
            elif line_count <= 200:
                complexity_stats['moderate'] += 1
            elif line_count <= 500:
                complexity_stats['complex'] += 1
            else:
                complexity_stats['very_complex'] += 1
                self.warnings.append(
                    f"非常に大きなシェーダー: {shader_file.relative_to(self.assets_path)} ({line_count} 行)"
                )

        self.stats['complexity'] = complexity_stats

        print(f"  ✓ 複雑度分析完了")
        print(f"    - シンプル (<50行): {complexity_stats['simple']}")
        print(f"    - 中程度 (51-200行): {complexity_stats['moderate']}")
        print(f"    - 複雑 (201-500行): {complexity_stats['complex']}")
        print(f"    - 非常に複雑 (>500行): {complexity_stats['very_complex']}")

    def check_mobile_optimizations(self):
        """モバイル最適化チェック"""
        print("\n[4/4] モバイル最適化チェック中...")

        shader_files = list(self.assets_path.rglob("*.shader"))

        mobile_issues = []

        for shader_file in shader_files:
            try:
                # 安全なファイル読み込み
                content = self.file_reader.read_text(shader_file)

                # precision 指定のチェック（安全な正規表現使用）
                precision_match = SafeRegex.search_with_timeout(r'\b(lowp|mediump|highp)\b', content, timeout_seconds=2.0)
                has_precision = bool(precision_match)
            except SecurityError as e:
                self.warnings.append(f"シェーダー分析エラー: {shader_file.relative_to(self.assets_path)} - {e}")
                continue

            if not has_precision and 'Mobile' not in shader_file.name:
                # モバイル向けでない可能性があるがチェック
                pass

            # アルファテストの使用（モバイルではclipが重い）
            if 'clip(' in content:
                mobile_issues.append({
                    'shader': str(shader_file.relative_to(self.assets_path)),
                    'issue': 'clip()使用（モバイルではdiscard推奨）'
                })

            # 複雑な計算（pow, sin, cos, tanなど）の過剰使用（安全な正規表現使用）
            try:
                expensive_op_matches = SafeRegex.finditer_with_timeout(r'\b(pow|sin|cos|tan|exp|log)\s*\(', content, timeout_seconds=2.0)
                expensive_ops = len(expensive_op_matches)
                if expensive_ops > 10:
                    mobile_issues.append({
                        'shader': str(shader_file.relative_to(self.assets_path)),
                        'issue': f'高負荷演算が多い ({expensive_ops} 箇所)'
                    })
            except SecurityError:
                pass

        self.stats['mobile_issues'] = len(mobile_issues)

        if mobile_issues:
            print(f"  ⚠ {len(mobile_issues)} 件のモバイル最適化懸念が見つかりました")
            for issue in mobile_issues[:5]:  # 最初の5件のみ表示
                self.warnings.append(f"モバイル最適化: {issue['shader']} - {issue['issue']}")
        else:
            print(f"  ✓ モバイル最適化の懸念は見つかりませんでした")

    def print_results(self):
        """分析結果の表示"""
        print("\n" + "=" * 60)
        print("分析結果")
        print("=" * 60)

        if self.errors:
            print(f"\n❌ エラー: {len(self.errors)} 件")
            for error in self.errors:
                print(f"  - {error}")

        if self.warnings:
            print(f"\n⚠️  警告: {len(self.warnings)} 件")
            for warning in self.warnings[:20]:  # 最初の20件のみ表示
                print(f"  - {warning}")
            if len(self.warnings) > 20:
                print(f"  ... および {len(self.warnings) - 20} 件の追加警告")

        if not self.errors and not self.warnings:
            print("\n✅ 問題は見つかりませんでした")

        print("\n" + "=" * 60)

    def print_statistics(self):
        """統計情報の表示"""
        print("\n📊 統計情報")
        print("=" * 60)
        print(f"シェーダーファイル数: {self.stats.get('shader_files', 0)}")
        print(f"Shader Graph数: {self.stats.get('shader_graphs', 0)}")
        print(f"総Pass数: {self.stats.get('total_passes', 0)}")
        print(f"平均Pass数/シェーダー: {self.stats.get('avg_passes_per_shader', 0)}")
        print(f"総ノード数 (Shader Graph): {self.stats.get('total_nodes', 0)}")
        print(f"平均ノード数/グラフ: {self.stats.get('avg_nodes_per_graph', 0)}")
        print(f"モバイル最適化懸念: {self.stats.get('mobile_issues', 0)} 件")

        # トップ5の複雑なシェーダーを表示
        complex_shaders = self.stats.get('complex_shaders', [])
        if complex_shaders:
            print("\n📌 最も複雑なシェーダー (Pass数順):")
            sorted_shaders = sorted(complex_shaders, key=lambda x: x['passes'], reverse=True)[:5]
            for shader in sorted_shaders:
                print(f"  - {shader['name']}: {shader['passes']} passes")

        # トップ5の複雑なShader Graph
        complex_graphs = self.stats.get('complex_graphs', [])
        if complex_graphs:
            print("\n📌 最も複雑なShader Graph (ノード数順):")
            sorted_graphs = sorted(complex_graphs, key=lambda x: x['nodes'], reverse=True)[:5]
            for graph in sorted_graphs:
                print(f"  - {graph['name']}: {graph['nodes']} nodes")

        print("=" * 60)

    def export_json_report(self, output_file: str):
        """JSON形式でレポートを出力"""
        # パス検証
        try:
            output_path = self.path_validator.validate_path(output_file, must_exist=False)
        except SecurityError as e:
            print(f"❌ レポート出力エラー: {e}", file=sys.stderr)
            return

        report = {
            'timestamp': __import__('datetime').datetime.now().isoformat(),
            'project': str(self.project_path),
            'statistics': self.stats,
            'errors': self.errors,
            'warnings': self.warnings
        }

        with open(output_path, 'w', encoding='utf-8') as f:
            json.dump(report, f, indent=2, ensure_ascii=False)

        print(f"\n📄 JSONレポートを出力しました: {output_path}")


def main():
    """メイン関数"""
    parser = argparse.ArgumentParser(description='ShaderOp シェーダープロファイリングツール')
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
    parser.add_argument(
        '--output',
        default='shader_profile_report.json',
        help='JSONレポートの出力先（デフォルト: shader_profile_report.json）'
    )
    parser.add_argument(
        '--export-json',
        action='store_true',
        help='JSONレポートを出力する'
    )

    args = parser.parse_args()

    profiler = ShaderProfiler(args.project)
    success = profiler.profile_all()

    if args.export_json:
        profiler.export_json_report(args.output)

    # 終了コードの決定
    if not success:
        sys.exit(1)
    elif args.fail_on_warning and profiler.warnings:
        sys.exit(1)
    else:
        sys.exit(0)


if __name__ == '__main__':
    main()
