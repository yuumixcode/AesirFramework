using System;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// Aesir 编辑器界面样式开关：在已安装 Odin Inspector 时，仍可选择改用 UI Toolkit 复刻样式。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     本开关只控制 <b>Aesir 自己的 Odin 集成是否生效</b>，与 Odin 自带的
    ///     "Disable Odin Inspector" 全局开关相互独立：Odin 全局开关会关掉所有 Odin 绘制，
    ///     而本开关让用户保留 Odin 用于其它插件、唯独让 Aesir 的面板/属性走 UI Toolkit 复刻版。
    ///     </para>
    ///     <para>
    ///     判据为 <see cref="UseOdin" />：为 true 时 Odin 程序集的 AttributeProcessor 注入特性、
    ///     窗口 Opener 注册 Odin 版；为 false 时这些注册点整体让路，菜单落回 UI Toolkit / 原生窗口。
    ///     值由 <see cref="AesirInspectorStyleSettingsSO" /> 持有（Editor 专属 ScriptableObject），
    ///     在 <c>Preferences → Aesir</c> 页切换。
    ///     </para>
    ///     <para>
    ///     本类零 Odin 依赖，放在核心编辑器程序集，RAA / RAM 的 Odin 程序集均可引用。
    ///     </para>
    /// </remarks>
    internal static class AesirInspectorStyleSettings
    {
        /// <summary>
        /// 是否使用 Odin 样式。为 false 时 Aesir 的 Odin 集成整体让路，
        /// 界面改用 UI Toolkit 复刻版。
        /// </summary>
        internal static bool UseOdin
        {
            get => AesirInspectorStyleSettingsSO.instance.UseOdin;
            set
            {
                if (AesirInspectorStyleSettingsSO.instance.UseOdin == value)
                {
                    return;
                }

                AesirInspectorStyleSettingsSO.instance.UseOdin = value;
                Changed?.Invoke();
            }
        }

        /// <summary>样式切换（已打开的窗口 / Preferences 页据此同步）。</summary>
        internal static event Action Changed;
    }
}
