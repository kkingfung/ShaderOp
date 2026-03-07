#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
セキュリティユーティリティモジュール

プロジェクト全体で使用するセキュリティ関連の共通機能を提供:
- パス検証
- ファイル読み込みの安全化
- 入力検証
- 正規表現の安全な実行
- セキュリティイベントロギング
"""

import os
import sys
import re
import signal
from pathlib import Path
from typing import List, Optional, Any
import subprocess

# セキュリティロガーのインポート（循環インポート回避のため遅延ロード）
_security_logger = None


def _get_security_logger():
    """セキュリティロガーを遅延ロード"""
    global _security_logger
    if _security_logger is None:
        try:
            from security_logger import get_security_logger
            _security_logger = get_security_logger()
        except ImportError:
            # ロガーが利用できない場合はNone
            _security_logger = None
    return _security_logger


class SecurityError(Exception):
    """セキュリティ関連のエラー"""
    pass


class PathValidator:
    """パス検証クラス"""

    def __init__(self, base_dir: Optional[Path] = None):
        """
        Args:
            base_dir: 許可するベースディレクトリ（デフォルトはプロジェクトルート）
        """
        if base_dir is None:
            # スクリプトの親ディレクトリをプロジェクトルートとする
            self.base_dir = Path(__file__).parent.parent.resolve()
        else:
            self.base_dir = base_dir.resolve()

    def validate_path(self, user_input: str, must_exist: bool = True) -> Path:
        """
        ユーザー入力パスを検証

        Args:
            user_input: ユーザーからの入力パス
            must_exist: パスが存在する必要があるか

        Returns:
            検証済みの絶対パス

        Raises:
            SecurityError: パスが不正な場合
        """
        # 空文字チェック
        if not user_input or not user_input.strip():
            raise SecurityError("パスが空です")

        # Null文字チェック
        if '\x00' in user_input:
            raise SecurityError("Null文字は許可されていません")

        # 危険な文字をチェック（Windows/Unix共通）
        dangerous_chars = r'[<>:"|?*\x00-\x1f]'
        if re.search(dangerous_chars, user_input):
            raise SecurityError(f"不正な文字が含まれています: {user_input}")

        # パストラバーサルチェック
        if '..' in user_input or user_input.startswith('/'):
            # 相対パスの..は許可しない
            if '..' in Path(user_input).parts:
                # セキュリティイベントをログに記録
                logger = _get_security_logger()
                if logger:
                    logger.log_path_traversal_attempt(user_input)
                raise SecurityError(f"パストラバーサルは許可されていません: {user_input}")

        # 絶対パスに変換して検証
        try:
            if Path(user_input).is_absolute():
                full_path = Path(user_input).resolve()
            else:
                full_path = (self.base_dir / user_input).resolve()
        except (ValueError, OSError) as e:
            raise SecurityError(f"パスの解決に失敗しました: {user_input} - {e}")

        # ベースディレクトリ内かチェック
        try:
            full_path.relative_to(self.base_dir)
        except ValueError:
            raise SecurityError(
                f"ベースディレクトリ外へのアクセスは許可されていません: {user_input}\n"
                f"Base: {self.base_dir}\nResolved: {full_path}"
            )

        # シンボリックリンクの検証
        if full_path.is_symlink():
            # シンボリックリンクの解決先もベースディレクトリ内かチェック
            real_path = full_path.resolve()
            try:
                real_path.relative_to(self.base_dir)
            except ValueError:
                raise SecurityError(
                    f"シンボリックリンクがベースディレクトリ外を指しています: {user_input}"
                )

        # 存在チェック
        if must_exist and not full_path.exists():
            # ログに記録
            logger = _get_security_logger()
            if logger:
                logger.log_path_validation(False, user_input, reason="パスが見つかりません")
            raise SecurityError(f"パスが見つかりません: {full_path}")

        # 検証成功をログに記録
        logger = _get_security_logger()
        if logger:
            logger.log_path_validation(True, user_input)

        return full_path


class SafeFileReader:
    """安全なファイル読み込みクラス"""

    def __init__(self, max_size_mb: int = 50):
        """
        Args:
            max_size_mb: 読み込み可能な最大ファイルサイズ（MB）
        """
        self.max_size_bytes = max_size_mb * 1024 * 1024

    def read_text(self, file_path: Path, encoding: str = 'utf-8') -> str:
        """
        テキストファイルを安全に読み込む

        Args:
            file_path: 読み込むファイルパス
            encoding: エンコーディング

        Returns:
            ファイル内容

        Raises:
            SecurityError: ファイルサイズが制限を超えている場合
        """
        file_size = file_path.stat().st_size

        if file_size > self.max_size_bytes:
            # DoS試行をログに記録
            logger = _get_security_logger()
            if logger:
                logger.log_attack_attempt(
                    "dos",
                    f"ファイルサイズ制限超過: {file_path.name} ({file_size / 1024 / 1024:.2f} MB)",
                    source=str(file_path)
                )
            raise SecurityError(
                f"ファイルが大きすぎます: {file_path.name} "
                f"({file_size / 1024 / 1024:.2f} MB > {self.max_size_bytes / 1024 / 1024} MB)"
            )

        try:
            content = file_path.read_text(encoding=encoding, errors='ignore')

            # ファイル読み込み成功をログに記録
            logger = _get_security_logger()
            if logger:
                logger.log_file_access("read", str(file_path), True)

            return content
        except Exception as e:
            # ファイル読み込み失敗をログに記録
            logger = _get_security_logger()
            if logger:
                logger.log_file_access("read", str(file_path), False, reason=str(e))
            raise SecurityError(f"ファイル読み込みエラー: {file_path.name} - {e}")

    def read_json(self, file_path: Path, max_depth: int = 20):
        """
        JSONファイルを安全に読み込む

        Args:
            file_path: JSONファイルパス
            max_depth: JSONの最大深さ

        Returns:
            パースされたJSONデータ

        Raises:
            SecurityError: JSONが不正または深すぎる場合
        """
        import json

        content = self.read_text(file_path)

        try:
            data = json.loads(content)
        except json.JSONDecodeError as e:
            raise SecurityError(f"JSON解析エラー: {file_path.name} - {e}")

        # JSONの深さをチェック
        def check_depth(obj: Any, depth: int = 0) -> None:
            if depth > max_depth:
                raise SecurityError(
                    f"JSONの深さが制限を超えています: {depth} > {max_depth}"
                )

            if isinstance(obj, dict):
                for value in obj.values():
                    check_depth(value, depth + 1)
            elif isinstance(obj, list):
                for item in obj:
                    check_depth(item, depth + 1)

        check_depth(data)
        return data


class SafeRegex:
    """安全な正規表現実行クラス"""

    @staticmethod
    def search_with_timeout(
        pattern: str,
        text: str,
        timeout_seconds: float = 1.0,
        flags: int = 0
    ) -> Optional[re.Match]:
        """
        タイムアウト付き正規表現検索

        Args:
            pattern: 正規表現パターン
            text: 検索対象テキスト
            timeout_seconds: タイムアウト時間（秒）
            flags: 正規表現フラグ

        Returns:
            マッチオブジェクトまたはNone

        Raises:
            SecurityError: タイムアウトした場合
        """
        # Windows環境ではsignalが使えないため、文字列長で制限
        if sys.platform == 'win32':
            max_length = 1000000  # 1MB
            if len(text) > max_length:
                raise SecurityError(
                    f"テキストが大きすぎます: {len(text)} > {max_length}"
                )

            try:
                return re.search(pattern, text, flags)
            except Exception as e:
                raise SecurityError(f"正規表現エラー: {e}")

        # Unix環境ではsignalを使用
        def timeout_handler(signum, frame):
            raise SecurityError("正規表現のタイムアウト")

        old_handler = signal.signal(signal.SIGALRM, timeout_handler)
        signal.alarm(int(timeout_seconds))

        try:
            result = re.search(pattern, text, flags)
            signal.alarm(0)  # タイムアウトをキャンセル
            return result
        except SecurityError:
            signal.alarm(0)
            raise
        finally:
            signal.signal(signal.SIGALRM, old_handler)

    @staticmethod
    def finditer_with_timeout(
        pattern: str,
        text: str,
        timeout_seconds: float = 1.0,
        flags: int = 0
    ) -> List[re.Match]:
        """
        タイムアウト付き正規表現全検索

        Args:
            pattern: 正規表現パターン
            text: 検索対象テキスト
            timeout_seconds: タイムアウト時間（秒）
            flags: 正規表現フラグ

        Returns:
            マッチオブジェクトのリスト

        Raises:
            SecurityError: タイムアウトした場合
        """
        # Windows環境では文字列長で制限
        if sys.platform == 'win32':
            max_length = 1000000  # 1MB
            if len(text) > max_length:
                raise SecurityError(
                    f"テキストが大きすぎます: {len(text)} > {max_length}"
                )

            try:
                return list(re.finditer(pattern, text, flags))
            except Exception as e:
                raise SecurityError(f"正規表現エラー: {e}")

        # Unix環境ではsignalを使用
        def timeout_handler(signum, frame):
            raise SecurityError("正規表現のタイムアウト")

        old_handler = signal.signal(signal.SIGALRM, timeout_handler)
        signal.alarm(int(timeout_seconds))

        try:
            result = list(re.finditer(pattern, text, flags))
            signal.alarm(0)
            return result
        except SecurityError:
            signal.alarm(0)
            raise
        finally:
            signal.signal(signal.SIGALRM, old_handler)


class SafeCommandExecutor:
    """安全なコマンド実行クラス"""

    @staticmethod
    def run(
        cmd: List[str],
        cwd: Optional[Path] = None,
        timeout: int = 30,
        capture_output: bool = True
    ) -> subprocess.CompletedProcess:
        """
        コマンドを安全に実行

        Args:
            cmd: コマンドと引数のリスト
            cwd: 作業ディレクトリ
            timeout: タイムアウト時間（秒）
            capture_output: 出力をキャプチャするか

        Returns:
            実行結果

        Raises:
            SecurityError: コマンド実行に失敗した場合
        """
        # コマンドリストの検証
        if not cmd or not isinstance(cmd, list):
            raise SecurityError("コマンドはリストで指定してください")

        if not all(isinstance(arg, (str, Path)) for arg in cmd):
            raise SecurityError("コマンド引数は文字列またはPathオブジェクトである必要があります")

        # 文字列に変換
        cmd_str = [str(c) for c in cmd]

        # 危険なコマンドのブラックリスト
        dangerous_commands = ['rm', 'del', 'format', 'mkfs', 'dd']
        if any(dangerous in cmd_str[0].lower() for dangerous in dangerous_commands):
            raise SecurityError(f"危険なコマンドは実行できません: {cmd_str[0]}")

        try:
            result = subprocess.run(
                cmd_str,
                cwd=str(cwd) if cwd else None,
                timeout=timeout,
                capture_output=capture_output,
                text=True,
                check=False  # エラーを例外にしない
            )
            return result
        except subprocess.TimeoutExpired:
            raise SecurityError(f"コマンドがタイムアウトしました: {' '.join(cmd_str)}")
        except Exception as e:
            raise SecurityError(f"コマンド実行エラー: {e}")


class SensitiveDataMasker:
    """機密情報マスキングクラス"""

    # 機密情報パターン
    PATTERNS = [
        (re.compile(r'(password\s*[:=]\s*)["\']?([^"\'"\s]{3,})', re.IGNORECASE), r'\1***'),
        (re.compile(r'(api[_-]?key\s*[:=]\s*)["\']?([^"\'"\s]{3,})', re.IGNORECASE), r'\1***'),
        (re.compile(r'(secret\s*[:=]\s*)["\']?([^"\'"\s]{3,})', re.IGNORECASE), r'\1***'),
        (re.compile(r'(token\s*[:=]\s*)["\']?([^"\'"\s]{3,})', re.IGNORECASE), r'\1***'),
        (re.compile(r'Bearer\s+([A-Za-z0-9\-._~+/]{10,})=*', re.IGNORECASE), r'Bearer ***'),
    ]

    @classmethod
    def mask(cls, text: str) -> str:
        """
        テキスト中の機密情報をマスキング

        Args:
            text: マスキング対象テキスト

        Returns:
            マスキング済みテキスト
        """
        masked_text = text
        for pattern, replacement in cls.PATTERNS:
            masked_text = pattern.sub(replacement, masked_text)
        return masked_text

    @staticmethod
    def mask_partial(text: str, show_start: int = 3, show_end: int = 3) -> str:
        """
        文字列の一部をマスキング

        Args:
            text: マスキング対象文字列
            show_start: 先頭から表示する文字数
            show_end: 末尾から表示する文字数

        Returns:
            部分マスキング済み文字列
        """
        if len(text) <= show_start + show_end:
            return "***"
        return text[:show_start] + "***" + text[-show_end:]


# 使用例
if __name__ == '__main__':
    # PathValidator の使用例
    validator = PathValidator()
    try:
        safe_path = validator.validate_path("ShaderOptimizer/Assets")
        print(f"✓ 安全なパス: {safe_path}")
    except SecurityError as e:
        print(f"✗ エラー: {e}")

    # SafeFileReader の使用例
    reader = SafeFileReader(max_size_mb=10)
    try:
        content = reader.read_text(Path("README.md"))
        print(f"✓ ファイル読み込み成功: {len(content)} 文字")
    except SecurityError as e:
        print(f"✗ エラー: {e}")

    # SensitiveDataMasker の使用例
    sensitive_text = 'password="secret123" api_key="abcdef123456"'
    masked = SensitiveDataMasker.mask(sensitive_text)
    print(f"マスキング前: {sensitive_text}")
    print(f"マスキング後: {masked}")
