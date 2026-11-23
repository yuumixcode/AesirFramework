# UI 坑位详解（12 条）

> `../SKILL.md` 的坑位速查表命中某条编号时读本文对应条目，拿现象/原因/修法全文。速查表只给后果，修法在这里。

## 坑位（违反即报错或静默失效）

1. **解绑/退订必须放 `OnDestroy`**。`OnClose` 只在受控销毁路径（Hide + `DestroyOnHide=true`）调用；场景卸载或外部 `Destroy` 时**只有 `OnDestroy`**——只写 OnClose 的清理会在切场景时泄漏。
2. **层 Canvas 的 sortingOrder 会被强制覆盖**为 100/200/300/400（每次 Awake / Build 重新应用），手动微调层序无效；窗口的 sortingOrder 不被覆盖。
3. **同一基类类型重复 Show 会报错**；`Hide` / `Get` 未命中只给 Warning。类型要一路用同一个。
4. **路径别当成 payload 传**：`Show<T>("UI/Xxx")` 会命中专门的单参路径重载；要同时带参数就写 `Show<T>(payload, "UI/Xxx")`——否则字符串会被 `object payload` 吞掉、随后报"未注册且未提供 path"而不揭示真因。经**引用注册**的条目带 path 调用会警告"path 被忽略"。
5. **主相机要自行排除 UI 层(5)**：UICamera 只渲染 UI + TransparentFX，主相机含 UI 层会重复渲染。
6. **加载器是同步契约**：`IUIAssetLoader.Load(path)` 无 await。Addressables 直接 `LoadAssetAsync` 的 `Handle.Result` 同步等待在 WebGL 会死锁——先预加载、再经自定义 loader 同步返回缓存。
7. **运行时 `AddComponent` 出来的面板/窗口不做 Binder 自动绑定**（绑定只在编辑期生成脚本 → 编译后挂载时写入 prefab 序列化数据）。
8. **覆写 `OnShow`/`OnHide` 第一行必须调 `base`**。基类默认实现是 `gameObject.SetActive(true/false)`——面板/窗口以**停用状态**实例化，激活动作就藏在 `base.OnShow` 里。只写自己的刷新逻辑而不调 base，实例数据全对但**永远不可见**，且 `Get<T>` 拿得到实例、注册表正常，极难定位。可见性断言要带 `gameObject.activeInHierarchy == true`。
9. **DDOL 下离开场景要主动收尾本场景的面板/窗口**。UIRoot 常驻，挂在层 Canvas 下的面板**不随场景卸载销毁**——"面板交给场景卸载处理"的假设永远不成立。切场景前 `HideSelf()`，并在场景根的 `OnDestroy` 里统一 `Hide`/`Close` 本场景打开的面板窗口。验证必须有**负向断言**（"不该在的不在"），只验"该在的在"抓不到泄漏。
10. **需要订阅/初始化的组件节点不能以 `SetActive(false)` 起始**。禁用节点不执行 `Start`，`AddListenerAndInvoke` 订阅建立不了，后续永远不会点亮（血条/光圈/护盾这类"默认隐藏"组件高发）。用组件级 `spriteRenderer.enabled = false` 控制显隐，节点保持激活。多个驱动源（订阅回调 + 生命周期事件）并存的显隐状态，强制关闭要用带 `_forced` 标志的 `ForceHide()` **压过**所有驱动源，否则下一帧回调又点亮。
11. **URP 工程 UICamera 会清掉全部游戏画面**（HUD 正常、精灵全消失，但运行时数据在动）。UIRoot 的 UICamera 按内置管线语义配成 Base + `clearFlags=Depth`；URP 的多个 Base 相机是**整屏替换**而非叠加，主相机画面被它整个清掉。需在业务侧把 UICamera 改成 Overlay 并加入主相机 `cameraStack`——反射零硬依赖的通用接线组件见同目录 `urp-ui-camera-stack.md`。真机"UI 在但画面黑"第一嫌疑就是此接线。
12. **"未受管理的 XxxPanel" 告警先查是否直接 `LoadScene` 跳场景**。验证脚本绕过 Boot→Start→Bootstrap 正常流程直接切场景时，模块实例与面板注册表错位会误报泄漏；编辑模式清点激活实例若为 0 则无残留，不必"修"。实例名已复位为预制体名（无 `(Clone)` 后缀），报警文本里的名字可直接对照预制体。

