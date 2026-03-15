#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
モバイルビルド自動化スクリプト

Android/iOS向けの最適化ビルドを自動生成します。
IL2CPP、テクスチャ圧縮、ストリッピングレベルなどを設定してビルドを実行します。
"""

import os
import sys
import io
import json
import argparse
import subprocess
import time
from pathlib import Path
from datetime import datetime
from typing import Dict, Any, Optional, List

# セキュリティユーティリティのインポート
from security_utils import (
    PathValidator,
    SafeFileReader,
    SafeCommandExecutor,
    SecurityError
)

# Windows環境でUTF-8出力を強制
if sys.platform == 'win32':
    if sys.stdout.encoding != 'utf-8':
        sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
    if sys.stderr.encoding != 'utf-8':
        sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8')


class MobileBuildAutomation:
    """モバイルビルド自動化クラス"""

    # サポートするプラットフォーム
    SUPPORTED_PLATFORMS = ['Android', 'iOS']

    # ビルドタイプ
    BUILD_TYPES = ['Development', 'Release']

    # スクリプティングバックエンド
    SCRIPTING_BACKENDS = ['Mono', 'IL2CPP']

    def __init__(self, project_path: str, unity_path: Optional[str] = None, require_unity: bool = True):
        """
        Args:
            project_path: Unityプロジェクトのパス
            unity_path: Unity.exeのパス（Noneの場合は自動検出）
            require_unity: Unityが必須かどうか（設定生成のみの場合はFalse）
        """
        # パス検証
        validator = PathValidator()
        try:
            self.project_path = validator.validate_path(project_path, must_exist=True)
        except SecurityError as e:
            print(f"❌ エラー: {e}", file=sys.stderr)
            sys.exit(1)

        # Unityエディタのパス設定
        self.unity_path = self._find_unity_editor(unity_path)
        if not self.unity_path and require_unity:
            print("❌ エラー: Unity Editorが見つかりません", file=sys.stderr)
            sys.exit(1)

        # ビルド出力ディレクトリ
        self.builds_dir = self.project_path.parent / "builds"
        self.builds_dir.mkdir(exist_ok=True)

        # セキュリティユーティリティ
        self.file_reader = SafeFileReader(max_size_mb=10)
        self.path_validator = validator
        self.command_executor = SafeCommandExecutor()

        print(f"✓ Unity プロジェクト: {self.project_path}")
        if self.unity_path:
            print(f"✓ Unity エディタ: {self.unity_path}")
        print(f"✓ ビルド出力先: {self.builds_dir}")

    def _find_unity_editor(self, user_path: Optional[str]) -> Optional[Path]:
        """Unity Editorのパスを検出"""
        # ユーザー指定がある場合
        if user_path:
            unity_exe = Path(user_path)
            if unity_exe.exists():
                return unity_exe
            else:
                print(f"⚠ 警告: 指定されたUnityパスが見つかりません: {user_path}")

        # 環境変数から検出
        if 'UNITY_PATH' in os.environ:
            unity_exe = Path(os.environ['UNITY_PATH'])
            if unity_exe.exists():
                return unity_exe

        # デフォルトパスから検出（Windows）
        if sys.platform == 'win32':
            # Unity Hub経由でインストールされた最新バージョンを探す
            hub_base = Path(r"C:\Program Files\Unity\Hub\Editor")
            if hub_base.exists():
                versions = sorted([v for v in hub_base.iterdir() if v.is_dir()], reverse=True)
                for version in versions:
                    unity_exe = version / "Editor" / "Unity.exe"
                    if unity_exe.exists():
                        return unity_exe

            # 古い方式のインストールパス
            default_paths = [
                Path(r"C:\Program Files\Unity\Editor\Unity.exe"),
                Path(r"C:\Program Files (x86)\Unity\Editor\Unity.exe"),
            ]
            for unity_exe in default_paths:
                if unity_exe.exists():
                    return unity_exe

        # macOS
        elif sys.platform == 'darwin':
            unity_exe = Path("/Applications/Unity/Unity.app/Contents/MacOS/Unity")
            if unity_exe.exists():
                return unity_exe

        return None

    def load_build_config(self, config_path: str) -> Dict[str, Any]:
        """ビルド設定ファイルを読み込み"""
        print(f"📄 ビルド設定を読み込み中: {config_path}")

        try:
            config_file = self.path_validator.validate_path(config_path, must_exist=True)
            config = self.file_reader.read_json(config_file)

            # 設定の検証
            self._validate_config(config)

            return config
        except SecurityError as e:
            print(f"❌ 設定ファイル読み込みエラー: {e}", file=sys.stderr)
            sys.exit(1)

    def _validate_config(self, config: Dict[str, Any]):
        """ビルド設定の検証"""
        required_fields = ['platform', 'buildType', 'scriptingBackend']

        for field in required_fields:
            if field not in config:
                raise ValueError(f"必須フィールドが不足しています: {field}")

        # プラットフォームチェック
        if config['platform'] not in self.SUPPORTED_PLATFORMS:
            raise ValueError(
                f"サポートされていないプラットフォーム: {config['platform']}\n"
                f"サポート: {', '.join(self.SUPPORTED_PLATFORMS)}"
            )

        # ビルドタイプチェック
        if config['buildType'] not in self.BUILD_TYPES:
            raise ValueError(
                f"サポートされていないビルドタイプ: {config['buildType']}\n"
                f"サポート: {', '.join(self.BUILD_TYPES)}"
            )

        # スクリプティングバックエンドチェック
        if config['scriptingBackend'] not in self.SCRIPTING_BACKENDS:
            raise ValueError(
                f"サポートされていないスクリプティングバックエンド: {config['scriptingBackend']}\n"
                f"サポート: {', '.join(self.SCRIPTING_BACKENDS)}"
            )

    def build(self, config: Dict[str, Any], dry_run: bool = False) -> Dict[str, Any]:
        """
        ビルドを実行

        Args:
            config: ビルド設定
            dry_run: ドライラン（実際にはビルドしない）

        Returns:
            ビルド結果レポート
        """
        platform = config['platform']
        build_type = config['buildType']
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")

        print("\n" + "="*60)
        print(f"🚀 モバイルビルド開始: {platform} ({build_type})")
        print("="*60)

        # ビルド設定の表示
        self._print_build_config(config)

        # ドライランの場合はここで終了
        if dry_run:
            print("\n✓ ドライラン完了（実際のビルドはスキップされました）")
            return {
                "status": "dry_run",
                "platform": platform,
                "buildType": build_type,
                "timestamp": timestamp
            }

        # ログファイルのパス
        log_file = self.builds_dir / f"build_{platform}_{timestamp}.log"

        # Unity バッチモードコマンドの構築
        unity_args = self._build_unity_command(config, log_file)

        # ビルド実行
        print(f"\n📦 Unityビルドを実行中...")
        print(f"   ログファイル: {log_file}")

        start_time = time.time()

        try:
            result = self.command_executor.run(
                unity_args,
                timeout=3600,  # 1時間タイムアウト
                capture_output=False  # リアルタイム出力
            )

            build_time = time.time() - start_time

            if result.returncode == 0:
                print(f"\n✓ ビルド成功！ ({build_time:.1f}秒)")

                # ビルド成果物の検証
                build_artifact = self._get_build_artifact_path(config)
                report = self._validate_build_artifact(build_artifact, config, build_time)

                # ビルドレポートを保存
                self._save_build_report(report, config, timestamp)

                return report
            else:
                print(f"\n❌ ビルド失敗 (終了コード: {result.returncode})")
                print(f"   ログファイルを確認してください: {log_file}")

                return {
                    "status": "failed",
                    "platform": platform,
                    "buildType": build_type,
                    "timestamp": timestamp,
                    "error": f"Unity終了コード: {result.returncode}"
                }

        except SecurityError as e:
            print(f"\n❌ ビルドエラー: {e}", file=sys.stderr)
            return {
                "status": "error",
                "platform": platform,
                "buildType": build_type,
                "timestamp": timestamp,
                "error": str(e)
            }

    def _print_build_config(self, config: Dict[str, Any]):
        """ビルド設定を表示"""
        print("\n📋 ビルド設定:")
        print(f"   Platform: {config['platform']}")
        print(f"   Build Type: {config['buildType']}")
        print(f"   Scripting Backend: {config['scriptingBackend']}")

        if 'apiLevel' in config:
            print(f"   API Level: {config['apiLevel']}")

        if 'architecture' in config:
            print(f"   Architecture: {config['architecture']}")

        if 'compressionMethod' in config:
            print(f"   Compression: {config['compressionMethod']}")

        if 'managedStripping' in config:
            print(f"   Managed Stripping: {config['managedStripping']}")

        if 'optimizations' in config:
            print(f"   Optimizations:")
            for key, value in config['optimizations'].items():
                print(f"     - {key}: {value}")

    def _build_unity_command(self, config: Dict[str, Any], log_file: Path) -> List[str]:
        """Unity バッチモードコマンドを構築"""
        platform = config['platform']
        build_type = config['buildType']

        # ビルドメソッド名を決定
        if platform == 'Android':
            build_method = 'BuildAndroidDev' if build_type == 'Development' else 'BuildAndroid'
        elif platform == 'iOS':
            build_method = 'BuildiOSDev' if build_type == 'Development' else 'BuildiOS'
        else:
            raise ValueError(f"未対応のプラットフォーム: {platform}")

        # ビルドパス
        build_artifact = self._get_build_artifact_path(config)

        # Unity コマンドライン引数
        unity_args = [
            str(self.unity_path),
            '-quit',
            '-batchmode',
            '-projectPath', str(self.project_path),
            '-executeMethod', f'ShaderOp.Editor.BuildScript.{build_method}',
            '-buildPath', str(build_artifact),
            '-logFile', str(log_file)
        ]

        # バージョン設定（あれば）
        if 'version' in config:
            unity_args.extend(['-version', config['version']])

        return unity_args

    def _get_build_artifact_path(self, config: Dict[str, Any]) -> Path:
        """ビルド成果物のパスを取得"""
        platform = config['platform']
        build_type = config['buildType']
        version = config.get('version', '0.4.0')
        timestamp = datetime.now().strftime("%Y%m%d")

        if platform == 'Android':
            filename = f"ShaderOp_{version}_{build_type}_{timestamp}.apk"
            return self.builds_dir / "Android" / filename
        elif platform == 'iOS':
            foldername = f"iOS_{version}_{build_type}_{timestamp}"
            return self.builds_dir / foldername
        else:
            raise ValueError(f"未対応のプラットフォーム: {platform}")

    def _validate_build_artifact(
        self,
        artifact_path: Path,
        config: Dict[str, Any],
        build_time: float
    ) -> Dict[str, Any]:
        """ビルド成果物を検証"""
        print(f"\n🔍 ビルド成果物を検証中: {artifact_path}")

        report = {
            "status": "success",
            "platform": config['platform'],
            "buildType": config['buildType'],
            "timestamp": datetime.now().isoformat(),
            "buildTime": round(build_time, 2),
            "artifact": str(artifact_path)
        }

        # ファイル/ディレクトリの存在チェック
        if not artifact_path.exists():
            report["status"] = "error"
            report["error"] = "ビルド成果物が見つかりません"
            print(f"   ❌ エラー: {artifact_path} が見つかりません")
            return report

        # APKの場合はサイズチェック
        if config['platform'] == 'Android' and artifact_path.suffix == '.apk':
            size_mb = artifact_path.stat().st_size / (1024 * 1024)
            report["sizeMB"] = round(size_mb, 2)

            print(f"   ✓ APKサイズ: {size_mb:.2f} MB")

            # サイズ警告（200MB超過）
            if size_mb > 200:
                report["warnings"] = [f"APKサイズが大きすぎます: {size_mb:.2f} MB > 200 MB"]
                print(f"   ⚠ 警告: APKサイズが200MBを超えています")

        # iOSの場合はディレクトリサイズ
        elif config['platform'] == 'iOS' and artifact_path.is_dir():
            size_mb = self._get_directory_size(artifact_path) / (1024 * 1024)
            report["sizeMB"] = round(size_mb, 2)

            print(f"   ✓ ビルドサイズ: {size_mb:.2f} MB")

        return report

    def _save_build_report(self, report: Dict[str, Any], config: Dict[str, Any], timestamp: str):
        """ビルドレポートを保存"""
        platform = config['platform']
        report_file = self.builds_dir / f"build_report_{platform}_{timestamp}.json"

        print(f"\n💾 ビルドレポートを保存中: {report_file}")

        # JSON形式で保存
        with open(report_file, 'w', encoding='utf-8') as f:
            json.dump({
                "report": report,
                "config": config
            }, f, indent=2, ensure_ascii=False)

        # Markdown形式でも保存
        md_file = report_file.with_suffix('.md')
        self._generate_markdown_report(report, config, md_file)

        print(f"   ✓ JSON: {report_file}")
        print(f"   ✓ Markdown: {md_file}")

    def _generate_markdown_report(self, report: Dict[str, Any], config: Dict[str, Any], output_file: Path):
        """Markdown形式のビルドレポートを生成"""
        status_emoji = "✅" if report['status'] == 'success' else "❌"

        md_content = f"""# ShaderOp モバイルビルドレポート

**Status**: {status_emoji} {report['status']}
**Platform**: {config['platform']}
**Build Type**: {config['buildType']}
**Date**: {report['timestamp']}
**Build Time**: {report['buildTime']}秒

---

## Build Configuration

| 項目 | 設定値 |
|------|--------|
| Platform | {config['platform']} |
| Build Type | {config['buildType']} |
| Scripting Backend | {config['scriptingBackend']} |
"""

        if 'apiLevel' in config:
            md_content += f"| API Level | {config['apiLevel']} |\n"

        if 'architecture' in config:
            md_content += f"| Architecture | {config['architecture']} |\n"

        if 'compressionMethod' in config:
            md_content += f"| Compression | {config['compressionMethod']} |\n"

        if 'managedStripping' in config:
            md_content += f"| Managed Stripping | {config['managedStripping']} |\n"

        md_content += "\n---\n\n## Build Artifact\n\n"

        if 'sizeMB' in report:
            md_content += f"**Size**: {report['sizeMB']} MB\n"

        md_content += f"**Path**: `{report['artifact']}`\n\n"

        # 警告があれば表示
        if 'warnings' in report:
            md_content += "## ⚠ Warnings\n\n"
            for warning in report['warnings']:
                md_content += f"- {warning}\n"
            md_content += "\n"

        # エラーがあれば表示
        if 'error' in report:
            md_content += f"## ❌ Error\n\n{report['error']}\n\n"

        # 最適化設定
        if 'optimizations' in config:
            md_content += "## Optimizations\n\n"
            for key, value in config['optimizations'].items():
                md_content += f"- **{key}**: {value}\n"
            md_content += "\n"

        # インストールテストチェックリスト
        md_content += """---

## Installation Testing Checklist

- [ ] APK/IPA転送完了
- [ ] デバイスへのインストール成功
- [ ] アプリ起動確認
- [ ] 全シーン遷移確認
- [ ] 4つのミニゲーム動作確認
- [ ] 60fps維持確認
- [ ] メモリリーク確認
- [ ] クラッシュなし

---

## Performance Validation (On-Device)

### Target Metrics

- Frame Time: <16.67ms (60fps)
- CPU Usage: <50% sustained
- Memory Usage: <300MB
- Battery Drain: <15% per hour

### Test Scenarios

1. Play 50 turns in each game
2. Monitor RAM usage and GC spikes
3. 30-minute gameplay session (thermal throttling)
4. Battery drain test (1 hour)

---

**Generated**: {datetime.now().isoformat()}
"""

        with open(output_file, 'w', encoding='utf-8') as f:
            f.write(md_content)

    @staticmethod
    def _get_directory_size(path: Path) -> int:
        """ディレクトリのサイズを取得（バイト単位）"""
        total_size = 0
        for file in path.rglob("*"):
            if file.is_file():
                total_size += file.stat().st_size
        return total_size

    def generate_default_config(self, output_file: str):
        """デフォルトのビルド設定ファイルを生成"""
        print(f"📄 デフォルト設定ファイルを生成中: {output_file}")

        config_template = {
            "platform": "Android",
            "buildType": "Development",
            "version": "0.4.0",
            "scriptingBackend": "IL2CPP",
            "apiLevel": "Android10",
            "architecture": "ARM64",
            "compressionMethod": "LZ4",
            "managedStripping": "Medium",
            "optimizations": {
                "graphicsAPI": "OpenGLES3",
                "textureCompression": "ASTC",
                "scriptOptimization": "Speed"
            },
            "scenes": [
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/MainCustomization.unity",
                "Assets/Scenes/HexChess.unity",
                "Assets/Scenes/HexCheckers.unity",
                "Assets/Scenes/HexReversi.unity",
                "Assets/Scenes/TicTacToeHex.unity"
            ]
        }

        with open(output_file, 'w', encoding='utf-8') as f:
            json.dump(config_template, f, indent=2, ensure_ascii=False)

        print(f"   ✓ 設定ファイルを保存しました: {output_file}")
        print(f"\n   編集後、以下のコマンドでビルド実行:")
        print(f"   python build_mobile.py --config {output_file}")


def main():
    """メイン関数"""
    parser = argparse.ArgumentParser(
        description='ShaderOp モバイルビルド自動化スクリプト',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
使用例:
  # デフォルト設定ファイルを生成
  python build_mobile.py --generate-config build_config.json

  # 設定ファイルを使用してビルド
  python build_mobile.py --config build_config.json

  # ドライラン（実際にはビルドしない）
  python build_mobile.py --config build_config.json --dry-run

  # Unityエディタのパスを手動指定
  python build_mobile.py --config build_config.json --unity-path "C:/Program Files/Unity/Hub/Editor/2022.3.10f1/Editor/Unity.exe"
        """
    )

    parser.add_argument(
        '--project',
        default='ShaderOptimizer',
        help='Unityプロジェクトのパス（デフォルト: ShaderOptimizer）'
    )

    parser.add_argument(
        '--unity-path',
        help='Unity.exeのパス（省略時は自動検出）'
    )

    parser.add_argument(
        '--config',
        help='ビルド設定JSONファイルのパス'
    )

    parser.add_argument(
        '--generate-config',
        metavar='OUTPUT_FILE',
        help='デフォルト設定ファイルを生成（例: build_config.json）'
    )

    parser.add_argument(
        '--dry-run',
        action='store_true',
        help='ドライラン（実際にはビルドしない）'
    )

    args = parser.parse_args()

    # ヘルプ表示
    if not args.config and not args.generate_config:
        parser.print_help()
        print("\n💡 ヒント: まず --generate-config でテンプレートを作成してください")
        sys.exit(0)

    # 設定ファイル生成モード（Unityは不要）
    if args.generate_config:
        automation = MobileBuildAutomation(args.project, args.unity_path, require_unity=False)
        automation.generate_default_config(args.generate_config)
        return

    # ビルドモード（Unityが必須）
    automation = MobileBuildAutomation(args.project, args.unity_path, require_unity=True)

    # ビルドモード
    if args.config:
        config = automation.load_build_config(args.config)
        report = automation.build(config, dry_run=args.dry_run)

        # 結果サマリー
        print("\n" + "="*60)
        if report['status'] == 'success':
            print("✅ ビルド成功！")
            if 'sizeMB' in report:
                print(f"   サイズ: {report['sizeMB']} MB")
            print(f"   ビルド時間: {report['buildTime']}秒")
            print(f"   成果物: {report['artifact']}")
        elif report['status'] == 'dry_run':
            print("✓ ドライラン完了")
        else:
            print("❌ ビルド失敗")
            if 'error' in report:
                print(f"   エラー: {report['error']}")
            sys.exit(1)
        print("="*60)


if __name__ == '__main__':
    main()
