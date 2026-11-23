---
title: IComponentBinder
description: "Runestone.AesirModules.IComponentBinder 的 API 文档"
---

# `IComponentBinder`

!!! note ""

    - **种类:** `interface`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

## 声明

``` csharp
public interface IComponentBinder
```

绑定引用接口。由 BinderAssistant 生成的脚本实现，用于自动绑定场景中的组件引用。
生成脚本在「绑定字段（自动生成）」region 之后实现 BindComponents 方法， 内部按配置的层级路径 transform.Find 并 GetComponent 赋值字段； 同一实现挂有 [ContextMenu("绑定引用")]，可在 Inspector 右键手动触发。 脚本编译后 BinderAssistant 会自动挂载组件并调用一次此方法完成首次绑定。

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`BindComponents()`](#method-bindcomponents) | 绑定引用。查找并赋值场景中的组件引用。 |

</div>

### BindComponents() {#method-bindcomponents}

绑定引用。查找并赋值场景中的组件引用。

``` csharp
public abstract void BindComponents()
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
