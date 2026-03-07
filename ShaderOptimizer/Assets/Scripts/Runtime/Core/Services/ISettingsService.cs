#nullable enable

using System;
using UnityEngine;

namespace ShaderOp.Core.Services
{
    /// <summary>
    /// 設定管理サービス
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>設定変更イベント</summary>
        event Action<GameSettings>? OnSettingsChanged;

        /// <summary>設定を取得</summary>
        GameSettings GetSettings();

        /// <summary>グラフィック品質を設定</summary>
        void SetGraphicsQuality(GraphicsQuality quality);

        /// <summary>解像度を設定</summary>
        void SetResolution(int width, int height, bool fullscreen);

        /// <summary>VSyncを設定</summary>
        void SetVSync(bool enabled);

        /// <summary>ターゲットフレームレートを設定</summary>
        void SetTargetFrameRate(int frameRate);

        /// <summary>すべての設定を適用</summary>
        void ApplySettings();

        /// <summary>設定をデフォルトに戻す</summary>
        void ResetToDefault();

        /// <summary>設定を保存</summary>
        void SaveSettings();

        /// <summary>設定を読み込み</summary>
        void LoadSettings();

        /// <summary>利用可能な解像度を取得</summary>
        Resolution[] GetAvailableResolutions();

        /// <summary>現在のFPSを取得</summary>
        float GetCurrentFPS();
    }

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
}
