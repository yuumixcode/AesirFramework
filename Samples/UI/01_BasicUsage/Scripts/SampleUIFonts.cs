#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
using UnityEngine;
using UnityEngine.UI;

namespace Runestone.AesirModules.Samples.UI.BasicUsage
{
    /// <summary>
    /// 跨平台中文动态字体工具：探测系统字体列表生成动态字体并应用到子树全部 Text，
    /// 预制体内静态保存的 Text 引用（默认字体）对中文字形不可靠，统一在运行时挂字体。
    /// </summary>
    public static class SampleUIFonts
    {
        static Font _cached;

        /// <summary>为指定根物体子树内全部 Text 应用系统中文动态字体。</summary>
        public static void Apply(Transform root)
        {
            if (_cached == null)
            {
                _cached = CreateChineseFont();
            }

            var texts = root.GetComponentsInChildren<Text>(true);
            foreach (var text in texts)
            {
                text.font = _cached;
            }
        }

        /// <summary>跨平台中文动态字体：候选探测顺序 Windows → macOS → Linux，全部不可用时回退内置字体。</summary>
        static Font CreateChineseFont()
        {
            var candidates = new[] { "Microsoft YaHei", "PingFang SC", "Noto Sans CJK SC", "WenQuanYi Zen Hei" };
            var installedFonts = Font.GetOSInstalledFontNames();
            foreach (var candidate in candidates)
            {
                foreach (var installedName in installedFonts)
                {
                    if (installedName == candidate)
                    {
                        return Font.CreateDynamicFontFromOSFont(installedName, 22);
                    }
                }
            }

            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
    }
}
#endif
