# `IQuery<TResult>`

## 介绍

- 种类: `interface`
- 所在程序集: `Runestone.AesirArchitecture`
- 所在命名空间: `Runestone.AesirArchitecture`

``` csharp
public interface IQuery<TResult> : Runestone.AesirArchitecture.IContextHolder, 
Runestone.AesirArchitecture.ICanSetContext, 
Runestone.AesirArchitecture.ICanGetModel, 
Runestone.AesirArchitecture.ICanGetService, 
Runestone.AesirArchitecture.ICanExecuteQuery 
```

### 注释

- 查询接口。通过 Query 执行读操作并返回结果，无副作用。 与 ICommand 的区别：Command 负责写操作（无返回值），Query 负责读操作（返回 ）。 能力：GetModel, GetService, ExecuteQuery

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Aesir Inspector`](https://github.com/yuumixcode/Unity-Aesir-Packages) 辅助生成。
