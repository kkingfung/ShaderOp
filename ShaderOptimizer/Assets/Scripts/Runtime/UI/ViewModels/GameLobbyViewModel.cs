#nullable enable

using System;
using System.Collections.Generic;
using UniRx;

namespace ShaderOp.UI.ViewModels
{
    /// <summary>
    /// プレイヤー情報
    /// </summary>
    public class PlayerInfo
    {
        public string Name { get; set; } = "";
        public int Level { get; set; }
        public bool IsReady { get; set; }
        public bool IsOnline { get; set; } = true;
    }

    /// <summary>
    /// ゲームロビー画面のViewModel
    /// </summary>
    public class GameLobbyViewModel : IDisposable
    {
        private readonly CompositeDisposable _disposables = new();

        /// <summary>ゲーム名</summary>
        public ReactiveProperty<string> GameName { get; } = new("Game Lobby");

        /// <summary>プレイヤーリスト</summary>
        public ReactiveCollection<PlayerInfo> Players { get; } = new();

        /// <summary>最大プレイヤー数</summary>
        public ReactiveProperty<int> MaxPlayers { get; } = new(2);

        /// <summary>ローカルプレイヤーの準備状態</summary>
        public ReactiveProperty<bool> IsLocalPlayerReady { get; } = new(false);

        /// <summary>ゲーム開始可能</summary>
        public ReactiveProperty<bool> CanStartGame { get; } = new(false);

        /// <summary>チャット表示状態</summary>
        public ReactiveProperty<bool> IsChatVisible { get; } = new(false);

        /// <summary>準備完了ボタンクリックイベント</summary>
        public event Action<bool>? OnReadyToggled;

        /// <summary>ゲーム開始イベント</summary>
        public event Action? OnGameStart;

        /// <summary>ロビー退出イベント</summary>
        public event Action? OnLeaveLobby;

        /// <summary>チャット切り替えイベント</summary>
        public event Action<bool>? OnChatToggled;

        public GameLobbyViewModel()
        {
            // 準備状態の変更を監視してゲーム開始可能かチェック
            IsLocalPlayerReady.Subscribe(_ => UpdateCanStartGame()).AddTo(_disposables);
            Players.ObserveCountChanged().Subscribe(_ => UpdateCanStartGame()).AddTo(_disposables);

            // テストデータ
            InitializeTestData();
        }

        /// <summary>
        /// テストデータを初期化
        /// </summary>
        private void InitializeTestData()
        {
            Players.Add(new PlayerInfo
            {
                Name = "You",
                Level = 42,
                IsReady = false,
                IsOnline = true
            });

            Players.Add(new PlayerInfo
            {
                Name = "Opponent",
                Level = 38,
                IsReady = false,
                IsOnline = true
            });
        }

        /// <summary>
        /// ゲーム開始可能状態を更新
        /// </summary>
        private void UpdateCanStartGame()
        {
            // すべてのプレイヤーが準備完了で、最小人数以上いる場合
            bool allReady = true;
            foreach (var player in Players)
            {
                if (!player.IsReady)
                {
                    allReady = false;
                    break;
                }
            }

            CanStartGame.Value = allReady && Players.Count >= 2 && IsLocalPlayerReady.Value;
        }

        /// <summary>
        /// 準備完了ボタン処理
        /// </summary>
        public void HandleReadyToggled()
        {
            IsLocalPlayerReady.Value = !IsLocalPlayerReady.Value;

            // ローカルプレイヤーの準備状態を更新
            if (Players.Count > 0)
            {
                Players[0].IsReady = IsLocalPlayerReady.Value;
            }

            UnityEngine.Debug.Log($"[GameLobbyViewModel] Ready state: {IsLocalPlayerReady.Value}");
            OnReadyToggled?.Invoke(IsLocalPlayerReady.Value);
        }

        /// <summary>
        /// ゲーム開始処理
        /// </summary>
        public void HandleGameStart()
        {
            if (!CanStartGame.Value)
            {
                UnityEngine.Debug.LogWarning("[GameLobbyViewModel] Cannot start game - not all players ready");
                return;
            }

            UnityEngine.Debug.Log("[GameLobbyViewModel] Starting game...");
            OnGameStart?.Invoke();
        }

        /// <summary>
        /// ロビー退出処理
        /// </summary>
        public void HandleLeaveLobby()
        {
            UnityEngine.Debug.Log("[GameLobbyViewModel] Leaving lobby...");
            OnLeaveLobby?.Invoke();
        }

        /// <summary>
        /// チャット切り替え処理
        /// </summary>
        public void HandleChatToggle()
        {
            IsChatVisible.Value = !IsChatVisible.Value;
            UnityEngine.Debug.Log($"[GameLobbyViewModel] Chat visible: {IsChatVisible.Value}");
            OnChatToggled?.Invoke(IsChatVisible.Value);
        }

        /// <summary>
        /// プレイヤーを追加
        /// </summary>
        public void AddPlayer(string name, int level)
        {
            Players.Add(new PlayerInfo
            {
                Name = name,
                Level = level,
                IsReady = false,
                IsOnline = true
            });
        }

        /// <summary>
        /// プレイヤーを削除
        /// </summary>
        public void RemovePlayer(string name)
        {
            for (int i = Players.Count - 1; i >= 0; i--)
            {
                if (Players[i].Name == name)
                {
                    Players.RemoveAt(i);
                    break;
                }
            }
        }

        public void Dispose()
        {
            _disposables?.Dispose();
        }
    }
}
