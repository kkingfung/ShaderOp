#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
フォルダ構造自動生成スクリプト

Usage:
    python generate_structure.py > FOLDER_STRUCTURE.md
"""

import os
import sys
from pathlib import Path

# UTF-8出力設定
if sys.platform == "win32":
    import io
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

# プロジェクトルート
PROJECT_ROOT = Path(__file__).parent.parent.parent
UNITY_PROJECT = PROJECT_ROOT / "ShaderOptimizer"

# 除外するフォルダ
EXCLUDE_DIRS = {
    "Library", "Temp", "Logs", "obj", "Builds", "UserSettings",
    ".vs", ".vscode", ".idea", "node_modules", "__pycache__",
    "SD Unity-Chan Haon Custom", "SoStylized", "8Set", "AITranslator",
    "TomatocolCharacterVarietyPackVol1DEMO"
}

# 除外する拡張子
EXCLUDE_EXTS = {
    ".meta", ".log", ".tmp", ".cache"
}

def should_include(path: Path) -> bool:
    """パスを含めるべきか判定"""
    # 除外フォルダチェック
    for parent in path.parents:
        if parent.name in EXCLUDE_DIRS:
            return False

    # 隠しファイル/フォルダ（.gitは含める）
    if path.name.startswith(".") and path.name not in {".gitignore", ".gitattributes", ".gitkeep"}:
        return False

    # 除外拡張子チェック
    if path.is_file() and path.suffix in EXCLUDE_EXTS:
        return False

    return True

def generate_tree(root: Path, prefix: str = "", max_depth: int = 4, current_depth: int = 0) -> list[str]:
    """ディレクトリツリー生成"""
    if current_depth >= max_depth:
        return []

    lines = []

    try:
        entries = sorted(root.iterdir(), key=lambda p: (not p.is_dir(), p.name.lower()))
        entries = [e for e in entries if should_include(e)]

        for i, entry in enumerate(entries):
            is_last = (i == len(entries) - 1)
            connector = "└── " if is_last else "├── "

            # ファイルの場合、説明を追加
            suffix = ""
            if entry.is_file():
                ext = entry.suffix
                if ext == ".shadergraph":
                    suffix = " # Shader Graph"
                elif ext == ".hlsl":
                    suffix = " # HLSL Custom Function"
                elif ext == ".asmdef":
                    suffix = " # Assembly Definition"
                elif ext == ".cs":
                    suffix = " # C# Script"
                elif ext == ".mat":
                    suffix = " # Material"
                elif ext == ".md":
                    suffix = " # Documentation"

            lines.append(f"{prefix}{connector}{entry.name}{suffix}")

            # サブディレクトリ再帰
            if entry.is_dir():
                extension = "    " if is_last else "│   "
                lines.extend(generate_tree(entry, prefix + extension, max_depth, current_depth + 1))

    except PermissionError:
        pass

    return lines

def main():
    """メイン処理"""
    print("# ShaderOp プロジェクト構造")
    print()
    print("**自動生成日時**: " + Path(__file__).stat().st_mtime.__str__())
    print()
    print("## プロジェクトルート")
    print()
    print("```")
    print("ShaderOp/")

    # ルートディレクトリのツリー
    for line in generate_tree(PROJECT_ROOT, ""):
        print(line)

    print("```")
    print()
    print("## Unity プロジェクト詳細")
    print()
    print("```")
    print("ShaderOptimizer/")

    # Unity プロジェクトのツリー（深さ5まで）
    for line in generate_tree(UNITY_PROJECT, "", max_depth=5):
        print(line)

    print("```")
    print()
    print("## 注意事項")
    print()
    print("- このファイルは自動生成されます。手動編集しないでください。")
    print("- 再生成: `python .claude/mcp/generate_structure.py > FOLDER_STRUCTURE.md`")
    print("- 除外フォルダ: Library, Temp, Logs, 参考アセット等")
    print()

if __name__ == "__main__":
    main()
