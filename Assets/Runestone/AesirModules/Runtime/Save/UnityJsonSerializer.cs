using System.Text;
using UnityEngine;

namespace Runestone.AesirModules
{
    /// <summary>
    /// Unity 自带 JsonUtility 序列化器（默认，零第三方依赖）。
    /// </summary>
    /// <remarks>
    /// JsonUtility 固有局限（选择序列化器时须知）：不支持 <c>Dictionary&lt;K,V&gt;</c>、多态（派生类型信息丢失）、
    /// 属性（只序列化公开字段与 [SerializeField] 字段）；null 集合反序列化为空集合。
    /// 需要这些形状时改用 Odin 序列化器（<c>OdinSaveSerializer</c>，ODIN_INSPECTOR 可用时）
    /// 或 Newtonsoft 序列化器（安装 com.unity.nuget.newtonsoft-json 时）。
    /// </remarks>
    public class UnityJsonSerializer : ISaveSerializer
    {
        /// <inheritdoc />
        public string Id => "unity-json";

        /// <inheritdoc />
        public byte[] Serialize<T>(T value) => Encoding.UTF8.GetBytes(JsonUtility.ToJson(value));

        /// <inheritdoc />
        public T Deserialize<T>(byte[] data) => JsonUtility.FromJson<T>(Encoding.UTF8.GetString(data));
    }
}