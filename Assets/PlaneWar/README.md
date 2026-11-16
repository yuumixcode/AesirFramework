# PlaneWar · 飞机大战

用 AesirFramework 仓库开发的**完整可玩小游戏**，也是框架的对照实验台：同一套玩法需求分别以原生 `MonoBehaviour` 与 Aesir Architecture（RAA）两种写法实现，用于对照"同一款游戏在两种组织方式下的差异"。

## 与包内示例的区别

| 项 | 包内 `Samples/` 示例 | 本案例 |
|---|---|---|
| 归属 | `Assets/Runestone/<包>/Samples/`，随包分发 | `Assets/PlaneWar/`，宿主工程私有 |
| 分发 | 进 unitypackage、`package.json` 的 samples 清单、Getting Started 面板 | 都不进，仅本仓库可见 |
| 构建 | 脚本整文件 `#if UNITY_EDITOR` 包裹 + 场景构建剔除 | 普通游戏程序集，正常编译进玩家 |
| 定位 | 单个 API 的最小用法 | 一款能从头玩到尾的游戏 |

## 目录结构

```
Assets/PlaneWar/
├── Art/                    # 精灵素材（自包含拷贝，出处见文末）
├── Materials/              # 材质预留位（当前精灵渲染使用 Unity 内置 Sprites-Default）
├── Prefabs/                # 玩家 / 三种敌机 / 子弹
├── Scenes/
│   └── PlaneWar.unity      # 主场景：打开即玩
├── Scripts/
│   └── Mono/               # 原生 MonoBehaviour 实现（程序集 PlaneWar.Mono）
└── README.md
```

`Scripts/` 按实现方式分子目录，每个子目录一个程序集：`Mono/`（已包含）与 `Raa/`（规划中，见下方路线）。

## 运行

1. 打开 `Assets/PlaneWar/Scenes/PlaneWar.unity`
2. 按 Play（主场景未登记在本机 Build Settings，需要出包时自行加入）

## 操作

- 方向键 / WASD —— 上下左右移动
- Space（按住连发）—— 发射子弹
- 失败后按 Space —— 重开当前场景

## 玩法规则

- 玩家位于画面下方朝上射击，敌机自上方往下飞，飞出底部即消失
- 击毁敌机得分（A 型 10 分 / B 型 20 分 / C 型 30 分）
- 玩家飞机与敌机碰撞即坠毁，游戏结束（HUD 左上角显示"游戏结束"，按 Space 重开）
- HUD 左上角显示得分与系统当前时间，字号随窗口宽度自适应

## Mono 版实现要点

| 文件 | 职责 | 原生写法的特征 |
|---|---|---|
| `PlaneWarGameManager.cs` | 得分与失败状态、重开场景 | 静态 `Instance` 单例 + 场景预放置；状态挂在组件上 |
| `PlaneWarPlayer.cs` | 读输入、移动、开火 | 一个组件同时负责输入 / 表现 / 实例化 |
| `PlaneWarBullet.cs` | 子弹推进与命中 | `Update` 推进 + `OnTriggerEnter2D` 判定 |
| `PlaneWarEnemy.cs` | 敌机移动、碰撞、计分 | 三型敌机共用脚本，差异全在预制体字段 |
| `PlaneWarEnemySpawner.cs` | 刷怪节奏 | `Update` 里递减本地计时器 |
| `PlaneWarGameHUD.cs` | 得分 / 时间 / 结束提示 | 每帧轮询 GameManager 并重建字符串 |

## RAA 版路线

`Scripts/Raa/`（独立程序集 `PlaneWar.Raa`）计划以 Quick 档组织同一套玩法：得分属于纯 C# 的 Model，组件退化为无状态表现层，计时与刷怪节奏改用 `AesirTimer`，HUD 订阅 `ObservableValue<int>` 而非每帧轮询。当前尚未包含，列出只为说明本案例的双实现结构。

## 素材出处

Sprites from [Vertical 2D Shooting BE4](https://www.goldmetal.co.kr) — Copyright © 2021 Goldmetal. 标注 Goldmetal 出处即可自由使用（含商用）。
