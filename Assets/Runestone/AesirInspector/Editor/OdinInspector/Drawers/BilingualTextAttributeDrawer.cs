using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// 双语文本特性的 Drawer。
    /// </summary>
    [DrawerPriority(DrawerPriorityLevel.SuperPriority)]
    public class BilingualTextAttributeDrawer : BilingualAttributeDrawer<BilingualTextAttribute>
    {
        ValueResolver<Color> _iconColorResolver;
        GUIContent _tempLabel;
        ValueResolver<string> _textProvider;

        protected override void OnInitialize()
        {
            _textProvider = ValueResolver.GetForString(Property, GetAttributeText());
            _iconColorResolver =
                ValueResolver.Get(Property, Attribute.IconColor, EditorStyles.label.normal.textColor);
            _tempLabel = new GUIContent();
        }

        protected override void OnLanguageChanged()
        {
            _textProvider = ValueResolver.GetForString(Property, GetAttributeText());
            base.OnLanguageChanged();
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (Property.GetAttribute<HideLabelAttribute>() != null)
            {
                CallNextDrawer(null);
            }

            if (_textProvider.HasError)
            {
                SirenixEditorGUI.ErrorMessageBox(_textProvider.ErrorMessage);
                CallNextDrawer(label);
            }
            else if (_iconColorResolver.HasError)
            {
                SirenixEditorGUI.ErrorMessageBox(_iconColorResolver.ErrorMessage);
                CallNextDrawer(label);
            }
            else
            {
                var str = _textProvider.GetValue();
                GUIContent nextLabel;
                if (str == null && Attribute.Icon == SdfIconType.None)
                {
                    nextLabel = label;
                }
                else
                {
                    var name = str ?? label.text;
                    if (Attribute.NicifyEnglishText)
                    {
                        name = ObjectNames.NicifyVariableName(name);
                    }

                    _tempLabel.text = name;
                    nextLabel = _tempLabel;
                    if (Attribute.Icon != SdfIconType.None)
                    {
                        var color = _iconColorResolver.GetValue();
                        nextLabel.image = SdfIcons.CreateTransparentIconTexture(Attribute.Icon, color,
                            24, 24, 0);
                    }
                }

                CallNextDrawer(nextLabel);
            }
        }

        string GetAttributeText() => Attribute.BilingualData.GetCurrentOrFallback();
    }
}
