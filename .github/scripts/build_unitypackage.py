#!/usr/bin/env python3
"""Build a Unity .unitypackage from one or more package directories — no Unity required.

Output format matches Unity's ExportPackage (byte-verified against a Unity 2022.3
reference export of this repo):
  <guid>/asset        raw asset bytes (absent for folder entries)
  <guid>/asset.meta   the asset's .meta content, byte-identical to the repo file
  <guid>/pathname     project-relative path, no trailing newline

Only entries that have a .meta file are included; the package root folder itself is
NOT included (same as Unity's FindAssets-based export). Unity's own ExportPackage
with IncludeDependencies additionally drags in engine built-in package sources
(ugui/URP scripts referenced by sample scenes) — deliberately NOT emulated here:
release packages stay self-contained and never copy engine sources.
Passing --package multiple times produces one combined package (used for the RAF
bundle of RAA + RAM).
"""

import argparse
import json
import os
import re
import tarfile
import tempfile


def read_guid(meta_path):
    """Extract the guid from a Unity .meta file (first 'guid:' line)."""
    with open(meta_path, "r", encoding="utf-8") as f:
        for line in f:
            if line.startswith("guid:"):
                return line[len("guid:"):].strip()
    return None


def _is_ignored_name(name):
    """Mirror Unity's AssetDatabase import rules: dot-prefixed entries and
    trailing-'~' hidden folders (Samples~, Documentation~) are never imported,
    so Unity's ExportPackage never contains them. Emulate that here — mirrored
    Samples~ copies even share GUIDs with their Samples/ originals."""
    return name.startswith(".") or name.endswith("~")


def collect_entries(package_dirs):
    """Yield (guid, asset_path_or_None, meta_path, pathname) for every meta-backed entry."""
    seen_guids = set()
    for package_dir in package_dirs:
        package_dir = os.path.normpath(package_dir)
        if not os.path.isdir(package_dir):
            raise SystemExit(f"package dir not found: {package_dir}")
        for root, dirs, files in os.walk(package_dir):
            dirs[:] = [d for d in dirs if not _is_ignored_name(d)]
            for name in dirs:
                dir_path = os.path.join(root, name)
                meta_path = dir_path + ".meta"
                guid = read_guid(meta_path) if os.path.exists(meta_path) else None
                if guid is None:
                    continue
                _register(seen_guids, guid, dir_path)
                yield guid, None, meta_path, dir_path.replace(os.sep, "/")
            for name in files:
                if name.endswith(".meta") or _is_ignored_name(name):
                    continue
                file_path = os.path.join(root, name)
                meta_path = file_path + ".meta"
                guid = read_guid(meta_path) if os.path.exists(meta_path) else None
                if guid is None:
                    continue
                _register(seen_guids, guid, file_path)
                yield guid, file_path, meta_path, file_path.replace(os.sep, "/")


def _register(seen_guids, guid, path):
    if guid in seen_guids:
        raise SystemExit(f"duplicate guid {guid} at {path} — aborted (corrupt output)")
    seen_guids.add(guid)


def read_package_version(package_dir):
    """Read the 'version' field from the package's package.json (dependency-free)."""
    pkg_json = os.path.join(package_dir, "package.json")
    if not os.path.isfile(pkg_json):
        raise SystemExit(f"package.json not found in {package_dir}")
    with open(pkg_json, "r", encoding="utf-8") as f:
        content = f.read()
    match = re.search(r'"version"\s*:\s*"([^"]+)"', content)
    if match is None:
        raise SystemExit(f"'version' field not found in {pkg_json}")
    return match.group(1)


def write_manifest(package_dirs, entries, manifest_path):
    """Write the files manifest JSON consumed by the in-editor Aesir updater.

    Format (array-based so Unity's JsonUtility can deserialize it directly):
      {"packages": [{"name": ..., "version": ..., "files": [...]}]}
    File lists are sorted for deterministic output."""
    normalized_dirs = [os.path.normpath(d).replace(os.sep, "/") for d in package_dirs]
    packages = {
        os.path.basename(norm): {"name": os.path.basename(norm),
                                 "version": read_package_version(norm),
                                 "files": []}
        for norm in normalized_dirs
    }
    for _, _, _, pathname in entries:
        # Longest-prefix match: combined builds span several sibling package dirs.
        candidates = [norm for norm in normalized_dirs
                      if pathname == norm or pathname.startswith(norm + "/")]
        if not candidates:
            continue
        best = max(candidates, key=len)
        packages[os.path.basename(best)]["files"].append(pathname)
    for entry in packages.values():
        entry["files"].sort()
    data = {"packages": [packages[name] for name in sorted(packages)]}

    output_dir = os.path.dirname(os.path.abspath(manifest_path))
    os.makedirs(output_dir, exist_ok=True)
    with open(manifest_path, "w", encoding="utf-8", newline="\n") as f:
        json.dump(data, f, ensure_ascii=False, indent=2)
        f.write("\n")


def build(package_dirs, output_path, manifest_path=None):
    entries = list(collect_entries(package_dirs))
    output_dir = os.path.dirname(os.path.abspath(output_path))
    os.makedirs(output_dir, exist_ok=True)
    with tempfile.TemporaryDirectory() as tmp, tarfile.open(output_path, "w:gz") as tar:
        for guid, asset_path, meta_path, pathname in entries:
            entry_dir = os.path.join(tmp, guid)
            os.mkdir(entry_dir)
            with open(os.path.join(entry_dir, "pathname"), "w",
                      encoding="utf-8", newline="") as f:
                f.write(pathname)
            with open(meta_path, "rb") as src, \
                    open(os.path.join(entry_dir, "asset.meta"), "wb") as dst:
                dst.write(src.read())
            if asset_path is not None:
                with open(asset_path, "rb") as src, \
                        open(os.path.join(entry_dir, "asset"), "wb") as dst:
                    dst.write(src.read())
            tar.add(entry_dir, arcname=guid)
    if manifest_path:
        write_manifest(package_dirs, entries, manifest_path)


def main():
    parser = argparse.ArgumentParser(
        description="Build a Unity .unitypackage without Unity (CI-friendly).")
    parser.add_argument("--package", action="append", required=True,
                        help="package dir relative to repo root; repeatable, "
                             "multiple dirs are combined into one package")
    parser.add_argument("--output", required=True, help="output .unitypackage path")
    parser.add_argument("--manifest", default=None,
                        help="optional output path for the files-manifest.json "
                             "(grouped by package dir; used by the Aesir updater)")
    args = parser.parse_args()
    build(args.package, args.output, args.manifest)


if __name__ == "__main__":
    main()
