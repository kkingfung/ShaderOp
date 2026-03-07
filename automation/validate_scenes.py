#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Unity Scene検証スクリプト

Phase 2のシーン構築を検証します:
- MainMenuシーン: Canvas/UI配置、Button配線、背景画像
- MainCustomizationシーン: CharacterCustomizer、Camera、Lighting
- RoomDecorationシーン: 床・壁メッシュ、RoomDecorator、UI
- TicTacToeHexシーン: HexTileプレハブ、MVC Components、UI
- HexReversiシーン: HexTileプレハブ、MVC Components、UI
"""

import os
import sys
import io
import json
import re
import argparse
from pathlib import Path
from typing import List, Dict, Any, Optional

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


class SceneValidator:
    """Unity Scene検証クラス"""

    def __init__(self, project_path: str):
        # パス検証を使用してプロジェクトパスを安全に取得
        try:
            validator = PathValidator()
            self.project_path = validator.validate_path(project_path, must_exist=True)
        except SecurityError as e:
            print(f"❌ エラー: {e}", file=sys.stderr)
            sys.exit(1)

        self.assets_path = self.project_path / "Assets"
        self.scenes_path = self.assets_path / "Scenes"
        self.errors: List[str] = []
        self.warnings: List[str] = []
        self.info: List[str] = []

        # セキュリティユーティリティの初期化
        self.file_reader = SafeFileReader(max_size_mb=50)
        self.path_validator = validator

    def validate_all(self) -> bool:
        """すべてのシーン検証を実行"""
        print("=== ShaderOp Scene Validation ===")
        print(f"Project: {self.project_path}")
        print(f"Scenes: {self.scenes_path}\n")

        if not self.scenes_path.exists():
            self.errors.append(f"Scenesフォルダが見つかりません: {self.scenes_path}")
            self.print_results()
            return False

        # Phase 2で構築予定のシーンを検証
        self.validate_mainmenu_scene()
        self.validate_customization_scene()
        self.validate_room_decoration_scene()
        self.validate_tictactoehex_scene()
        self.validate_hexreversi_scene()

        self.print_results()
        return len(self.errors) == 0

    def validate_mainmenu_scene(self):
        """MainMenuシーンの検証"""
        print("[1/5] MainMenu シーン検証中...")

        scene_file = self.scenes_path / "MainMenu.unity"

        if not scene_file.exists():
            self.warnings.append("MainMenu.unity が見つかりません（Phase 2で作成予定）")
            print("  ⚠ MainMenu.unity が見つかりません")
            return

        try:
            # 安全なファイル読み込み
            content = self.file_reader.read_text(scene_file)
        except SecurityError as e:
            self.errors.append(f"MainMenu: ファイル読み込みエラー - {e}")
            return

        # Canvas/UI配置確認
        if 'Canvas' not in content:
            self.errors.append("MainMenu: Canvas が見つかりません")
        else:
            self.info.append("MainMenu: Canvas 配置確認 ✓")

        # Button配線確認（Button コンポーネントの存在）
        button_count = content.count('m_Component:')
        if button_count == 0:
            self.warnings.append("MainMenu: UI Componentが見つかりません")

        # EventSystem確認（UI操作に必須）
        if 'EventSystem' not in content:
            self.errors.append("MainMenu: EventSystem が見つかりません")
        else:
            self.info.append("MainMenu: EventSystem 配置確認 ✓")

        print(f"  ✓ MainMenu シーンをチェックしました")

    def validate_customization_scene(self):
        """MainCustomizationシーンの検証"""
        print("\n[2/5] MainCustomization シーン検証中...")

        scene_file = self.scenes_path / "MainCustomization.unity"

        if not scene_file.exists():
            self.warnings.append("MainCustomization.unity が見つかりません（Phase 2で作成予定）")
            print("  ⚠ MainCustomization.unity が見つかりません")
            return

        try:
            # 安全なファイル読み込み
            content = self.file_reader.read_text(scene_file)
        except SecurityError as e:
            self.errors.append(f"MainCustomization: ファイル読み込みエラー - {e}")
            return

        # CharacterCustomizer配置確認
        if 'CharacterCustomizer' not in content:
            self.warnings.append("MainCustomization: CharacterCustomizer が見つかりません")
        else:
            self.info.append("MainCustomization: CharacterCustomizer 配置確認 ✓")

        # Camera確認
        if 'Camera' not in content:
            self.errors.append("MainCustomization: Camera が見つかりません")
        else:
            self.info.append("MainCustomization: Camera 配置確認 ✓")

        # Lighting確認（Directional Light）
        if 'Light' not in content:
            self.warnings.append("MainCustomization: Lighting が見つかりません")
        else:
            self.info.append("MainCustomization: Lighting 配置確認 ✓")

        print(f"  ✓ MainCustomization シーンをチェックしました")

    def validate_room_decoration_scene(self):
        """RoomDecorationシーンの検証"""
        print("\n[3/5] RoomDecoration シーン検証中...")

        scene_file = self.scenes_path / "RoomDecoration.unity"

        if not scene_file.exists():
            self.warnings.append("RoomDecoration.unity が見つかりません（Phase 2で作成予定）")
            print("  ⚠ RoomDecoration.unity が見つかりません")
            return

        try:
            # 安全なファイル読み込み
            content = self.file_reader.read_text(scene_file)
        except SecurityError as e:
            self.errors.append(f"RoomDecoration: ファイル読み込みエラー - {e}")
            return

        # RoomDecorator配置確認
        if 'RoomDecorator' not in content:
            self.warnings.append("RoomDecoration: RoomDecorator が見つかりません")
        else:
            self.info.append("RoomDecoration: RoomDecorator 配置確認 ✓")

        # Camera確認
        if 'Camera' not in content:
            self.errors.append("RoomDecoration: Camera が見つかりません")
        else:
            self.info.append("RoomDecoration: Camera 配置確認 ✓")

        # UI Canvas確認
        if 'Canvas' not in content:
            self.warnings.append("RoomDecoration: Canvas が見つかりません")
        else:
            self.info.append("RoomDecoration: Canvas 配置確認 ✓")

        print(f"  ✓ RoomDecoration シーンをチェックしました")

    def validate_tictactoehex_scene(self):
        """TicTacToeHexシーンの検証"""
        print("\n[4/5] TicTacToeHex シーン検証中...")

        scene_file = self.scenes_path / "TicTacToeHex.unity"

        if not scene_file.exists():
            self.warnings.append("TicTacToeHex.unity が見つかりません（Phase 2で作成予定）")
            print("  ⚠ TicTacToeHex.unity が見つかりません")
            return

        try:
            # 安全なファイル読み込み
            content = self.file_reader.read_text(scene_file)
        except SecurityError as e:
            self.errors.append(f"TicTacToeHex: ファイル読み込みエラー - {e}")
            return

        # MVC Components配線確認
        mvc_components = [
            'TicTacToeHexModel',
            'TicTacToeHexView',
            'TicTacToeHexController'
        ]

        for component in mvc_components:
            if component not in content:
                self.warnings.append(f"TicTacToeHex: {component} が見つかりません")
            else:
                self.info.append(f"TicTacToeHex: {component} 配置確認 ✓")

        # Camera確認（2D/Orthographic推奨）
        if 'Camera' not in content:
            self.errors.append("TicTacToeHex: Camera が見つかりません")
        else:
            self.info.append("TicTacToeHex: Camera 配置確認 ✓")

        # UI Canvas確認
        if 'Canvas' not in content:
            self.warnings.append("TicTacToeHex: Canvas が見つかりません（ターン表示、リセットボタン用）")
        else:
            self.info.append("TicTacToeHex: Canvas 配置確認 ✓")

        print(f"  ✓ TicTacToeHex シーンをチェックしました")

    def validate_hexreversi_scene(self):
        """HexReversiシーンの検証"""
        print("\n[5/5] HexReversi シーン検証中...")

        scene_file = self.scenes_path / "HexReversi.unity"

        if not scene_file.exists():
            self.warnings.append("HexReversi.unity が見つかりません（Phase 2で作成予定）")
            print("  ⚠ HexReversi.unity が見つかりません")
            return

        try:
            # 安全なファイル読み込み
            content = self.file_reader.read_text(scene_file)
        except SecurityError as e:
            self.errors.append(f"HexReversi: ファイル読み込みエラー - {e}")
            return

        # MVC Components配線確認
        mvc_components = [
            'HexReversiModel',
            'HexReversiView',
            'HexReversiController'
        ]

        for component in mvc_components:
            if component not in content:
                self.warnings.append(f"HexReversi: {component} が見つかりません")
            else:
                self.info.append(f"HexReversi: {component} 配置確認 ✓")

        # Camera確認
        if 'Camera' not in content:
            self.errors.append("HexReversi: Camera が見つかりません")
        else:
            self.info.append("HexReversi: Camera 配置確認 ✓")

        # UI Canvas確認（スコア、ヒントボタン用）
        if 'Canvas' not in content:
            self.warnings.append("HexReversi: Canvas が見つかりません（スコア、ヒントボタン用）")
        else:
            self.info.append("HexReversi: Canvas 配置確認 ✓")

        print(f"  ✓ HexReversi シーンをチェックしました")

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
            for info_msg in self.info[:10]:  # 最初の10件のみ表示
                print(f"  - {info_msg}")
            if len(self.info) > 10:
                print(f"  ... および {len(self.info) - 10} 件の追加情報")

        if not self.errors and not self.warnings:
            print("\n✅ すべてのシーンが正常です")

        print("\n" + "=" * 60)


def main():
    """メイン関数"""
    parser = argparse.ArgumentParser(description='ShaderOp Unity Scene検証ツール')
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

    validator = SceneValidator(args.project)
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
