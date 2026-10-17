#!/usr/bin/env sh
# Validate that all .meta files contain proper Unity GUIDs (32 lowercase hex).
# Also checks that GUIDs are unique across the repository.
# Skips build directories and other generated artifacts.
#
# Run:
#   sh scripts/validate-meta-guids.sh
#   ./scripts/validate-meta-guids.sh
# Windows: scripts\validate-meta-guids.bat
set -eu
cd "$(dirname "$0")/.." || exit 1

err=0
tmp_list=".validate-meta-guids.list.$$"
tmp_guids=".validate-meta-guids.guids.$$"
find . -name "*.meta" \
  -not -path "./.git/*" \
  -not -path "*/build/*" \
  -not -path "*/obj/*" \
  > "$tmp_list"

: > "$tmp_guids"
while IFS= read -r f; do
  [ -z "$f" ] && continue
  guid=$(awk '/^guid:/ { gsub(/\r/, "", $2); print $2; exit }' "$f")
  [ -z "$guid" ] && continue
  valid=$(printf '%s' "$guid" | awk '{ if (length($0) == 32 && $0 ~ /^[a-f0-9]+$/) print "ok" }')
  if [ "$valid" != "ok" ]; then
    echo "$f: Invalid GUID -> $guid"
    err=1
    continue
  fi
  printf '%s\t%s\n' "$guid" "$f" >> "$tmp_guids"
done < "$tmp_list"

if [ -s "$tmp_guids" ]; then
  dups=$(
    sort -t "$(printf '\t')" -k1,1 "$tmp_guids" | awk -F'\t' '
      NR > 1 && $1 == prev {
        if (!reported[$1]++) {
          printf "Duplicate GUID %s: %s, %s\n", $1, prev_file, $2
        } else {
          printf "Duplicate GUID %s: ... %s\n", $1, $2
        }
      }
      { prev = $1; prev_file = $2 }
    '
  )
  if [ -n "$dups" ]; then
    echo "$dups"
    err=1
  fi
fi

rm -f "$tmp_list" "$tmp_guids"

if [ "$err" -ne 0 ]; then
  exit 1
fi
echo "All .meta GUID checks passed."
