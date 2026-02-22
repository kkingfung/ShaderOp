# MCP (Model Context Protocol) Server 設定

## 概要

ShaderOpプロジェクト用のMCPサーバー設定。Claude CodeがUnityプロジェクトと直接連携するための設定です。

## 有効化されているサーバー

### 1. unity-project (Filesystem Server)

**用途**: Unity プロジェクトファイルへの直接アクセス

**機能**:
- ShaderOptimizer/ 配下のファイル読み書き
- .shadergraph, .hlsl, .cs ファイルの検索
- マテリアル、プレハブの確認

**コマンド**:
```bash
npx -y @modelcontextprotocol/server-filesystem D:\PersonalGameDev\ShaderOp\ShaderOptimizer
```

**使用例**:
- 「Assets/Shaders/ShaderGraphs/Character/ 配下のファイルを確認」
- 「SG_Character_Base.shadergraph を編集」
- 「新しいHLSLファイルを作成」

### 2. unity-docs (Fetch Server)

**用途**: Unity公式ドキュメント、Shader Graph リファレンス参照

**許可ドメイン**:
- `docs.unity3d.com` - Unity マニュアル
- `docs.unity.com` - 最新Unityドキュメント
- `github.com` - Unity GitHub リポジトリ、サンプルコード

**コマンド**:
```bash
npx -y @modelcontextprotocol/server-fetch
```

**使用例**:
- 「URP Shader Graph の最新ドキュメントを確認」
- 「Burst Compiler の使い方を調べる」
- 「Unity-Chan Toon Shader の GitHub を参照」

### 3. shader-graph-helper (Custom Server - 無効化中)

**用途**: Shader Graph 解析・検証カスタムサーバー

**機能**:
- .shadergraph ファイルのJSON解析
- ノード接続検証
- パフォーマンス警告
- ベストプラクティスチェック

**ステータス**: 🔴 未実装（disabled: true）

**実装予定**:
- Week 2-3（シェーダーテンプレート完成後）
- Python MCP サーバー実装
- `/claude/mcp/servers/shader_graph_helper/` に配置

## セットアップ手順

### 前提条件

```bash
# Node.js 18+ が必要
node --version  # v18.0.0 以上

# npm パッケージマネージャー
npm --version

# Python 3.10+ (カスタムサーバー用、オプション)
python --version
```

### 1. MCP サーバー設定の有効化

Claude Code の設定ファイルを編集：

**Windows:**
```
%APPDATA%\Claude\claude_desktop_config.json
```

**macOS/Linux:**
```
~/.config/Claude/claude_desktop_config.json
```

以下を追加：

```json
{
  "mcpServers": {
    "unity-project": {
      "command": "npx",
      "args": [
        "-y",
        "@modelcontextprotocol/server-filesystem",
        "D:\\PersonalGameDev\\ShaderOp\\ShaderOptimizer"
      ]
    },
    "unity-docs": {
      "command": "npx",
      "args": [
        "-y",
        "@modelcontextprotocol/server-fetch"
      ],
      "env": {
        "ALLOWED_DOMAINS": "docs.unity3d.com,docs.unity.com,github.com"
      }
    }
  }
}
```

### 2. Claude Code を再起動

設定反映のため、Claude Code アプリケーションを再起動してください。

### 3. 動作確認

Claude Code で以下を試してください：

```
「Unity プロジェクトの Assets/Shaders/ フォルダを確認して」
「URP Shader Graph の最新ドキュメントを参照して」
```

MCPサーバーが正しく動作していれば、直接Unityプロジェクトファイルにアクセスできます。

## トラブルシューティング

### MCPサーバーが起動しない

**症状**: Claude Code が「MCP server failed to start」エラー

**解決策**:
```bash
# Node.js のバージョン確認
node --version

# npx が正しく動作するか確認
npx -y @modelcontextprotocol/server-filesystem --help

# パスにスペースが含まれる場合、エスケープ確認
# Windows: バックスラッシュでエスケープ
# macOS/Linux: クォートで囲む
```

### ファイルアクセスできない

**症状**: 「Permission denied」エラー

**解決策**:
```bash
# プロジェクトフォルダの権限確認
ls -la D:\PersonalGameDev\ShaderOp\ShaderOptimizer

# 読み書き権限があるか確認
# 必要に応じて権限変更
```

### ドキュメント取得できない

**症状**: 「Fetch failed」エラー

**解決策**:
```bash
# 環境変数 ALLOWED_DOMAINS が正しく設定されているか確認
# プロキシ環境の場合、HTTP_PROXY/HTTPS_PROXY 設定

# 手動でテスト
curl https://docs.unity3d.com/
```

## カスタム Shader Graph Helper サーバー実装予定

### 機能仕様

```python
# mcp_server_shadergraph.py (予定)

class ShaderGraphServer:
    def analyze_shadergraph(self, filepath: str) -> dict:
        """
        .shadergraph ファイルを解析

        Returns:
        - ノード数、接続数
        - 使用しているCustom Function
        - パフォーマンス警告
        - ベストプラクティス違反
        """
        pass

    def validate_connections(self, filepath: str) -> list:
        """
        ノード接続の妥当性チェック

        Returns:
        - 未接続ノード警告
        - 型不一致エラー
        - 循環参照検出
        """
        pass

    def suggest_optimizations(self, filepath: str) -> list:
        """
        最適化提案

        Returns:
        - 不要なノード削除提案
        - より効率的なノード代替案
        - モバイル最適化ヒント
        """
        pass
```

### 実装タイミング

- **Week 2-3**: 基本実装（analyze, validate）
- **Week 4**: 最適化提案機能
- **Week 5**: ドキュメント生成機能

## セキュリティ考慮事項

### Filesystem Server

- ✅ ShaderOptimizer/ 配下のみアクセス可能
- ✅ 親ディレクトリへのトラバーサル防止
- ✅ システムファイルへのアクセス不可

### Fetch Server

- ✅ 許可ドメインのみアクセス可能
- ✅ docs.unity3d.com, docs.unity.com, github.com
- ✅ 外部スクリプト実行不可

### Custom Server

- ⚠️ Python実行のため、コードレビュー必須
- ✅ サンドボックス環境で実行
- ✅ ファイルシステムアクセス制限

## 参考リソース

- **MCP 公式ドキュメント**: https://modelcontextprotocol.io/
- **MCP Filesystem Server**: https://github.com/modelcontextprotocol/servers/tree/main/src/filesystem
- **MCP Fetch Server**: https://github.com/modelcontextprotocol/servers/tree/main/src/fetch
- **MCP Python SDK**: https://github.com/modelcontextprotocol/python-sdk

---
最終更新: 2026-02-21
