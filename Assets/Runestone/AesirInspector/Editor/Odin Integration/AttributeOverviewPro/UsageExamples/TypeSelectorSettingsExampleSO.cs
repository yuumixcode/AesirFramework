using System;
using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class TypeSelectorSettingsExampleSO : AttributeExampleSO<TypeSelectorSettingsExampleSO>
    {
        [Title("No Parameters")]
        [ShowInInspector]
        public Type Default;

        [Title("Parameter: PreferNamespaces")]
        [TypeSelectorSettings(PreferNamespaces = true, ShowCategories = false, ShowNoneItem = false)]
        [ShowInInspector]
        public Type PreferNamespacesOn;

        [Title("Parameter: ShowCategories")]
        [TypeSelectorSettings(ShowCategories = true, PreferNamespaces = false, ShowNoneItem = false)]
        [ShowInInspector]
        public Type ShowCategoriesOn;

        [Title("Parameter: ShowNoneItem")]
        [TypeSelectorSettings(ShowNoneItem = true, PreferNamespaces = false, ShowCategories = false)]
        [ShowInInspector]
        public Type ShowNoneItemOn;

        public override void AesirInspectorReset()
        {
            Default = null;
            PreferNamespacesOn = null;
            ShowCategoriesOn = null;
            ShowNoneItemOn = null;
        }
    }
}
