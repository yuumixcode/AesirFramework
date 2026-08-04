using System;
using System.Collections;
using System.Collections.Generic;
using Runestone.AesirInspector.OdinIntegration.Editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEngine;

public class NewBehaviourScriptProcessor : OdinAttributeProcessor<NewBehaviourScript>
{
    public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
    {
        base.ProcessSelfAttributes(property, attributes);
        attributes.Add(new AesirExampleAttribute());
    }
}

[InlineProperty]
public class NewBehaviourScript : MonoBehaviour
{
    [Button("输出脚本文件路径")]
    public void GetScriptPath()
    {
        var attr = typeof(NewBehaviourScript).GetCustomAttribute<AesirExampleAttribute>();
        if (attr == null)
        {
            Debug.Log("没有 AesirExampleAttribute");
        }
        else
        {
            Debug.Log(attr.FilePath);
        }

        var attr2 = typeof(NewBehaviourScript).GetCustomAttribute<InlinePropertyAttribute>();
        if (attr2 == null)
        {
            Debug.Log("没有 InlinePropertyAttribute");
        }
        else
        {
            Debug.Log(attr2.GetType().Name);
        }
    }
}
