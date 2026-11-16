---
name: unity-editor-scripting
description: Unity Editor scripting expert — EditorWindow, CustomInspector, PropertyDrawer, MenuItem, SerializedObject, GUILayout, AssetPostprocessor. Use when creating custom editor tools, batch operation utilities, custom Inspector displays, editor automation scripts, or extending the Unity Editor with new functionality.
---

# Unity Editor Scripting

## MenuItem

```csharp
public class MyTools
{
    [MenuItem("Tools/Batch Rename %#r")] // Ctrl+Shift+R
    static void BatchRename()
    {
        var selected = Selection.gameObjects;
        for (int i = 0; i < selected.Length; i++)
            selected[i].name = $"Enemy_{i:D3}";
    }

    // Validation — grey out when no selection
    [MenuItem("Tools/Batch Rename", true)]
    static bool ValidateBatchRename() => Selection.gameObjects.Length > 0;

    // Context menu (right-click in Inspector)
    [MenuItem("CONTEXT/Transform/Snap To Grid")]
    static void SnapToGrid(MenuCommand command)
    {
        var t = (Transform)command.context;
        t.position = new Vector3(
            Mathf.Round(t.position.x),
            Mathf.Round(t.position.y),
            Mathf.Round(t.position.z));
    }
}
```

**Shortcut syntax:** `%`=Ctrl/Cmd, `#`=Shift, `&`=Alt, `_`=no modifier (e.g., `_F1`).

## EditorWindow

```csharp
public class DataGeneratorWindow : EditorWindow
{
    [MenuItem("Tools/Data Generator")]
    static void Open() => GetWindow<DataGeneratorWindow>("Data Generator");

    int enemyCount = 10;
    string prefix = "Enemy";

    void OnGUI()
    {
        enemyCount = EditorGUILayout.IntField("Count", enemyCount);
        prefix = EditorGUILayout.TextField("Prefix", prefix);

        if (GUILayout.Button("Generate"))
        {
            for (int i = 0; i < enemyCount; i++)
            {
                var go = new GameObject($"{prefix}_{i:D3}");
                Undo.RegisterCreatedObjectUndo(go, "Generate Enemies");
            }
        }
    }
}
```

## Custom Inspector

```csharp
[CustomEditor(typeof(EnemyStats))]
public class EnemyStatsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var stats = (EnemyStats)target;

        // Draw default fields first
        DrawDefaultInspector();

        // Custom UI
        EditorGUILayout.Space();
        if (GUILayout.Button("Reset Health"))
        {
            Undo.RecordObject(stats, "Reset Health");
            stats.CurrentHealth = stats.MaxHealth;
            EditorUtility.SetDirty(stats);
        }

        // Color-coded health bar
        var rect = GUILayoutUtility.GetRect(18, 18);
        EditorGUI.ProgressBar(rect, stats.HealthRatio, $"HP: {stats.CurrentHealth}/{stats.MaxHealth}");
    }
}
```

## PropertyDrawer

```csharp
[CustomPropertyDrawer(typeof(RangeAttribute))]
public class RangeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
    {
        var range = (RangeAttribute)attribute;
        EditorGUI.Slider(pos, prop, range.Min, range.Max, label);
    }

    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        => EditorGUI.GetPropertyHeight(prop, label); // Default height
}
```

## SerializedObject Pattern

```csharp
// Preferred pattern — supports Undo, multi-edit, prefab overrides
public class ConfigEditor : Editor
{
    SerializedProperty _maxHealth;
    SerializedProperty _damage;

    void OnEnable()
    {
        _maxHealth = serializedObject.FindProperty("maxHealth");
        _damage = serializedObject.FindProperty("damage");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(_maxHealth);
        EditorGUILayout.PropertyField(_damage);
        serializedObject.ApplyModifiedProperties();
    }
}
```

## AssetPostprocessor

```csharp
// Auto-process imported assets
public class TextureImporter : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (assetPath.Contains("UI/"))
        {
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 100;
        }
    }

    // After all imports
    static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
    {
        foreach (var path in imported)
            Debug.Log($"Imported: {path}");
    }
}
```

## ScriptableWizard

```csharp
public class SpawnWizard : ScriptableWizard
{
    public GameObject prefab;
    public int count = 10;
    public float radius = 5f;

    [MenuItem("Tools/Spawn Wizard")]
    static void CreateWizard() => DisplayWizard<SpawnWizard>("Spawn Objects", "Spawn");

    void OnWizardCreate()
    {
        for (int i = 0; i < count; i++)
        {
            var pos = Random.insideUnitSphere * radius;
            var go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            go.transform.position = pos;
            Undo.RegisterCreatedObjectUndo(go, "Spawn");
        }
    }
}
```

## Key Gotchas

1. Editor scripts go in `Editor/` folder — they're stripped from builds.
2. Always use `Undo.RecordObject` before modifying — otherwise changes aren't undoable.
3. `EditorUtility.SetDirty(target)` after modifying — without it, changes aren't saved.
4. `serializedObject` pattern > direct `target` field access — supports multi-edit and prefabs.
5. `GUILayout` (automatic layout) for custom windows. `EditorGUILayout` for inspector-like controls.
6. `Selection.gameObjects` for batch operations — always validate count > 0.
7. `[InitializeOnLoad]` on a static class runs its static constructor on every domain reload (compilation).
