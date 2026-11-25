using System;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 可选的存档时间戳能力（旁路接口）：存储实现可选实现本接口，为门面
    /// <see cref="SaveModule.GetTimestamp" /> 提供存档最后写入时间。
    /// </summary>
    /// <remarks>
    /// 独立成接口而非并入 <see cref="ISaveStorage" />——自定义存储不实现即能力缺失，门面返回 null，
    /// 不破坏既有实现的编译（开闭原则）。时间戳是尽力而为的只读查询：不支持 / 取不到时返回 null。
    /// </remarks>
    public interface ISaveStorageTimestamp
    {
        /// <summary>
        /// 读取键对应主存档的最后写入时间（UTC）；无存档或取不到时返回 null。
        /// </summary>
        /// <param name="key">存档键（已校验合法）。</param>
        DateTime? GetLastWriteTimeUtc(string key);
    }
}
