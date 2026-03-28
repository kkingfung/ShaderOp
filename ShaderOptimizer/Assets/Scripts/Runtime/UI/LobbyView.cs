#nullable enable

using UnityEngine;
using UnityEngine.UIElements;
using ShaderOp.UI.ViewModels;
using ShaderOp.Core.Services;
using ShaderOp.Runtime.Core.Services.Online;
using UniRx;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Specialized;

namespace ShaderOp.Runtime.UI
{
    /// <summary>
    /// オンラインマルチプレイヤーロビー画面のView
    /// </summary>
    /// <remarks>
    /// Phase 5 Week 2 Day 4-5実装。
    /// LobbyViewModel と LobbyView.uxml を使用。
    ///
    /// 主要機能:
    /// - Join Code表示・コピー
    /// - プレイヤーリスト動的更新
    /// - Ready状態管理
    /// - ホスト専用のゲーム開始ボタン
    ///
    /// サービス依存:
    /// - INetworkService: ルーム管理、プレイヤー参加/離脱イベント
    /// - IGameSyncService: ゲーム開始同期
    /// - IPlayerIdService: PlayerId↔GameId変換
    /// </remarks>
    [RequireComponent(typeof(UIDocument))]
    public class LobbyView : MonoBehaviour
    {
        // ============================================
        // フィールド
        // ============================================

        private UIDocument? _uiDocument;
        private VisualElement? _root;
        private LobbyViewModel? _viewModel;
        private readonly CompositeDisposable _disposables = new();

        // サービス
        private INetworkService? _networkService;
        private IGameSyncService? _gameSyncService;
        private IPlayerIdService? _playerIdService;

        // UI要素 - Header
        private Label? _titleLabel;
        private Button? _backButton;

        // UI要素 - Join Code
        private Label? _joinCodeLabel;
        private Button? _copyJoinCodeButton;

        // UI要素 - Player List
        private VisualElement? _playerListContainer;
        private ScrollView? _playerListScrollView;

        // UI要素 - Controls
        private Button? _readyButton;
        private Button? _startGameButton;
        private Label? _statusLabel;

        // ============================================
        // Unity ライフサイクル
        // ============================================

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();

            // ServiceLocator存在チェック
            if (ServiceLocator.Instance == null)
            {
                Debug.LogError("[LobbyView] ServiceLocator.Instance is null! Make sure GameBootstrap has run.");
                return;
            }

            // サービス取得
            _networkService = ServiceLocator.Instance.Get<INetworkService>();
            _gameSyncService = ServiceLocator.Instance.Get<IGameSyncService>();
            _playerIdService = ServiceLocator.Instance.Get<IPlayerIdService>();

            if (_networkService == null)
            {
                Debug.LogError("[LobbyView] INetworkService not found in ServiceLocator!");
                return;
            }

            if (_playerIdService == null)
            {
                Debug.LogError("[LobbyView] IPlayerIdService not found in ServiceLocator!");
                return;
            }

            // ViewModel作成
            _viewModel = new LobbyViewModel();

            Debug.Log("[LobbyView] Initialized.");
        }

        private async void OnEnable()
        {
            if (_uiDocument == null || _uiDocument.rootVisualElement == null)
            {
                Debug.LogError("[LobbyView] UIDocument or rootVisualElement is null!");
                return;
            }

            _root = _uiDocument.rootVisualElement;

            // UI要素を取得
            QueryUIElements();

            // イベントハンドラ登録
            RegisterEventHandlers();

            // ViewModelバインド
            BindViewModel();

            // ルームを作成してJoin Codeを取得
            await CreateRoomWithJoinCode();

            Debug.Log("[LobbyView] OnEnable complete.");
        }

        private void OnDisable()
        {
            UnregisterEventHandlers();
            UnbindViewModel();

            // ルームから退出
            LeaveRoom().Forget();
        }

        private void OnDestroy()
        {
            _viewModel?.Dispose();
            _disposables.Dispose();

            Debug.Log("[LobbyView] Destroyed.");
        }

        // ============================================
        // UI要素取得
        // ============================================

        private void QueryUIElements()
        {
            if (_root == null) return;

            // Header
            _titleLabel = _root.Q<Label>("LobbyTitle");
            _backButton = _root.Q<Button>("LeaveButton");

            // Join Code
            _joinCodeLabel = _root.Q<Label>("JoinCodeLabel");
            _copyJoinCodeButton = _root.Q<Button>("CopyCodeButton");

            // Player List
            _playerListContainer = _root.Q<VisualElement>("PlayerListContainer");
            _playerListScrollView = _root.Q<ScrollView>("PlayerListScrollView");

            // Controls
            _readyButton = _root.Q<Button>("ReadyButton");
            _startGameButton = _root.Q<Button>("StartGameButton");
            _statusLabel = _root.Q<Label>("StatusLabel");

            // 警告: 見つからない要素
            if (_joinCodeLabel == null)
                Debug.LogWarning("[LobbyView] JoinCodeLabel not found in UXML.");
            if (_copyJoinCodeButton == null)
                Debug.LogWarning("[LobbyView] CopyCodeButton not found in UXML.");
            if (_playerListScrollView == null)
                Debug.LogWarning("[LobbyView] PlayerListScrollView not found in UXML.");
            if (_readyButton == null)
                Debug.LogWarning("[LobbyView] ReadyButton not found in UXML.");
            if (_startGameButton == null)
                Debug.LogWarning("[LobbyView] StartGameButton not found in UXML.");
        }

        // ============================================
        // イベントハンドラ登録/解除
        // ============================================

        private void RegisterEventHandlers()
        {
            // UI要素のイベント
            if (_backButton != null)
                _backButton.clicked += OnBackButtonClicked;

            if (_copyJoinCodeButton != null)
                _copyJoinCodeButton.clicked += OnCopyJoinCodeButtonClicked;

            if (_readyButton != null)
                _readyButton.clicked += OnReadyButtonClicked;

            if (_startGameButton != null)
                _startGameButton.clicked += OnStartGameButtonClicked;

            // ネットワークサービスのイベント
            if (_networkService != null)
            {
                _networkService.OnPlayerJoined += OnPlayerJoined;
                _networkService.OnPlayerLeft += OnPlayerLeft;
            }
        }

        private void UnregisterEventHandlers()
        {
            if (_backButton != null)
                _backButton.clicked -= OnBackButtonClicked;

            if (_copyJoinCodeButton != null)
                _copyJoinCodeButton.clicked -= OnCopyJoinCodeButtonClicked;

            if (_readyButton != null)
                _readyButton.clicked -= OnReadyButtonClicked;

            if (_startGameButton != null)
                _startGameButton.clicked -= OnStartGameButtonClicked;

            if (_networkService != null)
            {
                _networkService.OnPlayerJoined -= OnPlayerJoined;
                _networkService.OnPlayerLeft -= OnPlayerLeft;
            }
        }

        // ============================================
        // ViewModelバインディング
        // ============================================

        private void BindViewModel()
        {
            if (_viewModel == null) return;

            // Join Code変更を監視
            _viewModel.JoinCode
                .Subscribe(code =>
                {
                    if (_joinCodeLabel != null)
                        _joinCodeLabel.text = code;
                })
                .AddTo(_disposables);

            // ローカルプレイヤーReady状態を監視
            _viewModel.LocalPlayerReady
                .Subscribe(isReady =>
                {
                    if (_readyButton != null)
                        _readyButton.text = isReady ? "Cancel Ready" : "Ready";
                })
                .AddTo(_disposables);

            // ゲーム開始可能状態を監視
            _viewModel.CanStartGame
                .Subscribe(canStart =>
                {
                    if (_startGameButton != null)
                        _startGameButton.SetEnabled(canStart);
                })
                .AddTo(_disposables);

            // ホスト状態を監視（ホストのみStart Gameボタン表示）
            _viewModel.IsHost
                .Subscribe(isHost =>
                {
                    if (_startGameButton != null)
                        _startGameButton.style.display = isHost ? DisplayStyle.Flex : DisplayStyle.None;
                })
                .AddTo(_disposables);

            // プレイヤーリストの変更を監視（ReactiveCollection）
            _viewModel.PlayerList.ObserveAdd()
                .Subscribe(_ => UpdatePlayerListUI())
                .AddTo(_disposables);

            _viewModel.PlayerList.ObserveRemove()
                .Subscribe(_ => UpdatePlayerListUI())
                .AddTo(_disposables);

            // ViewModelのイベントを購読
            _viewModel.OnStartGameRequested += HandleStartGameRequested;
            _viewModel.OnLeaveRoomRequested += HandleLeaveRoomRequested;
            _viewModel.OnReadyToggleRequested += HandleReadyToggleRequested;
            _viewModel.OnCopyJoinCodeRequested += HandleCopyJoinCodeRequested;

            Debug.Log("[LobbyView] ViewModel bound.");
        }

        private void UnbindViewModel()
        {
            if (_viewModel != null)
            {
                _viewModel.OnStartGameRequested -= HandleStartGameRequested;
                _viewModel.OnLeaveRoomRequested -= HandleLeaveRoomRequested;
                _viewModel.OnReadyToggleRequested -= HandleReadyToggleRequested;
                _viewModel.OnCopyJoinCodeRequested -= HandleCopyJoinCodeRequested;
            }

            _disposables.Clear();
            Debug.Log("[LobbyView] ViewModel unbound.");
        }

        // ============================================
        // UI更新
        // ============================================

        /// <summary>
        /// プレイヤーリストUIを再構築
        /// </summary>
        private void UpdatePlayerListUI()
        {
            if (_playerListScrollView == null || _viewModel == null) return;

            // 既存のリストをクリア
            _playerListScrollView.Clear();

            // プレイヤーごとにUI要素を作成
            foreach (var player in _viewModel.PlayerList)
            {
                var playerElement = CreatePlayerElement(player);
                _playerListScrollView.Add(playerElement);
            }

            Debug.Log($"[LobbyView] Player list UI updated: {_viewModel.PlayerList.Count} players.");
        }

        /// <summary>
        /// プレイヤー情報のUI要素を作成
        /// </summary>
        private VisualElement CreatePlayerElement(LobbyPlayerInfo player)
        {
            var container = new VisualElement();
            container.AddToClassList("player-item");

            var nameLabel = new Label($"{player.Name} {(player.IsHost ? "(Host)" : "")}");
            nameLabel.AddToClassList("player-name");

            var statusLabel = new Label(player.IsReady ? "Ready ✓" : "Not Ready");
            statusLabel.AddToClassList("player-status");

            container.Add(nameLabel);
            container.Add(statusLabel);

            return container;
        }

        // ============================================
        // UIイベントハンドラ
        // ============================================

        private void OnBackButtonClicked()
        {
            Debug.Log("[LobbyView] Back button clicked.");
            _viewModel?.LeaveRoom();
        }

        private void OnCopyJoinCodeButtonClicked()
        {
            Debug.Log("[LobbyView] Copy Join Code button clicked.");
            _viewModel?.CopyJoinCode();
        }

        private void OnReadyButtonClicked()
        {
            Debug.Log("[LobbyView] Ready button clicked.");
            _viewModel?.ToggleReady();
        }

        private void OnStartGameButtonClicked()
        {
            Debug.Log("[LobbyView] Start Game button clicked.");
            _viewModel?.StartGame();
        }

        // ============================================
        // ネットワークイベントハンドラ
        // ============================================

        /// <summary>
        /// プレイヤーがルームに参加したとき
        /// </summary>
        private void OnPlayerJoined(int gameId)
        {
            Debug.Log($"[LobbyView] Player joined: GameId={gameId}");

            if (_playerIdService == null) return;

            string? playerId = _playerIdService.GetPlayerId(gameId);
            if (playerId == null)
            {
                Debug.LogWarning($"[LobbyView] Cannot find PlayerId for GameId={gameId}");
                return;
            }

            bool isHost = (_networkService?.IsMasterClient ?? false) && (gameId == _playerIdService.LocalGameId);
            _viewModel?.AddPlayer(playerId, gameId, isHost);
        }

        /// <summary>
        /// プレイヤーがルームから退出したとき
        /// </summary>
        private void OnPlayerLeft(int gameId)
        {
            Debug.Log($"[LobbyView] Player left: GameId={gameId}");
            _viewModel?.RemovePlayer(gameId);
        }

        // ============================================
        // ViewModelイベントハンドラ
        // ============================================

        private void HandleStartGameRequested()
        {
            Debug.Log("[LobbyView] Start game requested by ViewModel.");

            // ゲーム開始を同期
            if (_gameSyncService != null)
            {
                _gameSyncService.SendGameStartAsync().Forget();
            }

            // ゲームシーンに遷移（例: TicTacToeHex）
            var sceneService = ServiceLocator.Instance.Get<ISceneLoaderService>();
            if (sceneService != null)
            {
                // TODO: ゲームタイプに応じたシーンロード
                Debug.Log("[LobbyView] Transitioning to game scene...");
                // sceneService.LoadGameSceneAsync("TicTacToeHex").Forget();
            }
        }

        private void HandleLeaveRoomRequested()
        {
            Debug.Log("[LobbyView] Leave room requested by ViewModel.");
            LeaveRoom().Forget();

            // メインメニューに戻る
            var sceneService = ServiceLocator.Instance.Get<ISceneLoaderService>();
            if (sceneService != null)
            {
                sceneService.LoadMainMenuAsync().Forget();
            }
        }

        private void HandleReadyToggleRequested(bool isReady)
        {
            Debug.Log($"[LobbyView] Ready toggle requested: {isReady}");

            // ローカルプレイヤーのReady状態を更新
            if (_playerIdService != null && _viewModel != null)
            {
                int localGameId = _playerIdService.LocalGameId;
                _viewModel.UpdatePlayerReady(localGameId, isReady);
            }

            // TODO: ネットワーク経由でReady状態を同期
            // _networkService?.SyncReadyStateAsync(isReady).Forget();
        }

        private void HandleCopyJoinCodeRequested(string joinCode)
        {
            Debug.Log($"[LobbyView] Copy Join Code requested: {joinCode}");

            // クリップボードにコピー
            GUIUtility.systemCopyBuffer = joinCode;

            // ステータスラベルに通知
            if (_statusLabel != null)
            {
                _statusLabel.text = "Join Code copied to clipboard!";
            }
        }

        // ============================================
        // ネットワーク操作
        // ============================================

        /// <summary>
        /// ルームを作成してJoin Codeを取得
        /// </summary>
        private async UniTask CreateRoomWithJoinCode()
        {
            if (_networkService == null || _viewModel == null)
            {
                Debug.LogError("[LobbyView] NetworkService or ViewModel is null!");
                return;
            }

            try
            {
                // Unity Multiplayer Servicesを初期化
                if (_networkService is UnityMultiplayerNetworkService unityService)
                {
                    bool initialized = await unityService.InitializeAsync();
                    if (!initialized)
                    {
                        Debug.LogError("[LobbyView] Failed to initialize Unity Multiplayer Services.");
                        return;
                    }
                }

                // ルーム名を生成（タイムスタンプ使用）
                string roomName = $"Room_{DateTime.Now.Ticks}";

                // ルームを作成してJoin Codeを取得
                string? joinCode = null;

                if (_networkService is UnityMultiplayerNetworkService unityMultiplayerService)
                {
                    joinCode = await unityMultiplayerService.CreateRoomWithCodeAsync(roomName, maxPlayers: 2);
                }

                if (string.IsNullOrEmpty(joinCode))
                {
                    Debug.LogError("[LobbyView] Failed to create room with join code.");
                    return;
                }

                // ViewModelにJoin Codeを設定
                _viewModel.SetJoinCode(joinCode);

                // ローカルプレイヤーを追加（ホストとして）
                if (_playerIdService != null)
                {
                    int localGameId = _playerIdService.LocalGameId;
                    string? localPlayerId = _playerIdService.GetPlayerId(localGameId);

                    if (localPlayerId != null)
                    {
                        _viewModel.AddPlayer(localPlayerId, localGameId, isHost: true);
                        _viewModel.IsHost.Value = true;
                    }
                }

                Debug.Log($"[LobbyView] Room created successfully with join code: {joinCode}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[LobbyView] Error creating room: {e.Message}");
            }
        }

        /// <summary>
        /// ルームから退出
        /// </summary>
        private async UniTask LeaveRoom()
        {
            if (_networkService == null) return;

            try
            {
                await _networkService.LeaveRoomAsync();
                Debug.Log("[LobbyView] Left room successfully.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[LobbyView] Error leaving room: {e.Message}");
            }
        }
    }
}
