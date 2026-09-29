using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Runestone.AesirModules.Tests.Editor")]

// Odin 门控的测试程序集（Binder / ScriptDocGenerator / UI 窗口面板套件）需要直接访问编辑器侧 internal 成员。
[assembly: InternalsVisibleTo("Runestone.AesirModules.Tests.Editor.OdinInspector")]
