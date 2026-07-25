using System;
using System.Collections.Generic;
using System.Linq;
using Runestone.AesirArchitecture;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirModules
{
    /// <summary>
    /// Binder 标签组件。标记需要自动绑定引用的子物体。
    /// </summary>
    [DisallowMultipleComponent]
    public class BinderTag : AesirMonoBehaviour
    {
        [SerializeField]
        [HideInInspector]
        int componentNumber;

        [ShowInInspector]
        [LabelText("绑定组件数量: ")]
        [PropertyRange(1, "$MaxComponentNumber")]
        public int ComponentNumber
        {
            get => componentNumber;
            set => componentNumber = value;
        }

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
        /// 当前物体上可绑定的组件类型集合（含 GameObject 自身）
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
