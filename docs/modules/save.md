# 保存模块

保存模块提供面向独立游戏的事务式存档门面:`SaveModule` 单例的公开 API 全为静态成员,零配置调用即用 —— 存档槽 Save/Load、崩溃安全写入、完整性校验与备份回退、三轨序列化、四个可替换扩展点、版本迁移与自动保存。

## 核心能力

- **事务式写入** —— 新数据先写同目录临时文件并强制刷盘(`Flush(flushToDisk: true)`),再经双 rename 备份轮换与原子替换提交;写入流程的任意中断点(进程崩溃 / 断电)之后,磁盘上至少保留一份完整可读存档(旧值或新值)
- **校验与回退** —— 每份存档携带二进制信封(magic + 格式版本 + 序列化器标识 + 变换链标识 + 数据版本 + CRC32 校验和);加载校验失败自动回退上一份备份,结果对象标明来源(Primary / Backup);加载路径无磁盘写副作用
- **三轨序列化** —— Unity JsonUtility(默认,零依赖)/ Odin 二进制(Dictionary、多态,`ODIN_INSPECTOR` 门控)/ Newtonsoft JSON(可选集成,未安装包时整体编译剔除);每份存档记录序列化器 Id,切换默认不破坏旧档
- **四扩展点** —— `ISaveSerializer`(对象↔字节)/ `ISaveStorage`(字节持久化,契约含提交式写入与备份读取)/ `ISaveManager`(保存管理编排,**自定义保存方案总入口**)/ `ISaveTransform`(字节变换链);实现 `ISaveManager` 即可整体接入云存档等自定义方案
- **版本迁移** —— `ISaveMigrator<T>` 按声明版本升级旧档;未提供迁移器返回 `VersionMismatch` 状态而非静默错读;迁移结果不在加载路径写回磁盘
- **自动保存** —— 周期计时复用 RAA `AesirTimer`(unscaled,暂停菜单不影响存档节奏)+ `OnApplicationPause` / `OnApplicationQuit` 钩子(配置可关)

## 快速开始

零配置即可使用 —— 无预放置、无配置资产时使用代码默认值(存储目录 `Saves`、默认序列化器 `unity-json`、备份开启):

```csharp
using Runestone.AesirModules;

[Serializable]
public class MySaveData
{
    public int level;
    public float health;
    public List<string> inventory;
}

// 保存:事务式写入,任意崩溃点至少一份可读存档
var saveResult = SaveModule.Save("slot0", new MySaveData { level = 3, health = 0.8f });

// 加载:主存档损坏自动回退备份;业务分支用结果对象表达,不抛异常
var result = SaveModule.Load<MySaveData>("slot0");
if (result.IsSuccess)
    Apply(result.Value);          // result.Source 标明 Primary / Backup
else if (result.Status == LoadStatus.NoSave)
    StartNewGame();

SaveModule.Delete("slot0");       // 删主存档与备份
SaveModule.GetKeys();             // 存档槽列表(选档 UI)
```

存档键仅字母、数字、下划线、连字符(白名单校验防路径穿越);IO 等环境失败以结果表达,编程错误(非法键 / null 数据)抛异常。

## API 速查

```csharp
// 保存与加载
SaveModule.Save<T>(string key, T data, int dataVersion = 0);                    // → SaveResult
SaveModule.Load<T>(string key, int currentVersion = 0, ISaveMigrator<T> migrator = null); // → LoadResult<T>
SaveModule.Delete(key); SaveModule.Exists(key); SaveModule.GetKeys();

// 扩展点替换
SaveModule.RegisterSerializer(ISaveSerializer serializer);    // 同 Id 覆盖
SaveModule.SetStorage(ISaveStorage storage);                  // 默认管理器随之重建
SaveModule.SetManager(ISaveManager manager);                  // 整体接管保存/加载
SaveModule.SetTransformChain(params ISaveTransform[] transforms);

// 自动保存与配置
SaveModule.SetAutoSave(Action saveAction, float intervalSeconds);
SaveModule.ClearAutoSave();
SaveModule.ApplyConfig(SaveConfigSO config);
```

`LoadResult<T>` 状态:`Success` / `NoSave` / `Corrupted` / `VersionMismatch` / `SerializerMissing`。配置资产 `SaveConfigSO` 经菜单 `Tools/Aesir/Modules/Create Module Config Assets` 显式幂等创建(包导入不自动写工程)。

## 三轨序列化怎么选

| 数据形状 | 选轨 | 注册 |
|---|---|---|
| 公开字段 + List + Unity 值类型 | `unity-json`(默认) | 无需注册 |
| Dictionary / 多态 / 私有字段(装了 Odin) | `odin` | `RegisterSerializer(new OdinSaveSerializer())` |
| 上述 + 属性(装了 Newtonsoft 包) | `newtonsoft` | `RegisterSerializer(new NewtonsoftJsonSerializer())` |

JsonUtility 不支持 Dictionary / 多态 / 属性,null 集合反序列化为空 —— 需要这些形状必须换轨,不要硬塞。

## 自定义保存管理方案(云存档等)

实现 `ISaveManager`(只有 Save/Load 两方法)经 `SetManager` 整体替换;删除 / 存在性 / 枚举仍直通存储。典型骨架:本地仍委托默认 `TransactionalSaveManager`,Save 成功后入队云上传、Load 本地无档时尝试云端下载。压缩与加密经变换链按序施加(先压缩后加密),密钥由游戏侧代码注入、不入配置资产不入存档:

```csharp
SaveModule.SetTransformChain(new GZipSaveTransform(), new AesSaveTransform("my-secret"));
```

## 安全边界与平台边界

- **CRC32 防意外损坏,不防对抗篡改**;防作弊需服务端校验,超出模块范围
- **AES 防 casual 修改,不防逆向** —— 密钥在客户端二进制内可被提取
- **WebGL** 的 persistentDataPath 是 IndexedDB 虚拟文件系统,无真实 fsync 语义,崩溃安全弱于原生平台;关键进度建议配合云存档扩展点
- **全同步契约**:KB 级存档无感;MB 级大存档主线程捕获数据后自行后台写入(序列化与变换是纯函数可后台,Unity 对象访问不可)

## 设计边界

不做 Unity 对象引用序列化(GameObject / Component / 资产引用)、云存档实现(只留扩展点)、PlayerPrefs 存储后端、多代备份轮换(固定 1 代 `.bak`)、存档槽 UI、异步 / 多线程保存、加载路径磁盘修复(只读回退)。