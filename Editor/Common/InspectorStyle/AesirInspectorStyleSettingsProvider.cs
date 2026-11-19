using UnityEditor;
using UnityEngine.UIElements;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// Aesir 偏好设置页（<c>Preferences → Aesir</c>）—— Odin 样式切换从此页进入，不再占用 Tools 菜单。
    /// </summary>
    /// <remarks>
    /// 偏好值由 <see cref="AesirInspectorStyleSettingsSO" />（Editor 专属 ScriptableObject）持有；
    /// 本页只负责呈现与写入，样式切换的对外门面仍是 <see cref="AesirInspectorStyleSettings.UseOdin" />。
    /// </remarks>
    internal sealed class AesirInspectorStyleSettingsProvider : SettingsProvider
    {
        const string SettingsPath = "Preferences/Aesir";

        Toggle _useOdinToggle;

        AesirInspectorStyleSettingsProvider()
            : base(SettingsPath, SettingsScope.User,
                new[] { "Aesir", "Odin", "Inspector", "样式", "UI Toolkit" })
        {
        }

        [SettingsProvider]
        static SettingsProvider Create() => new AesirInspectorStyleSettingsProvider();

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            var description = new Label(
                "控制 Aesir 自己的 Odin 集成是否生效：关闭后，即使工程装了 Odin Inspector，" +
                "Aesir 的面板与属性绘制也改用 UI Toolkit 复刻版；Odin 仍可用于其它插件。");
            description.style.whiteSpace = WhiteSpace.Normal;
            description.AddToClassList("aesir-settings__description");
            rootElement.Add(description);

            _useOdinToggle = new Toggle("使用 Odin Inspector 样式")
            {
                value = AesirInspectorStyleSettings.UseOdin
            };
            _useOdinToggle.RegisterValueChangedCallback(evt => AesirInspectorStyleSettings.UseOdin = evt.newValue);
            rootElement.Add(_useOdinToggle);

            AesirInspectorStyleSettings.Changed += OnStyleChanged;
        }

        public override void OnDeactivate()
        {
            AesirInspectorStyleSettings.Changed -= OnStyleChanged;
            _useOdinToggle = null;
            base.OnDeactivate();
        }

        void OnStyleChanged()
        {
            if (_useOdinToggle != null)
            {
                _useOdinToggle.SetValueWithoutNotify(AesirInspectorStyleSettings.UseOdin);
            }
        }
    }
}
