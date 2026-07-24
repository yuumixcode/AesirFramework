# AGENTS.md

Unity/Tuanjie 自定义包集合：`Aesir Architecture`（MVP/MVC 框架）、`Aesir Modules`（UI 框架）、`Aesir Inspector`（规划中）。详细架构说明见 [`CODELY.md`](./CODELY.md)。

- **引擎：** Unity 2022.3.62f3c1（LTS），同时支持 Tuanjie 引擎
- **渲染管线：** URP 14.0.12
- **许可证：** MIT
- **默认分支：** `main`
- **语言：** C#；标识符用英文，XML 文档注释用中文

## 环境配置

在 Unity 2022.3.62f3c1（或 Tuanjie 等价版本）中打开项目根目录；首次导入时 Unity 会自动从 `Packages/manifest.json` 解析依赖。若不想开 GUI 预热包缓存：

```bash
Unity -batchmode -quit -projectPath . -nographics -logFile /dev/null
```

跑样例：打开 `Assets/Samples/Aesir Architecture/<version>/UI Counter-MVP/Scene/SampleForCounterMvp.unity`，按 **Play**。

## 构建与测试（CLI / batchmode）

构建：

```bash
Unity -batchmode -quit -projectPath . \
       -buildTarget StandaloneOSX -logFile build.log
```

跑测试（Edit 模式）：

```bash
Unity -batchmode -quit -projectPath . \
       -testPlatform editmode -runTests \
       -testResults TestResults.xml -logFile test.log
```

Play 模式测试：把 `-testPlatform` 换成 `playmode`。测试结果为 NUnit 风格 XML。

## 目录结构

- `Assets/Runestone/AesirArchitecture/` — 核心 MVP/MVC 框架（`Runtime/`、`Editor/`、`Tests/`）
- `Assets/Runestone/AesirModules/` — UI 框架（`UIManager`、`UIRoot`、面板、场景模块）
- `Assets/Runestone/AesirInspector/` — 占位目录（暂无代码）
- `Assets/Samples/` — 已导入的样例
- `Assets/Scenes/`、`Assets/Settings/` — Unity 资源
- `Assets/Plugins/Sirenix/` — Odin Inspector（第三方，**已 gitignore，别动**）
- `Packages/manifest.json` — UPM 依赖
- `ProjectSettings/` — Unity 工程配置

## 代码风格

- C#；类用 PascalCase，接口加 `I` 前缀，抽象类加 `Abstract` 前缀
- MonoBehaviour 单例：静态 `Instance` + `[DefaultExecutionOrder(-999)]` + `DontDestroyOnLoad`
- 通过 `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]` 自动启动
- 静态单例用 `ResetStaticsAssistant.Register()` 保证 Domain Reload 安全
- Runtime / Editor 代码用 asmdef 隔离；Odin 代码放独立 `*.OdinIntegration.asmdef`，由 `ODIN_INSPECTOR` 宏守护
- XML 文档注释用**中文**；标识符用**英文**

## 测试

- Edit 模式测试：`Assets/Runestone/AesirArchitecture/Tests/Editor/`
- Play 模式测试：`Assets/Runestone/AesirArchitecture/Tests/Runtime/`
- 框架：Unity Test Framework 1.1.33（NUnit 断言）
- 每个新行为都要补测试；PR 前所有测试必须通过

## PR 与提交规范

- 从 `main` 拉分支；**不要**直接推 `main`
- Conventional Commits（`feat:` / `fix:` / `docs:` / `refactor:` / `chore:`）
- 测试全绿后用 `gh pr create` 开 PR
- 发版时改对应包 `package.json` 的版本号，并在 `CHANGELOG.md` 加一条

## 样例同步

样例两边各有一份，必须保持一致：

- **源码**：`Assets/Runestone/<Package>/Samples~/<path>`（UPM 发布用，`path` 来自 `package.json` 的 `samples[]`）
- **开发副本**：`Assets/Samples/<Package 显示名>/<version>/<样例显示名>/`（直接编辑用）

发版流程：在 `Assets/Samples/` 下改完后跑：

```bash
python3 sync-samples.py to-source    # 开发副本 -> 源码
python3 sync-samples.py status       # 查看当前所有样例的对应关系
python3 sync-samples.py to-dev       # 源码 -> 开发副本（一般用于初次导入或回滚）
python3 sync-samples.py -n ...       # 任意命令加 -n 仅预览，不写盘
```

脚本会按 `package.json` 的 `displayName`（开发副本）↔ `path` 末段（源码）做自动映射，并在两边 meta 文件名不一致时自动重命名。

## 安全

- `Assets/Plugins/Sirenix/` 已 gitignore — Odin 是付费授权，**不要**提交或再分发
- `Library/`、`Temp/`、`UserSettings/`、IDE 自动生成文件已在 `.gitignore` 里，**不要** force-add
- 仓库里**不**存任何密钥；不要提交 `.env`、API Key、签名材料
