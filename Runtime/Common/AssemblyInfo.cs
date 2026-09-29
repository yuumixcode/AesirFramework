using System.Runtime.CompilerServices;

// 允许 Editor.OdinInspector 程序集访问 internal 成员，供 Odin AttributeProcessor 使用 nameof。
[assembly: InternalsVisibleTo("Runestone.AesirModules.Editor.OdinInspector")]

// 允许测试程序集访问 internal 成员（与 AesirArchitecture 包的 AssemblyInfo 同款先例）；
// Odin 门控测试程序集（UI 窗口/面板套件）同享该可见性。
[assembly: InternalsVisibleTo("Runestone.AesirModules.Tests.Editor")]
[assembly: InternalsVisibleTo("Runestone.AesirModules.Tests.Editor.OdinInspector")]

// UniTask 驱动链的 PlayMode 测试程序集（仅在装了 UniTask 的工程编译）需读取内部进度上限做归一化断言。
[assembly: InternalsVisibleTo("Runestone.AesirModules.Tests.UniTask")]
