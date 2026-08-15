using System;
using System.Collections.Generic;
using System.Linq;
using Runestone.AesirArchitecture;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirModules
{
    /// <summary>
    /// Binder 标签组件。挂载在需要自动绑定引用的子物体上，标记该物体可被 <see cref="BinderAssistant"/> 扫描并生成绑定信息。
    /// <para>
    /// 一个物体上可绑定多个不同类型的组件，通过 <see cref="ComponentNumber"/> 指定数量。
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    public class BinderTag : AesirMonoBehaviour
    {
        [SerializeField]
        [HideInInspector]
        int componentNumber;

        /// <summary>
        /// 当前物体上需要绑定的组件数量。<see cref="BinderAssistant"/> 据此为每个组件生成一条 <see cref="BinderInfo"/>。
        /// </summary>
        [ShowInInspector]
        [LabelText("绑定组件数量: ")]
        [PropertyRange(1, "$MaxComponentNumber")]
        public int ComponentNumber
        {
            get => componentNumber;
            set => componentNumber = value;
        }

        // 下拉范围上限：当前物体上可绑定的组件类型数量（含 GameObject 自身）
        double MaxComponentNumber => Types.Count();

        /// <summary>
        /// 当前物体引用
        /// </summary>
        public GameObject SelfObj => gameObject;

        /// <summary>
        /// 当前物体在场景层级中的绝对路径
        /// </summary>
        public string HierarchyPath => BinderHierarchyUtility.GetAbsolutePath(transform);

        /// <summary>
        /// 当前物体上可绑定的组件类型集合（含 GameObject 自身）。
        /// 排除了 <see cref="BinderTag"/> 自身，避免将标记组件纳入绑定选项。
        /// </summary>
        public IEnumerable<Type> Types
        {
            get
            {
                var types = GetComponents<Component>().Where(x => x is not BinderTag).Select(x => x.GetType())
                    .ToList();
                types.Add(typeof(GameObject));
                return types;
            }
        }
    }
}
