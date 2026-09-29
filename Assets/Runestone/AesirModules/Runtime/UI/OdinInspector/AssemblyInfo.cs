using System.Runtime.CompilerServices;

// Binder 的内部工具类型（BinderHierarchyUtility 等）由 Odin 门控的测试程序集
// Runestone.AesirModules.Tests.Editor.OdinInspector 直接调用。
// 同类声明见 Editor/UI/Binder/BinderCodeGenerator.cs（按类型所在程序集分别声明）。
[assembly: InternalsVisibleTo("Runestone.AesirModules.Tests.Editor.OdinInspector")]
