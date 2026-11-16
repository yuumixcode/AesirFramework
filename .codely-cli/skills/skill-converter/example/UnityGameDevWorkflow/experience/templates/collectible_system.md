# Unity 收集物系统

提供完整的收集物系统：悬浮旋转动画 + 收集粒子效果 + Trigger 碰撞检测。

## 适用场景

- 金币 / 宝石 / 星星收集
- 道具拾取（血瓶、弹药等）
- 经验球 / 能量球
- 任何需要"碰到即收集 + 视觉反馈"的游戏

## 输出文件

| 文件 | 路径 | 说明 |
|------|------|------|
| Collectible.cs | Assets/Scripts/Collectibles/ | 悬浮旋转动画 |
| CollectEffect.cs | Assets/Scripts/Visuals/ | 粒子爆发效果 |

另需在玩家脚本的 `OnTriggerEnter` 中添加碰撞检测代码片段。

## 代码模板

### Collectible.cs — 悬浮旋转动画

```csharp
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public float rotateSpeed = 180f;
    public float bobSpeed = 2f;
    public float bobHeight = 0.3f;

    Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
        Vector3 pos = startPos;
        pos.y += Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = pos;
    }
}
```

### CollectEffect.cs — 粒子爆发效果

```csharp
using UnityEngine;

public class CollectEffect : MonoBehaviour
{
    ParticleSystem ps;

    void Awake()
    {
        var go = new GameObject("CollectParticles");
        go.transform.SetParent(transform, false);
        ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.startLifetime = 0.5f;
        main.startSpeed = 3f;
        main.startSize = 0.15f;
        main.startColor = new Color(1f, 0.85f, 0.2f); // 金色，可按需修改
        main.maxParticles = 20;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 12) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.color = main.startColor.color;
    }

    public void Play()
    {
        if (ps != null) ps.Play();
    }
}
```

### 碰撞检测代码片段（加入玩家脚本）

```csharp
// 在玩家脚本中添加：
void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Coin")) // Tag 名称按需修改
    {
        // 调用游戏管理器的收集方法
        GameManager.Instance.CollectCoin();

        // 播放粒子效果
        var effect = GetComponent<CollectEffect>();
        if (effect != null) effect.Play();

        // 隐藏收集物
        other.gameObject.SetActive(false);
    }
}
```

## 集成说明

1. 使用 `unity_script` 创建两个脚本
2. 使用 `unity_editor` 确保 Tag 存在（如 "Coin"）
3. 收集物 GameObject 需要：
   - `MeshFilter` + `MeshRenderer`（外观）
   - `SphereCollider`（isTrigger = true, radius ≈ 0.5）
   - `Collectible` 组件
   - Tag 设为 "Coin"（或自定义）
4. 玩家 GameObject 需要：
   - `CollectEffect` 组件（自动创建粒子系统）
   - `OnTriggerEnter` 碰撞检测代码
   - `CharacterController` 或 `Rigidbody` + `Collider`

## 参数调优

| 参数 | 金币 | 血瓶 | 经验球 |
|------|------|------|--------|
| rotateSpeed | 180 | 90 | 360 |
| bobSpeed | 2 | 1.5 | 3 |
| bobHeight | 0.3 | 0.2 | 0.15 |
| 粒子颜色 | 金色 (1, 0.85, 0.2) | 红色 (1, 0.3, 0.3) | 蓝色 (0.3, 0.6, 1) |
| 粒子数量 | 12 | 8 | 20 |

## 已知陷阱

| 陷阱 | 后果 | 解决 |
|------|------|------|
| Collider 没设 isTrigger | OnTriggerEnter 不触发 | `sc.isTrigger = true` |
| 忘记设 Tag | CompareTag 永远 false | 确保 Tag 已创建并赋值 |
| 用 Destroy 而非 SetActive(false) | 对象池无法复用 | 用 `SetActive(false)` |
| 粒子材质用了 Standard shader | 粒子显示为方块 | 用 `Particles/Standard Unlit` |
| bobHeight 在 Update 中用 transform.position 累加 | 收集物越飘越远 | 记录 startPos，基于 startPos 偏移 |