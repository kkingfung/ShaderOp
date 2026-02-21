#!/usr/bin/env node
/**
 * Pre-Tool-Use Hook
 * ツール使用前に実行されるフック
 *
 * 機能:
 * - ツール使用のバリデーション
 * - コンテキストの準備
 * - セキュリティチェック
 */

const fs = require('fs');
const path = require('path');

/**
 * メイン処理
 */
async function main() {
  try {
    const context = JSON.parse(process.argv[2] || '{}');

    // ツール情報を取得
    const toolName = context.toolName;
    const toolArgs = context.toolArgs || {};

    console.log(`[PreToolUse] ツール実行前: ${toolName}`);

    // Write/Editツールの場合、ファイルパスをチェック
    if (toolName === 'Write' || toolName === 'Edit') {
      const filePath = toolArgs.file_path || '';

      // 機密ファイルへの書き込みを警告
      if (isSensitiveFile(filePath)) {
        console.warn(`[警告] 機密ファイルへの書き込み: ${filePath}`);
      }

      // バックアップディレクトリにバックアップを作成
      if (fs.existsSync(filePath)) {
        createBackup(filePath);
      }
    }

    // Bash toolの場合、危険なコマンドをチェック
    if (toolName === 'Bash') {
      const command = toolArgs.command || '';

      if (isDangerousCommand(command)) {
        console.error(`[エラー] 危険なコマンド検出: ${command}`);
        process.exit(1); // フックを失敗させてツール実行をブロック
      }
    }

    console.log('[PreToolUse] バリデーション完了');
    process.exit(0);

  } catch (error) {
    console.error('[PreToolUse] エラー:', error.message);
    process.exit(0); // エラーでもツール実行は継続
  }
}

/**
 * 機密ファイルかどうかをチェック
 */
function isSensitiveFile(filePath) {
  const sensitivePatterns = [
    /\.env$/,
    /\.key$/,
    /\.pem$/,
    /credentials/i,
    /secrets/i,
    /password/i,
  ];

  return sensitivePatterns.some(pattern => pattern.test(filePath));
}

/**
 * 危険なコマンドかどうかをチェック
 */
function isDangerousCommand(command) {
  const dangerousPatterns = [
    /rm\s+-rf\s+\//,  // rm -rf / (ルート削除)
    /:\(\)\{\s*:\|:&\s*\};:/,  // fork bomb
    /mkfs/,  // ファイルシステムフォーマット
  ];

  return dangerousPatterns.some(pattern => pattern.test(command));
}

/**
 * ファイルのバックアップを作成
 */
function createBackup(filePath) {
  try {
    const backupDir = path.join(process.cwd(), '.claude', 'backups');

    // バックアップディレクトリを作成
    if (!fs.existsSync(backupDir)) {
      fs.mkdirSync(backupDir, { recursive: true });
    }

    // タイムスタンプ付きバックアップファイル名
    const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
    const fileName = path.basename(filePath);
    const backupPath = path.join(backupDir, `${fileName}.${timestamp}.bak`);

    // ファイルをコピー
    fs.copyFileSync(filePath, backupPath);
    console.log(`[PreToolUse] バックアップ作成: ${backupPath}`);

  } catch (error) {
    console.warn(`[PreToolUse] バックアップ作成失敗: ${error.message}`);
  }
}

main();
