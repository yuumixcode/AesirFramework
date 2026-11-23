namespace Runestone.AesirModules
{
    /// <summary>
    /// 保存序列化器契约：数据对象 ↔ 字节。策略扩展点——实现本接口并经
    /// <see cref="SaveModule.RegisterSerializer" /> 注册即可接入自定义序列化格式。
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>契约：</b>实现必须保证 <c>Deserialize&lt;T&gt;(Serialize&lt;T&gt;(value))</c> 与 <c>value</c> 逐字段相等
    /// （往返一致）；序列化输出是纯字节，不得依赖 Unity 主线程或场景状态（以便未来后台线程复用）。
    /// </para>
    /// <para>
    /// <see cref="Id" /> 是序列化器的稳定标识，会写入每份存档的信封头；加载按存档记录的 Id 查注册表还原，
    /// 因此 Id 一经发布不得更改（改名等于旧档不可读）。内置 Id：<c>"unity-json"</c>（默认）、
    /// <c>"odin"</c>（ODIN_INSPECTOR 可用时）、<c>"newtonsoft"</c>（安装 com.unity.nuget.newtonsoft-json 时）。
    /// </para>
    /// </remarks>
    public interface ISaveSerializer
    {
        /// <summary>序列化器稳定标识（写入存档信封，加载按此还原；非空、不含空白，发布后不可更改）。</summary>
        string Id { get; }

        /// <summary>将数据对象序列化为字节。</summary>
        /// <typeparam name="T">数据类型（纯数据对象，不支持 Unity 对象引用）。</typeparam>
        /// <param name="data">待序列化数据（非 null，由调用方保证）。</param>
        /// <returns>序列化字节。</returns>
        byte[] Serialize<T>(T value);

        /// <summary>将字节还原为数据对象。</summary>
        /// <typeparam name="T">数据类型，须与序列化时一致。</typeparam>
        /// <param name="data">序列化字节。</param>
        /// <returns>还原的数据对象。</returns>
        T Deserialize<T>(byte[] data);
    }
}