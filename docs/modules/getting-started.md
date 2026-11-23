# Aesir Modules 快速开始

## 前置要求

- Unity / 团结引擎 2022.3+
- **Aesir Architecture**(必需依赖,两包同号发版,推荐同版本安装)
- Odin Inspector 可选(仅影响 Binder 绑定等增强功能)

## 安装

Unity Package Manager → `+` → `Add package from git URL...`:

```
https://github.com/yuumixcode/AesirFramework.git#AesirModules-latest
```

UPM 不支持在包内声明 Git URL 依赖(Unity 官方限制),本包不携带对 Aesir Architecture 的依赖声明——**两个包需要分别添加**;只添加本包也能安装成功,但核心程序集会因缺少 Aesir Architecture 编译失败,此时菜单 `Tools → Aesir → Modules → Install Dependencies` 会出现,一键补装:

```
https://github.com/yuumixcode/AesirFramework.git#AesirArchitecture-latest
```

或编辑 `Packages/manifest.json`:

```json
{
  "dependencies": {
    "cn.runestone.aesir.architecture": "https://github.com/yuumixcode/AesirFramework.git#AesirArchitecture-latest",
    "cn.runestone.aesir.modules": "https://github.com/yuumixcode/AesirFramework.git#AesirModules-latest"
  }
}
```

**升级**:Package Manager 不会对 Git URL 安装的包显示更新提示,但 `latest` 分支名永久固定——两条 URL 一次输入持续可用,升级 = 移除旧包后用同一 URL 重新添加,无需随发版修改。需要钉死旧版本时改用 Release tag(`?path=Assets/Runestone/<包目录>#v<版本>`,tag 永久保留)。

**unitypackage 方式**:从 [GitHub Releases](https://github.com/yuumixcode/AesirFramework/releases) 下载 `AesirModules-v<版本>.unitypackage`(注意不含依赖;或直接用两包合并的 `AesirFramework-v<版本>.unitypackage`)。只导入本包而缺 Aesir Architecture 时,菜单 `Tools → Aesir → Modules → Install Dependencies` 会出现,确认后经 UPM 自动补装(常驻 `latest` 分支最新版,装至 `Packages/` 下;依赖包在场时该菜单自动隐藏)。导入后经 `Tools → Aesir → Check for Updates` 一键更新。

## 第一个面板

### 1. 创建 UIRoot

菜单 `GameObject → Aesir Modules → Create UIRoot`,一键构建四层 Canvas(Background / Normal / Popup / Top)+ UICamera + EventSystem。层 Canvas / UICamera / EventSystem 为序列化引用持久化,构建后可自由调整。

### 2. 编写面板脚本

```csharp
using Runestone.AesirModules;

public class MainMenuPanel : AesirBasePanel
{
    protected override void OnInit() { }                 // 首次实例化后调用一次
    protected override void OnShow(object payload) { }   // 每次显示时调用(含首次)
    protected override void OnHide() { }                 // 隐藏时调用
    protected override void OnClose() { }                // 受控销毁(Hide + DestroyOnHide=true)前调用
}
```

制作面板预制体,根节点挂上面板脚本,在 Inspector 中设置 `layer`(层级)与 `destroyOnHide`(隐藏时销毁还是缓存复用)。

### 3. 注册并显示

```csharp
// 注册面板预制体
UIModule.RegisterPrefab<MainMenuPanel>(prefab);

// 显示面板
UIModule.Show<MainMenuPanel>();

// 带参数显示(强类型 payload)
UIModule.Show<ConfirmDialogPanel, ConfirmData>(new ConfirmData { message = "确定?" });

// 关闭(按面板的 DestroyOnHide 决定销毁或缓存复用)
UIModule.Hide<ConfirmDialogPanel>();

// 预热:预实例化并隐藏,首次 Show 直接复用,避免卡顿
UIModule.Prewarm<MainMenuPanel>();
```

> **生命周期细节**:面板以停用状态实例化(Awake / OnEnable 推迟到 Show 激活时才触发,保证 OnEnable 可安全访问 OnInit 之后才有值的引用),按 挂层 → `Initialize` → `Show` 顺序驱动;面板注册以实例的**实际类型**为键,以基类类型 Show 后需以实际类型(或面板内 `HideSelf()`)关闭。`OnClose` 仅在受控销毁路径调用,事件解绑请放 `OnDestroy`。

### 4. 自定义资源加载(可选)

默认从 Resources 目录加载(`ResourcesUILoader`,预制体路径约定为面板类型名)。实现 `IUIAssetLoader` 即可替换为其他同步可达方案(加载契约为同步语义):

```csharp
UIModule.Instance.RegisterAssetLoader(new MyAddressablesLoader());
```

## 导入示例

Package Manager → Aesir Modules → **Samples**:

| 示例 | 说明 |
|------|------|
| `Events/01_KeyPress` | 事件模块基本发布-订阅:按键发布事件、Script 订阅与句柄退订 |
| `Events/02_Filters` | 订阅者过滤器对照示例:`WithTag`+`InsideCollider2D` 双重过滤与 `OnlySelf` 家族命令 |
| `Events/03_SOAsset` | SO 资产化:事件资产配置载荷,`UnityEventOnAesirEvent` 零代码桥接 UnityEvent 回调 |
| `Events/04_Cancellation` | 取消传播:四档优先级订阅同一伤害事件,Space 观察执行链,Shift 对照不可取消事件 |
| `Events/05_Diagnostics` | 诊断 API:订阅 `TraceProduced` 实时展示事件链路快照 |
| `Audio/01_BasicUsage` | 音频模块基础用法:SFX 播放、BGM 淡入淡出切歌、三通道音量与静音持久化 |
| `UI/01_BasicUsage` | UI 模块基础用法:面板与 Canvas 根窗口协作、蒙版单遮/叠遮切换对照、点击蒙版关闭、全屏加载窗口 |

> Getting Started 窗口(Tools → Aesir → Getting Started)可一键导入:未导入示例点击右侧「导入 Sample」按钮,确认框含示例介绍与导入位置(可取消),确认后直接导入到 `Assets/Samples/` 并自动刷新示例清单。

## 下一步

- [特性一览](features.md) — 五模块与 Binder 速览
- [概览](index.md) — 模块总览与包结构