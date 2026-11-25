# 保存模块（Save Module）

## 概述

保存模块提供面向独立游戏的事务式存档门面：存档槽键值保存 / 加载、三轨序列化、完整性校验、备份回退、
版本迁移与自动保存。公开 API 全部为静态成员，经全局单例转发，调用即用。

- **事务式写入**：新数据先写临时文件并强制刷盘，再经双 rename 备份轮换与原子替换提交——写入流程的
  任意中断点（崩溃 / 断电）之后，磁盘上至少保留一份完整可读存档（旧值或新值）
- **损坏回退**：每份存档携带 CRC32 校验和；主存档损坏或缺失时自动回退上一份备份，结果标明来源
- **三轨序列化**：Unity JsonUtility（默认，零依赖）/ Odin 二进制（Dictionary、多态）/ Newtonsoft JSON（可选集成）
- **扩展点**：序列化器、存储、保存管理器、字节变换链四个接口各自可替换——实现 `ISaveManager` 即可整体
  接入自定义保存管理方案（云存档等）
- **自动保存**：周期计时（复用 RAA `AesirTimer`，不受 timeScale 影响）+ 切后台 / 退出钩子
- **批量原子会话**：多个命名条目打包成一个存档键一次事务提交，一组数据全成或全旧（见「批量原子会话」）
- **默认值加载 / 时间戳**：加载失败一行取默认值；存档最后写入时间只读查询（见对应小节）

> **注意**：模块只存**纯数据对象**（POCO），不做 Unity 对象引用序列化（GameObject / Component / 资产引用）。
> 全同步契约；云存档只留扩展点不做实现。安全边界见文末。

## 核心类型

### SaveModule

MonoBehaviour 单例，继承 `AesirMonoBehaviour`。预放置于场景时优先被发现（含未激活对象）；未预放置时
自动挂载到 `[Aesir Modules]` 宿主下（跟随宿主 DDOL 决策）。公开 API 全为静态成员。

```csharp
// 保存与加载（默认路径一行可用）
public static SaveResult Save<T>(string key, T data, int dataVersion = 0);
public static LoadResult<T> Load<T>(string key, int currentVersion = 0, ISaveMigrator<T> migrator = null);

// 存档槽管理（直通存储，不经管理器）
public static void Delete(string key);          // 删除主存档与备份；不存在时无副作用
public static bool Exists(string key);
public static IReadOnlyList<string> GetKeys();  // 枚举已提交存档键（不含 tmp/bak）

// 批量原子会话 / 默认值 / 时间戳
public static SaveResult SaveBatch(string key, Action<SaveBatch> configure, int dataVersion = 0);
public static LoadResult<SaveBatch> LoadBatch(string key, int currentVersion = 0, ISaveMigrator<SaveBatch> migrator = null);
public static T LoadOrDefault<T>(string key, T defaultValue, int currentVersion = 0, ISaveMigrator<T> migrator = null);
public static DateTime? GetTimestamp(string key);  // 存储不支持时返回 null

// 扩展点替换
public static void RegisterSerializer(ISaveSerializer serializer);   // 同 Id 覆盖
public static void SetStorage(ISaveStorage storage);                 // 默认管理器随之重建
public static void SetManager(ISaveManager manager);                 // 整体接管保存/加载编排
public static void SetTransformChain(params ISaveTransform[] transforms); // 空数组清除链

// 自动保存
public static void SetAutoSave(Action saveAction, float intervalSeconds); // 重复调用替换既有注册
public static void ClearAutoSave();                                    // 不创建模块实例

// 配置
public static void ApplyConfig(SaveConfigSO config);  // null 回退 SaveConfigSO.Instance

// 单例
public static SaveModule Instance { get; }
public static bool TryGetExisting(out SaveModule module);  // 非创建式查询

// 预放置实例可调（序列化字段）
// [SerializeField] SaveConfigSO config;          — 可选配置资产（留空回退 SaveConfigSO.Instance）
// [SerializeField] bool dontDestroyOnLoad = true; — 预放置为根物体时生效
```

**存档键**：仅字母、数字、下划线、连字符（白名单校验防路径穿越）；非法键抛 `ArgumentException`。

**失败边界**：IO 等可预期环境失败以结果对象表达（错误信息完整保留并 LogError，不是吞噬）；
编程错误（非法键 / null 数据 / 默认序列化器未注册）抛异常（fail-fast）。

### SaveConfigSO

模块配置资产。创建路径：`Assets → Create → Aesir Modules → Save → SaveConfig`，或菜单
`Tools/Aesir/Modules/Create Module Config Assets`（幂等）。**包导入时不自动创建**；无资产时运行时以
内存默认值兜底（不落盘）。单例 `SaveConfigSO.Instance` 按 Resources 兜底 → 内存默认解析。

| 字段                     | 类型   | 默认值         | 说明                                             |
|--------------------------|--------|----------------|--------------------------------------------------|
| `saveDirectory`          | string | `"Saves"`      | `Application.persistentDataPath` 下子目录名      |
| `defaultSerializerId`    | string | `"unity-json"` | 保存使用的序列化器（须已注册）                   |
| `backupEnabled`          | bool   | `true`         | 保留备份；关闭后提交仍原子，但失去损坏回退能力   |
| `saveOnApplicationPause` | bool   | `true`         | 切后台时触发自动保存回调                         |
| `saveOnApplicationQuit`  | bool   | `true`         | 退出时触发自动保存回调                           |

压缩 / 加密**不在本资产配置**：变换链与密钥是代码侧组合（密钥入资产即泄密），见下文变换链。

### 结果类型

```csharp
public readonly struct SaveResult
{
    public bool IsSuccess { get; }
    public string Error { get; }        // 成功时 null
}

public readonly struct LoadResult<T>
{
    public LoadStatus Status { get; }   // Success / NoSave / Corrupted / VersionMismatch / SerializerMissing
    public T Value { get; }             // 仅 Success 时有效
    public LoadSource Source { get; }   // Primary / Backup（仅成功时有意义）
    public string Error { get; }
    public int SavedVersion { get; }    // 仅 VersionMismatch 时有意义
    public int CurrentVersion { get; }
    public bool IsSuccess { get; }
}
```

| 状态                | 含义                                             | 典型处理                     |
|---------------------|--------------------------------------------------|------------------------------|
| `Success`           | 加载成功（`Source` 标明主存档或备份）            | 使用 `Value`                 |
| `NoSave`            | 主存档与备份均无                                 | 开新档                       |
| `Corrupted`         | 主备均损坏 / 解析失败 / 变换缺失                 | 提示玩家 + 开新档            |
| `VersionMismatch`   | 版本不一致且未提供迁移器                         | 提供迁移器重新加载           |
| `SerializerMissing` | 存档记录的序列化器未注册                         | 注册对应序列化器重新加载     |

## 批量原子会话

一组相关数据（如金币 + 背包 + 位置）若各自 `Save` 成独立键，分步写入的中途崩溃可能只落一部分，数据不一致。
批量会话把多个命名条目打包成**一个**存档键、一次事务提交——复用单条保存的序列化 / 变换链 / 信封 / 备份回退，
原子边界就是这一次提交：**全成或全旧**。设计思路参考 Easy Save 3 的 `ES3File` 内存缓存，但按 Aesir 极简基调
收敛为「一次回调 + 一次提交」，不留常驻缓存对象。

```csharp
// 保存：回调内登记命名条目（条目数据须为所选序列化器可序列化的 POCO）
var result = SaveModule.SaveBatch("slot0", batch =>
{
    batch.Set("gold", new GoldData { amount = 100 });
    batch.Set("inventory", new InventoryData { items = ... });
}, dataVersion: 1);

// 加载：按名取回，各自按写入时的类型还原
var loaded = SaveModule.LoadBatch("slot0", currentVersion: 1);
if (loaded.IsSuccess)
{
    var gold = loaded.Value.Get<GoldData>("gold");
    var inventory = loaded.Value.Get<InventoryData>("inventory");
}
```

- `SaveBatch` 的 `Set<T>(name, data)` / `Get<T>(name)` / `Contains(name)` / `Count`：同名条目覆盖；
  `Get` 在条目缺失 / 序列化器未注册 / 还原失败时返回 `default(T)`（与单条 `Load` 的 NoSave / Corrupted 同语义）。
- **条目数据须符合序列化器形状约束**：默认 `unity-json` 只支持 `[Serializable]` 公开字段类，裸 `int` / `string`
  等原始类型须包进 POCO 字段（与单条 `Save` 同限制——`JsonUtility.ToJson(100)` 返回 `{}`）。Odin / Newtonsoft 轨按各自能力放宽。
- **原子边界仅是一个批次键内部**：不同批次键之间无全局原子；需跨键原子的数据放进同一批次。
- 批次的 `dataVersion` 是整份批次的版本（走信封层），`LoadBatch` 可传 `ISaveMigrator<SaveBatch>` 迁移；条目内不再各自迁移。
- `SaveBatch` 实例不要自行 `new`（未绑定注册表，`Get` 返回 default）——只从 `SaveBatch` 回调 / `LoadBatch` 结果获取。

## 默认值加载

`LoadOrDefault<T>` 把「缺失 / 损坏 / 版本不匹配 / 序列化器缺失 → 用默认值」收敛成一行，省去调用点 `switch (Status)`：
成功时返回存档值（忽略默认值），其余四状态返回传入默认值；不写磁盘。

```csharp
var gold = SaveModule.LoadOrDefault("gold", new GoldData { amount = 0 });
```

- 独立方法名而非 `Load` 重载：`Load<int>(key, 5)` 会与现有 `currentVersion` 参数签名歧义。
- 需要区分失败原因（提示玩家「存档损坏」而非静默开新档）时用 `Load<T>` 读 `Status`。

## 存档时间戳

`GetTimestamp(key)` 返回主存档最后写入时间（UTC）；无存档、存储不支持、取不到时均返回 `null`，不创建模块实例、不写磁盘。
时间戳能力经可选接口 `ISaveStorageTimestamp` 提供——`FileSaveStorage` 已实现（读文件 mtime）；自定义存储可实现该接口接入，
未实现则门面返回 null。

```csharp
DateTime? when = SaveModule.GetTimestamp("slot0");   // 选档 UI 显示「上次保存时间」
```

- 时间戳是尽力而为的只读查询：精度依文件系统（NTFS / APFS / exFAT 不同），WebGL 虚拟文件系统可能无有效 mtime；
  不用于精确仲裁顺序。

## 三轨序列化

| 序列化器                  | Id            | 依赖                        | 支持形状                                   |
|---------------------------|---------------|-----------------------------|--------------------------------------------|
| `UnityJsonSerializer`     | `unity-json`  | 无（默认注册）              | 公开字段 / `[SerializeField]`、`List<T>`、Unity 值类型 |
| `OdinSaveSerializer`      | `odin`        | Odin（`ODIN_INSPECTOR`）    | 上述 + `Dictionary<K,V>`、多态、私有字段   |
| `NewtonsoftJsonSerializer`| `newtonsoft`  | `com.unity.nuget.newtonsoft-json` | 上述 + 属性；多态需自行子类化启用 TypeNameHandling |

JsonUtility 固有局限：不支持 Dictionary / 多态 / 属性；null 集合反序列化为空集合。需要这些形状时换轨：

```csharp
// Odin 轨（装了 Odin 即可用）
SaveModule.RegisterSerializer(new OdinSaveSerializer());
var config = SaveConfigSO.CreateDefault();
config.defaultSerializerId = "odin";
SaveModule.ApplyConfig(config);
```

可选序列化器缺席时对应程序集整体编译剔除（Newtonsoft 经 `versionDefines` 检测包安装），不影响默认轨运行。
每份存档记录所用序列化器 Id，加载按记录还原——**切换默认序列化器不破坏旧档**；不同键可混用序列化器。
Odin 二进制格式随 Odin 大版本可能变化，使用 Odin 轨的项目建议钉住 Odin 版本升级节奏。

## 扩展点（SOLID 接口层）

四个接口各自单一职责、可独立替换（策略 / 桥接 / 装饰器）：

```csharp
public interface ISaveSerializer   // 对象 ↔ 字节
{
    string Id { get; }
    byte[] Serialize<T>(T value);
    T Deserialize<T>(byte[] data);
}

public interface ISaveStorage      // 字节持久化（契约：提交式写入 + 备份读取）
{
    byte[] Read(string key);        // 不存在返回 null
    byte[] ReadBackup(string key);  // 无备份返回 null
    void Write(string key, byte[] data);  // 返回后读到的要么旧完整数据、要么新完整数据
    void Delete(string key);
    bool Exists(string key);
    IReadOnlyList<string> GetKeys();
}

public interface ISaveManager      // 保存/加载编排 —— 自定义保存管理方案总入口
{
    SaveResult Save<T>(string key, T data, int dataVersion);
    LoadResult<T> Load<T>(string key, int currentVersion, ISaveMigrator<T> migrator);
}

public interface ISaveTransform    // 字节 → 字节（压缩/加密）
{
    string Id { get; }
    byte[] Encode(byte[] data);
    byte[] Decode(byte[] data);
}
```

**自定义保存管理方案**（如云存档）：实现 `ISaveManager` 并经 `SetManager` 替换，默认事务式管理器不再参与；
删除 / 存在性 / 枚举仍直通存储。自定义管理器应遵守结果语义契约（环境失败用结果、编程错误抛异常）。

```csharp
// 云存档管理器骨架：本地事务保存 + 云端异步上传（上传失败不影响本地存档）
public class CloudSaveManager : ISaveManager
{
    readonly TransactionalSaveManager _local;   // 本地仍走事务式默认实现
    readonly ICloudUpload _cloud;               // 游戏侧自己的云上传抽象

    public CloudSaveManager(TransactionalSaveManager local, ICloudUpload cloud)
    {
        _local = local;
        _cloud = cloud;
    }

    public SaveResult Save<T>(string key, T data, int dataVersion)
    {
        var result = _local.Save(key, data, dataVersion);
        if (result.IsSuccess) _cloud.EnqueueUpload(key);   // 上传队列自行编排
        return result;
    }

    public LoadResult<T> Load<T>(string key, int currentVersion, ISaveMigrator<T> migrator)
    {
        var local = _local.Load(key, currentVersion, migrator);
        if (local.Status == LoadStatus.NoSave) return _cloud.TryDownload<T>(key); // 本地无档尝试云端
        return local;
    }
}
```

默认管理器 `TransactionalSaveManager` 经构造注入注册表 / 存储 / 变换链（依赖倒置）；注册表与变换链以引用
共享，门面后续注册 / 链变更立即生效。`SetStorage` 替换存储时默认管理器随之重建；自定义管理器自行编排存储。

## 变换链（压缩与加密）

按注册顺序施加（先压缩后加密），加载按存档记录的变换 Id 逆序还原；默认空链。

```csharp
SaveModule.SetTransformChain(new GZipSaveTransform(), new AesSaveTransform("my-secret-password"));
// 清除：SaveModule.SetTransformChain();
```

- `GZipSaveTransform`（Id `gzip`）：GZipStream 压缩。
- `AesSaveTransform`（Id `aes`）：AES-256，SHA256(password) 派生密钥；每次保存随机 IV 前置 16 字节。
  **密钥由游戏侧代码注入，不入配置资产、不入存档**；错误密钥加载判为损坏。
- 旧存档按各自记录的变换 Id 还原——链上每个 Id 须保持注册（缺失判损坏）。

## 版本迁移

保存声明 `dataVersion`，加载声明 `currentVersion`；不一致时经迁移器升级，未提供迁移器返回
`VersionMismatch`（携带两侧版本号）。迁移结果**不在加载路径写回磁盘**，下次保存自然落盘。

```csharp
public class MyMigrator : ISaveMigrator<MySaveData>
{
    public MySaveData Migrate(MySaveData data, int fromVersion)
    {
        if (fromVersion < 2) data.currency = data.gold;   // v2：gold 改名 currency
        return data;
    }
}

var result = SaveModule.Load("slot0", currentVersion: 2, migrator: new MyMigrator());
```

## 自动保存

```csharp
SaveModule.SetAutoSave(() => SaveModule.Save("slot0", Capture()), intervalSeconds: 60f);
// 切后台（OnApplicationPause(true)）与退出（OnApplicationQuit）按配置开关各触发一次
SaveModule.ClearAutoSave();   // 清除周期与钩子
```

周期计时复用 `AesirTimer.Every`（unscaled，暂停菜单 / 慢动作不影响存档节奏）。回调内异常向上传播
（fail-fast，游戏代码 bug 不兜底）。

## 事务式写入与备份回退（原理）

`FileSaveStorage.Write` 五步（同目录，`key.sav` / `key.sav.bak` / `key.sav.tmp`）：

1. 写 temp 全量字节（覆盖残留）
2. `FileStream.Flush(flushToDisk: true)` 强制落盘（Windows FlushFileBuffers / POSIX fsync）
3. 删旧 bak（Windows rename 到已存在名字会失败，备份槽是可牺牲副本）
4. 主 rename 为 bak
5. temp rename 为主（原子提交）

任意崩溃点之后至少一份完整可读存档；唯一非原子窗口（③→④ 之间）崩溃只损失备份副本，主存档完好。
加载校验链：magic → 信封格式版本 → 长度自洽 → CRC32；失败判损坏 → 回退备份（只读回退，不改写磁盘）。

## 安全边界与平台边界

- **CRC32 防意外损坏，不防对抗篡改**；防作弊需服务端校验，超出模块范围。
- **AES 防 casual 修改，不防逆向**——密钥在客户端二进制内可被提取。
- **WebGL**：`persistentDataPath` 是 IndexedDB 虚拟文件系统，`Flush(true)` 无真实 fsync 语义、rename 到
  持久化存在延迟窗口，崩溃安全弱于原生平台；关键进度建议配合云存档（`ISaveManager` 扩展点）。
- **同步契约**：KB 级存档同步写无感；MB 级大存档请**主线程捕获数据后自行后台写入**（序列化与变换是纯函数
  可后台，Unity 对象访问不可）。模块不内置异步 API。

## 设计边界（不做）

Unity 对象引用序列化（Easy Save 式）、云存档实现、PlayerPrefs 存储后端、多代备份轮换（固定 1 代 `.bak`）、
存档槽 UI、异步 / 多线程保存、加载路径磁盘修复（只读回退）。