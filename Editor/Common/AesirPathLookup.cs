using UnityEngine;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// Aesir 安装位置锚点资产 — 空壳标记资产，每包包根各放一份（<c>AesirPathLookup.asset</c>）。
    /// <para>
    /// 机制参照 Odin Inspector 的 SirenixPathLookupScriptableObject（OdinPathLookup.asset）：
    /// 资产 .meta 里的 GUID 在文件夹移动后保持不变，路径定位器 <see cref="AesirAssetPaths" />
    /// 经 GUID 查询拿到资产实际路径、再反推包的安装位置——由此 Assets 形态安装的 Runestone
    /// 目录可自由移动到项目任意文件夹，包内更新器、Getting Started 窗口与示例场景的构建剔除照常工作。
    /// </para>
    /// <para>勿删除本资产；其 Inspector 由 <see cref="AesirPathLookupAssetEditor" /> 绘制说明。</para>
    /// </summary>
    public class AesirPathLookup : ScriptableObject { }
}
