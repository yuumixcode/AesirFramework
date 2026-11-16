#!/usr/bin/env python3
"""同步包内示例：主位 Samples/ → 发布镜像 Samples~/。

用法：
    python3 scripts/sync_samples.py --check       # 只报告差异（先跑这个判断方向）
    python3 scripts/sync_samples.py --dry-run     # 预览将执行的动作
    python3 scripts/sync_samples.py               # 执行同步并校验

约定、方向判断要点与历史事故见同目录 SKILL.md。
"""

from __future__ import annotations

import argparse
import difflib
import filecmp
import json
import os
import shutil
import sys
from pathlib import Path

# 不应进入镜像的垃圾文件
JUNK = {".DS_Store", "Thumbs.db", "desktop.ini"}


def _is_junk(name: str) -> bool:
    return name in JUNK or name.startswith(".")


def find_packages(project_root: Path):
    """返回同时含 Samples/ 与 Samples~/ 的包目录。"""
    root = project_root / "Assets" / "Runestone"
    if not root.is_dir():
        return []
    return [
        d
        for d in sorted(root.iterdir())
        if d.is_dir() and (d / "Samples").is_dir() and (d / "Samples~").is_dir()
    ]


def sample_names(pkg_dir: Path):
    return sorted(d.name for d in (pkg_dir / "Samples").iterdir() if d.is_dir() and not _is_junk(d.name))


def rel_files(root: Path):
    """root 下全部文件的相对路径（跳过 junk 与点开头目录）。"""
    if not root.is_dir():
        return set()
    found = set()
    for dirpath, dirnames, filenames in os.walk(root):
        dirnames[:] = [d for d in dirnames if not _is_junk(d)]
        for name in filenames:
            if not _is_junk(name):
                found.add(os.path.relpath(os.path.join(dirpath, name), root))
    return found


def diff_summary(src: Path, dst: Path):
    """返回 (仅主位有, 仅镜像有, 内容不同)，三者均为已排序的相对路径列表。"""
    main_files, mirror_files = rel_files(src), rel_files(dst)
    changed = [
        rel for rel in sorted(main_files & mirror_files) if not filecmp.cmp(src / rel, dst / rel, shallow=False)
    ]
    return sorted(main_files - mirror_files), sorted(mirror_files - main_files), changed


def line_delta(src: Path, dst: Path):
    """粗略行级差异量 (+主位独有行, -镜像独有行)，用于判断哪一侧更新。"""
    try:
        main_lines = src.read_text(encoding="utf-8", errors="replace").splitlines()
        mirror_lines = dst.read_text(encoding="utf-8", errors="replace").splitlines()
    except OSError:
        return 0, 0
    plus = minus = 0
    for line in difflib.unified_diff(mirror_lines, main_lines, n=0, lineterm=""):
        if line.startswith("+") and not line.startswith("+++"):
            plus += 1
        elif line.startswith("-") and not line.startswith("---"):
            minus += 1
    return plus, minus


def sync_dir(src: Path, dst: Path, dry_run: bool):
    """把 src 内容同步到 dst：覆盖同名、删除镜像多余项、清理空目录。"""
    only_main, only_mirror, changed = diff_summary(src, dst)
    copies = only_main + changed
    actions = [("copy", rel) for rel in copies] + [("delete", rel) for rel in only_mirror]
    if dry_run:
        return actions

    dst.mkdir(parents=True, exist_ok=True)
    for rel in copies:
        target = dst / rel
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(src / rel, target)
    for rel in only_mirror:
        (dst / rel).unlink()
    for dirpath, dirnames, filenames in os.walk(dst, topdown=False):
        if Path(dirpath) != dst and not dirnames and not filenames:
            os.rmdir(dirpath)
    return actions


def sync_dir_metas(pkg_dir: Path, dry_run: bool):
    """补齐/清理顶层目录级 .meta（最容易漏的一环）。"""
    actions = []
    main_metas = {m.name for m in (pkg_dir / "Samples").glob("*.meta")}
    mirror_root = pkg_dir / "Samples~"
    for meta in sorted((pkg_dir / "Samples").glob("*.meta")):
        target = mirror_root / meta.name
        if not target.is_file() or not filecmp.cmp(meta, target, shallow=False):
            actions.append(("copy", meta.name))
            if not dry_run:
                shutil.copy2(meta, target)
    for meta in sorted(mirror_root.glob("*.meta")):
        if meta.name not in main_metas:
            actions.append(("delete", meta.name))
            if not dry_run:
                meta.unlink()
    return actions


def check_package_json(pkg_dir: Path):
    """校验 package.json 的 samples 清单与两处目录是否一一对应。"""
    pkg_json = pkg_dir / "package.json"
    if not pkg_json.is_file():
        return ["缺少 package.json"]
    try:
        data = json.loads(pkg_json.read_text(encoding="utf-8"))
    except json.JSONDecodeError as exc:
        return [f"package.json 解析失败：{exc}"]

    declared = []
    problems = []
    for entry in data.get("samples", []):
        path = entry.get("path", "")
        declared.append(path)
        if not path.startswith("Samples~/"):
            problems.append(f"samples 条目的 path 应以 'Samples~/' 开头：{path!r}")
            continue
        if not (pkg_dir / path.replace("Samples~/", "Samples/", 1)).is_dir():
            problems.append(f"samples 条目缺主位目录：{path}")
        if not (pkg_dir / path).is_dir():
            problems.append(f"samples 条目缺镜像目录：{path}")

    mirror_root = pkg_dir / "Samples~"
    for dirpath, dirnames, filenames in os.walk(mirror_root):
        dirnames[:] = [d for d in dirnames if not _is_junk(d)]
        if any(name.endswith(".asmdef") for name in filenames):
            rel = os.path.relpath(dirpath, pkg_dir)
            if not any(rel == d or rel.startswith(d + os.sep) for d in declared):
                problems.append(f"镜像中存在未登记的程序集目录：{rel}")
    return problems


def main():
    parser = argparse.ArgumentParser(description="同步包内示例 Samples/ → Samples~/")
    parser.add_argument("--project-root", default=".", help="Unity 工程根目录（默认当前目录）")
    parser.add_argument("--package", action="append", default=[], help="只处理指定包目录名（可重复）")
    parser.add_argument("--check", action="store_true", help="只报告差异，不写文件")
    parser.add_argument("--dry-run", action="store_true", help="预览动作，不写文件")
    args = parser.parse_args()

    project_root = Path(args.project_root).resolve()
    packages = [p for p in find_packages(project_root) if not args.package or p.name in args.package]
    if not packages:
        print("未找到同时含 Samples/ 与 Samples~/ 的包（检查 --project-root）。")
        return 1

    write = not (args.check or args.dry_run)
    drift = 0
    for pkg_dir in packages:
        print(f"===== {pkg_dir.name}")

        for name in sample_names(pkg_dir):
            src, dst = pkg_dir / "Samples" / name, pkg_dir / "Samples~" / name
            only_main, only_mirror, changed = diff_summary(src, dst)
            if not (only_main or only_mirror or changed):
                print(f"  OK    {name}")
                continue

            drift += len(only_main) + len(only_mirror) + len(changed)
            print(
                f"  DIFF  {name}  仅主位 {len(only_main)} · 仅镜像 {len(only_mirror)} · 内容不同 {len(changed)}"
            )
            for rel in only_main:
                print(f"          + {rel}（主位独有）")
            for rel in only_mirror:
                print(f"          - {rel}（镜像独有，同步时删除）")
            for rel in changed:
                plus, minus = line_delta(src / rel, dst / rel)
                print(f"          ~ {rel}（+{plus} 主位行 / -{minus} 镜像行）")

            if not args.check:
                actions = sync_dir(src, dst, dry_run=args.dry_run)
                copied = sum(1 for kind, _ in actions if kind == "copy")
                deleted = sum(1 for kind, _ in actions if kind == "delete")
                print(f"        {'DRY  ' if args.dry_run else 'SYNC '} 覆盖 {copied} · 删除 {deleted}")

        meta_actions = sync_dir_metas(pkg_dir, dry_run=not write)
        if meta_actions:
            drift += len(meta_actions)
            for kind, name in meta_actions:
                print(f"  META  {kind} {name}")
        else:
            print("  OK    顶层目录 .meta")

        for problem in check_package_json(pkg_dir):
            print(f"  WARN  {problem}")

    print()
    if args.check:
        print("（--check 只读，未写任何文件）" + (f"  漂移 {drift} 项" if drift else "  无漂移"))
        return 0
    if args.dry_run:
        print(f"[DRY RUN] 待处理 {drift} 项，未写任何文件。")
        return 0
    if drift == 0:
        print("已一致，无需同步。")
        return 0

    residual = 0
    for pkg_dir in packages:
        for name in sample_names(pkg_dir):
            only_main, only_mirror, changed = diff_summary(
                pkg_dir / "Samples" / name, pkg_dir / "Samples~" / name
            )
            residual += len(only_main) + len(only_mirror) + len(changed)
    if residual:
        print(f"同步完成，但校验仍有 {residual} 项差异 ✗")
        return 2
    print(f"同步完成，处理 {drift} 项；校验：Samples 与 Samples~ 完全一致 ✓")
    return 0


if __name__ == "__main__":
    sys.exit(main())
