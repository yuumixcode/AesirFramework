using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// Unity 原生序列化的特性案例 SO 泛型抽象基类，提供单例模式。
    /// </summary>
    [Summary("Unity 原生序列化的特性案例 SO 泛型抽象基类，提供单例模式")]
    public abstract class AttributeExampleSO<T> : ScriptableObject, IAesirInspectorReset
        where T : AttributeExampleSO<T>
    {
        static T _asset;

        /// <summary>
        /// 获取单例实例，若不存在则自动创建。
        /// </summary>
        [Summary("获取单例实例，若不存在则自动创建")]
        public static T Instance
        {
            get
            {
                if (_asset)
                {
                    return _asset;
                }

                _asset = ScriptableObjectSafeEditorUtility.GetSingletonAssetAndDeleteOther<T>(
                    AesirInspectorPaths.AttributeExamplesPath);
                return _asset;
            }
        }

        /// <summary>
        /// 重置案例数据到初始状态。
        /// </summary>
        [Summary("重置案例数据到初始状态")]
        public abstract void AesirInspectorReset();
    }
}
