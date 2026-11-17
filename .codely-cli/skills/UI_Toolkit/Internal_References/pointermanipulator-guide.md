# UI Toolkit Manipulator Reference

Pointer Manipulators handle pointer interactions like drag and drop, click, hover, and gestures.

## Base Pattern

```csharp
public class DragManipulator : PointerManipulator
{
    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<PointerDownEvent>(OnPointerDown);
        target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        target.RegisterCallback<PointerUpEvent>(OnPointerUp);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
        target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
        target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
    }
}
```

Attach with: `element.AddManipulator(new DragManipulator())`

## Drag and Drop Best Practices

1. **PointerDown**: Capture pointer, mark as dragging, set `Position.Absolute`, `BringToFront()`.
2. **PointerMove**: Update `style.translate` using StyleTranslate API.
3. **PointerUp**: Check for drop targets using `VisualElement.panel.Pick(position)`.
4. **Visual Feedback**: Use USS classes (`.dragging`, `.drop-target-active`) instead of inline styles.
5. **Performance**: Use `UsageHints.DynamicTransform`.

## StyleTranslate API
Always use `target.style.translate` for updating position during drag:
```csharp
target.style.translate = new StyleTranslate(newWorldPosition);
```
