# `IView`

## 介绍

- 种类: `interface`
- 所在程序集: `Runestone.AesirArchitecture`
- 所在命名空间: `Runestone.AesirArchitecture`

``` csharp
public interface IView : Runestone.AesirArchitecture.IContextHolder, 
Runestone.AesirArchitecture.ICanGetModel, 
Runestone.AesirArchitecture.ICanGetService
```

### 注释

- 表现层接口。View 层通过此接口与模块上下文交互。 能力：GetModel, GetService View 可读取 Model 和 Service，但不能执行 Command 或修改 Model 状态。

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Aesir Inspector`](https://github.com/yuumixcode/Unity-Aesir-Packages) 辅助生成。
