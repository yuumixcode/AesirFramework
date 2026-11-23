---
description: "AesirFramework 是面向 Unity / 团结引擎 2022.3+ 的渐进式 MVC 架构框架与功能模块集合。"
hide:
  - navigation
  - toc
---

<div class="aesir-hero" markdown>

<div class="aesir-hero__badge">0.35.0 · Unity / 团结引擎 2022.3+</div>

<h1 class="aesir-hero__title">
<span class="aesir-hero__word">AesirFramework</span>
<span class="aesir-hero__claim">面向 Unity / 团结引擎的渐进式 MVC 架构</span>
</h1>

<p class="aesir-hero__lead">从一个简单的 <code>MonoBehaviour</code> 开始,按项目复杂度逐步引入 Context、Model、Service、Command / Query 与 View。AesirFramework 不替你重建 Unity,而是把 Unity 原生能力组织成清晰、可测试、可扩展的工作流。</p>

<div class="aesir-hero__actions" markdown>

[安装与升级](start/installation.md){ .md-button .md-button--primary }
[特性总览](product/features.md){ .md-button }
[访问 GitHub](https://github.com/yuumixcode/AesirFramework){ .md-button }

</div>

<div class="aesir-hero__meta" markdown>

<span>MIT License</span>
<span>Architecture + Modules 0.35.0</span>
<span>800+ EditMode 单元测试</span>
<span>核心零第三方依赖</span>

</div>

</div>

## 从这里开始

<div class="aesir-paths" markdown>

<a href="start/installation.md"><b>第一次接触,想先把包跑起来</b><span>安装与升级 → Architecture 快速开始</span><em>开始</em></a>
<a href="product/features.md"><b>想知道这套框架能做什么</b><span>特性总览 · 示例总览</span><em>产品</em></a>
<a href="modules/getting-started.md"><b>需要现成的 UI / 事件 / 音频 / 场景能力</b><span>Modules 快速开始 · UI 框架</span><em>Modules</em></a>
<a href="product/samples.md"><b>想比较不同复杂度的写法</b><span>计数器六档对照 —— 同一界面六种分层</span><em>示例</em></a>
<a href="architecture/scripting-api/index.md"><b>要查具体类型与成员</b><span>Scripting API 命名空间与成员参考</span><em>API</em></a>
<a href="product/releases.md"><b>关心版本、升级或兼容性</b><span>版本与下载 · 兼容性</span><em>版本</em></a>

</div>

## 安装

推荐在 Package Manager 中使用**常驻 `latest` 分支** —— 分支名永久固定,Git URL 一次输入持续更新(升级 = 移除后用同一 URL 重新添加)。打开 **Package Manager → `+` → Add package from git URL...**:

=== "Aesir Architecture"

    ```text
    https://github.com/yuumixcode/AesirFramework.git#AesirArchitecture-latest
    ```

=== "Aesir Modules"

    Modules 依赖 Architecture。安装 Modules 时请同时添加两个包:

    ```text
    https://github.com/yuumixcode/AesirFramework.git#AesirArchitecture-latest
    https://github.com/yuumixcode/AesirFramework.git#AesirModules-latest
    ```

=== "unitypackage"

    从 [GitHub Releases](https://github.com/yuumixcode/AesirFramework/releases) 下载 `.unitypackage`;完整组合包用 `AesirFramework-v<版本>.unitypackage`。

    导入后代码位于 `Assets/Runestone/`,可用 `Tools → Aesir → Check for Updates` 检测并更新;Git URL 安装请通过 Package Manager 管理。

钉住旧版本、只装 Modules 时的依赖补全、升级与卸载细节见[安装与升级](start/installation.md)。

## 双包组成

AesirFramework 由两个同号发布的包组成。先用 **Aesir Architecture** 建立项目核心逻辑,需要 UI、场景或事件能力时再添加 **Aesir Modules**。两个包可以独立理解,也可以组合使用。

<div class="aesir-package-grid" markdown>

<div class="aesir-package-card" markdown>

<span class="aesir-card__eyebrow">Core Architecture</span>

### Aesir Architecture

渐进式 MVC / MVP 架构核心。以纯 C# 架构根连接 Unity 的 `PlayerLoop`、`ScriptableObject` 与 `Editor API`,让 Model、Service、Command / Query 和 View 保持清晰边界。

- 三档渐进路径:快捷 → 标准 → 严格
- `ObservableValue`、`MiniEvent` 与生命周期能力
- 11 个可导入示例,包含 6 个计数器对照;另有仓库级完整游戏案例 PlaneWar(不随包分发)

[快速开始](architecture/getting-started.md){ .md-button .md-button--primary }
[特性一览](architecture/features.md){ .md-button }

</div>

<div class="aesir-package-card" markdown>

<span class="aesir-card__eyebrow">Optional Modules</span>

### Aesir Modules

建立在 Architecture 之上的功能模块集合:UI 框架、事件模块、音频模块、场景模块与脚本文档生成工具(需 Odin)。

- UI:四层 Canvas、面板生命周期与 Binder
- Events:Script 订阅事件系统(过滤器、取消传播、SO 资产化、诊断)
- Audio:SFX 轮询、BGM 淡入淡出与音量持久化
- Scene:场景加载、卸载与 `SceneAssetWrapper`
- 可选集成:Odin Inspector、Addressables、Input System

[快速开始](modules/getting-started.md){ .md-button .md-button--primary }
[特性一览](modules/features.md){ .md-button }

</div>

</div>

跨包的能力矩阵与明确的设计边界,见[特性总览](product/features.md)。

## 代码一瞥

最小 MVC 闭环(快捷档):Context 注册 Model,面板订阅 `ObservableValue` 完成数据驱动 UI —— 不建 Command、不建独立 Controller。

```csharp
// 1. Context:注册 Model(快捷档按具体类注册,不做接口抽象)
public sealed class CounterContext : AbstractContext<CounterContext>
{
    protected override void Configure() => RegisterModel(new CounterModel());
}

// 2. Model:可写 ObservableValue 直接暴露
public sealed class CounterModel : AbstractModel
{
    [SerializeField] public ObservableValue<int> count = new ObservableValue<int>(0);
}

// 3. 面板(View 兼 Controller):订阅刷新 + 按钮直改
public class CounterPanel : MonoViewController<CounterContext>
{
    [SerializeField] Text countText;
    [SerializeField] Button increaseButton;

    void Start()
    {
        var model = this.GetModel<CounterModel>();
        model.count.AddListenerAndInvoke(UpdateText)
             .RemoveListenerWhenGameObjectOnDestroyed(gameObject);
        increaseButton.onClick.AddListener(() => model.count.Value++);
    }

    void UpdateText(int count) => countText.text = count.ToString();
}
```

项目长大后,再按 [三档渐进路径](architecture/index.md) 逐步引入接口注册、只读暴露与 Command / Query —— 每档只加一个概念,不是推翻重写。完整可运行版本见[计数器六档对照示例](product/samples.md)。

## 为什么选择 Aesir

<div class="aesir-value-grid" markdown>

<div class="aesir-value-card" markdown>

**渐进式**

不要求新项目一次性接受完整框架。先写能运行的代码,再按复杂度引入接口组合、Context 和严格分层。

</div>

<div class="aesir-value-card" markdown>

**Unity 原生优先**

深度使用 PlayerLoop、序列化与编辑器能力,不搭建与引擎平行的运行时;静态状态显式重置,反复进出 Play Mode 无残留。

</div>

<div class="aesir-value-card" markdown>

**依赖边界清楚**

核心架构零第三方依赖;Odin、Addressables、Input System 经独立程序集与条件编译接入,未安装可选依赖不影响基础流程。

</div>

<div class="aesir-value-card" markdown>

**工程化交付**

800+ EditMode 单元测试随包验证;CI 自动维护常驻 latest 分支与 unitypackage 发布;示例构建期自动剔除,不占包体。

</div>

</div>

## 兼容性

| 项目 | 支持范围 |
| --- | --- |
| Unity / 团结引擎 | **2022.3+**(开发与验证环境:2022.3.62f3c1) |
| 渲染管线 | 架构层与管线无关;Modules 使用 uGUI |
| Odin Inspector | 可选;仅提供 Inspector / Binder / 文档生成等增强能力 |
| Addressables / Input System | 可选;安装后自动启用对应集成 |
| 许可证 | MIT |

完整的可选依赖矩阵与运行时约定见[兼容性](start/compatibility.md)。

## 文档地图

<div class="aesir-map-grid" markdown>

<div class="aesir-map-card" markdown>

<span class="aesir-card__eyebrow">产品</span>

### 特性 · 示例 · 版本

特性总览、示例总览、版本与下载、路线图、工具链与生态。

[进入产品文档 →](product/features.md){ .md-button }

</div>

<div class="aesir-map-card" markdown>

<span class="aesir-card__eyebrow">上手</span>

### 安装 · 兼容性 · FAQ

安装与升级、兼容性矩阵、常见问题。

[进入上手文档 →](start/installation.md){ .md-button }

</div>

<div class="aesir-map-card" markdown>

<span class="aesir-card__eyebrow">Architecture</span>

### 架构核心

架构概念、响应式与事件、可观察集合、包内更新器。

[进入架构文档 →](architecture/index.md){ .md-button }

</div>

<div class="aesir-map-card" markdown>

<span class="aesir-card__eyebrow">Modules</span>

### 功能模块

UI 框架、事件模块、音频模块、场景模块。

[进入模块文档 →](modules/index.md){ .md-button }

</div>

</div>

发现问题时,请附上 Unity / 团结引擎版本、AesirFramework 版本和最小复现步骤,前往 [GitHub Issues](https://github.com/yuumixcode/AesirFramework/issues) 反馈。
