using System.Runtime.CompilerServices;

// 允许 Editor.OdinInspector 程序集访问 internal 成员，供 Odin AttributeProcessor 使用 nameof。
[assembly: InternalsVisibleTo("Runestone.AesirModules.Editor.OdinInspector")]

// 允许测试程序集访问 internal 成员（与 AesirArchitecture 包的 AssemblyInfo 同款先例）。
[assembly: InternalsVisibleTo("Runestone.AesirModules.Tests")]
