# UI 独立场景 + 叠加加载与 DDOL 取舍

> `../SKILL.md` 的展开参考：中大型项目希望 UI 生命周期随场景回收时读本文。这是主动配置方案，不是框架默认。

## 推荐方案：UI 独立场景 + 叠加加载（非默认，需主动配置）

框架的 `dontDestroyOnLoad` **默认全部为 true**，保证零配置开箱即用（UIModule / UIRoot 常驻跨场景）。中大型项目若希望 UI 生命周期随场景回收，采用如下**推荐**结构——这是主动配置，不是框架默认：

| 类别 | 典型类型 | DDOL 设置 | 理由 |
|---|---|---|---|
| 全局物体 | `AesirArchitecture` / `AesirModules` / `AudioModule` / `SceneModule` / `EventModule` | **保持默认 true** | 它们是进程级单例（配置、事件总线、音频、场景调度），本就该跨场景常驻 |
| UI 宿主 | `UIModule` + `UIRoot` | 预放置为**独立 UI 场景根物体**，关掉各自的 `dontDestroyOnLoad` | UI 场景可 `UnloadScene` 卸载；UI 子树随场景一次性回收，不留孤儿面板 |

要点：

- **UI 独立场景**（如 `UIScene.unity`）只放 UIModule / UIRoot 与其层级 Canvas，作为根物体预放置；两个组件的 `dontDestroyOnLoad` 取消勾选。以 `LoadSceneAdditive` 叠加载入，`UnloadScene` 卸载即完成整套 UI 回收。
- **运行时自动创建的实例仍跟随 `[Aesir Modules]` 宿主 DDOL**（此时字段不参与判断）——只有预放置为根物体时该字段才生效。所以"UI 场景可卸载"要求**主动预放置并关字段**，不是改框架默认。
- 面板/窗口实例挂在 UIRoot 层 Canvas 下，随 UI 场景卸载一并销毁；注册表随 UIModule 实例重建而清零，天然规避"旧场景孤儿面板"问题（见坑位 9/12）。卸载 UI 场景前若需保留数据，把状态放在 DDOL 的 Model（`AbstractContext` 纯 C# 单例）里，不放在面板字段。

