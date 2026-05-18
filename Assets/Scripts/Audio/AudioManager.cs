using System.Collections.Generic;
using UnityEngine;
using SYMVOLTA.Core;
using SYMVOLTA.Utilities;

namespace SYMVOLTA.Audio
{
    [RequireComponent(typeof(AudioListener))]
    public class AudioManager : Singleton<AudioManager>
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource bgmSource;

        [Header("Pooling")]
        [SerializeField] private int sfxPoolSize = 10;
        private List<AudioSource> _sfxPool;
        private int _currentSfxIndex = 0;

        [Header("Settings")]
        private float _musicVolume = 0.5f;
        private float _sfxVolume = 0.8f;
        private bool _musicMuted = false;
        private bool _sfxMuted = false;

        public float MusicVolume => _musicMuted ? 0f : _musicVolume;
        public float SfxVolume => _sfxMuted ? 0f : _sfxVolume;
        public bool IsMusicMuted => _musicMuted;
        public bool IsSfxMuted => _sfxMuted;

        protected override void Awake()
        {
            base.Awake();
            EnsureBgmSource();
            SetupSFXPool();
        }

        public void Initialize()
        {
            LoadAudioSettings();
            ApplyAudioSettings();
            if (SettingsManager.Instance != null)
                SettingsManager.Instance.OnSettingsChanged += HandleSettingsChanged;
            Debug.Log("[AudioManager] Initialized.");
        }

        private void EnsureBgmSource()
        {
            if (bgmSource != null) return;

            GameObject bgmObj = new GameObject("BGM_Source");
            bgmObj.transform.SetParent(transform, false);
            bgmSource = bgmObj.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
            bgmSource.spatialBlend = 0f;
        }

        private void SetupSFXPool()
        {
            _sfxPool = new List<AudioSource>();
            for (int i = 0; i < sfxPoolSize; i++)
            {
                GameObject sfxObj = new GameObject($"SFX_{i}");
                sfxObj.transform.SetParent(transform);
                AudioSource source = sfxObj.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0f; // 2D Sound
                _sfxPool.Add(source);
            }
        }

        #region Playback

        public void PlayBGM(AudioClip clip, bool loop = true)
        {
            if (bgmSource == null || clip == null) return;
            bgmSource.clip = clip;
            bgmSource.loop = loop;
            bgmSource.volume = MusicVolume;
            bgmSource.Play();
        }

        public void StopBGM()
        {
            if (bgmSource != null) bgmSource.Stop();
        }

        public void PlaySFX(AudioClip clip, float pitchVariation = 0f)
        {
            if (clip == null || _sfxMuted) return;

            AudioSource source = GetNextAvailableSFXSource();
            if (source == null) return;

            source.clip = clip;
            source.volume = SfxVolume;
            source.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
            source.Play();
        }

        private AudioSource GetNextAvailableSFXSource()
        {
            // Round-robin pool approach for performance
            for (int i = 0; i < _sfxPool.Count; i++)
            {
                int index = (_currentSfxIndex + i) % _sfxPool.Count;
                if (!_sfxPool[index].isPlaying)
                {
                    _currentSfxIndex = (index + 1) % _sfxPool.Count;
                    return _sfxPool[index];
                }
            }

            // If all are playing, force override the oldest one
            _currentSfxIndex = (_currentSfxIndex + 1) % _sfxPool.Count;
            return _sfxPool[_currentSfxIndex];
        }

        #endregion

        #region Settings

        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            ApplyAudioSettings();
            SaveAudioSettings();
        }

        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
            ApplyAudioSettings();
            SaveAudioSettings();
        }

        public void ToggleMusicMute()
        {
            _musicMuted = !_musicMuted;
            ApplyAudioSettings();
            SaveAudioSettings();
        }

        public void ToggleSFXMute()
        {
            _sfxMuted = !_sfxMuted;
            SaveAudioSettings();
        }

        public void SetMusicMuted(bool muted)
        {
            _musicMuted = muted;
            ApplyAudioSettings();
            SaveAudioSettings();
        }

        public void SetSfxMuted(bool muted)
        {
            _sfxMuted = muted;
            SaveAudioSettings();
        }

        private void ApplyAudioSettings()
        {
            if (bgmSource != null) bgmSource.volume = MusicVolume;
        }

        private void HandleSettingsChanged(GameSettings settings)
        {
            if (settings == null) return;
            _musicVolume = settings.musicVolume;
            _sfxVolume = settings.sfxVolume;
            _musicMuted = !settings.musicEnabled;
            _sfxMuted = !settings.sfxEnabled;
            ApplyAudioSettings();
        }

        private void LoadAudioSettings()
        {
            _musicVolume = SaveSystem.LoadFloat(Constants.SaveKeys.SETTINGS + "_musicVol", 0.5f);
            _sfxVolume = SaveSystem.LoadFloat(Constants.SaveKeys.SETTINGS + "_sfxVol", 0.8f);
            _musicMuted = SaveSystem.LoadBool(Constants.SaveKeys.SETTINGS + "_musicMute", false);
            _sfxMuted = SaveSystem.LoadBool(Constants.SaveKeys.SETTINGS + "_sfxMute", false);
        }

        private void SaveAudioSettings()
        {
            SaveSystem.SaveFloat(Constants.SaveKeys.SETTINGS + "_musicVol", _musicVolume);
            SaveSystem.SaveFloat(Constants.SaveKeys.SETTINGS + "_sfxVol", _sfxVolume);
            SaveSystem.SaveBool(Constants.SaveKeys.SETTINGS + "_musicMute", _musicMuted);
            SaveSystem.SaveBool(Constants.SaveKeys.SETTINGS + "_sfxMute", _sfxMuted);
        }

        protected override void OnDestroy()
        {
            if (SettingsManager.Instance != null)
                SettingsManager.Instance.OnSettingsChanged -= HandleSettingsChanged;

            base.OnDestroy();
        }

        #endregion
    }
}
