#!/usr/bin/env python3
"""校验 AesirFrameworkGame 扩展的结构与内容一致性。

检查项:
  1. gemini-extension.json 存在、UTF-8 无 BOM、可解析、含 name/version/description
  2. 每个技能目录都有 SKILL.md，frontmatter 含 name 与 description
  3. frontmatter 的 name 与目录名一致，且为 kebab-case
  4. Markdown 内的相对 *.md 链接都能解析到真实文件
  5. skills/ 下没有混入 Unity 的 *.meta
  6. 包内不再出现 TuanjieCowork~/Skills（技能唯一主位是本扩展；仅在能定位仓库根时检查）

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

KEBAB_RE = re.compile(r"^[a-z0-9]+(-[a-z0-9]+)*$")
MD_LINK_RE = re.compile(r"\[[^\]]*\]\(([^)\s]+)\)")

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


def check_manifest(ext_root: Path) -> None:
    manifest_path = ext_root / "gemini-extension.json"
    if not manifest_path.is_file():
        fail("缺少 gemini-extension.json")
        return

    raw = manifest_path.read_bytes()
    if raw.startswith(b"\xef\xbb\xbf"):
        fail("gemini-extension.json 含 BOM（应为 UTF-8 无 BOM）")

    try:
        manifest = json.loads(raw.decode("utf-8-sig"))
    except json.JSONDecodeError as exc:
        fail(f"gemini-extension.json 不是合法 JSON：{exc}")
        return

    for key in ("name", "version", "description"):
        value = manifest.get(key)
        if not isinstance(value, str) or not value.strip():
            fail(f"gemini-extension.json 缺少或为空的字段：{key}")

    if manifest.get("name") != ext_root.name:
        warn(f"manifest name（{manifest.get('name')}）与目录名（{ext_root.name}）不一致")

    if not (ext_root / "CODELY.md").is_file():
        warn("缺少 CODELY.md（扩展上下文说明），不影响加载")


def check_skill(skill_dir: Path) -> None:
    skill_md = skill_dir / "SKILL.md"
    if not skill_md.is_file():
        fail(f"{skill_dir.name}: 缺少 SKILL.md")
        return

    text = skill_md.read_text(encoding="utf-8")
    meta = parse_frontmatter(text)
    if not meta:
        fail(f"{skill_dir.name}: SKILL.md 缺少 frontmatter")
        return

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

    if "\r\n" in text:
        warn(f"{skill_dir.name}: SKILL.md 含 CRLF 换行")


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


def check_meta(ext_root: Path) -> None:
    metas = [p for p in (ext_root / "skills").rglob("*.meta")]
    for meta in metas:
        fail(f"skills/ 内混入 Unity .meta：{meta.relative_to(ext_root)}")


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

    check_manifest(ext_root)

    skills_dir = ext_root / "skills"
    if not skills_dir.is_dir():
        fail("缺少 skills/ 目录")
        skills: list[Path] = []
    else:
        skills = sorted(p for p in skills_dir.iterdir() if p.is_dir())
        if not skills:
            fail("skills/ 下没有任何技能目录")

    for skill_dir in skills:
        check_skill(skill_dir)

    check_links(ext_root)
    check_meta(ext_root)
    check_package_skills_removed(ext_root)

    if not quiet or errors or warnings:
        print(f"扩展根：{ext_root}")
        print(f"技能：{len(skills)} 个（{', '.join(p.name for p in skills) or '无'}）")
        refs = sum(1 for _ in (skills_dir / "aesir-framework" / "references").glob("*.md")) \
            if (skills_dir / "aesir-framework" / "references").is_dir() else 0
        if refs:
            print(f"aesir-framework references：{refs} 篇")

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
