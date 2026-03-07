#nullable enable

using System;
using UnityEngine;

namespace ShaderOp.Online.Models
{
    /// <summary>
    /// マッチセッションデータ
    /// </summary>
    /// <remarks>
    /// オンライン対戦のマッチング情報を保持します。
    /// </remarks>
    [Serializable]
    public class MatchSession
    {
        /// <summary>マッチID</summary>
        [SerializeField] private string _matchId = string.Empty;
        public string MatchId
        {
            get => _matchId;
            set => _matchId = value;
        }

        /// <summary>ゲームタイプ</summary>
        [SerializeField] private string _gameType = string.Empty;
        public string GameType
        {
            get => _gameType;
            set => _gameType = value;
        }

        /// <summary>Photonルーム名</summary>
        [SerializeField] private string _roomName = string.Empty;
        public string RoomName
        {
            get => _roomName;
            set => _roomName = value;
        }

        /// <summary>プレイヤー1のID</summary>
        [SerializeField] private string _player1Id = string.Empty;
        public string Player1Id
        {
            get => _player1Id;
            set => _player1Id = value;
        }

        /// <summary>プレイヤー2のID</summary>
        [SerializeField] private string _player2Id = string.Empty;
        public string Player2Id
        {
            get => _player2Id;
            set => _player2Id = value;
        }

        /// <summary>プレイヤー1のスコア</summary>
        [SerializeField] private int _player1Score = 0;
        public int Player1Score
        {
            get => _player1Score;
            set => _player1Score = value;
        }

        /// <summary>プレイヤー2のスコア</summary>
        [SerializeField] private int _player2Score = 0;
        public int Player2Score
        {
            get => _player2Score;
            set => _player2Score = value;
        }

        /// <summary>勝者のID</summary>
        [SerializeField] private string _winnerId = string.Empty;
        public string WinnerId
        {
            get => _winnerId;
            set => _winnerId = value;
        }

        /// <summary>マッチ状態</summary>
        [SerializeField] private MatchStatus _status = MatchStatus.Waiting;
        public MatchStatus Status
        {
            get => _status;
            set => _status = value;
        }

        /// <summary>開始日時</summary>
        [SerializeField] private string _startedAt = string.Empty;
        public string StartedAt
        {
            get => _startedAt;
            set => _startedAt = value;
        }

        /// <summary>終了日時</summary>
        [SerializeField] private string _endedAt = string.Empty;
        public string EndedAt
        {
            get => _endedAt;
            set => _endedAt = value;
        }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public MatchSession() { }

        /// <summary>
        /// マッチセッション作成用コンストラクタ
        /// </summary>
        public MatchSession(string gameType, string roomName, string player1Id)
        {
            _matchId = Guid.NewGuid().ToString();
            _gameType = gameType;
            _roomName = roomName;
            _player1Id = player1Id;
            _status = MatchStatus.Waiting;
            _startedAt = DateTime.UtcNow.ToString("o");
        }

        /// <summary>
        /// マッチを開始
        /// </summary>
        public void StartMatch(string player2Id)
        {
            _player2Id = player2Id;
            _status = MatchStatus.InProgress;
        }

        /// <summary>
        /// マッチを終了
        /// </summary>
        public void EndMatch(string winnerId, int player1Score, int player2Score)
        {
            _winnerId = winnerId;
            _player1Score = player1Score;
            _player2Score = player2Score;
            _status = MatchStatus.Completed;
            _endedAt = DateTime.UtcNow.ToString("o");
        }

        /// <summary>
        /// プレイ時間を取得（秒）
        /// </summary>
        public int GetPlayTimeSeconds()
        {
            try
            {
                DateTime start = DateTime.Parse(_startedAt);
                DateTime end = string.IsNullOrEmpty(_endedAt)
                    ? DateTime.UtcNow
                    : DateTime.Parse(_endedAt);
                return (int)(end - start).TotalSeconds;
            }
            catch
            {
                return 0;
            }
        }
    }

    /// <summary>
    /// マッチ状態列挙型
    /// </summary>
    public enum MatchStatus
    {
        /// <summary>対戦相手待ち</summary>
        Waiting = 0,

        /// <summary>進行中</summary>
        InProgress = 1,

        /// <summary>完了</summary>
        Completed = 2,

        /// <summary>キャンセル</summary>
        Cancelled = 3
    }
}
