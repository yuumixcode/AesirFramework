using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// 特性案例序列化类型。
    /// </summary>
    public enum AttributeExampleType
    {
        UnitySerialized = 0,
        OdinSerialized = 1
    }

    /// <summary>
    /// 单个特性使用案例预览项，持有案例名称与对应的 ScriptableObject 引用。
    /// </summary>
    public class AttributeExamplePreviewItem
    {
        SerializedScriptableObject _odinSerializedExample;

        ScriptableObject _unitySerializedExample;

        /// <summary>
        /// 案例序列化类型。
        /// </summary>
        public AttributeExampleType ExampleType { get; private set; }

        /// <summary>
        /// 案例显示名称。
        /// </summary>
        public string ItemName { get; private set; }

        /// <summary>
        /// Unity 原生序列化的案例 ScriptableObject。
        /// </summary>
        public ScriptableObject UnitySerializedExample
        {
            get
            {
                if (ExampleType == AttributeExampleType.UnitySerialized)
                {
                    return _unitySerializedExample;
                }

                Debug.LogError("Odin 序列化的案例应该获取 " + nameof(OdinSerializedExample));
                return null;
            }
        }

        /// <summary>
        /// Odin 序列化的案例 ScriptableObject。
        /// </summary>
        public SerializedScriptableObject OdinSerializedExample
        {
            get
            {
                if (ExampleType == AttributeExampleType.OdinSerialized)
                {
                    return _odinSerializedExample;
                }

                Debug.LogError("Unity 原生序列化的案例应该获取 " + nameof(UnitySerializedExample));
                return null;
            }
        }

        /// <summary>
        /// 初始化为 Unity 序列化案例。
        /// </summary>
        public AttributeExamplePreviewItem InitializeUnitySerializedExample(string itemName,
            ScriptableObject unitySerializedExample)
        {
            ExampleType = AttributeExampleType.UnitySerialized;
            ItemName = itemName;
            _unitySerializedExample = unitySerializedExample;
            return this;
        }

        /// <summary>
        /// 初始化为 Odin 序列化案例。
        /// </summary>
        public AttributeExamplePreviewItem InitializeOdinSerializedExample(string itemName,
            SerializedScriptableObject odinSerializedExample)
        {
            ExampleType = AttributeExampleType.OdinSerialized;
            ItemName = itemName;
            _odinSerializedExample = odinSerializedExample;
            return this;
        }

        /// <summary>
        /// 重置当前案例到初始状态。
        /// </summary>
        public void Reset()
        {
            switch (ExampleType)
            {
                case AttributeExampleType.OdinSerialized:
                    if (_odinSerializedExample is IAesirInspectorReset canResetOdin)
                    {
                        canResetOdin.AesirInspectorReset();
                        Debug.Log(_odinSerializedExample.GetType().Name + " 重置成功！");
                    }
                    else
                    {
                        Debug.LogWarning("当前案例 " + _odinSerializedExample.GetType().Name +
                                         " 未实现 IAesirInspectorReset 接口！");
                    }

                    break;

                case AttributeExampleType.UnitySerialized:
                    if (_unitySerializedExample is IAesirInspectorReset canResetUnity)
                    {
                        canResetUnity.AesirInspectorReset();
                        Debug.Log(_unitySerializedExample.GetType().Name + " 重置成功！");
                    }
                    else
                    {
                        Debug.LogWarning("当前案例 " + _unitySerializedExample.GetType().Name +
                                         " 未实现 IAesirInspectorReset 接口！");
                    }

                    break;
            }
        }
    }
}
