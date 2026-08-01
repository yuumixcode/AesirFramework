namespace Runestone.AesirInspector
{
    /// <summary>
    /// IOdinBridge 的静态定位器。OdinIntegration 程序集在加载时注入 OdinBridge 实现。
    /// </summary>
    public static class OdinBridgeLocator
    {
        public static IOdinBridge Bridge { get; set; } = new DefaultOdinBridge();
    }
}
