#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
セキュリティイベントダッシュボード

セキュリティログを解析して視覚的なダッシュボードを生成:
- イベント統計
- 攻撃試行の検出
- タイムラインビュー
- 重大度別集計
- HTMLレポート生成
"""

import os
import sys
import io
import json
import argparse
from pathlib import Path
from datetime import datetime, timedelta
from typing import Dict, List, Any, Optional
from collections import defaultdict, Counter

# Windows環境でUTF-8出力を強制
if sys.platform == 'win32':
    if sys.stdout.encoding != 'utf-8':
        sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
    if sys.stderr.encoding != 'utf-8':
        sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8')


class SecurityDashboard:
    """セキュリティダッシュボードクラス"""

    def __init__(self, log_file: Path):
        """
        Args:
            log_file: セキュリティログファイル（JSONL形式）
        """
        self.log_file = log_file
        self.events: List[Dict[str, Any]] = []

        if log_file.exists():
            self.load_events()

    def load_events(self):
        """ログファイルからイベントを読み込み"""
        with open(self.log_file, 'r', encoding='utf-8') as f:
            for line in f:
                try:
                    event = json.loads(line.strip())
                    self.events.append(event)
                except json.JSONDecodeError:
                    continue

    def get_event_summary(self) -> Dict[str, Any]:
        """イベントサマリーを取得"""
        summary = {
            "total_events": len(self.events),
            "by_type": Counter(),
            "by_severity": Counter(),
            "critical_events": [],
            "attack_attempts": [],
            "recent_events": []
        }

        for event in self.events:
            event_type = event.get("event_type", "unknown")
            severity = event.get("severity", "info")

            summary["by_type"][event_type] += 1
            summary["by_severity"][severity] += 1

            # クリティカルイベント
            if severity == "critical":
                summary["critical_events"].append(event)

            # 攻撃試行
            if "attempt" in event_type:
                summary["attack_attempts"].append(event)

        # 最近のイベント（最新50件）
        summary["recent_events"] = self.events[-50:] if len(self.events) > 50 else self.events

        return summary

    def get_timeline_data(self, hours: int = 24) -> Dict[str, List[Dict[str, Any]]]:
        """タイムライン別のイベントデータを取得"""
        now = datetime.utcnow()
        cutoff_time = now - timedelta(hours=hours)

        timeline = defaultdict(list)

        for event in self.events:
            try:
                timestamp = datetime.fromisoformat(event.get("timestamp", "").replace("Z", "+00:00"))
                if timestamp >= cutoff_time:
                    # 時間単位でグループ化
                    hour_key = timestamp.strftime("%Y-%m-%d %H:00")
                    timeline[hour_key].append(event)
            except (ValueError, AttributeError):
                continue

        return dict(timeline)

    def detect_anomalies(self) -> List[Dict[str, Any]]:
        """異常パターンを検出"""
        anomalies = []

        # 短時間での大量のpath_validation_failure
        recent_failures = [
            e for e in self.events
            if e.get("event_type") == "path_validation_failure"
        ]

        if len(recent_failures) > 10:
            anomalies.append({
                "type": "excessive_path_validation_failures",
                "count": len(recent_failures),
                "severity": "warning",
                "description": "短時間で大量のパス検証エラーが発生しています"
            })

        # パストラバーサル試行
        traversal_attempts = [
            e for e in self.events
            if e.get("event_type") == "path_traversal_attempt"
        ]

        if traversal_attempts:
            anomalies.append({
                "type": "path_traversal_attempts",
                "count": len(traversal_attempts),
                "severity": "critical",
                "description": "パストラバーサル攻撃が検出されました"
            })

        # DoS試行
        dos_attempts = [
            e for e in self.events
            if e.get("event_type") == "dos_attempt"
        ]

        if dos_attempts:
            anomalies.append({
                "type": "dos_attempts",
                "count": len(dos_attempts),
                "severity": "critical",
                "description": "DoS攻撃が検出されました"
            })

        return anomalies

    def generate_text_report(self) -> str:
        """テキスト形式のレポートを生成"""
        summary = self.get_event_summary()
        anomalies = self.detect_anomalies()

        report = "=" * 70 + "\n"
        report += "                セキュリティイベントダッシュボード\n"
        report += "=" * 70 + "\n\n"

        report += f"生成日時: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}\n"
        report += f"ログファイル: {self.log_file}\n"
        report += f"総イベント数: {summary['total_events']} 件\n\n"

        # 重大度別統計
        report += "重大度別統計:\n"
        for severity in ['critical', 'error', 'warning', 'info', 'debug']:
            count = summary['by_severity'].get(severity, 0)
            if count > 0:
                icon = {
                    'critical': '🔴',
                    'error': '🟠',
                    'warning': '🟡',
                    'info': '🔵',
                    'debug': '⚪'
                }.get(severity, '  ')
                report += f"  {icon} {severity.upper()}: {count} 件\n"

        report += "\n"

        # イベントタイプ別統計
        report += "イベントタイプ別統計（上位10件）:\n"
        for event_type, count in summary['by_type'].most_common(10):
            report += f"  - {event_type}: {count} 件\n"

        report += "\n"

        # 異常検出
        if anomalies:
            report += "🚨 異常検出:\n"
            for anomaly in anomalies:
                icon = '🔴' if anomaly['severity'] == 'critical' else '🟡'
                report += f"  {icon} [{anomaly['type']}] {anomaly['description']}\n"
                report += f"     検出数: {anomaly['count']} 件\n"
            report += "\n"

        # クリティカルイベント
        if summary['critical_events']:
            report += f"🔴 クリティカルイベント（{len(summary['critical_events'])} 件）:\n"
            for event in summary['critical_events'][-5:]:  # 最新5件のみ
                report += f"  - [{event.get('timestamp', 'N/A')}] {event.get('message', 'N/A')}\n"
            report += "\n"

        # 攻撃試行
        if summary['attack_attempts']:
            report += f"⚠️  攻撃試行（{len(summary['attack_attempts'])} 件）:\n"
            for event in summary['attack_attempts'][-5:]:  # 最新5件のみ
                report += f"  - [{event.get('timestamp', 'N/A')}] {event.get('message', 'N/A')}\n"
            report += "\n"

        # 最近のイベント
        report += "📋 最近のイベント（最新10件）:\n"
        for event in reversed(summary['recent_events'][-10:]):
            severity_icon = {
                'critical': '🔴',
                'error': '🟠',
                'warning': '🟡',
                'info': '🔵',
                'debug': '⚪'
            }.get(event.get('severity', 'info'), '  ')
            report += f"  {severity_icon} [{event.get('timestamp', 'N/A')}] {event.get('message', 'N/A')}\n"

        report += "\n" + "=" * 70 + "\n"

        return report

    def generate_html_report(self, output_file: Path):
        """HTML形式のレポートを生成"""
        summary = self.get_event_summary()
        anomalies = self.detect_anomalies()
        timeline = self.get_timeline_data(hours=24)

        html = f"""<!DOCTYPE html>
<html lang="ja">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>セキュリティイベントダッシュボード</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            margin: 0;
            padding: 20px;
            background-color: #f5f5f5;
        }}
        .container {{
            max-width: 1200px;
            margin: 0 auto;
            background-color: white;
            padding: 30px;
            border-radius: 8px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }}
        h1 {{
            color: #333;
            border-bottom: 3px solid #007bff;
            padding-bottom: 10px;
        }}
        .stats-grid {{
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 20px;
            margin: 20px 0;
        }}
        .stat-card {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 4px 6px rgba(0,0,0,0.1);
        }}
        .stat-card h3 {{
            margin: 0 0 10px 0;
            font-size: 14px;
            opacity: 0.9;
        }}
        .stat-card .value {{
            font-size: 32px;
            font-weight: bold;
        }}
        .severity-critical {{ background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); }}
        .severity-error {{ background: linear-gradient(135deg, #fa709a 0%, #fee140 100%); }}
        .severity-warning {{ background: linear-gradient(135deg, #ffecd2 0%, #fcb69f 100%); color: #333; }}
        .severity-info {{ background: linear-gradient(135deg, #a1c4fd 0%, #c2e9fb 100%); color: #333; }}
        .anomaly {{
            background-color: #fff3cd;
            border-left: 4px solid #ffc107;
            padding: 15px;
            margin: 10px 0;
            border-radius: 4px;
        }}
        .anomaly.critical {{
            background-color: #f8d7da;
            border-left-color: #dc3545;
        }}
        .event-list {{
            background-color: #f8f9fa;
            padding: 15px;
            border-radius: 4px;
            max-height: 400px;
            overflow-y: auto;
        }}
        .event-item {{
            background-color: white;
            padding: 10px;
            margin: 5px 0;
            border-radius: 4px;
            border-left: 3px solid #007bff;
        }}
        .event-item.critical {{ border-left-color: #dc3545; }}
        .event-item.error {{ border-left-color: #fd7e14; }}
        .event-item.warning {{ border-left-color: #ffc107; }}
        .timestamp {{
            font-size: 12px;
            color: #6c757d;
        }}
        table {{
            width: 100%;
            border-collapse: collapse;
            margin: 20px 0;
        }}
        th, td {{
            padding: 12px;
            text-align: left;
            border-bottom: 1px solid #ddd;
        }}
        th {{
            background-color: #007bff;
            color: white;
        }}
        tr:hover {{
            background-color: #f5f5f5;
        }}
    </style>
</head>
<body>
    <div class="container">
        <h1>🔒 セキュリティイベントダッシュボード</h1>
        <p><strong>生成日時:</strong> {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}</p>
        <p><strong>ログファイル:</strong> {self.log_file}</p>

        <h2>📊 重大度別統計</h2>
        <div class="stats-grid">
            <div class="stat-card severity-critical">
                <h3>Critical</h3>
                <div class="value">{summary['by_severity'].get('critical', 0)}</div>
            </div>
            <div class="stat-card severity-error">
                <h3>Error</h3>
                <div class="value">{summary['by_severity'].get('error', 0)}</div>
            </div>
            <div class="stat-card severity-warning">
                <h3>Warning</h3>
                <div class="value">{summary['by_severity'].get('warning', 0)}</div>
            </div>
            <div class="stat-card severity-info">
                <h3>Info</h3>
                <div class="value">{summary['by_severity'].get('info', 0)}</div>
            </div>
        </div>
"""

        # 異常検出
        if anomalies:
            html += "<h2>🚨 異常検出</h2>\n"
            for anomaly in anomalies:
                css_class = "critical" if anomaly['severity'] == 'critical' else ""
                html += f'<div class="anomaly {css_class}">\n'
                html += f'<strong>{anomaly["type"]}</strong>: {anomaly["description"]}<br>\n'
                html += f'検出数: {anomaly["count"]} 件\n'
                html += '</div>\n'

        # イベントタイプ統計
        html += "<h2>📈 イベントタイプ別統計</h2>\n"
        html += "<table>\n<tr><th>イベントタイプ</th><th>件数</th></tr>\n"
        for event_type, count in summary['by_type'].most_common(15):
            html += f"<tr><td>{event_type}</td><td>{count}</td></tr>\n"
        html += "</table>\n"

        # 最近のイベント
        html += "<h2>📋 最近のイベント</h2>\n"
        html += '<div class="event-list">\n'
        for event in reversed(summary['recent_events'][-20:]):
            severity = event.get('severity', 'info')
            html += f'<div class="event-item {severity}">\n'
            html += f'<div class="timestamp">{event.get("timestamp", "N/A")}</div>\n'
            html += f'<strong>[{event.get("event_type", "unknown")}]</strong> {event.get("message", "N/A")}\n'
            html += '</div>\n'
        html += '</div>\n'

        html += """
    </div>
</body>
</html>
"""

        output_file.write_text(html, encoding='utf-8')

    def generate_json_report(self, output_file: Path):
        """JSON形式のレポートを生成"""
        summary = self.get_event_summary()
        anomalies = self.detect_anomalies()
        timeline = self.get_timeline_data(hours=24)

        report = {
            "generated_at": datetime.now().isoformat(),
            "log_file": str(self.log_file),
            "summary": {
                "total_events": summary["total_events"],
                "by_type": dict(summary["by_type"]),
                "by_severity": dict(summary["by_severity"])
            },
            "anomalies": anomalies,
            "timeline_24h": {
                hour: len(events)
                for hour, events in timeline.items()
            },
            "critical_events": summary["critical_events"],
            "attack_attempts": summary["attack_attempts"]
        }

        output_file.write_text(json.dumps(report, indent=2, ensure_ascii=False), encoding='utf-8')


def main():
    """メイン関数"""
    parser = argparse.ArgumentParser(description='セキュリティイベントダッシュボード')
    parser.add_argument(
        '--log-file',
        default='logs/security_events.jsonl',
        help='セキュリティログファイルのパス'
    )
    parser.add_argument(
        '--output-html',
        help='HTML形式のレポート出力先'
    )
    parser.add_argument(
        '--output-json',
        help='JSON形式のレポート出力先'
    )
    parser.add_argument(
        '--text-only',
        action='store_true',
        help='テキスト形式のレポートのみ表示'
    )

    args = parser.parse_args()

    log_file = Path(args.log_file)

    if not log_file.exists():
        print(f"❌ ログファイルが見つかりません: {log_file}")
        print("セキュリティロガーが有効化されていない可能性があります")
        sys.exit(1)

    dashboard = SecurityDashboard(log_file)

    # テキストレポート
    text_report = dashboard.generate_text_report()
    print(text_report)

    if not args.text_only:
        # HTMLレポート
        if args.output_html:
            html_file = Path(args.output_html)
        else:
            html_file = Path("security_dashboard.html")

        dashboard.generate_html_report(html_file)
        print(f"✅ HTMLレポート生成: {html_file}")

        # JSONレポート
        if args.output_json:
            json_file = Path(args.output_json)
            dashboard.generate_json_report(json_file)
            print(f"✅ JSONレポート生成: {json_file}")


if __name__ == '__main__':
    main()
