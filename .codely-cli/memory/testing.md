# 测试 / 门禁 / batchmode（索引条目：`testing`）

> 从主 `memory.md` 拆出的主题块。触发词：测试、门禁、EditMode、PlayMode、batchmode、TestRunnerApi、burst、nunit、测试程序集。
> 门禁基线数字在 `version-release.md`；本文件是测试方法与时机的全文真源。

## 执行硬约束（规范本身见 CODELY.md「测试规范」）

- [2026-11-08 02:40:00] ①EditMode 测试内**禁止写脚本宏**（触发域重载 → `Unexpected assembly reload happened while running tests`，整轮丢失），只测 `ContainsSymbol/AddSymbol/RemoveSymbol`。②本引擎 nunit 被裁剪：**无 `Assert.ThrowsAsync`**，`async Task` 用例可能被静默忽略 → 同步断言 + `GetAwaiter().GetResult()`。③PlayMode 门禁必须**退出编辑器走 batchmode**（编辑器内 UTF 1.1.33 会误入 PlayerLauncher）：全绿 exit 0、失败 exit 2，需解析 XML。④日志断言用 `new Regex(...)`。⑤测试程序集命名 = `Runestone.<包>.Tests` + `.Editor` +（模块专属）`.<模块>`；依赖 Odin 的用例集中在 `*.Tests.Editor.OdinInspector`，基线程序集不得加 `ODIN_INSPECTOR` 门控；`overrideReferences: true` 时显式列 `nunit.framework.dll`。
- [2026-11-20 11:20:24] **测试程序集允许并应保留 Odin（Sirenix.*.dll）预编译引用**：测试只在本仓开发用、不随包分发（unitypackage 整目录剔除 Tests/），故基线测试 asmdef 不受"运行时/发布产物零 Odin 依赖"约束。Why: fdc5724b 误删 4 个测试 asmdef 的 Sirenix 引用后，Odin 环境下 AesirMonoBehaviour 基类链指向 SerializedMonoBehaviour，测试一碰即 CS0012；指向不存在程序集的预编译引用被 Unity 静默忽略，故保留引用在有/无 Odin 下都成立。How to apply: 清理 Odin 依赖时不要动 Tests/ 下 asmdef 的 Sirenix 引用；每处 3~4 条 Sirenix.*.dll。
- [2026-11-19 11:49:01] 用户允许为跑 batchmode 测试关闭当前编辑器：关闭前先经 `tuanjie command eval` 遍历所有已加载场景确认 `isDirty=false`，再 kill 编辑器进程；测试跑完不要自动重开编辑器。Why: 同项目不能双开 batchmode，用户明确选择此方式；场景脏时强关会丢改动。How to apply: 后续本仓需要 EditMode/PlayMode 门禁时可直接按此流程执行，不必重复询问；但一旦发现场景标脏仍须先问用户。

## 编辑器内跑 TestRunnerApi（非 batchmode 路径）

- [2026-09-25 22:27:54] `Filter.testMode` 必须显式设（否则静默不跑）；改完测试先 `unity_editor.refresh`（否则跑旧 DLL）；**别 `await RunFinished`**（会超时）→ 回调写 `SessionState`、另一脚本轮询；每次新建后 `UnregisterCallbacks`（回调跨 submission 累积），同会话多次 `Execute` 会互相取消。
- [2026-11-16 18:27:41] **本仓可用路径（与上面旧结论冲突，以本机现版本为准）**：编辑器开着时无法 batchmode（同项目不可双开）→ 驱动已运行的编辑器 Bridge：`/Applications/Tuanjie Cowork.app/Contents/MacOS/hub/tuanjie command <cmd> --project-path .`（status / editor refresh / editor compile / editor compile-summary / read_console get / execute_csharp_script --script_path xx.cs）。`execute_csharp_script` 支持「顶层语句 + 尾部 class 声明」，能跑 EditMode 全量：`ScriptableObject.CreateInstance<TestRunnerApi>()` + `Filter { testMode = EditMode, assemblyNames = [...] }` + `ICallbacks` 写 PassCount/FailCount 与失败明细到 `SessionState`，再用另一次 execute_csharp_script 轮询读回（实测 849 例 ~45 秒）——**与全局记忆「Bridge 内 TestRunnerApi 不推进」相反**，旧结论疑与上下文/Filter 形态有关，本机现版本可正常推进。附：外部删 Assets 子目录留 `.meta` 时 refresh 会报 "A meta data file exists but its folder can't be found, and has been created" 并重建空目录，先确认是并发会话动的，不是代码问题。

## batchmode 细节坑位

- [2026-10-17 13:42:06] 最小临时项目（E2E）：`-projectPath` 必须真实路径（macOS `/tmp` 是符号链接 → `Couldn't set project path to: (空)`）；必须有空 `Assets/` 与完整两行 `ProjectVersion.txt`；RAM 项目补 `com.unity.ugui` / `com.unity.modules.audio` / `com.unity.modules.physics2d`；有编译错误时 `-executeMethod` 被阻断；`-runTests` 与 `-quit` 互斥；`-logFile` 勿放项目 `Temp/`；同项目不可双开。离线可用 `~/.codely-cli/tmp/bin/dotnet` build 根 csproj 预检，但**Unity batchmode 才是真值**。
- [2026-11-13 17:45:15] batchmode PlayMode 测试的等待不能按"帧数预算"写：无 vsync 的 headless PlayMode 帧率可达数千 fps，`MaxWaitFrames = 300` 换算成真实时间可能不足 0.1 秒 → **正确的实现被判失败**（AesirTimer 两个倒计时用例 Expected 1 But was 0）。解法：`var deadline = Time.realtimeSinceStartup + 5f; while (!cond && Time.realtimeSinceStartup < deadline) yield return null;`——有界等待但与帧率解耦。Why: 按帧等待在编辑器里过、batchmode 里大概率不过。

## 消费者工程验证（L3，非单元测试目标）

- [2026-11-20 12:06:08] 包内测试在消费者工程（UPM + testables）"全绿"不是单元测试目标：testables 只保证消费者能编译并运行框架用例，而用例按开发仓库形态写（RAA 在 Assets/Runestone、未经 UPM 注册、有 Assets/Resources 基线资产、装了 Odin/UniTask），UPM 消费者这些前提全不成立。Why: 此前误把 L1 契约测试与 L3 消费者冒烟混为一谈。How to apply: 消费者工程硬指标只有编译通过 + 16 场景/15 菜单能用；失败按三问分型（环境假设=测试过拟合 / 真实功能不可用=缺陷 / 可选依赖缺失=编译或 API 问题），不为全绿扭曲实现；决策树与矩阵见 Aesir-Notebook `docs/engineering/package-validation-matrix.md` §5。
