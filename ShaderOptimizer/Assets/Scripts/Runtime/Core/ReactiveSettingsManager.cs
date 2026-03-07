#nullable enable

using System;
using UniRx;
using UnityEngine;

namespace ShaderOp.Core
{
    /// <summary>
    /// 設定管理（UniRx版）
    /// </summary>
    /// <remarks>
    /// ReactivePropertyを使用して設定値の変更を自動通知
    /// UIバインディングとの相性が良い
    /// </remarks>
    public class ReactiveSettingsManager : MonoBehaviour
    {
        /// <summary>シングルトンインスタンス</summary>
        private static ReactiveSettingsManager? _instance;
        public static ReactiveSettingsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("ReactiveSettingsManager");
                    _instance = go.AddComponent<ReactiveSettingsManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        /// <summary>マスター音量（0.0 - 1.0）</summary>
        public ReactiveProperty<float> MasterVolume { get; private set; } = new ReactiveProperty<float>(1.0f);

        /// <summary>BGM音量（0.0 - 1.0）</summary>
        public ReactiveProperty<float> BGMVolume { get; private set; } = new ReactiveProperty<float>(0.8f);

        /// <summary>SE音量（0.0 - 1.0）</summary>
        public ReactiveProperty<float> SEVolume { get; private set; } = new ReactiveProperty<float>(1.0f);

        /// <summary>グラフィック品質レベル（0-5）</summary>
        public ReactiveProperty<int> GraphicsQuality { get; private set; } = new ReactiveProperty<int>(3);

        /// <summary>フルスクリーン有効化</summary>
        public ReactiveProperty<bool> IsFullscreen { get; private set; } = new ReactiveProperty<bool>(true);

        /// <summary>VSync有効化</summary>
        public ReactiveProperty<bool> IsVSyncEnabled { get; private set; } = new ReactiveProperty<bool>(true);

        /// <summary>アンチエイリアス有効化</summary>
        public ReactiveProperty<bool> IsAntiAliasingEnabled { get; private set; } = new ReactiveProperty<bool>(true);

        /// <summary>影の品質（0=無効, 1=低, 2=中, 3=高）</summary>
        public ReactiveProperty<int> ShadowQuality { get; private set; } = new ReactiveProperty<int>(2);

        /// <summary>解像度スケール（0.5 - 2.0）</summary>
        public ReactiveProperty<float> ResolutionScale { get; private set; } = new ReactiveProperty<float>(1.0f);

        /// <summary>設定が変更されたかどうか（ダーティフラグ）</summary>
        public IReadOnlyReactiveProperty<bool> IsDirty { get; private set; } = null!;

        /// <summary>CompositeDisposable（全Subscriptionの管理）</summary>
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);

                InitializeReactiveProperties();
                LoadSettings();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// ReactivePropertiesの初期化と購読設定
        /// </summary>
        private void InitializeReactiveProperties()
        {
            // ダーティフラグの計算（いずれかの設定が変更されたらtrue）
            var isDirtyProperty = new ReactiveProperty<bool>(false);
            IsDirty = isDirtyProperty.ToReadOnlyReactiveProperty();

            Observable.Merge(
                MasterVolume.AsUnitObservable(),
                BGMVolume.AsUnitObservable(),
                SEVolume.AsUnitObservable(),
                GraphicsQuality.AsUnitObservable(),
                IsFullscreen.AsUnitObservable(),
                IsVSyncEnabled.AsUnitObservable(),
                IsAntiAliasingEnabled.AsUnitObservable(),
                ShadowQuality.AsUnitObservable(),
                ResolutionScale.AsUnitObservable()
            )
            .Skip(1) // 初期値は無視
            .Subscribe(_ => isDirtyProperty.Value = true)
            .AddTo(_disposables);

            // グラフィック品質が変更されたら自動的に適用
            GraphicsQuality
                .Subscribe(quality =>
                {
                    QualitySettings.SetQualityLevel(quality);
                    Debug.Log($"[ReactiveSettingsManager] Graphics quality changed to: {quality}");
                })
                .AddTo(_disposables);

            // フルスクリーンが変更されたら自動的に適用
            IsFullscreen
                .Subscribe(isFullscreen =>
                {
                    Screen.fullScreen = isFullscreen;
                    Debug.Log($"[ReactiveSettingsManager] Fullscreen changed to: {isFullscreen}");
                })
                .AddTo(_disposables);

            // VSyncが変更されたら自動的に適用
            IsVSyncEnabled
                .Subscribe(isEnabled =>
                {
                    QualitySettings.vSyncCount = isEnabled ? 1 : 0;
                    Debug.Log($"[ReactiveSettingsManager] VSync changed to: {isEnabled}");
                })
                .AddTo(_disposables);

            // アンチエイリアスが変更されたら自動的に適用
            IsAntiAliasingEnabled
                .Subscribe(isEnabled =>
                {
                    QualitySettings.antiAliasing = isEnabled ? 4 : 0;
                    Debug.Log($"[ReactiveSettingsManager] Anti-aliasing changed to: {isEnabled}");
                })
                .AddTo(_disposables);

            // 影の品質が変更されたら自動的に適用
            ShadowQuality
                .Subscribe(quality =>
                {
                    QualitySettings.shadows = quality switch
                    {
                        0 => UnityEngine.ShadowQuality.Disable,
                        1 => UnityEngine.ShadowQuality.HardOnly,
                        2 => UnityEngine.ShadowQuality.All,
                        _ => UnityEngine.ShadowQuality.All
                    };
                    Debug.Log($"[ReactiveSettingsManager] Shadow quality changed to: {quality}");
                })
                .AddTo(_disposables);

            // 音量変更のログ出力（デバッグ用）
            MasterVolume
                .Subscribe(vol => Debug.Log($"[ReactiveSettingsManager] Master volume: {vol:F2}"))
                .AddTo(_disposables);

            BGMVolume
                .Subscribe(vol => Debug.Log($"[ReactiveSettingsManager] BGM volume: {vol:F2}"))
                .AddTo(_disposables);

            SEVolume
                .Subscribe(vol => Debug.Log($"[ReactiveSettingsManager] SE volume: {vol:F2}"))
                .AddTo(_disposables);
        }

        /// <summary>
        /// 設定をPlayerPrefsから読み込み
        /// </summary>
        public void LoadSettings()
        {
            MasterVolume.Value = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
            BGMVolume.Value = PlayerPrefs.GetFloat("BGMVolume", 0.8f);
            SEVolume.Value = PlayerPrefs.GetFloat("SEVolume", 1.0f);
            GraphicsQuality.Value = PlayerPrefs.GetInt("GraphicsQuality", 3);
            IsFullscreen.Value = PlayerPrefs.GetInt("IsFullscreen", 1) == 1;
            IsVSyncEnabled.Value = PlayerPrefs.GetInt("IsVSyncEnabled", 1) == 1;
            IsAntiAliasingEnabled.Value = PlayerPrefs.GetInt("IsAntiAliasingEnabled", 1) == 1;
            ShadowQuality.Value = PlayerPrefs.GetInt("ShadowQuality", 2);
            ResolutionScale.Value = PlayerPrefs.GetFloat("ResolutionScale", 1.0f);

            Debug.Log("[ReactiveSettingsManager] Settings loaded from PlayerPrefs");
        }

        /// <summary>
        /// 設定をPlayerPrefsに保存
        /// </summary>
        public void SaveSettings()
        {
            PlayerPrefs.SetFloat("MasterVolume", MasterVolume.Value);
            PlayerPrefs.SetFloat("BGMVolume", BGMVolume.Value);
            PlayerPrefs.SetFloat("SEVolume", SEVolume.Value);
            PlayerPrefs.SetInt("GraphicsQuality", GraphicsQuality.Value);
            PlayerPrefs.SetInt("IsFullscreen", IsFullscreen.Value ? 1 : 0);
            PlayerPrefs.SetInt("IsVSyncEnabled", IsVSyncEnabled.Value ? 1 : 0);
            PlayerPrefs.SetInt("IsAntiAliasingEnabled", IsAntiAliasingEnabled.Value ? 1 : 0);
            PlayerPrefs.SetInt("ShadowQuality", ShadowQuality.Value);
            PlayerPrefs.SetFloat("ResolutionScale", ResolutionScale.Value);

            PlayerPrefs.Save();
            Debug.Log("[ReactiveSettingsManager] Settings saved to PlayerPrefs");
        }

        /// <summary>
        /// 設定をデフォルトに戻す
        /// </summary>
        public void ResetToDefault()
        {
            MasterVolume.Value = 1.0f;
            BGMVolume.Value = 0.8f;
            SEVolume.Value = 1.0f;
            GraphicsQuality.Value = 3;
            IsFullscreen.Value = true;
            IsVSyncEnabled.Value = true;
            IsAntiAliasingEnabled.Value = true;
            ShadowQuality.Value = 2;
            ResolutionScale.Value = 1.0f;

            Debug.Log("[ReactiveSettingsManager] Settings reset to default");
        }

        /// <summary>
        /// プリセット適用（低品質）
        /// </summary>
        public void ApplyLowQualityPreset()
        {
            GraphicsQuality.Value = 0;
            IsAntiAliasingEnabled.Value = false;
            ShadowQuality.Value = 0;
            ResolutionScale.Value = 0.75f;
        }

        /// <summary>
        /// プリセット適用（中品質）
        /// </summary>
        public void ApplyMediumQualityPreset()
        {
            GraphicsQuality.Value = 2;
            IsAntiAliasingEnabled.Value = true;
            ShadowQuality.Value = 1;
            ResolutionScale.Value = 1.0f;
        }

        /// <summary>
        /// プリセット適用（高品質）
        /// </summary>
        public void ApplyHighQualityPreset()
        {
            GraphicsQuality.Value = 5;
            IsAntiAliasingEnabled.Value = true;
            ShadowQuality.Value = 3;
            ResolutionScale.Value = 1.0f;
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }
    }
}
