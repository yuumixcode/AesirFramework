---
name: aesir-framework
description: AesirFramework（RAA）入口编码指南。用 Aesir 写游戏代码时首先加载——核心裁决、Quick 档三件套（Context / Model / 面板）、关键约定、命名、技能地图。触发词：Aesir、RAA、AbstractContext、AbstractModel、MonoViewController、用 Aesir 写、按 Aesir 架构组织、搭游戏骨架、怎么开始。
---

# AesirFramework 入口

**AesirFramework = RAA（渐进式 MVC/MVP 架构）+ RAM（功能模块：UI / 事件 / 音频 / 场景）**。两个包经 Git URL 安装（`AesirArchitecture-latest` / `AesirModules-latest`，国内优先 `cnb.cool/runestone-game/AesirFramework`、GitHub 兜底），RAM 缺 RAA 时经菜单 `Tools/Aesir/Modules/Install Dependencies` 一键补装。

> **写代码前先过依赖门禁**：探测工程实际装了哪个包 → 读记忆（记忆明确禁止装 RAM 就不装）→ 没有明确禁止就必须询问开发者是否经 Git URL 安装（CNB 优先、GitHub 兜底）。完整判定表与命令见 `aesir-router` 的「依赖门禁」。缺包时不要用原生 API 顶替。

本技能只讲"从零写出第一个能跑的模块"。档位、核心 API、决策、陷阱各有一篇独立的兄弟技能，按需加载（见文末技能地图）。

## 核心裁决（AI 必须内化）

1. **Model = 运行时的数据**，是底层核心数据源，不针对某个 UI。内容 = ① UI 绑定的 property（`ObservableValue<T>`）② 普通字段（布尔等）③ 部分需存档的数据。**运行时数据 ≠ 全部存档**——存档是选择性的，快速场景通常不做。
2. **默认按最低档（Quick，最宽松档位）执行**：不建 Model/Service 接口、不做只读 property 包装、不用 Command/Query；持有与注册 Model/Service 一律用**具体类**（`RegisterModel(new PlayerModel())`，引用字段 `PlayerModel _player`）。需要扩展性时再升级（升档是加法，见 `aesir-tiers`）。
3. **UI 与场景管理是框架核心两件套**，都有"零配置默认路径"（`UIModule.Show<T>(path: ...)` / `SceneModule.LoadSceneSingle(...)`）。
4. **资产加载默认 `Resources`**（demo 级最简；`IUIAssetLoader` 可替换是进阶能力，不碰就不存在）。
5. **暂不提供 MVVM**：快速开发首推 MVC，MVP 是 Unity 官方最佳实践范式（可选）。用户要求"用 MVVM 写"时，说明决策并引导到 MVC/MVP，不擅自引入 ViewModel 层或绑定引擎。理由见 `aesir-decisions`。

## 默认写法：Quick 档三件套

最少 3 个文件跑通数据闭环（逐行讲解的完整模板见 `aesir-tiers/references/mvc-quick.md`）：

```csharp
// 1. Context：按具体类注册
public sealed class GameContext : AbstractContext<GameContext>
{
    protected override void Configure()
    {
        RegisterModel(new PlayerModel());
        RegisterService(new AudioService());   // Service 同理：具体类
    }
}

// 2. Model：运行时数据——ObservableValue（要通知 UI 的）+ 普通字段（不要通知的）
[Serializable]
public sealed class PlayerModel : AbstractModel
{
    [SerializeField] ObservableValue<int> hp = new ObservableValue<int>(3);
    public ObservableValue<int> Hp => hp;     // 直接暴露可写 ObservableValue（默认档不做只读包装）

    public bool isInvincible;                  // 普通字段：不需要通知就别用 ObservableValue
}

// 3. 面板：AesirBasePanelViewController<T>（RAM 面板基类，View 兼 Controller，可读可写）
//    注意不是 RAA 的 MonoViewController<T>——那个是场景里直接挂的 MonoBehaviour，
//    不经 UIModule 的预制体加载/生命周期，无法配合 UIModule.Show<T>() 显示。
public sealed class PlayerPanel : AesirBasePanelViewController<GameContext>
{
    PlayerModel _model;

    protected override void OnInit()                                 // 取代 Awake：UIModule 装配期调用
    {
        _model = this.GetModel<PlayerModel>();                       // 缓存为字段，勿每帧 GetModel
        _model.Hp.AddListenerAndInvoke(OnHpChanged)                  // 订阅并立即拿当前值
            .RemoveListenerWhenGameObjectOnDestroyed(gameObject);     // 句柄必须绑生命周期
    }

    void OnHpChanged(int newHp) { /* 刷新 UI */ }
    public void OnHealButtonClicked() => _model.Hp.Value += 1;        // 直改（快捷档特有）
}

// 显示（预制体存 Resources 下，path 用命名参数——细节见 aesir-ui 技能）：
// UIModule.Show<PlayerPanel>(path: "UI/PlayerPanel");
```

**Context 访问方式**：View 基类内用 `this.GetModel<T>()`；世界物体/其它脚本用 `GameContext.Instance.GetModel<T>()`（纯 C# 单例懒加载，不随场景卸载销毁——重开必须先 `GameContext.Instance.Dispose()`）。

## 关键约定（违反即 fail-fast 或踩坑）

1. **Register 与 Get 类型参数必须一致**——默认档都按具体类。
2. **Configure() 中禁止访问 `Instance`**——递归创建第二个上下文。
3. **GetModel 在 Start/Awake 缓存为字段**——勿在每帧路径反复调用。
4. **监听回调不应抛异常**——MiniEvent fail-fast 会中断后续监听者。
5. **订阅句柄绑生命周期**——`RemoveListenerWhenGameObjectOnDestroyed/OnDisable`；清理逻辑放 `OnDestroy`（`OnClose` 只在受控销毁路径调用）。
6. **OnEnable/OnDisable 逐一对称**——禁用 `RemoveAllListeners()` 一刀切。
7. **禁止静态可变全局状态**（`public static` 可变字段）；例外：常量类、路径常量、纯函数门面。
8. **禁止手写 `.prefab`/`.unity`/`.asset` YAML**——一切资产经编辑器脚本生成（UI 生成器 / BootSceneBuilder）。
9. **禁止修改框架包**（`Assets/Runestone/**`、`Library/PackageCache/cn.runestone.*`）——框架问题在业务侧绕过并记录。
10. **Model 不引用 UnityEngine 组件类型**（Transform/GameObject/MonoBehaviour）。

## 工程初始化流程（jam 开工 / 新项目起步）

1. 装两个包（Git URL 两条都要加）。
2. **创建启动场景**：用 `aesir-boot-scene` 技能（`../aesir-boot-scene/SKILL.md`）——一键产出 `Bootstrap.unity`（只含启动脚本 + AudioListener，无相机）+ `Bootstrap.cs`，登记 Build Settings index 0。
3. 建目录结构：脚本按 `aesir-project-structure` §2.2 分——注册进 Context 的 Context / Model / Service 放 `Scripts/Contexts/` `Model/` `Service/`，没注册的 View / Controller 按业务放 `Scripts/Gameplay/` 或 `Scripts/UI/`，全部全局命名空间。延时与冷却直接用框架的 `AesirTimer`（见 `aesir-core-api`），不要自己抄一个计时器类。
4. 写 Context + 第一个 Model + 第一个面板，跑通闭环。
5. 自检一遍：Console 无编译错误、启动场景能进、第一个面板能正常打开。

## 命名规范

- Context：`XxxContext : AbstractContext<XxxContext>`；Model：`XxxModel : AbstractModel`；Service：`XxxService : AbstractService`
- View：`XxxPanel`（面板）/ `XxxWindow`（窗口，根自带 Canvas）；Controller：`XxxController`（纯 C# sealed 类）
- Command：`XxxCommand : AbstractCommand`；Query：`GetXxxQuery : AbstractQuery<TResult>`（升档才用）
- ObservableValue：私有 `[SerializeField]` 字段 `camelCase`，对外属性 `PascalCase`
- 事件参数载体（EventModule）：`XxxEventArgs : AesirEventArgs`；MVP View 事件无 On 前缀
- 目录：`Assets/_Game/Scripts/` 下——注册进 Context 的按角色 `Contexts/` `Model/` `Service/`，没注册的 View / Controller 按业务 `Gameplay/` `UI/`（详见 `aesir-project-structure` §2.2）

## 技能地图（按需加载，不要一次全读）

| 技能 | 什么时候加载 |
|---|---|
| `aesir-tiers`（`../aesir-tiers/SKILL.md`） | 要选档位、升档、写 Command/Query、写 MVP 时（六档模板就在那里） |
| `aesir-core-api`（`../aesir-core-api/SKILL.md`） | 需要 `ObservableValue` / `MiniEvent` / 可观察集合 / `AesirTimer` / PlayerLoop 的具体写法时 |
| `aesir-decisions`（`../aesir-decisions/SKILL.md`） | 选 MVC 还是 MVP、用户提 MVVM、犹豫用哪种通知机制时 |
| `aesir-pitfalls`（`../aesir-pitfalls/SKILL.md`） | 报错、行为诡异、要自查写法时 |
| `aesir-project-structure`（`../aesir-project-structure/SKILL.md`） | 新项目先立目录骨架、Assets 按用途分层、docs 文档站、CODELY.md 怎么写（不依赖包） |
| `aesir-boot-scene`（`../aesir-boot-scene/SKILL.md`） | 项目初始化、建启动场景 |
| `aesir-ui` / `aesir-events` / `aesir-audio` / `aesir-scene`（`../aesir-ui/SKILL.md` 等同级目录） | 做界面 / 发事件 / 播音效 / 切场景的细节。**RAM 模块细节以这四个技能为准**，本技能不重复 |
