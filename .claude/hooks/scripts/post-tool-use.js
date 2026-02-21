#!/usr/bin/env node
/**
 * Post-Tool-Use Hook
 * ツール使用後に実行されるフック
 *
 * 機能:
 * - コーディングパターンの抽出
 * - 使用統計の記録
 * - 学習データの蓄積
 */

const fs = require('fs');
const path = require('path');

/**
 * メイン処理
 */
async function main() {
  try {
    const context = JSON.parse(process.argv[2] || '{}');

    const toolName = context.toolName;
    const toolArgs = context.toolArgs || {};
    const result = context.result || {};

    console.log(`[PostToolUse] ツール実行後: ${toolName}`);

    // 統計データを記録
    recordToolUsage(toolName, toolArgs);

    // Write/Editツールの場合、パターンを抽出
    if (toolName === 'Write' || toolName === 'Edit') {
      const content = toolArgs.content || toolArgs.new_string || '';
      const filePath = toolArgs.file_path || '';

      extractPatterns(filePath, content);
    }

    console.log('[PostToolUse] パターン抽出完了');
    process.exit(0);

  } catch (error) {
    console.error('[PostToolUse] エラー:', error.message);
    process.exit(0);
  }
}

/**
 * ツール使用統計を記録
 */
function recordToolUsage(toolName, toolArgs) {
  try {
    const statsDir = path.join(process.cwd(), '.claude', 'stats');
    const statsFile = path.join(statsDir, 'tool-usage.json');

    // statsディレクトリを作成
    if (!fs.existsSync(statsDir)) {
      fs.mkdirSync(statsDir, { recursive: true });
    }

    // 既存の統計を読み込み
    let stats = {};
    if (fs.existsSync(statsFile)) {
      stats = JSON.parse(fs.readFileSync(statsFile, 'utf8'));
    }

    // 統計を更新
    if (!stats[toolName]) {
      stats[toolName] = { count: 0, lastUsed: null };
    }
    stats[toolName].count++;
    stats[toolName].lastUsed = new Date().toISOString();

    // ファイルに書き込み
    fs.writeFileSync(statsFile, JSON.stringify(stats, null, 2));

    console.log(`[PostToolUse] 統計更新: ${toolName} (${stats[toolName].count}回目)`);

  } catch (error) {
    console.warn(`[PostToolUse] 統計記録失敗: ${error.message}`);
  }
}

/**
 * コーディングパターンを抽出
 */
function extractPatterns(filePath, content) {
  try {
    const patterns = [];

    // ファイル拡張子を判定
    const ext = path.extname(filePath);

    // C# ファイルの場合
    if (ext === '.cs') {
      // async/await パターン
      if (/async\s+\w+\s+\w+/.test(content) && /await\s+/.test(content)) {
        patterns.push({
          type: 'csharp-async-pattern',
          description: 'async/awaitの使用',
          confidence: 0.9,
        });
      }

      // UniTask パターン
      if (/UniTask/.test(content)) {
        patterns.push({
          type: 'unitask-pattern',
          description: 'UniTaskの使用',
          confidence: 0.95,
        });
      }

      // LINQ パターン
      if (/\.Select\(|\.Where\(|\.FirstOrDefault\(/.test(content)) {
        patterns.push({
          type: 'linq-pattern',
          description: 'LINQの使用',
          confidence: 0.9,
        });
      }
    }

    // シェーダーファイルの場合
    if (ext === '.shader' || ext === '.hlsl') {
      // テクスチャサンプリング
      if (/tex2D|SAMPLE_TEXTURE2D/.test(content)) {
        patterns.push({
          type: 'shader-texture-sampling',
          description: 'テクスチャサンプリング',
          confidence: 0.95,
        });
      }
    }

    // Python ファイルの場合
    if (ext === '.py') {
      // Type hints
      if (/def\s+\w+\([^)]*:\s*\w+/.test(content)) {
        patterns.push({
          type: 'python-type-hints',
          description: '型ヒントの使用',
          confidence: 0.9,
        });
      }
    }

    // パターンを保存
    if (patterns.length > 0) {
      savePatterns(patterns);
    }

  } catch (error) {
    console.warn(`[PostToolUse] パターン抽出失敗: ${error.message}`);
  }
}

/**
 * パターンを保存
 */
function savePatterns(patterns) {
  try {
    const patternsDir = path.join(process.cwd(), '.claude', 'patterns');
    const patternsFile = path.join(patternsDir, 'extracted-patterns.json');

    if (!fs.existsSync(patternsDir)) {
      fs.mkdirSync(patternsDir, { recursive: true });
    }

    // 既存のパターンを読み込み
    let allPatterns = [];
    if (fs.existsSync(patternsFile)) {
      allPatterns = JSON.parse(fs.readFileSync(patternsFile, 'utf8'));
    }

    // 新しいパターンを追加
    patterns.forEach(pattern => {
      pattern.timestamp = new Date().toISOString();
      allPatterns.push(pattern);
    });

    // 最新100件のみ保持
    if (allPatterns.length > 100) {
      allPatterns = allPatterns.slice(-100);
    }

    fs.writeFileSync(patternsFile, JSON.stringify(allPatterns, null, 2));

    console.log(`[PostToolUse] パターン保存: ${patterns.length}件`);

  } catch (error) {
    console.warn(`[PostToolUse] パターン保存失敗: ${error.message}`);
  }
}

main();
