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
    [Summary("在 Odin Inspector 中绘制 BilingualTitleAttribute 标题，支持根据检查器语言动态切换标题与子标题")]
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
