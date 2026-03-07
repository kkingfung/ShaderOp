#nullable enable

using UnityEngine;

namespace ShaderOp.Core.Services
{
    /// <summary>
    /// オーディオ管理サービス
    /// </summary>
    public interface IAudioService
    {
        /// <summary>マスターボリューム</summary>
        float MasterVolume { get; set; }

        /// <summary>BGMボリューム</summary>
        float BGMVolume { get; set; }

        /// <summary>SFXボリューム</summary>
        float SFXVolume { get; set; }

        /// <summary>BGMミュート</summary>
        bool IsBGMMuted { get; set; }

        /// <summary>SFXミュート</summary>
        bool IsSFXMuted { get; set; }

        /// <summary>BGMを再生</summary>
        void PlayBGM(AudioClip clip, bool fadeIn = false, float fadeDuration = 1.0f);

        /// <summary>BGMを停止</summary>
        void StopBGM(bool fadeOut = false, float fadeDuration = 1.0f);

        /// <summary>SFXを再生</summary>
        void PlaySFX(AudioClip clip, float volumeScale = 1.0f);

        /// <summary>ワンショットSFXを再生</summary>
        void PlayOneShotSFX(AudioClip clip, float volumeScale = 1.0f);

        /// <summary>すべてのSFXを停止</summary>
        void StopAllSFX();

        /// <summary>マスターボリュームを設定</summary>
        void SetMasterVolume(float volume);

        /// <summary>BGMボリュームを設定</summary>
        void SetBGMVolume(float volume);

        /// <summary>SFXボリュームを設定</summary>
        void SetSFXVolume(float volume);

        /// <summary>BGMミュート切り替え</summary>
        void ToggleBGMMute();

        /// <summary>SFXミュート切り替え</summary>
        void ToggleSFXMute();

        /// <summary>設定を保存</summary>
        void SaveSettings();

        /// <summary>設定を読み込み</summary>
        void LoadSettings();
    }
}
