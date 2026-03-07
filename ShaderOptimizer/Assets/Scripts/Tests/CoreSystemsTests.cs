#nullable enable

using NUnit.Framework;
using ShaderOp.Core;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

namespace ShaderOp.Tests
{
    /// <summary>
    /// コアシステムのテスト
    /// </summary>
    [TestFixture]
    public class CoreSystemsTests
    {
        #region AudioManager Tests

        [Test]
        public void AudioManager_Singleton_ReturnsSameInstance()
        {
            AudioManager instance1 = AudioManager.Instance;
            AudioManager instance2 = AudioManager.Instance;

            Assert.IsNotNull(instance1);
            Assert.AreSame(instance1, instance2);
        }

        [Test]
        public void AudioManager_SetMasterVolume_ClampsValue()
        {
            AudioManager manager = AudioManager.Instance;

            manager.SetMasterVolume(1.5f);
            Assert.AreEqual(1.0f, manager.MasterVolume);

            manager.SetMasterVolume(-0.5f);
            Assert.AreEqual(0.0f, manager.MasterVolume);

            manager.SetMasterVolume(0.5f);
            Assert.AreEqual(0.5f, manager.MasterVolume);
        }

        [Test]
        public void AudioManager_SetBGMVolume_ClampsValue()
        {
            AudioManager manager = AudioManager.Instance;

            manager.SetBGMVolume(1.5f);
            Assert.AreEqual(1.0f, manager.BGMVolume);

            manager.SetBGMVolume(-0.5f);
            Assert.AreEqual(0.0f, manager.BGMVolume);

            manager.SetBGMVolume(0.7f);
            Assert.AreEqual(0.7f, manager.BGMVolume);
        }

        [Test]
        public void AudioManager_SetSFXVolume_ClampsValue()
        {
            AudioManager manager = AudioManager.Instance;

            manager.SetSFXVolume(1.5f);
            Assert.AreEqual(1.0f, manager.SFXVolume);

            manager.SetSFXVolume(-0.5f);
            Assert.AreEqual(0.0f, manager.SFXVolume);

            manager.SetSFXVolume(0.8f);
            Assert.AreEqual(0.8f, manager.SFXVolume);
        }

        [Test]
        public void AudioManager_ToggleBGMMute_TogglesState()
        {
            AudioManager manager = AudioManager.Instance;

            bool initialState = manager.IsBGMMuted;
            manager.ToggleBGMMute();
            Assert.AreEqual(!initialState, manager.IsBGMMuted);

            manager.ToggleBGMMute();
            Assert.AreEqual(initialState, manager.IsBGMMuted);
        }

        [Test]
        public void AudioManager_ToggleSFXMute_TogglesState()
        {
            AudioManager manager = AudioManager.Instance;

            bool initialState = manager.IsSFXMuted;
            manager.ToggleSFXMute();
            Assert.AreEqual(!initialState, manager.IsSFXMuted);

            manager.ToggleSFXMute();
            Assert.AreEqual(initialState, manager.IsSFXMuted);
        }

        [Test]
        public void AudioManager_SaveAndLoadSettings_PreservesValues()
        {
            AudioManager manager = AudioManager.Instance;

            // 設定を変更
            manager.SetMasterVolume(0.75f);
            manager.SetBGMVolume(0.6f);
            manager.SetSFXVolume(0.9f);
            manager.IsBGMMuted = true;
            manager.IsSFXMuted = false;

            // 保存
            manager.SaveSettings();

            // 値を変更
            manager.SetMasterVolume(0.5f);
            manager.SetBGMVolume(0.3f);
            manager.SetSFXVolume(0.4f);

            // 読み込み
            manager.LoadSettings();

            // 元の値に戻っているか確認
            Assert.AreEqual(0.75f, manager.MasterVolume, 0.01f);
            Assert.AreEqual(0.6f, manager.BGMVolume, 0.01f);
            Assert.AreEqual(0.9f, manager.SFXVolume, 0.01f);
            Assert.AreEqual(true, manager.IsBGMMuted);
            Assert.AreEqual(false, manager.IsSFXMuted);
        }

        #endregion

        #region SettingsManager Tests

        [Test]
        public void SettingsManager_Singleton_ReturnsSameInstance()
        {
            SettingsManager instance1 = SettingsManager.Instance;
            SettingsManager instance2 = SettingsManager.Instance;

            Assert.IsNotNull(instance1);
            Assert.AreSame(instance1, instance2);
        }

        [Test]
        public void SettingsManager_GetSettings_ReturnsValidSettings()
        {
            SettingsManager manager = SettingsManager.Instance;
            GameSettings settings = manager.GetSettings();

            Assert.IsNotNull(settings);
            Assert.IsTrue(settings.GraphicsQuality >= GraphicsQuality.Low);
            Assert.IsTrue(settings.GraphicsQuality <= GraphicsQuality.Ultra);
            Assert.Greater(settings.ResolutionWidth, 0);
            Assert.Greater(settings.ResolutionHeight, 0);
        }

        [Test]
        public void SettingsManager_SetGraphicsQuality_UpdatesSettings()
        {
            SettingsManager manager = SettingsManager.Instance;

            manager.SetGraphicsQuality(GraphicsQuality.Low);
            Assert.AreEqual(GraphicsQuality.Low, manager.GetSettings().GraphicsQuality);

            manager.SetGraphicsQuality(GraphicsQuality.High);
            Assert.AreEqual(GraphicsQuality.High, manager.GetSettings().GraphicsQuality);

            manager.SetGraphicsQuality(GraphicsQuality.Ultra);
            Assert.AreEqual(GraphicsQuality.Ultra, manager.GetSettings().GraphicsQuality);
        }

        [Test]
        public void SettingsManager_SetResolution_UpdatesSettings()
        {
            SettingsManager manager = SettingsManager.Instance;

            manager.SetResolution(1920, 1080, true);

            GameSettings settings = manager.GetSettings();
            Assert.AreEqual(1920, settings.ResolutionWidth);
            Assert.AreEqual(1080, settings.ResolutionHeight);
            Assert.IsTrue(settings.IsFullscreen);
        }

        [Test]
        public void SettingsManager_SetVSync_UpdatesSettings()
        {
            SettingsManager manager = SettingsManager.Instance;

            manager.SetVSync(true);
            Assert.IsTrue(manager.GetSettings().EnableVSync);

            manager.SetVSync(false);
            Assert.IsFalse(manager.GetSettings().EnableVSync);
        }

        [Test]
        public void SettingsManager_SetTargetFrameRate_UpdatesSettings()
        {
            SettingsManager manager = SettingsManager.Instance;

            manager.SetTargetFrameRate(60);
            Assert.AreEqual(60, manager.GetSettings().TargetFrameRate);

            manager.SetTargetFrameRate(120);
            Assert.AreEqual(120, manager.GetSettings().TargetFrameRate);
        }

        [Test]
        public void SettingsManager_ResetToDefault_RestoresDefaults()
        {
            SettingsManager manager = SettingsManager.Instance;

            // 設定を変更
            manager.SetGraphicsQuality(GraphicsQuality.Low);
            manager.SetTargetFrameRate(30);

            // デフォルトに戻す
            manager.ResetToDefault();

            GameSettings settings = manager.GetSettings();
            Assert.AreEqual(GraphicsQuality.High, settings.GraphicsQuality);
            Assert.AreEqual(60, settings.TargetFrameRate);
        }

        [Test]
        public void SettingsManager_SaveAndLoadSettings_PreservesValues()
        {
            SettingsManager manager = SettingsManager.Instance;

            // 設定を変更
            manager.SetGraphicsQuality(GraphicsQuality.Medium);
            manager.SetResolution(1280, 720, false);
            manager.SetVSync(false);
            manager.SetTargetFrameRate(90);

            // 保存
            manager.SaveSettings();

            // 値を変更
            manager.SetGraphicsQuality(GraphicsQuality.Low);
            manager.SetTargetFrameRate(30);

            // 読み込み
            manager.LoadSettings();

            GameSettings settings = manager.GetSettings();
            Assert.AreEqual(GraphicsQuality.Medium, settings.GraphicsQuality);
            Assert.AreEqual(1280, settings.ResolutionWidth);
            Assert.AreEqual(720, settings.ResolutionHeight);
            Assert.IsFalse(settings.IsFullscreen);
            Assert.IsFalse(settings.EnableVSync);
            Assert.AreEqual(90, settings.TargetFrameRate);
        }

        [Test]
        public void SettingsManager_OnSettingsChanged_TriggersEvent()
        {
            SettingsManager manager = SettingsManager.Instance;

            bool eventTriggered = false;
            GameSettings? eventSettings = null;

            manager.OnSettingsChanged += (settings) =>
            {
                eventTriggered = true;
                eventSettings = settings;
            };

            manager.SetGraphicsQuality(GraphicsQuality.Ultra);

            Assert.IsTrue(eventTriggered);
            Assert.IsNotNull(eventSettings);
            Assert.AreEqual(GraphicsQuality.Ultra, eventSettings!.GraphicsQuality);
        }

        [Test]
        public void SettingsManager_GetAvailableResolutions_ReturnsArray()
        {
            SettingsManager manager = SettingsManager.Instance;

            Resolution[] resolutions = manager.GetAvailableResolutions();

            Assert.IsNotNull(resolutions);
            Assert.Greater(resolutions.Length, 0);
        }

        [Test]
        public void SettingsManager_GetCurrentFPS_ReturnsPositiveValue()
        {
            SettingsManager manager = SettingsManager.Instance;

            float fps = manager.GetCurrentFPS();

            Assert.Greater(fps, 0f);
        }

        #endregion

        #region GameSettings Tests

        [Test]
        public void GameSettings_CreateDefault_CreatesValidSettings()
        {
            GameSettings settings = GameSettings.CreateDefault();

            Assert.IsNotNull(settings);
            Assert.AreEqual(GraphicsQuality.High, settings.GraphicsQuality);
            Assert.IsTrue(settings.IsFullscreen);
            Assert.Greater(settings.ResolutionWidth, 0);
            Assert.Greater(settings.ResolutionHeight, 0);
            Assert.IsTrue(settings.EnableVSync);
            Assert.AreEqual(60, settings.TargetFrameRate);
        }

        [Test]
        public void GameSettings_ToJson_CreatesValidJson()
        {
            GameSettings settings = GameSettings.CreateDefault();
            string json = settings.ToJson();

            Assert.IsNotNull(json);
            Assert.IsNotEmpty(json);
            Assert.IsTrue(json.Contains("GraphicsQuality"));
            Assert.IsTrue(json.Contains("ResolutionWidth"));
        }

        [Test]
        public void GameSettings_FromJson_RestoresSettings()
        {
            GameSettings original = new GameSettings
            {
                GraphicsQuality = GraphicsQuality.Medium,
                IsFullscreen = false,
                ResolutionWidth = 1280,
                ResolutionHeight = 720,
                EnableVSync = false,
                TargetFrameRate = 90,
                AntiAliasing = 2,
                ShadowQuality = ShadowQuality.HardOnly,
                TextureQuality = 1
            };

            string json = original.ToJson();
            GameSettings restored = GameSettings.FromJson(json);

            Assert.AreEqual(original.GraphicsQuality, restored.GraphicsQuality);
            Assert.AreEqual(original.IsFullscreen, restored.IsFullscreen);
            Assert.AreEqual(original.ResolutionWidth, restored.ResolutionWidth);
            Assert.AreEqual(original.ResolutionHeight, restored.ResolutionHeight);
            Assert.AreEqual(original.EnableVSync, restored.EnableVSync);
            Assert.AreEqual(original.TargetFrameRate, restored.TargetFrameRate);
            Assert.AreEqual(original.AntiAliasing, restored.AntiAliasing);
            Assert.AreEqual(original.ShadowQuality, restored.ShadowQuality);
            Assert.AreEqual(original.TextureQuality, restored.TextureQuality);
        }

        #endregion

        #region SceneLoader Tests

        [Test]
        public void SceneLoader_Singleton_ReturnsSameInstance()
        {
            SceneLoader instance1 = SceneLoader.Instance;
            SceneLoader instance2 = SceneLoader.Instance;

            Assert.IsNotNull(instance1);
            Assert.AreSame(instance1, instance2);
        }

        [Test]
        public void SceneLoader_InitialState_IsNotLoading()
        {
            SceneLoader loader = SceneLoader.Instance;

            Assert.IsFalse(loader.IsLoading);
            Assert.AreEqual(0f, loader.LoadProgress);
        }

        // Note: この基本テストはSceneLoaderAsyncTests.csに移動しました
        // より包括的な非同期テストについては、SceneLoaderAsyncTests.csとPlayMode/SceneLoaderPlayModeTests.csを参照してください

        #endregion

        #region InputManager Tests

        [Test]
        public void InputManager_Singleton_ReturnsSameInstance()
        {
            InputManager instance1 = InputManager.Instance;
            InputManager instance2 = InputManager.Instance;

            Assert.IsNotNull(instance1);
            Assert.AreSame(instance1, instance2);
        }

        // MousePositionとSupportsTouchプロパティは削除されました
        // [Test]
        // public void InputManager_MousePosition_ReturnsValidPosition()
        // {
        //     InputManager manager = InputManager.Instance;
        //
        //     Vector2 mousePos = manager.MousePosition;
        //
        //     // マウス位置は任意だが、取得できることを確認
        //     Assert.IsNotNull(mousePos);
        // }
        //
        // [Test]
        // public void InputManager_SupportsTouch_ReturnsBool()
        // {
        //     InputManager manager = InputManager.Instance;
        //
        //     bool supportsTouch = manager.SupportsTouch;
        //
        //     // タッチサポートの有無は環境依存だが、値は取得できる
        //     Assert.IsTrue(supportsTouch || !supportsTouch);
        // }

        #endregion

        #region GameManager Tests

        [Test]
        public void GameManager_Singleton_ReturnsSameInstance()
        {
            GameManager instance1 = GameManager.Instance;
            GameManager instance2 = GameManager.Instance;

            Assert.IsNotNull(instance1);
            Assert.AreSame(instance1, instance2);
        }

        #endregion
    }
}
