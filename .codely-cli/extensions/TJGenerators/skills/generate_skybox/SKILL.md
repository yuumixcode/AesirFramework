---
name: unity-skybox-generation
description: Generate a Unity skybox through the online generator MCP, then import its color panorama as a Cubemap and sky material. Use for skies, distant environments, scene atmosphere, or reference-image skyboxes. Preserve the project's render pipeline and existing scene settings unless the user requests applying the new sky.
---

> **执行前读取**：[共享执行约定](../../experience/templates/generator-async-pattern.md)。
> 主 agent 按需委托 `general-purpose`；已在子代理中则直接执行，不再委托。已有 task ID 从对应阶段续接。

# 天空盒：线上生成，Unity 导入与应用

生成只调用 **MCP `generate_skybox`**。不调用 Unity `execute_custom_tool("generate_skybox", ...)`；该生成入口已移除。

1. 提示词描述整体风格、光照、时间、天气和远景；适合游戏背景的要求是无接缝 360° 全景、水平地平线，近处可交互物体另建几何。若用户提供本地参考图，按 MCP `file_upload` 的返回说明上传实际字节，把 CDN `file_url` 传给 `image_urls`。
2. 调用 MCP `generate_skybox(prompt=..., image_urls=...)`，按实时 schema 传参。默认省略 `provider` 或用 `auto`，由服务端选择；不要把旧 `generator_id=rodin-skybox` 传给 MCP。用户指定的 `high_res` 可传给服务端；其适用模型由实时 schema 决定。遇到费用确认交回主 agent，不能替用户确认。
3. 记录 MCP task ID，执行返回的轮询命令，再用 MCP `check_task` 取终态、产物和导入指引。报告服务端实际返回的模型，不根据请求中的 provider 猜测。
4. 选择**彩色 2:1 全景图**：优先返回的 `skybox_high`，需要小纹理时选 `skybox_basic`；Rodin 使用响应指明的彩色全景 URL。按实际返回结构取值，不猜造 URL。不要选 `depth` 图，也不要把 Flare 的 LDR 衍生 EXR 当作真实 HDR 光照。
5. 用 `execute_custom_tool` 调 **`import_image_from_url`**，参数 `image_url=<彩色PNG/JPEG URL>`、`import_type="skybox"`，按需带 `prompt` 和 `output_path`。记录新的 Unity task ID；随后前台短等待并调用 **`query_local_task(task_id=<Unity ID>)`**，直到终态。MCP ID 与 Unity ID 不可互换。
6. 导入成功后验证 `texture_path` 能加载为 Cubemap，`material_path` 能加载为 `Skybox/Cubemap` 材质且 `_Tex` 指向该 Cubemap。导入器不修改场景。用户要求场景天空时应用一次；只要资产时到此结束。

## 导入参数与结果

```python
execute_custom_tool(
    tool_name="import_image_from_url",
    parameters={
        "image_url": "<MCP check_task 返回的彩色全景 URL>",
        "import_type": "skybox",
        "prompt": "soft sunset sky, seamless 360-degree panorama",
        # output_path 可省略，默认 Assets/TJGenerators/History/ImportSkybox/<url-hash>.png
        # 显式路径可带中文，扩展名为 .png/.jpg/.jpeg；不是 .mat 路径。
    })
```

单图模式创建可引用的 Cubemap 占位与同目录 `<图片名>_material.mat`；已存在的天空盒纹理/材质在下载成功前保留。成功替换保持 GUID、已有材质参数和场景引用，失败不能把占位当结果。为避免修改其它类型资产，已有输出不是 Cubemap 或同名材质不是 Skybox/Cubemap 时会拒绝，改用新输出路径。

`query_local_task` 成功结果包含 `task_type="image_import"`、`import_type="skybox"`、`image_path`/`texture_path`、`material_path` 和状态。`list_local_tasks(task_type="image_import")` 可定位本地导入任务；MCP 生成仍使用 `check_task`。

## 场景应用

- Built-in / URP：加载 `unity-place-assets-in-scene`，按天空盒小节将 `material_path` 赋给 `RenderSettings.skybox` 并更新环境。复用原场景，不创建新场景、不自行改摄像机。已经应用占位材质时不再重复赋值。
- HDRP：Cubemap 可作为 HDRI Sky 输入；使用当前工程已有的 HDRP Volume / Visual Environment / HDRI Sky 配置。通过现有 Unity 工具检查实际组件和 API，只按用户的场景要求设置；仅改 RenderSettings 不能证明 HDRP 天空已生效。没有 HDRP 环境时报告未应用，不安装渲染管线或谎报完成。
- 场景应用后检查实际天空设置和相机视图；资产检查在 Edit Mode 完成，不为验证进入 Play Mode。报告任务 ID、实际模型、Cubemap/材质路径及是否应用到场景。

## 恢复

导入类型和材质路径随 Unity task 持久化，domain reload 后继续原导入。网络失败重试已生成 URL，不重做收费生成。只返回 depth、格式不支持或尺寸不是 2:1 时报告已有产物问题；不要直接重生成。旧 Unity 原生天空盒任务仍能通过 `query_local_task` / `list_local_tasks(task_type="skybox")` 查询恢复，但不再创建新的原生生成任务。
