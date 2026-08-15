using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 绑定单元数据。描述一个自动绑定的组件引用信息。
    /// <para>
    /// 每条 <see cref="BinderInfo"/> 对应生成脚本中的一个 <c>[SerializeField]</c> 字段和 <c>BindReferences()</c> 中的一行赋值代码。
    /// </para>
    /// </summary>
    [Serializable]
    public class BinderInfo
    {
        /// <summary>
        /// 被绑定组件所在的 GameObject 引用。用于在 Inspector 中定位和展示。
        /// </summary>
        [HorizontalGroup(0.4F)]
        [HideLabel]
        public GameObject LabelObj;

        /// <summary>
        /// 组件类型的完整名称（含命名空间），如 <c>UnityEngine.UI.Button</c>。
        /// 作为代码生成时 <c>GetComponent&lt;T&gt;()</c> 的泛型参数。
        /// </summary>
        [HorizontalGroup(0.6F)]
        [ValueDropdown("GetTypesString")]
        [HideLabel]
        public string ComponentFullName = "UnityEngine.Transform";

        /// <summary>
        /// 生成脚本中的字段名。默认值为「物体名 + 组件类型简称」，可手动修改。
        /// </summary>
        [LabelText("组件变量名: ")]
        [LabelWidth(80)]
        [InlineButton("DefaultFieldName", "设置默认值")]
        public string FieldName;

        /// <summary>
        /// 相对于 <see cref="BinderAssistant"/> 的 <c>transform.Find()</c> 路径。
        /// 由 <see cref="UpdatePath"/> 自动计算，只读展示。
        /// </summary>
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

        /// <summary>
        /// 将字段名设为默认值：物体名 + 组件类型简称（如 <c>Button_obj</c>）
        /// </summary>
        void DefaultFieldName()
        {
            FieldName = LabelObj.name + ComponentFullName.Split('.')[^1];
        }

        /// <summary>
        /// 更新相对于 BinderAssistant 的层级路径。
        /// 当层级结构变动后调用此方法刷新路径。
        /// </summary>
        public void UpdatePath(BinderAssistant assistant)
        {
            HierarchyPath = BinderHierarchyUtility.GetRelativePath(assistant.HierarchyPath,
                LabelObj.GetComponent<BinderTag>().HierarchyPath);
        }

        /// <summary>
        /// 获取 <see cref="LabelObj"/> 上可绑定的组件类型下拉列表
        /// </summary>
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
