# 保存模块（SaveModule）与 Easy Save 3 对标

## Easy Save 3 现状（2026-11-23 实测）

- 项目内已安装第三方插件 Easy Save 3，**源码版本 3.5.27**，路径 `Assets/Plugins/Easy Save 3/`（Scripts/ 196 个 .cs）。
- 全静态门面 `ES3`（Save/Load/KeyExists/DeleteKey/GetKeys/SaveRaw/EncryptBytes/CompressBytes 等）；
  配置载体 `ES3Settings`（默认来自 `Resources/ES3/ES3Defaults`，path 默认 `SaveFile.es3`）；
  分层 Reader/Writer → Stream(File/PlayerPrefs/Resources/Cache) → `ES3Crypto`(AES/Gzip)。
- 枚举：`Location{File,PlayerPrefs,InternalMS,Resources,Cache}`（WebGL/tvOS 自动降级 PlayerPrefs）、
  `EncryptionType{None,AES}`、`CompressionType{None,Gzip}`、`Format{JSON}`、`ReferenceMode{ByRef,ByValue,ByRefAndValue}`。

### ES3 真正独占 / 值得借鉴的能力

1. **Unity 对象引用序列化**（真正独占）：`ES3UnityObjectType` 写 `writer.WriteRef`，靠随机 long `_ES3Ref`
   （编辑期写进场景/Prefab）+ 引用表 `ES3ReferenceMgr` 跨会话还原；`ES3Prefab.prefabId` 支持运行时实例化，
   场景外资产走 `ES3GlobalReferences`。**可借鉴稳定 ID + 引用表思路，不引入全场景依赖扫描 + Postprocessor + 全局引用表重机制。**
2. **类型系统**：`ES3TypeMgr` 注册 170+ 个手写 `ES3Type_*`（Unity 组件 / 粒子模块 / 集合 / 基础类型），未命中回落反射。
   可借鉴显式注册表思路，不照搬海量手写类型文件。
3. **ES3File 内存缓存批量提交**：`new ES3File` 全量读入，`Save/Load` 只改内存字典，`Sync()`/`StoreCachedFile()` 才落盘，
   dirty 标记避免无谓写。**这是本次借鉴的核心（见下 enhance-save-module）。**
4. 其它：场景级 AutoSave（`ES3AutoSave`+`ES3AutoSaveMgr`）、存档槽 UI（依赖 TMPro/UGUI）、ES3Spreadsheet、
   `GetTimestamp`、Visual Scripting 集成——对独立游戏精简框架多属过度设计。

## 与现有 SaveModule 的对比结论

`add-save-module` 的设计文档（`openspec/changes/add-save-module/design.md`）早已把 Easy Save 3 列为核心参照，
并明确排除：**Unity 对象引用序列化、场景 AutoSave、存档槽 UI、PlayerPrefs 后端、多代备份**——
这些正是 ES3 最重、最过度设计的部分，引入即违背 RAA 极简第一排序标准（RAM 功能性优先但仍有边界）。

现有模块已覆盖：事务写入、备份回退、三轨序列化（unity-json/odin/newtonsoft）、四扩展点、变换链（AES+GZip）、版本迁移、自动保存。
对照 ES3 仍缺的高频低风险能力只有三项 → 已立变更（见下）。

## enhance-save-module 变更（2026-11-23，规划完成待实施）

- 位置 `openspec/changes/enhance-save-module/`（gamespec-code schema），4/4 工件完成，`openspec validate` 0 issue。
- **三项增强**：① 批量原子保存会话（借鉴 ES3File）；② `LoadOrDefault<T>` 默认值重载；③ `GetTimestamp` 存档时间戳。
- **用户裁决**：三项全做；走 openspec 规格流程（先提案再实施），已确认。
- **关键设计决策**（详见该变更 design.md）：
  - D1 批量会话 = 一个批次键，条目是批次内命名 payload，整份复用现有信封 + temp/flush/双 rename 事务写，
    原子边界 = 一个批次键内部（不是跨键全局原子），信封格式版本保持 1、零 BREAKING。**实施收敛**：`SaveBatch` 自身即 `[Serializable]` 载荷、条目字节用 Base64 存（裸值受 unity-json 形状约束，须包 POCO——与单条 Save 同限制）。
  - D3 `LoadOrDefault<T>` 用**独立方法名**而非重载 `Load`——`Load<int>(key,5)` 会与现有 `currentVersion` 参数签名歧义。
  - D4 时间戳走**独立可选接口** `ISaveStorageTimestamp`，不并入 `ISaveStorage`（避免破坏自定义实现）；不支持返回 null。
  - 不引入 ES3 的对象引用序列化 / 场景 AutoSave / 存档槽 UI / 多 Location。
- **状态（2026-11-23）**：12/12 任务全部实施完成，门禁全绿（EditMode 943/940/0/3、PlayMode 41/41、sync 零漂移、validate 0 错、zensical 0 issue）。用户裁决**暂不归档**——因 openspec/specs/ 为空、add-save-module（31/31 未归档）才是 save-module 基础规格来源，待与它按正确顺序一起归档。变更留在 changes/ 活跃区。

## 复用要点

- 再被问「要不要引入 Easy Save 3 的 XX 功能」：先对照上面「独占能力」与 add-save-module 已排除边界，
  默认结论是只提取设计思路、按 Aesir 风格自有实现（借鉴第三方付费插件时替换命名/风格、禁止 1:1 复刻，见主索引 2026-11-23 条目）。
