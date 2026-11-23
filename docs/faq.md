# 常见问题(FAQ)

## 安装与更新

### Git URL 和 unitypackage 怎么选?

| 方式 | 适合谁 |
|------|--------|
| **Git URL(常驻 latest 分支)** | 常规项目;包只读、经 Package Manager 管理(URL 一次输入持续可用),不碰用户代码区 |
| **unitypackage + 包内更新器** | 想**改包源码**、或大陆 / 离线环境 GitHub 直连不稳的项目;安装在 `Assets/Runestone/` 下,`Tools → Aesir → Check for Updates` 一键更新 |
| **跟踪 main(`?path=`)** | 想吃最新开发预览的用户 |

### 为什么更新器检测不到刚发布的版本?

版本检测按「直连 GitHub → 镜像站 → CDN 中转」三层兜底,**能直连 GitHub 就是 100% 最新**(窗口会显示本次直连是否可用)。只有当直连与镜像站都不可用、结果来自 CDN 中转时,才可能因分支缓存(最长约 12 小时)拿到旧版本 —— 此时窗口会给出延迟提示,可点「打开 Releases 页面」人工确认。

### 更新器能更新 Git URL(UPM)安装的包吗?

不能。更新器只管辖 `Assets/Runestone/` 下的代码导入副本;UPM 副本请直接用 Package Manager 更新。**开发仓库(存在 `.git`)切勿点更新** —— Release 内容会覆盖本地源码(窗口已内置警告)。

### Git URL 分支策略是什么?

常驻分支 `AesirArchitecture-latest` / `AesirModules-latest` 由 CI 在每次推送 main 时自动按包目录 subtree split **滚动更新**,分支名永久固定——Git URL 一次输入持续可用,升级 = Package Manager 移除后用同一 URL 重新添加。想锁旧版本请用 Release tag(`?path=Assets/Runestone/<包目录>#v<版本>`,tag 永久保留)或 Releases 页面的 unitypackage。(旧策略的版本分支 `AesirXxx-v<版本>` 随发版轮换并删除,已废弃——钉住它的 URL 会在发版后失效。)

### 安装 Aesir Modules 时为什么还要添加 Architecture 的 URL?

Modules 依赖 Architecture(必需)。UPM 对 Git URL 安装的包不会自动解析包内 dependencies 为 Registry 引用,因此两个包的 Git URL 都要添加(推荐同版本)。

## 使用问题

### 没装 Odin Inspector 能用吗?

**能**。核心架构流程闭环(Context 注册 → Model/Service 初始化 → Command/Query 执行 → ObservableValue 通知)与 UI 框架核心功能无 Odin 完整可用;Odin 相关代码经 `#if ODIN_INSPECTOR` 条件编译自动排除,只影响 Inspector 增强体验(`AesirView<T>` 系列基类、Binder、Drawer 等)。

### 示例怎么导入?会进构建吗?

- **Git URL 安装**:Package Manager → 选中包 → `Samples` 标签页按需 Import
- **unitypackage 导入 / 本仓库浏览**:示例随包内含,位于各包 `Samples/` 目录
- 示例程序集为运行时程序集 + 整文件 `#if UNITY_EDITOR`:编辑器内可运行,**玩家构建整体剔除**(示例类型 0 入包)

### 示例场景打开后脚本 Missing / 无法挂载?

请确认使用 Unity / 团结引擎 2022.3+,并完整导入包(含 `Samples`)。本仓库直接浏览时示例就在 `Samples/` 目录下,场景可直接打开运行。

## 测试与 testables

### 能在自己的项目里跑框架的单元测试吗?

能。框架的测试程序集随 UPM 形态保留(包内 `Tests/` 目录不删),默认不编译、不污染你的工程。想跑的话,在 `Packages/manifest.json` 的 `testables` 数组里加上包 ID:

```json
"testables": [
  "cn.runestone.aesir.architecture",
  "cn.runestone.aesir.modules"
]
```

保存后 Package Manager 会编译对应测试程序集,在 `Window → General → Test Runner` 里即可运行。只需要哪个包就加哪个 ID,不影响另一个。

### 框架的单元测试到底在测什么?

测试守护的是**框架的公开行为契约** —— 例如 ObservableValue 的"无变化不通知"、可观察集合的变更事件语义、计时器的状态转换、UI 面板的生命周期等。这些用例在框架的**开发仓库环境**中编写和运行,"全绿"的含义是:**框架开发者改代码时没有破坏这些对外承诺**。

因此用例里带着开发环境的几个隐含前提:

- 包源码位于 `Assets/Runestone/` 下(而非 `Packages/`)
- Odin Inspector / UniTask 等可选依赖在场
- 工程内存在模块配置的基线资产

### 开了 testables,为什么有少量测试失败?

这通常是**环境差异**,不代表功能不可用。上面的隐含前提在你的 UPM 工程里大多不成立 —— 路径断言指向的 `Assets/Runestone/...` 不存在、"RAA 以源码形态在场且未经 UPM 注册"的判定被反转、`Resources` 基线资产没有创建。碰巧引用了这些前提的用例就会变红,而框架功能本身没坏。

判断标准请按这个顺序:

| 现象 | 结论 | 处理 |
|------|------|------|
| 包和测试程序集**编译通过**,仅少数用例因路径 / 环境假设失败 | 环境差异,功能正常 | 可忽略,或在 issue 里附用例名反馈 |
| 示例场景打不开、菜单报 Missing Script、功能在**你的工程里确实用不了** | 真缺陷 | 提 [GitHub Issues](https://github.com/yuumixcode/AesirFramework/issues),附复现步骤与 Unity 版本 |
| 编译就失败(CS 错误) | 依赖或版本问题 | 检查可选依赖安装情况与 Unity 版本,按错误信息排查 |

> 换句话说:测试"全绿"是**框架作者的开发门禁**,不是**消费者的验收标准**。你这边真正该确认的,是包能编译、示例能跑、需要的功能正常工作。

## 高频陷阱与修法

> 完整 10 条陷阱清单见 AI 技能 `aesir-pitfalls`(仓库根 `.codely-cli/extensions/AesirFrameworkGame/skills/aesir-pitfalls/SKILL.md`)。异常消息已含修复指引(fail-fast)。

### `GetModel<T>()` 抛"未注册"异常

按实现类注册、按接口获取(或反过来)—— **Register 与 Get 必须使用相同类型参数**(推荐都用接口)。异常消息含近失识别提示。

### 拿到了第二个 Context 实例 / 空引用

`Configure()` 与各模块 `OnInitialize()` 中**禁止访问 `Instance`** —— 会递归创建第二个上下文。模块间互相访问放到 `OnInitialize()`(此时全部已注册完毕)。

### 抛"该 Model 尚未初始化"

注册顺序错误或循环依赖 —— Configure 中**被依赖的模块先注册**(框架按注册顺序初始化 Model → Service)。

### 一个监听者抛异常后,后面的监听者不执行了

MiniEvent 是零分配直调,异常语义 = 原生 C# 事件(fail-fast)。**监听回调不应抛异常**(框架约定);业务异常在回调内部自行 try-catch。

### GameObject 销毁后仍收到事件 / 内存泄漏

`AddListener` 返回的 `AutoRemoveListenerHandle` 未绑定生命周期。一行修法:

```csharp
model.Count.AddListenerAndInvoke(OnCountChanged)
    .RemoveListenerWhenGameObjectOnDestroyed(gameObject);   // UI 面板用 RemoveListenerWhenGameObjectOnDisable
```

### 外部代码把我的事件链整体替换了(`view.Xxx = null`)

用 `public Action Xxx { get; set; }` 代替了 `event`。接口中声明 `event Action Xxx;` —— 编译期限制外部只能 `+=` / `-=`。

### 事件模块(EventModule)与 MiniEvent 怎么选?

跨模块解耦、按 4 档优先级编排订阅者、需要精确投递过滤器或 Inspector 配置(SO 资产化 / UnityEvent 桥接)时用 EventModule;单类型自包含事件、追求零开销直调时用 MiniEvent / ObservableValue。两者可共存 —— EventModule 负责发布-订阅调度,MiniEvent / ObservableValue 负责"事件实例"与"数据驱动"语义。

## 未找到答案?

前往[支持](support.md)或在 [GitHub Issues](https://github.com/yuumixcode/AesirFramework/issues) 提问(附复现步骤与 Unity 版本)。