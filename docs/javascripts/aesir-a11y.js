/*!
 * AesirFramework header 无障碍名称补丁
 *
 * zensical 0.1.1 起,header 里的图标按钮(抽屉 / 搜索)去掉了 aria-label,
 * 只剩一个 <label class="md-header__button md-icon">,对读屏软件是无名称元素。
 * 0.0.65 时代这两个按钮带 aria-label("导航栏" / "查找"),升级后一并补回。
 *
 * 这里只补 aria-label 与 title(悬停提示),不改任何结构与样式;
 * 若主题将来自己补上了名称,补丁会检测到并跳过,不会覆盖上游。
 */
(function () {
  "use strict";

  /* for 属性 -> 可访问名称 */
  var LABELS = {
    __drawer: "导航栏",
    __search: "搜索",
  };

  function labelFor(id) {
    return LABELS[id];
  }

  function patch() {
    var nodes = document.querySelectorAll("label.md-header__button");
    for (var i = 0; i < nodes.length; i++) {
      var node = nodes[i];
      var target = node.getAttribute("for");
      var name = target ? labelFor(target) : null;
      if (!name) {
        continue;
      }
      /* 上游已给出可访问名称时保持原样 */
      if (!node.getAttribute("aria-label") && !node.getAttribute("title")) {
        node.setAttribute("aria-label", name);
        node.setAttribute("title", name);
      }
    }
  }

  function boot() {
    patch();
    /* navigation.instant 切换页面时 header 可能被重建,监听一次即可覆盖 */
    if (!window.__aesirA11yObserved) {
      window.__aesirA11yObserved = true;
      new MutationObserver(patch).observe(document.body, {
        childList: true,
        subtree: true,
      });
    }
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", boot);
  } else {
    boot();
  }
})();
