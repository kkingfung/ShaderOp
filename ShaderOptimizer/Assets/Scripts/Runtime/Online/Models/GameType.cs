#nullable enable

namespace ShaderOp.Online.Models
{
    /// <summary>
    /// ゲームタイプ列挙型
    /// </summary>
    public enum GameType
    {
        /// <summary>三目並べ（ヘックス版）</summary>
        TicTacToeHex = 0,
        
        /// <summary>リバーシ（ヘックス版）</summary>
        HexReversi = 1,
        
        /// <summary>チェッカー（ヘックス版）</summary>
        HexCheckers = 2,
        
        /// <summary>チェス（ヘックス版）</summary>
        HexChess = 3
    }
}
