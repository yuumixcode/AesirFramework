# Runtime Data Binding Reference

Unity UI Toolkit runtime data binding for efficient UI updates.

## Data Source Setup

### Required: [CreateProperty] Attribute

```csharp
using Unity.Properties;
using UnityEngine;

public class PlayerData
{
    [CreateProperty]
    public int Health { get; set; }

    [SerializeField, DontCreateProperty]
    private float m_Speed;

    [CreateProperty]
    public float Speed
    {
        get => m_Speed;
        set => m_Speed = value;
    }
}
```


## CRITICAL: Always Use nameof()
```csharp
element.SetBinding("value", new DataBinding
{
    dataSourcePath = new PropertyPath(nameof(HealthData.HealthPercentage))
});
```

## C# SetBinding (Programmatic)

```csharp
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class HealthBarController : MonoBehaviour
{
    [SerializeField] private HealthData m_HealthData;
    private Label m_HealthLabel;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        m_HealthLabel = root.Q<Label>("health-label");

        m_HealthLabel.SetBinding("text", new DataBinding
        {
            dataSourcePath = new PropertyPath(nameof(HealthData.HealthText))
        });

        m_HealthLabel.dataSource = m_HealthData;
    }
}
```

## PanelRenderer with Bindings (Unity 6.6+)

Use `PanelRenderer` for runtime UI with bindings to ensure bindings are properly re-established if the UI reloads.

```csharp
GetComponent<PanelRenderer>().RegisterUIReloadCallback((panelRenderer, rootElement) => {
    rootElement.dataSource = m_Data;
    // set bindings here...
});
```
