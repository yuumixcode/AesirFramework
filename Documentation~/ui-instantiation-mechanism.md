# UIModule 实例化机制与一次"防御性编程过头"的复盘

> 2026-11-19：本文记录 UIModule 面板 / 窗口实例化方式的一次简化决策。
> 曾有"克隆停用模板 + 从模板实例化"的机制，经多轮质疑与实测后**确认是历史包袱，已移除**；
> 现改为最朴素的 `Instantiate → SetActive(false)`。
> 保留 `Resources.Load` 对象语义的实测与官方来源，因为它解释了"当初为什么踩坑"，且结论本身仍然成立。
> 基础用法见 [ui-module.md](ui-module.md)。

## 结论先行

- **现在的做法**：`Instantiate(prefab)` → 立即 `SetActive(false)` → 复位实例名 → 挂层 → `Initialize`（OnInit）→
  `Show`（OnShow 内 `SetActive(true)`）。
- **为此付出的代价**：克隆体创建瞬间若来源是 active，会跑一次 `Awake` / `OnEnable`。**这是可接受的**
  ——框架约定面板 / 窗口不写这两个原生回调，它们在本框架里是空的、幂等的。
- **曾有的机制（已移除）**：为了"克隆体创建时不触发
  Awake/OnEnable"，框架一度为每个预制体缓存一份停用模板克隆，再从模板实例化。这套机制收益为零、成本实实在在（每预制体常驻一份完整克隆），是防御性编程过头的产物。

## 为什么现在这样是对的：框架的生命周期约定

框架面板 / 窗口基类（`AesirBasePanel` / `AesirBaseWindow`，及其基类 `AesirMonoBehaviour`）**都没有 `Awake` /
`OnEnable`**。生命周期由 UIModule 显式驱动：

```
Instantiate → SetActive(false)（此刻空的 Awake/OnEnable 跑一轮，无副作用）
→ 挂到所属层 Canvas / UIRoot → 子树归入 UI 层(5)
→ 注册表登记 → Initialize()（OnInit，组件停用态）
→ Show(payload) → 基类 OnShow 内 gameObject.SetActive(true) → OnEnable 再跑（仍为空）
```

`OnInit` 取代 Awake 承担初始化、`OnShow` 承担刷新、`OnDestroy` 承担清理——这是框架对业务的约定，官方示例
`SampleUIHud` 也已按此规范写（`OnInit` 单例登记 + 字体、`OnShow` 刷新、`OnDestroy` 清理）。既然业务不依赖
Awake/OnEnable，它们跑几次都无所谓。

`SetActive(false)` 作用在**克隆体**上，不碰 `Resources.Load` 返回的共享资产对象——来源资产全程只读，这个好处在新旧两种做法里都成立。

## 当初为什么踩坑：框架的旧实现误改了共享资产

`CHANGELOG.md` 的 Fixed 条目记录了旧实现：`InstantiateInactive` 为了实现"停用态实例化"，用了
`prefab.SetActive(false) → Instantiate → 恢复`。这里的 `prefab` 可能来自 `Resources.Load` 的**资产对象本身**
——框架自己踩了"直接改共享资产"的坑。

注意踩坑主体是**框架自身**，不是业务：业务代码从不会拿 `Resources.Load` 的返回值当运行对象，标准流程永远是
`Load → Instantiate → 操作克隆体`。框架当时是为实现一个正当需求（停用态实例化）而误用了资产对象。

修正时的第一反应是"那就别碰资产，克隆一份停用模板，从模板实例化"——这解决了踩坑，但叠加了一层没必要的复杂度。直到维护者指出：
**需求的根源（避免 Awake/OnEnable 提前跑）在框架约定下根本不存在**，整套模板机制才被判定为历史包袱并移除。

## 被移除的机制与其真实代价

旧机制：`GetInactiveTemplate` 为每个预制体克隆一份停用模板，缓存在 `_inactiveTemplates` 字典、挂在恒停用的
`[UIModule]InactiveTemplates` 容器下；每次实例化走 `Instantiate(template)`。

- **收益（事后判定为零）**：克隆体从出生就是 inactive，Awake/OnEnable
  真正"一次都不提前跑"。但框架里这两个回调是空的——不提前跑与跑一轮空方法，对业务无差别。
- **成本（实实在在）**：
    - 每个实例化过的预制体常驻一份完整层级克隆体，存活至 UIModule 宿主销毁；
    - 模板持有来源预制体引用，不会被 `Resources.UnloadUnusedAssets` 回收；
    - 模板名带 `(Clone)`，从它克隆叠成 `Xxx(Clone)(Clone)`，为此又加了一层命名复位逻辑与回归测试。
- 用一份常驻克隆换"空回调不提前跑"——收益与成本严重不匹配。移除后直接 `Instantiate + SetActive(false)`，只剩一层
  `(Clone)`，复位为预制体名即可。

## 附：`Resources.Load` 对象语义（踩坑的原理，结论仍成立）

这部分解释"旧实现为什么踩坑"，其结论经 Unity 官方文档 + 编辑器实测确认，与当前简化做法无关但值得留存。

### 官方来源与实测

- Unity 官方脚本 API `Resources.Load`（<https://docs.unity3d.com/ScriptReference/Resources.Load.html>）：*"This
  method returns the asset at `path`…"*；官方示例即 `Instantiate(Resources.Load(...))`——Load 拿资产、Instantiate
  才造副本。
- Unity 员工 karl_jones
  在官方论坛（<https://discussions.unity.com/t/where-does-a-prefab-change-live-not-set-a-dirty/733053/6>
  ）确认：改工程加载的 prefab，改的是内存里的那个对象，*"written to disk when SaveAssets is called"*。
- Unity 官方《Safely edit a loaded
  asset》（<https://docs.unity3d.com/Packages/com.unity.addressables@1.21/manual/safely-edit-loaded-asset.html>
  ）：从 AssetBundle 加载 / Player 运行 / Use Existing Build 模式下，资产是内存副本、改动不影响磁盘；而编辑器默认
  Use Asset Database 模式直接读工程文件，改动会写回并跨会话保留。
- **编辑器实测**（`Tests/Editor/UI/ResourcesLoadAssetSemanticsTests.cs`，自包含 3 用例）：`Resources.Load` 与
  `AssetDatabase.LoadAssetAtPath` 返回**同一对象引用**；改内存同引用可见；但不 `SetDirty`/`SaveAssets` 时磁盘
  `.asset` 不变、对象也不自动标脏。

### 对你关心的层次逐条回答

1. **"缓存对象应与磁盘资产隔离"**：编辑器里 `Resources.Load` 缓存的就是 `AssetDatabase` 持有的资产对象本身（
   `ReferenceEquals=True`），不是隔离副本。玩家构建里则是打包数据的副本、改它纯内存且退出即丢。
2. **"运行时改缓存会影响编辑器资产吗"**：不保存不落盘（实测）；改动残留在内存版资产对象上，退出 Play 不还原，日后被
   `SaveAssets` / Inspector 标脏才写回。危害触发面很窄：日常开关 UI / 改位置改的是 `Instantiate`
   的场景实例、从不碰资产对象；只有"直接拿资产对象当实例改 + 改不该改的字段 +
   之后恰好保存"三条同时满足才翻车——所以这么用三年通常无感。
3. **Addressables 是否不同**：取决于 Play Mode Script。编辑器默认 Use Asset Database 直接读 AssetDatabase，与
   Resources 同类坑且官方称改动跨会话保留；只有 Use Existing Build / 玩家构建（真 AssetBundle）才是隔离副本。稳妥做法始终是
   **先 Instantiate 再改**。

## 决策记录：这份结论是怎么来的（开发过程实录）

> 保留完整质疑—核实—修正链，是为了让后续维护者理解"防御性编程是如何一步步叠加、又是如何被推翻的"。

1. **起因**：维护者问模板机制有什么好处、为何不直接实例化资产。初版文档列了三条理由。
2. **质疑一：空 Awake/OnEnable 提前触发有害吗？** 查源码确认基类无这两个回调，空方法提前触发无害。删掉该理由。
3. **质疑二：Resources.Load 的缓存对象直觉**。实测 + 官方来源：不保存不落盘，但编辑器里与 AssetDatabase
   是同一引用。纠正"立刻标脏"的夸大说法，加 `ResourcesLoadAssetSemanticsTests`。
4. **质疑三：危害多大 + Addressables**。危害评估成立（窄条件）；Addressables 取决于 Play Mode
   Script，默认模式同类坑。补"问题四"与危害评估。
5. **质疑四：这是在防"没人会犯的错"吗？** 厘清踩坑主体是框架旧实现（CHANGELOG L113），不是业务。
6. **最终裁决**：既然框架约定不写 Awake/OnEnable、OnInit/OnShow
   接管生命周期，"停用态实例化"这个需求本身就不存在。模板机制是防御性编程过头的产物——**移除**，改为直接
   `Instantiate → SetActive(false)`。

### 沉淀的原则

- 需求要追问到根源："避免 Awake/OnEnable 提前跑"的前提是业务依赖它们；框架约定已取消该依赖，需求随之消失。
- 防御性编程要有成本收益意识：一份常驻克隆换"空回调不提前跑"不划算。
- 踩坑主体要分清：框架自己的实现失误，不该固化成一层长期机制让所有调用方承担。
- 关键 Unity 行为以官方来源 + 编辑器实测为准；能实测就写用例让结果说话，不靠记忆或漂亮话维持结论。

## 相关

- 生命周期时序与 API：[ui-module.md](ui-module.md) 的"面板生命周期"章节。
- 该机制的引入与移除：包根 `CHANGELOG.md` UI 模块条目（`InstantiateInactive` 只读改造 → 本次移除模板机制）。
- 私有笔记仓 `Aesir-Notebook/docs/lessons/` 的复盘条目（2026-11-19）。
