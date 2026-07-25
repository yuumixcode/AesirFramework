using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class
        PolymorphicDrawerSettingsExampleSO : OdinAttributeExampleSO<PolymorphicDrawerSettingsExampleSO>
    {
        [Title("Default Polymorphic Style")]
        [PolymorphicDrawerSettings(ShowBaseType = true)]
        public IDemo<int> Default;

        [Title("Parameter: NonDefaultConstructorPreference")]
        [PolymorphicDrawerSettings(NonDefaultConstructorPreference =
            NonDefaultConstructorPreference.ConstructIdeal)]
        public IDemo<int> NonDefaultConstructorConstructIdeal;

        [Title("Parameter: ReadOnlyIfNotNullReference")]
        [PolymorphicDrawerSettings(ReadOnlyIfNotNullReference = true)]
        public IDemo<int> ReadOnlyIfNotNullReferenceOn;

        [Title("Parameter: ShowBaseType")]
        [PolymorphicDrawerSettings(ShowBaseType = false)]
        public IDemo<int> ShowBaseTypeOff;

        public override void AesirInspectorReset()
        {
            Default = null;
            ShowBaseTypeOff = null;
            ReadOnlyIfNotNullReferenceOn = null;
            NonDefaultConstructorConstructIdeal = null;
        }

        [Serializable]
        public class DemoInt32 : IDemo<int>
        {
            [OdinSerialize]
            public int Value { get; set; }
        }

        [Serializable]
        public class DemoString : IDemo<int>
        {
            public string ExtraInfo;

            [OdinSerialize]
            public int Value { get; set; }
        }
    }

    public interface IDemo<T>
    {
        T Value { get; set; }
    }
}
