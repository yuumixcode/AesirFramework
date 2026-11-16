# Unity 第三人称相机跟随

提供通用的第三人称相机跟随脚本，适用于所有需要相机跟随玩家的 3D 游戏。

## 适用场景

- 3D 跑酷 / 无尽奔跑
- 第三人称动作 / 冒险
- 赛车（后方视角）
- 任何需要相机平滑跟随目标的 3D 游戏

## 输入参数

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| target | Transform | — | 跟随目标（通常是玩家） |
| offset | Vector3 | (0, 5, -8) | 相机相对目标的偏移 |
| smoothSpeed | float | 8 | Lerp 插值速度，越大越紧跟 |
| lookAheadDistance | float | 5 | 注视点在目标前方的距离 |

## 输出

一个 C# 脚本文件：`CameraFollow.cs`

## 代码模板

```csharp
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 5f, -8f);
    public float smoothSpeed = 8f;
    public float lookAheadDistance = 5f;

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.LookAt(target.position + Vector3.forward * lookAheadDistance);
    }
}
```

## 集成说明

1. 使用 `unity_script` 创建脚本到指定路径（如 `Assets/Scripts/Camera/`）
2. 在场景构建时（GameSceneBuilder 或 execute_csharp_script）：
   - 创建 Camera GameObject，挂载 `Camera` + `AudioListener` + `CameraFollow`
   - 设置 `target` 为玩家 Transform
   - 根据游戏类型调整 `offset`（俯视角加大 Y，近距离减小 Z 绝对值）
3. 初始位置设为 `target.position + offset`

## 参数调优建议

| 游戏类型 | offset 建议 | smoothSpeed | lookAheadDistance |
|---------|------------|-------------|-------------------|
| 跑酷 | (0, 6, -9) | 8 | 6 |
| 动作冒险 | (0, 4, -6) | 5 | 3 |
| 赛车 | (0, 3, -10) | 10 | 15 |
| 俯视角 | (0, 15, -5) | 6 | 0 |

## 已知陷阱

| 陷阱 | 后果 | 解决 |
|------|------|------|
| 在 `Update` 而非 `LateUpdate` 中跟随 | 相机抖动 | **必须用 `LateUpdate`** |
| `smoothSpeed` 直接用于 Lerp 的 t 参数 | 帧率依赖 | 乘以 `Time.deltaTime` |
| 忘记 null 检查 target | NullReferenceException | 开头 `if (target == null) return;` |