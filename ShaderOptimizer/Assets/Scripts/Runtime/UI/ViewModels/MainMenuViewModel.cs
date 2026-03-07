#nullable enable

using System;
using UniRx;

namespace ShaderOp.UI.ViewModels
{
    /// <summary>
    /// メインメニュー画面のViewModel
    /// </summary>
    public class MainMenuViewModel : IDisposable
    {
        private readonly CompositeDisposable _disposables = new();

        /// <summary>プレイヤー名</summary>
        public ReactiveProperty<string> PlayerName { get; } = new("Player");

        /// <summary>プレイヤーレベル</summary>
        public ReactiveProperty<int> PlayerLevel { get; } = new(1);

        /// <summary>所持コイン</summary>
        public ReactiveProperty<int> Coins { get; } = new(0);

        /// <summary>所持ジェム</summary>
        public ReactiveProperty<int> Gems { get; } = new(0);

        /// <summary>未読通知数</summary>
        public ReactiveProperty<int> NotificationCount { get; } = new(0);

        /// <summary>デイリーログインボーナス受け取り可能</summary>
        public ReactiveProperty<bool> HasDailyReward { get; } = new(false);

        /// <summary>カスタマイズボタンクリックイベント</summary>
        public event Action? OnCustomizeClicked;

        /// <summary>ゲーム選択ボタンクリックイベント</summary>
        public event Action? OnGamesClicked;

        /// <summary>フレンドボタンクリックイベント</summary>
        public event Action? OnFriendsClicked;

        /// <summary>ショップボタンクリックイベント</summary>
        public event Action? OnShopClicked;

        /// <summary>設定ボタンクリックイベント</summary>
        public event Action? OnSettingsClicked;

        /// <summary>デイリーリワードクリックイベント</summary>
        public event Action? OnDailyRewardClicked;

        public MainMenuViewModel()
        {
            // プレイヤー情報の変更を監視
            PlayerLevel.Subscribe(level =>
            {
                UnityEngine.Debug.Log($"[MainMenuViewModel] Player level changed to {level}");
            }).AddTo(_disposables);

            // 初期データをロード
            LoadPlayerData();
        }

        /// <summary>
        /// プレイヤーデータをロード
        /// </summary>
        private void LoadPlayerData()
        {
            // TODO: 実際のサービスから取得
            PlayerName.Value = "TestPlayer";
            PlayerLevel.Value = 42;
            Coins.Value = 1000;
            Gems.Value = 50;
            NotificationCount.Value = 3;
            HasDailyReward.Value = true;
        }

        /// <summary>
        /// カスタマイズボタンクリック処理
        /// </summary>
        public void HandleCustomizeClicked()
        {
            UnityEngine.Debug.Log("[MainMenuViewModel] Customize button clicked");
            OnCustomizeClicked?.Invoke();
        }

        /// <summary>
        /// ゲーム選択ボタンクリック処理
        /// </summary>
        public void HandleGamesClicked()
        {
            UnityEngine.Debug.Log("[MainMenuViewModel] Games button clicked");
            OnGamesClicked?.Invoke();
        }

        /// <summary>
        /// フレンドボタンクリック処理
        /// </summary>
        public void HandleFriendsClicked()
        {
            UnityEngine.Debug.Log("[MainMenuViewModel] Friends button clicked");
            OnFriendsClicked?.Invoke();
        }

        /// <summary>
        /// ショップボタンクリック処理
        /// </summary>
        public void HandleShopClicked()
        {
            UnityEngine.Debug.Log("[MainMenuViewModel] Shop button clicked");
            OnShopClicked?.Invoke();
        }

        /// <summary>
        /// 設定ボタンクリック処理
        /// </summary>
        public void HandleSettingsClicked()
        {
            UnityEngine.Debug.Log("[MainMenuViewModel] Settings button clicked");
            OnSettingsClicked?.Invoke();
        }

        /// <summary>
        /// デイリーリワードクリック処理
        /// </summary>
        public void HandleDailyRewardClicked()
        {
            UnityEngine.Debug.Log("[MainMenuViewModel] Daily reward claimed");
            HasDailyReward.Value = false;
            Coins.Value += 100; // リワード付与
            OnDailyRewardClicked?.Invoke();
        }

        public void Dispose()
        {
            _disposables?.Dispose();
        }
    }
}
