## Purpose

定义框架 AI 扩展在文档站场景中处理 Zensical 工具链的行为契约：先确认用户是否真的需要站点，再由用户在项目内 venv 与项目内 uv 之间选择隔离方案，并始终禁止写入全局 Python 环境。

## ADDED Requirements

### Requirement: 使用 Zensical 前先确认是否需要

当 AI 因扩展技能而进入 Zensical 相关工作流（建站、本地预览、严格构建、CI 部署示例）时，MUST 先询问用户是否确实需要 Zensical；用户表示不需要时，AI MUST NOT 安装任何软件、MUST NOT 生成站点配置与部署流水线，只保留纯 Markdown 文档工作。

#### Scenario: 用户只需要文档不需要站点

- **WHEN** 用户请 AI 用项目结构技能整理 docs/ 文档，且明确表示不需要文档站
- **THEN** AI 不执行任何 Zensical 安装或构建命令，不写 `zensical.toml`，不写 Pages 部署 workflow

#### Scenario: 用户确认需要站点

- **WHEN** 用户确认需要本地或线上文档站
- **THEN** AI 进入安装方式选择步骤，不先行安装

### Requirement: 安装方式由用户在两种项目内隔离方案中选择

用户确认需要 Zensical 后，AI MUST 让用户选择项目内 venv 或项目内 uv，且 MUST 明确这两种方案的实际含义；AI MUST NOT 替用户决定、MUST NOT 默认采用其中一种、MUST NOT 在用户未作答时安装。

#### Scenario: 用户选择项目内 venv

- **WHEN** 用户选择 venv 方案
- **THEN** AI 给出的安装与调用命令全部限定在项目虚拟环境内，依赖与版本记录在项目内的依赖文件中

#### Scenario: 用户选择项目内 uv

- **WHEN** 用户选择 uv 方案
- **THEN** AI 使用项目内的 uv 依赖管理（`uv init` / `uv add` 锁版本，`uv run zensical` 调用），依赖与锁文件落在项目内

#### Scenario: 用户未选择或拒绝安装

- **WHEN** 用户未回答选择、或表态暂不安装
- **THEN** AI 不执行安装命令，不写站点配置与部署流水线

### Requirement: 禁止全局安装与全局调用

AI MUST NOT 执行会写入用户级或系统级 Python 环境的安装命令，包括但不限于 `pip install zensical`、`pip install --user zensical`、`pipx install zensical`、`uv tool install zensical`；MUST NOT 在未激活项目隔离环境的情况下调用 `zensical`。

#### Scenario: 技能文档含全局安装示例

- **WHEN** 扩展技能文本中出现裸 `pip install zensical` 或未限定环境的 `zensical serve` / `zensical build`
- **THEN** 该文本必须被改写为项目内隔离方案，且带禁止全局安装的提示

#### Scenario: 用户机器已有全局 zensical 可执行文件

- **WHEN** 用户环境中已经存在全局安装的 `zensical`
- **THEN** AI 不调用该全局可执行文件，仍按用户选择的项目内 venv 或项目内 uv 方式安装与调用

### Requirement: 门禁在技能文本中可发现

触发 Zensical 工作流的扩展技能 MUST 在正文保留可发现的门禁结论（先问是否需要、再让用户选隔离方式、禁止全局安装），MUST 在展开参考中提供两种隔离方案的可复制命令与禁止清单。

#### Scenario: AI 只读技能正文

- **WHEN** AI 只载入该技能的 SKILL.md 正文
- **THEN** 能看到门禁结论或指向展开参考的明确链接，不会把全局安装当作默认路径

#### Scenario: AI 读展开参考

- **WHEN** AI 按需读取该技能的 Zensical 展开参考
- **THEN** 能拿到项目内 venv 与项目内 uv 两套完整命令，以及禁止全局安装的清单
