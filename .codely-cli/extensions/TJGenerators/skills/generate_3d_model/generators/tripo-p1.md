# Tripo（MCP provider）

provider: `tripo`  
适用场景：低面数小游戏 / 移动端 / 稳定面数批量 / **多视图（4 视角）图生3D** / P2 新一代低模 / v3.1 高模  
输出：**FBX**（`with_fbx` 默认 true）；产物通常带 `rendered_image`，导入时作为主贴图

> 生成提交 / 轮询纪律 / 成本确认见 [mcp.md](mcp.md)；本文件只列 **tripo 独有参数**。

## 何时选择 Tripo

- 明确要求「低面 / 低模 / 移动端 / 包体限制」（P1 面数 48–20000）
- 批量生成需要稳定面数与体积
- 需要**多视图**生成（rodin 不支持）
- 用户点名 Tripo P2 / 新一代 P 系列 / v3.1 高模

## Tripo 独有参数（`generate_3d_model`）

| 参数 | 类型 | 默认 | 说明 |
|------|------|------|------|
| `model_version` | string | `P1-20260311` | `P1-20260311`（默认，低模优化）/ `P2-20260801`（新一代 P 系列：四边面输出 + 更优 UV，**订阅专属**，面数不足时选用）/ `v3.1-20260211`（H3.1 超高精度，仅用户明确要求高模或质量重试时用） |
| `face_limit` | number | 不发送 | P1 低面控制 48–20000 |
| `quad` | bool | false | 四边面网格输出（P2/v3.1 档位；P1 不支持） |
| `style` | string | — | 风格预设（非 P1 档位） |
| `with_segmentation` | bool | false | 网格语义分割（把模型拆为多个子网格；prompt 里说明要拆分时才开） |
| `negative_prompt` | string | — | 反向提示词 |
| `pbr` | bool | true | PBR 材质 |
| `export_uv` | bool | true | UV 展开（关闭可提速减体积） |

### ⚠️ P1-20260311 约束

`model_version=P1-20260311` 时，**不要传**以下参数（仅 P2 / v3.1 支持，传了会被拒）：
`quad`、`style`、`negative_prompt`。

## 输入模式

| 模式 | 参数 | 适用 |
|------|------|------|
| 文生3D | `prompt` | 文字描述 |
| 图生3D | `image_url`（先 `file_upload`） | 参考图，可叠加 `prompt` |
| 多视图 | `image_urls` = `"<front>,<left>,<back>,<right>"` | 4 视角（front 0° 必须第一，之后 left 90° / back 180° / right 270°）；每张图先 `file_upload` 拿 CDN URL；front 必须有，最少 3 张 |

## 示例

```
# 文生低模
generate_3d_model(mode="text_to_model", provider="tripo",
                  model_version="P1-20260311",
                  prompt="wooden crate, low poly game asset", face_limit=5000)

# 多视图（4 角色设定图 → 3D）
file_upload("Assets/Refs/char_front.png")   # → file_url_front
file_upload("Assets/Refs/char_left.png")    # → file_url_left
file_upload("Assets/Refs/char_back.png")    # → file_url_back
file_upload("Assets/Refs/char_right.png")   # → file_url_right
generate_3d_model(mode="multiview_to_model", provider="tripo",
                  image_urls="<front>,<left>,<back>,<right>")

# P2 高质量低模（订阅专属；非订阅会收到订阅提示，回退 P1）
generate_3d_model(mode="text_to_model", provider="tripo",
                  model_version="P2-20260801",
                  prompt="hero shield, low poly", quad=True)
```

## 导入衔接

`check_task` 拿到 fbx URL 后：`import_3d_model_from_url(provider="tripo", ...)`，
并把 `rendered_image` URL 一并传给 `rendered_image_url`（作为主贴图）。
多模型互不混目录（`.fbm` 提取目录按模型隔离，已由导入工具处理）。
