# Unity / 编辑器 / Bridge / 工具链坑位（索引条目：`unity-tooling`）

> 从主 `memory.md` 拆出的主题块。触发词：Bridge、编译冻结、域重载、条件编译、asmdef、Editor、Inspector、场景保存、tuanjie CLI、ScriptableSingleton、触发器、PlayerSettings。
> 与 `testing.md` 的分工：这里是"编辑器/脚本行为坑位"，那里是"测试用例怎么写与在哪跑"。

## 编译冻结 / 域重载卡死的诊断链

- [2026-11-08 02:40:00] 任一程序集编译失败（含 Assembly-CSharp 里的生成文件）→ **域不重载，全部程序集停在旧版本**（`isCompiling=started` 长期不落、磁盘 DLL 已新但反射找不到新成员、测试永远跑旧代码）。解法：`grep "error CS" Editor.log` 找到阻塞源修掉 → 按 **PID** 前置编辑器（进程名可能是 `Tuanjie`）→ `CompilationPipeline.RequestScriptCompilation()` → 等域重载 → 反射复验。**判定磁盘 DLL 是否已含新代码**用 python/strings 搜 DLL 内 UTF-16 断言文本（区分"没编译"与"没重载"）。**SIGILL（exit 132，编辑器无声消失）先读 `~/Library/Logs/DiagnosticReports/Unity-*.ips` 的 `exception.message`**：`excessive recursion` = 托管层无限递归爆栈（曾因属性 expression-body 自引用笔误），用 `public\s+\w[\w<>\[\],\s]*?\s(\w+)\s*=>\s*\1\s*;` 全仓扫。

## 条件编译（2022.3.62f3c1）

- [2026-09-29 15:59:48] ①**asmdef `versionDefines` 产生的宏只对声明它的程序集可见**——以某宏作 `defineConstraints` 的程序集必须各自再声明一份，否则被 Unity **静默排除**（零报错、用例凭空消失）。②`defineConstraints` 不满足时先排除程序集再解析 references，故胶水程序集可用 name 引用缺失包。③name 引用缺失程序集被静默忽略（非硬依赖）。④第三方程序集引用/检测一律用 **asmdef 名**而非命名空间名（UniTask 的 asmdef 名是 `UniTask`）。

## 编辑器窗口 / 脚本验证方法

- [2026-09-29 16:00:07] ①**同步收敛循环必须同步改变循环条件**（只置"下一帧才生效"的字段当退出条件 = 主线程死循环，曾卡死编辑器）。②`EditorWindow.SendEvent` 合成鼠标事件**不可靠** → 按钮命中只能真鼠标。③`[InitializeOnLoad*]` 里禁止直接 `AssetDatabase.CreateAsset` / 写脚本宏（重入报错），推迟到 `EditorApplication.delayCall`；**后台失焦时 delayCall 完全不派发** → 反射取 delayCall 表 DynamicInvoke 或前置编辑器。④`exec_editor_script` 内 `new GameObject()` / `DestroyImmediate` **不标脏场景**（要显式 MarkSceneDirty）；`Scene` 快照跨 OpenScene 失效（先存 path 字符串）。⑤脚本上下文同框坑：裸 `Object` 报 CS0104（写 `UnityEngine.Object.*`）、`ReferenceEquals` 写 `object.ReferenceEquals`、`Transform.Cast<>()` 需 `using System.Linq`、特性类名是 `UnityEditor.MenuItem`（无 Attribute 后缀）。⑥**窗口像素级验证**：多实例用 Swift `CGWindowListCopyWindowInfo` 按 PID 取 window number → `screencapture -x -o -l <id>`；文字亮度用 PIL 行扫描"最亮 1% 均值"（≈128 禁用 / ≈196 正常）；锁屏时截屏全黑。⑦临时探针类放 `Assets/Editor/`（编译进真实程序集、零反射开销），用完连 `.meta` 一起删。
- [2026-11-14 02:23:50] **Unity 场景保存 API 边界**：`EditorSceneManager.SaveOpenScenes()` 对**无资产路径的临时场景**（新建未保存 / 崩溃恢复出的 `Temp/__Backupscenes` 场景）无处可写，会弹出「Save Scene 另存为」面板强制命名（表现为点按钮莫名被模态窗口挡住）。需要"静默保存"时遍历 `SceneManager.GetSceneAt(i)`：有 path 的 `EditorSceneManager.SaveScene(scene)` 静默落盘、无 path 的交给 `SaveCurrentModifiedScenesIfUserWantsTo()`。Why: 崩溃恢复后场景就是无路径态。
- [2026-09-29 13:45:38] **`ScriptableSingleton<T>` 落盘**：`[FilePath]` + 每个持久字段 `[SerializeField]`；`instance` 小写。**判别法**："改脚本触发域重载看值是否保留"**无判别力**（进程内实例被 Unity 保留）——正解是看设置文件是否出现字段键 + 清私有静态 `s_Instance`（等价重启）后读回；反复清会产生原生孤实例（`ScriptableSingleton already exists`），用 `Resources.FindObjectsOfTypeAll<T>()` 销毁恢复。

## 引擎实测坑位（2022.3.62f3c1 中国版）

- [2026-09-06 16:14:34] ①两个 Trigger Collider2D 之间必须至少一个 Kinematic Rigidbody2D 才能触发 `OnTriggerEnter2D`。②`TextureImporter` 切片 API 是 `spritesheet` / `spritePixelsPerUnit`（不是文档里的 `sprites` / `spritePixelsToUnit`）。③`PlayerSettings.SetScriptingDefineSymbols` 值未变化时不落盘，变更也是延迟落盘 → 写宏后 grep `ProjectSettings.asset` 验证。④`BuildPlayerWindow.DefaultBuildMethods.BuildPlayer` 返回 void；`RegisterBuildPlayerHandler` 是覆盖式注册（域重载清空，可重复注册）。⑤`SetScriptingDefineSymbols` 对废弃 BuildTargetGroup 抛异常，枚举清理需 try/catch。⑥编辑器进程名可能是 `Tuanjie`，`osascript activate "Unity"` 静默无效。
- [2026-11-13 17:45:11] **Unity Inspector 两个硬事实**：①原生 Inspector 不为不可序列化类型创建 SerializedProperty，`[CustomPropertyDrawer(typeof(普通引用类))]` 对 `public MyClass field;` **永远不会被调用**——字段必须标 `[SerializeReference]`，Odin 侧则需宿主是 SerializedMonoBehaviour / SerializedScriptableObject 或字段标 `[ShowInInspector]`。②`SerializedProperty.managedReferenceValue` 的 **getter 会为 null 引用即时实例化对象**（每重绘多一个实例）→ 绘制器里取值改用 `PropertyDrawer.fieldInfo.GetValue(property.serializedObject.targetObject)`（仅 depth 0 可靠）。
- [2026-11-14 02:42:53] 本引擎 `UnityEditor.Editor.OnInspectorUpdate()` **不是 virtual**，`public override void OnInspectorUpdate()` 报 `CS0115`。How to apply: 自定义 Inspector 要周期重绘走 `OnEnable` 里 `EditorApplication.update += ...` + `EditorApplication.timeSinceStartup` 自己门控（示例 0.1s ≈ 10 Hz），`OnDisable` 退订；编辑模式下直接不重绘。
- [2026-11-14 02:45] 示例（Samples/）里新增编辑器脚本必须放进**该示例 asmdef 覆盖的文件夹**（现有约定 `Samples/<示例>/Scripts/`），不要新开同级的 `Editor/`：asmdef 在 `Scripts/` 只是旁系不是祖先，同级 `Editor/` 不归属任何 asmdef，而 Unity 仍当特殊目录 → 脚本落进 `Assembly-CSharp-Editor`，示例 asmdef 是 `autoReferenced: false` → `CS0246`。**顺带成立**：运行时 asmdef 里写 `using UnityEditor` + 整文件 `#if UNITY_EDITOR` 可行（`Runtime/UI/UIRoot.cs` 有先例），`[CustomEditor]` 放运行时示例程序集里能被识别。
- [2026-11-13 18:26:47] 新增 RAA 示例的实测做法：①**场景别手写 YAML**——复制可用示例场景改 `m_Name` + `m_Script` 的 guid（取自 `.cs.meta`）即可；asmdef 的 meta 必须是 `AssemblyDefinitionImporter` 段（手写 MonoImporter 时 Unity 不修正）。②**冒烟验证**：`Tests/Runtime` 不引用示例程序集，用反射 `Type.GetType("...XxxSample, ...Samples.Xxx")` + `AddComponent(type)` 触发 Awake，`LogAssert.Expect(LogType.Log, new Regex(...))` 等首个回调，跑完删临时用例。③batchmode 下 OnGUI 不产生 GUI 事件，面板绘制无法验证（别把"冒烟通过"说成"界面已验证"）。④运行时 `GUI` 没有 `DrawRect`（那是 EditorGUI 的），自绘色块用 `GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0f, color, 0f, 0f)`。

## tuanjie CLI / Bridge

- [2026-11-14 02:42:53] ①`tuanjie command eval` 默认 `--mode editor`，**会顺手退出 Play 模式**（返回 `mode_switched:true`）→ 要在 Play 中探针（读运行时静态、组件实例、自定义 Inspector）必须加 `--mode runtime`。②编辑器开着时 `Unity -batchmode -quit` 直接失败 "Multiple Unity instances cannot open the same project"，改用 `tuanjie command editor compile` 对着运行中的编辑器编译；判断是否被占用看 `lsof Temp/UnityLockfile`（`ps ax | grep Unity` 会被 Hub 进程挤掉误判）。
- [2026-09-06 12:21:57] Codely Bridge / 工具链坑位：①`unity_editor.refresh` 在域重载前会 SaveScene 当前场景（刷新前确保临时对象已删）；无编译时 refresh 不保存，需显式 SaveScene。②脚本上下文**不含 `UNITY_EDITOR` 宏**（`[Conditional]` 方法被整体剔除，看起来像被测代码 bug）→ 用反射或内联等价逻辑。③脚本上下文 API 面比真实引擎窄（`Material.GetPropertyCount`、`TMP_TextInfo.glyphCount` 不可用）→ 读材质用 `GetFloat/GetColor/GetTexture/HasProperty`，改属性走 SerializedObject 或改 YAML 后 refresh。④引用本会话新编译的 asmdef 类型会 CS0246 → `AppDomain.CurrentDomain.GetAssemblies()` 解析 Type + 反射/AddComponent(Type)。⑤Bridge refresh 偶发打印 `currentFileSystemTime.ticks` Assert，与项目代码无关。⑥Codely 轮换 Bridge 版本后 refresh 报 TJGenerators 大量 CS0246 → 反射调 `Client.Resolve()` 再 refresh。⑦长驻进程必须 `run_in_background`。

## 目标铁律（编辑器侧）

- [2026-11-18 03:56:05] **AesirFramework 开发仓库是 Unity 2022.3 最新 LTS（`2022.3.62f3c1`，f 版），不是 Tuanjie 引擎版本**。禁止把项目仓库变成 Tuanjie 项目仓库。**识别标志**：ProjectVersion 的 `t` 后缀、`m_TuanjieEditorVersion` 字段、manifest 里 `-t1` 包、`yousandi.cn` YAML tag、`packages.tuanjie.cn` 源、ProjectSettings 下 tuanjie 专属 Settings 资产。Why: Tuanjie 定制包在 Unity LTS 上无法解析（batchmode 报 com.unity.timeline@1.8.13-t1 cannot be found）。
- [2026-11-18 03:56:15] **删除必须走废纸篓，保证可回退**（用户明确）：任何删除（文件/目录）都不能 `rm`/`rm -rf` 永久删除，必须用可恢复方式（macOS `osascript -e 'tell application "Finder" to delete POSIX file "..."'` 或 `trash`）。Why: 无 Odin 验证中 `rm -rf Assets/Plugins/Sirenix` 误删完整付费 Odin（gitignored 无备份），虽从其它项目同版本恢复但差点不可逆。How to apply: 验证脚本里的 `rm -f` 也改；嵌套/同名目录重命名先确认目标不存在再 mv。
