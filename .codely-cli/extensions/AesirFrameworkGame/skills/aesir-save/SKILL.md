---
name: aesir-save
description: Aesir Modules 保存模块（SaveModule）编码指南。做存档时使用——Save/Load 存档槽、事务写入崩溃不丢档、备份回退、三轨序列化、ISaveManager 自定义保存方案（云存档）、压缩加密、版本迁移、自动保存。触发词：SaveModule、存档、保存、读档、序列化、JsonUtility、存档损坏、版本迁移、自动保存、云存档、加密存档、ISaveSerializer、ISaveStorage、ISaveManager。
---

# Aesir 保存模块（SaveModule）

独立游戏级事务式存档门面：**存档槽键值 Save/Load** + **崩溃安全写入与备份回退** + **三轨序列化** + **四扩展点**。公开 API 全为静态成员，`SaveModule.xxx` 调用即用（首次调用自动创建，挂在 `[Aesir Modules]` 宿主下，零预放置零配置）。

> 前置：本包依赖 Aesir Architecture（RAA）。模块只存**纯数据对象**（POCO），不做 Unity 对象引用序列化。
>
> **缺包先过依赖门禁**：工程里没有 `cn.runestone.aesir.modules`（或连带缺 RAA）时，不要改用 `File.WriteAllText + JsonUtility` 直写目标文件顶替（写入中途崩溃即丢档）——先按 `aesir-router` 的「依赖门禁」探测 → 读记忆 → 询问安装（Git URL，CNB 优先、GitHub 兜底）。

## 默认路径

```csharp
using Runestone.AesirModules;

[Serializable]
public class MySaveData
{
    public int level;
    public float health;
    public List<string> inventory;
}

// 保存（事务式：temp 写入 → 刷盘 → 备份轮换 → 原子提交，任意崩溃点至少一份可读存档）
var saveResult = SaveModule.Save("slot0", new MySaveData { level = 3, health = 0.8f });
if (!saveResult.IsSuccess) Debug.LogWarning(saveResult.Error);

// 加载（主存档损坏自动回退备份；结果对象表达业务分支，不抛异常）
var result = SaveModule.Load<MySaveData>("slot0");
switch (result.Status)
{
    case LoadStatus.Success:
        Apply(result.Value);                    // result.Source 标明 Primary / Backup
        break;
    case LoadStatus.NoSave:
        StartNewGame();                         // 从未存档
        break;
    case LoadStatus.Corrupted:
        ShowSaveCorruptedDialog();              // 主备均损坏
        break;
}

SaveModule.Delete("slot0");                     // 删主存档与备份
SaveModule.Exists("slot0");
SaveModule.GetKeys();                           // 存档槽列表（做选档 UI）
```

**存档键**仅字母、数字、下划线、连字符（`"slot_0"`、`"Quicksave-1"`）；含路径分隔符 / 空白 / `..` 抛 `ArgumentException`（防路径穿越）。

## API 速查

```csharp
SaveModule.Save<T>(string key, T data, int dataVersion = 0);        // → SaveResult
SaveModule.Load<T>(string key, int currentVersion = 0, ISaveMigrator<T> migrator = null); // → LoadResult<T>
SaveModule.Delete / Exists / GetKeys;                               // 直通存储

SaveModule.RegisterSerializer(ISaveSerializer serializer);          // 同 Id 覆盖
SaveModule.SetStorage(ISaveStorage storage);                        // 默认管理器随之重建
SaveModule.SetManager(ISaveManager manager);                        // 整体接管保存/加载
SaveModule.SetTransformChain(params ISaveTransform[] transforms);   // 空数组清除

SaveModule.SetAutoSave(Action saveAction, float intervalSeconds);   // 周期 + 切后台/退出钩子
SaveModule.ClearAutoSave();
SaveModule.ApplyConfig(SaveConfigSO config);                        // null 回退 Instance
```

`LoadResult<T>`：`Status`（Success/NoSave/Corrupted/VersionMismatch/SerializerMissing）、`Value`、`Source`（Primary/Backup）、`Error`、`SavedVersion`/`CurrentVersion`。IO 环境失败用结果表达；编程错误（非法键/null 数据）抛异常。

预放置实例可在 Inspector 调：`config`（`SaveConfigSO`）、`dontDestroyOnLoad`。配置资产经菜单 `Tools/Aesir/Modules/Create Module Config Assets` 显式创建（包导入不自动写工程）；无资产时内存默认值兜底。

## 三轨序列化怎么选

| 数据形状 | 选轨 | 注册 |
|---|---|---|
| 公开字段 + List + Vector3 | `unity-json`（默认，零依赖） | 无需注册 |
| Dictionary / 多态 / 私有字段，装了 Odin | `odin` | `RegisterSerializer(new OdinSaveSerializer())` |
| 上述 + 属性，装了 Newtonsoft 包 | `newtonsoft` | `RegisterSerializer(new NewtonsoftJsonSerializer())` |

```csharp
SaveModule.RegisterSerializer(new OdinSaveSerializer());
var config = SaveConfigSO.CreateDefault();
config.defaultSerializerId = "odin";
SaveModule.ApplyConfig(config);
```

每份存档记录序列化器 Id，加载按记录还原——切换默认不破坏旧档。JsonUtility 不支持 Dictionary/多态/属性、null 集合反序列化为空——需要这些形状必须换轨，不要给 JsonUtility 硬塞 Dictionary。

## 自定义保存管理方案（云存档等）

实现 `ISaveManager`（只有 Save/Load 两方法）经 `SetManager` 整体替换；删除/存在性/枚举仍直通存储。典型骨架：本地仍委托默认 `TransactionalSaveManager`，Save 成功后入队云上传、Load 本地无档时尝试云端下载。自定义管理器遵守同一结果语义（环境失败用结果、编程错误抛异常）。存储后端也可单独换（`ISaveStorage` 契约：提交式写入 + `ReadBackup`）。

## 压缩 / 加密 / 迁移 / 自动保存

```csharp
// 变换链：先压缩后加密；密钥代码注入，不入资产不入存档；旧档按记录的变换 Id 还原
SaveModule.SetTransformChain(new GZipSaveTransform(), new AesSaveTransform("my-secret"));

// 版本迁移：不一致且无迁移器 → VersionMismatch；迁移结果不在加载路径写回磁盘
var result = SaveModule.Load("slot0", currentVersion: 2, migrator: new MyMigrator());

// 自动保存：周期不受 timeScale 影响；切后台/退出按配置开关各触发一次
SaveModule.SetAutoSave(() => SaveModule.Save("slot0", Capture()), 60f);
```

## 陷阱

- **直写目标文件丢档**：`File.WriteAllText(path)` 写入中途崩溃即截断——这正是本模块事务写入解决的事故，不要绕开模块自写。
- **序列化器 Id 改名**：Id 写入每份存档信封，改名等于旧档全部 `SerializerMissing`；发布后不可改。
- **变换链 Id 缺失**：加载按存档记录的变换 Id 查注册表，链上每个 Id 须保持注册（缺失判损坏）；加密档换密钥加载判损坏（不是静默错读）。
- **密钥入资产**：`SaveConfigSO` 没有加密字段是有意设计——资产进版本库即泄密；密钥只走代码注入。
- **版本不匹配静默错读**：不传 `currentVersion` 与迁移器时旧档按新类型反序列化可能字段错位——声明版本并按需给迁移器。
- **WebGL 崩溃安全弱化**：IndexedDB 虚拟文件系统无真实 fsync，关键进度配合云存档扩展点。
- **大存档卡主线程**：全同步契约；MB 级数据主线程捕获后自行 `Task.Run` 写入（序列化/变换是纯函数可后台，Unity 对象访问不可）。
- **CRC32/AES 不是防作弊**：校验防意外损坏、加密防 casual 修改；密钥在客户端可被提取，防篡改需服务端校验。