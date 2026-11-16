---
name: unity-scriptableobjects
description: Unity ScriptableObjects architecture expert — data-driven design, SO creation, editor integration, runtime instances, event systems, polymorphic inheritance. Use when designing decoupled architectures with ScriptableObjects, creating reusable data assets, building configuration systems, or implementing event-driven systems with SO events.
---

# Unity ScriptableObjects

## What & Why

ScriptableObject (SO) = data asset stored in Project, not scene. Lives in `.asset` files.

**Advantages over MonoBehaviour:**
- No per-instance overhead in scene (one asset, many references)
- Editable in Inspector without entering play mode
- Shared across scenes
- Can be referenced by multiple components (single source of truth)
- No transform/GameObject baggage

## Creating SO

```csharp
[CreateAssetMenu(fileName = "NewWeapon", menuName = "Game/Weapon")]
public class WeaponData : ScriptableObject
{
    public string displayName;
    public float damage = 10f;
    public float attackRange = 2f;
    public Sprite icon;
    public GameObject prefab;
    [TextArea] public string description;
}
```

Right-click in Project → `Create > Game > Weapon` → `.asset` file created.

## Using SO in Components

```csharp
public class Weapon : MonoBehaviour
{
    public WeaponData data; // Drag .asset into Inspector

    public void Attack(Health target)
    {
        target.TakeDamage(data.damage);
    }
}
```

## Runtime Mutability (Clone Pattern)

```csharp
// SO fields are shared — modifying one affects all references
// If you need per-instance data, CLONE the SO:
public WeaponData runtimeData;

void Awake()
{
    runtimeData = Instantiate(data); // Cloned copy, safe to modify
    runtimeData.damage *= levelMultiplier;
}
```

## SO Event System

```csharp
[CreateAssetMenu(menuName = "Events/GameEvent")]
public class GameEvent : ScriptableObject
{
    private readonly List<GameEventListener> _listeners = new();

    public void Raise()
    {
        // Iterate backwards for safe removal during raise
        for (int i = _listeners.Count - 1; i >= 0; i--)
            _listeners[i].OnEventRaised();
    }

    public void Register(GameEventListener listener) => _listeners.Add(listener);
    public void Unregister(GameEventListener listener) => _listeners.Remove(listener);
}
```

```csharp
public class GameEventListener : MonoBehaviour
{
    public GameEvent gameEvent;
    public UnityEvent response; // Assign in Inspector

    void OnEnable() => gameEvent?.Register(this);
    void OnDisable() => gameEvent?.Unregister(this);
    public void OnEventRaised() => response?.Invoke();
}
```

## SO Variables (Typed)

```csharp
public abstract class Variable<T> : ScriptableObject
{
    public T Value;
    public T DefaultValue;

    public void SetValue(T value) => Value = value;
    public void Reset() => Value = DefaultValue;
}

[CreateAssetMenu(menuName = "Variables/FloatVariable")]
public class FloatVariable : Variable<float> { }
```

## Polymorphic SO (Item System)

```csharp
public abstract class ItemBase : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public abstract void Use(GameObject user);
}

[CreateAssetMenu(menuName = "Items/Health Potion")]
public class HealthPotion : ItemBase
{
    public float healAmount = 50f;
    public override void Use(GameObject user)
    {
        var health = user.GetComponent<Health>();
        if (health) health.Heal(healAmount);
    }
}
```

## Editor Integration

```csharp
[CustomEditor(typeof(WeaponData))]
public class WeaponDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var data = (WeaponData)target;
        DrawDefaultInspector();

        EditorGUILayout.Space();
        if (GUILayout.Button("Preview Damage"))
            Debug.Log($"Damage: {data.damage}");
    }
}
```

## Key Gotchas

1. SO values persist across play mode sessions — changes in play mode are NOT reverted. Use `DefaultValue` + `OnEnable` to reset, or the clone pattern.
2. SO references in prefabs work, but the SO must exist as a `.asset` file in the project (can't be inline).
3. `Instantiate(so)` creates a runtime copy (not saved to disk) — safe to modify without affecting the original asset.
4. SO events are synchronous — if you need async, use a coroutine inside the listener's response.
5. SO can't reference scene-specific objects (they're project assets). Use a `GameEventListener` in the scene to bridge.
6. For save systems: SO holds the "template", a separate serializable struct holds "runtime state". Don't save SO references directly.
