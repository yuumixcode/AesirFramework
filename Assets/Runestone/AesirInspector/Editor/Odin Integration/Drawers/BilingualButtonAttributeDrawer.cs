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
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// 双语按钮特性的 Drawer。
    /// </summary>
    [DrawerPriority(DrawerPriorityLevel.WrapperPriority)]
    [Summary("双语按钮特性的 Drawer，根据当前语言显示中文或英文按钮文本")]
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
