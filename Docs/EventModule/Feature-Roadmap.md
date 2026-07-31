# Event Module 待完成功能清单

> 基于 V1-V8 迭代计划与当前代码实际状态整理。仅列出可添加的功能，不含实现方案。

---

## 当前已完成

| 版本 | 功能 | 状态 |
|------|------|------|
| V1 | Attribute 订阅（`[AesirListener]` + `AddListener(obj)`） | ✅ |
| V1 | 事件触发（`InvokeEvent` / `eventArgs.Invoke(sender)`） | ✅ |
| V1 | 退订（`RemoveListener(obj)`） | ✅ |
| V1 | 单例懒加载（`EventModule.Instance`） | ✅ |
| V1 | 异常隔离（单订阅者异常不影响其他） | ✅ |
| V2 | 5 档优先级排序（Essential/High/Medium/Low/Cleanup） | ✅ |
| V2 | Script 订阅（`AddListener<T>(obj, callback)` 动态重载） | ✅ |
| V2 | 双轨共存（Attribute + Script 混存于同一 BindingRegistry） | ✅ |
| V2 | 退订委托（`AddListener<T>` 返回 `Action unsub`） | ✅ |
| V2 | `[AesirListener]` 优先级构造重载 | ✅ |

---

## 已移除（原 V2 计划中存在，因无实际使用场景砍掉）

| 功能 | 移除原因 |
|------|---------|
| StopPropagation / 取消传播 | 扁平 pub/sub 无冒泡层级，DOM 事件模型不适用 |
| Seal / 事件密封 | 防御性编程，但用户无感知，增加复杂度 |
| Shared / Unique 模式 | 需要共享可变状态通过事件传递的场景极少 |
| CopyEvent | 依赖上述机制，一并移除 |
| InvokeDelayed / 延迟触发 | 需要时可用协程自行包装 |
| PropagationResult / 分阶段分发 | 依赖取消传播机制，简化为 OrderBy 排序 |
| IsInitialized | Instance 已自动创建，此属性多余 |

> 以上功能遇到实际需求时可重新添加。

---

## 待完成功能

### 过滤器（精确投递）

- **ISubscriberFilter 接口** — 策略接口，发布者可声明"只让特定范围的订阅者收到"
- **WithTag** — 按 Unity Tag 过滤订阅者
- **WithPriority** — 按优先级档位过滤
- **SameSceneAsEmitter** — 只投递给与发布者同场景的订阅者
- **InsideCollider2D** — 只投递给碰撞体范围内的订阅者
- **OnlySelf** — 只投递给发布者自身/子树/父级链
- **WithFilter(s) 链式 API** — 在 AesirEventArgs 上链式添加过滤器
- **DefaultChannel 特性** — 类级特性，为事件标记语义化频道标签

### 系统事件（元事件）

- **OnEventSystemStarted** — 模块启动时触发
- **OnObjectBound** — 订阅者绑定时触发
- **OnObjectUnbound** — 订阅者解绑时触发
- **OnEventRaised** — 每次事件分发时触发，携带发布者与订阅者信息
- **SystemEventAttribute** — 标记系统事件，工具可过滤

### 可靠性保障

- **死引用清理** — 分发前自动检测并移除已销毁的 Unity 对象订阅者
- **性能监控** — 分发耗时超阈值告警（ExecutionMsLimit + Stopwatch）
- **单实例合并** — 自动检测并合并多余的 EventModule 实例

### SO 资产化集成

- **AesirEventArgsSO** — ScriptableObject 包装，事件可作为 .asset 资源保存
- **SubclassSelector** — UI Toolkit PropertyDrawer，让 `[SerializeReference]` 字段在 Inspector 显示子类下拉
- **ExcludeSubclassSelector** — 排除特性，标记不想出现在下拉中的类型
- **UnityEventOnAesirEvent** — 桥接组件，非程序员在 Inspector 用 UnityEvent 串联事件回调
- **Project 右键菜单** — 快速创建 AesirEventArgsSO 资产

### Animator 集成

- **PublishOnPlayback** — StateMachineBehaviour，在动画状态机的特定归一化时间点自动发布事件
- **反射注入** — 自动将当前 Animator 注入到事件参数中类型为 Animator 的字段
- **Editor 动画预览** — 在 Inspector 拖动时间轴实时预览动画姿态

### 编辑器工具链

- **Subscription Monitor** — 窗口，实时显示所有事件类型及其订阅者数量
- **Event Log** — 窗口，事件活动历史，支持按类型/频道过滤
- **Event Tester** — 窗口，手动触发事件测试
- **Event Detail** — 窗口，显示事件完整元数据
- **Event Actors** — 窗口，显示发布者与订阅者列表，支持 Ping 定位
- **EventModule 自定义 Inspector** — 显示活跃计数 + 工具入口按钮
- **EditorPrefs 持久化** — 工具过滤器配置跨 Editor 重启保留

### 文档与引导

- **Welcome 窗口** — 首次安装自动弹出引导
- **内嵌文档系统** — Editor 内 Markdown 渲染，按章节组织
- **Hierarchy 右键菜单** — 快速创建 EventModule
- **Troubleshooting 文档** — 常见故障排查指南

---

## 功能优先级建议

按实用性排序：

1. **死引用清理** — 不做会导致 Destroy 后 Publish 报错（当前靠 try/catch 兜底但不清理）
2. **过滤器** — 局域广播在游戏中有实际需求（如爆炸范围内实体响应）
3. **SO 资产化** — 让非程序员在 Inspector 配置事件
4. **编辑器工具** — 调试可视化，开发期效率提升
5. **系统事件** — 工具的数据源，依赖编辑器工具才有价值
6. **Animator 集成** — 特定场景需求，优先级低
7. **文档系统** — 商业化收尾，非核心功能
