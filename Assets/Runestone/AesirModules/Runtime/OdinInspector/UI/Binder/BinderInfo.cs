using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 绑定单元数据。描述一个自动绑定的组件引用信息。
    /// </summary>
    [Serializable]
    public class BinderInfo
    {
        [HorizontalGroup(0.4F)]
        [HideLabel]
        public GameObject LabelObj;

        [HorizontalGroup(0.6F)]
        [ValueDropdown("GetTypesString")]
        [HideLabel]
        public string ComponentFullName = "UnityEngine.Transform";

        [LabelText("组件变量名: ")]
        [LabelWidth(80)]
        [InlineButton("DefaultFieldName", "设置默认值")]
        public string FieldName;

        [DisplayAsString(13, Overflow = false)]
        [LabelText("Find ( ) 路径: ")]
        [LabelWidth(80)]
        public string HierarchyPath;

        /// <summary>
        /// 构造绑定单元，自动计算层级路径
        /// </summary>
        public BinderInfo(BinderAssistant assistant, BinderTag tagObj)
        {
            LabelObj = tagObj.SelfObj;
            UpdatePath(assistant);
        }

        void DefaultFieldName()
        {
            FieldName = LabelObj.name + ComponentFullName.Split('.')[^1];
        }

        /// <summary>
        /// 更新相对于 BinderAssistant 的层级路径
        /// </summary>
        public void UpdatePath(BinderAssistant assistant)
        {
            HierarchyPath = BinderHierarchyUtility.GetRelativePath(assistant.HierarchyPath,
                LabelObj.GetComponent<BinderTag>().HierarchyPath);
        }

        ValueDropdownList<string> GetTypesString()
        {
            var list = new ValueDropdownList<string>();
            if (!LabelObj)
            {
                list.Add("Transform", "UnityEngine.Transform");
                return list;
            }

            foreach (var type in LabelObj.GetComponent<BinderTag>().Types)
            {
                list.Add(type.Name, type.FullName);
            }

            return list;
        }
    }
}
