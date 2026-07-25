using Sirenix.OdinInspector;

#pragma warning disable CS0414 // 字段已被赋值，但它的值从未被使用
namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// ShowInInspector 特性案例。
    /// </summary>
    [AesirExample]
    internal class ShowInInspectorExampleSO : AttributeExampleSO<ShowInInspectorExampleSO>
    {
        [Title("Usage with Private Fields")]
        [ShowInInspector]
        int _privateField = 10;

        [Title("Usage with Properties")]
        [ShowInInspector]
        public int PropertyExample { get; set; } = 20;

        [ShowInInspector]
        public string ReadOnlyProperty => "I am a read-only property";

        public override void AesirInspectorReset()
        {
            _privateField = 10;
            PropertyExample = 20;
        }
    }
}
