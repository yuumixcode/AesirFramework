# 实战模板：原生 MonoBehaviour 写法 → Aesir 写法迁移表

> 原料是 `Assets/PlaneWar/Scripts/Mono/`（同一玩法的原生版，6 个文件 469 行）与 `Samples/Counter-*` 六档。原生版不是"错误示范"，而是**迁移前的基线**：逐项对照就知道哪些该换、哪些该留。
> 迁移原则：**只换"数据归属 + 刷新路径 + 时间与场景调度"，输入、碰撞、生成保留 Unity 原生。**

## 一页对照表

| # | 原生写法（PlaneWar Mono 实际代码） | Aesir 写法 | 换的理由 |
|---|---|---|---|
| 1 | `public static XxxManager Instance { get; private set; }` + `Awake` 赋值，场景必须预放置 | `GameContext` 注册 `GameModel`，任意处 `GameContext.Instance.GetModel<GameModel>()` | 数据与场景解耦：场景可以全空，重开不靠重新实例化场景物体 |
| 2 | HUD 在 `Update` 里每帧读 `Instance.Score` 重建字符串 | `ObservableValue<int>` + `AddListenerAndInvoke` 订阅 | 每帧字符串拼接是纯浪费；订阅只在变化时跑 |
| 3 | `Update` 里 `_timer -= Time.deltaTime; if (_timer <= 0) Spawn();` | `AesirTimer.Every(interval, Spawn)`（`OnDestroy` 里 `Cancel`） | 计时逻辑离开表现层；纯 C# 的 Model/Service 也能用 |
| 4 | 开火冷却 `_fireTimer += Time.deltaTime` 手写累加 | `AesirTimer.Countdown(fireCooldown)` + `IsFinished` / `Restart()` | 自带 `Progress`（做冷却环 UI 不用另算），可 `Pause()` |
| 5 | `SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex)` | `GameContext.Instance.Dispose()` → `SceneModule.ReloadScene()` | 模块统一记账；且纯 C# Context 不随场景销毁，必须显式 Dispose |
| 6 | `AudioSource.PlayOneShot(clip)` | `AudioModule.PlaySfx(clip, volume, pitch, pitchJitter)` | N 源轮询池化，逐音调/音量独立，不用每次 Instantiate |
| 7 | `public static event Action<int> OnScore;` 全局广播 | RAM `EventModule`（`[AesirListener]` 或 Lambda 句柄） | 带优先级、订阅者过滤器、死引用自动清理 |
| 8 | `Input.GetAxisRaw` / `OnTriggerEnter2D` / `Instantiate(prefab)` | **保持不变** | 框架不接管输入、碰撞与物体生命周期；包一层只会更难查 |

## 迁移前后：状态归属

```csharp
// 原生：状态住在场景里的一个组件上
public class PlaneWarGameManager : MonoBehaviour
{
    public static PlaneWarGameManager Instance { get; private set; }
    int _score; bool _isGameOver;          // 非序列化字段，防止 Play 残留
    public int Score => _score;
    public bool IsGameOver => _isGameOver;
    void Awake() { Instance = this; _score = 0; _isGameOver = false; }
    public void AddScore(int v) => _score += v;
    public void SetGameOver() => _isGameOver = true;
}
```

```csharp
// Aesir：状态是纯 C# Model，组件只做表现
[Serializable]
public sealed class GameModel : AbstractModel
{
    [SerializeField] ObservableValue<int> score = new ObservableValue<int>(0);
    [SerializeField] ObservableValue<bool> isGameOver = new ObservableValue<bool>(false);
    public ObservableValue<int> Score => score;
    public ObservableValue<bool> IsGameOver => isGameOver;
}
```

> 原版 `GameManager` 整个类在迁移后**消失**：它的两个职责被拆给 Model（数据）与 HUD 面板（表现）。看到"Map 里找不到 GameManager 了"不是漏迁。

## 迁移前后：刷新路径

```csharp
// 原生：HUD 每帧轮询，还把"游戏结束"文案与得分混在同一个分支里
void Update()
{
    if (PlaneWarGameManager.Instance.IsGameOver)
    {
        scoreText.text = "游戏结束";
        timeText.text = $"得分: {PlaneWarGameManager.Instance.Score}";
    }
    else
    {
        scoreText.text = $"得分: {PlaneWarGameManager.Instance.Score}";
        timeText.text = DateTime.Now.ToString("HH:mm:ss");
    }
}
```

```csharp
// Aesir：订阅驱动，变化才跑
protected override void OnInit()
{
    _model = this.GetModel<GameModel>();
    _model.Score.AddListenerAndInvoke(OnScoreChanged)
        .RemoveListenerWhenGameObjectOnDestroyed(gameObject);
    _model.IsGameOver.AddListenerAndInvoke(OnGameOverChanged)
        .RemoveListenerWhenGameObjectOnDestroyed(gameObject);
}
void OnScoreChanged(int v) => scoreText.text = $"得分：{v}";
void OnGameOverChanged(bool over) => gameOverRoot.SetActive(over);
```

例外要保留：**时钟这类"每帧都必须变"的内容**（原版的 `DateTime.Now`）应由驱动方自己 `Update` 刷新——它不是状态变化通知，`ObservableValue` 不该承担它。

## 迁移步骤（顺序不可打乱）

1. **先立 Context + Model**，把状态字段从组件搬进 Model（先不改任何 UI）。此时游戏应当还能跑，只是数据换了住处。
2. **改刷新路径**：HUD 从轮询改成订阅，删掉 `Update`。这一步最容易暴露"某个字段其实没人写"的盲点。
3. **再换调度**：`Update` 计时 → `AesirTimer`；`SceneManager` → `SceneModule`；`PlayOneShot` → `AudioModule`。
4. **最后拆文件**：Controller / Presenter 独立出来（Standard 档起），或维持 Quick 档不拆。
5. **收尾自检**：见下。

**不要一次全迁**。做法是"数据先走 Model，表现后走订阅"，每步都能跑起来再进下一步。

## 常见过迁（不该换的硬换）

| 过度迁移 | 后果 | 正确做法 |
|---|---|---|
| 给每个敌机实例建一个 Model | 注册表膨胀、生命周期混乱 | 敌机是**瞬态对象**，字段留在自己的组件上；只有"一局游戏的状态"进 Model |
| 把 `Transform` / `GameObject` 塞进 Model | Model 依赖 UnityEngine 组件，无法纯 C# 测试 | Model 只放数据；位置/朝向留在 Mono 层 |
| 用 `ObservableValue` 包每帧都会变的量 | 每帧触发通知，比轮询还慢 | 每帧量由 `Update` 直接读 |
| 用 `EventModule` 替代同一组件内的状态刷新 | 多一层分发，还得管理句柄 | 同组件内状态 → `ObservableValue`；跨系统广播才用 `EventModule` |
| 自建对象池 | 框架边界明确不做池化 | 直接 `Instantiate` / `Destroy`；真需要性能再引上游方案 |

## 收尾自检

- [ ] `grep -rn "static.*Instance" Assets/_Game/Scripts` 只剩框架类型，业务里没有自建单例。
- [ ] `grep -rn "SceneManager\." Assets/_Game/Scripts` 无结果（全部走 `SceneModule`）。
- [ ] `grep -rn "Update()" Assets/_Game/Scripts` 命中的只剩"每帧必然要变"的驱动（输入、时钟、朝向相机），没有状态轮询。
- [ ] 每个 `AddListener` 都有对应移除路径（`RemoveListenerWhenGameObjectOnDestroyed` 或 `OnDisable` 对称移除）。
- [ ] 重开一局时数据归零（即 `Dispose()` 在 `ReloadScene()` 之前）。

## 附：中文文本的字体坑（迁移时最常踩）

原生版 HUD 用 `Font.GetOSInstalledFontNames()` 探测候选系统字体，再 `Font.CreateDynamicFontFromOSFont` 动态生成（`Microsoft YaHei` / `PingFang SC` / `Noto Sans CJK SC` …），这是**示例里为了零资产依赖的权宜写法**，不适合正式项目。

正式项目按 UI 技术栈选：

| 技术栈 | 做法 |
|---|---|
| UGUI `Text` | 用打包进项目的字体资产，别在运行时探测系统字体（构建到用户机器上可能一个都不命中，回退成无中文字形） |
| TextMeshPro | 需要中文字体资产 + 相应字符集（否则表现为方块 / 空白 / 缺字）；先确认 TMP Essential Resources 已导入 |

## 何时继续升档

迁移完成后仍是 Quick 档。出现以下任一情况再看 `mvc-standard.md`：写入逻辑超过 30 行、多个面板要执行同一操作、需要运行期替换 Model 实现。
