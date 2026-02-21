#!/usr/bin/env node
/**
 * Session-End Hook
 * セッション終了時に実行されるフック
 *
 * 機能:
 * - セッションコンテキストの保存
 * - パターンの分析と保存
 * - レポート生成
 */

const fs = require('fs');
const path = require('path');
const readline = require('readline');

/**
 * メイン処理
 */
async function main() {
  try {
    console.log('\n[SessionEnd] セッション終了処理を開始');
    console.log('='.repeat(60));

    // セッションのサマリーを取得
    const summary = await getSessionSummary();

    // コンテキストを保存
    saveSessionContext(summary);

    // パターンを分析
    analyzePatterns();

    // レポートを生成
    generateReport(summary);

    console.log('='.repeat(60));
    console.log('[SessionEnd] お疲れ様でした！\n');

    process.exit(0);

  } catch (error) {
    console.error('[SessionEnd] エラー:', error.message);
    process.exit(0);
  }
}

/**
 * セッションのサマリーを取得
 */
async function getSessionSummary() {
  // 統計から作業内容を推測
  try {
    const statsFile = path.join(process.cwd(), '.claude', 'stats', 'tool-usage.json');

    if (fs.existsSync(statsFile)) {
      const stats = JSON.parse(fs.readFileSync(statsFile, 'utf8'));

      const summary = {
        timestamp: new Date().toISOString(),
        toolsUsed: Object.keys(stats).length,
        mostUsedTool: getMostUsedTool(stats),
        summary: 'シェーダー開発作業',
      };

      return summary;
    }
  } catch (error) {
    // エラーは無視
  }

  return {
    timestamp: new Date().toISOString(),
    summary: '一般的な開発作業',
  };
}

/**
 * 最も使用されたツールを取得
 */
function getMostUsedTool(stats) {
  const entries = Object.entries(stats);
  if (entries.length === 0) return 'なし';

  const sorted = entries.sort((a, b) => b[1].count - a[1].count);
  return sorted[0][0];
}

/**
 * セッションコンテキストを保存
 */
function saveSessionContext(summary) {
  try {
    const contextDir = path.join(process.cwd(), '.claude', 'context');
    const contextFile = path.join(contextDir, 'last-session.json');
    const historyFile = path.join(contextDir, 'session-history.json');

    // ディレクトリを作成
    if (!fs.existsSync(contextDir)) {
      fs.mkdirSync(contextDir, { recursive: true });
    }

    // 最新セッションを保存
    fs.writeFileSync(contextFile, JSON.stringify(summary, null, 2));

    // 履歴に追加
    let history = [];
    if (fs.existsSync(historyFile)) {
      history = JSON.parse(fs.readFileSync(historyFile, 'utf8'));
    }
    history.push(summary);

    // 最新30件のみ保持
    if (history.length > 30) {
      history = history.slice(-30);
    }

    fs.writeFileSync(historyFile, JSON.stringify(history, null, 2));

    console.log(`💾 セッションコンテキスト保存完了`);

  } catch (error) {
    console.warn(`[SessionEnd] コンテキスト保存失敗: ${error.message}`);
  }
}

/**
 * パターンを分析
 */
function analyzePatterns() {
  try {
    const patternsFile = path.join(process.cwd(), '.claude', 'patterns', 'extracted-patterns.json');

    if (fs.existsSync(patternsFile)) {
      const patterns = JSON.parse(fs.readFileSync(patternsFile, 'utf8'));

      // パターンタイプ別に集計
      const patternCounts = {};
      patterns.forEach(pattern => {
        patternCounts[pattern.type] = (patternCounts[pattern.type] || 0) + 1;
      });

      // 頻出パターンを表示
      const frequent = Object.entries(patternCounts)
        .filter(([_, count]) => count >= 3)
        .sort((a, b) => b[1] - a[1]);

      if (frequent.length > 0) {
        console.log('🔍 頻出パターン検出:');
        frequent.forEach(([type, count]) => {
          console.log(`   • ${type}: ${count}回`);
        });
        console.log('   💡 これらをskillsに追加することを検討してください');
      }
    }
  } catch (error) {
    // エラーは無視
  }
}

/**
 * レポートを生成
 */
function generateReport(summary) {
  try {
    const reportsDir = path.join(process.cwd(), '.claude', 'reports');
    const reportFile = path.join(reportsDir, `session-${Date.now()}.md`);

    if (!fs.existsSync(reportsDir)) {
      fs.mkdirSync(reportsDir, { recursive: true });
    }

    const report = `# セッションレポート

## 日時
${new Date(summary.timestamp).toLocaleString('ja-JP')}

## サマリー
${summary.summary}

## 統計
- 使用ツール数: ${summary.toolsUsed || 0}
- 最も使用したツール: ${summary.mostUsedTool || 'なし'}

---
自動生成レポート
`;

    fs.writeFileSync(reportFile, report);
    console.log(`📄 レポート生成: ${path.basename(reportFile)}`);

  } catch (error) {
    console.warn(`[SessionEnd] レポート生成失敗: ${error.message}`);
  }
}

main();
