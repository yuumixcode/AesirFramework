using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    internal sealed class
        AesirInspectorLanguageSettingsProcessor : OdinAttributeProcessor<AesirInspectorLanguageSettingsSO>
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty,
            MemberInfo member,
            List<Attribute> attributes)
        {
            switch (member.Name)
            {
                case nameof(AesirInspectorLanguageSettingsSO.SetChinese):
                    attributes.Add(new ButtonAttribute("Switch Chinese", ButtonSizes.Large));
                    attributes.Add(
                        new ShowIfAttribute(nameof(AesirInspectorLanguageSettingsSO.CurrentIsEnglish)));
                    attributes.Add(new ShowInInspectorAttribute());
                    break;
                case nameof(AesirInspectorLanguageSettingsSO.SetEnglish):
                    attributes.Add(new ButtonAttribute("设置为英文", ButtonSizes.Large));
                    attributes.Add(
                        new ShowIfAttribute(nameof(AesirInspectorLanguageSettingsSO.CurrentIsChinese)));
                    attributes.Add(new ShowInInspectorAttribute());
                    break;
            }
        }
    }
}
