# Jam / 快速开发实战手册（Playbook）

> 面向 24 小时 Game Jam、demo、快速原型场景的工程组织与实战规则。框架常规 API 见本 skill 的 SKILL.md 与 [modules-api.md](modules-api.md)；本文只收"工程怎么搭、缺什么补什么、什么不能做"。
>
> **档位口径（2026-10-15 起）**：默认按最低档（Quick）执行——具体类注册、可写 `ObservableValue` 直接暴露、直写直读、零接口、零 Command/Query。需要更强纪律时再升档（见文末升档路径）。

## 1. 工程约定（开工 10 分钟）

### 1.1 目录结构（照此创建，不自创层级）

```
Assets/
├── Game/
│   ├── Core/            # JamTimer / JamInput / GameFlowModel / UIPaths / GameConfig / GameBootstrap
│   ├── Model/           # XxxModel（运行时数据）
│   ├── Controller/      # XxxController（跨 Model 编排；纯 C# sealed 类，可选）
│   ├── View/            # XxxPanel / XxxWindow / 世界物体驱动脚本
│   ├── Context/         # GameContext.cs（唯一）
│   └── UI/
│       ├── Resources/GameUI/   # 所有 UI 预制体（供 ResourcesUILoader 按路径加载）
│       └── Art/               # AI 生成的 UI 图（PNG）
├── Editor/
│   └── Game/            # UiPrefabBuilder（UI 生成器）+ JamSelfCheck（自检）+ BootSceneBuilder
├── Art/                 # 世界素材（Sprite / Audio）
└── Scenes/              # Boot.unity（启动场景）+ Main.unity（玩法场景）
```

### 1.2 明确"不用"什么

| 不用 | 原因 |
|------|------|
| **asmdef** | 全部业务代码放默认 `Assembly-CSharp`（框架包对新程序集默认自动引用）。少一层编译上下文，AI 出错面减半 |
| **Odin Inspector** | 能装但本手册不依赖；RAM 的 Binder 需要 Inspector 手动操作，AI 驱动不了 |
| **Addressables** | UI 走 `Resources`（同步契约，最简单） |
| **UniTask / async** | 24 小时内异步几乎只会带来时序 bug；需要延时用 JamTimer |
| **TextMeshPro** | 中文需要预烘焙字体图集，快速场景是纯负债；用 uGUI `Text` + JamFonts 动态字体 |

工程设置：`Active Input Handling = Input Manager (Old)`（统一用 `Input.GetKey*`）；启动场景登记 Build Settings 且为 index 0（用 AesirBootScene-Skill 的 BootSceneBuilder 一键完成）。

## 2. 硬规则（违反即返工）

1. **禁止手写 `.prefab` / `.unity` / `.asset` 的 YAML**——一切资产经编辑器脚本生成（UI 用第 4 节生成器；场景用 BootSceneBuilder）。
2. **禁止修改框架包**（`Assets/Runestone/**` 或 `Library/PackageCache/cn.runestone.*`）；发现框架问题在业务代码绕过并记录，不改包。
3. **禁止静态可变全局状态**（`public static` 可变字段/单例）。唯一例外：`GameConfig` 常量、`UIPaths` 路径常量、`JamInput` 纯函数门面。
4. **禁止在 Model 里引用 UnityEngine 组件类型**（`Transform`/`GameObject`/`Camera`/`MonoBehaviour`）；Model 只放 `ObservableValue` / `MiniEvent` / 值类型 / 纯数据。**运行时数据不代表全部存档**——存档是选择性的，jam 里通常不做存档。
5. **禁止把配置只放 Inspector**：可调数值进 `GameConfig`，UI 引用由生成器回填。Inspector 只作调试加成。
6. **订阅必须绑生命周期句柄**：`AddListener(...).RemoveListenerWhenGameObjectOnDestroyed(gameObject)`；清理逻辑放 `OnDestroy`（`OnClose` 只在受控销毁路径调用）。
7. **所有读输入的点先判流程态**：`Update` 首行 `if (!_flow.IsPlaying) return;`（唯一例外：暂停/重开键，集中在 GameFlowDriver）。
8. **禁止顺手重构**：一次会话只做被要求的事。
9. **改动后必须**：确认 Console 无编译错误；动了 UI 提示重跑「重建 UI 预制体」；跑一次自检。

## 3. 缺口补件（框架没有、jam 高频，原样落文件）

### 3.1 JamTimer — 延时 / 冷却 / 倒计时

框架无时间调度设施（AesirScheduler 已移除）。全天只用它，禁止各写各的 `_timer += Time.deltaTime`：

```csharp
// Assets/Game/Core/JamTimer.cs
using UnityEngine;

namespace Game.Core
{
    /// <summary>帧驱动计时器：延时 / 冷却 / 倒计时统一用它。每帧调 Tick()，走完那一帧返回 true。</summary>
    public sealed class JamTimer
    {
        float _remaining;
        bool _useUnscaled;

        public bool IsRunning => _remaining > 0f;
        public float Remaining => _remaining;

        /// <param name="useUnscaled">true = 不受 Time.timeScale 影响（暂停时仍走）</param>
        public void Start(float seconds, bool useUnscaled = false)
        {
            _remaining = seconds < 0f ? 0f : seconds;
            _useUnscaled = useUnscaled;
        }

        public void Cancel() => _remaining = 0f;

        public bool Tick()
        {
            if (_remaining <= 0f) return false;
            _remaining -= _useUnscaled ? Time.unscaledDeltaTime : Time.deltaTime;
            if (_remaining > 0f) return false;
            _remaining = 0f;
            return true;
        }
    }
}
```

### 3.2 JamInput — 输入唯一入口

```csharp
// Assets/Game/Core/JamInput.cs
using UnityEngine;

namespace Game.Core
{
    /// <summary>全部输入读取的唯一入口。改键、暂停屏蔽都只改这里。</summary>
    public static class JamInput
    {
        public static float Horizontal => Input.GetAxisRaw("Horizontal");
        public static float Vertical   => Input.GetAxisRaw("Vertical");
        public static bool  Fire       => Input.GetKey(KeyCode.Space);
        public static bool  Pause      => Input.GetKeyDown(KeyCode.Escape);
        public static bool  Confirm    => Input.GetKeyDown(KeyCode.Return);
        public static bool  Restart    => Input.GetKeyDown(KeyCode.R);
    }
}
```

**禁止**在 View / Model / Controller 里直接写 `Input.*`。

### 3.3 GameFlowModel + GameFlowDriver — 流程状态与暂停

**状态放 Model（唯一真源），副作用集中一个驱动脚本（唯一响应点）**——24 小时内最容易写错的环节。

```csharp
// Assets/Game/Core/GameFlowModel.cs
using UnityEngine;
using Runestone.AesirArchitecture;

namespace Game.Core
{
    public enum JamState { Boot, Menu, Playing, Paused, GameOver }

    public sealed class GameFlowModel : AbstractModel
    {
        [SerializeField] ObservableValue<JamState> state = new ObservableValue<JamState>(JamState.Boot);

        // 最低档：直接暴露可写 ObservableValue（不做只读包装）
        public ObservableValue<JamState> State => state;

        public bool IsPlaying => state.Value == JamState.Playing;
        public bool IsPaused  => state.Value == JamState.Paused;

        /// <summary>流程写入约定入口——经它写入可让 GameFlowDriver 的副作用（timeScale/暂停窗口）集中响应。</summary>
        public void Enter(JamState next) => state.Value = next;
    }
}
```

```csharp
// Assets/Game/View/GameFlowDriver.cs — 场景里唯一负责"流程副作用"的物体
using UnityEngine;
using Runestone.AesirArchitecture;
using Runestone.AesirModules;
using Game.Core;
using Game.Model;

namespace Game.View
{
    public sealed class GameFlowDriver : MonoBehaviour
    {
        GameFlowModel _flow;
        ScoreModel _score;

        void Start()
        {
            _flow = GameContext.Instance.GetModel<GameFlowModel>();
            _score = GameContext.Instance.GetModel<ScoreModel>();
            _flow.State.AddListenerAndInvoke(OnStateChanged);
        }

        void Update()
        {
            // 暂停 / 重开键是全工程唯一"不判态也要读"的输入，集中放在这里
            if (JamInput.Pause)
            {
                if (_flow.IsPlaying) _flow.Enter(JamState.Paused);
                else if (_flow.IsPaused) _flow.Enter(JamState.Playing);
            }

            if (JamInput.Restart && !_flow.IsPlaying) GameRestart.Run();
        }

        void OnDestroy()
        {
            if (_flow != null) _flow.State.RemoveListener(OnStateChanged);
        }

        void OnStateChanged(JamState state)
        {
            Time.timeScale = state == JamState.Paused ? 0f : 1f;

            switch (state)
            {
                case JamState.Paused:
                    AudioModule.PauseAll();
                    UIModule.Open<PauseWindow>(path: UIPaths.PauseWindow);
                    break;
                case JamState.Playing:
                    AudioModule.ResumeAll();
                    UIModule.Close<PauseWindow>();
                    break;
                case JamState.GameOver:
                    UIModule.Open<ResultWindow>(_score.Score.Value, path: UIPaths.ResultWindow);
                    break;
            }
        }
    }
}
```

硬规则：

- **只在一个地方**（`GameFlowDriver`）做流程副作用（`Time.timeScale`、音频暂停、开关窗口）。其它脚本不许写 `Time.timeScale = ...`。
- **每个读输入的 `Update` 首行判态**（`if (!_flow.IsPlaying) return;`，`_flow` 在 `Start` 缓存）。`Time.timeScale = 0` **不会**阻止 `Update` 执行与 `Input.GetKey`——不判态就会"暂停时还能开火"。

### 3.4 GameRestart — 重开必须先 Dispose Context

`AbstractContext<T>.Instance` 是**纯 C# 单例，不随场景卸载销毁**，直接 `ReloadScene` 会把上一局 Model 值带进新一局：

```csharp
// Assets/Game/Core/GameRestart.cs
using UnityEngine;
using Runestone.AesirArchitecture;
using Runestone.AesirModules;

namespace Game.Core
{
    public static class GameRestart
    {
        public static void Run()
        {
            GameContext.Instance.Dispose();                             // 清空 Model（旧订阅随之失效）
            SceneModule.ReloadScene(onFailed: e => Debug.LogError(e)); // 重载玩法场景 → Bootstrap 重新跑
        }
    }
}
```

（不想重载场景则逐个 `Model.ResetState()`——不推荐，AI 极易漏。）

### 3.5 UIPaths / GameConfig / JamFonts

```csharp
// Assets/Game/Core/UIPaths.cs
namespace Game.Core
{
    /// <summary>UI 资源路径（相对任意 Resources 目录）。改路径只改这里。</summary>
    public static class UIPaths
    {
        public const string MenuPanel    = "GameUI/MenuPanel";
        public const string HudPanel     = "GameUI/HudPanel";
        public const string PauseWindow  = "GameUI/PauseWindow";
        public const string ResultWindow = "GameUI/ResultWindow";
    }
}
```

- `GameConfig`：`public static class` + `const`/`static readonly`，全部可调数值集中一处。
- `JamFonts`：跨平台中文动态字体（候选顺序 Microsoft YaHei → PingFang SC → Noto Sans CJK SC → WenQuanYi Zen Hei，`Font.CreateDynamicFontFromOSFont`），在面板/窗口 `OnInit` 里对子树 `Apply(transform)`。不做 TMP。

## 4. UI 资产生成器（AI 禁止手写预制体 YAML 的落地手段）

工作流（必须照此）：

```
① AI 改生成器脚本 Assets/Editor/Game/UiPrefabBuilder.cs（每个界面一个 BuildXxx() 方法）
② 点菜单重建（或编辑器桥执行 RebuildAll()）
③ 生成器把层级建在场景中 → 不需要 Play 就能看效果、直接拖拽调版式
④ 生成器把层级另存为 Assets/Game/UI/Resources/GameUI/*.prefab
⑤ 运行时按路径加载（UIModule.Show<T>(path: UIPaths.Xxx)，无需序列化引用）
```

生成器三个必知坑：

1. `[SerializeField]` 私有字段不能用 `obj.field = x` 赋值（不编译），**必须**走 `SerializedObject.FindProperty(fieldName)` + `ApplyModifiedPropertiesWithoutUndo()` 回填。
2. 窗口（Window 形态）根节点**必须**同时有 `Canvas + CanvasScaler + GraphicRaycaster`，否则 UIModule 挂载时报错中止。
3. 窗口的 `Mask` 子物体是**按名字**查找的（`transform.Find("Mask")`），蒙版点击的 `Button` 必须在 `Mask` 节点上；`Content` 只是"框架不触碰"的结构约定。

边界（硬规则）：生成器**只负责结构**（节点、组件、布局、引用回填）；重跑生成器 = 覆盖该预制体，禁止在产物上手改结构（加节点），要改就改生成器；手工微调的样式记录在 `<预制体>.md` 里。

## 5. 自检脚本（开工第一小时内写好）

`Assets/Editor/Game/JamSelfCheck.cs`，菜单一键跑。至少覆盖：

| 检查 | 要点 |
|------|------|
| 每个 `AbstractModel` 子类都已注册 | 扫 `Assets/Game/**` 提取 `: AbstractModel` 类名，比对 GameContext.cs 文本 |
| `UIPaths` 每个常量能解析到预制体 | `Resources.Load<GameObject>(path)` 非空，失败列全部缺失项 |
| 每个 UI 预制体根组件类型与文件名一致 | 遍历 `Assets/Game/UI/Resources/GameUI/*.prefab` 断言根组件名 == 文件名 |
| 场景里 `GameBootstrap` 恰好一个 | `FindObjectsByType` 计数 |
| 启动场景在 Build Settings index 0 | `EditorBuildSettings.scenes[0].path` |

价值：把静默失败（`LogError` + 返回 null、未注册 Model、路径拼错）变成点一下菜单就能发现。

## 6. 会话协作规则

- 一个 AI 会话只碰**一个系统目录**（`Model/` 或 `View/` 或 `Controller/` 之一）+ 该系统的生成器方法；跨系统需求拆两次会话（先 Model/Controller，再 UI）。
- 提交粒度：一个可玩增量 = 一次 commit，中文提交信息写"做了什么 + 为什么"。

## 7. 升档路径（默认 Quick，触发条件到了再加法升级）

| 触发条件 | 动作 | 成本 |
|---|---|---|
| 某写入逻辑 > 30 行，或 ≥2 个 View 触发同一操作 | 抽 `XxxController`（纯 C# sealed 类，构造注入 Model） | 0.5 小时 |
| 一次操作跨 ≥2 个 Model 且要"要么全成要么全不成" | 引入 Command（`XxxCommand : AbstractCommand`，Controller 实现 `IController<T>` 改走 ExecuteCommand） | 1 小时 |
| 运行期要替换 Model 实现（多难度/多模式共用一套 UI） | Model 改接口注册（`RegisterModel<IXxxModel>(...)`），View 按接口持有 | 1-2 小时 |
| 需要真正的异步加载（大场景/远端资源） | 装 UniTask + `SceneModuleUniTask` 可 await API | 1 小时（快速场景不推荐） |

档位详情与六档模板见 SKILL.md 的分级表与 [references/mvc-quick.md](mvc-quick.md) 等六份模板。

## 8. 反模式清单（会立刻吃掉时间的写法）

| 写法 | 后果 | 正确做法 |
|------|------|----------|
| 手写 `.prefab` / `.unity` YAML | 打开场景一片 None / Missing Script | 生成器（第 4 节）+ BootSceneBuilder |
| 视图里直接 `Input.GetKey` | 暂停仍开火、改键满仓搜 | 走 `JamInput` + 判态 |
| `Time.timeScale = 0` 当暂停全部 | `Update` 照跑、输入照样进来 | GameFlowDriver 判态规则（3.3） |
| 直接 `ReloadScene` 重开 | Context 是纯 C# 单例，上一局状态还在 | 先 `GameContext.Instance.Dispose()`（3.4） |
| `OnEnable` 里 `AddListener` 却只在 `OnClose` 清理 | 场景切换时泄漏 / 重复订阅 | 清理放 `OnDestroy`，句柄绑生命周期 |
| `RemoveAllListeners()` 一刀切 | 别的系统挂的监听被清掉 | 逐一对称 Remove |
| 在 `Configure()` 里访问 `Instance` | 递归创建第二个 Context | Configure 只写 RegisterModel/RegisterService |
| 按接口注册却按具体类取 | `InvalidOperationException`「未注册」 | Register 与 Get 类型参数一致（默认都按具体类） |
| 每帧 `this.GetModel<T>()` | 帧率下降 + GC | `Start` 里缓存到字段 |
| 每个脚本各写 `_timer += Time.deltaTime` | 暂停语义各不相同 | 统一 `JamTimer` |
| 给每个 Model 建接口 | 文件数翻倍、收益为零 | 默认不建，升档触发条件到了再建 |
| 用 TextMeshPro 显示中文 | 字体图集要烘焙 | uGUI `Text` + JamFonts 动态字体 |
| 用 Odin Binder 做 UI 绑定 | 需要 Inspector 手动操作，AI 驱动不了 | 生成器回填 + 代码订阅 |
| 场景混用原生 `SceneManager.LoadScene` | 绕过模块记账，`AddedScenePaths`/`LastLoadedScene` 失真 | 统一走 `SceneModule` 静态 API |
