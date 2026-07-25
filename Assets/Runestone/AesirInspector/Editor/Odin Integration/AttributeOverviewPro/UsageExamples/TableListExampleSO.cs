using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class TableListExampleSO : AttributeExampleSO<TableListExampleSO>
    {
        [Title("No Parameters")]
        [TableList(ShowIndexLabels = true)]
        public List<Item> items = new List<Item>
        {
            new Item { ID = 1, Name = "Apple", Price = 1.2f },
            new Item { ID = 2, Name = "Banana", Price = 0.8f },
            new Item { ID = 3, Name = "Orange", Price = 1.5f }
        };

        [Title("Parameter: DrawScrollView")]
        [TableList(DrawScrollView = true, MaxScrollViewHeight = 200)]
        public List<Item> scrollableItems = new List<Item>();

        [Title("Parameter: HideToolbar, AlwaysExpanded")]
        [TableList(HideToolbar = true, AlwaysExpanded = true)]
        public List<Item> simpleTable = new List<Item>
        {
            new Item { ID = 1, Name = "Item A" },
            new Item { ID = 2, Name = "Item B" }
        };

        public override void AesirInspectorReset()
        {
            items = new List<Item>
            {
                new Item { ID = 1, Name = "Apple", Price = 1.2f },
                new Item { ID = 2, Name = "Banana", Price = 0.8f },
                new Item { ID = 3, Name = "Orange", Price = 1.5f }
            };
            scrollableItems = new List<Item>();
            for (var i = 0; i < 10; i++)
                scrollableItems.Add(new Item { ID = i, Name = "Item " + i });
            simpleTable = new List<Item>
            {
                new Item { ID = 1, Name = "Item A" },
                new Item { ID = 2, Name = "Item B" }
            };
        }

        [Serializable]
        public class Item
        {
            [TableColumnWidth(50, false)]
            public int ID;

            [PreviewField(Height = 40)]
            [TableColumnWidth(50, false)]
            public Texture2D Icon;

            public string Name;

            public float Price;
        }
    }
}
