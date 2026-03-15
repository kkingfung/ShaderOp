# Profiler Screenshots Directory

このディレクトリは、Unity Profiler分析の証拠スクリーンショットを保存します。

## 必要なファイル

### HexChess
- `hexchess_before.png` - Week 1ベースライン（もしあれば）
- `hexchess_after_cpu.png` - Week 3最適化後のCPU Usage
- `hexchess_after_memory.png` - Week 3最適化後のMemory Profiler

### HexCheckers
- `hexcheckers_before.png` - Week 2ベースライン
- `hexcheckers_after_cpu.png` - Week 3最適化後のCPU Usage
- `hexcheckers_after_memory.png` - Week 3最適化後のMemory Profiler

### HexReversi
- `hexreversi_before.png` - Week 2ベースライン
- `hexreversi_after_cpu.png` - Week 3最適化後のCPU Usage
- `hexreversi_after_memory.png` - Week 3最適化後のMemory Profiler

### TicTacToeHex
- `tictactoe_baseline_cpu.png` - 現状ベースライン（最適化不要）
- `tictactoe_baseline_memory.png` - 現状メモリ使用量

## スクリーンショット取得方法

1. Unity Editorで `Window → Analysis → Profiler` を開く
2. Deep Profilingを有効化
3. 各シーンをPlay Modeで実行
4. Profilerでフレームを選択
5. **CPU Module**: Hierarchy Viewで対象メソッドを展開
6. **Memory Module**: GC Allocを確認
7. スクリーンショット取得（Windows: Win+Shift+S, Mac: Cmd+Shift+4）
8. このディレクトリに保存

## 測定完了後

`PHASE4_PROFILER_RESULTS.md` の「6.2 比較表」を実測値で更新してください。

---
最終更新: 2026-03-15
