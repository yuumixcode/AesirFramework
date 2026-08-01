namespace Runestone.AesirInspector
{
    /// <summary>
    /// Aesir Inspector 重置接口，实现该接口的类可以通过 AesirInspectorReset() 方法重置所有字段到默认值。
    /// </summary>
    public interface IAesirInspectorReset
    {
        /// <summary>
        /// 将所有字段重置为默认值。
        /// </summary>
        void AesirInspectorReset();
    }
}
