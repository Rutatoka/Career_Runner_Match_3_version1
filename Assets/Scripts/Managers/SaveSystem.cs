using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private const string SaveFileName = "save_v1.json";
    private static readonly object fileLock = new object();

    private static string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    [Serializable]
    public class SaveData
    {
        public bool tutorialShown = false;
        public int totalRuns;
     //   public PreferenceData preferenceData;
        public int[] objectsCollected;
        public int[] portalsActivated;
        public int[] challengeSuccess;
        public int[] challengeFail;
        public int gems = 0;
        public int coins = 0;
        public CharacterData character = new CharacterData();
        public string[] ownedItemKeys = new string[0];
        public string version = "v1";
        public string lastSavedUtc = DateTime.UtcNow.ToString("o");
    }

    private static SaveData cache;
    private static bool loaded = false;

    public static void Load()
    {
        if (loaded) return;
        lock (fileLock)
        {
            if (loaded) return;
            if (File.Exists(SaveFilePath))
            {
                try
                {
                    string json = File.ReadAllText(SaveFilePath);
                    cache = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
                }
                catch (Exception e)
                {
                    Debug.LogError($"SaveSystem.Load failed: {e}");
                    cache = new SaveData();
                }
            }
            else cache = new SaveData();
            loaded = true;
        }
    }

    public static void Save()
    {
        if (!loaded) Load();
        lock (fileLock)
        {
            try
            {
                cache.lastSavedUtc = DateTime.UtcNow.ToString("o");
                string json = JsonUtility.ToJson(cache, true);
                File.WriteAllText(SaveFilePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveSystem.Save failed: {e}");
            }
        }
    }
    public static bool IsTutorialShown()
    {
        Load();
        return cache.tutorialShown;
    }

    public static void SetTutorialShown()
    {
        Load();
        cache.tutorialShown = true;
        Save();
        Debug.Log("SaveSystem: Tutorial marked as shown."+ cache.tutorialShown);
    }
    public static string ExportJson() { if (!loaded) Load(); return JsonUtility.ToJson(cache, true); }
    public static void ClearSave() { lock (fileLock) { cache = new SaveData(); loaded = true; try { if (File.Exists(SaveFilePath)) File.Delete(SaveFilePath); } catch (Exception e) { Debug.LogError($"SaveSystem.ClearSave: {e}"); } } }

    // Profile
    public static CharacterData GetCharacter() { Load(); return cache.character; }
    public static void SetCharacter(CharacterData data) { if (data == null) return; Load(); cache.character = data; Save(); }

    // Gems
    public static int GetGems() { Load(); return cache.gems; }
    public static void AddGems(int amount) { if (amount == 0) return; Load(); cache.gems = Mathf.Clamp(cache.gems + amount, 0, 999999); Save(); }
    public static bool SpendGems(int amount) { if (amount <= 0) return false; Load(); if (cache.gems >= amount) { cache.gems -= amount; Save(); return true; } return false; }

    // Coins
    public static int GetCoins() { Load(); return cache.coins; }
    public static void AddCoins(int amount) { if (amount == 0) return; Load(); cache.coins = Mathf.Clamp(cache.coins + amount, 0, 999999); Save(); }
    public static bool SpendCoins(int amount) { if (amount <= 0) return false; Load(); if (cache.coins >= amount) { cache.coins -= amount; Save(); return true; } return false; }

    // Items
    public static bool IsItemOwned(string key) { if (string.IsNullOrEmpty(key)) return false; Load(); if (cache.ownedItemKeys == null || cache.ownedItemKeys.Length == 0) return false; foreach (var k in cache.ownedItemKeys) if (k == key) return true; return false; }
    public static void MarkItemOwned(string key) { if (string.IsNullOrEmpty(key)) return; Load(); var set = new HashSet<string>(cache.ownedItemKeys ?? Array.Empty<string>()); if (set.Add(key)) { cache.ownedItemKeys = new List<string>(set).ToArray(); Save(); } }
    public static void UnmarkItemOwned(string key) { if (string.IsNullOrEmpty(key)) return; Load(); var list = new List<string>(cache.ownedItemKeys ?? Array.Empty<string>()); if (list.Remove(key)) { cache.ownedItemKeys = list.ToArray(); Save(); } }
    public static IReadOnlyList<string> GetOwnedItems() { Load(); return Array.AsReadOnly(cache.ownedItemKeys ?? Array.Empty<string>()); }

    public static void MigrateFromPlayerPrefs() { Load(); try { if (PlayerPrefs.HasKey("Gems")) cache.gems = PlayerPrefs.GetInt("Gems", cache.gems); if (PlayerPrefs.HasKey("Coins")) cache.coins = PlayerPrefs.GetInt("Coins", cache.coins); if (PlayerPrefs.HasKey("CharacterData")) { try { var json = PlayerPrefs.GetString("CharacterData"); var cd = JsonUtility.FromJson<CharacterData>(json); if (cd != null) cache.character = cd; } catch { } } } catch (Exception e) { Debug.LogError($"SaveSystem.MigrateFromPlayerPrefs: {e}"); } Save(); }
}
