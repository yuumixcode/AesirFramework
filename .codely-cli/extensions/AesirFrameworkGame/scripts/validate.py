#!/usr/bin/env python3
"""校验 AesirFrameworkGame 扩展的结构与内容一致性。

检查项:
  1. gemini-extension.json 存在、UTF-8 无 BOM、可解析，含 name/version/description
     与归属字段 author/license/repository/keywords
  2. 门面文档：README.md 存在且五节齐全；CHANGELOG.md 存在且顶部版本号与清单一致
  3. 每个技能目录都有 SKILL.md，frontmatter 含 name 与 description
  4. frontmatter 的 name 与目录名一致，且为 kebab-case
  5. 篇幅护栏：正文 ≤ MAX_BODY_CHARS、单条 description ≤ MAX_DESC_CHARS、
     全部 description 总和 ≤ MAX_DESC_TOTAL（references 不受限）
  6. Markdown 内的相对 *.md 链接都能解析到真实文件
  7. 目录洁净：skills/ 下无 Unity *.meta；整个扩展无 .DS_Store / Thumbs.db
  8. hooks/hooks.json（若存在）：合法 JSON、事件名在白名单、命令用 ${extensionPath}
     相对路径而非机器绝对路径、引用的脚本真实存在
  9. 包内不再出现 TuanjieCowork~/Skills（技能唯一主位是本扩展；仅在能定位仓库根时检查）

用法:
    python3 validate.py            # 全量校验
    python3 validate.py --quiet    # 只在出错时输出

退出码: 0 全部通过；1 存在错误。
"""

from __future__ import annotations

import json
import re
import sys
from pathlib import Path

# ── 护栏阈值（改护栏只改这里）──
MAX_BODY_CHARS = 6000
MAX_DESC_CHARS = 260
MAX_DESC_TOTAL = 2800

REQUIRED_MANIFEST_FIELDS = ("name", "version", "description", "author", "license", "repository", "keywords")
README_SECTIONS = ("## 概览", "## 技能一览", "## 安装", "## 用法", "## 维护")
JUNK_NAMES = {".DS_Store", "Thumbs.db"}
# Codely 实测支持前三个（TJSpec 在用）；其余为插件生态通用事件名，一并放行避免误报
HOOK_EVENTS = {
    "SessionStart", "BeforeAgent", "FileChanged", "SessionEnd",
    "PreToolUse", "PostToolUse", "Stop", "SubagentStop",
    "UserPromptSubmit", "PreCompact", "Notification",
}
ABS_PATH_RE = re.compile(r"(?:/Users/|/home/|[A-Za-z]:[\\/])")
EXT_PATH_RE = re.compile(r"\$\{extensionPath\}([^\"'\s]+)")
SEMVER_RE = re.compile(r"^\d+\.\d+\.\d+$")
KEBAB_RE = re.compile(r"^[a-z0-9]+(-[a-z0-9]+)*$")
MD_LINK_RE = re.compile(r"\[[^\]]*\]\(([^)\s]+)\)")
FRONTMATTER_RE = re.compile(r"^---\n.*?\n---\n?", re.S)

errors: list[str] = []
warnings: list[str] = []


def fail(message: str) -> None:
    errors.append(message)


def warn(message: str) -> None:
    warnings.append(message)


def parse_frontmatter(text: str) -> dict[str, str]:
    """极简 YAML frontmatter 解析：只取顶层 key: value。"""
    if not text.startswith("---"):
        return {}
    parts = text.split("---", 2)
    if len(parts) < 3:
        return {}
    result: dict[str, str] = {}
    for line in parts[1].splitlines():
        if not line.strip() or line.lstrip().startswith("#"):
            continue
        if line.startswith((" ", "\t")):
            continue
        if ":" not in line:
            continue
        key, value = line.split(":", 1)
        result[key.strip()] = value.strip()
    return result


def check_manifest(ext_root: Path) -> dict:
    manifest_path = ext_root / "gemini-extension.json"
    if not manifest_path.is_file():
        fail("缺少 gemini-extension.json")
        return {}

    raw = manifest_path.read_bytes()
    if raw.startswith(b"\xef\xbb\xbf"):
        fail("gemini-extension.json 含 BOM（应为 UTF-8 无 BOM）")

    try:
        manifest = json.loads(raw.decode("utf-8-sig"))
    except json.JSONDecodeError as exc:
        fail(f"gemini-extension.json 不是合法 JSON：{exc}")
        return {}

    for key in REQUIRED_MANIFEST_FIELDS:
        value = manifest.get(key)
        empty = value is None or (isinstance(value, (str, list, dict)) and len(value) == 0)
        if empty or (isinstance(value, str) and not value.strip()):
            fail(f"gemini-extension.json 缺少或为空的字段：{key}")

    version = manifest.get("version", "")
    if isinstance(version, str) and version and not SEMVER_RE.match(version):
        fail(f"gemini-extension.json version 不是 semver 三段式：{version}")

    if manifest.get("name") != ext_root.name:
        warn(f"manifest name（{manifest.get('name')}）与目录名（{ext_root.name}）不一致")

    if not (ext_root / "CODELY.md").is_file():
        warn("缺少 CODELY.md（扩展上下文说明），不影响加载")

    return manifest


def check_facade_docs(ext_root: Path, manifest: dict) -> None:
    readme = ext_root / "README.md"
    if not readme.is_file():
        fail("缺少 README.md（人类可读门面：概览 / 技能一览 / 安装 / 用法 / 维护）")
    else:
        text = readme.read_text(encoding="utf-8")
        for section in README_SECTIONS:
            if section not in text:
                fail(f"README.md 缺少章节：{section}")

    changelog = ext_root / "CHANGELOG.md"
    if not changelog.is_file():
        fail("缺少 CHANGELOG.md（版本变更记录）")
    else:
        text = changelog.read_text(encoding="utf-8")
        found = re.search(r"^## (\d+\.\d+\.\d+)", text, re.M)
        version = manifest.get("version")
        if not found:
            fail("CHANGELOG.md 找不到 `## <版本号>` 形式的版本条目")
        elif version and found.group(1) != version:
            fail(f"CHANGELOG.md 顶部版本（{found.group(1)}）与清单 version（{version}）不一致")


def check_skill(skill_dir: Path) -> int:
    """返回该技能 description 的字符数（供总和校验）。"""
    skill_md = skill_dir / "SKILL.md"
    if not skill_md.is_file():
        fail(f"{skill_dir.name}: 缺少 SKILL.md")
        return 0

    text = skill_md.read_text(encoding="utf-8")
    meta = parse_frontmatter(text)
    if not meta:
        fail(f"{skill_dir.name}: SKILL.md 缺少 frontmatter")
        return 0

    name = meta.get("name", "")
    if not name:
        fail(f"{skill_dir.name}: frontmatter 缺少 name")
    elif name != skill_dir.name:
        fail(f"{skill_dir.name}: frontmatter name（{name}）与目录名不一致")
    elif not KEBAB_RE.match(name):
        fail(f"{skill_dir.name}: name 不是 kebab-case")

    description = meta.get("description", "")
    if not description:
        fail(f"{skill_dir.name}: frontmatter 缺少 description")
    elif "\n" in description:
        fail(f"{skill_dir.name}: description 必须单行")
    elif len(description) < 30:
        warn(f"{skill_dir.name}: description 偏短（{len(description)} 字符），可能触发不到")
    if len(description) > MAX_DESC_CHARS:
        fail(f"{skill_dir.name}: description 超篇幅上限（{len(description)} > {MAX_DESC_CHARS} 字符）"
             "——删能力罗列，保留 API 名与用户原话触发词")

    head = FRONTMATTER_RE.match(text)
    body_chars = len(text) - len(head.group(0)) if head else len(text)
    if body_chars > MAX_BODY_CHARS:
        fail(f"{skill_dir.name}: SKILL.md 正文超篇幅上限（{body_chars} > {MAX_BODY_CHARS} 字符）"
             "——把展开与完整代码下沉到 references/，正文留决策与结论")

    if "\r\n" in text:
        warn(f"{skill_dir.name}: SKILL.md 含 CRLF 换行")

    return len(description)


def check_links(ext_root: Path) -> None:
    for md in sorted(ext_root.rglob("*.md")):
        if ".git" in md.parts:
            continue
        text = md.read_text(encoding="utf-8")
        for target in MD_LINK_RE.findall(text):
            if target.startswith(("http://", "https://", "mailto:", "#")):
                continue
            clean = target.split("#", 1)[0].split("?", 1)[0]
            if not clean or not clean.endswith(".md"):
                continue
            resolved = (md.parent / clean).resolve()
            if not resolved.is_file():
                rel = md.relative_to(ext_root)
                fail(f"{rel}: 链接指向不存在的文件 → {target}")


def check_junk(ext_root: Path) -> None:
    for meta in sorted((ext_root / "skills").rglob("*.meta")):
        fail(f"skills/ 内混入 Unity .meta：{meta.relative_to(ext_root)}")
    for junk in sorted(ext_root.rglob("*")):
        if junk.is_file() and junk.name in JUNK_NAMES:
            fail(f"扩展内混入系统垃圾文件：{junk.relative_to(ext_root)}（删除请走废纸篓，保证可回退）")


def check_hooks(ext_root: Path) -> None:
    config = ext_root / "hooks" / "hooks.json"
    if not config.is_file():
        return  # hook 是可选组件
    try:
        data = json.loads(config.read_text(encoding="utf-8"))
    except json.JSONDecodeError as exc:
        fail(f"hooks/hooks.json 不是合法 JSON：{exc}")
        return

    hooks = data.get("hooks")
    if not isinstance(hooks, dict) or not hooks:
        fail("hooks/hooks.json 缺少非空的顶层 hooks 对象")
        return

    for event, groups in hooks.items():
        if event not in HOOK_EVENTS:
            fail(f"hooks/hooks.json 事件名不在受支持集合内：{event}")
        if not isinstance(groups, list):
            fail(f"hooks/hooks.json 事件 {event} 的值必须是数组")
            continue
        for group in groups:
            for hook in group.get("hooks", []) if isinstance(group, dict) else []:
                command = hook.get("command", "")
                if hook.get("type") != "command" or not command:
                    fail(f"hooks/hooks.json 事件 {event} 存在非 command 类型或空命令的条目")
                    continue
                if ABS_PATH_RE.search(command):
                    fail(f"hooks/hooks.json 事件 {event} 的命令含机器绝对路径（应改用 ${{extensionPath}}）：{command}")
                for rel in EXT_PATH_RE.findall(command):
                    script = (ext_root / rel.lstrip("/")).resolve()
                    if not script.is_file():
                        fail(f"hooks/hooks.json 事件 {event} 引用的脚本不存在：{rel}")


def find_repo_root(start: Path) -> Path | None:
    for parent in [start, *start.parents]:
        if (parent / "Assets" / "Runestone").is_dir():
            return parent
    return None


def check_package_skills_removed(ext_root: Path) -> None:
    """包内不得再留技能副本，否则会出现两份互相漂移的正文。"""
    repo_root = find_repo_root(ext_root)
    if repo_root is None:
        return
    for leftover in sorted((repo_root / "Assets" / "Runestone").glob("*/TuanjieCowork~/Skills")):
        if leftover.is_dir():
            fail(f"包内仍存在技能目录：{leftover.relative_to(repo_root)}（应只保留 TuanjieCowork~/README.md 导流文档）")


def main() -> int:
    quiet = "--quiet" in sys.argv
    ext_root = Path(__file__).resolve().parent.parent

    manifest = check_manifest(ext_root)
    check_facade_docs(ext_root, manifest)

    skills_dir = ext_root / "skills"
    if not skills_dir.is_dir():
        fail("缺少 skills/ 目录")
        skills: list[Path] = []
    else:
        skills = sorted(p for p in skills_dir.iterdir() if p.is_dir())
        if not skills:
            fail("skills/ 下没有任何技能目录")

    desc_total = sum(check_skill(skill_dir) for skill_dir in skills)
    if desc_total > MAX_DESC_TOTAL:
        fail(f"全部 description 总和超上限（{desc_total} > {MAX_DESC_TOTAL} 字符）"
             "——这是每次会话都付的常驻上下文成本")

    check_links(ext_root)
    check_junk(ext_root)
    check_hooks(ext_root)
    check_package_skills_removed(ext_root)

    if not quiet or errors or warnings:
        refs = sum(1 for _ in skills_dir.rglob("references/*.md")) if skills_dir.is_dir() else 0
        print(f"扩展根：{ext_root}")
        print(f"技能：{len(skills)} 个（{', '.join(p.name for p in skills) or '无'}）")
        print(f"references：{refs} 篇")
        print(f"description 总和：{desc_total} / {MAX_DESC_TOTAL} 字符")
        print(f"hook：{'已配置' if (ext_root / 'hooks' / 'hooks.json').is_file() else '未配置'}")

    for message in warnings:
        print(f"[警告] {message}")
    for message in errors:
        print(f"[错误] {message}")

    if errors:
        print(f"\n校验失败：{len(errors)} 个错误，{len(warnings)} 个警告。")
        return 1

    print(f"\n校验通过：0 个错误，{len(warnings)} 个警告。")
    return 0


if __name__ == "__main__":
    sys.exit(main())