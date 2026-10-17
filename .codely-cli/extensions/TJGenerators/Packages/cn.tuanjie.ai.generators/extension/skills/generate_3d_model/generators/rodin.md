# Rodin Gen-2.5（MCP provider）

provider: `rodin`（`generate_3d_model` 的默认值）  
适用场景：默认通用 / 快速出模 / 轻量道具 / PBR / 高精度 hero 资产（调高 tier）  
输出：**FBX**（`with_fbx` 默认 true），贴图内嵌，导入后由 `import_3d_model_from_url` 走 rodin 预设后处理（-90° 朝向修正 + auto-fit）

> 生成提交 / 轮询纪律 / 成本确认见 [mcp.md](mcp.md)；本文件只列 **rodin 独有参数**。

## 何时选择 Rodin

- 用户没明确指定 provider → **rodin**（MCP 默认）
- 要 PBR 材质 / 贴图内嵌 FBX
- 需要精确控制面数预算（`face_limit` 500–2000000）
- 高精度 hero 资产 → 显式 `tier=Gen-2.5-Extreme-High`

## Rodin 独有参数（`generate_3d_model`）

| 参数 | 类型 | 默认 | 说明 |
|------|------|------|------|
| `tier` | string | `Gen-2.5-Extreme-Low` | 生成档位（见下表）。⚠️ MCP 默认是**最快**的 Extreme-Low；要高精度必须显式调高 |
| `texture_mode` | string | 服务端默认 | 贴图质量：`legacy` / `extreme-low` / `low` / `medium` / `high` |
| `geometry_instruct_mode` | string | `faithful` | `faithful`（贴近输入）/ `creative`（更自由发挥） |
| `hd_texture` | bool | false | 贴图增强后处理（可能降低与输入的相似度） |
| `texture_delight` | bool | false | 从贴图中去除光照信息 |
| `face_limit` | number | 不发送 | 目标面数上限 **500–2000000**（等价旧 quality_override）。低档 tier 不传时后端自动封顶：Extreme-Low→20000、Low→60000 |
| `pbr` | bool | true | 是否生成 PBR 材质 |
| `export_uv` | bool | true | 是否导出 UV |

### tier 选项

| 值 | 描述 |
|----|------|
| `Gen-2.5-Extreme-Low` | 超低（**默认**，最快，适合快速预览 / 轻量道具） |
| `Gen-2.5-Low` | 低（快，简单物体） |
| `Gen-2.5-Medium` | 中（平衡） |
| `Gen-2.5-High` | 高（精细） |
| `Gen-2.5-Extreme-High` | 极高（hero 资产 / 最高细节，必须显式指定） |

### 面数控制速查

- 低面（小游戏/移动端）：`tier=Gen-2.5-Low` 或 `face_limit=8000` 等显式数值
- 精确预算：直接传 `face_limit`（优先于 tier 预设）
- 高精度高面：`tier=Gen-2.5-Extreme-High`（不传 `face_limit` 即不封顶）

## ⚠️ 与旧 Unity 工具的差异

旧 `generate_3d_model_by_rodin` 的以下参数在 MCP 上**暂无对应**，不要传：
`material`（PBR/Shaded）、`mesh_mode`（Quad/Raw）、`ta_pose`、`geometry_format`。
（`ta_pose` 对从零带动画有帮助；后端如后续补齐会同步到本文档。）

## 输入模式

| 模式 | 参数 | 适用 |
|------|------|------|
| 文生3D | `prompt` | 文字描述 |
| 图生3D | `image_url`（先 `file_upload`） | 参考图 |
| 文+图 | `prompt` + `image_url` | 带文字指导的图生 |
| 多视图 | ❌ **rodin 不支持**（仅 tripo） | — |

## 示例

```
# 轻量默认
generate_3d_model(mode="text_to_model", provider="rodin",
                  prompt="wooden barrel, medieval style")

# 高精度 hero 武器
generate_3d_model(mode="text_to_model", provider="rodin",
                  prompt="ornate golden sword with gem-encrusted hilt",
                  tier="Gen-2.5-Extreme-High", pbr=True)

# 精确面数预算
generate_3d_model(mode="image_to_model", provider="rodin",
                  image_url="<file_url>", face_limit=6000)
```

## 导入衔接

`check_task` 拿到 fbx URL 后：`import_3d_model_from_url(provider="rodin", ...)`。
rodin FBX 为 cm 文件单位，导入工具的 auto-fit 会按包围盒归一到 ~1m，**不要**手改 importer scale。
