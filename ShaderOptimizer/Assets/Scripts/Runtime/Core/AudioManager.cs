#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShaderOp.Core
{
    /// <summary>
    /// オーディオ種別
    /// </summary>
    public enum AudioType
    {
        BGM,
        SFX,
        Voice,
        UI
    }

    /// <summary>
    /// オーディオマネージャー
    /// </summary>
    /// <remarks>
    /// BGM、効果音、ボイスの再生管理
    /// ボリューム制御、フェードイン/アウト機能を提供
    /// </remarks>
    public class AudioManager : MonoBehaviour
    {
        /// <summary>シングルトンインスタンス</summary>
        private static AudioManager? _instance;
        public static AudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("AudioManager");
                    _instance = go.AddComponent<AudioManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        /// <summary>BGM用AudioSource</summary>
        private AudioSource? _bgmSource;

        /// <summary>SFX用AudioSourceプール</summary>
        private List<AudioSource> _sfxSources = new();

        /// <summary>最大同時SFX再生数</summary>
        [SerializeField] private int _maxSfxSources = 10;

        /// <summary>マスターボリューム</summary>
        public float MasterVolume { get; set; } = 1.0f;

        /// <summary>BGMボリューム</summary>
        public float BGMVolume { get; set; } = 0.7f;

        /// <summary>SFXボリューム</summary>
        public float SFXVolume { get; set; } = 1.0f;

        /// <summary>BGMミュート</summary>
        public bool IsBGMMuted { get; set; } = false;

        /// <summary>SFXミュート</summary>
        public bool IsSFXMuted { get; set; } = false;

        /// <summary>現在再生中のBGM</summary>
        private AudioClip? _currentBGM;

        /// <summary>BGMフェード中フラグ</summary>
        private bool _isFading = false;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudioSources();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// AudioSourceを初期化
        /// </summary>
        private void InitializeAudioSources()
        {
            // BGM用AudioSource
            _bgmSource = gameObject.AddComponent<AudioSource>();
            _bgmSource.loop = true;
            _bgmSource.playOnAwake = false;

            // SFX用AudioSourceプールを作成
            for (int i = 0; i < _maxSfxSources; i++)
            {
                AudioSource sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
                _sfxSources.Add(sfxSource);
            }

            Debug.Log($"[AudioManager] Initialized with {_maxSfxSources} SFX sources");
        }

        /// <summary>
        /// BGMを再生
        /// </summary>
        public void PlayBGM(AudioClip clip, bool fadeIn = false, float fadeDuration = 1.0f)
        {
            if (clip == null || _bgmSource == null)
            {
                Debug.LogWarning("[AudioManager] Cannot play BGM: clip or source is null");
                return;
            }

            // 同じBGMが再生中の場合はスキップ
            if (_currentBGM == clip && _bgmSource.isPlaying)
            {
                return;
            }

            _currentBGM = clip;

            if (fadeIn)
            {
                StartCoroutine(FadeInBGM(clip, fadeDuration));
            }
            else
            {
                _bgmSource.clip = clip;
                _bgmSource.volume = GetBGMVolume();
                _bgmSource.Play();
            }

            Debug.Log($"[AudioManager] Playing BGM: {clip.name}");
        }

        /// <summary>
        /// BGMを停止
        /// </summary>
        public void StopBGM(bool fadeOut = false, float fadeDuration = 1.0f)
        {
            if (_bgmSource == null)
                return;

            if (fadeOut)
            {
                StartCoroutine(FadeOutBGM(fadeDuration));
            }
            else
            {
                _bgmSource.Stop();
                _currentBGM = null;
            }

            Debug.Log("[AudioManager] BGM stopped");
        }

        /// <summary>
        /// BGMフェードイン
        /// </summary>
        private System.Collections.IEnumerator FadeInBGM(AudioClip clip, float duration)
        {
            if (_bgmSource == null)
                yield break;

            _isFading = true;
            _bgmSource.clip = clip;
            _bgmSource.volume = 0f;
            _bgmSource.Play();

            float elapsed = 0f;
            float targetVolume = GetBGMVolume();

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _bgmSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
                yield return null;
            }

            _bgmSource.volume = targetVolume;
            _isFading = false;
        }

        /// <summary>
        /// BGMフェードアウト
        /// </summary>
        private System.Collections.IEnumerator FadeOutBGM(float duration)
        {
            if (_bgmSource == null)
                yield break;

            _isFading = true;
            float startVolume = _bgmSource.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }

            _bgmSource.volume = 0f;
            _bgmSource.Stop();
            _currentBGM = null;
            _isFading = false;
        }

        /// <summary>
        /// SFXを再生
        /// </summary>
        public void PlaySFX(AudioClip clip, float volumeScale = 1.0f)
        {
            if (clip == null || IsSFXMuted)
                return;

            // 空いているAudioSourceを探す
            AudioSource? availableSource = GetAvailableSFXSource();
            if (availableSource != null)
            {
                availableSource.clip = clip;
                availableSource.volume = GetSFXVolume() * volumeScale;
                availableSource.Play();
            }
            else
            {
                Debug.LogWarning($"[AudioManager] No available SFX source for {clip.name}");
            }
        }

        /// <summary>
        /// ワンショットSFXを再生
        /// </summary>
        public void PlayOneShotSFX(AudioClip clip, float volumeScale = 1.0f)
        {
            if (clip == null || IsSFXMuted)
                return;

            AudioSource? availableSource = GetAvailableSFXSource();
            if (availableSource != null)
            {
                availableSource.PlayOneShot(clip, GetSFXVolume() * volumeScale);
            }
        }

        /// <summary>
        /// 空いているSFX AudioSourceを取得
        /// </summary>
        private AudioSource? GetAvailableSFXSource()
        {
            foreach (AudioSource source in _sfxSources)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }
            return null;
        }

        /// <summary>
        /// BGMの実効ボリュームを取得
        /// </summary>
        private float GetBGMVolume()
        {
            return IsBGMMuted ? 0f : MasterVolume * BGMVolume;
        }

        /// <summary>
        /// SFXの実効ボリュームを取得
        /// </summary>
        private float GetSFXVolume()
        {
            return IsSFXMuted ? 0f : MasterVolume * SFXVolume;
        }

        /// <summary>
        /// マスターボリュームを設定
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            MasterVolume = Mathf.Clamp01(volume);
            UpdateBGMVolume();
            Debug.Log($"[AudioManager] Master volume set to {MasterVolume:F2}");
        }

        /// <summary>
        /// BGMボリュームを設定
        /// </summary>
        public void SetBGMVolume(float volume)
        {
            BGMVolume = Mathf.Clamp01(volume);
            UpdateBGMVolume();
            Debug.Log($"[AudioManager] BGM volume set to {BGMVolume:F2}");
        }

        /// <summary>
        /// SFXボリュームを設定
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            SFXVolume = Mathf.Clamp01(volume);
            Debug.Log($"[AudioManager] SFX volume set to {SFXVolume:F2}");
        }

        /// <summary>
        /// BGMのボリュームを更新
        /// </summary>
        private void UpdateBGMVolume()
        {
            if (_bgmSource != null && !_isFading)
            {
                _bgmSource.volume = GetBGMVolume();
            }
        }

        /// <summary>
        /// BGMミュート切り替え
        /// </summary>
        public void ToggleBGMMute()
        {
            IsBGMMuted = !IsBGMMuted;
            UpdateBGMVolume();
            Debug.Log($"[AudioManager] BGM mute: {IsBGMMuted}");
        }

        /// <summary>
        /// SFXミュート切り替え
        /// </summary>
        public void ToggleSFXMute()
        {
            IsSFXMuted = !IsSFXMuted;
            Debug.Log($"[AudioManager] SFX mute: {IsSFXMuted}");
        }

        /// <summary>
        /// すべてのSFXを停止
        /// </summary>
        public void StopAllSFX()
        {
            foreach (AudioSource source in _sfxSources)
            {
                if (source.isPlaying)
                {
                    source.Stop();
                }
            }
        }

        /// <summary>
        /// 設定をPlayerPrefsに保存
        /// </summary>
        public void SaveSettings()
        {
            PlayerPrefs.SetFloat("Audio_MasterVolume", MasterVolume);
            PlayerPrefs.SetFloat("Audio_BGMVolume", BGMVolume);
            PlayerPrefs.SetFloat("Audio_SFXVolume", SFXVolume);
            PlayerPrefs.SetInt("Audio_BGMMuted", IsBGMMuted ? 1 : 0);
            PlayerPrefs.SetInt("Audio_SFXMuted", IsSFXMuted ? 1 : 0);
            PlayerPrefs.Save();
            Debug.Log("[AudioManager] Settings saved");
        }

        /// <summary>
        /// 設定をPlayerPrefsから読み込み
        /// </summary>
        public void LoadSettings()
        {
            MasterVolume = PlayerPrefs.GetFloat("Audio_MasterVolume", 1.0f);
            BGMVolume = PlayerPrefs.GetFloat("Audio_BGMVolume", 0.7f);
            SFXVolume = PlayerPrefs.GetFloat("Audio_SFXVolume", 1.0f);
            IsBGMMuted = PlayerPrefs.GetInt("Audio_BGMMuted", 0) == 1;
            IsSFXMuted = PlayerPrefs.GetInt("Audio_SFXMuted", 0) == 1;

            UpdateBGMVolume();
            Debug.Log("[AudioManager] Settings loaded");
        }
    }
}
