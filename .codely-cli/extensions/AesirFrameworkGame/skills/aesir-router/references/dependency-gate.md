# 依赖门禁：探测判据与安装操作（展开参考）

> `../SKILL.md` 的展开参考：会话启动 hook 未注入装包结论、或需要手工复验安装状态时读本文。三步门禁的骨架与失效矩阵在 SKILL.md。

### ① 触发条件：先探测工程实际装了什么

命中任一条即算已装（不要只凭用户口述）：

- `Packages/manifest.json` 或 `Packages/packages-lock.json` 里有包 ID 或 `Aesir*-latest` 的 Git URL；
- `Packages/cn.runestone.aesir.*/package.json` 或 `Library/PackageCache/cn.runestone.aesir.*/` 存在；
- 内联安装（unitypackage 形态）：`Assets/Runestone/AesirArchitecture/`、`Assets/Runestone/AesirModules/` 存在；
- 能写出一行 `using Runestone.AesirArchitecture;` / `using Runestone.AesirModules;` 且编译通过。

| 工程实际有 | 可用 | 失效（不许写，也不许用原生 API 顶替） |
|---|---|---|
| RAA + RAM | 全部 13 个技能 | — |
| 只有 RAA | router / framework / tiers / core-api / decisions / pitfalls / boot-scene / project-structure | `aesir-ui` / `aesir-events` / `aesir-audio` / `aesir-scene` / `aesir-save` 全部失效——五个模块一个都编不过 |
| 只有 RAM | 仅 project-structure（纯目录/文档知识，不依赖包） | 其余全部——先补 RAA |
| 都没有 | 仅 project-structure（立目录骨架时第一个用它）+ 解释概念与写法 | 其余全部——先装包 |

> `aesir-project-structure` 是唯一不依赖任何包的技能（纯目录结构与文档规范），四种状态都可用——全新工程往往最先用它立骨架，再装包。

缺包时的禁止项：改用原生 `SceneManager`、自建 Canvas 栈、`AudioSource.PlayOneShot`、`static event` 总线顶替，或凭印象编造框架 API。正确做法是走 ② ③。

### ③ 询问与安装：没有明确禁止就必须问

用选择题（`ask_user`）问，问句里必须说清三件事：**装哪个包**、**从哪里装**、**装完会改什么**（写 `Packages/manifest.json` 并从网络下载）。**线路顺序是 CNB 优先、GitHub 兜底**，不要把 GitHub 当首选。

| 包 | 国内直连（首选） | 国际镜像（兜底） |
|---|---|---|
| RAA | `https://cnb.cool/runestone-game/AesirFramework.git#AesirArchitecture-latest` | `https://github.com/yuumixcode/AesirFramework.git#AesirArchitecture-latest` |
| RAM | `https://cnb.cool/runestone-game/AesirFramework.git#AesirModules-latest` | `https://github.com/yuumixcode/AesirFramework.git#AesirModules-latest` |

- 两个都缺且开发者同意 → **先 RAA 后 RAM**（RAM 编译依赖 RAA）。
- 只缺 RAA 但已装 RAM → 不要手写 manifest 依赖（RAM 的 `package.json` 不声明 RAA，UPM 不支持包内 Git 依赖），走官方菜单 `Tools/Aesir/Modules/Install Dependencies`。
- 装完等域重载结束，按 ① 的探测方法复验，再把结论写进工程记忆（"已装 RAA+RAM" / "开发者明确只要 RAA"），避免下次重复询问。
- 开发者拒绝安装 → 停留在他已有包的能力范围内，明确告知哪些需求做不了、为什么。

