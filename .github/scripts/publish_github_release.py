#!/usr/bin/env python3
"""Create or refresh a GitHub Release with built unitypackage assets.

Idempotent rolling-release semantics: if a Release already exists for the tag
it is deleted first, then recreated and re-uploaded. The tag itself is NEVER
touched (tags are owned by the upstream pipeline — CNB rolls them forward).

Auth: reads GITHUB_TOKEN (or GIT_ACCESS_TOKEN) from the environment, needs
``contents: write``. Designed to run inside CNB's ``v*`` tag_push pipeline with
the same PAT git-sync uses, so GitHub Releases no longer depend on the
tag-push webhook that was observed to unreliably trigger GitHub Actions.

Usage:
    GITHUB_TOKEN=... python3 publish_github_release.py \
        --owner yuumixcode --repo AesirFramework \
        --tag v0.34.0 --notes release_notes.md \
        --assets dist/*.unitypackage
"""

import argparse
import json
import os
import sys
import urllib.error
import urllib.request

API_ROOT = "https://api.github.com"


def request(method, target, token, *, body=None, raw=None, content_type=None):
    """Return parsed JSON on success, or a ('HTTP_ERROR', code, text) tuple on failure."""
    url = target if target.startswith("http") else f"{API_ROOT}{target}"
    headers = {
        "Accept": "application/vnd.github+json",
        "Authorization": f"Bearer {token}",
        "User-Agent": "aesir-release-publisher",
        "X-GitHub-Api-Version": "2022-11-28",
    }
    data = None
    if raw is not None:
        data = raw
        headers["Content-Type"] = content_type or "application/octet-stream"
    elif body is not None:
        data = json.dumps(body).encode("utf-8")
        headers["Content-Type"] = "application/json"

    req = urllib.request.Request(url, data=data, headers=headers, method=method)
    try:
        with urllib.request.urlopen(req, timeout=120) as resp:
            payload = resp.read()
            return json.loads(payload) if payload else None
    except urllib.error.HTTPError as exc:
        return ("HTTP_ERROR", exc.code, exc.read().decode("utf-8", "replace"))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--owner", required=True)
    parser.add_argument("--repo", required=True)
    parser.add_argument("--tag", required=True)
    parser.add_argument("--notes", required=True, help="path to release notes markdown")
    parser.add_argument("--assets", nargs="+", required=True, help="files to upload")
    parser.add_argument("--title", default=None, help="release title (defaults to tag)")
    args = parser.parse_args()

    token = os.environ.get("GITHUB_TOKEN") or os.environ.get("GIT_ACCESS_TOKEN")
    if not token:
        print("error: GITHUB_TOKEN / GIT_ACCESS_TOKEN not set", file=sys.stderr)
        return 1

    repo = f"/repos/{args.owner}/{args.repo}"

    # 1. Delete an existing release for this tag (rolling semantics). Keep the tag.
    existing = request("GET", f"{repo}/releases/tags/{args.tag}", token)
    if isinstance(existing, tuple):
        if existing[1] != 404:
            print(f"error: lookup release failed: {existing}", file=sys.stderr)
            return 1
    else:
        deleted = request("DELETE", f"{repo}/releases/{existing['id']}", token)
        if isinstance(deleted, tuple):
            print(f"error: delete release failed: {deleted}", file=sys.stderr)
            return 1
        print(f"deleted previous release for {args.tag}")

    # 2. Create release from CHANGELOG notes.
    with open(args.notes, "r", encoding="utf-8") as fh:
        notes = fh.read()
    created = request(
        "POST",
        f"{repo}/releases",
        token,
        body={
            "tag_name": args.tag,
            "name": args.title or args.tag,
            "body": notes,
            "draft": False,
            "prerelease": False,
        },
    )
    if isinstance(created, tuple):
        print(f"error: create release failed: {created}", file=sys.stderr)
        return 1
    upload_url = created["upload_url"].split("{", 1)[0]
    print(f"created release {args.tag}: {created['html_url']}")

    # 3. Upload assets.
    for path in args.assets:
        name = os.path.basename(path)
        with open(path, "rb") as fh:
            raw = fh.read()
        result = request(
            "POST",
            f"{upload_url}?name={name}",
            token,
            raw=raw,
            content_type="application/octet-stream",
        )
        if isinstance(result, tuple):
            print(f"error: upload {name} failed: {result}", file=sys.stderr)
            return 1
        print(f"  uploaded {name} ({len(raw)} bytes)")

    print(f"done: {created['html_url']}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
