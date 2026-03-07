#nullable enable

using System;

namespace ShaderOp.Core.Services
{
    /// <summary>
    /// ゲーム状態管理サービス
    /// </summary>
    public interface IGameStateService
    {
        /// <summary>現在のゲームモード</summary>
        GameMode CurrentMode { get; }

        /// <summary>モード変更イベント</summary>
        event Action<GameMode>? OnGameModeChanged;

        /// <summary>メインメニューに戻る</summary>
        void LoadMainMenu();

        /// <summary>キャラクターカスタマイズをロード</summary>
        void LoadCharacterCustomization();

        /// <summary>部屋デコレーションをロード</summary>
        void LoadRoomDecoration();

        /// <summary>ミニゲームをロード</summary>
        void LoadMinigame(string minigameName);

        /// <summary>Tic-Tac-Toe Hexをロード</summary>
        void LoadTicTacToeHex();

        /// <summary>Hex Reversiをロード</summary>
        void LoadHexReversi();

        /// <summary>Hex Checkersをロード</summary>
        void LoadHexCheckers();

        /// <summary>Hex Chessをロード</summary>
        void LoadHexChess();

        /// <summary>アプリケーション終了</summary>
        void QuitApplication();
    }

    /// <summary>
    /// ゲームモード定義
    /// </summary>
    public enum GameMode
    {
        MainMenu,
        CharacterCustomization,
        RoomDecoration,
        Minigame
    }
}
