using System.Runtime.CompilerServices;

// 编辑器程序集的 internal 成员对 EditMode 测试开放（更新器/菜单工具的可测逻辑）
[assembly: InternalsVisibleTo("Runestone.AesirArchitecture.Tests.Editor")]

// Odin 版窗口与绘制器复用核心编辑器程序集的调试模型与绘制工具
[assembly: InternalsVisibleTo("Runestone.AesirArchitecture.Editor.OdinInspector")]

// RAM 的兜底窗口与 Odin Processor 读取样式开关（AesirInspectorStyleSettings）
[assembly: InternalsVisibleTo("Runestone.AesirModules.Editor")]
[assembly: InternalsVisibleTo("Runestone.AesirModules.Editor.OdinInspector")]
