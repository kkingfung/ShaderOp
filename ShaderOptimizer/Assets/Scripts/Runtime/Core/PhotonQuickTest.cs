#nullable enable

using UnityEngine;
using Cysharp.Threading.Tasks;
using ShaderOp.Core.Services;
using ShaderOp.Online.Services;

namespace ShaderOp.Core
{
    /// <summary>
    /// Photon接続テストスクリプト
    /// </summary>
    /// <remarks>
    /// ゲーム起動時に自動的にPhotonサーバーに接続し、テストルームを作成します。
    /// デバッグ用のため、本番ビルドでは無効化してください。
    /// </remarks>
    public class PhotonQuickTest : MonoBehaviour
    {
        [Header("Auto Connect Settings")]
        [Tooltip("起動時に自動接続")]
        [SerializeField] private bool _autoConnectOnStart = true;

        [Tooltip("接続後に自動でルーム作成")]
        [SerializeField] private bool _autoJoinRoomAfterConnect = true;

        [Tooltip("テストルーム名")]
        [SerializeField] private string _testRoomName = "TestRoom_001";

        [Tooltip("最大プレイヤー数")]
        [SerializeField] private int _maxPlayers = 4;

        /// <summary>ネットワークサービス</summary>
        private INetworkService? _networkService;

        /// <summary>接続状態テキスト</summary>
        private string _statusText = "Initializing...";

        private async void Start()
        {
            // ServiceLocatorからネットワークサービスを取得
            _networkService = ServiceLocator.Instance.Get<INetworkService>();

            if (_networkService == null)
            {
                Debug.LogError("[PhotonQuickTest] INetworkService not found! Make sure GameBootstrap is in the scene.");
                _statusText = "ERROR: NetworkService not found";
                return;
            }

            // イベント購読
            _networkService.OnConnectedToMaster += OnConnectedToMaster;
            _networkService.OnConnectionFailed += OnConnectionFailed;
            _networkService.OnJoinedRoom += OnJoinedRoom;
            _networkService.OnJoinRoomFailed += OnJoinRoomFailed;
            _networkService.OnLeftRoom += OnLeftRoom;

            if (_autoConnectOnStart)
            {
                await ConnectAndJoinAsync();
            }
            else
            {
                _statusText = "Waiting for manual connection...";
            }
        }

        /// <summary>
        /// 接続してルーム参加
        /// </summary>
        private async UniTask ConnectAndJoinAsync()
        {
            if (_networkService == null) return;

            _statusText = "Connecting to Photon...";

            // 1. Photonに接続
            bool connected = await _networkService.ConnectAsync();

            if (!connected)
            {
                _statusText = "Connection failed. (Photon PUN2 not installed)";
                return;
            }

            _statusText = "Connected! Waiting for room join...";

            // 2. ルーム作成または参加
            if (_autoJoinRoomAfterConnect)
            {
                await UniTask.Delay(500); // 少し待機

                _statusText = $"Joining room: {_testRoomName}...";
                bool joined = await _networkService.JoinOrCreateRoomAsync(_testRoomName, _maxPlayers);

                if (!joined)
                {
                    _statusText = "Failed to join room.";
                    return;
                }
            }
        }

        // イベントハンドラ
        private void OnConnectedToMaster()
        {
            Debug.Log("[PhotonQuickTest] Connected to Photon Master Server!");
            _statusText = "Connected to Master Server";
        }

        private void OnConnectionFailed(string reason)
        {
            Debug.LogError($"[PhotonQuickTest] Connection failed: {reason}");
            _statusText = $"Connection failed: {reason}";
        }

        private void OnJoinedRoom(string roomName)
        {
            Debug.Log($"[PhotonQuickTest] Joined room: {roomName}");

            if (_networkService != null)
            {
                _statusText = $"In Room: {roomName}\n" +
                             $"Players: {_networkService.CurrentPlayerCount}\n" +
                             $"Master: {_networkService.IsMasterClient}";
            }
        }

        private void OnJoinRoomFailed(string reason)
        {
            Debug.LogError($"[PhotonQuickTest] Join room failed: {reason}");
            _statusText = $"Join room failed: {reason}";
        }

        private void OnLeftRoom()
        {
            Debug.Log("[PhotonQuickTest] Left room.");
            _statusText = "Left room. Disconnected.";
        }

        /// <summary>
        /// デバッグUI表示
        /// </summary>
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 400, 500));
            GUILayout.BeginVertical("box");

            GUILayout.Label("=== Photon Quick Test ===", GUI.skin.GetStyle("boldLabel"));
            GUILayout.Space(10);

            GUILayout.Label($"Status: {_statusText}");
            GUILayout.Space(10);

            if (_networkService != null)
            {
                GUILayout.Label($"Connected: {_networkService.IsConnected}");
                GUILayout.Label($"In Room: {_networkService.IsInRoom}");
                GUILayout.Label($"Room Name: {_networkService.CurrentRoomName ?? "N/A"}");
                GUILayout.Label($"Players: {_networkService.CurrentPlayerCount}");
                GUILayout.Label($"Is Master: {_networkService.IsMasterClient}");
            }

            GUILayout.Space(20);

            // ボタン
            GUI.enabled = _networkService != null && !_networkService.IsConnected;
            if (GUILayout.Button("Connect to Photon"))
            {
                ConnectAndJoinAsync().Forget();
            }
            GUI.enabled = true;

            GUI.enabled = _networkService != null && _networkService.IsConnected && !_networkService.IsInRoom;
            if (GUILayout.Button($"Join Room: {_testRoomName}"))
            {
                _networkService?.JoinOrCreateRoomAsync(_testRoomName, _maxPlayers).Forget();
            }
            GUI.enabled = true;

            GUI.enabled = _networkService != null && _networkService.IsInRoom;
            if (GUILayout.Button("Leave Room"))
            {
                _networkService?.LeaveRoomAsync().Forget();
            }
            GUI.enabled = true;

            GUI.enabled = _networkService != null && _networkService.IsConnected;
            if (GUILayout.Button("Disconnect"))
            {
                _networkService?.DisconnectAsync().Forget();
                _statusText = "Disconnected manually.";
            }
            GUI.enabled = true;

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void OnDestroy()
        {
            if (_networkService != null)
            {
                _networkService.OnConnectedToMaster -= OnConnectedToMaster;
                _networkService.OnConnectionFailed -= OnConnectionFailed;
                _networkService.OnJoinedRoom -= OnJoinedRoom;
                _networkService.OnJoinRoomFailed -= OnJoinRoomFailed;
                _networkService.OnLeftRoom -= OnLeftRoom;
            }
        }
    }
}
