# AesirFramework 文档站（同仓工程说明）

**AesirFramework 的产品综合站点** —— 产品介绍、上手路径、使用文档与 Scripting API 参考。

- 线上地址:https://yuumixcode.github.io/AesirFramework/
- 框架源码:https://github.com/yuumixcode/AesirFramework
- 开发者内部笔记:`Aesir-Notebook`(私有,不对外)

## 站点定位与仓库边界

站点自 2026-11-23 起**与框架同仓**:内容在主仓根 `docs/` + `zensical.toml`,由 `.github/workflows/docs.yml` 构建发布;本目录(`tools/docs-site/`)存放站点本地工具、品牌源图与工程约定。原独立仓库 `AesirFramework-Site` 保留为全路径跳转壳与历史留档(旧深链自动跳新站对应页)。

站点承担**产品门面 + 文档中心**两层职责:

| 分区 | 面向 | 内容 |
| --- | --- | --- |
| 首页 | 第一次来的人 | 产品定位、入口分流、快速安装、价值主张 |
| **开始** | 准备上手的人 | 安装与升级、两包快速开始、兼容性、FAQ |
| **产品** | 评估选型的人 | 特性总览、示例总览、版本与下载、路线图、工具链与生态 |
| **文档** | 已经上手的人 | Aesir Architecture / Aesir Modules 的专题文档 |
| **Scripting API** | 查类型与成员的人 | 330 个类型的自动生成参考页 |

## 技术栈

- [Zensical](https://zensical.org/)(Rust 静态站生成器,Material for MkDocs 继任者),当前锁定 `0.1.1`
- 内容:Markdown(主仓根 `docs/`),配置:主仓根 `zensical.toml`
- 部署:GitHub Pages(GitHub Actions,见主仓 `.github/workflows/docs.yml`;仅 `docs/**` / `zensical.toml` / workflow 自身变更触发)

## 设计系统

站点视觉基于 **黑曜石 + 熔金符文**(源图 `docs/assets/Runestone-Icon-1.png` 的黑曜石碑与灼烧金字),令牌层集中在 `docs/stylesheets/extra.css` 顶部的 `:root` / `[data-md-color-scheme="slate"]` 两组变量里。

| 角色 | 值 | 说明 |
| --- | --- | --- |
| 纸面 / 石面 | `#fbfbfa` / `#0e1013` | 浅色近白,暗色近黑 |
| 墨色 | `#14161b` / `#f2f3f5` | 正文与标题 |
| 熔金 | `#8a5a0b` / `#e0a545` | 链接、强调、活动项 |
| 展示体 | Space Grotesk | 标题、品牌字 |
| 正文体 | Inter | 段落、界面文字 |
| 等宽体 | JetBrains Mono | 代码、标签、版本号 |

五条约定:

- **字体自托管。** `docs/assets/fonts/` 放的是可变字体的拉丁子集(共约 111 KB),`zensical.toml` 里 `theme.font = false` 关掉了主题默认的 Google Fonts 链接 —— 避免国内不可达与第三方依赖。中文交给系统字体(PingFang SC / 微软雅黑 / Noto Sans SC)。
- **Material modern 把 `html` 设为 20px**,所以 `1rem = 20px`。写尺寸前先按目标 px 折算,别按 16px 想当然。
- **改 Material 变量要压过调色板 CSS。** 调色板用 `[data-md-color-scheme=x][data-md-color-primary=y]`(0,2,0),本站用 `body[data-md-color-scheme=x][data-md-color-primary]`(0,2,1) 覆盖。
- **页签活动条只画一条。** Material 把活动指示条画在 `.md-tabs__item--active` 上(`border-bottom:.05rem solid var(--md-default-fg-color)`),不在 `.md-tabs__link` 上。在链接上再画一条会叠成双线 —— 本站只覆写颜色:`.md-tabs__item--active { border-bottom-color: var(--aesir-gold) }`,链接上不再加边框。
- **`edit_uri` 必须显式置空。** zensical 0.1.x 会把「仓库地址 + `edit_uri` + 页面源路径」拼成绝对 URL 并按 IRI 校验,而 Scripting API 页的源文件名含 C# 泛型占位(`ObservableList{T}.md`),花括号在 IRI 中非法 → 每个此类页面触发一次 panic(共 41 次),连带 51 条链接告警。写成 `edit_uri = ""`(空串)即可不生成编辑链接;**删掉这一行不行**,zensical 会自动回填成 `edit/master/docs`,照样 panic。仓库入口由 header 的仓库按钮与 nav 末项「GitHub」承担。

### 图标资产

全部从 `docs/assets/Runestone-Icon-1.png`(766×926 的 Runestone 石碑)提取生成,几何完全一致:

| 文件 | 尺寸 | 用途 |
| --- | --- | --- |
| `assets/logo.png` | 256×256 | 导航栏 logo(`theme.logo`) |
| `assets/favicon.png` | 64×64 | `<link rel="icon">`(`theme.favicon`) |
| `assets/favicon.ico` | 16/32/48/64/128/256 | 多尺寸 ICO |
| `assets/apple-touch-icon.png` | 180×180 | iOS 主屏(iOS 不支持透明,必须实底) |
| `assets/rune.png` | 332×688 | 英雄区符文刻印的 CSS mask(alpha-only) |
| `favicon.ico` / `apple-touch-icon.png` | — | 放在 `docs/` 根目录,构建后落到站点根,浏览器自动发现 |

三处要点:

- **符文是整块独石在 16px 下不可读的。** 独石整体裁下来在 16/32px 就是一条灰棍,所以图标一律用「黑曜石圆角方块 + 符文」,方块提供识别度,符文提供品牌。
- **符文笔画在原图里只有 ~10px 宽**,缩到 32px 会掉到亚像素,所以图标里统一加粗 1px(`dilate`)。
- **英雄区的 CSS 符文已换成真符文。** 原来是照旧 logo 手画的 SVG 字形,与新标志不是同一个字;现在直接用 `assets/rune.png` 做 `mask`,颜色仍由 `--aesir-gold-hi` 决定。PNG 只存 alpha 通道,提取后做了「模糊 + 对比曲线」抗锯齿,否则 JPEG 噪点会在斜边上形成阶梯。

### 搜索面板在 Shadow DOM 里

zensical 0.1.1 的搜索组件把 UI 渲染进 `attachShadow({mode:"open"})` 的 shadow root(类名是哈希值,如 `e`/`p`/`l`,不可写 CSS 选择器)。两个结论:

- **外部样式表触达不到面板内部。** 主题 CSS 变量可以穿透继承(面板的 `--color-highlight` 就指向 `--md-accent-fg-color`),所以熔金高亮是自动生效的。
- **验证搜索是否可用必须穿透 shadow root。** `document.querySelector(".md-search__form")` 永远返回 `null`,那是**测量方式错了,不是功能坏了**。正确写法是遍历 `document.querySelectorAll("*")` 找带 `shadowRoot` 的元素,再 `host.shadowRoot.querySelector("input")`。

面板内烤死的英文文案(placeholder、`Filters`、`Tags`、`N results`)没有 locale 表可覆盖,由 `docs/javascripts/aesir-search.js` 在运行时替换;同时补一个熔金焦点环。面板每次重绘都会重建 DOM,脚本用 MutationObserver 持续补齐。

### 版本:0.0.65 → 0.1.1

0.1.1 相对 0.0.65 有两处与本站直接相关:

- **构建会 panic。** 见上面第 5 条约定(`edit_uri` 必须置空)。41 个带泛型占位的 API 页各触发一次 panic,并连带 51 条「page does not exist」链接告警,`--strict` 与非 strict 都会以 exit 1 失败。这与上游 issue [#1147](https://github.com/zensical/zensical/issues/1147) 同源(该 issue 描述的是方括号,成因相同)。
- **header 图标按钮丢了可访问名称。** 0.0.65 的抽屉 / 搜索按钮带 `aria-label`,0.1.1 去掉了,图标按钮对读屏软件变成无名称元素。由 `docs/javascripts/aesir-a11y.js` 补回(检测到上游已给出名称时会跳过,不覆盖)。

除此之外零回归:根字号仍是 `html{font-size:125%}`(1rem=20px)、页签活动条仍画在 `.md-tabs__item--active` 上、搜索仍在 shadow DOM 内、图标与产物文件数一致(343 个)。用同一套脚本跑 0.0.65 与 0.1.1,12 项断言结果完全相同。

0.1.x 还原生支持 MkDocs 的 `social` 插件(自动生成 OG / Twitter 卡片与社交分享图),这是补社交分享图的最省事路径,目前尚未启用。

## 本地开发

以下命令均在**主仓根**运行:

```bash
pip install "zensical==0.1.1"   # 或 pipx install zensical
zensical serve                   # 本地预览,默认 http://localhost:8000(site_url 子路径会自动 302)
zensical build                   # 构建到 site/(gitignored)
zensical build --strict          # 严格模式(CI 用,警告即失败;docs 变更的门禁)
```

改样式时用 `tools/docs-site/serve-preview.py` 起一个**禁用缓存**的静态服务器,否则浏览器会一直用旧的 `extra.css`:

```bash
python3 tools/docs-site/serve-preview.py 8900 site    # http://127.0.0.1:8900(带 no-store 头)
node tools/docs-site/shoot.mjs /tmp/aesir-shots http://127.0.0.1:8900   # 多断点截图:桌面/平板/手机 × 浅色/暗色(临时产物用完即删)
```

## 目录结构

```
<主仓根>/
├── zensical.toml              # 站点配置(site_url / nav / theme;edit_uri 必须置空)
├── docs/
│   ├── index.md               # 产品落地页
│   ├── start/                 # 开始:安装、兼容性
│   ├── product/               # 产品:特性、示例、版本、路线图、生态
│   ├── architecture/          # Aesir Architecture 文档(+ scripting-api/)
│   ├── modules/               # Aesir Modules 文档(+ scripting-api/)
│   ├── changelog.md / faq.md / support.md
│   ├── assets/                # logo、favicon、apple-touch-icon、符文遮罩、源图
│   │   ├── Runestone-Icon-1.png # 品牌源图(766×926,图标全部由它提取)
│   │   ├── logo.png / favicon.png / favicon.ico / apple-touch-icon.png
│   │   ├── rune.png           # 英雄区符文的 alpha-only 遮罩
│   │   └── fonts/             # 自托管字体(拉丁子集 woff2)
│   ├── javascripts/           # aesir-search.js(搜索面板 shadow DOM 补丁)、aesir-a11y.js(header 图标按钮可访问名称)
│   └── stylesheets/           # extra.css(设计系统 + 产品页样式)、api.css(API 页样式)
├── site/                      # 构建产物(gitignored)
├── .cache/                    # zensical 增量缓存(gitignored)
└── tools/docs-site/
    ├── serve-preview.py       # 禁用缓存的本地预览服务器
    ├── shoot.mjs              # Playwright 多断点视觉回归截图
    ├── assets-src/logo.png    # 品牌源图备份
    └── README.md              # 本文件
```

`docs/architecture/` 与 `docs/modules/` 的**物理路径不要改** —— Scripting API 的生成与导航重建脚本(`.codely-cli/memory/docs-site-rebuild-api-nav.py`)按这些路径读写,`package.json` 的 `documentationUrl` / `changelogUrl` 也指向它们。

## 内容维护约定

- **正文使用半角标点**(`,` `:` `;` `(` `)`),与主仓 README 的全角风格不同;从 README 拷贝内容时需要转换
- **中文标题必须带显式英文锚点** `## 标题 {#anchor}` —— slugify 会丢弃中文字符,含中文锚点的页内链接会构建失败
- 表格加 class 必须用 `<div class="x" markdown="1">` 包裹(`{: .cls }` 对表格无效)
- **版本落点标注**("`<版本>` 起 / 已于 `<版本>`")随发版替换为当前版本(随主仓发版提交,见 `aesir-version-sync` 技能);`changelog.md` 的历史段永不改写
- 提交信息:`docs: 中文单行主题`(Conventional Commits,与主仓一致);提交身份 `zeriying@gmail.com`(与主仓 git config 一致)

## Scripting API 页的更新流程

API 页由包内 Script Doc Generator 反射生成,不手写。**增量重生成**(2026-11-16 实证口径,替代早期「清目录全量 rsync」):

1. 编辑器开着时用 `tuanjie command eval --stdin` 调 `ScriptDocGeneratorAPI.GenerateDocsForAssembly(<程序集短名>, ScriptDocGeneratorAPI.ZensicalSettings, "/tmp/xxx")`,把目标程序集全量生成到临时目录(**settings 必须显式传 ZensicalSettings**,传 null 落中文 Default 设置,格式不是站点这套)
2. `diff -rq` 与主仓 `docs/architecture/scripting-api/`、`docs/modules/scripting-api/` 比对,**只拷差异文件**(生成器自带增量合并,保留各页 `## Additional Notes` 之后的手写内容)
3. 三件套必做:`scripting-api/index.md` 索引行(分节按字母序 + 命名空间类型计数)、`zensical.toml` **逐页** nav 条目(漏加 = 页面存在但侧栏看不到)、本 README 的 API 类型总数
4. `zensical build --strict` 验证(0 issue 才算通)

辅助脚本:`.codely-cli/memory/docs-site-rebuild-api-nav.py`(索引 / nav 重生成)。

MIT License © 2026 Runestone Yuumix