using System.Text;
using Newtonsoft.Json;

namespace Runestone.AesirModules
{
    /// <summary>
    /// Newtonsoft JSON 序列化器（可选集成：工程安装 com.unity.nuget.newtonsoft-json 时本程序集才参与编译）。
    /// 相比默认 UnityJsonSerializer 额外支持 <c>Dictionary&lt;K,V&gt;</c>、多态（TypeNameHandling 按需）与属性。
    /// </summary>
    /// <remarks>
    /// 注册方式：<c>SaveModule.RegisterSerializer(new NewtonsoftJsonSerializer())</c>，
    /// 再经配置 / ApplyConfig 将 <c>defaultSerializerId</c> 设为 <c>"newtonsoft"</c>。
    /// 默认不启用 TypeNameHandling（安全：反序列化类型名注入是已知攻击面）；
    /// 需要多态时自行构造带 TypeNameHandling.Auto 的 JsonSerializerSettings 子类化本类。
    /// </remarks>
    public class NewtonsoftJsonSerializer : ISaveSerializer
    {
        /// <inheritdoc />
        public string Id => "newtonsoft";

        /// <inheritdoc />
        public byte[] Serialize<T>(T value) => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value));

        /// <inheritdoc />
        public T Deserialize<T>(byte[] data) =>
            JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(data));
    }
}