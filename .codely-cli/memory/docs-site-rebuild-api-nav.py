import io, os, re, glob, sys

SITE = "/Users/yuumix/Projects/Unity/AesirFramework"
KIND_ORDER = ["类", "结构体", "接口", "枚举", "委托"]
KIND_MAP = {
    "class": "类", "static class": "类", "abstract class": "类", "sealed class": "类",
    "struct": "结构体", "readonly struct": "结构体", "interface": "接口", "enum": "枚举",
    "delegate": "委托",
}

def parse_page(path):
    t = io.open(path, encoding="utf-8").read()
    title = re.search(r'^title:\s*(.+?)\s*$', t, re.M).group(1).strip().strip('"')
    kind_raw = re.search(r'\*\*种类:\*\*\s*`([^`]+)`', t).group(1).strip()
    ns = re.search(r'\*\*命名空间:\*\*\s*`([^`]+)`', t).group(1).strip()
    asm = re.search(r'\*\*程序集:\*\*\s*`([^`]+)`', t).group(1).strip()
    kind = KIND_MAP.get(kind_raw)
    if kind is None:
        raise SystemExit("未知种类 %r (%s)" % (kind_raw, path))

    # summary: 第一个 ``` csharp 代码块之后、第一个空行前的连续段落
    i = t.index("## 声明")
    j = t.index("```", i)                 # 开栅栏
    k = t.index("```", j + 3)             # 闭栅栏
    rest = t[k + 3:]
    lines = rest.split("\n")
    para = []
    for ln in lines:
        s = ln.strip()
        if not s:
            if para:
                break
            continue
        if s.startswith("#") or s.startswith("**") or s.startswith("---") or s.startswith("|") or s.startswith("*"):
            break
        para.append(s)
    summary = " ".join(para).strip()
    summary = summary.replace("|", "\\|")
    if not summary:
        summary = "—"
    elif len(summary) > 90:
        summary = summary[:88] + "…"
    return dict(title=title, kind=kind, ns=ns, asm=asm, summary=summary, path=path)

def collect(api_root):
    pages = []
    for p in glob.glob(os.path.join(api_root, "Runestone", "**", "*.md"), recursive=True):
        pages.append(parse_page(p))
    return pages

def build_index(pages, desc, api_root):
    by_ns = {}
    for p in pages:
        by_ns.setdefault(p["ns"], []).append(p)
    out = ["---",
           "title: Scripting API",
           'description: "%s"' % desc,
           "---",
           "",
           "# Scripting API",
           "",
           "本参考由 Script Doc Generator 基于 C# 反射离线生成，重新生成时自动保留各页面 `## Additional Notes` 之后的手写补充。页面按字母序排列；使用左侧目录或站内搜索定位类型。",
           ""]
    for ns in sorted(by_ns):
        items = by_ns[ns]
        asms = "、".join("`%s`" % a for a in sorted({i["asm"] for i in items}))
        out.append("## %s" % ns)
        out.append("")
        out.append("命名空间 `%s` · 程序集 %s · 共 %d 个类型。" % (ns, asms, len(items)))
        out.append("")
        for kind in KIND_ORDER:
            group = sorted([i for i in items if i["kind"] == kind], key=lambda x: x["title"].lower())
            if not group:
                continue
            out.append("### %s" % kind)
            out.append("")
            out.append('<div class="api-summary-table" markdown="1">')
            out.append("")
            out.append("| 名称 | 描述 |")
            out.append("| :--- | :--- |")
            for it in group:
                rel = os.path.relpath(it["path"], os.path.dirname(api_root + "/index.md")).replace(os.sep, "/")
                out.append("| [`%s`](<%s>) | %s |" % (it["title"], rel, it["summary"]))
            out.append("")
            out.append("</div>")
            out.append("")
    return "\n".join(out).rstrip() + "\n"

def build_nav(pages, url_root, index_label, index_url):
    by_asm = {}
    for p in pages:
        by_asm.setdefault(p["asm"], []).append(p)
    lines = ['      { "%s" = "%s" },' % (index_label, index_url)]
    for asm in sorted(by_asm):
        lines.append('      { "%s" = [' % asm)
        items = sorted(by_asm[asm], key=lambda x: x["path"])
        for it in items:
            rel = os.path.relpath(it["path"], os.path.join(SITE, "docs")).replace(os.sep, "/")
            lines.append('          { "%s" = "%s" },' % (it["title"], rel))
        lines.append('      ]},')
    return lines

# ---------- RAA ----------
raa_root = os.path.join(SITE, "docs/architecture/scripting-api")
raa_pages = collect(raa_root)
io.open(os.path.join(raa_root, "index.md"), "w", encoding="utf-8").write(
    build_index(raa_pages, "Runestone.AesirArchitecture 系列命名空间的 Scripting API 参考", raa_root))
print("RAA index: %d pages, %d namespaces" % (len(raa_pages), len({p['ns'] for p in raa_pages})))

# ---------- RAM ----------
ram_root = os.path.join(SITE, "docs/modules/scripting-api")
ram_pages = collect(ram_root)
io.open(os.path.join(ram_root, "index.md"), "w", encoding="utf-8").write(
    build_index(ram_pages, "Runestone.AesirModules 系列命名空间的 Scripting API 参考", ram_root))
print("RAM index: %d pages, %d namespaces" % (len(ram_pages), len({p['ns'] for p in ram_pages})))

# ---------- nav ----------
nav_path = os.path.join(SITE, "zensical.toml")
nav = io.open(nav_path, encoding="utf-8").read().split("\n")
start = None
end = None
for i, l in enumerate(nav):
    if l == '  { "Scripting API" = [':
        start = i
        break
assert start is not None, "未找到 Scripting API 导航块"
for j in range(start + 1, len(nav)):
    if nav[j] == '  ]},':
        end = j
        break
assert end is not None, "未找到 Scripting API 导航块结束"
print("nav block lines: %d..%d (1-based)" % (start + 1, end + 1))

new_block = ['  { "Scripting API" = [']
new_block += build_nav(raa_pages, "architecture/scripting-api", "命名空间参考", "architecture/scripting-api/index.md")
new_block += build_nav(ram_pages, "modules/scripting-api", "Modules 命名空间参考", "modules/scripting-api/index.md")
new_block.append('  ]},')
nav[start:end + 1] = new_block
io.open(nav_path, "w", encoding="utf-8").write("\n".join(nav))
print("nav rebuilt: %d -> %d lines" % (end + 1 - start, len(new_block)))
