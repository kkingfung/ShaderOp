#nullable enable

using System;
using ShaderOp.Minigames.HexGrid;

namespace ShaderOp.Minigames.Games
{
    /// <summary>
    /// ヘックスボードゲームの基底Model
    /// </summary>
    /// <remarks>
    /// すべてのミニゲームの共通ロジックを定義
    /// ゲーム固有のルールは派生クラスで実装
    /// </remarks>
    public abstract class HexBoardGameModel
    {
        /// <summary>ヘックスグリッド</summary>
        public HexGrid.HexGrid Grid { get; protected set; }

        /// <summary>現在のプレイヤーインデックス（0-3）</summary>
        public int CurrentPlayerIndex { get; protected set; }

        /// <summary>現在のプレイヤー種別</summary>
        public PieceType CurrentPlayer => IndexToPieceType(CurrentPlayerIndex);

        /// <summary>ゲーム状態</summary>
        public GameState State { get; protected set; }

        /// <summary>プレイヤー数</summary>
        public int PlayerCount { get; protected set; }

        /// <summary>ゲーム状態変更イベント</summary>
        public event Action<GameState>? OnGameStateChanged;

        /// <summary>ターン変更イベント</summary>
        public event Action<int>? OnTurnChanged;

        /// <summary>タイル更新イベント（駒配置・削除時）</summary>
        public event Action<HexCoordinate>? OnTileUpdated;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        protected HexBoardGameModel(int playerCount = 2)
        {
            Grid = new HexGrid.HexGrid(1.0f);
            CurrentPlayerIndex = 0;
            State = GameState.Uninitialized;
            PlayerCount = Math.Clamp(playerCount, 1, 4);
        }

        /// <summary>
        /// ゲームを初期化（派生クラスで実装）
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// ゲーム固有のルール検証（派生クラスで実装）
        /// </summary>
        public abstract bool IsValidMove(HexCoordinate from, HexCoordinate to);

        /// <summary>
        /// 手を実行（派生クラスでオーバーライド可能）
        /// </summary>
        public virtual bool ExecuteMove(HexCoordinate from, HexCoordinate to)
        {
            if (State != GameState.Playing)
            {
                return false;
            }

            if (!IsValidMove(from, to))
            {
                return false;
            }

            // 移動処理（基本実装）
            HexTile? fromTile = Grid.GetTile(from);
            HexTile? toTile = Grid.GetTile(to);

            if (fromTile == null || toTile == null)
            {
                return false;
            }

            // 駒を移動
            toTile.Piece = fromTile.Piece;
            fromTile.Piece = PieceType.None;

            OnTileUpdated?.Invoke(from);
            OnTileUpdated?.Invoke(to);

            // 勝利判定
            if (CheckWinCondition())
            {
                SetGameState(GetWinStateForPlayer(CurrentPlayerIndex));
                return true;
            }

            // 引き分け判定
            if (CheckDrawCondition())
            {
                SetGameState(GameState.Draw);
                return true;
            }

            // 次のプレイヤーへ
            NextTurn();
            return true;
        }

        /// <summary>
        /// 勝利条件チェック（派生クラスで実装）
        /// </summary>
        protected abstract bool CheckWinCondition();

        /// <summary>
        /// 引き分け条件チェック（派生クラスで実装）
        /// </summary>
        protected abstract bool CheckDrawCondition();

        /// <summary>
        /// 次のターンへ
        /// </summary>
        protected virtual void NextTurn()
        {
            CurrentPlayerIndex = (CurrentPlayerIndex + 1) % PlayerCount;
            OnTurnChanged?.Invoke(CurrentPlayerIndex);
        }

        /// <summary>
        /// ゲーム状態を設定
        /// </summary>
        protected void SetGameState(GameState newState)
        {
            State = newState;
            OnGameStateChanged?.Invoke(newState);
        }

        /// <summary>
        /// ゲームをリセット
        /// </summary>
        public virtual void Reset()
        {
            Grid.ResetAll();
            CurrentPlayerIndex = 0;
            SetGameState(GameState.WaitingToStart);
            Initialize();
        }

        /// <summary>
        /// ゲームを開始
        /// </summary>
        public virtual void StartGame()
        {
            SetGameState(GameState.Playing);
        }

        /// <summary>
        /// ゲームを一時停止
        /// </summary>
        public virtual void PauseGame()
        {
            if (State == GameState.Playing)
            {
                SetGameState(GameState.Paused);
            }
        }

        /// <summary>
        /// ゲームを再開
        /// </summary>
        public virtual void ResumeGame()
        {
            if (State == GameState.Paused)
            {
                SetGameState(GameState.Playing);
            }
        }

        /// <summary>
        /// プレイヤーインデックスをPieceTypeに変換
        /// </summary>
        protected PieceType IndexToPieceType(int index)
        {
            return index switch
            {
                0 => PieceType.Player1,
                1 => PieceType.Player2,
                2 => PieceType.Player3,
                3 => PieceType.Player4,
                _ => PieceType.None
            };
        }

        /// <summary>
        /// プレイヤーインデックスから勝利状態を取得
        /// </summary>
        protected GameState GetWinStateForPlayer(int index)
        {
            return index switch
            {
                0 => GameState.Player1Won,
                1 => GameState.Player2Won,
                2 => GameState.Player3Won,
                3 => GameState.Player4Won,
                _ => GameState.Draw
            };
        }
    }
}
