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
    /// 带有详细信息的双语信息框特性的 Drawer。
    /// </summary>
    [DrawerPriority(0.0, 10001.0)]
    public class DetailInfoBoxAttributeDrawer : OdinAttributeDrawer<DetailInfoBoxAttribute>
    {
        ValueResolver<string> _detailsResolver;
        bool _drawMessageBox;
        LocalPersistentContext<bool> _isExpanded;
        ValueResolver<string> _messageResolver;
        MessageType _messageType;
        ValueResolver<bool> _visibleIfResolver;

        protected override void Initialize()
        {
            _visibleIfResolver = ValueResolver.Get(Property, Attribute.VisibleIf, true);
            _messageResolver =
                ValueResolver.GetForString(Property, Attribute.BilingualData.GetCurrentOrFallback());
            _detailsResolver =
                ValueResolver.GetForString(Property, Attribute.DetailsBilingualData.GetCurrentOrFallback());

            _isExpanded = Property.Context.GetPersistent(this, "isExpanded", false);
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
            _detailsResolver =
                ValueResolver.GetForString(Property, Attribute.DetailsBilingualData.GetCurrentOrFallback());
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

            if (_detailsResolver.HasError)
            {
                SirenixEditorGUI.MessageBox(_detailsResolver.ErrorMessage, MessageType.Error,
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
                    var details = _detailsResolver.GetValue();

                    _isExpanded.Value =
                        SirenixEditorGUI.DetailedMessageBox(message, details, _messageType,
                            _isExpanded.Value);
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
