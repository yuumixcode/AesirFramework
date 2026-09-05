# PlaneWar 飞机大战示例

用同一套玩法需求分别以两种方式实现，用于对比学习：

- `Scripts/Mono` —— 原生 MonoBehaviour 写法
- `Scripts/Raa` —— Aesir Architecture（RAA）架构写法（后续补充）

## 场景

`Scene/SampleForPlaneWarMono.unity`

## 操作

- 方向键 / WASD —— 上下左右移动
- Space（按住连发）—— 发射子弹
- 失败后按 Space —— 重开当前场景

## 玩法规则

- 玩家位于画面下方朝上射击，敌机自上方往下飞，飞出底部即消失
- 击毁敌机得分（A 型 10 分 / B 型 20 分 / C 型 30 分）
- 玩家飞机与敌机碰撞即坠毁，游戏结束（HUD 左上角显示"游戏结束"，按 Space 重开）
- HUD 左上角显示得分与系统当前时间，字号随窗口宽度自适应

## 素材出处

Sprites from [Vertical 2D Shooting BE4](https://www.goldmetal.co.kr) — Copyright © 2021 Goldmetal.
