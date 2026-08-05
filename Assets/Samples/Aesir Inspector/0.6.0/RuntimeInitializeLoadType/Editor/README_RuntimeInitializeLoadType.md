# RuntimeInitializeLoadType：五个初始化时机的执行顺序与最佳实践

## 五个时机执行顺序

SubsystemRegistration → AfterAssembliesLoaded → BeforeSplashScreen → BeforeSceneLoad →
（场景加载、Awake、OnEnable） → AfterSceneLoad

## 执行顺序的完整时间线

为了更清晰地理解整个初始化流程，让我们看看一个完整的启动时间线：

```plaintext
1. 应用程序启动
2. 底层系统初始化
3. [SubsystemRegistration] 回调执行
4. [AfterAssembliesLoaded] 回调执行
5. [BeforeSplashScreen] 回调执行
6. Unity 启动画面显示(如果启用)
7. 第一个场景加载完成，但是 Awake 还没有执行，所有物体对象都认为是未激活
8. [BeforeSceneLoad] 回调执行
9. 场景中所有 MonoBehaviour 的 Awake() 执行
10. 场景中所有 MonoBehaviour 的 OnEnable() 执行
11. [AfterSceneLoad] 回调执行
12. 场景中所有 MonoBehaviour 的 Start() 执行
13. 游戏主循环开始(Update、FixedUpdate 等)
```

官方文档原文: https://docs.unity3d.com/2022.3/Documentation/ScriptReference/RuntimeInitializeOnLoadMethodAttribute.html

```plaintext
First various low level systems are initialized (window, assemblies, gfx etc.)
Then SubsystemRegistration and AfterAssembliesLoaded callbacks are invoked.
More setup (input systems etc.)
Then BeforeSplashScreen callback is invoked.
Now the first scene starts loading.
Then BeforeSceneLoad callback is invoked. Here objects of the scene is loaded but Awake() has not been called yet. All objects are considered inactive here.
Now Awake() and OnEnable() are invoked on MonoBehaviours.
Then AfterSceneLoad callback is invoked. Here objects of the scene are considered fully loaded and setup. Active objects can be found with FindObjectsByType.
```

## 各时机的详细分析与使用场景

### 1. SubsystemRegistration - 子系统注册阶段

**执行时机**：在 Unity 底层系统（窗口、程序集、图形等）初始化完成后，SubsystemRegistration 和 AfterAssembliesLoaded
会被调用，但 SubsystemRegistration 的执行顺序在 AfterAssembliesLoaded 之前。

**特点**：

- 执行时机最早，此时 Unity 引擎的大部分系统尚未完全初始化

- 主要用于注册底层子系统和重置静态变量

- 在 Editor 模式下，每次进入 Play 模式都会触发，这对于重置静态状态非常重要

**适用场景**：

- **静态变量重置**：在 Editor 模式下，静态变量在退出 Play 模式后不会自动重置。使用 SubsystemRegistration 可以确保每次进入
  Play 模式时，静态变量都被正确初始化

- **底层子系统注册**：注册自定义的渲染管线、物理系统扩展等底层子系统

- **全局状态清理**：清理上一次运行遗留的全局状态

对于运行时脚本，你必须使用属性 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] 来重置静态字段和事件处理程序。

对于编辑器脚本，如自定义编辑器窗口或 Inspectors 使用静态代码的脚本，必须使用 [InitializeOnEnterPlayMode] 属性来重置静态字段和事件处理程序。

官方文档原文：https://docs.unity3d.com/2022.3/Documentation/Manual/DomainReloading.html

```plaintext
For runtime scripts, you must use the [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] attribute to reset static fields and event handlers.
For Editor scripts such as custom Editor windows or Inspectors that use statics, you must use the [InitializeOnEnterPlayMode] attribute to reset static fields and event handlers.
```

### 2. AfterAssembliesLoaded - 程序集加载完成

**执行时机**：所有程序集和预加载资源初始化完成后触发。

**特点**：

- 此时所有的 C# 代码已经加载完成，可以安全地访问所有类型和静态成员

- 场景尚未开始加载，没有任何场景对象存在

- 适合执行不依赖场景的纯逻辑初始化

**适用场景**：

- **依赖注入容器初始化**：配置 IoC 容器，注册服务和依赖关系

- **配置文件加载**：从 Resources 或 StreamingAssets 加载配置文件

- **第三方 SDK 初始化**：初始化分析 SDK、广告 SDK 等不依赖场景的第三方服务

- **事件系统注册**：注册全局事件总线和消息系统

### 3. BeforeSplashScreen - 启动画面显示前

**执行时机**：在 Unity 启动画面（Splash Screen）显示之前触发。

**特点**：

- 这是在用户看到任何视觉反馈之前的最后一个时机

- 可以用于预加载关键资源，避免启动后的卡顿

- 执行时间应该尽可能短，避免延长启动时间

**适用场景**：

- **关键资源预加载**：预加载启动画面后立即需要的资源

- **启动参数解析**：解析命令行参数或深度链接参数

- **许可证验证**：执行软件许可证或 DRM 验证

- **启动日志记录**：记录应用启动的时间戳和环境信息

**性能考虑**：这个阶段的代码会直接影响启动时间，应该只执行最必要的操作，避免耗时的同步加载。

### 4. BeforeSceneLoad - 场景加载前

**执行时机**：第一个场景开始加载，但场景对象尚未实例化，Awake 尚未被调用。

**特点**：

- 场景文件已经开始加载到内存，但所有对象都处于未激活状态

- 此时可以创建在 Awake 之前就需要存在的对象

- 适合创建持久化的管理器对象

**适用场景**：

- **DontDestroyOnLoad 对象创建**：创建跨场景持久化的游戏对象

- **场景加载监听器注册**：注册 SceneManager.sceneLoaded 事件监听器

- **全局单例 GameObject 创建**：创建需要在所有 MonoBehaviour 的 Awake 之前就存在的单例管理器，比如音频管理器、网络管理器等全局对象

**重要提示**：在 BeforeSceneLoad 中通过 `new GameObject()` 创建的对象，其 Awake 和 OnEnable
方法会立即被调用，这发生在场景中其他对象的 Awake 之前。这是一个非常有用的特性，可以确保你的管理器在其他脚本需要它们之前就已经准备好了。

### 5. AfterSceneLoad - 场景加载后

**执行时机**：第一个场景加载完成，所有场景对象的 Awake 和 OnEnable 已经执行完毕，但 Start 尚未被调用。

**特点**：

- 这是默认的执行时机（不指定参数时使用）

- 场景中的所有对象都已经初始化完成，可以安全地访问场景对象

- 在所有 Start 方法之前执行

**适用场景**：

- **场景对象引用获取**：查找和缓存场景中的关键对象引用

- **游戏状态初始化**：根据场景内容初始化游戏状态

- **UI 系统初始化**：初始化 UI 管理器并建立与场景 UI 的连接

- **游戏逻辑启动**：启动游戏主循环、关卡逻辑等

**与 Start 的关系**： AfterSceneLoad 在所有 Awake 和 OnEnable 之后、所有 Start 之前执行。如果你需要在 Start
方法中访问某些全局状态，可以在 AfterSceneLoad 中初始化这些状态。

## 常见陷阱与注意事项

### 1. 执行顺序的不确定性

在同一个时机的不同方法，它们的执行顺序是不确定的。

**解决方案**：如果存在依赖关系，要么在同一个方法中按顺序执行，要么使用不同的 RuntimeInitializeLoadType 时机。

### 2. 仅在第一个场景触发

所有的 RuntimeInitializeOnLoadMethod 回调只在应用程序启动时、第一个场景加载时触发一次。后续的场景切换不会再次触发这些回调。如果需要在每次场景加载时执行逻辑，应该使用
`SceneManager.sceneLoaded` 事件。

### 3. Editor 模式的特殊行为

在 Unity Editor 中，每次进入 Play 模式时，所有的 RuntimeInitializeOnLoadMethod 都会重新执行。这对于
SubsystemRegistration 尤其重要，因为它可以用来重置静态变量。

但是，如果你在 Editor 中打开了多个场景，进入 Play 模式时的行为可能与预期不同。BeforeSceneLoad 和 AfterSceneLoad
只会针对第一个加载的场景触发。

### 4. 不要在早期时机访问场景对象

在 SubsystemRegistration、AfterAssembliesLoaded 和 BeforeSplashScreen 时机，场景尚未加载，不要尝试访问场景对象或使用
FindObjectOfType 等方法。

### 5. 性能影响

虽然 RuntimeInitializeOnLoadMethod 很方便，但不要滥用。每个标记的方法都会增加启动时间。特别是在
BeforeSplashScreen 时机，耗时操作会直接延长用户等待时间。

## Enter Play Mode 设置的影响

Unity 2019.3 引入了 Configurable Enter Play Mode 功能，允许开发者配置进入 Play 模式时的行为，以加快迭代速度。这个设置对
RuntimeInitializeOnLoadMethod 的执行有重要影响，理解这些影响对于正确使用初始化时机至关重要。

### Enter Play Mode Options 配置

在 **Edit > Project Settings > Editor** 中，你可以找到 Enter Play Mode Settings。启用后，会看到两个选项：

- **Reload Domain**（域重载）：是否重新加载脚本域，重置所有静态变量和类型信息

- **Reload Scene**（场景重载）：是否重新加载场景，销毁并重建所有场景对象

Enter Play Mode 设置对于五个时机没有影响，无论哪种情况，五个时机的方法都会执行，可以使用 RuntimeInitializeLoadTypeSettings 快捷修改配置进行测试。

### 常见问题排查

**问题 1**：静态变量保持上次的值

```csharp
// 原因：Domain Reload 禁用时，静态变量不会自动重置
private static int counter = 0; // 不会重置

// 解决方案：在 SubsystemRegistration 中手动重置
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
private static void ResetCounter()
{
    counter = 0;
}
```

**问题 2**：事件处理器重复注册

```csharp
// 原因：Domain Reload 禁用时，之前注册的处理器仍然存在
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
private static void RegisterHandler()
{
    Application.quitting += OnQuit; // 每次都会添加新的处理器
}

// 解决方案：先取消注册再注册
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
private static void ResetHandler()
{
    Application.quitting -= OnQuit; // 先移除
}

[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
private static void RegisterHandler()
{
    Application.quitting += OnQuit; // 再添加
}
```

## RuntimeInitializeOnLoadMethod vs InitializeOnLoad(Editor)

`InitializeOnLoad` 是 Editor 专用的特性，在编辑器启动时或脚本重新编译后执行，而 `RuntimeInitializeOnLoadMethod`
在运行时（包括打包后的游戏）执行。两者可以配合使用，实现编辑器和运行时的完整初始化覆盖。

## 最佳实践总结

1. **选择正确的时机**：根据初始化逻辑的依赖关系，选择最合适的 RuntimeInitializeLoadType

2. **避免依赖同时机内的执行顺序**：同一时机内的多个方法执行顺序不确定

3. **使用静态方法**： RuntimeInitializeOnLoadMethod 只能标记静态方法

4. **控制执行时间**：特别是 BeforeSplashScreen，避免耗时操作影响启动体验

5. **合理组织代码**：将相关的初始化逻辑组织在一起，便于维护和调试

6. **添加日志**：在每个初始化方法中添加日志，便于追踪初始化流程

7. **处理异常**：添加适当的异常处理，避免初始化失败导致游戏无法启动

8. **考虑 Editor 模式**：利用 SubsystemRegistration 重置静态状态，确保 Editor 中的 Play 模式行为一致

## 结语

RuntimeInitializeOnLoadMethod 是 Unity
提供的强大工具，它让我们能够在游戏启动的不同阶段精确地执行初始化逻辑。通过深入理解这五个时机的执行顺序和适用场景，我们可以构建更加健壮、模块化的游戏架构，避免对特定启动场景的依赖，提高代码的可维护性和可测试性。

正确使用 RuntimeInitializeOnLoadMethod，可以让你的游戏初始化流程更加清晰、可控，也能避免许多常见的初始化顺序问题。希望这篇文章能帮助你更好地理解和应用这个特性，在
Unity 开发中游刃有余。

---

**参考资料**：

- Unity 官方文档：RuntimeInitializeOnLoadMethodAttribute
- https://docs.unity3d.com/2022.3/Documentation/ScriptReference/RuntimeInitializeOnLoadMethodAttribute.html

- Unity 官方文档：RuntimeInitializeLoadType
- https://docs.unity3d.com/ScriptReference/RuntimeInitializeLoadType.html
- Unity 源码仓库
- https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Scripting/RuntimeInitializeOnLoadAttribute.cs