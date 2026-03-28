#nullable enable

using System;
using System.Collections.ObjectModel;
using UniRx;
using UnityEngine;

namespace ShaderOp.UI.ViewModels
{
    /// <summary>
    /// オンラインマルチプレイヤーロビーのプレイヤー情報
    /// </summary>
    public class LobbyPlayerInfo
    {
        /// <summary>プレイヤー名</summary>
        public string Name { get; set; } = "Player";

        /// <summary>GameId（ゲームロジック用の整数ID）</summary>
        public int GameId { get; set; } = -1;

        /// <summary>PlayerId（Unity Multiplayer ServicesのGUID）</summary>
        public string PlayerId { get; set; } = "";

        /// <summary>準備完了状態</summary>
        public bool IsReady { get; set; } = false;

        /// <summary>ホストプレイヤーか</summary>
        public bool IsHost { get; set; } = false;
    }

    /// <summary>
    /// オンラインマルチプレイヤーロビー画面のViewModel
    /// </summary>
    /// <remarks>
    /// Phase 5 Week 2 Day 4-5実装。
    /// Unity Multiplayer Services v2のSession/Join Code APIを使用。
    ///
    /// 主要機能:
    /// - 6桁Join Code生成・表示
    /// - プレイヤー参加/離脱のリアルタイム反映
    /// - Ready状態管理
    /// - ホスト専用のゲーム開始ボタン
    /// </remarks>
    public class LobbyViewModel : IDisposable
    {
        private readonly CompositeDisposable _disposables = new();

        // ============================================
        // リアクティブプロパティ
        // ============================================

        /// <summary>6桁のJoin Code</summary>
        public ReactiveProperty<string> JoinCode { get; } = new("");

        /// <summary>プレイヤーリスト（ObservableCollectionでUI自動更新）</summary>
        public ReactiveCollection<LobbyPlayerInfo> PlayerList { get; } = new();

        /// <summary>ローカルプレイヤーの準備完了状態</summary>
        public ReactiveProperty<bool> LocalPlayerReady { get; } = new(false);

        /// <summary>ローカルプレイヤーがホストか</summary>
        public ReactiveProperty<bool> IsHost { get; } = new(false);

        /// <summary>ゲーム開始可能か（全員Ready & 最小2人 & ホスト）</summary>
        public ReactiveProperty<bool> CanStartGame { get; } = new(false);

        /// <summary>現在のプレイヤー数</summary>
        public ReactiveProperty<int> PlayerCount { get; } = new(0);

        /// <summary>最大プレイヤー数</summary>
        public ReactiveProperty<int> MaxPlayers { get; } = new(2);

        // ============================================
        // コマンド（イベント駆動）
        // ============================================

        /// <summary>ゲーム開始要求イベント（ホスト専用）</summary>
        public event Action? OnStartGameRequested;

        /// <summary>ロビー退出要求イベント</summary>
        public event Action? OnLeaveRoomRequested;

        /// <summary>Ready状態トグル要求イベント（新しい状態をboolで通知）</summary>
        public event Action<bool>? OnReadyToggleRequested;

        /// <summary>Join Codeコピー要求イベント</summary>
        public event Action<string>? OnCopyJoinCodeRequested;

        // ============================================
        // コンストラクタ
        // ============================================

        public LobbyViewModel()
        {
            // プレイヤー数の変更を監視してCanStartGameを更新
            PlayerList.ObserveAdd()
                .Subscribe(_ => UpdateCanStartGame())
                .AddTo(_disposables);

            PlayerList.ObserveRemove()
                .Subscribe(_ => UpdateCanStartGame())
                .AddTo(_disposables);

            // Ready状態の変更を監視
            LocalPlayerReady
                .Subscribe(_ => UpdateCanStartGame())
                .AddTo(_disposables);

            // ホスト状態の変更を監視
            IsHost
                .Subscribe(_ => UpdateCanStartGame())
                .AddTo(_disposables);

            Debug.Log("[LobbyViewModel] ViewModel initialized.");
        }

        // ============================================
        // パブリックメソッド（Viewから呼び出し）
        // ============================================

        /// <summary>
        /// Join Codeを設定
        /// </summary>
        /// <param name="code">6桁のJoin Code</param>
        public void SetJoinCode(string code)
        {
            JoinCode.Value = code;
            Debug.Log($"[LobbyViewModel] Join Code set: {code}");
        }

        /// <summary>
        /// プレイヤーを追加
        /// </summary>
        /// <param name="playerId">Unity Multiplayer ServicesのPlayerId</param>
        /// <param name="gameId">ゲームロジック用のGameId</param>
        /// <param name="isHost">ホストプレイヤーか</param>
        public void AddPlayer(string playerId, int gameId, bool isHost = false)
        {
            var playerInfo = new LobbyPlayerInfo
            {
                Name = $"Player {gameId}",
                PlayerId = playerId,
                GameId = gameId,
                IsReady = false,
                IsHost = isHost
            };

            PlayerList.Add(playerInfo);
            PlayerCount.Value = PlayerList.Count;

            Debug.Log($"[LobbyViewModel] Player added: GameId={gameId}, IsHost={isHost}");
        }

        /// <summary>
        /// プレイヤーを削除
        /// </summary>
        /// <param name="gameId">削除するプレイヤーのGameId</param>
        public void RemovePlayer(int gameId)
        {
            for (int i = PlayerList.Count - 1; i >= 0; i--)
            {
                if (PlayerList[i].GameId == gameId)
                {
                    PlayerList.RemoveAt(i);
                    PlayerCount.Value = PlayerList.Count;
                    Debug.Log($"[LobbyViewModel] Player removed: GameId={gameId}");
                    break;
                }
            }
        }

        /// <summary>
        /// プレイヤーのReady状態を更新
        /// </summary>
        /// <param name="gameId">プレイヤーのGameId</param>
        /// <param name="isReady">新しいReady状態</param>
        public void UpdatePlayerReady(int gameId, bool isReady)
        {
            foreach (var player in PlayerList)
            {
                if (player.GameId == gameId)
                {
                    player.IsReady = isReady;
                    UpdateCanStartGame();
                    Debug.Log($"[LobbyViewModel] Player {gameId} ready state: {isReady}");
                    break;
                }
            }
        }

        /// <summary>
        /// Ready状態をトグル
        /// </summary>
        public void ToggleReady()
        {
            bool newState = !LocalPlayerReady.Value;
            LocalPlayerReady.Value = newState;
            OnReadyToggleRequested?.Invoke(newState);
            Debug.Log($"[LobbyViewModel] Local player ready toggled: {newState}");
        }

        /// <summary>
        /// ゲーム開始を要求（ホスト専用）
        /// </summary>
        public void StartGame()
        {
            if (!CanStartGame.Value)
            {
                Debug.LogWarning("[LobbyViewModel] Cannot start game - conditions not met.");
                return;
            }

            Debug.Log("[LobbyViewModel] Start game requested.");
            OnStartGameRequested?.Invoke();
        }

        /// <summary>
        /// ロビーから退出
        /// </summary>
        public void LeaveRoom()
        {
            Debug.Log("[LobbyViewModel] Leave room requested.");
            OnLeaveRoomRequested?.Invoke();
        }

        /// <summary>
        /// Join Codeをクリップボードにコピー
        /// </summary>
        public void CopyJoinCode()
        {
            if (string.IsNullOrEmpty(JoinCode.Value))
            {
                Debug.LogWarning("[LobbyViewModel] Join Code is empty, cannot copy.");
                return;
            }

            OnCopyJoinCodeRequested?.Invoke(JoinCode.Value);
            Debug.Log($"[LobbyViewModel] Join Code copy requested: {JoinCode.Value}");
        }

        // ============================================
        // プライベートメソッド
        // ============================================

        /// <summary>
        /// ゲーム開始可能状態を更新
        /// </summary>
        /// <remarks>
        /// 条件:
        /// 1. ホストである
        /// 2. 最低2人のプレイヤーがいる
        /// 3. 全員がReady状態
        /// </remarks>
        private void UpdateCanStartGame()
        {
            // ホストでなければゲーム開始不可
            if (!IsHost.Value)
            {
                CanStartGame.Value = false;
                return;
            }

            // 最低2人必要
            if (PlayerList.Count < 2)
            {
                CanStartGame.Value = false;
                return;
            }

            // 全員がReady状態か確認
            bool allReady = true;
            foreach (var player in PlayerList)
            {
                if (!player.IsReady)
                {
                    allReady = false;
                    break;
                }
            }

            CanStartGame.Value = allReady;

            Debug.Log($"[LobbyViewModel] CanStartGame updated: {CanStartGame.Value} " +
                      $"(IsHost={IsHost.Value}, PlayerCount={PlayerList.Count}, AllReady={allReady})");
        }

        // ============================================
        // IDisposable
        // ============================================

        public void Dispose()
        {
            _disposables?.Dispose();
            Debug.Log("[LobbyViewModel] Disposed.");
        }
    }
}
