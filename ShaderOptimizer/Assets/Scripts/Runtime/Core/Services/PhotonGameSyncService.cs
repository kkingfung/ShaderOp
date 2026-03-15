using Photon.Pun;
using Photon.Realtime;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System;
using System.Linq;
using ShaderOp.Minigames.HexGrid;

namespace ShaderOp.Core.Services
{
    /// <summary>
    /// Photon RPCを使用したゲーム状態同期サービス
    /// ターンベースゲームの移動・状態を同期
    /// </summary>
    /// <remarks>
    /// Phase 5 Week 1で実装。
    /// Photon RPC ([PunRPC])を使用して、プレイヤー間で手番・移動・ゲーム状態を同期。
    /// Phase 4で実装された最適化パターン（Direction-Based、Attack Map等）と
    /// 完全互換性を保ち、HexCoordinateを直接使用して60fps維持したまま
    /// オンラインマルチプレイを実現。
    /// </remarks>
    public class PhotonGameSyncService : MonoBehaviourPunCallbacks, IGameSyncService
    {
        // IGameSyncService実装
        public bool IsSyncEnabled => PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.PlayerCount >= 2;
        public bool IsMyTurn => _currentPlayerId == PhotonNetwork.LocalPlayer?.ActorNumber;
        public int CurrentPlayerId => _currentPlayerId;
        public string GameType { get; set; } = "";

        // イベント
        public event Action<HexCoordinate, HexCoordinate>? OnMoveReceived;
        public event Action? OnGameStarted;
        public event Action<int>? OnGameEnded;
        public event Action<int>? OnTurnChanged;
        public event Action? OnResetRequested;

        // 内部状態
        private int _currentPlayerId = -1;
        private PhotonView? _photonView;

        /// <summary>
        /// 初期化時にPhotonViewコンポーネントを取得
        /// </summary>
        private void Awake()
        {
            _photonView = GetComponent<PhotonView>();
            if (_photonView == null)
            {
                _photonView = gameObject.AddComponent<PhotonView>();
                Debug.Log("[PhotonGameSyncService] PhotonViewコンポーネントを追加");
            }
        }

        /// <summary>
        /// 手番移動を送信（自分のターンのみ）
        /// </summary>
        /// <param name="from">移動元座標</param>
        /// <param name="to">移動先座標</param>
        /// <returns>送信成功でtrue</returns>
        public async UniTask<bool> SendMoveAsync(HexCoordinate from, HexCoordinate to)
        {
            if (!IsSyncEnabled)
            {
                Debug.LogWarning("[PhotonGameSyncService] 同期無効のため移動送信スキップ");
                return false;
            }

            if (!IsMyTurn)
            {
                Debug.LogWarning("[PhotonGameSyncService] 自分のターンではないため移動不可");
                return false;
            }

            // RPC送信（他プレイヤーに通知）
            _photonView?.RPC(nameof(RPC_ReceiveMove), RpcTarget.Others, from.Q, from.R, to.Q, to.R);
            Debug.Log($"[PhotonGameSyncService] 移動送信: {from} → {to}");

            return true;
        }

        /// <summary>
        /// ゲーム開始を同期（全プレイヤーに通知）
        /// </summary>
        public async UniTask SyncGameStartAsync()
        {
            if (!IsSyncEnabled)
            {
                Debug.LogWarning("[PhotonGameSyncService] 同期無効のためゲーム開始送信スキップ");
                return;
            }

            // ルームマスターが最初のプレイヤーを決定
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogWarning("[PhotonGameSyncService] ルームマスターではないためゲーム開始不可");
                return;
            }

            // 最初のプレイヤーをルーム内の最小Actor IDに設定
            int firstPlayerId = PhotonNetwork.CurrentRoom.Players.Values.OrderBy(p => p.ActorNumber).First().ActorNumber;
            _photonView?.RPC(nameof(RPC_GameStart), RpcTarget.All, firstPlayerId);
            Debug.Log($"[PhotonGameSyncService] ゲーム開始送信: 最初のターン Player {firstPlayerId}");
        }

        /// <summary>
        /// ゲーム終了を同期（勝者IDを通知）
        /// </summary>
        /// <param name="winnerId">勝者のActor ID（引き分けは-1）</param>
        public async UniTask SyncGameEndAsync(int winnerId)
        {
            if (!IsSyncEnabled)
            {
                Debug.LogWarning("[PhotonGameSyncService] 同期無効のためゲーム終了送信スキップ");
                return;
            }

            _photonView?.RPC(nameof(RPC_GameEnd), RpcTarget.All, winnerId);
            Debug.Log($"[PhotonGameSyncService] ゲーム終了送信: 勝者 Player {winnerId}");
        }

        /// <summary>
        /// ターンを次のプレイヤーに渡す
        /// </summary>
        public async UniTask PassTurnAsync()
        {
            if (!IsSyncEnabled)
            {
                Debug.LogWarning("[PhotonGameSyncService] 同期無効のためターン変更送信スキップ");
                return;
            }

            // 次のプレイヤーにターンを渡す
            var players = PhotonNetwork.CurrentRoom.Players.Values.OrderBy(p => p.ActorNumber).ToList();
            int currentIndex = players.FindIndex(p => p.ActorNumber == _currentPlayerId);
            int nextIndex = (currentIndex + 1) % players.Count;
            int nextPlayerId = players[nextIndex].ActorNumber;

            _photonView?.RPC(nameof(RPC_TurnChange), RpcTarget.All, nextPlayerId);
            Debug.Log($"[PhotonGameSyncService] ターン変更送信: Player {nextPlayerId}");
        }

        /// <summary>
        /// ゲーム状態をリセット（再戦準備）
        /// </summary>
        public async UniTask ResetGameStateAsync()
        {
            if (!IsSyncEnabled)
            {
                Debug.LogWarning("[PhotonGameSyncService] 同期無効のためリセット送信スキップ");
                return;
            }

            _photonView?.RPC(nameof(RPC_ResetGame), RpcTarget.All);
            Debug.Log("[PhotonGameSyncService] ゲームリセット送信");
        }

        // ===== Photon RPC Methods =====

        /// <summary>
        /// 移動受信RPC（他プレイヤーからの移動を受信）
        /// </summary>
        /// <param name="fromQ">移動元Q座標</param>
        /// <param name="fromR">移動元R座標</param>
        /// <param name="toQ">移動先Q座標</param>
        /// <param name="toR">移動先R座標</param>
        [PunRPC]
        private void RPC_ReceiveMove(int fromQ, int fromR, int toQ, int toR)
        {
            HexCoordinate from = new HexCoordinate(fromQ, fromR);
            HexCoordinate to = new HexCoordinate(toQ, toR);
            Debug.Log($"[PhotonGameSyncService] 移動受信: {from} → {to}");
            OnMoveReceived?.Invoke(from, to);
        }

        /// <summary>
        /// ゲーム開始RPC（全プレイヤーに通知）
        /// </summary>
        /// <param name="firstPlayerId">最初のターンプレイヤーID</param>
        [PunRPC]
        private void RPC_GameStart(int firstPlayerId)
        {
            _currentPlayerId = firstPlayerId;
            Debug.Log($"[PhotonGameSyncService] ゲーム開始受信: 最初のターン Player {firstPlayerId}");
            OnGameStarted?.Invoke();
            OnTurnChanged?.Invoke(firstPlayerId);
        }

        /// <summary>
        /// ゲーム終了RPC（全プレイヤーに通知）
        /// </summary>
        /// <param name="winnerId">勝者ID</param>
        [PunRPC]
        private void RPC_GameEnd(int winnerId)
        {
            Debug.Log($"[PhotonGameSyncService] ゲーム終了受信: 勝者 Player {winnerId}");
            OnGameEnded?.Invoke(winnerId);
        }

        /// <summary>
        /// ターン変更RPC（全プレイヤーに通知）
        /// </summary>
        /// <param name="newPlayerId">新しいターンプレイヤーID</param>
        [PunRPC]
        private void RPC_TurnChange(int newPlayerId)
        {
            _currentPlayerId = newPlayerId;
            Debug.Log($"[PhotonGameSyncService] ターン変更受信: Player {newPlayerId}");
            OnTurnChanged?.Invoke(newPlayerId);
        }

        /// <summary>
        /// ゲームリセットRPC（全プレイヤーに通知）
        /// </summary>
        [PunRPC]
        private void RPC_ResetGame()
        {
            Debug.Log("[PhotonGameSyncService] ゲームリセット受信");
            OnResetRequested?.Invoke();
        }

        // ===== Photon Callbacks =====

        /// <summary>
        /// ルーム参加時コールバック（自動でゲーム開始準備）
        /// </summary>
        public override void OnJoinedRoom()
        {
            Debug.Log($"[PhotonGameSyncService] ルーム参加: {PhotonNetwork.CurrentRoom.Name}");

            // ルームマスターの場合、2人揃ったら自動でゲーム開始
            if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount >= 2)
            {
                SyncGameStartAsync().Forget();
            }
        }

        /// <summary>
        /// 他プレイヤー参加時コールバック（2人揃ったらゲーム開始）
        /// </summary>
        /// <param name="newPlayer">参加プレイヤー</param>
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.Log($"[PhotonGameSyncService] プレイヤー参加: {newPlayer.NickName} (ID: {newPlayer.ActorNumber})");

            // ルームマスターが2人揃ったらゲーム開始
            if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount >= 2)
            {
                SyncGameStartAsync().Forget();
            }
        }

        /// <summary>
        /// 他プレイヤー退出時コールバック（ゲーム中断）
        /// </summary>
        /// <param name="otherPlayer">退出プレイヤー</param>
        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.LogWarning($"[PhotonGameSyncService] プレイヤー退出: {otherPlayer.NickName} (ID: {otherPlayer.ActorNumber})");

            // 対戦相手が退出したらゲーム終了（不戦勝）
            if (IsSyncEnabled)
            {
                int localPlayerId = PhotonNetwork.LocalPlayer?.ActorNumber ?? -1;
                SyncGameEndAsync(localPlayerId).Forget();
            }
        }
    }
}
