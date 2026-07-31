using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 能力扩展方法集合
    /// </summary>
    public static class CapabilityExtensions
    {
        /// <summary>
        /// 获取已注册的 Model。若未注册则抛出包含调用者和目标类型信息的异常。
        /// 若已注册但尚未初始化，则抛出注册顺序错误或循环依赖异常。
        /// </summary>
        public static T GetModel<T>(this ICanGetModel self) where T : class, IModel
        {
            var model = self.Context.GetModel<T>();
            if (model == null)
            {
                throw new InvalidOperationException(
                    $"{AesirArchitectureDebug.ErrorTag} [{self.GetType().Name}] 尝试获取 Model [{typeof(T).Name}]，" +
                    $"但该 Model 未在 Context 中注册，需要提前调用 RegisterModel<{typeof(T).Name}>() 注册到 Context 中。");
            }

            if (!model.Initialized)
            {
                throw new InvalidOperationException(
                    $"{AesirArchitectureDebug.ErrorTag} [{self.GetType().Name}] 尝试获取 Model [{typeof(T).Name}]，" +
                    "但该 Model 尚未初始化。这通常表示注册顺序错误或存在循环依赖——" +
                    $"被依赖的 Model 应先注册。请检查 Configure() 中 RegisterModel<{typeof(T).Name}>() 的调用顺序。");
            }

            return model;
        }

        /// <summary>
        /// 获取已注册的 Service。若未注册则抛出包含调用者和目标类型信息的异常。
        /// 若已注册但尚未初始化，则抛出注册顺序错误或循环依赖异常。
        /// </summary>
        public static T GetService<T>(this ICanGetService self) where T : class, IService
        {
            var service = self.Context.GetService<T>();
            if (service == null)
            {
                throw new InvalidOperationException(
                    $"{AesirArchitectureDebug.ErrorTag} [{self.GetType().Name}] 尝试获取 Service [{typeof(T).Name}]，" +
                    $"但该 Service 未在 Context 中注册，需要提前调用 RegisterService<{typeof(T).Name}>() 注册到 Context 中。");
            }

            if (!service.Initialized)
            {
                throw new InvalidOperationException(
                    $"{AesirArchitectureDebug.ErrorTag} [{self.GetType().Name}] 尝试获取 Service [{typeof(T).Name}]，" +
                    "但该 Service 尚未初始化。这通常表示注册顺序错误或存在循环依赖——" +
                    $"被依赖的 Service 应先注册。请检查 Configure() 中 RegisterService<{typeof(T).Name}>() 的调用顺序。");
            }

            return service;
        }

        /// <summary>
        /// 执行带参命令
        /// </summary>
        public static void ExecuteCommand<T>(this ICanExecuteCommand self, T command) where T : ICommand
        {
            command.SetContext(self.Context);
            command.Execute();
        }

        /// <summary>
        /// 执行无参命令
        /// </summary>
        public static void ExecuteCommand<T>(this ICanExecuteCommand self) where T : ICommand, new()
        {
            var command = new T();
            command.SetContext(self.Context);
            command.Execute();
        }

        /// <summary>
        /// 执行带参查询
        /// </summary>
        public static TResult ExecuteQuery<TResult>(this ICanExecuteQuery self, IQuery<TResult> query)
        {
            query.SetContext(self.Context);
            return query.Execute();
        }

        /// <summary>
        /// 执行无参查询
        /// </summary>
        public static TResult ExecuteQuery<TQuery, TResult>(this ICanExecuteQuery self)
            where TQuery : IQuery<TResult>, new()
        {
            var query = new TQuery();
            query.SetContext(self.Context);
            return query.Execute();
        }
    }
}
