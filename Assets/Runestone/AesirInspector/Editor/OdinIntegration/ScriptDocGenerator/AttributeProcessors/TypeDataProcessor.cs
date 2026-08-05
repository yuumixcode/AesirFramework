using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    public class TypeDataProcessor : OdinAttributeProcessor<TypeData>
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty,
            MemberInfo member,
            List<Attribute> attributes)
        {
            if (member.DeclaringType != typeof(TypeData) || member.ReflectedType != typeof(TypeData))
            {
                return;
            }

            if (member.MemberType == MemberTypes.Property)
            {
                attributes.Add(new ShowInInspectorAttribute());
            }
        }
    }
}
