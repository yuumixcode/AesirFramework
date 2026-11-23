# Tasks: add-save-module

依据：`specs/save-module/spec.md`（行为契约）、`design.md`（D1–D12 技术决策）。
实施纪律：中文 XML 注释、`AesirModulesDebug` 日志门面、静态字段 `SubsystemRegistration` 重置、
运行时状态字段显式非序列化、DDOL 字段为类体第一个字段（RAM 模块惯例，见 design Context）。

## 1. 核心契约与内部件

- [x] 1.1 写五个扩展点接口 `ISaveSerializer` / `ISaveStorage` / `ISaveManager` / `ISaveTransform` / `ISaveMigrator<T>`（形状按 design D1/D5/D6/D7；中文 XML 注释写明契约语义，尤其存储的"提交式写入"与 `ReadBackup`）。验证：编译预热 0 error。
- [x] 1.2 写结果类型 `SaveResult` / `LoadResult<T>` / `LoadStatus` / `LoadSource`（design D4 语义：Success/NoSave/Corrupted/VersionMismatch/SerializerMissing × Primary/Backup）。验证：编译 0 error。
- [x] 1.3 写 internal 件：`SaveEnvelope`（D3 二进制头读写 + 校验链）、`Crc32`（IEEE 表驱动）、`SaveKey`（白名单校验，非法抛 ArgumentException）。验证：编译 0 error；1.4 的往返用例覆盖。
- [x] 1.4 写信封与键的 EditMode 用例（`Tests/Editor/Save/`，Temp/ 目录）：信封头往返、位翻转→校验失败、长度不自洽→损坏、非法键拒绝（spec「完整性校验」「存档键合法性」）。验证：EditMode 过滤跑绿。

## 2. 默认存储 FileSaveStorage

- [x] 2.1 实现 `FileSaveStorage`：D2 五步事务写入（temp→Flush(true)→删旧 bak→主轮转 bak→原子 rename）、`Read`/`ReadBackup`/`Delete`/`Exists`/`GetKeys`（枚举 *.sav 排除 tmp/bak）、存储根懒创建、`backupEnabled=false` 分支。验证：编译 0 error。
- [x] 2.2 写事务崩溃点 EditMode 用例：手工构造 D2 表各磁盘状态（tmp 残留 / 主缺 bak 在 / 主坏 bak 好 / 主备均坏），断言加载行为与"加载无磁盘写副作用"（spec「事务式写入」「完整性校验与损坏回退」全部场景）。验证：EditMode 过滤跑绿。

## 3. 默认序列化器与变换链

- [x] 3.1 实现 `UnityJsonSerializer`（Id "unity-json"，JsonUtility + UTF8）。验证：编译 0 error。
- [x] 3.2 实现 `GZipSaveTransform`（GZipStream）与 `AesSaveTransform`（SHA256 派生密钥 + 随机 IV 前置 16 字节，design D6）。验证：编译 0 error。
- [x] 3.3 写序列化与变换 EditMode 用例：UnityJson 往返（List/Vector3 公开字段）、GZip/AES 往返、错误密钥→失败、默认无变换时 payload 为原始序列化字节（spec「三轨序列化」默认轨 +「可选字节变换链」全部场景）。验证：EditMode 过滤跑绿。

## 4. 默认管理器 TransactionalSaveManager

- [x] 4.1 实现 `TransactionalSaveManager`：构造注入序列化器注册表/存储/变换链；Save 编排（序列化→变换链→信封→存储提交）；Load 编排（读主→校验→按信封 Id 还原序列化器与变换链→版本比对/迁移→失败回退备份重走校验链）；IO 异常转失败结果 + LogError，编程错误抛异常（D4）。验证：编译 0 error。
- [x] 4.2 写管理器 EditMode 用例：按档 Id 还原（切换默认序列化器后旧档仍可读）、未注册序列化器→SerializerMissing、版本不匹配无迁移器→VersionMismatch、有迁移器→升级成功且不回写磁盘、主坏回退 Source=Backup（spec「序列化器标识」「数据版本与迁移」全部场景）。验证：EditMode 过滤跑绿。

## 5. 门面 SaveModule 与配置

- [x] 5.1 实现 `SaveModule`（AudioModule 同构：Instance/Ready/TryGetExisting、DDOL 字段、SubsystemRegistration 重置、静态门面 Save/Load/Delete/Exists/GetKeys/RegisterSerializer/SetStorage/SetManager/SetTransformChain/ApplyConfig；Delete/Exists/GetKeys 直通存储）。验证：编译 0 error。
- [x] 5.2 实现自动保存：`SetAutoSave(Action, interval)` 复用 `AesirTimer.Every(unscaled: true)`、`ClearAutoSave`、`OnApplicationPause(true)`/`OnApplicationQuit` 钩子（配置开关）。验证：编译 0 error；PlayMode 用例在 9.2 覆盖。
- [x] 5.3 写 `SaveConfigSO`（D11 五字段，AesirScriptableObject 基类 + CreateAssetMenu）与内存兜底载入。验证：编译 0 error；无资产运行用例在 9.1 覆盖。
- [x] 5.4 写门面 EditMode 用例：无配置资产正常往返、自定义 ISaveManager 接管、自定义 ISaveSerializer 注册往返、SetStorage 换内存存储后文件系统零产出（spec「保存管理扩展点」「配置资产与内存兜底」场景）。验证：EditMode 过滤跑绿。

## 6. Odin 序列化适配器

- [x] 6.1 核对宿主 `Runestone.AesirModules.OdinInspector.asmdef` 的 precompiledReferences 含 Sirenix.Serialization.dll（缺则补，design D9 核对项）；写 `Runtime/Save/OdinInspector/OdinSaveSerializer.cs`（SerializationUtility Binary 往返）+ asmref。验证：编译 0 error（ODIN_INSPECTOR 开启态）。
- [x] 6.2 写 Odin 轨 EditMode 用例（`Tests/Editor/OdinInspector/Save/`）：Dictionary<string,int> + 多态派生类字段往返（spec「Odin 序列化器往返字典与多态」场景）。验证：EditMode 过滤跑绿。

## 7. Newtonsoft 可选集成

- [x] 7.1 写 `Runtime/Integration/Newtonsoft/NewtonsoftJsonSerializer.cs` + `Runestone.AesirModules.Newtonsoft.asmdef`（versionDefines + defineConstraints 双声明 AESIR_MODULES_NEWTONSOFT，design D9）。验证：本仓未装包→编译预热 0 error 且该程序集被剔除（Library/ScriptAssemblies 无对应 DLL）；asmdef JSON 与 UniTask 集成逐字段同构核对。

## 8. Editor 集成

- [x] 8.1 写 `Editor/Save/SaveModuleConfigAssetInitializer.cs`（幂等创建，参照 SceneModuleConfigAssetInitializer）并接入 `ModuleConfigAssetsMenuItem.CreateConfigAssets`（追加一行）。验证：编译 0 error；菜单执行两次资产只一份（EditMode 用例或编辑器手动核对留截图证据于 PR）。
- [x] 8.2 SaveConfigSO 原生 Inspector 兜底（无 Odin 时可读可改，参照 SceneModuleConfigSO 的 Drawer 口径；Odin 侧 AttributeProcessor 如需要则 internal sealed 同文件）。验证：无 Odin 编译态 Inspector 正常绘制（手动核对）。

## 9. 测试补全与生命周期

- [x] 9.1 EditMode 收口：配置菜单幂等、GetKeys 枚举正确性、Delete 不存在键无副作用；全模块 EditMode 过滤跑一遍确认无遗漏场景（对照 spec 逐条勾）。验证：`Unity -batchmode -testPlatform editmode -testFilter Runestone.AesirModules.Tests.Editor` 相关用例全绿。
- [x] 9.2 PlayMode 用例（`Tests/Runtime/Save/`）：自动保存周期触发、OnApplicationPause 钩子（SendMessage 模拟）、预放置实例优先识别、ClearAutoSave 后不触发（spec「自动保存」「模块生命周期惯例」场景）。验证：batchmode playmode 过滤跑绿。
- [x] 9.3 关闭 Domain Reload 连按两次 Play 手动验证静态无残留（spec「连续两次 Play 无静态残留」场景；仓库纪律：改静态状态必做）。验证：第二次 Play 注册表与自动保存从零开始（日志或调试输出证据）。

## 10. 示例与镜像

- [x] 10.1 写 `Samples/Save/01_BasicUsage/`（计数器+背包保存/加载/删除 UI、自动保存开关、Status/Source 状态文本、编辑器专属"破坏主存档"演示按钮；整文件 #if UNITY_EDITOR 包裹；场景内 GraphicRaycaster m_Bits=55 惯例；TMP 中文走 TMPChineseFont 技能口径）。验证：编辑器内 Play 可完整走一遍保存→破坏→备份回退流程。
- [x] 10.2 `package.json.samples[]` 登记 + `Samples~/` 镜像同步。验证：`python3 .codely-cli/skills/sync-samples/scripts/sync_samples.py --check` 零漂移。

## 11. 文档与 AI 技能

- [x] 11.1 写 `Documentation~/save-module.md`：Quick 用法、三序列化器选择表（含 JsonUtility 局限）、扩展点三接口示例（自定义 ISaveManager 云存档骨架）、变换链与密钥管理、版本迁移、安全边界声明（CRC32 防意外不防篡改 / AES 防 casual 不防逆向 / WebGL 崩溃安全弱化）、同步契约与大存档指导。验证：文档内代码示例与最终 API 逐字一致。
- [x] 11.2 RAM 包根中英 README 特性表 + `CHANGELOG.md [Unreleased]` Added 段；根 README 模块列表如需同步则一并。验证：grep 确认 Save 模块出现在特性表与 Unreleased。
- [x] 11.3 AI 扩展：新增 `.codely-cli/extensions/AesirFrameworkGame/skills/aesir-save/SKILL.md`（依赖门禁指回 router、Quick 三件套式写法、三轨选择、扩展点、陷阱），更新 `aesir-router` 分派表与扩展 CODELY.md / gemini-extension.json 描述。验证：`python3 .codely-cli/extensions/AesirFrameworkGame/scripts/validate.py` 0 错 0 警。
- [x] 11.4 产品站 `AesirFramework-Site/docs/modules/` 新增 save 页 + 示例总览登记（外部关联目录，物理路径不可移动）。验证：`zensical build --strict` 无 issue。

## 12. 门禁与收尾

- [x] 12.1 全量门禁：编译预热 0 error → EditMode 全量（基线 881+新增）→ PlayMode 全量（基线 36+新增）→ sync_samples --check 零漂移 → validate.py 0 错 0 警。验证：各命令 exit 0，失败解析 XML 定位。
- [x] 12.2 对照 spec 逐条需求核对实现与测试覆盖（openspec-verify-change 流程），确认 design D1–D12 无未落项；精确路径 git add（勿 -A），Conventional Commits 中文提交（主题禁含跳过标记字面量）。验证：`openspec validate add-save-module --strict` 通过 + git status 干净。

## 归档时自动执行

- 本变更的规格 delta（save-module）合并进主规格库（由 `openspec archive` 执行）
- 变更目录移入 `openspec/changes/archive/`（由 `openspec archive` 执行）