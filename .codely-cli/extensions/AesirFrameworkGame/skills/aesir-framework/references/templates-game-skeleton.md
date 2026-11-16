# 实战模板：完整可玩小游戏骨架（竖版射击型）

> 从 `Assets/PlaneWar`（原生版）与 `Samples/Counter-Mvc-Quick` 提炼：**用框架把"能玩的一局游戏"跑通**，而不是只跑通一个计数器。
> 覆盖六件事：得分状态、失败状态、HUD 订阅刷新、节奏化生成敌人、开火冷却、失败重开。
> 默认 **Quick 档**（具体类注册 + 可写 `ObservableValue` 直接改）；需要收窄时按 `mvc-standard.md` 升档。

## 文件清单

| 文件 | 位置 | 职责 |
|---|---|---|
| `GameContext.cs` | `Assets/Game/Context/` | 注册 Model |
| `GameModel.cs` | `Assets/Game/Model/` | 得分 + 失败状态（运行时数据） |
| `GameHudPanel.cs` | `Assets/Game/View/` | HUD 面板，订阅刷新（`AesirBasePanelViewController<T>`） |
| `EnemySpawner.cs` | `Assets/Game/Core/` | 节奏化生成敌人（`AesirTimer.Every`） |
| `PlayerController.cs` | `Assets/Game/Core/` | 移动 + 开火冷却（`AesirTimer.Countdown`） |
| `GameBootstrap.cs` | `Assets/Game/Core/` | 启动引导（由 `aesir-boot-scene` 技能产出，勿手写） |

关键基类选择（易错点）：RAM 面板要么用 `AesirBasePanel`（拿不到 Model），要么用 **`AesirBasePanelViewController<TContext>`**（`AesirBasePanel` + `IController`，可 `this.GetModel<T>()`）。业务面板一律用后者。

## 1. Context

```csharp
// Assets/Game/Context/GameContext.cs
using Runestone.AesirArchitecture;

public sealed class GameContext : AbstractContext<GameContext>
{
    protected override void Configure()
    {
        RegisterModel(new GameModel());   // Quick 档：注册具体类，不建接口
    }
}
```

## 2. Model：得分与失败状态

```csharp
// Assets/Game/Model/GameModel.cs
using System;
using UnityEngine;
using Runestone.AesirArchitecture;

[Serializable]
public sealed class GameModel : AbstractModel
{
    // 需要通知 UI 的用 ObservableValue；不需要通知的（如内部标记）用普通字段
    [SerializeField] ObservableValue<int> score = new ObservableValue<int>(0);
    [SerializeField] ObservableValue<bool> isGameOver = new ObservableValue<bool>(false);

    // Quick 档：直接暴露可写实例，表现层改 .Value
    public ObservableValue<int> Score => score;
    public ObservableValue<bool> IsGameOver => isGameOver;
}
```

## 3. HUD 面板：订阅而不是每帧轮询

```csharp
// Assets/Game/View/GameHudPanel.cs
using UnityEngine;
using UnityEngine.UI;
using Runestone.AesirModules;

public class GameHudPanel : AesirBasePanelViewController<GameContext>
{
    [SerializeField] Text scoreText;
    [SerializeField] GameObject gameOverRoot;

    GameModel _model;

    // OnInit 在 UIModule 装配完成、OnShow 之前调用一次（比 Start 更早且只一次）
    protected override void OnInit()
    {
        _model = this.GetModel<GameModel>();   // 缓存为字段，勿在刷新路径反复 GetModel

        // AddListenerAndInvoke：订阅并立即拿当前值，省掉一次手动刷新
        _model.Score.AddListenerAndInvoke(OnScoreChanged)
            .RemoveListenerWhenGameObjectOnDestroyed(gameObject);
        _model.IsGameOver.AddListenerAndInvoke(OnGameOverChanged)
            .RemoveListenerWhenGameObjectOnDestroyed(gameObject);
    }

    void OnScoreChanged(int value) => scoreText.text = $"得分：{value}";
    void OnGameOverChanged(bool over) => gameOverRoot.SetActive(over);
}
```

显示方式（`path:` 必须用命名参数，否则字符串会被当成 payload 吞掉）：

```csharp
UIModule.Show<GameHudPanel>(path: "UI/GameHudPanel");   // 预制体存 Assets/Resources/UI/，根节点不带 Canvas
```

## 4. 生成器：节奏交给 `AesirTimer`，不写 `Update` 计时

```csharp
// Assets/Game/Core/EnemySpawner.cs
using UnityEngine;
using Runestone.AesirArchitecture;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject[] enemyPrefabs;
    [SerializeField] float interval = 0.8f;
    [SerializeField] float spawnHalfWidth = 2.5f;
    [SerializeField] float spawnY = 6.5f;
    [SerializeField] float stopWhenScoreReaches = 30f;   // 示例：达到分数后停刷

    GameModel _model;
    AesirTimer _spawnTimer;

    void Start()
    {
        _model = GameContext.Instance.GetModel<GameModel>();   // 非 View/Controller 的世界物体：走 Instance

        // 无限重复，间隔驱动；不依赖 MonoBehaviour 的 Update
        _spawnTimer = AesirTimer.Every(interval, Spawn);
    }

    void Spawn()
    {
        // 失败后停止膨胀（框架不吞异常，也不需要自愈轮询——只在生成时机判断一次）
        if (_model.IsGameOver.Value)
        {
            _spawnTimer.Cancel();
            return;
        }

        var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        var x = Random.Range(-spawnHalfWidth, spawnHalfWidth);
        Instantiate(prefab, new Vector3(x, spawnY, 0f), Quaternion.identity);
    }

    // 计时器是框架级（PlayerLoop）对象，不随场景销毁——必须自己取消
    void OnDestroy() => _spawnTimer?.Cancel();
}
```

要点：
- `AesirTimer` 没有 `Start()`，工厂方法即开始；`Every` 的首次回调在 `interval` 之后。
- 计时器挂在框架 PlayerLoop 上，**场景卸载不会自动回收** → `OnDestroy` 里 `Cancel()`。
- 追求瞬时手感不要用 `repeatCount` 做计数上限，直接在回调里判断业务状态。

## 5. 玩家：开火冷却用可暂停的计时器

```csharp
// Assets/Game/Core/PlayerController.cs
using UnityEngine;
using Runestone.AesirArchitecture;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 8f;
    [SerializeField] float fireCooldown = 0.15f;
    [SerializeField] Vector2 moveBounds = new Vector2(2.5f, 5.5f);
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] float muzzleOffsetY = 0.6f;

    GameModel _model;
    AesirTimer _cooldown;

    void Start()
    {
        _model = GameContext.Instance.GetModel<GameModel>();
        _cooldown = AesirTimer.Countdown(fireCooldown);   // 建好即跑完一次冷却
    }

    void Update()
    {
        if (_model.IsGameOver.Value) return;   // 失败后停手停火

        Move();
        Fire();
    }

    void Move()
    {
        // 归一化保证斜向不快于正向；输入保持 Unity 原生（框架不接管输入）
        var direction = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0f).normalized;
        var position = transform.position + direction * (moveSpeed * Time.deltaTime);
        position.x = Mathf.Clamp(position.x, -moveBounds.x, moveBounds.x);
        position.y = Mathf.Clamp(position.y, -moveBounds.y, moveBounds.y);
        transform.position = position;
    }

    void Fire()
    {
        if (!Input.GetKey(KeyCode.Space) || !_cooldown.IsFinished) return;

        Instantiate(bulletPrefab, transform.position + Vector3.up * muzzleOffsetY, Quaternion.identity);
        _cooldown.Restart();   // 复用同一实例，不换引用
    }
}
```

## 6. 得分、失败与重开

```csharp
// 敌机命中时（敌人脚本内，世界物体走 Instance 取 Model）
GameContext.Instance.GetModel<GameModel>().Score.Value += 10;

// 玩家与敌机碰撞：只置位，不在这里切场景
GameContext.Instance.GetModel<GameModel>().IsGameOver.Value = true;
```

```csharp
// 重开（HUD 面板里的按钮或 Update 里的按键都行）
GameContext.Instance.Dispose();          // Context 是纯 C# 单例，不随场景销毁 → 必须先释放
SceneModule.ReloadScene();               // 不要用原生 SceneManager；混用会让模块记账失真
```

顺序不能反：先 `Dispose()` 再重载。只重载不 Dispose 时，旧 Model 里的得分与失败标记会原样留在新一局里——这是"重开数据没清"的唯一根因。

## 自检清单（跑一遍）

- [ ] 场景里只有 `GameBootstrap` 物体 + `AudioListener`（无 Main Camera），Build Settings index 0 指向 Boot 场景。
- [ ] 启动后 HUD 出现且得分显示 `0`（说明 `AddListenerAndInvoke` 生效，而非等第一次变化才刷新）。
- [ ] 连续击杀时得分文本变化，**HUD 里没有任何 `Update`**。
- [ ] 失败后敌人停止生成、玩家无法移动开火，HUD 出现结束提示。
- [ ] 按重开键 → 得分归零、失败提示消失（验证 `Dispose()` + `ReloadScene()` 顺序）。
- [ ] Console 无 `GetModel` 未注册异常、无 `2 audio listeners` 警告。

## 何时升档

| 触发条件 | 升到 | 改什么 |
|---|---|---|
| 写入逻辑 > 30 行 / 多面板要同一操作 | Standard | Model 收窄为只读接口 + 写方法，View 与 Controller 分开 |
| 需要运行期替换 Model 实现 / 回放操作 | Strict | Model 按接口注册，写入走 Command，加工读取走 Query |

看到 `mvc-standard.md` / `mvc-strict.md`。**升档是加法**：Model 字段与面板订阅代码不用重写。
