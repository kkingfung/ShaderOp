#!/usr/bin/env node
/**
 * Session-Start Hook
 * セッション開始時に実行されるフック
 *
 * 機能:
 * - 前回セッションのコンテキスト読み込み
 * - 作業環境の準備
 * - 統計情報の表示
 */

const fs = require('fs');
const path = require('path');

/**
 * メイン処理
 */
async function main() {
  try {
    console.log('[SessionStart] セッション開始');
    console.log('='.repeat(60));

    // プロジェクト情報を表示
    showProjectInfo();

    // 前回セッションのコンテキストを読み込み
    loadPreviousContext();

    // 統計情報を表示
    showStatistics();

    // 最近の活動を表示
    showRecentActivity();

    console.log('='.repeat(60));
    console.log('[SessionStart] 準備完了\n');

    process.exit(0);

  } catch (error) {
    console.error('[SessionStart] エラー:', error.message);
    process.exit(0);
  }
}

/**
 * プロジェクト情報を表示
 */
function showProjectInfo() {
  console.log('📁 プロジェクト: ShaderOp');
  console.log('🎯 目的: Unity Shader開発 & 自動化ツール');
  console.log('');
}

/**
 * 前回セッションのコンテキストを読み込み
 */
function loadPreviousContext() {
  try {
    const contextFile = path.join(process.cwd(), '.claude', 'context', 'last-session.json');

    if (fs.existsSync(contextFile)) {
      const context = JSON.parse(fs.readFileSync(contextFile, 'utf8'));

      console.log('📝 前回のセッション:');
      console.log(`   日時: ${new Date(context.timestamp).toLocaleString('ja-JP')}`);
      console.log(`   作業内容: ${context.summary || '不明'}`);
      console.log('');
    }
  } catch (error) {
    // エラーは無視
  }
}

/**
 * 統計情報を表示
 */
function showStatistics() {
  try {
    const statsFile = path.join(process.cwd(), '.claude', 'stats', 'tool-usage.json');

    if (fs.existsSync(statsFile)) {
      const stats = JSON.parse(fs.readFileSync(statsFile, 'utf8'));

      // 最も使用されたツール
      const sortedTools = Object.entries(stats)
        .sort((a, b) => b[1].count - a[1].count)
        .slice(0, 3);

      console.log('📊 統計情報:');
      sortedTools.forEach(([tool, data], index) => {
        console.log(`   ${index + 1}. ${tool}: ${data.count}回`);
      });
      console.log('');
    }
  } catch (error) {
    // エラーは無視
  }
}

/**
 * 最近の活動を表示
 */
function showRecentActivity() {
  try {
    const patternsFile = path.join(process.cwd(), '.claude', 'patterns', 'extracted-patterns.json');

    if (fs.existsSync(patternsFile)) {
      const patterns = JSON.parse(fs.readFileSync(patternsFile, 'utf8'));

      // 最新のパターンを取得
      const recentPatterns = patterns.slice(-3).reverse();

      if (recentPatterns.length > 0) {
        console.log('🎨 最近検出されたパターン:');
        recentPatterns.forEach(pattern => {
          console.log(`   • ${pattern.description} (信頼度: ${(pattern.confidence * 100).toFixed(0)}%)`);
        });
        console.log('');
      }
    }
  } catch (error) {
    // エラーは無視
  }
}

main();
