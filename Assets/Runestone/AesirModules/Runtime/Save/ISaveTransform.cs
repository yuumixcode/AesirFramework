namespace Runestone.AesirModules
{
    /// <summary>
    /// 保存字节变换契约：字节 → 字节（压缩、加密等）。装饰器扩展点——经
    /// <see cref="SaveModule.SetTransformChain" /> 按序注册，保存时按注册顺序施加、加载时逆序还原。
    /// </summary>
    /// <remarks>
    /// <see cref="Id" /> 是变换的稳定标识，按施加顺序写入存档信封；加载按信封记录查注册表逆序还原，
    /// 因此 Id 一经发布不得更改，且加载时链上每个 Id 都必须已注册（否则该存档判为损坏）。
    /// 内置 Id：<c>"gzip"</c>（<see cref="GZipSaveTransform" />）、<c>"aes"</c>（<see cref="AesSaveTransform" />）。
    /// 变换必须是纯函数（同输入同输出，加密的随机 IV 等 nonce 须随输出字节自带），不得依赖场景状态。
    /// </remarks>
    public interface ISaveTransform
    {
        /// <summary>变换稳定标识（写入存档信封，加载按此还原；非空，发布后不可更改）。</summary>
        string Id { get; }

        /// <summary>施加变换（保存方向）。</summary>
        /// <param name="data">输入字节。</param>
        /// <returns>变换后字节。</returns>
        byte[] Encode(byte[] data);

        /// <summary>还原变换（加载方向，与 <see cref="Encode" /> 互逆）。</summary>
        /// <param name="data">变换后字节。</param>
        /// <returns>还原字节。</returns>
        byte[] Decode(byte[] data);
    }
}