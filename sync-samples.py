#!/usr/bin/env python3
"""sync-samples.py — 同步 Unity 包的样例在源码（Samples~/...）和开发副本
（Assets/Samples/<包显示名>/<版本>/<样例显示名>/）之间。

每个包通过 `package.json` 的 `samples[]` 声明：
  - `displayName`: Package Manager 显示名，也是开发副本的文件夹名
  - `path`:        源码路径，相对包根（如 "Samples~/Counter-MVC"）
                   末段既是源码文件夹名，也是源码 .meta 的同伴文件名

当 `path` 末段与 `displayName` 不一致时（如 AesirArchitecture 的
"ObservableValue" vs "ObservableValue (Dependency Odin Inspector)"），
脚本在同步时重命名 .meta。

用法：
    python3 sync-samples.py to-source    # dev -> source
    python3 sync-samples.py to-dev       # source -> dev
    python3 sync-samples.py status       # 列出所有样例的当前状态
    python3 sync-samples.py -n ...       # 仅预览，不写盘
"""

import argparse
import json
import shutil
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent
RUNESTONE = ROOT / "Assets" / "Runestone"
SAMPLES_DEV = ROOT / "Assets" / "Samples"


def discover_packages() -> list[Path]:
    """返回 Assets/Runestone/ 下所有包含 package.json 的包目录。"""
    if not RUNESTONE.exists():
        return []
    return sorted(
        d for d in RUNESTONE.iterdir()
        if d.is_dir() and (d / "package.json").exists()
    )


def load_package(pkg_dir: Path) -> dict | None:
    pkg_json = pkg_dir / "package.json"
    if not pkg_json.exists():
        return None
    return json.loads(pkg_json.read_text(encoding="utf-8"))


def resolve_paths(pkg: dict, pkg_dir: Path, sample: dict) -> tuple[Path, Path]:
    """返回 (source_folder, dev_folder)。"""
    display_name = sample["displayName"]
    src_rel = sample["path"]                              # "Samples~/Counter-MVC"
    src_folder = pkg_dir / src_rel
    pkg_display = pkg.get("displayName") or pkg_dir.name
    pkg_version = pkg.get("version") or "0.0.0"
    dev_folder = SAMPLES_DEV / pkg_display / pkg_version / display_name
    return src_folder, dev_folder


def copy_folder_with_meta(src: Path, dst: Path) -> str:
    """镜像复制 src -> dst（含 .meta 重命名）。返回人类可读的结果描述。"""
    src_meta = src.parent / (src.name + ".meta")
    dst_meta = dst.parent / (dst.name + ".meta")
    if dst.exists():
        shutil.rmtree(dst)
    shutil.copytree(src, dst)
    if dst_meta.exists():
        dst_meta.unlink()
    if src_meta.exists():
        shutil.copy2(src_meta, dst_meta)
    note = ""
    if src.name != dst.name:
        note = f"  (meta 重命名: {src_meta.name} -> {dst_meta.name})"
    return f"  ✓ {src.relative_to(ROOT)} -> {dst.relative_to(ROOT)}{note}"


def run(direction: str, dry_run: bool) -> int:
    failures = 0
    for pkg_dir in discover_packages():
        pkg = load_package(pkg_dir)
        if pkg is None:
            continue
        samples = pkg.get("samples") or []
        if not samples:
            continue
        pkg_display = pkg.get("displayName") or pkg_dir.name
        pkg_version = pkg.get("version") or "0.0.0"
        print(f"\n[{pkg_dir.name}] {pkg_display} v{pkg_version}")
        for sample in samples:
            display = sample["displayName"]
            src, dev = resolve_paths(pkg, pkg_dir, sample)
            src_meta = src.parent / (src.name + ".meta")
            dev_meta = dev.parent / (dev.name + ".meta")
            print(f"  · {display}")
            if direction == "status":
                src_ok = "✓" if src.exists() else "✗"
                dev_ok = "✓" if dev.exists() else "✗"
                src_meta_ok = "✓" if src_meta.exists() else "✗"
                dev_meta_ok = "✓" if dev_meta.exists() else "✗"
                print(f"    src  [{src_ok} folder / {src_meta_ok} meta] {src.relative_to(ROOT)}")
                print(f"    dev  [{dev_ok} folder / {dev_meta_ok} meta] {dev.relative_to(ROOT)}")
                continue
            if direction == "to-source":
                a, b = dev, src
            else:                                                # to-dev
                a, b = src, dev
            if not a.exists():
                print(f"  ✗ source 不存在: {a.relative_to(ROOT)}")
                failures += 1
                continue
            if not b.parent.exists():
                print(f"  ✗ destination 父目录不存在: {b.parent.relative_to(ROOT)}")
                failures += 1
                continue
            if dry_run:
                src_n, dst_n = (a.name, b.name)
                rename_note = ""
                if src_n != dst_n:
                    rename_note = f"  (meta 重命名: {src_n}.meta -> {dst_n}.meta)"
                print(f"  → 将复制 {a.relative_to(ROOT)} -> {b.relative_to(ROOT)}{rename_note}")
            else:
                print(copy_folder_with_meta(a, b))
    return 0 if failures == 0 else 2


def main() -> int:
    parser = argparse.ArgumentParser(
        description="同步 Unity 包的样例（源码 <-> Assets/Samples/）。",
    )
    parser.add_argument(
        "direction",
        choices=["to-source", "to-dev", "status"],
        help="to-source: 开发副本 -> 源码；to-dev: 源码 -> 开发副本；status: 列出当前状态。",
    )
    parser.add_argument(
        "-n", "--dry-run",
        action="store_true",
        help="只显示将要执行的操作，不实际写盘。",
    )
    args = parser.parse_args()
    return run(args.direction, args.dry_run)


if __name__ == "__main__":
    sys.exit(main())
