using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    internal sealed class AesirInspectorResetProcessor : OdinAttributeProcessor<IAesirInspectorReset>
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty,
            MemberInfo member,
            List<Attribute> attributes)
        {
            attributes.Add(new CustomContextMenuAttribute("Aesir Inspector Reset",
                nameof(IAesirInspectorReset.AesirInspectorReset)));
        }
    }
}
