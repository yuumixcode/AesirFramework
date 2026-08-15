using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class HideInTablesExampleSO : AttributeExampleSO<HideInTablesExampleSO>
    {
        [Title("No Parameters")]
        public MyItem item = new MyItem();

        [TableList]
        public List<MyItem> table = new List<MyItem>
        {
            new MyItem(),
            new MyItem(),
            new MyItem()
        };

        public override void AesirInspectorReset()
        {
            item = new MyItem();
            table = new List<MyItem> { new MyItem(), new MyItem(), new MyItem() };
        }

        [Serializable]
        public class MyItem
        {
            public string name;
            public int value;

            [HideInTables]
            public int hiddenValue;
        }
    }
}
