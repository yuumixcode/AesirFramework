# `IModel`

## 介绍

- 种类: `interface`
- 所在程序集: `Runestone.AesirArchitecture`
- 所在命名空间: `Runestone.AesirArchitecture`

``` csharp
public interface IModel : Runestone.AesirArchitecture.IContextHolder, 
Runestone.AesirArchitecture.ICanSetContext, 
Runestone.AesirArchitecture.ICanInitialize, 
Runestone.AesirArchitecture.ICanGetModel, 
System.IDisposable
```

### 注释

- 数据层接口。持有状态（通常使用 ObservableValue{T}）。 能力：GetModel, Initialize, Dispose

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Aesir Inspector`](https://github.com/yuumixcode/Unity-Aesir-Packages) 辅助生成。
