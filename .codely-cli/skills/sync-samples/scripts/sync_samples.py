#!/usr/bin/env python3
"""Sync Unity package samples from Assets/Samples/ to Samples~ folders."""

import argparse
import json
import os
import shutil
import sys
from pathlib import Path


def find_packages(project_root):
    """Find all packages with a samples array in package.json."""
    packages = []
    runestone_dir = Path(project_root) / "Assets" / "Runestone"
    if not runestone_dir.is_dir():
        return packages
    for pkg_dir in sorted(runestone_dir.iterdir()):
        if not pkg_dir.is_dir():
            continue
        pkg_json_path = pkg_dir / "package.json"
        if not pkg_json_path.is_file():
            continue
        with open(pkg_json_path, encoding="utf-8") as f:
            data = json.load(f)
        samples = data.get("samples")
        if not samples:
            continue
        pkg_name = data.get("name", pkg_dir.name)
        for s in samples:
            display_name = s["displayName"]
            samples_path = s["path"]
            samples_folder = samples_path.split("/")[-1]
            packages.append({
                "package_name": pkg_name,
                "package_dir": pkg_dir,
                "display_name": display_name,
                "samples_folder": samples_folder,
            })
    return packages


def find_imported_sample_dir(project_root, package_name, display_name):
    """Find the imported sample folder in Assets/Samples/<Package>/<Version>/<displayName>/."""
    samples_base = Path(project_root) / "Assets" / "Samples" / package_name
    if not samples_base.is_dir():
        return None
    version_dirs = [d for d in samples_base.iterdir() if d.is_dir()]
    if not version_dirs:
        return None
    latest_version = sorted(version_dirs)[-1]
    target = latest_version / display_name
    return target if target.is_dir() else None


def sync_sample(src, dst, dry_run=False):
    """Sync content from src to dst, replacing existing files."""
    if dry_run:
        changed = []
        for root, _, files in os.walk(src):
            for f in files:
                rel = Path(root, f).relative_to(src)
                changed.append(str(rel))
        return changed
    if dst.is_dir():
        shutil.rmtree(dst)
    shutil.copytree(src, dst)
    changed = []
    for root, _, files in os.walk(dst):
        for f in files:
            rel = Path(root, f).relative_to(dst)
            changed.append(str(rel))
    return changed


def main():
    parser = argparse.ArgumentParser(description="Sync Unity package Samples from Assets/Samples to Samples~")
    parser.add_argument("--project-root", default=".", help="Unity project root (default: current directory)")
    parser.add_argument("--dry-run", action="store_true", help="Show what would be synced without modifying files")
    args = parser.parse_args()

    project_root = os.path.abspath(args.project_root)
    packages = find_packages(project_root)

    if not packages:
        print("No packages with samples found.")
        return 0

    total_synced = 0
    for pkg in packages:
        src = find_imported_sample_dir(project_root, pkg["package_name"], pkg["display_name"])
        if src is None:
            continue
        dst = pkg["package_dir"] / "Samples~" / pkg["samples_folder"]
        changed = sync_sample(src, dst, args.dry_run)
        if changed:
            total_synced += len(changed)
            mode = "[DRY RUN] " if args.dry_run else ""
            print(f"{mode}Synced: {pkg['package_name']} / {pkg['display_name']} -> {dst.relative_to(project_root)}")
            print(f"         {len(changed)} files")

    if total_synced == 0:
        print("Everything is already in sync.")
    else:
        mode = "[DRY RUN] " if args.dry_run else ""
        print(f"\n{mode}Total: {total_synced} files synced.")

    return 0


if __name__ == "__main__":
    sys.exit(main())
