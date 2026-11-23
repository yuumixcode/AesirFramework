# 世界空间 UI（血条 / 名字牌）

> `../SKILL.md` 的展开参考：要做头顶血条、名字牌、交互提示这类随世界物体移动的 UI 时读本文。

## 世界空间 UI（血条 / 名字牌）

```csharp
// 方式一：组件直接挂在目标节点下（FollowTarget 留空，随父级移动）
// Add Component → Aesir Modules → UI → World Space UI Canvas

// 方式二：运行时创建
var popup = UIRoot.Instance.CreateWorldSpaceCanvas(interactable.transform, "InteractHint");
popup.PositionOffset = new Vector3(0f, 2f, 0f);
popup.Billboard = WorldSpaceUICanvas.WorldSpaceBillboardMode.CameraFacingFull; // LateUpdate 每帧读取，立即生效
```

- `Awake` 自动补齐 `Canvas(WorldSpace)` + `RectTransform`（正方形像素网格，避免非等比拉伸）+ `CanvasScaler` + `GraphicRaycaster`，子树归 UI 层。
- `sortingOrder` 默认 **450**（介于面板 ≤400 与窗口 ≥500 之间）。
- 配置来源优先级：组件上显式配置 → `UIRoot.WorldSpaceCanvasConfig` → 内存默认 `UICanvasConfigSO.CreateWorldSpaceDefault()`。世界空间链与屏幕空间链**互不覆盖**。
- 跟随目标 / `PositionOffset` / `FaceCamera` / `Billboard` / `FlipFacing` 由 `LateUpdate` 每帧读取，运行时改动即时生效；**配置与 `sortingOrder` 属接线期参数**——换配置需再调 `Build(overrideConfig)`，`sortingOrder` 为序列化字段（`SortingOrder` 属性无 setter），Inspector 改动后同样要 `Build()` 才刷新 Canvas。

