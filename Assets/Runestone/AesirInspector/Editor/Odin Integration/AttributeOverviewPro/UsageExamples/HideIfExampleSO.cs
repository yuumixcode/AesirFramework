using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    // [AesirInspectorReset]
    public class HideIfExampleSO : AttributeExampleSO<HideIfExampleSO>
    {
        [Title("Controls")]
        public Object someObject;

        [EnumToggleButtons]
        public InfoMessageType someEnum;

        public bool isToggled;

        [FoldoutGroup("Basic Usage")]
        [HideIf("someEnum", InfoMessageType.Info)]
        public Vector2 hideIfInfo;

        [FoldoutGroup("Basic Usage")]
        [HideIf("someEnum", InfoMessageType.Error)]
        public Vector2 hideIfError;

        [FoldoutGroup("Basic Usage")]
        [HideIf("someEnum", InfoMessageType.Warning)]
        public Vector2 hideIfWarning;

        [FoldoutGroup("Basic Usage")]
        [HideIf("isToggled")]
        public int hideIfToggled;

        [FoldoutGroup("Basic Usage")]
        [HideIf("someObject")]
        public Vector3 hideWhenIsNotNull;

        [FoldoutGroup("Basic Usage")]
        [HideIf("Method")]
        public int hideWithMethod;

        [FoldoutGroup("Expression (@)")]
        [HideIf("@this.isToggled && this.someObject != null || this.someEnum == InfoMessageType.Error")]
        public int hideWithExpression;

        bool Method() =>
            (isToggled && someObject != null) || someEnum == InfoMessageType.Error;

        public override void AesirInspectorReset()
        {
            someObject = null;
            someEnum = InfoMessageType.Info;
            isToggled = false;
        }
    }
}
