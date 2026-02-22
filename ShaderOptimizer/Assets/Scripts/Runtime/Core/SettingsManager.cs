#nullable enable

using System;
using UnityEngine;

namespace ShaderOp.Core
{
    /// <summary>
    /// グラフィック品質設定
    /// </summary>
    public enum GraphicsQuality
    {
        Low,
        Medium,
        High,
        Ultra
    }

    /// <summary>
    /// 設定データ
    /// </summary>
    [Serializable]
    public class GameSettings
    {
        /// <summary>グラフィック品質</summary>
        public GraphicsQuality GraphicsQuality = GraphicsQuality.High;

        /// <summary>フルスクリーンモード</summary>
        public bool IsFullscreen = true;

        /// <summary>解像度幅</summary>
        public int ResolutionWidth = 1920;

        /// <summary>解像度高さ</summary>
        public int ResolutionHeight = 1080;

        /// <summary>VSync有効</summary>
        public bool EnableVSync = true;

        /// <summary>フレームレート上限</summary>
        public int TargetFrameRate = 60;

        /// <summary>アンチエイリアス</summary>
        public int AntiAliasing = 4;

        /// <summary>影品質</summary>
        public ShadowQuality ShadowQuality = ShadowQuality.All;

        /// <summary>テクスチャ品質</summary>
        public int TextureQuality = 0;

        /// <summary>
        /// デフォルト設定を作成
        /// </summary>
        public static GameSettings CreateDefault()
        {
            return new GameSettings
            {
                GraphicsQuality = GraphicsQuality.High,
                IsFullscreen = true,
                ResolutionWidth = Screen.currentResolution.width,
                ResolutionHeight = Screen.currentResolution.height,
                EnableVSync = true,
                TargetFrameRate = 60,
                AntiAliasing = 4,
                ShadowQuality = ShadowQuality.All,
                TextureQuality = 0
            };
        }

        /// <summary>
        /// JSONに変換
        /// </summary>
        public string ToJson()
        {
            return JsonUtility.ToJson(this, true);
        }

        /// <summary>
        /// JSONから復元
        /// </summary>
        public static GameSettings FromJson(string json)
        {
            return JsonUtility.FromJson<GameSettings>(json);
        }
    }

    /// <summary>
    /// 設定マネージャー
    /// </summary>
    /// <remarks>
    /// ゲーム設定の管理と適用
    /// グラフィック品質、解像度、オーディオ設定を統合管理
    /// </remarks>
    public class SettingsManager : MonoBehaviour
    {
        /// <summary>シングルトンインスタンス</summary>
        private static SettingsManager? _instance;
        public static SettingsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("SettingsManager");
                    _instance = go.AddComponent<SettingsManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        /// <summary>現在の設定</summary>
        private GameSettings _settings = GameSettings.CreateDefault();

        /// <summary>設定変更イベント</summary>
        public event Action<GameSettings>? OnSettingsChanged;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                LoadSettings();
                ApplySettings();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 設定を取得
        /// </summary>
        public GameSettings GetSettings()
        {
            return _settings;
        }

        /// <summary>
        /// グラフィック品質を設定
        /// </summary>
        public void SetGraphicsQuality(GraphicsQuality quality)
        {
            _settings.GraphicsQuality = quality;
            ApplyGraphicsQuality(quality);
            OnSettingsChanged?.Invoke(_settings);
            Debug.Log($"[SettingsManager] Graphics quality set to {quality}");
        }

        /// <summary>
        /// グラフィック品質を適用
        /// </summary>
        private void ApplyGraphicsQuality(GraphicsQuality quality)
        {
            switch (quality)
            {
                case GraphicsQuality.Low:
                    QualitySettings.SetQualityLevel(0, true);
                    _settings.ShadowQuality = ShadowQuality.Disable;
                    _settings.AntiAliasing = 0;
                    _settings.TextureQuality = 2;
                    break;

                case GraphicsQuality.Medium:
                    QualitySettings.SetQualityLevel(1, true);
                    _settings.ShadowQuality = ShadowQuality.HardOnly;
                    _settings.AntiAliasing = 2;
                    _settings.TextureQuality = 1;
                    break;

                case GraphicsQuality.High:
                    QualitySettings.SetQualityLevel(2, true);
                    _settings.ShadowQuality = ShadowQuality.All;
                    _settings.AntiAliasing = 4;
                    _settings.TextureQuality = 0;
                    break;

                case GraphicsQuality.Ultra:
                    QualitySettings.SetQualityLevel(3, true);
                    _settings.ShadowQuality = ShadowQuality.All;
                    _settings.AntiAliasing = 8;
                    _settings.TextureQuality = 0;
                    break;
            }

            QualitySettings.shadows = _settings.ShadowQuality;
            QualitySettings.antiAliasing = _settings.AntiAliasing;
            QualitySettings.masterTextureLimit = _settings.TextureQuality;
        }

        /// <summary>
        /// 解像度を設定
        /// </summary>
        public void SetResolution(int width, int height, bool fullscreen)
        {
            _settings.ResolutionWidth = width;
            _settings.ResolutionHeight = height;
            _settings.IsFullscreen = fullscreen;

            Screen.SetResolution(width, height, fullscreen);
            OnSettingsChanged?.Invoke(_settings);
            Debug.Log($"[SettingsManager] Resolution set to {width}x{height} (Fullscreen: {fullscreen})");
        }

        /// <summary>
        /// VSyncを設定
        /// </summary>
        public void SetVSync(bool enabled)
        {
            _settings.EnableVSync = enabled;
            QualitySettings.vSyncCount = enabled ? 1 : 0;
            OnSettingsChanged?.Invoke(_settings);
            Debug.Log($"[SettingsManager] VSync {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// ターゲットフレームレートを設定
        /// </summary>
        public void SetTargetFrameRate(int frameRate)
        {
            _settings.TargetFrameRate = frameRate;
            Application.targetFrameRate = frameRate;
            OnSettingsChanged?.Invoke(_settings);
            Debug.Log($"[SettingsManager] Target frame rate set to {frameRate}");
        }

        /// <summary>
        /// すべての設定を適用
        /// </summary>
        public void ApplySettings()
        {
            ApplyGraphicsQuality(_settings.GraphicsQuality);
            Screen.SetResolution(_settings.ResolutionWidth, _settings.ResolutionHeight, _settings.IsFullscreen);
            QualitySettings.vSyncCount = _settings.EnableVSync ? 1 : 0;
            Application.targetFrameRate = _settings.TargetFrameRate;

            Debug.Log("[SettingsManager] All settings applied");
        }

        /// <summary>
        /// 設定をデフォルトに戻す
        /// </summary>
        public void ResetToDefault()
        {
            _settings = GameSettings.CreateDefault();
            ApplySettings();
            OnSettingsChanged?.Invoke(_settings);
            Debug.Log("[SettingsManager] Settings reset to default");
        }

        /// <summary>
        /// 設定を保存
        /// </summary>
        public void SaveSettings()
        {
            string json = _settings.ToJson();
            PlayerPrefs.SetString("GameSettings", json);
            PlayerPrefs.Save();
            Debug.Log("[SettingsManager] Settings saved");
        }

        /// <summary>
        /// 設定を読み込み
        /// </summary>
        public void LoadSettings()
        {
            if (PlayerPrefs.HasKey("GameSettings"))
            {
                string json = PlayerPrefs.GetString("GameSettings");
                _settings = GameSettings.FromJson(json);
                Debug.Log("[SettingsManager] Settings loaded from PlayerPrefs");
            }
            else
            {
                _settings = GameSettings.CreateDefault();
                Debug.Log("[SettingsManager] Using default settings");
            }
        }

        /// <summary>
        /// 利用可能な解像度を取得
        /// </summary>
        public Resolution[] GetAvailableResolutions()
        {
            return Screen.resolutions;
        }

        /// <summary>
        /// 現在のFPSを取得
        /// </summary>
        public float GetCurrentFPS()
        {
            return 1.0f / Time.deltaTime;
        }
    }
}
