using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// 双语按钮特性的 Drawer。
    /// </summary>
    [DrawerPriority(DrawerPriorityLevel.WrapperPriority)]
    public class BilingualButtonAttributeDrawer : OdinAttributeDrawer<BilingualButtonAttribute>
    {
        ButtonAttribute _buttonAttribute;
        ValueResolver<string> _chineseGetter;
        ValueResolver<string> _englishGetter;

        protected override void Initialize()
        {
            _buttonAttribute = Property.GetAttribute<ButtonAttribute>();
            _chineseGetter = ValueResolver.GetForString(Property, Attribute.ChineseName);
            _englishGetter = ValueResolver.GetForString(Property, Attribute.EnglishName);
            _buttonAttribute.Name =
                $"@{nameof(AesirInspectorLanguageSettingsSO)}.{nameof(AesirInspectorLanguageSettingsSO.CurrentIsChinese)} ? \"{_chineseGetter.GetValue()}\" : \"{_englishGetter.GetValue()}\"";
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            CallNextDrawer(label);
        }
    }
}
