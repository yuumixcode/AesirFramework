namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 自定义生命周期接口集合。实现这些接口的类可通过
    /// <see cref="MonoLifecycleProxy.Register(object)" /> 自动注册到对应的生命周期事件。
    /// </summary>
    /// <remarks>
    /// 每个接口对应一个 <see cref="MonoLifecycleEvent" />，方法名以 <c>OnCustom</c> 前缀区分 Unity 原生回调。
    /// <para>
    /// <see cref="MonoLifecycleProxy.RegisterAuto(object)" /> 会扫描目标对象实现的所有
    /// ICustomXXX 接口，将对应方法注册到匹配的事件中，并在对象销毁时自动取消订阅。
    /// </para>
    /// </remarks>
    /// <seealso cref="MonoLifecycleProxy" />
    /// <seealso cref="MonoLifecycleEvent" />
    /// <summary>
    /// 自定义 FixedUpdate 生命周期。对应 <see cref="MonoLifecycleEvent.FixedUpdate" />。
    /// </summary>
    public interface ICustomFixedUpdate
    {
        /// <summary>
        /// 在 FixedUpdate 阶段执行的自定义逻辑
        /// </summary>
        void OnCustomFixedUpdate();
    }

    /// <summary>
    /// 自定义 BeforeUpdate 生命周期。对应 <see cref="MonoLifecycleEvent.BeforeUpdate" />。
    /// </summary>
    public interface ICustomBeforeUpdate
    {
        /// <summary>
        /// 在 BeforeUpdate 阶段执行的自定义逻辑（PlayerLoop 注入，在 Update 之前）
        /// </summary>
        void OnCustomBeforeUpdate();
    }

    /// <summary>
    /// 自定义 Update 生命周期。对应 <see cref="MonoLifecycleEvent.Update" />。
    /// </summary>
    public interface ICustomUpdate
    {
        /// <summary>
        /// 在 Update 阶段执行的自定义逻辑
        /// </summary>
        void OnCustomUpdate();
    }

    /// <summary>
    /// 自定义 LateUpdate 生命周期。对应 <see cref="MonoLifecycleEvent.LateUpdate" />。
    /// </summary>
    public interface ICustomLateUpdate
    {
        /// <summary>
        /// 在 LateUpdate 阶段执行的自定义逻辑
        /// </summary>
        void OnCustomLateUpdate();
    }

    /// <summary>
    /// 自定义 AfterUpdate 生命周期。对应 <see cref="MonoLifecycleEvent.AfterUpdate" />。
    /// </summary>
    public interface ICustomAfterUpdate
    {
        /// <summary>
        /// 在 AfterUpdate 阶段执行的自定义逻辑（PlayerLoop 注入，在 PostLateUpdate 之后）
        /// </summary>
        void OnCustomAfterUpdate();
    }

    /// <summary>
    /// 自定义 OnApplicationFocus 生命周期。对应 <see cref="MonoLifecycleEvent.OnApplicationFocus" />。
    /// </summary>
    public interface ICustomOnApplicationFocus
    {
        /// <summary>
        /// 在应用获得或失去焦点时执行的自定义逻辑
        /// </summary>
        void OnCustomApplicationFocus();
    }

    /// <summary>
    /// 自定义 OnApplicationPause 生命周期。对应 <see cref="MonoLifecycleEvent.OnApplicationPause" />。
    /// </summary>
    public interface ICustomOnApplicationPause
    {
        /// <summary>
        /// 在应用被系统暂停或恢复时执行的自定义逻辑
        /// </summary>
        void OnCustomApplicationPause();
    }

    /// <summary>
    /// 自定义 OnApplicationQuit 生命周期。对应 <see cref="MonoLifecycleEvent.OnApplicationQuit" />。
    /// </summary>
    public interface ICustomOnApplicationQuit
    {
        /// <summary>
        /// 在应用退出时执行的自定义逻辑
        /// </summary>
        void OnCustomApplicationQuit();
    }
}
