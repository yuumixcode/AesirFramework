using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// Odin 序列化的特性案例 SO 泛型抽象基类，提供单例模式。
    /// 子资产持久化在 AttributeOverviewDatabaseSO 中。
    /// </summary>
    public abstract class OdinAttributeExampleSO<T> : SerializedScriptableObject, IAesirInspectorReset
        where T : OdinAttributeExampleSO<T>
    {
        static T _asset;

        /// <summary>
        /// 获取单例实例，若不存在则作为数据库子资产自动创建。
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_asset)
                {
                    return _asset;
                }

                _asset = AttributeOverviewDatabaseSO.GetOrCreateExampleSubAsset<T>();
                return _asset;
            }
        }

        /// <summary>
        /// 重置案例数据到初始状态。
        /// </summary>
        public abstract void AesirInspectorReset();
    }
}
