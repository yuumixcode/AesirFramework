using Sirenix.Serialization;

namespace Runestone.AesirModules
{
    /// <summary>
    /// Odin 二进制序列化器（可选集成，ODIN_INSPECTOR 可用时经 asmref 汇入 Odin 程序集）。
    /// 相比默认 UnityJsonSerializer 额外支持 <c>Dictionary&lt;K,V&gt;</c>、多态（派生类型信息保留）与私有字段。
    /// </summary>
    /// <remarks>
    /// 注册方式：<c>SaveModule.RegisterSerializer(new OdinSaveSerializer())</c>，
    /// 再经配置 / ApplyConfig 将 <c>defaultSerializerId</c> 设为 <c>"odin"</c>。
    /// 兼容性提示：Sirenix 二进制格式随 Odin 大版本可能变化，使用本轨的项目建议钉住 Odin 版本升级节奏；
    /// 信封记录的序列化器 Id 保证版本不兼容时明确失败而非静默错读。
    /// </remarks>
    public class OdinSaveSerializer : ISaveSerializer
    {
        /// <inheritdoc />
        public string Id => "odin";

        /// <inheritdoc />
        public byte[] Serialize<T>(T value) => SerializationUtility.SerializeValue(value, DataFormat.Binary);

        /// <inheritdoc />
        public T Deserialize<T>(byte[] data) => SerializationUtility.DeserializeValue<T>(data, DataFormat.Binary);
    }
}