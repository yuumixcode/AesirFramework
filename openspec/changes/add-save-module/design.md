# Design: add-save-module

## Context

动机见 `proposal.md - Why`；行为契约见 `specs/save-module/spec.md`。 shaping 本设计的现状约束（均已实地核查）：

- **RAM 模块范式**（`AudioModule` / `AudioConfigSO` 实读）：MonoBehaviour 单例 + 全静态门面；`Instance` getter 先 `FindAnyObjectByType(FindObjectsInactive.Include)` 找预放置实例，未命中经 `AesirModules.GetOrAddChild<T>()` 懒创建；`Ready()` 统一懒初始化入口；非创建式查询走 `TryGetExisting`；配置 SO 字段 `internal [SerializeField]` + Tooltip，无资产时代码默认值兜底；`[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]` 重置静态字段；DDOL 序列化字段为类体第一个字段。
- **接口层先例**：`IUIAssetLoader`（同步契约、可插拔、注册替换）——Save 的三扩展点沿用此口径。
- **可选集成先例**：`Runestone.AesirModules.UniTask.asmdef` 的 `versionDefines` + `defineConstraints` **双声明**（宏只对声明它的程序集可见）；Addressables 集成在本仓未安装、测试自适应 skip——Newtonsoft 集成照此办理。Odin 侧经 asmref 汇入既有 `Runestone.AesirModules.OdinInspector`（`ODIN_INSPECTOR` 门控）。
- **依赖现状**：本仓已装 Odin（`Assets/Plugins/Sirenix/`）、未装 Newtonsoft（`Packages/manifest.json` 无 `com.unity.nuget.newtonsoft-json`）、已装 UniTask。
- **商业级方案调研结论**（一手来源）：
  - 原子写入定式 = temp 写入 → 强制刷盘 → rename 覆盖（rename 在主流 OS 上是原子目录项更新；`File.Move` 映射 rename，`File.Copy` 不是）——Unity Discussions「safer way to write data」、Rust Forum 引 POSIX 文档、LWN《Ensuring data reaches disk》（fsync 语义：rename 原子 ≠ 掉电原子，需先 flush 到稳定存储）。
  - 双 rename 备份轮换（save→.bak，temp→save）保证**任意崩溃点至少一份可读存档**；Windows 上 rename 到已存在名字可能失败，故先删旧 .bak——备份槽是可牺牲副本，不可牺牲的主存档永远不在非原子步骤中暴露——SELOdev《Atomic save files in Godot 4》。
  - 校验和（checksum/hash）用于损坏检测，加载失败与校验失败同路径处理——Unity Discussions 同上。
  - 商业插件功能面参照 Easy Save 3：AES 加密（默认关、密码派生密钥、明示"加密≠防一切攻击"）、GZip 压缩、备份、存档槽、云存储扩展点、自动保存。本模块取其"独立游戏够用"的子集。

## Goals / Non-Goals

**Goals:**

- 三层扩展点各自单一职责、可独立替换（SOLID 的 S/O/L/I/D 全部落到具体契约，见 D1）。
- 默认路径一行代码可用（Quick 基调）：`SaveModule.Save("slot0", data)` / `Load<T>("slot0")`。
- 崩溃安全与损坏回退是**核心功能**而非防御性兜底——存档损坏是高概率真实场景（断电 / 崩溃 / 磁盘错误），与 RAA"低概率问题不做兜底"基调不冲突。
- 零新增硬依赖；可选序列化器缺席时编译基线不变。

**Non-Goals（设计级边界，proposal 边界之外补充）：**

- 不做加载路径的磁盘修复（只读回退，不 promote rename）——读路径无副作用，兼容只读介质。
- 不做序列化器自动注册（跨程序集静态耦合 + 静态重置复杂度）——切换序列化器是一行显式注册。
- 不做 `ISaveManager` 的 Delete/Exists/GetKeys 编排——这些是存储直通操作，门面直接转发存储，管理器接口只留 Save/Load 两方法（ISP）。
- 不做存档文件的向前兼容承诺（信封格式版本化，旧版本信封在新代码中明确拒绝而非猜测解析）。

## Decisions

### D1 — 三层扩展点的职责切分（SOLID 落点）

```
SaveModule（静态门面 / Facade 模式）
  │  生命周期、自动保存计时、配置载入、注册表持有
  ▼
ISaveManager（策略：保存管理编排 —— 用户自定义保存方案的总入口）
  │  Save<T>(key, data, dataVersion) → SaveResult
  │  Load<T>(key, currentVersion, migrator) → LoadResult<T>
  ▼  默认实现 TransactionalSaveManager 依赖：
  ├── ISaveSerializer（策略：对象↔字节，Id 标识）
  ├── ISaveStorage（桥接：字节持久化，契约含提交式写入 + 备份读取）
  └── ISaveTransform[]（装饰器链：字节→字节，压缩 / 加密）
```

- **S**：序列化器只做对象↔字节；存储只做字节持久化（原子性与备份是存储契约的一部分）；管理器只做编排（信封封装 / 校验 / 变换链 / 回退决策 / 迁移调用）；门面只做转发与 Unity 生命周期适配。
- **O**：新增序列化器 / 存储 / 管理器 / 变换均不改既有代码；注册表 + 替换 API 是唯一扩展通道。
- **L**：三接口契约以文档 + 测试固化（存储的"提交式写入"契约：`Write` 返回后读到的要么是旧完整数据要么是新完整数据），任何实现可互换。
- **I**：三个小接口而非一个大 `ISaveSystem`；管理器只有 Save/Load 两方法，Delete/Exists/GetKeys 由门面直通存储。
- **D**：管理器依赖抽象（构造注入序列化器注册表 / 存储 / 变换链），门面依赖 `ISaveManager`；具体类只在组合根（门面懒初始化 / 用户组合代码）出现。

**备选与否决**：① 单一大接口 `ISaveSystem`（序列化 + IO + 编排一把抓）——违反 ISP，用户只想换序列化器却被迫实现 IO，否决。② 事务流程放管理器、存储只做裸读写——云存储 / PlayerPrefs 等无 rename 语义的后端会被管理器的文件事务逻辑错误套用；把"提交式写入"下沉为存储契约后，换存储只需满足契约，管理器保持后端无关，否决裸读写方案。③ 事务流程放存储内部且管理器不感知备份——备份回退决策（主损坏读备份）需要管理器知道校验结果，存储暴露 `ReadBackup` 即可，不需要 `ISaveTransaction` 第四接口（过度设计），否决。

### D2 — 事务式写入流程与崩溃点分析

`FileSaveStorage.Write(key, bytes)` 五步（同目录，`.sav` / `.sav.bak` / `.sav.tmp` 三文件名）：

```
1. 写 key.sav.tmp（全量字节，覆盖残留）
2. FileStream.Flush(flushToDisk: true)   // Windows: FlushFileBuffers; POSIX: fsync
3. 若 key.sav.bak 存在 → 删除
4. 若 key.sav 存在 → rename 为 key.sav.bak
5. rename key.sav.tmp → key.sav          // 原子提交
```

| 崩溃点 | 磁盘状态 | 加载行为 |
|---|---|---|
| 1/2 中途 | tmp 残缺，主完好 | 读主；tmp 被下次写入覆盖 |
| 3 之后 | bak 缺失，主完好 | 读主（备份暂缺，下次保存重建） |
| 4 之后 | 主缺失，bak 完好 | 读 bak（Source=Backup） |
| 5 中途 | rename 原子：要么主=旧（bak 在）要么主=新 | 读主 |

唯一非原子窗口是步骤 3→4 之间（bak 已删、主未轮转）：此时崩溃主存档仍完好，损失的只是备份副本——**不可牺牲的主存档从不暴露在非原子步骤中**（SELOdev 定式）。

**备选与否决**：① `File.Replace`——仅 Windows 语义完整，跨平台不一致，否决。② 直接 `File.WriteAllText` 覆盖——写入中途崩溃即截断丢档，正是要解决的事故，否决。③ WAL / 日志式事务（SQLite 式）——独立游戏存档是单文件全量写，WAL 的复杂度无收益，否决。④ 多代备份轮换（.bak1/.bak2/…）——防"逻辑错误存了坏数据"需要人工选档 UI 配合，超出独立游戏默认需求；固定 1 代 + 双 rename 已覆盖崩溃安全，多代留给自定义 `ISaveStorage`，否决内置。

### D3 — 信封二进制格式与 CRC32

存档文件 = 固定二进制信封头 + payload（变换后的序列化字节）。信封头**不经过用户序列化器**（否则加载时连头都读不出）：

```
偏移   大小      字段
0      4        magic "ASAV"
4      1        信封格式版本（当前 = 1）
5      1        serializerId 字节长 N（UTF8，≤255）
6      N        serializerId（如 "unity-json" / "odin" / "newtonsoft"）
6+N    1        变换数 M
…      M×(1+len) 各 transformId（1 字节长 + UTF8，按施加顺序）
…      4        dataVersion（int32 小端）
…      4        payload 字节长（int32 小端）
…      4        payload CRC32（uint32 小端，IEEE 多项式，对变换后字节计算）
…      其余     payload
```

加载校验链：magic 匹配 → 信封版本支持 → 头部长度自洽（payload 长 = 文件实际剩余）→ CRC32 匹配。任一失败 = 该份损坏 → 回退备份。

- **CRC32 而非 SHA256**：目标是检测意外损坏（截断 / 位翻转），非对抗篡改；CRC32 表驱动约 40 行、零依赖、快一个量级。防篡改不是本模块目标（见 Risks）。
- **信封版本化**：格式演进时 bump 版本，旧版本明确拒绝（`Corrupted` + 描述），不做猜测解析。
- **备选否决**：JSON 信封（头也走 JsonUtility）——头与 payload 序列化器耦合，且文本头无法表达二进制 payload 长度自洽；无校验裸 payload——损坏检测无从谈起，商业方案共识是有校验。

### D4 — 结果对象 vs 异常的边界

- **可预期环境失败 → 结果对象**：`SaveResult { IsSuccess, Error }`；`LoadResult<T> { Status, Value, Source, Error }`，`LoadStatus = Success / NoSave / Corrupted / VersionMismatch / SerializerMissing`，`LoadSource = Primary / Backup`。IO 异常（磁盘满 / 权限）被捕获转为失败结果 **并 LogError**（错误信息完整保留，不是吞噬）——游戏侧可据此提示重试。存档损坏 / 缺失 / 版本不匹配是存档系统的**正常业务分支**，用异常表达会迫使每个调用点 try/catch。
- **编程错误 → 抛异常（fail-fast）**：非法键（`ArgumentException`）、null 数据、配置指定了未注册的默认序列化器（`InvalidOperationException` 带修复提示，与 `AbstractContext.GetModel` 同口径）。
- 自动保存回调内的异常不捕获（`AesirTimer` fail-fast 语义，回调是游戏代码，异常即游戏 bug）。

**备选否决**：全异常（商业插件 Easy Save 也是结果 / 默认值风格；异常流不符合"简单"目标）；全结果（编程错误也静默返回失败会掩盖 bug，违反 fail-fast 基调）。

### D5 — 序列化器注册表与按档还原

`Dictionary<string, ISaveSerializer>`，键为 `Id`。默认注册 `UnityJsonSerializer`（Id `"unity-json"`）。信封记录 Id，加载按 Id 查表——不同键可混用序列化器，切换默认不破坏旧档。Odin（Id `"odin"`）/ Newtonsoft（Id `"newtonsoft"`）/ 自定义序列化器一律**显式注册**：`SaveModule.RegisterSerializer(new OdinSaveSerializer())`。

接口形状：`string Id { get; }` + `byte[] Serialize<T>(T value)` + `T Deserialize<T>(byte[] data)`（泛型方法，三实现均天然支持：JsonUtility / JsonConvert / Sirenix `SerializationUtility` 都是泛型 API）。

**备选否决**：全局单一序列化器（切换即旧档不可读）；自动注册（Odin 程序集 `[RuntimeInitializeOnLoadMethod]` 反向注册进核心——跨程序集静态耦合 + 重置时序复杂，且违反"显式优于隐式"的 Quick 基调）。

### D6 — 变换链（装饰器）与密钥管理

`ISaveTransform { string Id; byte[] Encode(byte[]); byte[] Decode(byte[]); }`。`SetTransformChain(params ISaveTransform[])` 按序 Encode（先压缩后加密）、加载按信封记录的 Id 列表**逆序** Decode。默认空链。

- `GZipSaveTransform`：`System.IO.Compression.GZipStream`（.NET Standard 2.1 内置）。
- `AesSaveTransform(string password)`：SHA256(password) 派生 256-bit 密钥；**每次保存随机 IV，前置 16 字节写入变换输出**（IV 无需保密）；解密先读 IV。密钥只存在于运行时内存，不入配置资产、不入存档。
- 加载时信封记录的 transformId 未注册 → `Corrupted`（无法还原即损坏语义）；AES 密钥错误 → 解密产出垃圾 → CRC 校验失败 → 同路径回退 / 失败。

**备选否决**：配置资产布尔开关（`enableEncryption`）——密钥无处安放（资产进版本库即泄密），且开关无法表达链序；每存档 options 传变换——Quick 路径参数膨胀，全局链 + 信封记录已覆盖"旧档按旧链还原"。

### D7 — 版本迁移钩子

`ISaveMigrator<T> { T Migrate(T data, int fromVersion); }`，作为 `Load<T>` 的可选参数传入（不放全局注册表——迁移器与数据类型强绑定，泛型注册表会引入类型擦除复杂度）。版本一致直接返回；不一致且有迁移器 → 调用后返回成功；不一致且无迁移器 → `VersionMismatch`（携带两侧版本号）。迁移结果不回写磁盘（读路径无副作用，D2 口径）。

**备选否决**：迁移链 `List<ISaveMigration>` 逐版本步进（独立游戏版本跨度小，单回调内 switch 足够，链式是过度设计）；无迁移支持（版本不匹配静默错读是商业方案明确反对的事故源）。

### D8 — 自动保存复用 AesirTimer + Unity 生命周期钩子

`SetAutoSave(Action saveAction, float intervalSeconds)` → `AesirTimer.Every(interval, saveAction, unscaled: true)`（RAA 既有能力组合，不新造计时原语；unscaled 保证暂停菜单 / 慢动作不影响存档节奏）。`OnApplicationPause(true)` 与 `OnApplicationQuit` 各触发一次回调，开关来自配置。`ClearAutoSave()` 取消计时器并清回调。这正是 SaveModule 做成 MonoBehaviour 单例（而非纯静态类）的理由：需要 Unity 生命周期消息与宿主挂载。

### D9 — 程序集与目录布局

```
Runtime/Save/                          → 汇入 Runestone.AesirModules（无新 asmdef）
├── SaveModule.cs                      门面 + 单例 + 自动保存 + 注册表
├── SaveConfigSO.cs                    配置资产
├── ISaveSerializer.cs / ISaveStorage.cs / ISaveManager.cs
├── ISaveTransform.cs / ISaveMigrator.cs
├── SaveResult.cs                      SaveResult / LoadResult<T> / LoadStatus / LoadSource
├── TransactionalSaveManager.cs        默认管理器
├── FileSaveStorage.cs                 默认存储（D2 事务流程）
├── UnityJsonSerializer.cs             默认序列化器
├── GZipSaveTransform.cs / AesSaveTransform.cs
├── Internal/                          SaveEnvelope / Crc32 / SaveKey（internal）
└── OdinInspector/
    ├── OdinSaveSerializer.cs          Sirenix.Serialization.SerializationUtility（Binary）
    └── *.asmref → Runestone.AesirModules.OdinInspector
Runtime/Integration/Newtonsoft/
├── NewtonsoftJsonSerializer.cs
└── Runestone.AesirModules.Newtonsoft.asmdef
    （versionDefines: com.unity.nuget.newtonsoft-json → AESIR_MODULES_NEWTONSOFT；
      defineConstraints: [AESIR_MODULES_NEWTONSOFT] —— 双声明纪律；
      references: RAA + RAM；Newtonsoft.Json 为 autoReferenced 预编译 DLL，无需显式引用）
Editor/Save/
└── SaveModuleConfigAssetInitializer.cs（幂等创建，接入 ModuleConfigAssetsMenuItem）
```

- 核心不新增 asmdef（模块以子目录汇入锚点程序集，RAM 惯例）。
- Odin 适配器走 asmref（与 `Runtime/UI/OdinInspector` 同构）；实施时核对宿主 Odin asmdef 的 `precompiledReferences` 含 `Sirenix.Serialization.dll`。
- Newtonsoft 适配器本仓不参与编译（未安装包），与 Addressables 集成同例；文档给出安装后的注册示例。
- 存储根目录：`Application.persistentDataPath/<config.saveDirectory>`（默认 `Saves`），首次写入懒创建；`GetKeys` 枚举 `*.sav`（排除 `.tmp` / `.bak`）。

### D10 — 同步契约

保存 / 加载全同步（与 `IUIAssetLoader` 同口径）。独立游戏存档体量（KB 级）同步写 < 1ms，异步收益不抵复杂度（线程安全、主线程捕获时序、UniTask 双驱动面）。大存档场景文档指导：**主线程捕获数据 → 自行 `Task.Run` 写入字节**（序列化与变换是纯函数可后台，Unity 对象访问不可）。未来确需异步再按 SceneModule 的 UniTask 集成模式扩展，不预留半成品 API。

### D11 — 配置资产字段

`SaveConfigSO`（`AesirScriptableObject` 基类，`[CreateAssetMenu("Aesir Modules/Save/SaveConfig")]`）：

| 字段 | 默认 | 说明 |
|---|---|---|
| `saveDirectory` | `"Saves"` | persistentDataPath 下子目录名 |
| `defaultSerializerId` | `"unity-json"` | 保存未指定时的序列化器 |
| `backupEnabled` | `true` | 关闭后 Write 跳过步骤 3/4（仍 temp+rename 原子提交），ReadBackup 恒 null |
| `saveOnApplicationPause` | `true` | 切后台自动保存开关 |
| `saveOnApplicationQuit` | `true` | 退出自动保存开关 |

压缩 / 加密**不进配置**（D6：密钥与链序是代码侧组合）。无资产时内存默认值兜底（与 AudioModule 同口径）；`ApplyConfig` 支持运行时替换。

### D12 — 测试与示例策略

- **EditMode**（文件 IO 走 `Temp/` 目录，TearDown 回收；静态重置沿用 `AudioModuleTests` 的既有手法）：序列化往返（UnityJson + Odin）、信封校验与位翻转、事务崩溃点模拟（手工构造 D2 表中各磁盘状态）、主备回退、键合法性、变换链往返与错误密钥、迁移分支、配置兜底。Newtonsoft 用例不写（本仓未安装，无法编译验证；安装者按文档自验）。
- **PlayMode**：自动保存周期触发（AesirTimer 帧驱动）、`OnApplicationPause` 钩子（SendMessage 模拟）、预放置实例识别。
- **示例** `Samples/Save/01_BasicUsage`：计数器 + 简单背包的保存 / 加载 / 删除 UI、自动保存开关、加载状态文本（展示 Status/Source）、编辑器专属"破坏主存档"按钮演示备份回退。整文件 `#if UNITY_EDITOR` 包裹（运行时程序集纪律），`Samples~/` 镜像 + `package.json.samples` 登记。
- 测试密度遵守仓库规范（单类型 3–10 例、一缺陷一例、不测 BCL 已保证的事）。

## Risks / Trade-offs

- **[WebGL 崩溃安全弱于原生]** `persistentDataPath` 在 WebGL 是 IndexedDB 虚拟文件系统，`Flush(true)` 无真实 fsync 语义、rename 到 IndexedDB 持久化存在延迟窗口 → 文档明示 WebGL 边界，建议关键进度配合云存档（`ISaveManager` 扩展点）；代码不做平台特判。
- **[CRC32 不防篡改 / AES 不防逆向]** 校验只检测意外损坏；密钥在客户端二进制内可被提取 → 文档安全边界声明（与 Easy Save 官方文档同口径："加密≠防一切攻击"）；防作弊需服务端校验，超出模块范围。
- **[Odin 二进制格式跨版本兼容]** Sirenix 序列化格式随 Odin 大版本可能变化 → 文档建议 Odin 序列化存档的项目钉住 Odin 版本升级节奏；信封 serializerId 保证至少明确失败而非静默错读。
- **[JsonUtility 固有局限]** 不支持 Dictionary / 多态 / 属性，null 集合反序列化为空 → 序列化器选择表进文档；需要这些形状时引导 Odin / Newtonsoft 轨。
- **[同步写大存档卡主线程]** KB 级无感，MB 级可感知 → D10 文档指导；不内置异步避免半成品 API。
- **[Newtonsoft 适配器本仓零编译验证]** 与 Addressables 集成同风险 → 代码按公开 API 形状编写（JsonConvert 泛型 API 稳定多年），文档标注"安装后自验"；后续本仓安装该包时补测试。
- **[双 rename 的备份暂缺窗口]** D2 步骤 3→4 之间崩溃会短暂失去备份（主完好）→ 下次保存重建；接受此窗口（商业定式同款取舍）。
- **[信封格式演进]** 当前版本 1，未来字段变更需 bump 并拒绝旧版 → 旧档明确失败优于猜测解析；格式变更走完整迭代流程（CHANGELOG 破坏性记录）。

## Migration Plan

纯新增模块，无存量迁移：不改动既有 API / 资产 / 场景；合入后随下一次版本 bump 走标准发版流程（version-sync skill 11 步，不在本变更内）。回滚 =  revert 本变更提交（模块目录整体新增，无交叉修改，仅 `ModuleConfigAssetsMenuItem.cs` 一行追加需一并还原）。

## Open Questions

（无——扩展点形状、事务流程、信封格式、结果语义均已定案；实施期若发现 Odin 宿主 asmdef 缺 `Sirenix.Serialization` 引用，按 D9 核对项就地补齐，属实现细节不改变设计。）