# Unity 2D 肉鸽游戏 - 正确开发顺序版本

> 本教程按照真实Unity开发流程编写：先场景→动画→物体→脚本→挂载→测试

---

## 执行规则（必须遵守）

1. **严格按顺序执行**：从步骤 1 到步骤 12，不跳步
2. **每步完成后验证**：执行完一步，立即检查结果
3. **遇错即停**：如果某步失败 2 次，停止并报告问题
4. **不要过度输出**：只输出"✓ 步骤X完成"或"✗ 步骤X失败：原因"

---

## 开发顺序总览

```
步骤 1：验证资源文件（图片、场景）
步骤 2：配置场景基础（Camera + Ground）✓ 可立即看到效果
步骤 3：创建玩家动画（3个 AnimationClip）
步骤 4：创建玩家动画控制器（Animator Controller）
步骤 5：创建敌人动画（3个 AnimationClip）
步骤 6：创建敌人动画控制器（Animator Controller）
步骤 7：创建玩家 Prefab（只有基础组件）✓ 可看到角色动画
步骤 8：创建敌人 Prefab（只有基础组件）✓ 可看到敌人动画
步骤 9：编写所有脚本（4个脚本文件）
步骤 10：挂载脚本到 Prefab（给物体添加行为）
步骤 11：配置游戏启动（生成玩家和敌人）
步骤 12：最终验证（完整游戏测试）
```

---

## 步骤 1：验证项目资源

### 1.1 检查序列帧资源

执行以下检查：

```
必须存在的资源：
□ Assets/Scenes/SampleScene.unity
□ Assets/Resources/Sprites/Background/bg1.png
□ Assets/Resources/Sprites/Characters/YaoXiu/Idel/ (至少14帧)
□ Assets/Resources/Sprites/Characters/YaoXiu/Run/ (至少14帧)
□ Assets/Resources/Sprites/Characters/YaoXiu/Die/ (至少14帧)
□ Assets/Resources/Sprites/Characters/Mouse/stand/ (至少14帧)
□ Assets/Resources/Sprites/Characters/Mouse/walk/ (至少14帧)
□ Assets/Resources/Sprites/Characters/Mouse/die/ (至少14帧)
```

**验证方式**：使用 read 工具检查目录

**如果缺失**：报告缺失的文件，停止执行

---

## 步骤 2：配置场景基础

> 这一步完成后，你应该能看到地面

### 2.1 打开场景

**操作**：打开 `Assets/Scenes/SampleScene.unity`

### 2.2 配置 Main Camera

**操作**：
1. 在 Hierarchy 中选中 `Main Camera`
2. 在 Inspector 中设置：
   - Transform Position: (0, 0, -10)
   - Camera Projection: Orthographic
   - Camera Size: 120
   - Camera Background: #1A1A2E (深蓝黑色)

**验证**：Camera Size 应该显示为 120

### 2.3 创建 Ground 地面

**操作**：
1. 在 Hierarchy 右键 → Create Empty
2. 命名为 `Ground`
3. 选中 Ground，点击 Add Component → Sprite Renderer
4. 设置以下属性：
   - Transform Position: (0, 0, 0)
   - Transform Scale: (120, 120, 1)
   - Sprite Renderer → Sprite: 拖入 `Assets/Resources/Sprites/Background/bg1.png`
   - Sprite Renderer → Sorting Layer: Default
   - Sprite Renderer → Order in Layer: 0

**验证**：在 Scene 视图中应该能看到地面背景

### 2.4 保存场景

**操作**：Ctrl+S 保存场景

**验证标准**：
- ✓ Scene 视图中能看到地面
- ✓ Game 视图中能看到地面
- ✓ Console 无错误

---

## 步骤 3：创建玩家动画

> 这是2D游戏的核心！必须先创建动画，再创建Prefab

### 3.1 创建 AnimationClips 文件夹

**操作**：
1. 在 Project 窗口，确认 `Assets/Resources/AnimationClips/` 文件夹存在
2. 如果不存在，创建该文件夹

### 3.2 创建 YaoXiu_Idle 动画

**操作**：
1. 在 `Assets/Resources/AnimationClips/` 文件夹右键
2. 选择 **Create → Animation**
3. 命名为 `YaoXiu_Idle`
4. 双击打开 Animation 窗口
5. 点击窗口中的 "Add Property" 按钮
6. 选择 **Sprite Renderer → Sprite**
7. 在 Project 窗口选中 `Assets/Resources/Sprites/Characters/YaoXiu/Idel/` 文件夹
8. 按住 Ctrl，选中所有 frame_0000 到 frame_0013（共14帧）
9. 将选中的帧拖入 Animation 窗口的时间轴
10. 在 Animation 窗口左上角设置：
    - Sample Rate: 12 (帧率)
    - 勾选 Loop (循环播放)
11. 保存（Ctrl+S）

**关键参数**：
- 帧数：14帧
- 帧率：12 FPS
- 循环：是
- 时长：约1.17秒

**验证**：点击 Animation 窗口的播放按钮，应该能看到动画循环播放

### 3.3 创建 YaoXiu_Run 动画

**操作**：
1. 创建新动画：`YaoXiu_Run`
2. 添加 Sprite Renderer → Sprite 属性
3. 将 `Assets/Resources/Sprites/Characters/YaoXiu/Run/` 中的所有帧拖入
4. 设置 Sample Rate: 12
5. 勾选 Loop
6. 保存

**验证**：播放动画，应该看到跑步循环

### 3.4 创建 YaoXiu_Die 动画

**操作**：
1. 创建新动画：`YaoXiu_Die`
2. 添加 Sprite Renderer → Sprite 属性
3. 将 `Assets/Resources/Sprites/Characters/YaoXiu/Die/` 中的所有帧拖入
4. 设置 Sample Rate: 12
5. **不勾选 Loop**（死亡动画只播放一次）
6. 保存

**验证**：播放动画，应该播放一次后停止

---

## 步骤 4：创建玩家动画控制器

> Animator Controller 控制动画之间的切换

### 4.1 创建 Animators 文件夹

**操作**：确认 `Assets/Resources/Animators/` 文件夹存在

### 4.2 创建 YaoXiuAnimator Controller

**操作**：
1. 在 `Assets/Resources/Animators/` 文件夹右键
2. 选择 **Create → Animator Controller**
3. 命名为 `YaoXiuAnimator`
4. 双击打开 Animator 窗口

### 4.3 添加动画状态

**操作**：
1. 将 `YaoXiu_Idle` 拖入 Animator 窗口（自动成为默认状态，显示为橙色）
2. 将 `YaoXiu_Run` 拖入 Animator 窗口
3. 将 `YaoXiu_Die` 拖入 Animator 窗口

**验证**：Animator 窗口中应该有3个状态方块

### 4.4 创建参数

**操作**：
1. 在 Animator 窗口左侧 "Parameters" 面板点击 "+"
2. 选择 **Float**，命名为 `Speed`，默认值 0
3. 再次点击 "+"，选择 **Bool**，命名为 `IsDead`，默认值 false

**验证**：Parameters 面板应该显示 Speed(Float) 和 IsDead(Bool)

### 4.5 创建状态转换

**操作**：

**转换 1：Idle → Run**
1. 右键 `YaoXiu_Idle` 状态 → Make Transition
2. 点击 `YaoXiu_Run` 状态（创建箭头）
3. 选中箭头，在 Inspector 中设置：
   - 取消勾选 "Has Exit Time"
   - Transition Duration: 0.1
   - Conditions: 点击 "+" → 选择 `Speed` → Greater → 0.1

**转换 2：Run → Idle**
1. 右键 `YaoXiu_Run` → Make Transition → 点击 `YaoXiu_Idle`
2. 选中箭头，设置：
   - 取消勾选 "Has Exit Time"
   - Transition Duration: 0.1
   - Conditions: `Speed` → Less → 0.1

**转换 3：Any State → Die**
1. 右键 `Any State` → Make Transition → 点击 `YaoXiu_Die`
2. 选中箭头，设置：
   - 取消勾选 "Has Exit Time"
   - Transition Duration: 0
   - Conditions: `IsDead` → true

### 4.6 保存

**操作**：Ctrl+S 保存

**验证标准**：
- ✓ 有3个状态（Idle、Run、Die）
- ✓ 有2个参数（Speed、IsDead）
- ✓ 有3个转换箭头

---

## 步骤 5：创建敌人动画

> 与玩家动画类似，但使用 Mouse 的序列帧

### 5.1 创建 Mouse_Stand 动画

**操作**：
1. 在 `Assets/Resources/AnimationClips/` 创建动画：`Mouse_Stand`
2. 添加 Sprite Renderer → Sprite 属性
3. 将 `Assets/Resources/Sprites/Characters/Mouse/stand/` 中的所有帧拖入
4. Sample Rate: 12，Loop: 勾选
5. 保存

### 5.2 创建 Mouse_Walk 动画

**操作**：
1. 创建动画：`Mouse_Walk`
2. 添加 Sprite 属性
3. 将 `Mouse/walk/` 中的所有帧拖入
4. Sample Rate: 12，Loop: 勾选
5. 保存

### 5.3 创建 Mouse_Die 动画

**操作**：
1. 创建动画：`Mouse_Die`
2. 添加 Sprite 属性
3. 将 `Mouse/die/` 中的所有帧拖入
4. Sample Rate: 12，Loop: **不勾选**
5. 保存

---

## 步骤 6：创建敌人动画控制器

### 6.1 创建 MouseAnimator Controller

**操作**：
1. 在 `Assets/Resources/Animators/` 创建 Animator Controller
2. 命名为 `MouseAnimator`
3. 双击打开

### 6.2 添加状态和参数

**操作**：
1. 拖入 `Mouse_Stand`（默认状态）
2. 拖入 `Mouse_Walk`
3. 拖入 `Mouse_Die`
4. 添加参数：
   - Float: `Speed`，默认值 0
   - Bool: `IsDead`，默认值 false

### 6.3 创建转换

**操作**：
1. Stand → Walk：Speed Greater 0.1
2. Walk → Stand：Speed Less 0.1
3. Any State → Die：IsDead true

### 6.4 保存

**验证**：与 YaoXiuAnimator 结构相同

---

## 步骤 7：创建玩家 Prefab

> 现在动画已经准备好，可以创建Prefab了

### 7.1 创建 YaoXiu GameObject

**操作**：
1. 在 Hierarchy 右键 → Create Empty
2. 命名为 `YaoXiu`

### 7.2 添加基础组件

**按顺序添加以下组件**：

**1) Transform**
- Position: (0, 0, 0)
- Rotation: (0, 0, 0)
- Scale: (20, 20, 1)

**2) Sprite Renderer**
- 点击 Add Component → Sprite Renderer
- Sprite: 拖入 `YaoXiu/Idel/frame_0000.png`
- Sorting Layer: Default
- Order in Layer: 1
- Color: (1, 1, 1, 1)

**3) Animator**
- 点击 Add Component → Animator
- Controller: 拖入 `YaoXiuAnimator`
- Avatar: None
- Apply Root Motion: 不勾选
- Update Mode: Normal
- Culling Mode: Always Animate

**4) Rigidbody 2D**
- 点击 Add Component → Rigidbody 2D
- Body Type: Dynamic
- Material: None
- Simulated: 勾选
- Use Auto Mass: 不勾选
- Mass: 1
- Linear Drag: 0
- Angular Drag: 0.05
- Gravity Scale: 0
- Collision Detection: Continuous
- Sleeping Mode: Never Sleep
- Interpolate: Interpolate
- Constraints: 勾选 Freeze Rotation Z

**5) Capsule Collider 2D**
- 点击 Add Component → Capsule Collider 2D
- Is Trigger: 不勾选
- Used By Effector: 不勾选
- Offset: (0, 0)
- Size: (0.5, 0.8)
- Direction: Vertical

### 7.3 设置 Tag

**操作**：
1. 在 Inspector 顶部找到 "Tag" 下拉菜单
2. 选择 **Player**
3. 如果没有 Player 选项：
   - 选择 "Add Tag..."
   - 点击 "+"
   - 输入 "Player"
   - 保存
   - 回到 YaoXiu，设置 Tag 为 Player

### 7.4 测试动画

**操作**：
1. 确保 YaoXiu 在场景中
2. 点击 Play 按钮
3. 观察 YaoXiu 是否播放 Idle 动画

**验证标准**：
- ✓ 能看到 YaoXiu 角色
- ✓ 角色播放 Idle 动画（循环）
- ✓ Console 无错误

### 7.5 保存为 Prefab

**操作**：
1. 在 Project 窗口，确认 `Assets/Resources/Prefabs/Characters/` 文件夹存在
2. 将 Hierarchy 中的 `YaoXiu` 拖到该文件夹
3. 保存为 `YaoXiu.prefab`
4. **删除** Hierarchy 中的 YaoXiu 实例（后续会通过脚本生成）

**验证**：Project 窗口中应该有 YaoXiu.prefab 文件

---

## 步骤 8：创建敌人 Prefab

### 8.1 创建 Mouse GameObject

**操作**：
1. 在 Hierarchy 创建空物体，命名 `Mouse`

### 8.2 添加基础组件

**1) Transform**
- Position: (30, 0, 0)
- Scale: (10, 10, 1)

**2) Sprite Renderer**
- Sprite: `Mouse/stand/frame_0000.png`
- Order in Layer: 1

**3) Animator**
- Controller: `MouseAnimator`

**4) Rigidbody 2D**
- Body Type: Dynamic
- Gravity Scale: 0
- Collision Detection: Continuous
- Interpolate: Interpolate
- Freeze Rotation Z: 勾选

**5) Capsule Collider 2D**
- Size: (0.4, 0.6)
- Direction: Vertical

### 8.3 测试动画

**操作**：
1. 点击 Play
2. 观察 Mouse 是否播放 Stand 动画

**验证**：Mouse 应该播放站立动画

### 8.4 保存为 Prefab

**操作**：
1. 拖到 `Assets/Resources/Prefabs/Characters/` 文件夹
2. 保存为 `Mouse.prefab`
3. **删除** Hierarchy 中的 Mouse 实例（后续会通过脚本自动生成）

**说明**：敌人会在步骤11中通过 GameStart 脚本自动生成，不需要手动放置

---

## 步骤 9：编写所有脚本

> 现在物体和动画都准备好了，可以编写脚本了

### 9.1 创建脚本文件夹

**操作**：确认以下文件夹存在，不存在则创建：
- `Assets/Scripts/Combat/Core/`
- `Assets/Scripts/Characters/Implementations/Yao
Xiu/`
- `Assets/Scripts/Characters/Implementations/Mouse/`
- `Assets/UI/StartPage/`

### 9.2 创建 CameraFollow.cs（摄像机跟随脚本）

**路径**：`Assets/Scripts/Core/CameraFollow.cs`

**代码**：
```csharp
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("跟随设置")]
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);
    
    private void LateUpdate()
    {
        if (target == null)
        {
            // 自动查找玩家
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            return;
        }
        
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }
}
```

### 9.3 创建 CombatUnit.cs

**路径**：`Assets/Scripts/Combat/Core/CombatUnit.cs`

**代码**：（完整代码见 TUTORIAL.md 步骤 2.1）

### 9.4 创建 YaoXiuCharacter.cs

**路径**：`Assets/Scripts/Characters/Implementations/YaoXiu/YaoXiuCharacter.cs`

**代码**：（完整代码见 TUTORIAL.md 步骤 2.2）

### 9.5 创建 MouseController.cs

**路径**：`Assets/Scripts/Characters/Implementations/Mouse/MouseController.cs`

**代码**：（完整代码见 TUTORIAL.md 步骤 2.3）

### 9.6 创建 GameStart.cs

**路径**：`Assets/UI/StartPage/GameStart.cs`

**代码**：
```csharp
using UnityEngine;

public class GameStart : MonoBehaviour
{
    [Header("角色 Prefab")]
    [SerializeField] private GameObject yaoxiuPrefab;
    [SerializeField] private GameObject mousePrefab;
    
    [Header("敌人生成设置")]
    [SerializeField] private int initialEnemyCount = 3;
    [SerializeField] private float spawnMinDistance = 20f;
    [SerializeField] private float spawnMaxDistance = 40f;
    
    private void Start()
    {
        // 生成玩家
        if (yaoxiuPrefab != null)
        {
            GameObject player = Instantiate(yaoxiuPrefab, Vector3.zero, Quaternion.identity);
            player.tag = "Player";
            player.name = "Player";
            Debug.Log("玩家已生成");
            
            // 生成敌人（在玩家周围远处）
            SpawnEnemiesAroundPlayer(player.transform);
        }
        else
        {
            Debug.LogError("GameStart: yaoxiuPrefab 未设置！");
        }
    }
    
    private void SpawnEnemiesAroundPlayer(Transform playerTransform)
    {
        if (mousePrefab == null)
        {
            Debug.LogWarning("GameStart: mousePrefab 未设置，跳过敌人生成");
            return;
        }
        
        for (int i = 0; i < initialEnemyCount; i++)
        {
            // 在玩家周围随机位置生成敌人
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(spawnMinDistance, spawnMaxDistance);
            
            Vector3 spawnPosition = playerTransform.position + new Vector3(
                Mathf.Cos(angle) * distance,
                Mathf.Sin(angle) * distance,
                0
            );
            
            GameObject enemy = Instantiate(mousePrefab, spawnPosition, Quaternion.identity);
            enemy.name = $"Mouse_{i + 1}";
            Debug.Log($"敌人已生成：{enemy.name} at {spawnPosition}");
        }
    }
}
```

**说明**：
- `spawnMinDistance: 20` - 敌人最少距离玩家20单位
- `spawnMaxDistance: 40` - 敌人最多距离玩家40单位
- `initialEnemyCount: 3` - 初始生成3个敌人
- 敌人会在玩家周围随机角度和距离生成

### 9.7 等待编译

**操作**：
1. 保存所有脚本
2. 等待 Unity 编译完成
3. 检查 Console 是否有错误

**验证**：编译成功，Console 无错误

---

## 步骤 10：挂载脚本和配置摄像机跟随

> 现在脚本已经编译完成，可以挂载到物体上了

### 10.1 配置摄像机跟随

**操作**：
1. 在 Hierarchy 中选中 `Main Camera`
2. 点击 Add Component
3. 搜索并添加 `CameraFollow` 脚本
4. 在 Inspector 中设置：
   - Target: 暂时留空（脚本会自动查找 Player）
   - Smooth Speed: 5
   - Offset: (0, 0, -10)

**说明**：
- 脚本会在 LateUpdate 中自动查找 Tag 为 "Player" 的对象
- 使用平滑跟随（Lerp），不会太生硬
- Offset 保持 Z=-10，确保相机在正确位置

### 10.2 给 YaoXiu.prefab 挂载脚本

**操作**：
1. 在 Project 窗口打开 `YaoXiu.prefab`
2. 点击 Add Component，添加 `CombatUnit`
3. 配置 CombatUnit：
   - unitName: "妖修"
   - isPlayer: 勾选
   - stats.maxHealth: 150
   - stats.currentHealth: 150
   - stats.moveSpeed: 25
4. 点击 Add Component，添加 `YaoXiuCharacter`
5. 配置 YaoXiuCharacter：
   - moveSpeed: 25
   - useCombatUnitSpeed: 勾选
6. 保存 Prefab

### 10.3 给 Mouse.prefab 挂载脚本

**操作**：
1. 在 Project 窗口打开 `Mouse.prefab`
2. 添加 `CombatUnit`：
   - unitName: "老鼠"
   - isPlayer: 不勾选
   - stats.maxHealth: 30
   - stats.moveSpeed: 3
   - expOnDeath: 10
3. 添加 `MouseController`：
   - moveSpeed: 3
   - stopDistance: 0.5
   - targetTag: "Player"
   - deathAnimationDuration: 0.8
4. 保存 Prefab

---

## 步骤 11：配置游戏启动

### 11.1 创建 StartPage 对象

**操作**：
1. 在 Hierarchy 创建空物体，命名 `StartPage`
2. 添加 `GameStart` 组件
3. 配置 GameStart 组件：
   - yaoxiuPrefab: 拖入 `YaoXiu.prefab`
   - mousePrefab: 拖入 `Mouse.prefab`
   - initialEnemyCount: 3（初始生成3个敌人）
   - spawnMinDistance: 20（敌人最少距离玩家20单位）
   - spawnMaxDistance: 40（敌人最多距离玩家40单位）
4. **删除** Hierarchy 中手动放置的 Mouse 实例（现在由脚本自动生成）
5. 保存场景

**说明**：
- 游戏开始时会在玩家周围随机位置生成3个敌人
- 敌人生成距离为20-40单位，不会太近也不会太远
- 可以调整 initialEnemyCount 来改变敌人数量

---

## 步骤 12：最终验证

### 12.1 运行游戏

**操作**：点击 Play 按钮

### 12.2 完整检查清单

```
□ 能看到地面
□ 能看到玩家（YaoXiu）
□ 玩家播放 Idle 动画
□ 按 WASD 移动，玩家速度适中（25单位/秒）✓ 已调整
□ 摄像机平滑跟随玩家移动
□ 停止移动，玩家切换回 Idle 动画
□ 能看到3个敌人（Mouse）
□ 敌人在玩家周围远处生成（20-40单位距离）✓ 已调整
□ Mouse 播放 Stand 动画
□ Mouse 会追击玩家
□ Mouse 追击时播放 Walk 动画
□ Mouse 碰到玩家会造成伤害
□ Console 无错误
```

### 12.3 验证结果

**如果所有检查都通过**：
- 输出"✓ 完整游戏完成！包含动画、AI、战斗和摄像机跟随。"

**如果任何检查失败**：
- 输出"✗ 验证失败：[具体问题]"

---

## 常见问题快速修复

### 问题：摄像机不跟随玩家

**检查**：
1. Main Camera 是否有 CameraFollow 组件
2. 玩家的 Tag 是否为 "Player"
3. Console 是否有脚本错误

**修复**：
```
1. 选中 Main Camera，确认有 CameraFollow 组件
2. 选中玩家，确认 Tag = "Player"
3. 如果 Target 字段为空，这是正常的（脚本会自动查找）
4. 运行游戏，移动玩家，观察摄像机是否跟随
```

### 问题：摄像机跟随太生硬或太慢

**调整参数**：
```
Smooth Speed 参数说明：
- 1-3: 慢速跟随（延迟感明显）
- 5: 标准速度（推荐）
- 10-15: 快速跟随（几乎无延迟）
- 100+: 瞬间跟随（无平滑效果）
```

---

## 执行完成标准

完成后应该达到：

1. ✓ 玩家有完整的 Idle/Run 动画
2. ✓ 敌人有完整的 Stand/Walk/Die 动画
3. ✓ 敌人会追击玩家
4. ✓ 碰撞会造成伤害
5. ✓ 敌人死亡播放死亡动画后销毁
6. ✓ 摄像机平滑跟随玩家移动 ✓ 新增

这是一个**完整可玩的2D肉鸽游戏基础版本**，包含摄像机跟随功能！
