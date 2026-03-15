using Photon.Pun;
using Photon.Realtime;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System;

namespace ShaderOp.Core.Services
{
    /// <summary>
    /// Photon PUN 2を使用したネットワーク接続管理サービス
    /// MonoBehaviourPunCallbacksを継承してPhotonコールバックを受信
    /// </summary>
    /// <remarks>
    /// Phase 5 Week 1で実装。
    /// UniTaskを使用して非同期接続処理を提供し、UI blockingを防止。
    /// Photonのコールバックシステムと統合し、イベント駆動で状態変更を通知。
    /// </remarks>
    public class PhotonNetworkService : MonoBehaviourPunCallbacks, INetworkService
    {
        // INetworkService実装
        public bool IsConnected => PhotonNetwork.IsConnected;
        public bool IsInRoom => PhotonNetwork.InRoom;
        public string? CurrentRoomName => PhotonNetwork.CurrentRoom?.Name;
        public int PlayerCount => PhotonNetwork.CurrentRoom?.PlayerCount ?? 0;
        public int LocalPlayerId => PhotonNetwork.LocalPlayer?.ActorNumber ?? -1;
        public bool IsMasterClient => PhotonNetwork.IsMasterClient;

        // イベント
        public event Action<bool>? OnConnectedChanged;
        public event Action<string>? OnRoomJoined;
        public event Action? OnRoomLeft;
        public event Action<int>? OnPlayerJoined;
        public event Action<int>? OnPlayerLeft;

        // UniTask用CompletionSource（非同期処理の完了通知）
        private UniTaskCompletionSource<bool>? _connectTcs;
        private UniTaskCompletionSource<bool>? _createRoomTcs;
        private UniTaskCompletionSource<bool>? _joinRoomTcs;
        private UniTaskCompletionSource<bool>? _joinRandomTcs;

        /// <summary>
        /// Photonマスターサーバーに接続
        /// </summary>
        /// <returns>接続成功でtrue、失敗でfalse</returns>
        public async UniTask<bool> ConnectToServerAsync()
        {
            if (IsConnected)
            {
                Debug.Log("[PhotonNetworkService] 既に接続済み");
                return true;
            }

            _connectTcs = new UniTaskCompletionSource<bool>();
            PhotonNetwork.ConnectUsingSettings();
            Debug.Log("[PhotonNetworkService] Photonサーバーに接続中...");

            return await _connectTcs.Task;
        }

        /// <summary>
        /// 新規ルームを作成して参加
        /// </summary>
        /// <param name="roomName">ルーム名（ユニークである必要あり）</param>
        /// <param name="maxPlayers">最大プレイヤー数（デフォルト2）</param>
        /// <returns>作成成功でtrue、失敗でfalse</returns>
        public async UniTask<bool> CreateRoomAsync(string roomName, int maxPlayers = 2)
        {
            if (!IsConnected)
            {
                Debug.LogError("[PhotonNetworkService] サーバー未接続のためルーム作成不可");
                return false;
            }

            _createRoomTcs = new UniTaskCompletionSource<bool>();
            RoomOptions roomOptions = new RoomOptions
            {
                MaxPlayers = (byte)maxPlayers,
                IsVisible = true,
                IsOpen = true
            };

            PhotonNetwork.CreateRoom(roomName, roomOptions);
            Debug.Log($"[PhotonNetworkService] ルーム作成中: {roomName}");

            return await _createRoomTcs.Task;
        }

        /// <summary>
        /// 指定名のルームに参加
        /// </summary>
        /// <param name="roomName">参加するルーム名</param>
        /// <returns>参加成功でtrue、失敗でfalse</returns>
        public async UniTask<bool> JoinRoomAsync(string roomName)
        {
            if (!IsConnected)
            {
                Debug.LogError("[PhotonNetworkService] サーバー未接続のためルーム参加不可");
                return false;
            }

            _joinRoomTcs = new UniTaskCompletionSource<bool>();
            PhotonNetwork.JoinRoom(roomName);
            Debug.Log($"[PhotonNetworkService] ルーム参加中: {roomName}");

            return await _joinRoomTcs.Task;
        }

        /// <summary>
        /// ランダムなルームに参加（マッチメイキング）
        /// </summary>
        /// <returns>参加成功でtrue、失敗でfalse</returns>
        public async UniTask<bool> JoinRandomRoomAsync()
        {
            if (!IsConnected)
            {
                Debug.LogError("[PhotonNetworkService] サーバー未接続のためランダム参加不可");
                return false;
            }

            _joinRandomTcs = new UniTaskCompletionSource<bool>();
            PhotonNetwork.JoinRandomRoom();
            Debug.Log("[PhotonNetworkService] ランダムルーム参加中...");

            return await _joinRandomTcs.Task;
        }

        /// <summary>
        /// 現在のルームから退出
        /// </summary>
        public async UniTask LeaveRoomAsync()
        {
            if (!IsInRoom)
            {
                Debug.LogWarning("[PhotonNetworkService] ルーム未参加のため退出不要");
                return;
            }

            PhotonNetwork.LeaveRoom();
            Debug.Log("[PhotonNetworkService] ルーム退出中...");

            // 退出完了まで待機（OnLeftRoomコールバック発火まで）
            await UniTask.WaitUntil(() => !IsInRoom);
        }

        /// <summary>
        /// Photonサーバーから切断
        /// </summary>
        public async UniTask DisconnectAsync()
        {
            if (!IsConnected)
            {
                Debug.LogWarning("[PhotonNetworkService] 未接続のため切断不要");
                return;
            }

            PhotonNetwork.Disconnect();
            Debug.Log("[PhotonNetworkService] サーバー切断中...");

            // 切断完了まで待機
            await UniTask.WaitUntil(() => !IsConnected);
        }

        // ===== Photon Callbacks =====

        /// <summary>
        /// Photonマスターサーバー接続成功時コールバック
        /// </summary>
        public override void OnConnectedToMaster()
        {
            Debug.Log("[PhotonNetworkService] Photonマスターサーバー接続成功");
            _connectTcs?.TrySetResult(true);
            OnConnectedChanged?.Invoke(true);
        }

        /// <summary>
        /// Photonサーバー切断時コールバック
        /// </summary>
        /// <param name="cause">切断理由</param>
        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.LogWarning($"[PhotonNetworkService] Photonサーバー切断: {cause}");
            _connectTcs?.TrySetResult(false);
            OnConnectedChanged?.Invoke(false);
        }

        /// <summary>
        /// ルーム作成成功時コールバック
        /// </summary>
        public override void OnCreatedRoom()
        {
            Debug.Log($"[PhotonNetworkService] ルーム作成成功: {CurrentRoomName}");
        }

        /// <summary>
        /// ルーム参加成功時コールバック
        /// </summary>
        public override void OnJoinedRoom()
        {
            Debug.Log($"[PhotonNetworkService] ルーム参加成功: {CurrentRoomName} (Players: {PlayerCount})");
            _createRoomTcs?.TrySetResult(true);
            _joinRoomTcs?.TrySetResult(true);
            _joinRandomTcs?.TrySetResult(true);
            OnRoomJoined?.Invoke(CurrentRoomName ?? "Unknown");
        }

        /// <summary>
        /// ルーム退出時コールバック
        /// </summary>
        public override void OnLeftRoom()
        {
            Debug.Log("[PhotonNetworkService] ルーム退出完了");
            OnRoomLeft?.Invoke();
        }

        /// <summary>
        /// ルーム作成失敗時コールバック
        /// </summary>
        /// <param name="returnCode">エラーコード</param>
        /// <param name="message">エラーメッセージ</param>
        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"[PhotonNetworkService] ルーム作成失敗: {message} (Code: {returnCode})");
            _createRoomTcs?.TrySetResult(false);
        }

        /// <summary>
        /// ルーム参加失敗時コールバック
        /// </summary>
        /// <param name="returnCode">エラーコード</param>
        /// <param name="message">エラーメッセージ</param>
        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"[PhotonNetworkService] ルーム参加失敗: {message} (Code: {returnCode})");
            _joinRoomTcs?.TrySetResult(false);
        }

        /// <summary>
        /// ランダムルーム参加失敗時コールバック
        /// </summary>
        /// <param name="returnCode">エラーコード</param>
        /// <param name="message">エラーメッセージ</param>
        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.LogWarning($"[PhotonNetworkService] ランダムルーム参加失敗: {message} (Code: {returnCode})");
            _joinRandomTcs?.TrySetResult(false);
        }

        /// <summary>
        /// 他プレイヤーがルームに参加した時のコールバック
        /// </summary>
        /// <param name="newPlayer">参加プレイヤー</param>
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.Log($"[PhotonNetworkService] プレイヤー参加: {newPlayer.NickName} (ID: {newPlayer.ActorNumber})");
            OnPlayerJoined?.Invoke(newPlayer.ActorNumber);
        }

        /// <summary>
        /// 他プレイヤーがルームから退出した時のコールバック
        /// </summary>
        /// <param name="otherPlayer">退出プレイヤー</param>
        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.Log($"[PhotonNetworkService] プレイヤー退出: {otherPlayer.NickName} (ID: {otherPlayer.ActorNumber})");
            OnPlayerLeft?.Invoke(otherPlayer.ActorNumber);
        }
    }
}
