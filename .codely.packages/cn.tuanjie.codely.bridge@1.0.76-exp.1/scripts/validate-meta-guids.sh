#!/usr/bin/env sh
# Validate that all .meta files contain proper Unity GUIDs (32 lowercase hex).
# Skips build directories and other generated artifacts.
#
# Run:
#   sh scripts/validate-meta-guids.sh
#   ./scripts/validate-meta-guids.sh
# Windows: scripts\validate-meta-guids.bat
set -eu
cd "$(dirname "$0")/.." || exit 1

err=0
find . -name "*.meta" \
  -not -path "./.git/*" \
  -not -path "*/build/*" \
  -not -path "*/obj/*" \
  -not -path "*/_fix_guids_tmp*" \
  | while IFS= read -r f; do
  guid=$(awk '/^guid:/ { gsub(/\r/, "", $2); print $2 }' "$f")
  [ -z "$guid" ] && continue
  valid=$(printf '%s' "$guid" | awk '{ if (length($0) == 32 && $0 ~ /^[a-f0-9]+$/) print "ok" }')
  if [ "$valid" != "ok" ]; then
    echo "$f: Invalid GUID -> $guid"
    err=1
  fi
done

if [ "$err" -ne 0 ]; then
  exit 1
fi
echo "All .meta GUID checks passed."
