// ----------------------------------------------------------------------------
// MIT License
// 
// Copyright (c) 2026 Runestone - Yuumix
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ----------------------------------------------------------------------------

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
    [Summary("双语文本特性的 Drawer，根据当前编辑器语言生成对应的标签文本与图标显示")]
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
