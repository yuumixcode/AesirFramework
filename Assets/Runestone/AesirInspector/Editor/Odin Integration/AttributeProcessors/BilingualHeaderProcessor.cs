using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    public class BilingualHeaderProcessor : OdinAttributeProcessor<BilingualHeaderControl>
    {
        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        {
            attributes.Add(new InlinePropertyAttribute());
            attributes.Add(new HideLabelAttribute());
        }

        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty,
            MemberInfo member,
            List<Attribute> attributes)
        {
            switch (member.Name)
            {
                case nameof(BilingualHeaderControl.headerName):
                    attributes.Add(new PropertyOrderAttribute(0));
                    attributes.Add(new PropertySpaceAttribute(13));
                    attributes.Add(new BoxGroupAttribute("OuterBox"));
                    attributes.Add(new HorizontalGroupAttribute("OuterBox/HoriTop", 0.75f));
                    break;
                case nameof(BilingualHeaderControl.headerIntroduction):
                    attributes.Add(
                        new HideIfAttribute(nameof(BilingualHeaderControl.HideHeaderIntroduction)));
                    attributes.Add(new PropertyOrderAttribute(30));
                    attributes.Add(new BoxGroupAttribute("OuterBox"));
                    attributes.Add(new HorizontalGroupAttribute("OuterBox/HoriBottom", 0.98f));
                    attributes.Add(new PropertySpaceAttribute(10, 8));
                    break;
                case nameof(BilingualHeaderControl.PlaceholderMethod1):
                    attributes.Add(new PropertyOrderAttribute(-10));
                    attributes.Add(new OnInspectorGUIAttribute());
                    attributes.Add(new BoxGroupAttribute("OuterBox", false));
                    attributes.Add(new HorizontalGroupAttribute("OuterBox/HoriTop", 0.01f));
                    break;
                case nameof(BilingualHeaderControl.SwitchLanguage):
                    attributes.Add(new PropertyOrderAttribute(5));
                    attributes.Add(new BoxGroupAttribute("OuterBox"));
                    attributes.Add(new PropertySpaceAttribute(8, 5));
                    attributes.Add(new HorizontalGroupAttribute("OuterBox/HoriTop", 0.22f));
                    attributes.Add(new VerticalGroupAttribute("OuterBox/HoriTop/VerRight"));
                    attributes.Add(new ButtonAttribute("@" + nameof(AesirInspectorLanguageSettingsSO) + "." +
                                                       nameof(AesirInspectorLanguageSettingsSO
                                                           .CurrentIsChinese) + " ? \"中文\" : \"English\"")
                    {
                        ButtonHeight = 24,
                        Icon = SdfIconType.Translate
                    });
                    break;
                case nameof(BilingualHeaderControl.OpenUrl):
                    attributes.Add(new PropertyOrderAttribute(10));
                    attributes.Add(new BoxGroupAttribute("OuterBox"));
                    attributes.Add(new HorizontalGroupAttribute("OuterBox/HoriTop", 0.22f));
                    attributes.Add(new VerticalGroupAttribute("OuterBox/HoriTop/VerRight"));
                    attributes.Add(new ButtonAttribute("@" + nameof(AesirInspectorLanguageSettingsSO) + "." +
                                                       nameof(AesirInspectorLanguageSettingsSO
                                                           .CurrentIsChinese) +
                                                       " ? \"文档\" : \"Documentation\"")
                    {
                        ButtonHeight = 24,
                        Icon = SdfIconType.Link45deg
                    });
                    break;
                case nameof(BilingualHeaderControl.PlaceholderMethod2):
                    attributes.Add(new BoxGroupAttribute("OuterBox"));
                    attributes.Add(new HorizontalGroupAttribute("OuterBox/HoriBottom", 0.01f));
                    attributes.Add(new OnInspectorGUIAttribute());
                    break;
            }
        }
    }
}
