using System;
using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class TypeDrawerSettingsExampleSO : AttributeExampleSO<TypeDrawerSettingsExampleSO>
    {
        [Title("Parameter: BaseType")]
        [TypeDrawerSettings(BaseType = typeof(IBaseGeneric<>))]
        public Type BaseTypeSet;

        [Title("No Parameters")]
        public Type Default;

        [Title("Parameter: Filter")]
        [TypeDrawerSettings(BaseType = typeof(IBaseGeneric<>), Filter = TypeInclusionFilter.IncludeAll)]
        public Type FilterAll;

        public override void AesirInspectorReset()
        {
            Default = null;
            BaseTypeSet = null;
            FilterAll = null;
        }

        public interface IBaseGeneric<T> { }

        public interface IBase : IBaseGeneric<int> { }

        public abstract class Base : IBase { }

        public class Concrete : Base { }
    }
}
