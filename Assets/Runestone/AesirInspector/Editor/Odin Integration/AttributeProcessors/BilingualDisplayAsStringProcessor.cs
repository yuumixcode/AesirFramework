using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    public class BilingualDisplayAsStringProcessor : OdinAttributeProcessor<BilingualDisplayAsStringControl>
    {
        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        {
            attributes.Add(new HideLabelAttribute());
            attributes.Add(new InlinePropertyAttribute());
        }

        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty,
            MemberInfo member,
            List<Attribute> attributes)
        {
            var widget = (BilingualDisplayAsStringControl)parentProperty.ValueEntry.WeakSmartValue;
            switch (member.Name)
            {
                case nameof(BilingualDisplayAsStringControl.ChineseDisplay)
                    or nameof(BilingualDisplayAsStringControl.EnglishDisplay):
                    attributes.Add(new HideLabelAttribute());
                    attributes.Add(new ShowInInspectorAttribute());
                    attributes.Add(new EnableGUIAttribute());
                    attributes.Add(new ReadOnlyAttribute());
                    if (member.Name == nameof(BilingualDisplayAsStringControl.ChineseDisplay))
                    {
                        attributes.Add(new ShowIfAttribute(
                            "@" + nameof(AesirInspectorLanguageSettingsSO) + "." +
                            nameof(AesirInspectorLanguageSettingsSO.CurrentIsChinese), false));
                    }
                    else
                    {
                        attributes.Add(new ShowIfAttribute(
                            "@" + nameof(AesirInspectorLanguageSettingsSO) + "." +
                            nameof(AesirInspectorLanguageSettingsSO.CurrentIsEnglish), false));
                    }

                    if (widget != null)
                    {
                        attributes.Add(new DisplayAsStringAttribute(widget.overflow)
                        {
                            Alignment = widget.alignment,
                            FontSize = widget.fontSize,
                            EnableRichText = widget.enableRichText,
                            Format = widget.format
                        });
                    }

                    break;

                case nameof(BilingualDisplayAsStringControl.fontSize):
                case nameof(BilingualDisplayAsStringControl.alignment):
                case nameof(BilingualDisplayAsStringControl.enableRichText):
                case nameof(BilingualDisplayAsStringControl.format):
                case nameof(BilingualDisplayAsStringControl.overflow):
                    attributes.Add(new HideInInspector());
                    break;
            }
        }
    }
}
