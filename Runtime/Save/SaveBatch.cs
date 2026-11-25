using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 批量保存会话：把多个命名条目打包成<b>一个</b>存档批次，整份作为一次事务提交。
    /// <para>由 <see cref="SaveModule.SaveBatch" /> 创建并经回调登记条目，或由 <see cref="SaveModule.LoadBatch" /> 加载还原；
    /// 用户不直接构造。批次本身是可序列化载荷，直接交给默认 <see cref="ISaveManager" /> 走完整保存 / 加载链
    /// （序列化 → 变换链 → 信封 → 事务写 / 校验 → 还原 → 版本迁移 → 备份回退），因此原子边界就是这一次事务写。</para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// 每个条目记录写入时所用序列化器的 <see cref="ISaveSerializer.Id" />，加载时按各自 Id 还原，混用类型 / 序列化器不串扰。
    /// 条目的序列化字节以 Base64 文本存入载荷——JsonUtility 对 string 字段的往返稳定，避免二进制字节在文本序列化中的编码风险。
    /// </para>
    /// <para>
    /// 注册表引用与默认序列化器 Id 是运行时依赖（<c>[NonSerialized]</c>，不进存档），经 internal <see cref="Bind" />
    /// 在门面创建 / 加载后注入；未绑定时 <see cref="Set{T}" /> / <see cref="Get{T}" /> 抛异常或返回默认值。
    /// </para>
    /// </remarks>
    [Serializable]
    public class SaveBatch
    {
        /// <summary>批次内单个条目：名称 + 写入时序列化器 Id + Base64 序列化字节。</summary>
        [Serializable]
        internal sealed class BatchEntry
        {
            public string name;
            public string serializerId;
            public string payload;
        }

        [SerializeField]
        List<BatchEntry> _entries = new List<BatchEntry>();

        [NonSerialized]
        IReadOnlyDictionary<string, ISaveSerializer> _serializers;

        [NonSerialized]
        string _defaultSerializerId;

        // JsonUtility.FromJson 对普通 [Serializable] 类不保证字段初始化器生效，访问处统一走此属性兜底。
        List<BatchEntry> Entries => _entries ?? (_entries = new List<BatchEntry>());

        /// <summary>批次内条目数。</summary>
        public int Count => Entries.Count;

        /// <summary>批次是否含指定名称的条目。</summary>
        public bool Contains(string name) => Find(name) != null;

        /// <summary>
        /// 登记 / 覆盖一个条目：用当前默认序列化器序列化数据，记录其 Id 与 Base64 字节。
        /// </summary>
        /// <typeparam name="T">数据类型（纯数据对象）。</typeparam>
        /// <param name="name">条目名（非空，允许任意字符——它不是文件名，不受存档键白名单约束）。</param>
        /// <param name="data">条目数据（非 null）。</param>
        /// <returns>本批次实例（支持链式登记）。</returns>
        public SaveBatch Set<T>(string name, T data)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("批次条目名不可为空", nameof(name));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), "批次条目数据不可为空");
            }

            EnsureBound();
            if (!_serializers.TryGetValue(_defaultSerializerId, out var serializer))
            {
                throw new InvalidOperationException(
                    $"默认序列化器 '{_defaultSerializerId}' 未注册，无法写入批次；" +
                    "请先 SaveModule.RegisterSerializer 注册或改用已注册的序列化器");
            }

            var entry = new BatchEntry
            {
                name = name,
                serializerId = serializer.Id,
                payload = Convert.ToBase64String(serializer.Serialize(data))
            };

            var existing = Find(name);
            if (existing != null)
            {
                Entries.Remove(existing);
            }

            Entries.Add(entry);
            return this;
        }

        /// <summary>
        /// 读取一个条目：按条目记录的序列化器 Id 还原。条目不存在、序列化器未注册或还原失败时返回
        /// <c>default(T)</c>（与单条 <see cref="SaveModule.Load{T}" /> 的 NoSave / Corrupted 同语义，不抛异常）。
        /// </summary>
        public T Get<T>(string name)
        {
            var entry = Find(name);
            if (entry == null)
            {
                return default;
            }

            EnsureBound();
            if (!_serializers.TryGetValue(entry.serializerId, out var serializer))
            {
                return default;
            }

            try
            {
                return serializer.Deserialize<T>(Convert.FromBase64String(entry.payload));
            }
            catch (Exception e) when (e is FormatException || e is ArgumentException ||
                                      e is System.Runtime.Serialization.SerializationException)
            {
                return default;
            }
        }

        /// <summary>由门面注入序列化器注册表与默认序列化器 Id（不进存档，加载后需重新绑定）。</summary>
        internal void Bind(IReadOnlyDictionary<string, ISaveSerializer> serializers, string defaultSerializerId)
        {
            _serializers = serializers ?? throw new ArgumentNullException(nameof(serializers));
            if (string.IsNullOrEmpty(defaultSerializerId))
            {
                throw new ArgumentException("默认序列化器 Id 不可为空", nameof(defaultSerializerId));
            }

            _defaultSerializerId = defaultSerializerId;
        }

        BatchEntry Find(string name)
        {
            var entries = Entries;
            for (var i = 0; i < entries.Count; i++)
            {
                if (entries[i].name == name)
                {
                    return entries[i];
                }
            }

            return null;
        }

        void EnsureBound()
        {
            if (_serializers == null || string.IsNullOrEmpty(_defaultSerializerId))
            {
                throw new InvalidOperationException(
                    "批次未绑定序列化器注册表：请通过 SaveModule.SaveBatch / LoadBatch 获取批次实例，不要自行 new SaveBatch");
            }
        }
    }
}
