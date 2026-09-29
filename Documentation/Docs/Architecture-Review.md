# AesirArchitecture 代码审查报告

## 审查范围与方法

- **目标包：** `Assets/Runestone/AesirArchitecture`
- **检查内容：** Runtime、Editor、包内 Tests、渐进式样例与相关说明。
- **明确排除：** 未审查 `Assets/Runestone/AesirModules` 的实现代码；仅在说明架构包测试边界时指出其测试代码会扫描 Modules 样例目录。
- **限制：** 本次只做审查，没有修改项目代码。

## 总体结论

架构包的渐进式设计意图清楚：MVC/MVP 均有 Quick、Standard、Strict 示例，分别从直接读写和较少抽象，逐步过渡到接口边界、Command/Query 与分离的 View/Controller/Presenter。核心 Runtime 与 Editor、Odin 扩展分层；Odin 作为可选 Editor 扩展，没有成为 Runtime 必需依赖。核心公共 API 注释整体较完整。

当前需要优先处理的是更新器的路径安全、生命周期释放异常时的清理一致性，以及 PlayerLoop 延迟命令异常后的重放风险。测试覆盖面整体较广，但关键异常边界尚有空白，且有部分测试属于 Unity/Editor 集成测试，不应全部视为纯单元测试。

## 主要发现

### AARCH-01 — 高：更新清理路径未做目录边界校验，可能删除包目录以外的文件

**位置**

- `Editor/UpdateChecker/AesirUpdateService.cs:1571–1600`（`ComputeStaleFiles`）
- `Editor/UpdateChecker/AesirUpdateService.cs:1644–1673`（`DeleteStaleEntries`）
- `Editor/UpdateChecker/AesirUpdateService.cs:241–242`（`ToAbsolutePath`）
- `Editor/UpdateChecker/AesirUpdateService.cs:1807–1810`（更新流程调用）

**问题与影响**

`ComputeStaleFiles` 仅以字符串前缀判断旧清单路径属于目标包，再将剩余子路径拼到本地包路径。它没有拒绝 `..` 路径段或校验规范化后的绝对路径仍处于包根目录内。后续 `DeleteStaleEntries` 把该路径交给 `ToAbsolutePath`（会执行 `Path.GetFullPath`），然后直接调用 `File.Delete`。

因此，若本地 `.aesir/installed-manifest.json` 中的旧文件路径被篡改或损坏为目标包前缀下带有 `../` 的路径，规范化后可能指向包目录外的项目文件；更新器随后会按该路径删除文件。现有测试覆盖了不误匹配 `AesirArchitecture.Samples` 等同名前缀目录（`Tests/Editor/AesirUpdateServiceTests.cs:251–267`），但没有覆盖路径穿越或绝对路径输入。

**建议修复**

在生成待删差集与执行删除两处都做校验：拒绝绝对路径、盘符路径、`.`/`..` 段及不允许的分隔符形式；规范化完整路径后，使用带目录分隔符边界的路径包含判断，确保最终目标严格位于目标包根目录内。任一条目校验失败时应跳过并记录诊断，不要尝试删除。增加隔离临时目录的回归测试，证明越界文件始终保留。

### AARCH-02 — 中：释放回调抛异常时，Submodule 与 Context 的释放状态不一致

**位置**

- `Runtime/Core/Engine/Modules/Abstracts/AbstractSubmodule.cs:44–49`
- `Runtime/Core/Engine/Context/AbstractContext.cs:273–300`

**问题与影响**

`AbstractSubmodule.Dispose()` 先调用可覆写的 `OnDispose()`，之后才清空上下文引用并重置 `Initialized`。如果子类清理逻辑抛异常，后两项不会执行。`AbstractContext.Dispose()` 逐个释放 Service 与 Model；任何一个模块抛异常都会中断后续释放和 Locator 清空（第 277–289 行）。Context 的 `finally` 虽会将 `Initialized` 设为 `false` 并解除单例缓存，但旧 Context 再调用 `Dispose()` 会因 `Initialized == false` 直接返回，无法完成剩余清理。

这与常规成功释放路径下的状态保证不同：单个模块清理失败可能让其他模块未释放，并让失败模块继续持有上下文引用。

**建议修复**

将 `AbstractSubmodule` 的状态重置和上下文断开移入 `finally`。Context 释放时逐个模块隔离失败，确保后续模块与 Locator 仍被清理；完成后再按既定策略向上抛出原异常或聚合异常。增加 `OnDispose()` 抛异常的测试，验证所有模块均被尝试释放、引用状态可预测且再次调用 `Dispose()` 不会造成重复释放。

### AARCH-03 — 中：PlayerLoop 待处理命令执行失败时会保留并重放已执行命令

**位置**

- `Runtime/Common/AesirPlayerLoop.cs:279–287`
- `Runtime/Common/AesirPlayerLoop.cs:325–349`

**问题与影响**

`ExecutePendingCommands()` 在遍历所有待处理命令完成后才调用 `PendingCommands.Clear()`。某个命令抛异常时，清空语句不会执行；此前已经运行过的命令仍留在列表里。异常从 `InvokeHooks()` 的 `finally` 传播后，下一次阶段回调可能再次执行这些命令，导致注册/移除等状态变更重复发生。

**建议修复**

明确命令失败策略，并确保每条命令不会因前一条命令抛错而被重复执行。可按队列逐条出队后再执行，或先取出并清空快照后执行；若需要继续剩余命令，应收集异常并在执行完后再统一报告。新增命令中途抛异常的测试，断言已执行命令不重放、剩余命令行为符合契约。

### AARCH-04 — 中：轻量 JSON 字段提取器不遵守字符串转义规则

**位置**

- `Editor/UpdateChecker/AesirUpdateService.cs:327–355`
- 远程版本标签使用处：`Editor/UpdateChecker/AesirUpdateService.cs:717–720`

**问题与影响**

`ExtractJsonField` 通过字段后的第一个 `"` 和之后第一个 `"` 截取值，不识别 JSON 中的 `\"`、反斜杠及其他转义序列，也不还原 JSON 转义。合法 JSON 字符串若包含转义引号，返回值会被截断；该函数用于读取远程 Release 响应的 `tag_name`，也作为公开 API 提供。

**建议修复**

改用 Unity 可用的 JSON 反序列化载体读取字段，或实现具备完整 JSON 字符串转义处理的解析逻辑。对缺字段、错误类型和非法 JSON 的返回契约应明确，并补充引号、反斜杠、Unicode 转义及畸形输入测试。

### AARCH-05 — 中：UPM Sample 导入异常会绕过结果枚举与用户提示

**位置**

- `Editor/GetStarted/AesirGetStartedService.cs:649–710`
- 默认查询/导入调用：`Editor/GetStarted/AesirGetStartedService.cs:717–739`
- UI 调用处：`Editor/GetStarted/AesirGetStartedWindow.cs:199–215`

**问题与影响**

`ConfirmAndImportUpmSample` 最终调用 `ImportUpmSample`；finder、默认 `Sample.FindByPackage` 查询与 `Sample.Import` 均没有异常转换逻辑。异常会直接从 Editor 按钮调用链传播，无法按 API 的 `Failed` 结果和 message 流程向用户显示重试指引。测试注入的 `UpmSampleHandle.Import` 委托为空时也会直接触发空引用异常。

**建议修复**

在 Package Manager 查询/导入边界处理预期异常，将可恢复失败转换为 `Failed` 和明确消息，并保留异常日志。调用可空委托前显式校验。补充 finder 与导入委托抛异常、空委托的测试，明确错误是否向上抛出的 API 契约。

### AARCH-06 — 低：公开 Editor API 的 XML 文档缺少参数、返回值和边界说明

**位置**

- `Editor/UpdateChecker/AesirUpdateService.cs:327–328`（`ExtractJsonField`）
- `Editor/UpdateChecker/AesirUpdateService.cs:1640–1644`（`DeleteStaleEntries`）

**问题与影响**

上述公开方法有摘要，但没有完整的 `<param>` 与 `<returns>` 契约；`DeleteStaleEntries` 也未说明空输入、失败及路径必须安全位于包根目录内等边界。对会删除文件的 API，仅有“项目相对路径”说明不足以指导调用者安全使用。

**建议修复**

为所有公开成员补齐精简的参数、返回值及异常/边界说明，并确保文档与实际实现一致。路径安全必须由运行时校验保障，不能只靠注释约定。

### AARCH-07 — 低：Architecture 测试会扫描 AesirModules 样例，测试边界跨包

**位置**

- `Tests/Editor/AesirSamplesScriptGuardTests.cs:53–80`

**问题与影响**

Architecture 包中的 `RuntimeSampleScripts_AreWholeFileEditorWrapped` 测试会枚举安装根下的 `AesirModules/Samples`、`AesirModules/Samples~`，也会扫描 `Assets/Samples/Aesir Modules`。这使单独运行 Architecture 测试时仍会检查另一个包的样例；它不属于本次审查的实现范围，也会造成包间测试耦合。

**建议修复**

若该守护只服务 Architecture 包，限制为 Architecture 安装根与 Architecture Package Manager Samples。若它本意是仓库级跨包规则，应将其移入独立的仓库集成测试，并清楚标记测试范围。

### AARCH-08 — 低：静态重置注册接口接受空回调并静默跳过

**位置**

- `Runtime/Common/ResetStaticsAssistant.cs:35–50`
- 空回调跳过位置：`Runtime/Common/ResetStaticsAssistant.cs:69–74`

**问题与影响**

公开的 `Register(Action callback)` 不拒绝 `null`，会将其加入回调列表；后续重置时该项被静默跳过。调用方的注册错误因此不会及时暴露，相关静态状态可能未按预期重置。当前未发现直接测试 `Register` 空值及重置回调执行语义的专项用例。

**建议修复**

注册时对空回调抛出 `ArgumentNullException`，并增加空值、多个回调及某回调抛异常后其余回调仍执行的单元测试。

## 测试审查结果

### 实际执行结果

- `Runestone.AesirArchitecture.Tests.Editor`：**242 项通过，0 项失败，0 项跳过**。
- `Runestone.AesirArchitecture.Tests`：发现 **17 项 PlayMode 测试**，本次未执行。当前项目还包含 AesirModules 的 PlayMode 测试；为遵守本次只审查 Architecture 的范围，没有启动会连带执行其他包用例的未过滤 PlayMode 测试。
- 以上结果只代表这次执行的 EditMode 测试通过，不代表未执行的 PlayMode 测试通过。

### 覆盖优点与不足

- Locator、Context 注册/初始化、Command/Query 能力、MiniEvent、Observable 值与集合、PlayerLoop、Editor 更新器均有针对性测试；不少测试对事件载荷、执行顺序、对象状态及失败结果有明确断言。
- `Tests/Editor` 中既有纯逻辑测试，也有依赖 AssetDatabase、文件系统、GameObject、SerializedObject 或 Editor 状态的测试；`Tests/Runtime` 中有依赖 Unity 帧与 SceneManager 的 PlayMode 测试。因此不能把整个测试程序集都视为纯单元测试，建议在测试文档或 CI 报告中区分 Unit、Editor Integration 与 PlayMode Integration。
- 建议优先补齐与 AARCH-01 至 AARCH-05 对应的失败路径测试；另建议为 `AbstractSubmodule.Dispose()`、`ResetStaticsAssistant.Register()` 建立直接专项测试。现有 `MiniEventTests` 覆盖句柄重复释放、异常 fail-fast 与事件清理，但未覆盖回调中新增/移除监听以及 Dispose 后重新订阅的约定；建议明确该生命周期契约并测试。

## 建议修复顺序

1. **先修复 AARCH-01**：防止更新流程删除包目录外文件，并添加越界回归测试。
2. **处理 AARCH-02 与 AARCH-03**：统一异常路径下资源/命令状态，避免遗留引用或重复执行。
3. **处理 AARCH-04 与 AARCH-05**：修正 JSON 解析与 Sample 导入失败契约。
4. **补充 AARCH-06 至 AARCH-08**：完善公开 API 文档、隔离测试范围并补足输入校验。

## 本次改动说明

只新增了本审查报告；项目代码未作修改。当前报告路径：`Assets/Runestone/AesirArchitecture/Documentation/Docs/Architecture-Review.md`。
