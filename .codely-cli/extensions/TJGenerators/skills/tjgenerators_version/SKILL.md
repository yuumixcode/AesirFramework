---
name: unity-tjgenerators-version
description: Look up the TJGenerators (cn.tuanjie.ai.generators) package version that Unity is actually compiling. Use when asked for TJGenerators / 团结AI资产生成 version — e.g. "TJGenerators 版本是多少", "当前包版本", "what version of TJGenerators". Do NOT read Assets/TJGenerators/ (History output) or .codely-cli/extensions/TJGenerators/ (Codely skill bundle, may be a different version).
---

用 `execute_csharp_script` 查 Unity 实际加载的包版本：

```csharp
#if UNITY_EDITOR
using TJGenerators.Utils;
string version = GenerationRequestOrigin.GetPackageVersion();
string resolved = PathUtils.TryGetTjGeneratorsPackageRoot() ?? "";
return $"version={version}\nresolvedPath={resolved}";
#endif
```

- 把 `version` 告诉用户，附 `resolvedPath` 说明读的是哪份磁盘目录。
- 返回空 → 报 `(unknown)`；不要再去 `.codely-cli/extensions/TJGenerators/` 查。
