using UnityEngine;
using ShaderOp.Core.Services;
using Cysharp.Threading.Tasks;
using System;

namespace ShaderOp.Tests
{
    /// <summary>
    /// ネットワーク接続テスト用スクリプト
    /// Phase 5 Week 1でPhoton PUN統合後の接続確認に使用
    /// </summary>
    /// <remarks>
    /// 使用方法:
    /// 1. Startupシーン（またはテストシーン）に空のGameObjectを作成
    /// 2. このスクリプトをアタッチ
    /// 3. Play Modeで実行
    /// 4. Consoleでログ確認
    /// 
    /// 期待されるログ:
    /// - [NetworkTestRunner] 接続テスト開始
    /// - [PhotonNetworkService] Photonサーバーに接続中...
    /// - [PhotonNetworkService] Photonマスターサーバー接続成功
    /// - [NetworkTestRunner] ✓ Photon接続成功
    /// - [PhotonNetworkService] ルーム作成中: TestRoom_xxxxxxxx
    /// - [NetworkTestRunner] ✓ ルーム作成成功: TestRoom_xxxxxxxx
    /// - [NetworkTestRunner] 現在のプレイヤー数: 1
    /// - [NetworkTestRunner] ローカルプレイヤーID: 1
    /// - [NetworkTestRunner] ===== 全テスト成功 =====
    /// </remarks>
    public class NetworkTestRunner : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private float _initializationDelay = 3.0f;
        [SerializeField] private bool _autoRunOnStart = true;

        private INetworkService? _networkService;
        private IGameSyncService? _gameSyncService;

        private void Start()
        {
            if (_autoRunOnStart)
            {
                RunTestsAsync().Forget();
            }
        }

        /// <summary>
        /// 接続テストを実行
        /// </summary>
        public async UniTaskVoid RunTestsAsync()
        {
            try
            {
                // GameBootstrap初期化完了待ち
                Debug.Log($"[NetworkTestRunner] {_initializationDelay}秒待機中...");
                await UniTask.Delay(TimeSpan.FromSeconds(_initializationDelay));

                Debug.Log("[NetworkTestRunner] ===== 接続テスト開始 =====");

                // Test 1: サービス取得確認
                if (!await Test1_ServiceRegistrationAsync())
                {
                    Debug.LogError("[NetworkTestRunner] Test 1 失敗: サービス未登録");
                    return;
                }

                // Test 2: Photonサーバー接続確認
                if (!await Test2_ConnectToServerAsync())
                {
                    Debug.LogError("[NetworkTestRunner] Test 2 失敗: Photon接続失敗");
                    return;
                }

                // Test 3: ルーム作成確認
                if (!await Test3_CreateRoomAsync())
                {
                    Debug.LogError("[NetworkTestRunner] Test 3 失敗: ルーム作成失敗");
                    return;
                }

                // Test 4: IGameSyncService確認
                if (!Test4_GameSyncServiceCheck())
                {
                    Debug.LogError("[NetworkTestRunner] Test 4 失敗: IGameSyncService未登録");
                    return;
                }

                Debug.Log("[NetworkTestRunner] ===== 全テスト成功 ✅ =====");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NetworkTestRunner] テスト実行中にエラー: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Test 1: サービス登録確認
        /// </summary>
        private async UniTask<bool> Test1_ServiceRegistrationAsync()
        {
            Debug.Log("[NetworkTestRunner] Test 1: サービス登録確認");

            // INetworkService取得
            _networkService = ServiceLocator.Instance.Get<INetworkService>();
            if (_networkService == null)
            {
                Debug.LogError("[NetworkTestRunner] ✗ INetworkService未登録");
                return false;
            }

            Debug.Log("[NetworkTestRunner] ✓ INetworkService登録確認");

            // IGameSyncService取得
            _gameSyncService = ServiceLocator.Instance.Get<IGameSyncService>();
            if (_gameSyncService == null)
            {
                Debug.LogError("[NetworkTestRunner] ✗ IGameSyncService未登録");
                return false;
            }

            Debug.Log("[NetworkTestRunner] ✓ IGameSyncService登録確認");
            Debug.Log("[NetworkTestRunner] Test 1: ✅ 成功");
            return true;
        }

        /// <summary>
        /// Test 2: Photonサーバー接続確認
        /// </summary>
        private async UniTask<bool> Test2_ConnectToServerAsync()
        {
            Debug.Log("[NetworkTestRunner] Test 2: Photonサーバー接続確認");

            if (_networkService == null)
            {
                Debug.LogError("[NetworkTestRunner] INetworkServiceがnull");
                return false;
            }

            // Photonサーバー接続
            bool connected = await _networkService.ConnectToServerAsync();
            if (!connected)
            {
                Debug.LogError("[NetworkTestRunner] ✗ Photon接続失敗");
                return false;
            }

            Debug.Log("[NetworkTestRunner] ✓ Photon接続成功");
            Debug.Log($"[NetworkTestRunner] IsConnected: {_networkService.IsConnected}");
            Debug.Log("[NetworkTestRunner] Test 2: ✅ 成功");
            return true;
        }

        /// <summary>
        /// Test 3: ルーム作成確認
        /// </summary>
        private async UniTask<bool> Test3_CreateRoomAsync()
        {
            Debug.Log("[NetworkTestRunner] Test 3: ルーム作成確認");

            if (_networkService == null)
            {
                Debug.LogError("[NetworkTestRunner] INetworkServiceがnull");
                return false;
            }

            // ユニークなルーム名生成
            string roomName = "TestRoom_" + System.Guid.NewGuid().ToString().Substring(0, 8);
            Debug.Log($"[NetworkTestRunner] ルーム名: {roomName}");

            // ルーム作成
            bool roomCreated = await _networkService.CreateRoomAsync(roomName, maxPlayers: 2);
            if (!roomCreated)
            {
                Debug.LogError("[NetworkTestRunner] ✗ ルーム作成失敗");
                return false;
            }

            Debug.Log($"[NetworkTestRunner] ✓ ルーム作成成功: {roomName}");
            Debug.Log($"[NetworkTestRunner] IsInRoom: {_networkService.IsInRoom}");
            Debug.Log($"[NetworkTestRunner] RoomName: {_networkService.RoomName}");
            Debug.Log($"[NetworkTestRunner] PlayerCount: {_networkService.PlayerCount}");
            Debug.Log($"[NetworkTestRunner] LocalPlayerId: {_networkService.LocalPlayerId}");
            Debug.Log($"[NetworkTestRunner] IsMasterClient: {_networkService.IsMasterClient}");
            Debug.Log("[NetworkTestRunner] Test 3: ✅ 成功");
            return true;
        }

        /// <summary>
        /// Test 4: IGameSyncService確認
        /// </summary>
        private bool Test4_GameSyncServiceCheck()
        {
            Debug.Log("[NetworkTestRunner] Test 4: IGameSyncService確認");

            if (_gameSyncService == null)
            {
                Debug.LogError("[NetworkTestRunner] IGameSyncServiceがnull");
                return false;
            }

            Debug.Log($"[NetworkTestRunner] IsSyncEnabled: {_gameSyncService.IsSyncEnabled}");
            Debug.Log($"[NetworkTestRunner] IsMyTurn: {_gameSyncService.IsMyTurn}");
            Debug.Log($"[NetworkTestRunner] CurrentPlayerId: {_gameSyncService.CurrentPlayerId}");

            // 同期無効の確認（2人未満のため）
            if (_gameSyncService.IsSyncEnabled)
            {
                Debug.LogWarning("[NetworkTestRunner] ⚠ IsSyncEnabled=true（2人未満のため通常はfalse）");
            }
            else
            {
                Debug.Log("[NetworkTestRunner] ✓ IsSyncEnabled=false（正常: 2人未満）");
            }

            Debug.Log("[NetworkTestRunner] Test 4: ✅ 成功");
            return true;
        }

        /// <summary>
        /// 手動でテストを再実行
        /// </summary>
        [ContextMenu("Run Tests")]
        public void RunTestsManually()
        {
            RunTestsAsync().Forget();
        }

        /// <summary>
        /// ルーム退出テスト
        /// </summary>
        [ContextMenu("Test: Leave Room")]
        public async void TestLeaveRoom()
        {
            if (_networkService == null)
            {
                Debug.LogError("[NetworkTestRunner] INetworkServiceがnull");
                return;
            }

            Debug.Log("[NetworkTestRunner] ルーム退出テスト開始");
            await _networkService.LeaveRoomAsync();
            Debug.Log($"[NetworkTestRunner] IsInRoom: {_networkService.IsInRoom}");
            Debug.Log("[NetworkTestRunner] ルーム退出テスト完了");
        }

        /// <summary>
        /// サーバー切断テスト
        /// </summary>
        [ContextMenu("Test: Disconnect")]
        public async void TestDisconnect()
        {
            if (_networkService == null)
            {
                Debug.LogError("[NetworkTestRunner] INetworkServiceがnull");
                return;
            }

            Debug.Log("[NetworkTestRunner] サーバー切断テスト開始");
            await _networkService.DisconnectAsync();
            Debug.Log($"[NetworkTestRunner] IsConnected: {_networkService.IsConnected}");
            Debug.Log("[NetworkTestRunner] サーバー切断テスト完了");
        }
    }
}
