# Aesir Modules 特性一览

## UI 框架

### 核心类型

| 类型 | 说明 |
|------|------|
| `UIModule` | UI 管理器单例(Manager of Managers):面板注册、显示、隐藏、预热与注册表维护;提供静态快捷 API 全局调用 |
| `UIRoot` | UI 根节点:构建四层 Canvas(Background / Normal / Popup / Top)+ UICamera + EventSystem,Canvas 统一配置经 `UICanvasConfigSO` |
| `IUIPanel` | 面板契约:生命周期 `Initialize → Show(payload) → Hide → DestroyPanel`;属性 `Layer` / `DestroyOnHide` / `IsOpen` |
| `AesirBasePanel` | 面板基类:虚方法 `OnInit` / `OnShow` / `OnHide` / `OnClose`,便捷方法 `HideSelf()` |
| `AesirBasePanelView<T>` | MVP 模式面板视图基类:继承 `AesirBasePanel` 并按 Context 类型绑定,经 Context 访问 Model / Service |
| `AesirBasePanelViewController<T>` | MVC 模式面板控制器基类:继承 `AesirBasePanel` 并按 Context 类型绑定,可执行 Command / Query |
| `IUIAssetLoader` / `ResourcesUILoader` | 可插拔资源加载契约(同步语义)与默认实现 |
| `UICanvasConfigSO` | Canvas 统一配置资产(层序基准 100/200/300/400 每次初始化强制覆盖) |
| `UILayer` | 层级枚举:Background / Normal / Popup / Top |

### 面板生命周期

```
激活 → 停用(缓存)→ 销毁
```

- 面板以停用状态实例化,按 挂层 → `Initialize` → `Show` 顺序驱动
- `DestroyOnHide = false` 时隐藏仅 SetActive(false) 缓存复用;`true` 时隐藏即销毁(受控销毁走 `OnHide` → `OnClose`)
- **`OnClose` 仅受控销毁路径调用**;场景卸载 / 外部 Destroy 只触发 `OnDestroy` —— 事件解绑放 `OnDestroy`
- 注册以实例**实际类型**为键:重复 Show 报错拒绝、基类类型 Hide / Get 警告提示(键语义诊断)
- `Prewarm<T>()` 逐帧预实例化,`PrewarmAll()` 分摊首次打开的实例化卡顿
- `AesirBasePanel.OnDestroy` 静态反清理 `UIModule` 注册表,防止域内残留

### Binder 组件绑定(Odin 可选)

`BinderAssistant` / `BinderTag` 将 UI 元素自动绑定到面板脚本:

- `BinderTag` 挂在子物体上做标记;`BinderAssistant` 挂在面板根,「构建绑定单元」按标记增量维护绑定列表
- 两种生成模式:**同一脚本增量**(只替换 `*.cs` 内「绑定字段(自动生成)」region,region 外归开发者)与 **Partial 分部类**(手写 partial + 自动维护文件,Rider 用户推荐)
- 生成脚本基类可下拉选择:`MonoBehaviour`、Aesir 面板家族(`AesirBasePanel` / `AesirBasePanelView<T>` / `AesirBasePanelViewController<T>`)或用户以 `[BinderBaseType]` 标记的类
- 编译完成后自动挂载组件并执行一次绑定;配套 EditMode 测试

### Input System 适配(可选)

独立程序集;启用 Input System 包时自动以 `InputSystemUIInputModule` 替换 UIRoot 的默认输入模块。

## 事件模块

Script 订阅事件系统:`AddListener<T>` 注册委托、返回 `AutoRemoveListenerHandle` 精确退订(唯一订阅形态),按 4 档优先级(First / High / Medium / Last)稳定执行(同档按注册顺序;注册期有序插入,分发零排序);分发基于注册表快照迭代,跨实例重入安全(同一参数实例的重入发布被拒绝)。**热路径零反射、零排序、零字符串分配、零装箱**。内置**订阅者过滤器**(精确投递)、**取消传播**、**死引用自动清理**与可选**分发耗时告警**;支持 **SO 资产化**(`AesirEventArgsSO` + `UnityEventOnAesirEvent` 桥接),非程序员可在 Inspector 配置事件;可观测性由公开门面 `AesirEventDiagnostics` + 三个调试窗口提供。详见[事件模块](events.md)。

```csharp
// 1. 定义事件参数(数据载体)
public class OnPlayerScored : AesirEventArgs
{
    public int points;
    public string playerName;
}

// 2. Script 订阅(返回 AutoRemoveListenerHandle,OnDisable 中 Dispose)
_handle = EventModule.AddListener<OnPlayerScored>(this, e => Debug.Log(e.points));
_handle.Dispose();

// 3. 发布
new OnPlayerScored { points = 10, playerName = "Player1" }.Invoke(this);
```

核心类型:`AesirEventArgs`(参数基类,支持 `WithFilter` 链式过滤器与 `Cancel` 取消传播)、`EventModule`(单例)、`ISubscriberFilter`(过滤器策略接口)、`AesirEventArgsSO` / `UnityEventOnAesirEvent`(SO 资产化)、`SubscriberPriority`、`AesirEventDiagnostics`(诊断公开门面)。

```csharp
// 订阅者过滤器:只有 Tag 为 "Enemy" 且在爆炸范围内的订阅者收到
new OnExplosion()
    .WithFilter(new InsideCollider2D())
    .WithFilter(new WithTag("Enemy"))
    .Invoke(this);
```

## 音频模块

`AudioModule` 单例的公开 API 全为静态成员,零配置调用即用 —— 2D 音频极简门面:

- **SFX** — 固定数量独占音源轮询(默认 8):无每播实例化开销,每次播放局部音量 / 音调独立,`pitchJitter` 抖动防机械感
- **BGM** — 专用循环源:同曲在播幂等返回;切换支持协程淡入淡出(`unscaledDeltaTime`,slow motion 不变调)
- **音量与静音** — Master / BGM / SFX 三通道乘法链,设置即时生效并经 PlayerPrefs 持久化
- **暂停** — `PauseAll` / `ResumeAll` 一对

```csharp
AudioModule.PlaySfx(clickClip, pitchJitter: 0.1f);   // SFX 一击即走
AudioModule.PlayBgm(sceneB, fadeSeconds: 1.5f);      // 切歌淡入淡出
AudioModule.SfxVolume = 0.5f;                        // 设置即生效、即持久化
```

详见[音频模块](audio.md)。

## 场景模块

`SceneModule`(MonoBehaviour 单例,预放置优先)负责场景加载、叠加追踪与卸载回收:

- **场景加载** — `LoadSceneSingle` / `LoadSceneAdditive` 纯叠加追踪,路径或 `SceneAssetWrapper` 双重重载,完成 / 失败 / 进度回调齐全(`onProgress` 已按 Unity 激活上限 0.9 归一化)
- **激活场景切换** — `SetActiveScene` 决定多场景叠加工作流的光照设置来源与 Instantiate 默认落点
- **场景事件广播** — `SceneLoadedEvent` / `SceneUnloadedEvent`(MiniEvent,句柄自动清理)
- **`SceneAssetWrapper`** — 可序列化场景引用:GUID 锚点自愈、状态机校验、`TryGet` 安全读取家族、专用异常族;安装 Addressables 时自动扩展地址查询能力(Inspector 面板效果需 Odin)
- **编辑器配套** — `Tools → Aesir → Modules → Scene Editor Settings` 设置窗口;`BootstrapSceneHelper` 搜集注册 Bootstrapper 场景(默认关闭,运行时只持引用不做自动流转)

## 脚本文档生成模块(需 Odin)

- **Script Doc Generator** — 反射分析 C# 类型生成结构化 API 文档:全离线、增量生成(保留 `## Additional Notes` 之后的手写内容)、支持命名空间子目录与四种来源粒度(单类型 / 多类型 / 单程序集 / 多程序集),Markdown 输出可直接用于 AI 知识库。入口 `Tools → Aesir → Modules → Script Doc Generator`
- **Summary 工具** — 在 XML `<summary>` 注释与 `[Summary]` 特性之间同步(**特性为权威内容源**):Sync 双向对齐 / Replace 收敛为特性 / Remove 移除(带确认),批量单次刷新。入口 Project 右键 `Assets → Script Doc Generator → Process Summary`
- **自定义特性** — `[Summary]`(运行时可经 `GetSummary()` 读取)、`[ReferenceLinkURL]`(为类型附加文档链接)