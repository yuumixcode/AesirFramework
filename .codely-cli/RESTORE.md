# Codely 环境还原

换机或新克隆仓库后，按本文恢复本机的 Codely CLI 开发环境。

原则：**自研扩展全量入库，其余一律轻量——只入库文本资产，扩展产物与本机凭证靠一条命令重建。**

## 已入库

| 路径 | 体积 | 作用 |
|---|---|---|
| `settings.json` | 4K | 项目级 MCP 服务配置（TJGenerators / AssetStore） |
| `memory.md` + `memory/` | 424K | 项目记忆、归档、辅助脚本 |
| `skills/` | ~300K | 项目级 Skill（version-sync / code-review-unity / sync-samples / unity-*） |
| `extensions/AesirFrameworkGame/` | 204K | **自研扩展，全量入库**：架构口径、六档分级、模块用法、实战模板 |

`AesirFrameworkGame` 是仓库自有资产（内容与 `TuanjieCowork/Skills` 同源），涉及「架构怎么做」与「让 AI 怎么用好这个框架」，必须全量版本化，不能靠重装获取。

## 不入库（重装即恢复）

| 路径 | 体积 | 重建方式 |
|---|---|---|
| `extensions/TJGenerators/` | 115M | 装扩展（见下节第一条），**Unity 打开前必须完成** |
| `extensions/codely-unity-lsp-server/` | 72M（其中 `worker/` 70M） | 装扩展，官方 zip 自带预编译 worker |
| `extensions/TMPChineseFont/` | 8M（主要是 Noto Sans SC 字体） | 装扩展 |
| `extensions/vtsls/` | 8K | 装扩展 |
| `UnityInsight/` | 数百 MB | 编辑器打开项目后自动重建索引 |
| `auto-saves/`、`tool-outputs/`、`tmp/`、`.codely/clipboard/` | — | 无需恢复 |

## 重装扩展

```bash
codely extensions install https://codely.tuanjie.cn/skills/download/f3f76746-78d0-4e83-9fc0-83e550e9ba62/TJGenerators.zip --scope workspace --consent
codely extensions install https://codely.tuanjie.cn/skills/download/4cd8be5b-b5bb-45bc-94f3-9ad6e0803962/codely-unity-lsp-server-0.1.4.zip --scope workspace --consent
codely extensions install https://codely.tuanjie.cn/skills/download/6e0cea3b-0fec-419e-a28a-e7fc86719f45/TMPChineseFont.zip --scope workspace --consent
codely extensions install https://codely.tuanjie.cn/skills/download/3c0c0bdd-db0a-424c-ac25-e3083fa1f123/vtsls.zip --scope workspace --consent
```

### 顺序要求：TJGenerators 先于 Unity

`Packages/manifest.json` 里有一条本地路径依赖：

```json
"cn.tuanjie.ai.generators": "file:../.codely-cli/extensions/TJGenerators/Packages/cn.tuanjie.ai.generators"
```

该目录不入库，所以**新克隆必须先装 TJGenerators 扩展，再打开 Unity**；否则 Unity 解析不到这个包，会报包缺失、相关脚本不编译。安装完成后路径与包名不变，manifest 无需改动。

## lsp server 的 worker

`worker/` 只有预编译产物（`worker/runtime/release/UnityRoslynWorker.dll` 及 Roslyn 依赖），不含 `.csproj` / `.cs` 源码。

- **恢复**：装 `codely-unity-lsp-server` 扩展即可，官方 zip 的 runtime 版自带预编译 worker。
- **从源码重建**（仅当需要改 worker，需完整源码包）：扩展目录内 `npm ci && npm run build` 生成 `dist/`，再 `dotnet build -c Release`（`UnityRoslynWorker.csproj` 的 `OutputPath` 指向 `worker/runtime/release/`）。

## 验证

```bash
codely extensions list
ls .codely-cli/extensions/TJGenerators/Packages/cn.tuanjie.ai.generators/package.json
ls .codely-cli/extensions/codely-unity-lsp-server/worker/runtime/release/UnityRoslynWorker.dll
node .codely-cli/extensions/codely-unity-lsp-server/dist/server.js   # stdio 服务，挂起等待即正常，Ctrl+C 退出
python3 .codely-cli/extensions/AesirFrameworkGame/scripts/validate.py   # 期望输出「校验通过：0 个错误，0 个警告」
```

## 本机凭证（不入库，需在新机重新登录）

`~/.codely-cli/mcp-oauth-tokens.json`、`oauth_creds.json`、`org.json`、全局 `settings.json`、`model-config.json` 都是账号态数据，新机登录 Codely 后自动重建。`.gitignore` 已按文件名模式兜底（`*token*.json` / `*secret*` / `*oauth*` / `*credential*` / `.env*` / `*.pem` / `*.p12` / `*.key`），即使被复制进本仓库也不会入库。
