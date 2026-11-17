---
name: unity-ui-toolkit-advanced
description: Unity UI Toolkit advanced expert — UXML layout, USS styling, Visual Tree API, custom elements, data binding, animation transitions, ListView/TreeView optimization. Use when building complex editor tools and runtime UI, implementing data-driven interfaces, or optimizing list performance.
---

# Unity UI Toolkit Advanced

## UXML Layout

```xml
<!-- MainUI.uxml -->
<ui:UXML xmlns:ui="UnityEngine.UIElements" xmlns:uie="UnityEditor.UIElements">
    <ui:VisualElement class="root-container">
        <ui:Label text="Game Settings" class="title" />

        <ui:VisualElement class="settings-row">
            <ui:Label text="Master Volume" class="label" />
            <ui:Slider name="master-volume" value="80" low-value="0" high-value="100" class="slider" />
        </ui:VisualElement>

        <ui:VisualElement class="settings-row">
            <ui:Label text="Difficulty" class="label" />
            <ui:DropdownField name="difficulty" choices="Easy,Normal,Hard" value="Normal" />
        </ui:VisualElement>

        <ui:Button text="Apply" name="apply-btn" class="btn-primary" />
    </ui:VisualElement>
</ui:UXML>
```

## USS Styling

```css
/* MainUI.uss */
.root-container {
    flex-direction: column;
    padding: 20px;
    background-color: rgba(0, 0, 0, 0.85);
}

.title {
    font-size: 24px;
    color: #ffffff;
    margin-bottom: 20px;
    -unity-font-style: bold;
}

.settings-row {
    flex-direction: row;
    align-items: center;
    margin-bottom: 12px;
}

.label {
    width: 150px;
    color: #cccccc;
    font-size: 14px;
}

.slider {
    flex-grow: 1;
}

.btn-primary {
    background-color: #6366f1;
    color: white;
    border-radius: 8px;
    padding: 10px 20px;
    font-size: 14px;
    margin-top: 20px;
}

.btn-primary:hover {
    background-color: #4f46e5;
}

.btn-primary:active {
    background-color: #4338ca;
    scale: 0.98;
}

/* Transition animations */
.btn-primary {
    transition: background-color 0.15s ease, scale 0.1s ease;
}
```

## Loading UXML at Runtime

```csharp
using UnityEngine.UIElements;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private UIDocument _document;
    [SerializeField] private VisualTreeAsset _uxml;
    [SerializeField] private StyleSheet _uss;

    void Start()
    {
        var root = _document.rootVisualElement;
        root.Clear();

        // Load UXML
        var tree = _uxml.CloneTree();
        root.Add(tree);

        // Apply USS
        root.styleSheets.Add(_uss);

        // Query elements
        var slider = root.Q<Slider>("master-volume");
        slider.RegisterValueChangedCallback(evt =>
        {
            AudioListener.volume = evt.newValue / 100f;
        });

        var dropdown = root.Q<DropdownField>("difficulty");
        dropdown.RegisterValueChangedCallback(evt =>
        {
            PlayerPrefs.SetString("difficulty", evt.newValue);
        });

        var applyBtn = root.Q<Button>("apply-btn");
        applyBtn.clicked += OnApply;
    }

    void OnApply()
    {
        Debug.Log("Settings applied!");
        // Save settings
        PlayerPrefs.Save();
    }
}
```

## Visual Tree API (Code-only UI)

```csharp
// Create UI entirely in code (no UXML/USS files)
public class DynamicUI : MonoBehaviour
{
    private UIDocument _document;

    void Start()
    {
        var root = _document.rootVisualElement;

        // Create elements
        var container = new VisualElement();
        container.AddToClassList("container");
        root.Add(container);

        var label = new Label("Dynamic List");
        label.AddToClassList("title");
        container.Add(label);

        var scrollView = new ScrollView();
        container.Add(scrollView);

        // Dynamic items
        for (int i = 0; i < 100; i++)
        {
            var item = new VisualElement();
            item.AddToClassList("list-item");

            var nameLabel = new Label($"Item {i}");
            item.Add(nameLabel);

            var deleteBtn = new Button { text = "✕" };
            deleteBtn.clicked += () => item.RemoveFromHierarchy();
            item.Add(deleteBtn);

            scrollView.Add(item);
        }
    }
}
```

## ListView (Virtualized, High Performance)

```csharp
// ListView only creates visible items — handles 10,000+ entries efficiently
public class InventoryUI : MonoBehaviour
{
    private List<Item> _items = new(); // Could be 10,000+ items

    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        var listView = new ListView(_items, 40, MakeItem, BindItem);
        listView.selectionType = SelectionType.Single;
        listView.onItemsChosen += items =>
        {
            foreach (var item in items)
                Debug.Log($"Selected: {item.Name}");
        };
        root.Add(listView);
    }

    VisualElement MakeItem()
    {
        var container = new VisualElement();
        container.style.flexDirection = FlexDirection.Row;

        var icon = new VisualElement();
        icon.AddToClassList("item-icon");
        container.Add(icon);

        var label = new Label();
        label.AddToClassList("item-label");
        container.Add(label);

        return container;
    }

    void BindItem(VisualElement element, int index)
    {
        var label = element.Q<Label>();
        label.text = _items[index].Name;

        var icon = element.Children().First();
        icon.style.backgroundImage = _items[index].Icon;
    }
}
```

## Custom Visual Element

```csharp
using UnityEngine.UIElements;

// Custom element with custom rendering
public class RadialHealthBar : VisualElement
{
    private float _health = 1f;

    public float Health
    {
        get => _health;
        set { _health = value; MarkDirtyRepaint(); }
    }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        UxmlFloatAttributeDescription _health = new UxmlFloatAttributeDescription
        {
            name = "health", defaultValue = 1f
        };

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);
            ((RadialHealthBar)ve).Health = _health.GetValueFromBag(bag, cc);
        }
    }

    public new class UxmlFactory : UxmlFactory<RadialHealthBar, UxmlTraits> { }

    public RadialHealthBar()
    {
        generateVisualContent += OnGenerateVisualContent;
    }

    void OnGenerateVisualContent(MeshGenerationContext ctx)
    {
        // Custom drawing using MeshGenerationContext (like IMGUI)
        var painter = ctx.painter2D;
        painter.fillColor = Color.red;
        painter.BeginPath();
        painter.Arc(new Vector2(layout.width / 2, layout.height / 2), 50f, 0, _health * 360);
        painter.Fill();
    }
}
```

## Data Binding (Unity 2023.1+)

```csharp
// Runtime data binding — auto-syncs UI with data
using UnityEngine.UIElements;

// Serializable data source
[Serializable]
public class PlayerData
{
    public string Name;
    public int Level;
    public float Health;
}

public class DataBindingUI : MonoBehaviour
{
    [SerializeField] private UIDocument _document;
    [SerializeField] private PlayerData _data;

    void Start()
    {
        var root = _document.rootVisualElement;

        // Set data source
        root.dataSource = _data;

        // Bind label to data
        var nameLabel = root.Q<Label>("player-name");
        nameLabel.SetBinding("text", new DataBinding
        {
            dataSourcePath = new PropertyPath("Name"),
            bindingMode = BindingMode.TwoWay
        });

        // Bind slider to health
        var healthSlider = root.Q<Slider>("health-slider");
        healthSlider.SetBinding("value", new DataBinding
        {
            dataSourcePath = new PropertyPath("Health"),
            bindingMode = BindingMode.TwoWay
        });
    }
}
```

## Key Gotchas

1. **UI Toolkit vs uGUI**: UI Toolkit is CSS-like, better for editor tools and complex data UIs. uGUI is better for runtime game UI with lots of sprites/animations.
2. **USS ≠ CSS**: USS is a subset of CSS. No grid, no animations (use transitions), limited pseudo-classes (:hover, :active, :focus).
3. **ListView performance**: ListView is virtualized — only creates visible items. Use `bindItem` callback to populate. Don't create all items upfront.
4. **MarkDirtyRepaint**: Custom elements need `MarkDirtyRepaint()` when data changes, otherwise they won't redraw.
5. **UXML at runtime**: Requires `UIDocument` component. UXML/USS files must be referenced (not loaded from disk at runtime).
6. **No GameObject**: UI Toolkit elements are NOT GameObjects. No `transform.position`, no `GetComponent`. Use `element.worldBound` for position.
7. **Transitions**: USS supports `transition` property. For complex animations, use `element.experimental.animation` API.
8. **Editor vs Runtime**: In Editor, UI Toolkit is default for Inspector windows. For runtime, need `UIDocument` component.
