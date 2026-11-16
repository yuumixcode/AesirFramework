# Unity 游戏 UI 面板系统

提供完整的游戏 UI 系统：Canvas 搭建 + 面板切换 + 事件驱动更新 + 文本闪烁效果。

## 适用场景

- 所有需要 Menu → HUD → GameOver 流程的游戏
- 需要分数/距离/等级等 HUD 显示的游戏
- 需要重新开始按钮的游戏

## 输出文件

| 文件 | 路径 | 说明 |
|------|------|------|
| GameUI.cs | Assets/Scripts/UI/ | 面板切换 + 事件订阅 + HUD 更新 |
| StartPromptBlink.cs | Assets/Scripts/UI/ | 文本 alpha 脉冲闪烁 |

另需在 GameSceneBuilder（或 execute_csharp_script）中添加 BuildUI 代码片段。

## 代码模板

### GameUI.cs — 面板管理器

```csharp
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject menuPanel, hudPanel, gameOverPanel;

    [Header("HUD Texts")]
    public Text scoreText, distanceText, levelText, coinText;

    [Header("Game Over Texts")]
    public Text finalScoreText, finalDistanceText;

    [Header("Buttons")]
    public Button restartButton;

    [Header("Menu Texts")]
    public Text titleText, startPromptText;

    void Start()
    {
        // 【关键】按钮事件必须在运行时 Start 中绑定，Editor 中绑定不会序列化！
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStart += OnGameStart;
            GameManager.Instance.OnGameOver += OnGameOver;
            GameManager.Instance.OnCoinCollected += OnCoinCollected;
            GameManager.Instance.OnLevelChanged += OnLevelChanged;
        }

        ShowMenu();
    }

    void OnDestroy()
    {
        // 【关键】必须取消订阅，防止空引用
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStart -= OnGameStart;
            GameManager.Instance.OnGameOver -= OnGameOver;
            GameManager.Instance.OnCoinCollected -= OnCoinCollected;
            GameManager.Instance.OnLevelChanged -= OnLevelChanged;
        }
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;
        if (scoreText != null) scoreText.text = $"SCORE: {GameManager.Instance.Score}";
        if (distanceText != null) distanceText.text = $"{Mathf.FloorToInt(GameManager.Instance.Distance)}m";
    }

    void ShowMenu()
    {
        SetPanel(menu: true, hud: false, gameOver: false);
    }

    void OnGameStart()
    {
        SetPanel(menu: false, hud: true, gameOver: false);
    }

    void OnGameOver()
    {
        SetPanel(menu: false, hud: true, gameOver: true);
        if (finalScoreText != null) finalScoreText.text = $"SCORE: {GameManager.Instance.Score}";
        if (finalDistanceText != null) finalDistanceText.text = $"DISTANCE: {Mathf.FloorToInt(GameManager.Instance.Distance)}m";
    }

    void OnCoinCollected(int total)
    {
        if (coinText != null) coinText.text = total.ToString();
    }

    void OnLevelChanged(int level)
    {
        if (levelText != null) levelText.text = $"LEVEL {level}";
    }

    void OnRestartClicked()
    {
        if (GameManager.Instance != null) GameManager.Instance.RestartGame();
    }

    void SetPanel(bool menu, bool hud, bool gameOver)
    {
        if (menuPanel != null) menuPanel.SetActive(menu);
        if (hudPanel != null) hudPanel.SetActive(hud);
        if (gameOverPanel != null) gameOverPanel.SetActive(gameOver);
    }
}
```

### StartPromptBlink.cs — 文本闪烁

```csharp
using UnityEngine;
using UnityEngine.UI;

public class StartPromptBlink : MonoBehaviour
{
    public float blinkSpeed = 2f;

    Text text;

    void Start()
    {
        text = GetComponent<Text>();
    }

    void Update()
    {
        if (text == null) return;
        float alpha = Mathf.Lerp(0.3f, 1f, (Mathf.Sin(Time.unscaledTime * blinkSpeed) + 1f) / 2f);
        Color c = text.color;
        c.a = alpha;
        text.color = c;
    }
}
```

### BuildUI 代码片段（用于 GameSceneBuilder 或 execute_csharp_script）

```csharp
static void BuildUI(Material accentMat)
{
    // ---- Canvas ----
    var canvasObj = new GameObject("Canvas");
    var canvas = canvasObj.AddComponent<Canvas>();
    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    canvas.sortingOrder = 100;
    var scaler = canvasObj.AddComponent<CanvasScaler>();
    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
    scaler.referenceResolution = new Vector2(1920, 1080);
    scaler.matchWidthOrHeight = 0.5f;
    canvasObj.AddComponent<GraphicRaycaster>();

    // 【关键】必须创建 EventSystem，否则 UI 点击全部静默失效！
    var es = new GameObject("EventSystem");
    es.AddComponent<UnityEngine.EventSystems.EventSystem>();
    es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

    // ---- 面板 ----
    var menuPanel = CreateFullscreenPanel(canvasObj.transform, "MenuPanel");
    var hudPanel = CreateFullscreenPanel(canvasObj.transform, "HUDPanel");
    var gameOverPanel = CreateFullscreenPanel(canvasObj.transform, "GameOverPanel");

    // ... 在面板中创建 Text、Button 等 UI 元素 ...
    // ... 最后挂 GameUI 组件并连接所有引用 ...

    var gameUI = canvasObj.AddComponent<GameUI>();
    gameUI.menuPanel = menuPanel;
    gameUI.hudPanel = hudPanel;
    gameUI.gameOverPanel = gameOverPanel;
    // gameUI.scoreText = ...;
    // gameUI.restartButton = ...;
}

static GameObject CreateFullscreenPanel(Transform parent, string name)
{
    var panel = new GameObject(name);
    panel.transform.SetParent(parent, false);
    var rt = panel.AddComponent<RectTransform>();
    rt.anchorMin = Vector2.zero;
    rt.anchorMax = Vector2.one;
    rt.offsetMin = rt.offsetMax = Vector2.zero;
    return panel;
}
```

## 集成说明

1. 使用 `unity_script` 创建 `GameUI.cs` 和 `StartPromptBlink.cs`
2. 在场景构建代码中调用 `BuildUI()` 创建完整 UI 层级
3. GameUI 的所有 public 字段必须在场景构建时赋值（不能留 null）
4. **所有 UI 文本必须用英文**（内置字体不支持 CJK）

## 已知陷阱

| 陷阱 | 后果 | 解决 |
|------|------|------|
| 没创建 EventSystem | UI 点击全部静默失效（无报错！） | **必须创建 EventSystem + StandaloneInputModule** |
| 在 Editor 脚本中 `onClick.AddListener` | 监听器不会序列化，Play 时按钮无响应 | **在 GameUI.Start() 中绑定** |
| OnDestroy 中不取消订阅事件 | 场景重载后空引用 | **OnDestroy 中 -= 所有事件** |
| UI 文本写中文 | 显示为 □□□ 或空白 | **所有文本用英文** |
| 字体只用 LegacyRuntime.ttf | 部分 Unity 版本找不到 | fallback 到 Arial.ttf |
| 用了 TextMeshPro 但没装包 | 编译报错 | 用 `UnityEngine.UI.Text`，或先安装 TMP 包 |