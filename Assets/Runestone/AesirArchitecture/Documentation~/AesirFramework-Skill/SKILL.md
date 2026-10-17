---
name: aesir-framework
description: AesirFramework（RAA 架构框架 + RAM 功能模块）的 AI 编码指南。为 Unity/团结引擎项目用 Aesir 框架写游戏代码时使用——覆盖 Context/Model/View/Controller 组织与默认最低档（Quick）写法、六档分级模板、UI（Panel 默认/Window 进阶）、场景一行切换、事件机制（ObservableValue/MiniEvent/EventModule）、音频、jam 快速开发实战。触发场景：用户提到 Aesir、AesirFramework、RAA、RAM、AbstractContext、ObservableValue、MiniEvent、UIModule、SceneModule、AudioModule，或要求"用 Aesir 框架写"、"按 Aesir 架构组织代码"、"搭游戏骨架"。
---

# AesirFramework AI 编码指南

## 框架定位（2026-10-15 定稿）

AesirFramework 是 **game 快速使用框架**：渐进式 MVC/MVP 架构（RAA）+ 功能模块（RAM：UI / 场景 / 事件 / 音频）。两个包经 Git URL 安装（`AesirArchitecture-latest` / `AesirModules-latest`），RAM 缺 RAA 时经菜单 `Tools/Aesir/Modules/Install Dependencies` 一键补装。

**核心裁决（AI 必须内化）**：

1. **Model = 运行时的数据**，是底层核心数据源，不针对某个 UI。内容 = ① UI 绑定的 property（`ObservableValue<T>`）② 普通字段（布尔等）③ 部分需存档的数据。**运行时数据 ≠ 全部存档**——存档是选择性的，快速场景通常不做。
2. **默认按最低档（Quick，最宽松档位）执行**：不建 Model/Service 接口、不做只读 property 包装、不用 Command/Query；持有与注册 Model/Service 一律用**具体类**（`RegisterModel(new PlayerModel())`，引用字段 `PlayerModel _player`）。AI 时代，需要扩展性时再升级（升档是加法，见文末）。
3. **UI 与场景管理是框架核心两件套**，都有"零配置默认路径"（下文速查）。
4. **资产加载默认 `Resources`**（demo 级最简；`IUIAssetLoader` 可替换是进阶能力，不碰就不存在）。

## 默认写法：Quick 档三件套

最少 3 个文件跑通数据闭环（完整模板见 [references/mvc-quick.md](references/mvc-quick.md)）：

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

// 3. 面板：MonoViewController<T>（View 兼 Controller，可读可写）
public class PlayerPanel : MonoViewController<GameContext>
{
    PlayerModel _model;

    void Start()
    {
        _model = this.GetModel<PlayerModel>();                       // 缓存为字段，勿每帧 GetModel
        _model.Hp.AddListenerAndInvoke(OnHpChanged)                  // 订阅并立即拿当前值
            .RemoveListenerWhenGameObjectOnDestroyed(gameObject);     // 句柄必须绑生命周期
    }

    void OnHpChanged(int newHp) { /* 刷新 UI */ }
    public void OnHealButtonClicked() => _model.Hp.Value += 1;        // 直改（快捷档特有）
}
```

**Context 访问方式**：View 基类内用 `this.GetModel<T>()`；世界物体/其它脚本用 `GameContext.Instance.GetModel<T>()`（纯 C# 单例懒加载，不随场景卸载销毁——重开必须先 `GameContext.Instance.Dispose()`，见 [references/jam-playbook.md](references/jam-playbook.md) 第 3.4 节）。

## 六档分级（默认 Quick，按需升档）

| 档位 | Model 注册 | Model 暴露面 | 写入路径 | 文件数 |
|------|-----------|-------------|---------|--------|
| **Quick（默认）** | 具体类 | 可写 `ObservableValue` 直接暴露 | 直改 `.Value` | ~3-4 |
| Standard | 具体类 | 只读接口 `IReadOnlyObservableValue<T>` + 写方法 | 调 Model 写方法 | ~4-5 |
| Strict | 接口 | 只读接口 + 写方法 | `ExecuteCommand` | ~10-11 |

- MVC（推荐）：View 基类 `MonoViewController<T>`（Quick）/ `MonoView<T>`（Standard+）
- MVP（可选）：View 纯 `MonoBehaviour`，Presenter 推送刷新
- **升档是加法不是重写**：Quick → Standard 收窄暴露 + 加写方法；→ Strict 叠加接口与 Command/Query。升档触发条件（写入逻辑 >30 行 / 跨多 View 同一操作 / 运行期替换实现）见 jam-playbook 第 7 节。

完整模板按需加载：[mvc-quick](references/mvc-quick.md) / [mvc-standard](references/mvc-standard.md) / [mvc-strict](references/mvc-strict.md) / [mvp-quick](references/mvp-quick.md) / [mvp-standard](references/mvp-standard.md) / [mvp-strict](references/mvp-strict.md)。

## UI 速查（详细见 [references/modules-api.md](references/modules-api.md)）

**默认用 Panel**：预制体根节点**不带 Canvas**，挂到框架预设的四层 Canvas 之下；简单/默认情况下单个 Prefab 不要以 Canvas 为根。Window（根自带 Canvas）是进阶形态，只用于模态弹窗/全屏流转页。

```csharp
UIModule.Show<MainMenuPanel>(path: "UI/MainMenuPanel");   // 必须 path: 命名参数！
UIModule.Hide<MainMenuPanel>();
UIModule.Open<PauseWindow>(payload, path: "UI/PauseWindow");  // 窗口（模态/蒙版/独立排序）
UIModule.Close<PauseWindow>();
```

- 一行 `Show` 自动完成全部装配（`[Aesir Modules]` 宿主 → UIModule → UIRoot + UICamera + EventSystem + 四层 Canvas），**场景零预放置零配置**。
- `path` 经 Resources 加载并自动注册，后续同类型直接 `Show<T>()`。
- 主相机 cullingMask 排除 UI(5) 与 TransparentFX(1)，防 UI 重复渲染。

## 场景速查（详细见 [references/modules-api.md](references/modules-api.md)）

```csharp
SceneModule.LoadSceneSingle("Assets/Scenes/Gameplay.unity",
    onCompleted: () => { }, onFailed: e => Debug.LogError(e));
```

- **一行切换已成立**：零预放置零配置，唯一前置 = 目标场景登记 Build Settings。
- 传**完整资产路径**（含 `.unity`），勿传裸名；勿混用原生 `SceneManager`（记账失真）。
- 重开：先 `GameContext.Instance.Dispose()` 再 `SceneModule.ReloadScene()`（Context 是纯 C# 单例不随场景销毁）。

## 通知机制（每个场景只有一个默认答案）

| 场景 | 用 |
|------|------|
| 状态变化通知（血量/分数/流程） | `ObservableValue<T>`（订阅用 `AddListenerAndInvoke`） |
| 一次性瞬时事件（"怪死了"） | `MiniEvent` / `MiniEvent<T>`（挂 Model 上，句柄绑生命周期） |
| View→Presenter 输入（MVP） | C# `event` |
| Inspector 拖拽连线 | `UnityEvent` |
| 跨系统广播（成就/全局播报） | RAM `EventModule`（受限使用，见 modules-api.md） |

决策表全文见 [references/event-decisions.md](references/event-decisions.md)。

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
2. **创建启动场景**：用 `AesirBootScene-Skill`（同目录 `../AesirBootScene-Skill/SKILL.md`）——一键产出 `Boot.unity`（只含启动脚本 + AudioListener，无相机）+ `GameBootstrap.cs`，登记 Build Settings index 0。
3. 建目录结构与缺口补件（JamTimer / JamInput / GameFlowModel / UIPaths / GameConfig / JamFonts）：按 [references/jam-playbook.md](references/jam-playbook.md) 第 1、3 节。
4. 写 Context + 第一个 Model + 第一个面板，跑通闭环。
5. 写自检脚本（jam-playbook 第 5 节）。

## references 索引

| 文件 | 内容 | 何时加载 |
|------|------|---------|
| [mvc-quick.md](references/mvc-quick.md) | 默认档完整模板（Context/Model/Panel 三件套） | 写第一个系统时 |
| [mvc-standard.md](references/mvc-standard.md) / [mvc-strict.md](references/mvc-strict.md) | MVC 中间/严格档模板 | 升档时 |
| [mvp-quick.md](references/mvp-quick.md) / [mvp-standard.md](references/mvp-standard.md) / [mvp-strict.md](references/mvp-strict.md) | MVP 三档模板 | 选 MVP 模式时 |
| [core-api.md](references/core-api.md) | ObservableValue / MiniEvent / Context / PlayerLoop / 能力矩阵 | 需要辅助模块细节时 |
| [modules-api.md](references/modules-api.md) | RAM 四模块 API 白名单（UI/Scene/Event/Audio）+ 陷阱 | 写 UI/场景/音频时 |
| [jam-playbook.md](references/jam-playbook.md) | jam/快速开发实战：工程约定、缺口补件、UI 生成器、自检、反模式 | 快速开发/jam 场景开工时 |
| [pitfalls.md](references/pitfalls.md) | 10 条高频陷阱（现象+原因+修法） | 排错时 |
| [event-decisions.md](references/event-decisions.md) | 事件机制决策表 | 选通知机制时 |

## 命名规范

- Context：`XxxContext : AbstractContext<XxxContext>`；Model：`XxxModel : AbstractModel`；Service：`XxxService : AbstractService`
- View：`XxxPanel`（面板）/ `XxxWindow`（窗口，根自带 Canvas）；Controller：`XxxController`（纯 C# sealed 类）
- Command：`XxxCommand : AbstractCommand`；Query：`GetXxxQuery : AbstractQuery<TResult>`（升档才用）
- ObservableValue：私有 `[SerializeField]` 字段 `camelCase`，对外属性 `PascalCase`
- 事件参数载体（EventModule）：`XxxEventArgs : AesirEventArgs`；MVP View 事件无 On 前缀
- 目录：`Context/` `Model/` `Controller/` `View/` `Core/`（业务工程约定，见 jam-playbook 第 1.1 节）
