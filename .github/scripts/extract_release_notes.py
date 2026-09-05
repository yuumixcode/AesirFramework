#!/usr/bin/env python3
"""Extract one version's section from the root CHANGELOG.md for GitHub Release notes.

The changelog uses Keep-a-Changelog style headings ("## [0.14.0] - 2026-09-05"); the
section runs until the next "## " heading (### subsections are kept inside it).
Exits non-zero if the version is not found, so CI fails loudly on drift.
"""

import argparse


def extract(changelog_path, version):
    with open(changelog_path, "r", encoding="utf-8") as f:
        lines = f.read().splitlines()

    start = None
    prefix = f"## [{version}]"
    for i, line in enumerate(lines):
        if line.startswith(prefix):
            start = i
            break
    if start is None:
        raise SystemExit(f"version [{version}] not found in {changelog_path}")

    end = len(lines)
    for j in range(start + 1, len(lines)):
        # "### " does not match "## ", so subsections stay inside the section
        if lines[j].startswith("## "):
            end = j
            break

    section = lines[start:end]
    while section and not section[-1].strip():
        section.pop()
    return "\n".join(section)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--changelog", required=True, help="path to CHANGELOG.md")
    parser.add_argument("--version", required=True, help="version to extract, e.g. 0.14.0")
    args = parser.parse_args()
    print(extract(args.changelog, args.version))


if __name__ == "__main__":
    main()
