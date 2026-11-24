# 变更记录

本扩展的版本号在 `gemini-extension.json` 的 `version`。技能正文的唯一主位是本扩展 `skills/`，包内不再有副本。

## 1.5.1 — 2026-11-24

纯文案修正，技能与功能零变化（按用户裁决不进规格文档）。

### 修复

- 依赖门禁 hook `hooks/detect-packages.mjs`：`MODULE_SKILLS` 补 `aesir-save`；"12 个技能全部可用"→"13 个技能全部可用"、"四个模块技能失效"→"五个模块技能失效"，与实际的 13 技能 / RAM 五模块对齐。
- `aesir-router/references/dependency-gate.md`：失效矩阵与"只有 RAA"行补 `aesir-save`，"全部 12 个技能"→"全部 13 个技能"、"四个模块"→"五个模块"。
- 主仓 `CODELY.md`：当前状态两处技能数 12→13（历史变更记录 1.3.0 那条 12 保持不动）。

### 变更

- **Zensical 安装门禁**：`aesir-project-structure` 技能明确——涉及 Zensical 前先问用户是否需要；需要则由用户在**项目内 venv** 与**项目内 uv** 之间二选一，**禁止全局安装**与复用机器已有的全局 `zensical`（CI 一次性 runner 不在此限）。展开参考给出两套锁 `0.1.1` 的命令与禁止清单。规格见 `openspec/changes/zensical-install-gate/`。

## 1.5.0 — 2026-11-23

新增保存模块技能（aesir-save），与框架仓 RAM SaveModule 同步（规格与验收口径见框架仓库 `openspec/changes/add-save-module/`）。

### 新增

- **`aesir-save` 技能**：SaveModule 保存/加载默认路径、三轨序列化选择表、四扩展点（ISaveSerializer / ISaveStorage / ISaveManager / ISaveTransform）、变换链与密钥管理、版本迁移、自动保存、八条陷阱。
- `aesir-router` 分派表、误路由表、组合路径补 save 路由；依赖门禁失效矩阵覆盖 aesir-save。

### 变更

- **触发描述再压缩**（为第 13 个技能在总和 ≤2800 硬约束内腾空间）：save / events / ui / core-api / scene / framework / pitfalls 七条 description 压缩，高辨识触发词保留。
- `aesir-router` 正文再瘦身至 ≤6000 字符（容纳 save 路由行）。
- 清单 version 1.4.0 → 1.5.0，description 与 keywords 同步 save 技能。

## 1.4.0 — 2026-11-23

按 Cowork 插件规范（结构、渐进披露、门面文档）优化扩展形态，技能的技术结论零变化。规格与验收口径见框架仓库 `openspec/changes/optimize-aesir-extension/`。

### 新增

- **依赖门禁 hook**：`hooks/hooks.json` + `hooks/detect-packages.mjs`，`SessionStart` 时用纯文件系统检查判定 RAA/RAM 的安装形态（内联 / UPM / 仅声明），注入一行结论与可用技能范围。非 Unity 工程零注入；任何异常静默放行；不写文件、不联网、不自动装包。可经 `hooks.disabled` 按名 `aesir-package-gate` 禁用。
- **人类可读门面**：本 `README.md`（概览 / 技能一览 / 安装 / 用法 / 维护）与本 `CHANGELOG.md`。
- **8 篇 references**（长内容下沉，正文只留决策与结论）：
  - `aesir-project-structure`：`directory-layout.md`、`docs-and-site.md`
  - `aesir-boot-scene`：`bootstrap-scripts.md`（两段完整脚本）
  - `aesir-ui`：`ui-pitfalls.md`（12 条详解）、`world-space-ui.md`、`ui-scene-ddol.md`
  - `aesir-events`：`dispatch-semantics.md`
  - `aesir-router`：`dependency-gate.md`
- **清单归属信息**：`author` / `license` / `repository` / `keywords`。
- **校验门禁六组**：清单归属字段、门面文档（含 CHANGELOG 版本号与清单一致）、正文与触发描述篇幅、系统垃圾文件、hook 配置结构；阈值集中为 `validate.py` 头部常量。

### 变更

- **5 个技能正文瘦身**（结论与默认路径全部保留，只搬展开与代码）：`aesir-project-structure` 15334 → 5599、`aesir-boot-scene` 8006 → 5107、`aesir-ui` 7561 → 5493、`aesir-router` 6466 → 5879、`aesir-events` 6170 → 5887 字符。UI 的 12 条坑位在正文保留编号 + 触发 + 后果速查表，详解进 references。
- **12 条触发描述压缩**：合计 4099 → 2799 字符（降 32%），单条 ≤260。删能力罗列，保留 API 名与用户原话；7 条独立场景说法回填，其余由更短子串覆盖。
- `aesir-router` 的依赖门禁第①步改为优先采信 hook 注入结论，未注入时按 references 手工探测；②读记忆与③询问安装两步不变。

### 移除

- 扩展根 `.DS_Store`（移入废纸篓，可回退）。

### 篇幅护栏（新增硬约束）

| 项 | 上限 |
|---|---|
| SKILL.md 正文（去 frontmatter） | 6000 字符 |
| 单条 frontmatter description | 260 字符 |
| 全部 description 总和 | 2800 字符 |

超限即 `validate.py` 报错（非警告）。references 不受篇幅上限约束。

## 1.3.0

- 新增 `aesir-project-structure` 技能，技能数达 12 个。
- 统一资产命名去类型前缀（场景／预制体／图片不加 `scn_` / `pref_`）与工程目录约定。
- 按 PlaneWar 三篇开发复盘优化 `aesir-boot-scene` / `aesir-framework` / `aesir-ui` / `aesir-router`：启动脚本模板补 `AssetDatabase.SaveAssets()`、主示例面板改用 `AesirBasePanelViewController<T>`、UI 坑位扩到 12 条并新增 `urp-ui-camera-stack.md`。

## 1.2.0 及更早

本扩展自 1.4.0 起才有独立 CHANGELOG。更早的演进见框架仓库 `CODELY.md` 的「AI 技能与 Codely 环境」节与本目录的 git 历史。