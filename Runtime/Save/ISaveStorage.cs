using System.Collections.Generic;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 保存存储契约：字节级持久化（键 → 存档字节）。桥接扩展点——实现本接口并经
    /// <see cref="SaveModule.SetStorage" /> 替换即可接入自定义存储后端（内存、云存储等）。
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>提交式写入契约：</b><see cref="Write" /> 返回后，<see cref="Read" /> 读到的要么是旧的完整数据、
    /// 要么是新的完整数据，不得出现半截数据。实现自选手段：文件系统用「临时文件 + 刷盘 + 原子替换」
    /// （见 <see cref="FileSaveStorage" />），内存存储直接交换引用即可。
    /// </para>
    /// <para>
    /// <b>备份契约：</b><see cref="ReadBackup" /> 返回上一份已提交数据的字节（无备份时返回 null）。
    /// 不支持备份的实现恒返回 null，加载方据此跳过损坏回退。备份的存在是为了让主存档损坏时可回退，
    /// 实现应保证备份本身是完整提交过的数据。
    /// </para>
    /// <para>
    /// 键的合法性由调用方（门面 / 管理器）经内部校验保证，实现可直接信任键为安全文件名；
    /// IO 异常向上抛出，由管理器转为失败结果。
    /// </para>
    /// </remarks>
    public interface ISaveStorage
    {
        /// <summary>读取键对应的主存档字节；不存在时返回 null。</summary>
        /// <param name="key">存档键（已校验合法）。</param>
        byte[] Read(string key);

        /// <summary>读取键对应的备份存档字节；无备份或实现不支持备份时返回 null。</summary>
        /// <param name="key">存档键（已校验合法）。</param>
        byte[] ReadBackup(string key);

        /// <summary>
        /// 提交式写入：返回后读到的要么是旧完整数据、要么是新完整数据（见类 remarks 契约）。
        /// </summary>
        /// <param name="key">存档键（已校验合法）。</param>
        /// <param name="data">待写入字节（非 null）。</param>
        void Write(string key, byte[] data);

        /// <summary>删除键对应的主存档与备份；键不存在时无副作用。</summary>
        /// <param name="key">存档键（已校验合法）。</param>
        void Delete(string key);

        /// <summary>键对应的主存档是否存在。</summary>
        /// <param name="key">存档键（已校验合法）。</param>
        bool Exists(string key);

        /// <summary>枚举全部已提交存档的键（不含临时文件与备份）。</summary>
        IReadOnlyList<string> GetKeys();
    }
}