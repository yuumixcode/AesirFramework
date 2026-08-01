using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class DictionaryDrawerSettingsExampleSO : OdinAttributeExampleSO<DictionaryDrawerSettingsExampleSO>
    {
        [Title("Parameter: KeyLabel, ValueLabel")]
        [DictionaryDrawerSettings(KeyLabel = "ID", ValueLabel = "Name", KeyColumnWidth = 60)]
        public Dictionary<int, string> simpleDictionary = new Dictionary<int, string>
        {
            { 1, "Alice" },
            { 2, "Bob" }
        };

        [Title("Parameter: DisplayMode")]
        [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.Foldout)]
        public Dictionary<string, List<int>> complexDictionary = new Dictionary<string, List<int>>
        {
            { "Group A", new List<int> { 1, 2, 3 } },
            { "Group B", new List<int> { 4, 5 } }
        };

        [Title("Parameter: IsReadOnly")]
        [DictionaryDrawerSettings(IsReadOnly = true)]
        public Dictionary<string, int> readOnlyDictionary = new Dictionary<string, int>
        {
            { "Fixed Key", 100 }
        };

        public override void AesirInspectorReset()
        {
            simpleDictionary = new Dictionary<int, string> { { 1, "Alice" }, { 2, "Bob" } };
            complexDictionary = new Dictionary<string, List<int>>
            {
                { "Group A", new List<int> { 1, 2, 3 } },
                { "Group B", new List<int> { 4, 5 } }
            };
            readOnlyDictionary = new Dictionary<string, int> { { "Fixed Key", 100 } };
        }
    }
}
