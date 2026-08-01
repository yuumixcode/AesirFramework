using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class SearchableExampleSO : AttributeExampleSO<SearchableExampleSO>
    {
        [Title("No Parameters")]
        [Searchable]
        public List<Item> items = new List<Item>
        {
            new Item { name = "Apple", value = 10, tags = new List<string> { "Fruit", "Red" } },
            new Item { name = "Banana", value = 20, tags = new List<string> { "Fruit", "Yellow" } },
            new Item { name = "Carrot", value = 5, tags = new List<string> { "Vegetable", "Orange" } }
        };

        [Title("Parameter: Recursive (False)")]
        [Searchable(Recursive = false)]
        public List<Item> nonRecursiveItems = new List<Item>
        {
            new Item { name = "Apple", value = 10, tags = new List<string> { "Fruit", "Red" } },
            new Item { name = "Banana", value = 20, tags = new List<string> { "Fruit", "Yellow" } }
        };

        [Title("Parameter: FuzzySearch (False)")]
        [Searchable(FuzzySearch = false)]
        public List<Item> exactMatchItems = new List<Item>
        {
            new Item { name = "Apple", value = 10 },
            new Item { name = "Banana", value = 20 }
        };

        public override void AesirInspectorReset()
        {
            items = new List<Item>
            {
                new Item { name = "Apple", value = 10, tags = new List<string> { "Fruit", "Red" } },
                new Item { name = "Banana", value = 20, tags = new List<string> { "Fruit", "Yellow" } },
                new Item { name = "Carrot", value = 5, tags = new List<string> { "Vegetable", "Orange" } }
            };
            nonRecursiveItems = new List<Item>
            {
                new Item { name = "Apple", value = 10, tags = new List<string> { "Fruit", "Red" } },
                new Item { name = "Banana", value = 20, tags = new List<string> { "Fruit", "Yellow" } }
            };
            exactMatchItems = new List<Item>
            {
                new Item { name = "Apple", value = 10 },
                new Item { name = "Banana", value = 20 }
            };
        }

        [Serializable]
        public class Item
        {
            public string name;
            public int value;
            public List<string> tags;
        }
    }
}
