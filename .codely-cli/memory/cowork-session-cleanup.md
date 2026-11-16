# Cowork 会话批量清理实录（2026-11-07，本机实测）

## 数据落点（macOS）

| 路径 | 内容 |
|---|---|
| `~/.codely/sessions/<workspaceUUID>/sessions.sqlite` | 每工作区一个库；**根目录那份 `~/.codely/sessions/sessions.sqlite` 是空的**，别只看它 |
| `~/.codely-cli/tmp/<sha256(工作区绝对路径)>/` | 工作区运行时目录（`chats/` 正文、`tasks/` 子任务、`file-history/` 编辑快照、`tool-call-artifacts/` 工具产物、`plans/`、`tool-outputs/`） |

- 工作区 UUID ≠ sha256：UUID 目录名靠 `sessions.sqlite` 里的 `workspaceDirectory` 反查。
- `session_metadata` 关键列：`sessionId / title / messageCount / dateUpdated / state`，`state ∈ active | archived | pinned`。
- 除 `chats/` 与 `plans/`（`<sessionId>.jsonl` / `<sessionId>.md`）外，`tasks/`、`file-history/`、`tool-call-artifacts/` 也是**按会话 ID 命名的顶层目录**，可与 DB 里的 ID 集合做差集找出孤儿。

## 官方删除通道（无 CLI 子命令，但有 IPC）

`cowork --help` 只有 workspace/session list/new/select，没有 delete；删除按钮走本地 IPC：

```
POST http://127.0.0.1:31415/api/tauri/invoke
X-Window-Label: hub-main
Content-Type: application/json

{"messageType":"history/delete","messageId":"任意","data":{"id":"<sessionId>"},
 "workspaceRef":{"runOn":"local","workspaceDir":"<绝对路径>"},
 "workspaceKey":"local:%2F...（工作区路径 URL 编码）"}
```

- `workspaceRef` / `workspaceKey` 取自 `cowork workspace list` 输出。
- 相关命令：`history/list`（data 空，返回含 state 的全部会话）、`history/delete`（data `{id}`）、`history/clear`（清空整个工作区）、`history/updateState`（data `{sessionId, state}`，即归档/置顶）。
- 进程与端口：`lsof -nP -iTCP -sTCP:LISTEN -a -p <cowork pid>` → 127.0.0.1:31415。

## 关键坑

1. **`history/delete` 只删记录，不删磁盘**。实测：返回 `status: success`、`sessions.sqlite` 行消失、侧栏列表少一条，但 `chats/<id>.jsonl`、`tasks/<id>/` 原样保留（仍为旧 mtime）。要真正回收空间必须自己清这些孤儿目录。
2. 清理顺序：**先走 IPC 删记录 → 再删磁盘孤儿**（反过来会先丢可用的侧栏入口，且记录在、正文没，变成"无正文"会话）。
3. 删磁盘前务必先备份（本次 50 个会话正文+子任务 304 MB，全量复制后逐文件比对大小）。
4. `~/.Trash` 在本机终端无 TCC 权限（`ls: Operation not permitted`），想让文件进废纸篓只能走 Finder/osascript；有备份时直接 `rm` 更省事。
5. 当前正在运行的会话也在 `history/list` 里（按 `dateUpdated` 最新 + `chats/*.jsonl` mtime 判断），批量删除时要显式排除，否则本次对话正文会被中途删掉。
6. 只属于已删会话的派生目录（`file-history/`、`tool-call-artifacts/`）同样按"UUID 目录名 ∉ DB 会话集合"做孤儿判定；`plans/` 用 `<sessionId>.md` 命名。
