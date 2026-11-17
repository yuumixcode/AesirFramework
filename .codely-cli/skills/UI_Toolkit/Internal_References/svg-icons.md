# SVG Icon Generation (Unity 6.3+)

## When to Use SVG

**Prefer SVG for:**
- Simple icons (arrows, chevrons, checkmarks)
- Geometric shapes
- UI symbols (close, menu, settings)
- Any icon that can be drawn with paths

**Use image generators for:**
- Complex illustrations
- Detailed artwork

## Priority Order

1. **Reuse existing project icons** — always search first
2. **Generate SVG** — fast, resolution-independent, low cost
3. **Image generators** — last resort

## SVG Format

Unity imports SVG as VectorImage assets. Use standard SVG markup:

```svg
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24">
  <path d="..." stroke="currentColor" stroke-width="2" fill="none"/>
</svg>
```

## Common Icon Examples

### Arrow Right
```svg
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24">
  <path d="M8 4l8 8-8 8" stroke="currentColor" stroke-width="2" fill="none" stroke-linecap="round" stroke-linejoin="round"/>
</svg>
```

### Checkmark
```svg
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24">
  <path d="M4 12l6 6L20 6" stroke="currentColor" stroke-width="2" fill="none" stroke-linecap="round" stroke-linejoin="round"/>
</svg>
```

### Close (X)
```svg
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24">
  <path d="M6 6l12 12M18 6L6 18" stroke="currentColor" stroke-width="2" fill="none" stroke-linecap="round"/>
</svg>
```

## Usage in Unity

1. Save SVG file to project (e.g., `Assets/UI/Icons/arrow-right.svg`)
2. Unity auto-imports as VectorImage
3. Set "Generated Asset Type" to "UI Toolkit Vector Image" in Inspector
4. Reference in USS:

```uss
.icon-arrow {
  background-image: url("project://database/Assets/UI/Icons/arrow-right.svg");
  width: 24px;
  height: 24px;
}
```
