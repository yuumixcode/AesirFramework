---
name: unity-save-system
description: Unity save and serialization expert — JSON serialization, binary serialization, PlayerPrefs, ScriptableObject saves, encrypted saves, cloud saves, version migration. Use when designing save systems, implementing data persistence, handling save compatibility, or encrypting sensitive data.
---

# Unity Save & Serialization

## PlayerPrefs (Simple Key-Value)

```csharp
// Lightweight, stored in registry/plist — NOT for complex data
PlayerPrefs.SetInt("Level", 5);
PlayerPrefs.SetFloat("Volume", 0.8f);
PlayerPrefs.SetString("PlayerName", "Hero");
PlayerPrefs.Save(); // Explicit save (auto-saved on quit, but call for safety)

int level = PlayerPrefs.GetInt("Level", 1); // Default 1 if not found
```

**Limitations:** No nested data, no arrays, not encrypted, ~1MB limit on some platforms.

## JSON Serialization (Recommended)

```csharp
using UnityEngine;

// Define save data structure
[Serializable]
public class SaveData
{
    public int level;
    public float health;
    public string playerName;
    public Vector3 position;
    public List<string> inventory;
    public Dictionary<string, int> stats; // JsonUtility doesn't support Dictionary!
}

// JsonUtility (built-in, fast, but limited)
public static class SaveSystem
{
    static string SavePath => Application.persistentDataPath + "/save.json";

    public static void Save(SaveData data) {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
    }

    public static SaveData Load() {
        if (!File.Exists(SavePath)) return new SaveData();
        string json = File.ReadAllText(SavePath);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public static void Delete() {
        if (File.Exists(SavePath)) File.Delete(SavePath);
    }
}
```

### JsonUtility Limitations & Workarounds

```csharp
// JsonUtility DOESN'T support:
//   - Dictionary<K,V>
//   - polymorphism (derived types)
//   - properties (only public fields)
//   - null arrays become empty arrays

// Workaround for Dictionary: use List<SerializableKVP>
[Serializable]
public class SerializableKVP
{
    public string key;
    public int value;
}

[Serializable]
public class SaveData
{
    public List<SerializableKVP> stats = new();
}

// Or use Newtonsoft.Json (install via Package Manager):
// "com.unity.nuget.newtonsoft-json"
using Newtonsoft.Json;

string json = JsonConvert.SerializeObject(data); // Supports Dictionary, polymorphism
SaveData data = JsonConvert.DeserializeObject<SaveData>(json);
```

## Binary Serialization

```csharp
using System.Runtime.Serialization.Formatters.Binary;

// Note: BinaryFormatter is deprecated in newer .NET for security reasons.
// Use alternatives: protobuf-net, MessagePack, or custom binary writer.

// protobuf-net (recommended for binary):
// Install via NuGet → protobuf-net
using ProtoBuf;

[ProtoContract]
public class SaveData
{
    [ProtoMember(1)] public int level;
    [ProtoMember(2)] public float health;
    [ProtoMember(3)] public string playerName;
}

public static void SaveBinary(SaveData data) {
    using (var stream = File.Create(SavePath + ".bin")) {
        Serializer.Serialize(stream, data);
    }
}

public static SaveData LoadBinary() {
    if (!File.Exists(SavePath + ".bin")) return new SaveData();
    using (var stream = File.OpenRead(SavePath + ".bin")) {
        return Serializer.Deserialize<SaveData>(stream);
    }
}
```

## Encrypted Save

```csharp
using System.Security.Cryptography;
using System.Text;

public static class EncryptedSaveSystem
{
    static string SavePath => Application.persistentDataPath + "/save.enc";
    // 32-byte key (AES-256) — in production, derive from device/user, not hardcoded
    static byte[] Key = Encoding.UTF8.GetBytes("0123456789ABCDEF0123456789ABCDEF");
    static byte[] IV = Encoding.UTF8.GetBytes("ABCDEF0123456789"); // 16 bytes for AES

    public static void Save(SaveData data) {
        string json = JsonUtility.ToJson(data);
        byte[] encrypted = Encrypt(Encoding.UTF8.GetBytes(json));
        File.WriteAllBytes(SavePath, encrypted);
    }

    public static SaveData Load() {
        if (!File.Exists(SavePath)) return new SaveData();
        byte[] encrypted = File.ReadAllBytes(SavePath);
        byte[] decrypted = Decrypt(encrypted);
        string json = Encoding.UTF8.GetString(decrypted);
        return JsonUtility.FromJson<SaveData>(json);
    }

    static byte[] Encrypt(byte[] data) {
        using (Aes aes = Aes.Create()) {
            aes.Key = Key; aes.IV = IV;
            using (var ms = new MemoryStream())
            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write)) {
                cs.Write(data, 0, data.Length);
                cs.FlushFinalBlock();
                return ms.ToArray();
            }
        }
    }

    static byte[] Decrypt(byte[] data) {
        using (Aes aes = Aes.Create()) {
            aes.Key = Key; aes.IV = IV;
            using (var ms = new MemoryStream(data))
            using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
            using (var sr = new StreamReader(cs))
                return Encoding.UTF8.GetBytes(sr.ReadToEnd());
        }
    }
}
```

## Save Version Migration

```csharp
[Serializable]
public class SaveData
{
    public int saveVersion = 1; // Increment on breaking changes
    public int level;
    public float health;
    // v2: added gold
    public int gold;
    // v3: renamed gold → currency
    public int currency;
}

public static SaveData LoadWithMigration() {
    if (!File.Exists(SavePath)) return new SaveData();
    string json = File.ReadAllText(SavePath);
    var data = JsonUtility.FromJson<SaveData>(json);

    switch (data.saveVersion) {
        case 1:
            data.gold = 0; // Add default for new field
            data.saveVersion = 2;
            goto case 2;
        case 2:
            data.currency = data.gold; // Migrate renamed field
            data.gold = 0;
            data.saveVersion = 3;
            break;
    }
    return data;
}
```

## Auto-Save

```csharp
public class AutoSave : MonoBehaviour
{
    [SerializeField] private float _autoSaveInterval = 60f; // seconds
    private float _timer;

    void Update() {
        _timer += Time.deltaTime;
        if (_timer >= _autoSaveInterval) {
            _timer = 0;
            SaveSystem.Save(GetCurrentSaveData());
            Debug.Log("Auto-saved");
        }
    }

    void OnApplicationPause(bool paused) {
        if (paused) SaveSystem.Save(GetCurrentSaveData()); // Save on mobile background
    }

    void OnApplicationQuit() {
        SaveSystem.Save(GetCurrentSaveData());
    }
}
```

## Key Gotchas

1. **Application.persistentDataPath**: Different per platform:
   - Windows: `%userprofile%\AppData\LocalLow\<company>\<product>`
   - Android: `/storage/emulated/0/Android/data/<package>/files`
   - iOS: `<Application>/Documents`
   - WebGL: No filesystem — use PlayerPrefs or IndexedDB via plugin.

2. **JsonUtility** doesn't support Dictionary, nullable types, or polymorphism. Use Newtonsoft.Json for complex data.

3. **BinaryFormatter** is deprecated and has security vulnerabilities. Use protobuf-net or MessagePack instead.

4. **WebGL**: No filesystem. Use `PlayerPrefs` (limited) or `IndexedDB` via JavaScript plugin. Or use server-side cloud save.

5. **Save corruption**: Write to temp file first, then rename (atomic operation):
   ```csharp
   File.WriteAllText(SavePath + ".tmp", json);
   File.Move(SavePath + ".tmp", SavePath); // Atomic on most OS
   ```

6. **Threading**: File I/O on main thread causes hitches. Use `Task.Run` or coroutines for large saves.

7. **Multiple save slots**: Use indexed filenames: `save_0.json`, `save_1.json`, etc.
