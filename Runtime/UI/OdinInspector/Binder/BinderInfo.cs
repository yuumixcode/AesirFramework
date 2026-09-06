using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 绑定单元数据。描述一条要绑定到生成脚本字段的组件引用信息。
    /// <para>
    /// 在 <see cref="BinderAssistant" /> 的「绑定单元列表」中以表格形式配置，
    /// 由「构建绑定单元」按子物体上的 <see cref="BinderTag" /> 标记增量维护；
    /// 每条记录对应生成脚本「绑定字段（自动生成）」region 中的一个
    /// <c>[SerializeField]</c> 字段和 <c>BindComponents()</c> 中的一行赋值代码。
    /// </para>
    /// </summary>
    [Serializable]
    public class BinderInfo
    {
        /// <summary>
        /// 被绑定组件所在的 GameObject 引用。
        /// </summary>
        [TableColumnWidth(160)]
        [LabelText("物体")]
        public GameObject LabelObj;

        /// <summary>
        /// 组件类型的完整名称（含命名空间），作为代码生成时 <c>GetComponent&lt;T&gt;()</c> 的泛型参数；
        /// 选择 <c>GameObject</c> 表示绑定物体本身。
        /// </summary>
        [TableColumnWidth(160)]
        [LabelText("组件类型")]
        [ValueDropdown(nameof(GetTypesString))]
        public string ComponentFullName = "UnityEngine.Transform";

        /// <summary>
        /// 生成脚本中的字段名（camelCase）。默认值为「物体名_类型简称」，可手动修改；
        /// 重复字段名会在校验时报错。
        /// </summary>
        [TableColumnWidth(170)]
        [LabelText("字段名")]
        [InlineButton(nameof(DefaultFieldName), "默认")]
        public string FieldName;

        /// <summary>
        /// 相对于 <see cref="BinderAssistant" /> 的 <c>transform.Find()</c> 路径，
        /// 由 <see cref="UpdatePath" /> 自动计算；空字符串表示绑定 Assistant 自身所在物体。
        /// </summary>
        [LabelText("路径")]
        [DisplayAsString]
        public string HierarchyPath;

        /// <summary>
        /// 供序列化创建实例使用的无参构造。
        /// </summary>
        public BinderInfo() { }

        /// <summary>
        /// 由 <see cref="BinderAssistant" /> 扫描 <see cref="BinderTag" /> 时构造，自动计算路径并生成默认字段名。
        /// </summary>
        public BinderInfo(BinderAssistant assistant, BinderTag tagObj)
        {
            LabelObj = tagObj.SelfObj;
            DefaultFieldName();
            UpdatePath(assistant);
        }

        /// <summary>
        /// 将字段名重置为默认值: 「物体名_类型简称」的 camelCase 形式（如 <c>playButton_Button</c>）。
        /// 可能与其他单元重名，重名会在校验时报错。
        /// </summary>
        public void DefaultFieldName()
        {
            var objectName = LabelObj ? LabelObj.name : "Element";
            FieldName = BinderCodeGenerator.ComposeDefaultFieldName(objectName, ComponentFullName);
        }

        /// <summary>
        /// 更新相对于 BinderAssistant 的层级路径。
        /// 物体丢失或缺少 BinderTag 标记时置空，交由校验报错。
        /// </summary>
        public void UpdatePath(BinderAssistant assistant)
        {
            if (!LabelObj || !LabelObj.TryGetComponent<BinderTag>(out var tag))
            {
                HierarchyPath = null;
                return;
            }

            HierarchyPath = BinderHierarchyUtility.GetRelativePath(assistant.HierarchyPath, tag.HierarchyPath);
        }

        /// <summary>
        /// 获取 <see cref="LabelObj" /> 上可绑定的组件类型下拉列表（含 GameObject 自身）。
        /// </summary>
        ValueDropdownList<string> GetTypesString()
        {
            var list = new ValueDropdownList<string>();
            if (!LabelObj || !LabelObj.TryGetComponent<BinderTag>(out var tag))
            {
                list.Add("GameObject", typeof(GameObject).FullName);
                return list;
            }

            foreach (var type in tag.Types)
            {
                list.Add(type.Name, type.FullName);
            }

            return list;
        }
    }
}
