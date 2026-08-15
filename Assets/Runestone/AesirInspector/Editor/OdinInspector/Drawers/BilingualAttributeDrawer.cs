using System;
using Sirenix.OdinInspector.Editor;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// 支持双语自动重绘的 Odin 特性绘制器基类。
    /// 自动订阅语言变更事件并触发重绘或重新加载解析器。
    /// </summary>
    public abstract class BilingualAttributeDrawer<TAttribute> : OdinAttributeDrawer<TAttribute>, IDisposable
        where TAttribute : Attribute
    {
        public void Dispose()
        {
            AesirInspectorLanguageSettingsSO.LanguageChanged -= InternalLanguageChanged;
        }

        protected override void Initialize()
        {
            AesirInspectorLanguageSettingsSO.LanguageChanged += InternalLanguageChanged;
            OnInitialize();
        }

        /// <summary>
        /// 子类初始化逻辑。
        /// </summary>
        protected virtual void OnInitialize() { }

        /// <summary>
        /// 当语言发生变化时调用。默认行为是触发 Property 的重绘。
        /// 如果需要重新加载解析器，请重写此方法。
        /// </summary>
        protected virtual void OnLanguageChanged()
        {
            Property.Tree.DelayAction(() => Property.RefreshSetup());
        }

        void InternalLanguageChanged()
        {
            OnLanguageChanged();
        }
    }
}
