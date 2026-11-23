/*!
 * AesirFramework 搜索面板补丁
 *
 * zensical 0.0.65 把搜索 UI 渲染进 attachShadow({mode:"open"}) 的 shadow root,
 * 外部样式表无法触达,主题 CSS 变量则可以继承。因此这里只做三件外部做不到的事:
 *   1. 把输入框 placeholder 从英文 "Search" 换成中文;
 *   2. 给输入框补一个与 Aesir 熔金令牌一致的焦点环;
 *   3. 把面板内烤死在 Rust 模板里的英文文案换成中文。
 *
 * 搜索面板每次重新渲染结果都会重建 DOM,故用 MutationObserver 持续补齐。
 */
(function () {
  "use strict";

  var PLACEHOLDER = "搜索文档";
  var STYLE_ID = "aesir-search-style";

  /* 面板文案 —— Rust 搜索插件模板里烤死的英文,无 locale 表可覆盖 */
  var TEXT_MAP = {
    Filters: "筛选",
    Tags: "标签",
  };

  var STYLE_TEXT = [
    /* 熔金焦点环 —— 与 --aesir-gold-hi 同源 */
    "input:focus{outline:2px solid var(--md-accent-fg-color,rgba(255,201,120,.7));outline-offset:-2px;}",
    /* placeholder 用次级前景色,避免抢焦点态的注意力 */
    "input::placeholder{color:var(--md-default-fg-color--light,rgba(20,22,27,.5));opacity:1;}",
    /* 搜索词高亮跟随品牌强调色 */
    "mark{color:inherit;background-color:var(--md-accent-fg-color,rgba(255,201,120,.28));}",
  ].join("");

  /* shadow host 是 gc() 追加到 body 的匿名 div,唯一特征是带 shadowRoot */
  function findHost() {
    var children = document.body.children;
    for (var i = 0; i < children.length; i++) {
      if (children[i].shadowRoot) {
        return children[i];
      }
    }
    return null;
  }

  function localize(root) {
    if (!root) {
      return;
    }
    var input = root.querySelector("input");
    if (input && input.getAttribute("placeholder") !== PLACEHOLDER) {
      input.setAttribute("placeholder", PLACEHOLDER);
    }
    translate(root);
  }

  /* 只替换整段等于已知文案的文本节点,避免误伤正文与代码片段 */
  function translate(root) {
    var walker = document.createTreeWalker(root, NodeFilter.SHOW_TEXT);
    var node;
    while ((node = walker.nextNode())) {
      var raw = node.nodeValue;
      var trimmed = raw.trim();
      if (!trimmed) {
        continue;
      }

      /* 结果计数渲染成 <span>233</span> results,数字独立成元素,
         这里只替换 "results" 这一段,前导空白归一化成单个空格 */
      if (trimmed === "results") {
        node.nodeValue = (/^\s/.test(raw) ? " " : "") + "条结果";
        continue;
      }

      var next = TEXT_MAP[trimmed];
      if (next === undefined) {
        var matched = /^(\d+)\s+results?$/.exec(trimmed);
        if (matched) {
          next = matched[1] + " 条结果";
        }
      }
      if (next !== undefined && next !== trimmed) {
        node.nodeValue = raw.replace(trimmed, next);
      }
    }
  }

  function ensureStyle(root) {
    if (!root || root.getElementById(STYLE_ID)) {
      return;
    }
    var style = document.createElement("style");
    style.id = STYLE_ID;
    style.textContent = STYLE_TEXT;
    root.appendChild(style);
  }

  function attach(host) {
    if (host.__aesirSearchPatched) {
      return;
    }
    host.__aesirSearchPatched = true;

    var root = host.shadowRoot;
    localize(root);
    ensureStyle(root);

    /* 面板重绘会重建 input 与 style,变化时补齐 */
    new MutationObserver(function () {
      localize(root);
      ensureStyle(root);
    }).observe(root, { childList: true, subtree: true });
  }

  function boot() {
    var host = findHost();
    if (!host) {
      return false;
    }
    attach(host);
    return true;
  }

  if (!boot()) {
    /* 搜索组件晚于本脚本初始化,监听 body 子节点变化 */
    var observer = new MutationObserver(function () {
      if (boot()) {
        observer.disconnect();
      }
    });
    observer.observe(document.body, { childList: true });
  }
})();
