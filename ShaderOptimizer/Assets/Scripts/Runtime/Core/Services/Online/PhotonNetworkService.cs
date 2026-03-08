#nullable enable

using System;
using Cysharp.Threading.Tasks;
using ShaderOp.Core.Services.Online;
using UnityEngine;
// using Photon.Pun;
// using Photon.Realtime;

namespace ShaderOp.Core.Services.Online
{
    /// <summary>
    /// Photon PUN2ネットワークサービス実装
    /// </summary>
    /// <remarks>
    /// 現在はモック実装です。
    /// Photon PUN2インストール後、MonoBehaviourPunCallbacksを継承してコールバックを実装します。
    /// </remarks>
    public class PhotonNetworkService : MonoBehaviour, INetworkService
    // public class PhotonNetworkService : MonoBehaviourPunCallbacks, INetworkService
    {
        /// <summary>接続完了待ちタスク</summary>
        private UniTaskCompletionSource<bool>? _connectTcs;

        /// <summary>ルーム参加完了待ちタスク</summary>
        private UniTaskCompletionSource<bool>? _joinRoomTcs;

        // プロパティ実装（モック）
        public bool IsConnected => false; // PhotonNetwork.IsConnected;
        public bool IsInRoom => false; // PhotonNetwork.InRoom;
        public string PlayerName => "TestPlayer"; // PhotonNetwork.NickName;
        public string? LocalPlayerId => "TestPlayer"; // PhotonNetwork.LocalPlayer?.UserId;
        public string? CurrentRoomName => null; // PhotonNetwork.CurrentRoom?.Name;
        public int CurrentPlayerCount => 0; // PhotonNetwork.CurrentRoom?.PlayerCount ?? 0;
        public bool IsMasterClient => false; // PhotonNetwork.IsMasterClient;

        // イベント（INetworkService準拠）
        public event Action<bool>? OnConnectionStatusChanged;
        public event Action<string>? OnRoomJoined;
        public event Action? OnRoomLeft;
        public event Action<byte, object>? OnEventReceived;
        public event Action? OnConnectedToMaster;
        public event Action<string>? OnConnectionFailed;
        public event Action<string>? OnJoinedRoom;
        public event Action<string>? OnJoinRoomFailed;
        public event Action? OnLeftRoom;

        /// <summary>
        /// Photonサーバーに接続
        /// </summary>
        public async UniTask<bool> ConnectAsync()
        {
            Debug.Log("[PhotonNetworkService] Connecting to Photon...");

            // モック実装：即座に失敗を返す
            Debug.LogWarning("[PhotonNetworkService] Photon PUN2 not installed. This is a mock implementation.");
            OnConnectionFailed?.Invoke("Photon PUN2 not installed");
            await UniTask.Delay(100);
            return false;

            /* Photon PUN2インストール後の実装:
            _connectTcs = new UniTaskCompletionSource<bool>();
            PhotonNetwork.ConnectUsingSettings();
            return await _connectTcs.Task;
            */
        }

        /// <summary>
        /// Photonサーバーから切断
        /// </summary>
        public void Disconnect()
        {
            Debug.Log("[PhotonNetworkService] Disconnecting from Photon...");

            // モック実装
            OnConnectionStatusChanged?.Invoke(false);

            /* Photon PUN2インストール後の実装:
            PhotonNetwork.Disconnect();
            */
        }

        /// <summary>
        /// Photonサーバーから切断（非同期版、後方互換用）
        /// </summary>
        public async UniTask DisconnectAsync()
        {
            Disconnect();
            await UniTask.Delay(100);
        }

        /// <summary>
        /// ルームを作成
        /// </summary>
        public async UniTask<bool> CreateRoomAsync(string roomName, int maxPlayers = 2)
        {
            Debug.Log($"[PhotonNetworkService] Creating room: {roomName} (max {maxPlayers} players)");

            // モック実装
            Debug.LogWarning("[PhotonNetworkService] Photon PUN2 not installed. This is a mock implementation.");
            await UniTask.Delay(100);
            return false;
        }

        /// <summary>
        /// ルームに参加
        /// </summary>
        public async UniTask<bool> JoinRoomAsync(string roomName)
        {
            Debug.Log($"[PhotonNetworkService] Joining room: {roomName}");

            // モック実装
            Debug.LogWarning("[PhotonNetworkService] Photon PUN2 not installed. This is a mock implementation.");
            await UniTask.Delay(100);
            return false;
        }

        /// <summary>
        /// ルームを作成または参加（後方互換用）
        /// </summary>
        public async UniTask<bool> JoinOrCreateRoomAsync(string roomName, int maxPlayers = 4)
        {
            Debug.Log($"[PhotonNetworkService] Joining or creating room: {roomName} (max {maxPlayers} players)");

            // モック実装
            Debug.LogWarning("[PhotonNetworkService] Photon PUN2 not installed. This is a mock implementation.");
            await UniTask.Delay(100);
            return false;

            /* Photon PUN2インストール後の実装:
            if (!IsConnected)
            {
                Debug.LogError("[PhotonNetworkService] Not connected to Photon. Call ConnectAsync() first.");
                return false;
            }

            _joinRoomTcs = new UniTaskCompletionSource<bool>();
            
            RoomOptions roomOptions = new RoomOptions
            {
                MaxPlayers = (byte)maxPlayers,
                IsVisible = true,
                IsOpen = true
            };

            PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
            return await _joinRoomTcs.Task;
            */
        }

        /// <summary>
        /// ルームから退出
        /// </summary>
        public void LeaveRoom()
        {
            Debug.Log("[PhotonNetworkService] Leaving room...");

            // モック実装
            OnRoomLeft?.Invoke();

            /* Photon PUN2インストール後の実装:
            if (IsInRoom)
            {
                PhotonNetwork.LeaveRoom();
            }
            */
        }

        /// <summary>
        /// ルームから退出（非同期版、後方互換用）
        /// </summary>
        public async UniTask LeaveRoomAsync()
        {
            LeaveRoom();
            await UniTask.Delay(100);
        }

        /// <summary>
        /// カスタムイベント送信
        /// </summary>
        public void SendCustomEvent(byte eventCode, object data)
        {
            Debug.Log($"[PhotonNetworkService] Sending custom event {eventCode}");
            // モック実装
        }

        /// <summary>
        /// イベント送信（SendCustomEventのエイリアス）
        /// </summary>
        public void SendEvent(byte eventCode, object data)
        {
            SendCustomEvent(eventCode, data);
        }

        /// <summary>
        /// カスタムイベントハンドラ登録
        /// </summary>
        public void RegisterCustomEventHandler(byte eventCode, Action<object> callback)
        {
            Debug.Log($"[PhotonNetworkService] Registering handler for event {eventCode}");
            // モック実装
        }

        /// <summary>
        /// カスタムイベントハンドラ解除
        /// </summary>
        public void UnregisterCustomEventHandler(byte eventCode)
        {
            Debug.Log($"[PhotonNetworkService] Unregistering handler for event {eventCode}");
            // モック実装
        }

        /// <summary>
        /// ランダムマッチング
        /// </summary>
        public async UniTask<bool> JoinRandomRoomAsync()
        {
            Debug.Log("[PhotonNetworkService] Joining random room");

            // モック実装
            Debug.LogWarning("[PhotonNetworkService] Photon PUN2 not installed. This is a mock implementation.");
            await UniTask.Delay(100);
            return false;

            /* Photon PUN2インストール後の実装:
            if (!IsConnected)
            {
                Debug.LogError("[PhotonNetworkService] Not connected to Photon. Call ConnectAsync() first.");
                return false;
            }

            _joinRoomTcs = new UniTaskCompletionSource<bool>();
            PhotonNetwork.JoinRandomRoom();
            return await _joinRoomTcs.Task;
            */
        }

        // Photonコールバック実装（Photon PUN2インストール後に有効化）
        /*
        public override void OnConnectedToMaster()
        {
            Debug.Log("[PhotonNetworkService] Connected to Photon Master Server.");
            _connectTcs?.TrySetResult(true);
            OnConnectedToMaster?.Invoke();
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.LogWarning($"[PhotonNetworkService] Disconnected: {cause}");
            _connectTcs?.TrySetResult(false);
            OnConnectionFailed?.Invoke(cause.ToString());
        }

        public override void OnJoinedRoom()
        {
            Debug.Log($"[PhotonNetworkService] Joined room: {PhotonNetwork.CurrentRoom.Name}");
            _joinRoomTcs?.TrySetResult(true);
            OnJoinedRoom?.Invoke(PhotonNetwork.CurrentRoom.Name);
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"[PhotonNetworkService] Join room failed: {message} (Code: {returnCode})");
            _joinRoomTcs?.TrySetResult(false);
            OnJoinRoomFailed?.Invoke(message);
        }

        public override void OnLeftRoom()
        {
            Debug.Log("[PhotonNetworkService] Left room.");
            OnLeftRoom?.Invoke();
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"[PhotonNetworkService] Create room failed: {message} (Code: {returnCode})");
            _joinRoomTcs?.TrySetResult(false);
            OnJoinRoomFailed?.Invoke(message);
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log($"[PhotonNetworkService] No random room found. Creating new room...");
            // ランダムマッチング失敗時は新しいルームを作成
            string randomRoomName = "Room_" + UnityEngine.Random.Range(1000, 9999);
            RoomOptions roomOptions = new RoomOptions
            {
                MaxPlayers = 4,
                IsVisible = true,
                IsOpen = true
            };
            PhotonNetwork.CreateRoom(randomRoomName, roomOptions, TypedLobby.Default);
        }
        */
    }
}
