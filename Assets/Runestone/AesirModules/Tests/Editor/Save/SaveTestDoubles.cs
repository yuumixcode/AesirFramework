using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Runestone.AesirModules.Tests.Editor.Save
{
    /// <summary>测试数据载体：公开字段 + List + Unity 值类型（JsonUtility 可序列化形态）。</summary>
    [Serializable]
    public class SaveTestPayload
    {
        public int Score;
        public float Health;
        public string Name;
        public List<int> Inventory;
        public Vector3 Position;

        public static SaveTestPayload Sample(int score = 42) => new SaveTestPayload
        {
            Score = score,
            Health = 0.75f,
            Name = "Hero",
            Inventory = new List<int> { 1, 2, 3 },
            Position = new Vector3(1.5f, -2f, 3f)
        };
    }

    /// <summary>轻量单字段 POCO：批次会话测试用（验证不同 POCO 类型混用不串扰）。</summary>
    [Serializable]
    public class BatchValue
    {
        public int value;
    }

    /// <summary>轻量单字段 POCO：批次会话测试用（字符串字段）。</summary>
    [Serializable]
    public class BatchName
    {
        public string text;
    }

    /// <summary>
    /// 测试序列化器：Base64 文本形态（与 unity-json 的原始 JSON 字节可区分），
    /// 用于验证「按存档记录的序列化器 Id 还原」与自定义注册往返。
    /// </summary>
    public class Base64TestSerializer : ISaveSerializer
    {
        public string Id => "test-base64";

        public byte[] Serialize<T>(T value) =>
            Encoding.UTF8.GetBytes(Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonUtility.ToJson(value))));

        public T Deserialize<T>(byte[] data) =>
            JsonUtility.FromJson<T>(Encoding.UTF8.GetString(Convert.FromBase64String(Encoding.UTF8.GetString(data))));
    }

    /// <summary>内存存储替身：验证 SetStorage 替换后文件系统零产出。</summary>
    public class MemoryTestStorage : ISaveStorage
    {
        readonly Dictionary<string, byte[]> _main = new Dictionary<string, byte[]>();
        readonly Dictionary<string, byte[]> _backup = new Dictionary<string, byte[]>();

        public int WriteCount { get; private set; }

        public byte[] Read(string key) => _main.TryGetValue(key, out var data) ? data : null;

        public byte[] ReadBackup(string key) => _backup.TryGetValue(key, out var data) ? data : null;

        public void Write(string key, byte[] data)
        {
            WriteCount++;
            if (_main.TryGetValue(key, out var old))
            {
                _backup[key] = old;
            }

            _main[key] = data;
        }

        public void Delete(string key)
        {
            _main.Remove(key);
            _backup.Remove(key);
        }

        public bool Exists(string key) => _main.ContainsKey(key);

        public IReadOnlyList<string> GetKeys() => new List<string>(_main.Keys);
    }

    /// <summary>记录式管理器替身：验证 SetManager 整体接管。</summary>
    public class RecordingTestManager : ISaveManager
    {
        public int SaveCalls { get; private set; }
        public int LoadCalls { get; private set; }

        public SaveResult Save<T>(string key, T data, int dataVersion)
        {
            SaveCalls++;
            return SaveResult.Success();
        }

        public LoadResult<T> Load<T>(string key, int currentVersion, ISaveMigrator<T> migrator)
        {
            LoadCalls++;
            return LoadResult<T>.NoSave();
        }
    }

    /// <summary>测试迁移器：旧版本 Score 加 100（可断言的确定性升级）。</summary>
    public class AddTenMigrator : ISaveMigrator<SaveTestPayload>
    {
        public SaveTestPayload Migrate(SaveTestPayload data, int fromVersion)
        {
            data.Score += 100;
            return data;
        }
    }
}