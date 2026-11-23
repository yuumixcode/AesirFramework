# Proposal: add-save-module

## Why

RAM 现有 UI / 事件 / 音频 / 场景四模块，唯独缺少存档基础设施：游戏侧只能自写 `File.WriteAllText + JsonUtility` 直写目标文件——写入中途崩溃或断电即永久丢档，且无校验、无备份、无版本迁移。这是独立游戏最常见的存档事故，商业级保存方案（原子写入 + 备份回退 + 完整性校验）已有成熟定式，适合作为 RAM 的通用功能模块沉淀下来，与"功能性优先"的 RAM 定位一致。

## What Changes

- **新增 Save 模块**（`Runtime/Save/`，汇入 `Runestone.AesirModules` 核心程序集）：静态门面 `SaveModule`（MonoBehaviour 单例，与 `AudioModule` 同构：懒创建 + 预放置识别 + DDOL 序列化字段 + 非创建式查询）。
- **事务式保存**：数据经信封封装（magic + 信封版本 + 序列化器标识 + 数据版本 + CRC32 校验和）后走「temp 写入 → `Flush(true)` 落盘 → 双 rename 备份轮换 → 原子提交」；任意崩溃点磁盘上至少保留一份完整可读存档。加载时主文件损坏自动回退 `.bak` 备份。
- **三轨序列化器**（策略可替换）：
  - `UnityJsonSerializer` —— JsonUtility，默认，零依赖；
  - `OdinSaveSerializer` —— Sirenix 二进制序列化（支持 Dictionary / 多态），经 `ODIN_INSPECTOR` 门控 + asmref 汇入既有 Odin 程序集；
  - `NewtonsoftJsonSerializer` —— 可选集成，新增 `Runestone.AesirModules.Newtonsoft` asmdef（`versionDefines` 检测 `com.unity.nuget.newtonsoft-json`，未安装时整体编译剔除，与 UniTask / Addressables 集成同构）。
- **接口层（SOLID，接口隔离）**：`ISaveSerializer`（对象↔字节）、`ISaveStorage`（字节持久化，契约含提交式写入与备份读取）、`ISaveManager`（保存管理编排）。用户实现 `ISaveManager` 即可整体接入自定义保存管理方案（云存档、自定义事务策略等），经 `SaveModule.SetManager` 替换默认 `TransactionalSaveManager`。
- **可选字节变换链** `ISaveTransform`（装饰器）：内置 GZip 压缩与 AES-256 加密两个实现，默认关闭；密钥由游戏侧代码注入，不入配置资产。变换标识记入信封，加载时按记录逆序还原。
- **版本迁移钩子** `ISaveMigrator<T>`：数据版本不匹配时按注册的迁移器升级；未注册则返回 `VersionMismatch` 状态而非静默错读。
- **自动保存**：`SetAutoSave(Action, interval)` 复用 RAA `AesirTimer`（unscaled），并挂 `OnApplicationPause` / `OnApplicationQuit` 钩子（可经配置关闭）。
- **配置资产** `SaveConfigSO`（存储目录 / 默认序列化器 / 备份开关 / 退出与切后台保存开关），接入既有 `Tools/Aesir/Modules/Create Module Config Assets` 菜单；无资产时内存默认值兜底。
- **配套交付**：示例 `Samples/Save/01_BasicUsage`（+ `Samples~/` 镜像 + `package.json.samples` 登记）、EditMode / PlayMode 测试、`Documentation~/save-module.md`、两包 README 特性表与 CHANGELOG `[Unreleased]`、AI 扩展新增 `aesir-save` 技能并更新 router 分派表。

**明确不做（边界）**：异步 / 多线程保存（同步契约，与 `IUIAssetLoader` 同口径；大数据量场景文档指导"主线程捕获 + 自行后台写入"）；云存档具体实现（只留 `ISaveManager` / `ISaveStorage` 扩展点）；Unity 对象引用序列化（GameObject / Component / 资产引用，Easy Save 式能力——只存纯数据对象）；PlayerPrefs 存储后端；多代备份轮换（固定 1 代 `.bak`，双 rename 已保证任意崩溃点可读）；存档槽 UI；防作弊 / 防篡改（CRC32 只检测意外损坏，加密只防 casual 修改，文档明示安全边界）。

## Capabilities

### New Capabilities

- `save-module`: RAM 保存模块的行为契约——存档槽键值语义、三轨序列化、信封校验、事务式写入与备份回退、损坏 / 缺失 / 版本不匹配的加载结果语义、扩展点替换（序列化器 / 存储 / 管理器 / 变换链）、版本迁移、自动保存与生命周期钩子、配置兜底。

### Modified Capabilities

（无——规格库当前为空，本变更不改动既有能力的要求。）

## Impact

- **RAM 包新增**：`Runtime/Save/`（核心 + `OdinInspector/` asmref）、`Runtime/Integration/Newtonsoft/`（新 asmdef）、`Editor/Save/`（配置 Initializer + 原生 Drawer 兜底）、`Tests/Editor/Save/`、`Tests/Editor/OdinInspector/Save/`、`Tests/Runtime/Save/`、`Samples/Save/01_BasicUsage/` + `Samples~/` 镜像。
- **RAM 包修改**：`Editor/Common/ModuleConfigAssetsMenuItem.cs`（追加 SaveConfigSO 初始化）、`package.json`（samples 登记）、`CHANGELOG.md`、中英 README 特性表、`Documentation~/save-module.md`（新增）。
- **AI 扩展**（`.codely-cli/extensions/AesirFrameworkGame/`）：新增 `skills/aesir-save/`，更新 `aesir-router` 分派表、`CODELY.md` 与 `gemini-extension.json` 描述，跑 `validate.py`。
- **依赖**：零新增硬依赖。Newtonsoft（`com.unity.nuget.newtonsoft-json`）与 Odin 均为可选集成，未安装时对应程序集被编译剔除；本仓未装 Newtonsoft，其适配器与 Addressables 集成同例——代码交付但本仓不参与编译测试。
- **不受影响**：RAA 包零改动；RAM 其余模块零依赖（Save 不引用 UI / 事件 / 音频 / 场景）；宿主工程与既有示例零改动。
- **文档站**（外部关联目录 `AesirFramework-Site/`）：模块页与示例总览需同步，物理路径受 SDG 与 `documentationUrl` 依赖不可移动。