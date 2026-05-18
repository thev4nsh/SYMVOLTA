using System;
using UnityEngine;
using SYMVOLTA.Utilities;

namespace SYMVOLTA.Core
{
    [Serializable]
    public class GameSettings
    {
        public float musicVolume = 0.5f;
        public float sfxVolume = 0.8f;
        public bool musicEnabled = true;
        public bool sfxEnabled = true;
        public bool notificationsEnabled = true;
    }

    public class SettingsManager : Singleton<SettingsManager>
    {
        private GameSettings _settings = new GameSettings();

        public GameSettings Current => _settings;
        public event Action<GameSettings> OnSettingsChanged;

        public void Initialize()
        {
            _settings.musicVolume = SaveSystem.LoadFloat(Constants.SaveKeys.SETTINGS + "_musicVol", 0.5f);
            _settings.sfxVolume = SaveSystem.LoadFloat(Constants.SaveKeys.SETTINGS + "_sfxVol", 0.8f);
            _settings.musicEnabled = !SaveSystem.LoadBool(Constants.SaveKeys.SETTINGS + "_musicMute", false);
            _settings.sfxEnabled = !SaveSystem.LoadBool(Constants.SaveKeys.SETTINGS + "_sfxMute", false);
            _settings.notificationsEnabled = SaveSystem.LoadBool(Constants.SaveKeys.NOTIFICATION_ENABLED, true);
            OnSettingsChanged?.Invoke(_settings);
        }

        public void SetMusicVolume(float value)
        {
            _settings.musicVolume = Mathf.Clamp01(value);
            Save();
        }

        public void SetSfxVolume(float value)
        {
            _settings.sfxVolume = Mathf.Clamp01(value);
            Save();
        }

        public void SetMusicEnabled(bool enabled)
        {
            _settings.musicEnabled = enabled;
            Save();
        }

        public void SetSfxEnabled(bool enabled)
        {
            _settings.sfxEnabled = enabled;
            Save();
        }

        public void SetNotificationsEnabled(bool enabled)
        {
            _settings.notificationsEnabled = enabled;
            SaveSystem.SaveBool(Constants.SaveKeys.NOTIFICATION_ENABLED, enabled);
            Save();
        }

        private void Save()
        {
            SaveSystem.SaveFloat(Constants.SaveKeys.SETTINGS + "_musicVol", _settings.musicVolume);
            SaveSystem.SaveFloat(Constants.SaveKeys.SETTINGS + "_sfxVol", _settings.sfxVolume);
            SaveSystem.SaveBool(Constants.SaveKeys.SETTINGS + "_musicMute", !_settings.musicEnabled);
            SaveSystem.SaveBool(Constants.SaveKeys.SETTINGS + "_sfxMute", !_settings.sfxEnabled);
            SaveSystem.SaveBool(Constants.SaveKeys.NOTIFICATION_ENABLED, _settings.notificationsEnabled);
            OnSettingsChanged?.Invoke(_settings);
        }
    }
}
