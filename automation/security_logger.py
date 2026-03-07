#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
セキュリティイベントロギングシステム

構造化ログでセキュリティイベントを追跡:
- 認証試行（成功/失敗）
- ファイルアクセス（許可/拒否）
- パス検証エラー
- 機密情報検出
- 異常なアクティビティ
"""

import os
import sys
import io
import json
import logging
import threading
from pathlib import Path
from datetime import datetime
from typing import Dict, Any, Optional, List
from enum import Enum

# Windows環境でUTF-8出力を強制
if sys.platform == 'win32':
    if sys.stdout.encoding != 'utf-8':
        sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
    if sys.stderr.encoding != 'utf-8':
        sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8')


class SecurityEventType(Enum):
    """セキュリティイベントタイプ"""
    # 認証・認可
    AUTH_SUCCESS = "auth_success"
    AUTH_FAILURE = "auth_failure"
    ACCESS_GRANTED = "access_granted"
    ACCESS_DENIED = "access_denied"

    # ファイル操作
    FILE_READ = "file_read"
    FILE_WRITE = "file_write"
    FILE_DELETE = "file_delete"

    # パス検証
    PATH_VALIDATION_SUCCESS = "path_validation_success"
    PATH_VALIDATION_FAILURE = "path_validation_failure"
    PATH_TRAVERSAL_ATTEMPT = "path_traversal_attempt"

    # 入力検証
    INPUT_VALIDATION_SUCCESS = "input_validation_success"
    INPUT_VALIDATION_FAILURE = "input_validation_failure"

    # 機密情報
    SENSITIVE_DATA_DETECTED = "sensitive_data_detected"
    SENSITIVE_DATA_MASKED = "sensitive_data_masked"

    # 攻撃検出
    COMMAND_INJECTION_ATTEMPT = "command_injection_attempt"
    REDOS_ATTEMPT = "redos_attempt"
    DOS_ATTEMPT = "dos_attempt"

    # システムイベント
    SECURITY_SCAN_START = "security_scan_start"
    SECURITY_SCAN_COMPLETE = "security_scan_complete"
    SECURITY_VIOLATION = "security_violation"


class SecurityEventSeverity(Enum):
    """セキュリティイベントの重大度"""
    DEBUG = "debug"
    INFO = "info"
    WARNING = "warning"
    ERROR = "error"
    CRITICAL = "critical"


class SecurityLogger:
    """
    セキュリティイベント専用ロガー

    構造化ログ形式でセキュリティイベントを記録します。
    """

    _instance = None
    _lock = threading.Lock()

    def __new__(cls):
        """シングルトンパターン"""
        if cls._instance is None:
            with cls._lock:
                if cls._instance is None:
                    cls._instance = super().__new__(cls)
        return cls._instance

    def __init__(self):
        """ロガーの初期化"""
        if hasattr(self, '_initialized'):
            return

        self._initialized = True

        # ログディレクトリの設定
        self.log_dir = Path(__file__).parent.parent / "logs"
        self.log_dir.mkdir(parents=True, exist_ok=True)

        # ログファイルパス
        self.security_log_file = self.log_dir / "security_events.log"
        self.security_json_file = self.log_dir / "security_events.jsonl"

        # 標準Pythonロガーの設定
        self.logger = logging.getLogger("SecurityLogger")
        self.logger.setLevel(logging.DEBUG)

        # ファイルハンドラ（テキスト形式）
        text_handler = logging.FileHandler(
            self.security_log_file,
            encoding='utf-8'
        )
        text_handler.setLevel(logging.INFO)
        text_formatter = logging.Formatter(
            '%(asctime)s [%(levelname)s] %(message)s',
            datefmt='%Y-%m-%d %H:%M:%S'
        )
        text_handler.setFormatter(text_formatter)
        self.logger.addHandler(text_handler)

        # コンソールハンドラ（重要なイベントのみ）
        console_handler = logging.StreamHandler()
        console_handler.setLevel(logging.WARNING)
        console_formatter = logging.Formatter(
            '🔒 [%(levelname)s] %(message)s'
        )
        console_handler.setFormatter(console_formatter)
        self.logger.addHandler(console_handler)

        # イベントカウンター
        self.event_counts: Dict[str, int] = {}
        self._counts_lock = threading.Lock()

    def log_event(
        self,
        event_type: SecurityEventType,
        severity: SecurityEventSeverity,
        message: str,
        **kwargs
    ):
        """
        セキュリティイベントをログに記録

        Args:
            event_type: イベントタイプ
            severity: 重大度
            message: イベントメッセージ
            **kwargs: 追加のコンテキスト情報
        """
        # イベントカウントを更新
        with self._counts_lock:
            event_key = event_type.value
            self.event_counts[event_key] = self.event_counts.get(event_key, 0) + 1

        # 構造化ログデータ
        log_data = {
            "timestamp": datetime.utcnow().isoformat() + "Z",
            "event_type": event_type.value,
            "severity": severity.value,
            "message": message,
            "event_count": self.event_counts[event_key],
            **kwargs
        }

        # JSONLファイルに追記（機械可読形式）
        try:
            with open(self.security_json_file, 'a', encoding='utf-8') as f:
                json.dump(log_data, f, ensure_ascii=False)
                f.write('\n')
        except Exception as e:
            print(f"警告: JSONLログ書き込みエラー: {e}", file=sys.stderr)

        # 標準ロガーにも記録
        log_message = f"[{event_type.value}] {message}"
        if kwargs:
            log_message += f" | Context: {json.dumps(kwargs, ensure_ascii=False)}"

        level_map = {
            SecurityEventSeverity.DEBUG: logging.DEBUG,
            SecurityEventSeverity.INFO: logging.INFO,
            SecurityEventSeverity.WARNING: logging.WARNING,
            SecurityEventSeverity.ERROR: logging.ERROR,
            SecurityEventSeverity.CRITICAL: logging.CRITICAL,
        }

        self.logger.log(level_map[severity], log_message)

    def log_path_validation(
        self,
        success: bool,
        path: str,
        reason: Optional[str] = None,
        user: Optional[str] = None
    ):
        """パス検証イベントをログに記録"""
        if success:
            self.log_event(
                SecurityEventType.PATH_VALIDATION_SUCCESS,
                SecurityEventSeverity.DEBUG,
                f"パス検証成功: {path}",
                path=path,
                user=user or "unknown"
            )
        else:
            self.log_event(
                SecurityEventType.PATH_VALIDATION_FAILURE,
                SecurityEventSeverity.WARNING,
                f"パス検証失敗: {path} - {reason}",
                path=path,
                reason=reason,
                user=user or "unknown"
            )

    def log_path_traversal_attempt(self, path: str, user: Optional[str] = None):
        """パストラバーサル試行をログに記録"""
        self.log_event(
            SecurityEventType.PATH_TRAVERSAL_ATTEMPT,
            SecurityEventSeverity.ERROR,
            f"パストラバーサル試行を検出: {path}",
            path=path,
            user=user or "unknown",
            alert=True
        )

    def log_sensitive_data_detection(
        self,
        data_type: str,
        location: str,
        masked_value: Optional[str] = None
    ):
        """機密情報検出をログに記録"""
        self.log_event(
            SecurityEventType.SENSITIVE_DATA_DETECTED,
            SecurityEventSeverity.WARNING,
            f"機密情報検出: {data_type} in {location}",
            data_type=data_type,
            location=location,
            masked_value=masked_value
        )

    def log_file_access(
        self,
        operation: str,
        path: str,
        success: bool,
        reason: Optional[str] = None
    ):
        """ファイルアクセスをログに記録"""
        event_type_map = {
            "read": SecurityEventType.FILE_READ,
            "write": SecurityEventType.FILE_WRITE,
            "delete": SecurityEventType.FILE_DELETE,
        }

        event_type = event_type_map.get(operation, SecurityEventType.FILE_READ)

        if success:
            self.log_event(
                event_type,
                SecurityEventSeverity.INFO,
                f"ファイル{operation}成功: {path}",
                path=path,
                operation=operation
            )
        else:
            self.log_event(
                SecurityEventType.ACCESS_DENIED,
                SecurityEventSeverity.WARNING,
                f"ファイル{operation}拒否: {path} - {reason}",
                path=path,
                operation=operation,
                reason=reason
            )

    def log_attack_attempt(
        self,
        attack_type: str,
        details: str,
        source: Optional[str] = None
    ):
        """攻撃試行をログに記録"""
        attack_event_map = {
            "command_injection": SecurityEventType.COMMAND_INJECTION_ATTEMPT,
            "redos": SecurityEventType.REDOS_ATTEMPT,
            "dos": SecurityEventType.DOS_ATTEMPT,
        }

        event_type = attack_event_map.get(
            attack_type,
            SecurityEventType.SECURITY_VIOLATION
        )

        self.log_event(
            event_type,
            SecurityEventSeverity.CRITICAL,
            f"攻撃試行を検出: {attack_type} - {details}",
            attack_type=attack_type,
            details=details,
            source=source or "unknown",
            alert=True
        )

    def log_security_scan(
        self,
        scan_type: str,
        status: str,
        findings: Optional[Dict[str, Any]] = None
    ):
        """セキュリティスキャン結果をログに記録"""
        if status == "start":
            self.log_event(
                SecurityEventType.SECURITY_SCAN_START,
                SecurityEventSeverity.INFO,
                f"セキュリティスキャン開始: {scan_type}",
                scan_type=scan_type
            )
        elif status == "complete":
            self.log_event(
                SecurityEventType.SECURITY_SCAN_COMPLETE,
                SecurityEventSeverity.INFO,
                f"セキュリティスキャン完了: {scan_type}",
                scan_type=scan_type,
                findings=findings or {}
            )

    def get_event_summary(self) -> Dict[str, int]:
        """イベントサマリーを取得"""
        with self._counts_lock:
            return self.event_counts.copy()

    def generate_report(self, output_file: Optional[Path] = None) -> str:
        """
        セキュリティレポートを生成

        Args:
            output_file: 出力ファイルパス（Noneの場合は文字列で返す）

        Returns:
            str: レポート内容
        """
        report = "=== セキュリティイベントレポート ===\n\n"
        report += f"生成日時: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}\n"
        report += f"ログファイル: {self.security_log_file}\n\n"

        # イベント統計
        report += "イベント統計:\n"
        summary = self.get_event_summary()

        if summary:
            for event_type, count in sorted(summary.items(), key=lambda x: x[1], reverse=True):
                report += f"  {event_type}: {count} 件\n"
        else:
            report += "  イベントなし\n"

        report += "\n"

        # 最近のイベントを読み込み（最大20件）
        report += "最近のイベント（最新20件）:\n"

        try:
            if self.security_json_file.exists():
                with open(self.security_json_file, 'r', encoding='utf-8') as f:
                    lines = f.readlines()
                    recent_events = lines[-20:] if len(lines) > 20 else lines

                    for line in reversed(recent_events):
                        try:
                            event = json.loads(line)
                            report += f"  [{event['timestamp']}] {event['severity'].upper()}: {event['message']}\n"
                        except json.JSONDecodeError:
                            continue
            else:
                report += "  ログファイルが見つかりません\n"
        except Exception as e:
            report += f"  エラー: {e}\n"

        report += "\n" + "=" * 60 + "\n"

        if output_file:
            output_file.write_text(report, encoding='utf-8')

        return report


# グローバルインスタンス
security_logger = SecurityLogger()


def get_security_logger() -> SecurityLogger:
    """
    セキュリティロガーのグローバルインスタンスを取得

    Returns:
        SecurityLogger: セキュリティロガーインスタンス
    """
    return security_logger


# 便利関数
def log_security_event(
    event_type: SecurityEventType,
    severity: SecurityEventSeverity,
    message: str,
    **kwargs
):
    """セキュリティイベントをログに記録（便利関数）"""
    security_logger.log_event(event_type, severity, message, **kwargs)


if __name__ == '__main__':
    # テスト実行
    print("セキュリティロガーのテスト実行\n")

    logger = get_security_logger()

    # テストイベント
    logger.log_path_validation(True, "/safe/path/file.txt", user="test_user")
    logger.log_path_validation(False, "../../../etc/passwd", reason="パストラバーサル検出")
    logger.log_path_traversal_attempt("../../../etc/passwd", user="malicious_user")
    logger.log_sensitive_data_detection("api_key", "config.py", masked_value="sk_***")
    logger.log_file_access("read", "/safe/file.txt", True)
    logger.log_file_access("write", "/protected/file.txt", False, reason="権限なし")
    logger.log_attack_attempt("command_injection", "危険な文字を検出: $(rm -rf /)", source="script.py")
    logger.log_security_scan("asset_validation", "start")
    logger.log_security_scan("asset_validation", "complete", findings={"errors": 0, "warnings": 2})

    # レポート生成
    print(logger.generate_report())

    print(f"\n✅ ログファイル: {logger.security_log_file}")
    print(f"✅ JSONログファイル: {logger.security_json_file}")
