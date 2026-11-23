# Aesir Architecture 快速开始

## 前置要求

- Unity 或团结引擎 **2022.3+**(开发环境:2022.3.62f3c1)
- Odin Inspector **可选**(3.3.x+):未安装时核心架构流程完整可用,仅排除 Inspector 增强功能

## 安装

### 方式 1:常驻 latest 分支(推荐)

Unity Package Manager → 左上角 `+` → `Add package from git URL...`:

```
https://github.com/yuumixcode/AesirFramework.git#AesirArchitecture-latest
```

`latest` 分支由 CI 自动按包目录 subtree split 滚动更新,分支名永久固定——Git URL 只需输入一次,后续升级无需修改。**升级**:Package Manager 不会对 Git URL 安装的包显示更新提示,移除旧包后用同一 URL 重新添加即可(或删除 `packages-lock.json` 中对应条目后重新解析)。需要钉死旧版本时改用 Release tag:`?path=Assets/Runestone/AesirArchitecture#v<版本>`(tag 永久保留)。

### 方式 2:unitypackage 导入 + 包内更新器(大陆 / 离线友好)

从 [GitHub Releases](https://github.com/yuumixcode/AesirFramework/releases) 下载 `AesirArchitecture-v<版本>.unitypackage`(或两包合并的 `AesirFramework-v<版本>.unitypackage`)导入。

以此方式安装的包位于 `Assets/Runestone/` 下(代码可改),**更新无需手动重新下载**:菜单 `Tools → Aesir → Check for Updates` 打开包内更新器,一键完成"检测新版本 → 静默导入 → 差集清理残留"。版本检测按「直连 GitHub → 镜像站 → CDN 中转」三层兜底,能直连 GitHub 即为 100% 最新;窗口会显示本次的连接状态、获取线路与各层尝试详情,仅在落到 CDN 中转时提示可能有数小时延迟。

!!! warning "更新器管辖范围"

    经 Git URL(UPM)安装的副本**不在更新器管辖内**——纯 UPM 安装形态下该菜单经 validate 整体隐藏;Package Manager 也不会对 Git URL 包显示更新提示,升级 = 移除旧包后用同一 Git URL 重新添加(`latest` 分支名永久固定)。开发仓库(存在 `.git`)切勿点更新。

### 方式 3:跟踪 main 最新(开发预览)

```
https://github.com/yuumixcode/AesirFramework.git?path=Assets/Runestone/AesirArchitecture
```

## 第一个 MVC 应用

第一课(快捷档)3 个脚本跑通数据闭环:Context + Model + View 兼 Controller 面板。

### 1. 定义 Context

```csharp
using Runestone.AesirArchitecture;

// [InternalContext]:标记为框架内部用途,使其不出现在用户工作流的 Context 选择器中
//(如 AesirModules Binder 的「Context 类型」下拉)。业务项目通常**不加**该标记。
public class CounterContext : AbstractContext<CounterContext>
{
    protected override void Configure()
    {
        // 第一课按具体类注册(无接口抽象)
        RegisterModel(new CounterModel());
    }
}
```

### 2. 定义 Model

```csharp
[Serializable]
public sealed class CounterModel : AbstractModel
{
    // 私有字段 + 只读属性暴露可写 ObservableValue(快捷档 View 可直改,封装不倒退)
    [SerializeField] ObservableValue<int> count = new ObservableValue<int>(0);

    public ObservableValue<int> Count => count;
}
```

### 3. 定义面板(View 兼 Controller)

```csharp
public class CounterPanel : MonoViewController<CounterContext>
{
    [SerializeField] Text countText;
    [SerializeField] Button increaseButton;

    CounterModel _model;

    void Start()
    {
        // GetModel 缓存为字段,避免每次字典查找
        _model = this.GetModel<CounterModel>();
        // AddListenerAndInvoke:订阅并立即触发一次(拿到当前值);物体销毁自动退订
        _model.Count.AddListenerAndInvoke(UpdateCountText)
            .RemoveListenerWhenGameObjectOnDestroyed(gameObject);
    }

    void OnEnable() => increaseButton.onClick.AddListener(Increase);
    void OnDisable() => increaseButton.onClick.RemoveListener(Increase);

    // 快捷档特有写法:View 兼 Controller 直改 ObservableValue
    void Increase() => _model.Count.Value++;

    public void UpdateCountText(int count) => countText.text = count.ToString();
}
```

三个脚本挂上场景物体、按 Play 即可运行。完整示例见 `Counter-Mvc-Quick`。

### 进阶:标准档与严格档

标准档起 Model 收窄为**只读暴露 + 写方法**(View 拆出 `MonoView<T>` 与 Controller 分离);严格档进一步**按接口注册**、写入经 Command 分发、加工读取走 Query:

```csharp
public interface ICounterModel : IModel
{
    IReadOnlyObservableValue<int> Count { get; }
    void Increase();
    void Decrease();
}

public sealed class CounterModel : AbstractModel, ICounterModel
{
    [SerializeField] ObservableValue<int> count = new ObservableValue<int>(0);

    public IReadOnlyObservableValue<int> Count => count;
    public void Increase() => count.Value++;
    public void Decrease() => count.Value--;

    protected override void OnInitialize() { }
}

// 严格档:按接口注册(Register 与 Get 类型参数必须一致)
RegisterModel<ICounterModel>(new CounterModel());
```

```csharp
// 严格档的 View 改用 MonoView<T>(仅只读能力)并按业务窄接口持有纯 C# Controller
public class UICounterPanel : MonoView<CounterContext>
{
    [SerializeField] Text countText;
    [SerializeField] Button increaseButton;

    ICounterModel _model;
    CounterController _ctrl;

    void Start()
    {
        _model = this.GetModel<ICounterModel>();
        _model.Count.AddListenerAndInvoke(UpdateCountText)
            .RemoveListenerWhenGameObjectOnDestroyed(gameObject);
        _ctrl = new CounterController();
    }

    void OnEnable() => increaseButton.onClick.AddListener(_ctrl.Increase);
    void OnDisable() => increaseButton.onClick.RemoveListener(_ctrl.Increase);

    public void UpdateCountText(int count) => countText.text = count.ToString();
}
```

### Command / Query(严格档写入 / 加工读取)

```csharp
// 定义命令(写操作)
public class AddScoreCommand : AbstractCommand
{
    protected override void OnExecute()
    {
        var model = this.GetModel<IScoreModel>();
        model.AddScore(10);
    }
}

// Controller 中执行
this.ExecuteCommand<AddScoreCommand>();
```

## 导入示例

Package Manager → Aesir Architecture → **Samples** 标签页,按需导入:

| 示例 | 档次 | 亮点 |
|------|------|------|
| `Counter-Mvc-Quick / Standard / Strict` | MVC 三课 | View 兼 Controller 直改 → 只读暴露 + 写方法 → Command 写 + Query 读 |
| `Counter-Mvp-Quick / Standard / Strict` | MVP 三课 | 与 MVC 逐课同构,View 被动、Presenter 推送 |
| `MiniEvent` / `ObservableValue` | 工具类 | MiniEvent 用法 / Odin Drawer 演示(后者需 Odin) |
| `ObservableCollections` | 工具类 | 单轨变更通知 `AddListener` 与集合运算(ContextMenu 驱动) |
| `RuntimeInitializeLoadType` | 进阶 | 五个初始化时机的执行顺序与静态重置最佳实践(Editor-only) |
| `AesirTimer` | 工具类 | 延时 / 冷却 / 重复 / 暂停 + 只读查询面板与自定义 Inspector |

本仓库用户也可直接浏览包内 `Samples/` 目录;示例场景可运行,构建时自动剔除(整文件 `#if UNITY_EDITOR`)。完整游戏案例 `Assets/PlaneWar/`(纵版射击)位于仓库根而非包内,克隆仓库后直接打开 `Assets/PlaneWar/Scenes/PlaneWar.unity` 即可运行。

> Getting Started 窗口(Tools → Aesir → Getting Started)可一键导入:未导入示例点击右侧「导入 Sample」按钮,确认框含示例介绍与导入位置(可取消),确认后直接导入到 `Assets/Samples/` 并自动刷新示例清单。

## 下一步

- [特性一览](features.md) — 核心机制速览与选型决策
- [概览](index.md) — 架构角色与三档渐进路径
- [FAQ](../faq.md) — 高频问题与陷阱修法