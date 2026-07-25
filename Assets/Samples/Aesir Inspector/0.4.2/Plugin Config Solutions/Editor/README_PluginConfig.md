# Unity 插件配置解决方案

## 背景

Unity 插件或者实际项目通常会需要设计一些配置类，根据配置类的需求，选择不同的解决方案。目前列举五种场景的解决方案。

## 选择逻辑

```markdown
1.是否需要跨项目调用？
2.是否需要便于查看和调试？
3.是否需要 Editor 程序集调用？
4.是否需要 Runtime 程序集可以调用？
5.是否需要构建后的程序运行时调用？

---
1,2,3 选择方案一
1,3 选择方案二
2,3 选择方案三
2,3,4 选择方案四
2,3,4,5 选择方案五
```

## 参考链接

### ScriptableSingleton

https://docs.unity3d.com/2022.3/Documentation/ScriptReference/ScriptableSingleton_1.html

### EditorPrefs

https://docs.unity3d.com/2022.3/Documentation/ScriptReference/EditorPrefs.html

### EditorBuildSettings

https://docs.unity3d.com/ScriptReference/EditorBuildSettings.html

### PlayerSettings.SetPreloadedAssets

https://docs.unity3d.com/ScriptReference/PlayerSettings.SetPreloadedAssets.html

## 一、支持跨项目调用，Unity 编辑器内方便查看，仅 Editor 程序集调用

使用 `ScriptableSingleton`，配置类继承自 `ScriptableSingleton<T>`，同时添加 `[FilePath]` 属性来指定配置文件的存储路径。

其中 `Location` 枚举设置为 `FilePathAttribute.Location.PreferencesFolder`。此时代表的根路径是在 `Tuanjie/Editor-5.x/` 目录下。

本质是一个  `ScriptableObject` ，内部有一个静态单例可以直接获取到实例对象，便于在 Unity 编辑器内部显示。

注意：每次修改值，需要调用 `Save()` 写入磁盘，推荐使用 `Save(true)`，表示以文本格式存储。

Sample 的案例路径如下：

```markdown
~/Library/Preferences/Tuanjie/Editor-5.x/PreferencesResources/ScriptableSingletonInPreferencesSample.asset
```

## 二、支持跨项目调用，不需要在 Unity 编辑器中直接查看和调试，仅 Editor 程序集调用

使用 `EditorPrefs`。以基础类型键值对的方式存储，二进制。

需要针对每个值做处理，才能便于查看和调试。

针对不同的操作系统，具有不同的存放路径。具体路径参考 EditorPrefs 官方文档。

## 三、不需要跨项目调用，Unity 编辑器内方便查看，仅 Editor 程序集调用

使用 `ScriptableSingleton`，配置类继承自 `ScriptableSingleton<T>`，同时添加 `[FilePath]` 属性来指定配置文件的存储路径。

其中 `Location` 枚举值为 `FilePathAttribute.Location.ProjectFolder`，此时代表的根路径和 `Assets/` 同级别目录。

注意：每次修改值，需要调用 `Save()` 写入磁盘，推荐使用 `Save(true)`，表示以文本格式存储。

Sample 的案例路径如下：

```markdown
// 和 Assets/ 是同级别目录
ProjectEditorResources/Samples/PluginConfigSolutions/ScriptableSingletonInProjectSample.asset
```

## 四、不需要跨项目调用，需要 Runtime 程序集和 Editor 程序集可以调用，便于查看和调试，不需要构建后调用

在 Runtime 程序集中声明配置类，继承 `ScriptableObject`。

使用公共静态单例，获取单例时检测项目中是否包含实例对象，如果不存在，则动态生成资产到本地磁盘，并且注册到  `EditorBuildSettings` ，推荐使用 `Assets/Editor Default Resources/` 路径存放，该路径是 Unity 的特殊文件夹路径，只在编辑器阶段存在，所以一定不会打包进构建后的程序。

`EditorBuildSettings` 可以存储资产对象的引用，移动资产也不会出现错误。

## 五、不需要跨项目调用，需要 Runtime 程序集和 Editor 程序集可以调用，便于查看和调试，需要构建后调用

在 Runtime 程序集中声明配置类，继承 `ScriptableObject`。

使用公共静态单例，获取单例时检测项目中是否包含实例对象，如果不存在，则动态生成资产到本地磁盘，并且注册到  `EditorBuildSettings` ，同时使用 `PlayerSettings.SetPreloadedAssets();` 设置为预加载资源，资产推荐路径为 `Assets/Settings/`。这样在游戏场景加载前，把配置类对象加载到内存中，保证其他类调用时不报错。

或者选择手动在游戏启动场景加载。

`EditorBuildSettings` 可以存储资产对象的引用，移动资产也不会出现错误。

官方文档源码案例：

```csharp
using System.Linq;
using UnityEngine;

// We use this class to store general config data that can be used in the player
public class ConfigObject : ScriptableObject
{
    public string text;

    public static ConfigObject configInstance;

    #if UNITY_EDITOR
    [UnityEditor.MenuItem("Assets/Create/Config Object")]
    public static void CreateAsset()
    {
        var path = UnityEditor.EditorUtility.SaveFilePanelInProject("Save Config", "config", "asset", string.Empty);
        if (string.IsNullOrEmpty(path))
            return;

        var configObject = CreateInstance<ConfigObject>();
        UnityEditor.AssetDatabase.CreateAsset(configObject, path);

        // Add the config asset to the build
        var preloadedAssets = UnityEditor.PlayerSettings.GetPreloadedAssets().ToList();
        preloadedAssets.Add(configObject);
        UnityEditor.PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
    }
    #endif

    void OnEnable()
    {
        configInstance = this;
    }
}
```
