using System;
using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class InlinePropertyExampleSO : AttributeExampleSO<InlinePropertyExampleSO>
    {
        [Title("Struct Property")]
        public Vector2Int position;

        [Title("Class Property")]
        public SimpleData data;

        public override void AesirInspectorReset()
        {
            position = new Vector2Int { x = 10, y = 20 };
            data = new SimpleData { name = "Example", id = 1 };
        }

        [Serializable]
        [InlineProperty(LabelWidth = 50)]
        public struct Vector2Int
        {
            [HorizontalGroup]
            public int x;

            [HorizontalGroup]
            public int y;
        }

        [Serializable]
        [InlineProperty]
        public class SimpleData
        {
            [HorizontalGroup]
            [HideLabel]
            public string name;

            [HorizontalGroup(Width = 60)]
            [HideLabel]
            public int id;
        }
    }
}
