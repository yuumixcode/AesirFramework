using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// Odin 序列化的特性案例 SO 泛型抽象基类，提供单例模式。
    /// </summary>
    [Summary("Odin 序列化的特性案例 SO 泛型抽象基类，提供单例模式")]
    public abstract class OdinAttributeExampleSO<T> : SerializedScriptableObject, IAesirInspectorReset
        where T : OdinAttributeExampleSO<T>
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
