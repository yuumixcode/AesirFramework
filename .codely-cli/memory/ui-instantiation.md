# UI 实例化机制 / Resources.Load 语义（索引条目：`ui-instantiation`）

> 从主 `memory.md` 拆出的主题块。触发词：UIModule、停用模板、Instantiate、Resources.Load、资产语义、Addressables、(Clone)、DDOL、UI 场景叠加。
> 该机制最终**已移除**（2026-11-19 裁决）；本块记录完整质疑—推翻—落地过程和 Unity 资产语义定论，解释"为什么移除"，避免日后重复论证。

## 最终定论（先看这里）

- [2026-11-19 11:59:40] **已落地简化：移除 UIModule 停用模板机制**。用户裁决"框架约定不写 Awake/OnEnable，模板收益为零、是防御性编程过头"，已执行：UIModule.InstantiateInactive 改为直接 `Instantiate(prefab) → SetActive(false) → 复位名`；删 `_inactiveTemplates/_templateContainer/GetInactiveTemplate/InstantiateWithCleanName`。测试/文档/CHANGELOG 全部同步（文档 `ui-instantiation-mechanism.md` 重写为决策复盘）。踩坑点记入 notebook `docs/lessons/defensive-instantiation-overengineering.md`。门禁全绿：编译 0 error、UIModuleTests 22/22、UIModuleWindowTests 26/26、ResourcesLoadAssetSemanticsTests 3/3、zensical build --strict 0 issue。**How to apply:** 下次遇到"某机制为什么要这样"先质疑需求本身是否在约定下成立，别默认机制合理去圆理由。
- [2026-11-19 02:47:51] (Clone)(Clone) 命名修复 + DDOL 裁决：①根因 = 旧 InstantiateInactive 先从预制体克隆停用模板（名 Xxx(Clone)）再克隆实例（叠成 Xxx(Clone)(Clone)），修法曾新增 InstantiateWithCleanName（现随机制移除）。②用户明确：**框架 DDOL 默认值全部保持 true 不变**（零配置开箱即用是底线），"UI 独立 UIScene + UIModule/UIRoot 关 DDOL + 叠加加载/卸载"只是推荐方案、须主动预放置配置；全局物体（AesirArchitecture/AesirModules/AudioModule/SceneModule/EventModule）保持 DDOL。已写入 aesir-ui 技能与 `Documentation~/ui-module.md`。Why: 用户否定"把默认改成非 DDOL"的提议。How to apply: 后续别改默认 DDOL 值。

## Resources.Load / Addressables 资产语义（实测定论）

- [2026-11-19 11:35:52] **Resources.Load 资产语义实测（纠正此前所有夸大说法）**：Bridge eval 在编辑器（2022.3.62f3c1，非 Play）对 UIModuleConfig.asset 四条断言全 PASS：①Resources.Load 与 AssetDatabase.LoadAssetAtPath 返回**同一对象引用**（ReferenceEquals=True，非隔离副本）；②改该对象字段，AssetDatabase 入口同引用立即读到新值；③脚本直接改字段**不自动 SetDirty**（IsDirty 改前改后都 False）；④不 SetDirty/SaveAssets 只 Refresh，**磁盘 .asset 不变**，ImportAsset(ForceUpdate) 后仍返回同一内存对象。结论：**用户理解在"磁盘/玩家构建"层面正确**——不保存不落盘，Player 构建资产只读。真正风险 = 内存版共享资产对象残留、退出 Play 不像场景对象那样还原，日后被 SaveAssets/Inspector 标脏才写回。新增 `Tests/Editor/UI/ResourcesLoadAssetSemanticsTests.cs`（3 用例，TearDown 强制还原+SaveAssets）。**How to apply:** 再谈此机制务必区分内存对象 vs 磁盘资产、编辑器 Play vs 玩家构建，别再说"改 Resources.Load 立刻标脏资产"。
- [2026-11-19 11:45:05] 危害评估 + Addressables：①用户"危害很小、三年没感觉到"分析成立：日常开关 UI/改位置改的是 Instantiate 的场景实例（退出 Play 还原、从不碰资产对象），只有直接改 Resources.Load 返回值才踩坑。②Addressables 不是"和 Resources 完全不同"——官方《Safely edit a loaded asset》逐字：编辑器默认 Use Asset Database（Play Mode Script 默认 BuildScriptFastMode）直接读 AssetDatabase、与 Resources 同类坑且改动跨会话保留；只有 Use Existing Build/玩家构建（真 AssetBundle）才是隔离副本。稳妥做法无论哪种都先 Instantiate 再改。③`ResourcesLoadAssetSemanticsTests.cs` 改为自包含（SetUp 在 Assets/Resources/AesirResourcesLoadProbe/ 建临时 SO、TearDown 删）。

## 质疑—推翻全过程（历史留档，日常无需通读）

- [2026-11-19 11:25:13] 新增 RAM 文档 `Documentation~/ui-instantiation-mechanism.md` 解释旧停用模板机制；**三轮质疑最终定论**：①AesirBasePanel/Window/AesirMonoBehaviour 均无 Awake/OnEnable（生命周期走 UIModule 显式 Initialize/Show + OnInit/OnShow 虚方法），"空实现提前触发无害——不作为机制理由"。②机制真正两条动机：来源预制体只读 + Prewarm 需天然停用态（避免 OnEnable 往返）；联网核实官方 Resources.Load 语义（见上）后精确表述：机制收益 = 从源头避免改动落到共享资产内存对象上，断绝日后被标脏写回的可能；停用克隆不跑 Awake/OnEnable 由官方 Object.Instantiate 文档坐实。③"模板克隆不跑 Awake/OnEnable"是 Unity 既有语义，解释了为何"先 Instantiate active 再 SetActive(false)"不等价。④用户裁决：业务在 Awake/OnEnable 写逻辑 = 案例自身错误，应修示例而非迁就；已把官方 SampleUIHud（Samples/ 与 Samples~/ 双目录）从 OnEnable/OnDisable 改为 OnInit/OnShow/OnDestroy。
- [2026-11-19 11:46:34] 第四次质疑定位澄清：用户在业务侧判断对（业务代码都是 Load→Instantiate→操作克隆体，无人直接改资产对象），但机制的踩坑主体是**框架自己**——旧 InstantiateInactive 为了实现停用态实例化写了 `prefab.SetActive(false) → Instantiate → 恢复`，而 prefab 来自 Resources.Load，正是"直接操作资产对象"。所以模板机制不是防业务误用，是框架自身换的正解。How to apply: 解释此机制别定位成"防用户误操作"，要讲清踩坑主体是框架旧实现。
