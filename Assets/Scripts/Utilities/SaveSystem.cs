using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace SYMVOLTA.Utilities
{
    /// <summary>
    /// Handles local persistence of game data on Android.
    /// Uses Application.persistentDataPath and XOR obfuscation to deter casual tampering.
    /// 
    /// IMPORTANT: The XOR "encryption" is obfuscation, NOT cryptographic security.
    /// The key is embedded in the source code and can be extracted from the compiled APK.
    /// This is acceptable for preventing casual Notepad editing of save files,
    /// but should NOT be relied upon for protecting sensitive data.
    /// For true security, server-side validation (SecurityManager + Firestore) is the
    /// authoritative source of truth.
    /// </summary>
    public static class SaveSystem
    {
        private static string _saveDirectory;
        private static string SaveDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(_saveDirectory))
                {
                    _saveDirectory = Application.persistentDataPath;
                }
                return _saveDirectory;
            }
        }
        // XOR obfuscation key — NOT a secret. Deters casual editing only.
        private static readonly string EncryptionKey = "SYM7V0LT4_S3CUR1TY_K3Y_2024!";

        #region Generic Object Save/Load (JSON + Obfuscated)

        /// <summary>
        /// Saves any serializable object to an obfuscated JSON file.
        /// </summary>
        public static bool SaveData<T>(string key, T data)
        {
            try
            {
                string json = JsonUtility.ToJson(data, true);
                string obfuscatedJson = XORObfuscate(json);
                string filePath = GetFilePath(key);

                File.WriteAllText(filePath, obfuscatedJson);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to save data for key {key}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Loads a serializable object from an obfuscated JSON file.
        /// </summary>
        public static T LoadData<T>(string key) where T : class, new()
        {
            try
            {
                string filePath = GetFilePath(key);

                if (!File.Exists(filePath))
                {
                    Debug.LogWarning($"[SaveSystem] No save file found for key {key}. Returning default.");
                    return new T();
                }

                string obfuscatedJson = File.ReadAllText(filePath);
                string json = XORObfuscate(obfuscatedJson);

                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to load data for key {key}: {e.Message}. Returning default.");
                return new T();
            }
        }

        /// <summary>
        /// Checks if a save file exists for a given key.
        /// </summary>
        public static bool SaveExists(string key)
        {
            return File.Exists(GetFilePath(key));
        }

        /// <summary>
        /// Deletes a specific save file.
        /// </summary>
        public static void DeleteSave(string key)
        {
            string filePath = GetFilePath(key);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        /// <summary>
        /// Deletes all game save files.
        /// </summary>
        public static void DeleteAllSaves()
        {
            try
            {
                string[] files = Directory.GetFiles(SaveDirectory, "*.sym");
                foreach (string file in files)
                {
                    File.Delete(file);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to delete all saves: {e.Message}");
            }
        }

        #endregion

        #region Primitive Save/Load (PlayerPrefs for simple settings)

        public static void SaveBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static bool LoadBool(string key, bool defaultValue = false)
        {
            return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
        }

        public static void SaveInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
            PlayerPrefs.Save();
        }

        public static int LoadInt(string key, int defaultValue = 0)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }

        public static void SaveFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
            PlayerPrefs.Save();
        }

        public static float LoadFloat(string key, float defaultValue = 0f)
        {
            return PlayerPrefs.GetFloat(key, defaultValue);
        }

        public static void SaveString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
            PlayerPrefs.Save();
        }

        public static string LoadString(string key, string defaultValue = "")
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }

        #endregion

        #region Obfuscation Helpers

        private static string GetFilePath(string key)
        {
            return Path.Combine(SaveDirectory, $"{key}.sym"); // Custom extension
        }

        /// <summary>
        /// Simple XOR obfuscation/de-obfuscation.
        /// Applies a key to the string to prevent casual Notepad editing of saves.
        /// This is NOT encryption — see class header comment for details.
        /// </summary>
        private static string XORObfuscate(string text)
        {
            StringBuilder result = new StringBuilder(text.Length);
            for (int i = 0; i < text.Length; i++)
            {
                char c = (char)(text[i] ^ EncryptionKey[i % EncryptionKey.Length]);
                result.Append(c);
            }
            return result.ToString();
        }

        #endregion
    }
}