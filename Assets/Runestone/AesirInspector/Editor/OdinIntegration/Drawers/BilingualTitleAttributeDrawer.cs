using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// 在 Odin Inspector 中绘制 BilingualTitleAttribute 标题。
    /// </summary>
    [DrawerPriority(1)]
    public class BilingualTitleAttributeDrawer : BilingualAttributeDrawer<BilingualTitleAttribute>
    {
        ValueResolver<string> _subTitleResolver;
        ValueResolver<string> _titleResolver;

        protected override void OnInitialize()
        {
            _titleResolver = ValueResolver.GetForString(Property, GetAttributeTitle());
            _subTitleResolver = ValueResolver.GetForString(Property, GetAttributeSubTitle());
        }

        protected override void OnLanguageChanged()
        {
            _titleResolver = ValueResolver.GetForString(Property, GetAttributeTitle());
            _subTitleResolver = ValueResolver.GetForString(Property, GetAttributeSubTitle());
            base.OnLanguageChanged();
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (Attribute.BeforeSpace)
            {
                if (Property != Property.Tree.GetRootProperty(0))
                {
                    EditorGUILayout.Space();
                }
            }

            var flag = true;
            if (_titleResolver.HasError)
            {
                SirenixEditorGUI.ErrorMessageBox(_titleResolver.ErrorMessage);
                flag = false;
            }

            if (_subTitleResolver.HasError)
            {
                SirenixEditorGUI.ErrorMessageBox(_subTitleResolver.ErrorMessage);
                flag = false;
            }

            if (flag)
            {
                SirenixEditorGUI.Title(_titleResolver.GetValue(), _subTitleResolver.GetValue(),
                    (TextAlignment)Attribute.TitleAlignment, Attribute.HorizontalLine, Attribute.Bold);
            }

            CallNextDrawer(label);
        }

        string GetAttributeTitle() => Attribute.TitleData.GetCurrentOrFallback();

        string GetAttributeSubTitle() => Attribute.SubtitleData.GetCurrentOrFallback();
    }
}
