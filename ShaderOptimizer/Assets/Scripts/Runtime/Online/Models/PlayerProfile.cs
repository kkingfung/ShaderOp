#nullable enable

using System;
using UnityEngine;

namespace ShaderOp.Online.Models
{
    /// <summary>
    /// プレイヤープロフィールデータ
    /// </summary>
    /// <remarks>
    /// ユーザーの基本情報とゲーム統計を保持します。
    /// バックエンドAPIとの同期用データモデルです。
    /// </remarks>
    [Serializable]
    public class PlayerProfile
    {
        /// <summary>プレイヤーID（UUID）</summary>
        [SerializeField] private string _playerId = string.Empty;
        public string PlayerId
        {
            get => _playerId;
            set => _playerId = value;
        }

        /// <summary>表示名</summary>
        [SerializeField] private string _displayName = string.Empty;
        public string DisplayName
        {
            get => _displayName;
            set => _displayName = value;
        }

        /// <summary>プレイヤーレベル</summary>
        [SerializeField] private int _level = 1;
        public int Level
        {
            get => _level;
            set => _level = value;
        }

        /// <summary>経験値</summary>
        [SerializeField] private int _experience = 0;
        public int Experience
        {
            get => _experience;
            set => _experience = value;
        }

        /// <summary>所持コイン</summary>
        [SerializeField] private int _coins = 0;
        public int Coins
        {
            get => _coins;
            set => _coins = value;
        }

        /// <summary>VIPメンバーか</summary>
        [SerializeField] private bool _isVIP = false;
        public bool IsVIP
        {
            get => _isVIP;
            set => _isVIP = value;
        }

        /// <summary>VIP有効期限</summary>
        [SerializeField] private string _vipExpiresAt = string.Empty;
        public string VipExpiresAt
        {
            get => _vipExpiresAt;
            set => _vipExpiresAt = value;
        }

        /// <summary>総プレイ時間（秒）</summary>
        [SerializeField] private int _totalPlayTimeSeconds = 0;
        public int TotalPlayTimeSeconds
        {
            get => _totalPlayTimeSeconds;
            set => _totalPlayTimeSeconds = value;
        }

        /// <summary>総勝利数</summary>
        [SerializeField] private int _totalWins = 0;
        public int TotalWins
        {
            get => _totalWins;
            set => _totalWins = value;
        }

        /// <summary>総敗北数</summary>
        [SerializeField] private int _totalLosses = 0;
        public int TotalLosses
        {
            get => _totalLosses;
            set => _totalLosses = value;
        }

        /// <summary>作成日時</summary>
        [SerializeField] private string _createdAt = string.Empty;
        public string CreatedAt
        {
            get => _createdAt;
            set => _createdAt = value;
        }

        /// <summary>最終ログイン日時</summary>
        [SerializeField] private string _lastLoginAt = string.Empty;
        public string LastLoginAt
        {
            get => _lastLoginAt;
            set => _lastLoginAt = value;
        }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public PlayerProfile() { }

        /// <summary>
        /// 新規プレイヤー作成用コンストラクタ
        /// </summary>
        public PlayerProfile(string playerId, string displayName)
        {
            _playerId = playerId;
            _displayName = displayName;
            _level = 1;
            _experience = 0;
            _coins = 0;
            _isVIP = false;
            _totalPlayTimeSeconds = 0;
            _totalWins = 0;
            _totalLosses = 0;
            _createdAt = DateTime.UtcNow.ToString("o");
            _lastLoginAt = DateTime.UtcNow.ToString("o");
        }

        /// <summary>
        /// 勝率を計算
        /// </summary>
        public float GetWinRate()
        {
            int totalGames = _totalWins + _totalLosses;
            if (totalGames == 0) return 0f;
            return (float)_totalWins / totalGames;
        }

        /// <summary>
        /// プレイ時間を時間単位で取得
        /// </summary>
        public float GetPlayTimeHours()
        {
            return _totalPlayTimeSeconds / 3600f;
        }
    }
}
