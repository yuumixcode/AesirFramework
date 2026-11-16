#!/usr/bin/env python3
"""把扩展内的技能批量安装到 Codely CLI 的技能目录。

等价于对每个技能目录执行一次 `codely skills install <dir>`，但一次装完、可 dry-run、可只装指定技能。

用法:
    python3 install.py                      # 装到本项目 .codely-cli/skills/
    python3 install.py --scope global       # 装到 ~/.codely-cli/skills/
    python3 install.py --check              # 只列出将安装/会覆盖什么，不写盘
    python3 install.py --only aesir-router  # 只装指定技能（逗号分隔）
    python3 install.py --force              # 覆盖已存在的同名技能目录

退出码: 0 成功；1 参数错误或存在未加 --force 的冲突。
"""

from __future__ import annotations

import argparse
import shutil
import sys
from pathlib import Path


def find_repo_root(start: Path) -> Path | None:
    for parent in [start, *start.parents]:
        if (parent / "Assets" / "Runestone").is_dir():
            return parent
    return None


def main() -> int:
    parser = argparse.ArgumentParser(description="批量安装 AesirFrameworkGame 扩展内的技能")
    parser.add_argument("--scope", choices=["project", "global"], default="project", help="安装范围（默认 project）")
    parser.add_argument("--check", action="store_true", help="只列出将要做的事，不写盘")
    parser.add_argument("--force", action="store_true", help="覆盖已存在的同名技能目录")
    parser.add_argument("--only", help="只安装指定技能，逗号分隔（如 aesir-router,aesir-ui）")
    args = parser.parse_args()

    extension_root = Path(__file__).resolve().parent.parent
    skills_dir = extension_root / "skills"
    if not skills_dir.is_dir():
        print(f"[错误] 找不到扩展技能目录：{skills_dir}")
        return 1

    available = sorted(p.name for p in skills_dir.iterdir() if p.is_dir() and (p / "SKILL.md").is_file())
    if not available:
        print("[错误] 扩展内没有任何技能（缺少 skills/<name>/SKILL.md）")
        return 1

    if args.only:
        wanted = [name.strip() for name in args.only.split(",") if name.strip()]
        unknown = [name for name in wanted if name not in available]
        if unknown:
            print(f"[错误] 未知技能：{', '.join(unknown)}")
            print(f"可用技能：{', '.join(available)}")
            return 1
        selected = [name for name in available if name in wanted]
    else:
        selected = available

    if args.scope == "global":
        target_root = Path.home() / ".codely-cli" / "skills"
        scope_note = "全局（所有项目共享）"
    else:
        repo_root = find_repo_root(extension_root)
        base = repo_root if repo_root else Path.cwd()
        target_root = base / ".codely-cli" / "skills"
        scope_note = f"本项目（{base}）"

    print(f"扩展：{extension_root}")
    print(f"范围：{scope_note}")
    print(f"目标：{target_root}")
    print()

    conflicts: list[str] = []
    to_install: list[str] = []
    for name in selected:
        target = target_root / name
        if target.exists():
            if args.force:
                print(f"  覆盖  {name}")
                to_install.append(name)
            else:
                print(f"  冲突  {name}（已存在，--force 可覆盖）")
                conflicts.append(name)
        else:
            print(f"  新增  {name}")
            to_install.append(name)

    if args.check:
        print(f"\n[dry-run] 将安装 {len(to_install)} 个，冲突 {len(conflicts)} 个。")
        return 1 if conflicts else 0

    if conflicts:
        print(f"\n[中止] {len(conflicts)} 个技能已存在且未加 --force，未做任何写入。")
        return 1

    target_root.mkdir(parents=True, exist_ok=True)
    for name in to_install:
        target = target_root / name
        if target.exists():
            shutil.rmtree(target)
        shutil.copytree(
            skills_dir / name,
            target,
            ignore=shutil.ignore_patterns("*.meta", "__pycache__"),
        )

    print(f"\n安装完成：{len(to_install)} 个技能 → {target_root}")
    print("验证：codely skills list")
    return 0


if __name__ == "__main__":
    sys.exit(main())
