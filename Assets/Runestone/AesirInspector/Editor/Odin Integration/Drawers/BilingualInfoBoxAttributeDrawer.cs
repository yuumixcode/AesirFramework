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
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// 双语信息框特性的 Drawer。
    /// </summary>
    [DrawerPriority(0.0, 10001.0)]
    [Summary("双语信息框特性的 Drawer，根据当前编辑器语言渲染对应语言的信息文本与图标样式")]
    public class BilingualInfoBoxAttributeDrawer : OdinAttributeDrawer<BilingualInfoBoxAttribute>
    {
        bool _drawMessageBox;
        ValueResolver<Color> _iconColorResolver;
        ValueResolver<string> _messageResolver;
        MessageType _messageType;
        ValueResolver<bool> _visibleIfResolver;

        protected override void Initialize()
        {
            _visibleIfResolver = ValueResolver.Get(Property, Attribute.VisibleIf, true);
            _messageResolver =
                ValueResolver.GetForString(Property, Attribute.BilingualData.GetCurrentOrFallback());
            _iconColorResolver = ValueResolver.Get(Property, Attribute.IconColor,
                EditorStyles.label.normal.textColor);
            _drawMessageBox = _visibleIfResolver.GetValue();
            switch (Attribute.InfoMessageType)
            {
                case InfoMessageType.Info:
                    _messageType = MessageType.Info;
                    break;
                case InfoMessageType.Warning:
                    _messageType = MessageType.Warning;
                    break;
                case InfoMessageType.Error:
                    _messageType = MessageType.Error;
                    break;
                default:
                    _messageType = MessageType.None;
                    break;
            }

            AesirInspectorLanguageSettingsSO.LanguageChanged -= ReloadResolver;
            AesirInspectorLanguageSettingsSO.LanguageChanged += ReloadResolver;
        }

        void ReloadResolver()
        {
            _messageResolver =
                ValueResolver.GetForString(Property, Attribute.BilingualData.GetCurrentOrFallback());
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            var flag = true;
            if (_visibleIfResolver.HasError)
            {
                SirenixEditorGUI.MessageBox(_visibleIfResolver.ErrorMessage, MessageType.Error,
                    GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
                flag = false;
            }

            if (_messageResolver.HasError)
            {
                SirenixEditorGUI.MessageBox(_messageResolver.ErrorMessage, MessageType.Error,
                    GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
                flag = false;
            }

            if (_iconColorResolver.HasError)
            {
                SirenixEditorGUI.MessageBox(_iconColorResolver.ErrorMessage, MessageType.Error,
                    GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
                flag = false;
            }

            if (flag)
            {
                if (Attribute.GUIAlwaysEnabled)
                {
                    GUIHelper.PushGUIEnabled(true);
                }

                if (Event.current.type == EventType.Layout)
                {
                    _drawMessageBox = _visibleIfResolver.GetValue();
                }

                if (_drawMessageBox)
                {
                    var message = _messageResolver.GetValue();
                    if (Attribute.HasDefinedIcon)
                    {
                        // 获取 Odin 全局设置，使其适配 Odin Inspector
                        SirenixEditorGUI.MessageBox(message, Attribute.Icon, _iconColorResolver.GetValue(),
                            GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
                    }
                    else
                    {
                        SirenixEditorGUI.MessageBox(message, _messageType,
                            GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
                    }
                }

                if (Attribute.GUIAlwaysEnabled)
                {
                    GUIHelper.PopGUIEnabled();
                }
            }

            CallNextDrawer(label);
        }
    }
}
