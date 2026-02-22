#nullable enable

namespace ShaderOp.Minigames.Games
{
    /// <summary>
    /// ゲームの状態
    /// </summary>
    public enum GameState
    {
        /// <summary>初期化前</summary>
        Uninitialized,

        /// <summary>ゲーム開始待ち</summary>
        WaitingToStart,

        /// <summary>プレイ中</summary>
        Playing,

        /// <summary>一時停止</summary>
        Paused,

        /// <summary>ゲーム終了</summary>
        GameOver,

        /// <summary>Player 1勝利</summary>
        Player1Won,

        /// <summary>Player 2勝利</summary>
        Player2Won,

        /// <summary>Player 3勝利</summary>
        Player3Won,

        /// <summary>Player 4勝利</summary>
        Player4Won,

        /// <summary>引き分け</summary>
        Draw
    }
}
