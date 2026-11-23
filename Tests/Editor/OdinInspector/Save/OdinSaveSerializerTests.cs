using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Runestone.AesirModules.Tests.Editor.OdinInspector.Save
{
    /// <summary>
    /// OdinSaveSerializer 的 EditMode 用例：Dictionary 与多态派生类字段往返
    /// （spec「Odin 序列化器往返字典与多态」——JsonUtility 轨不支持的形状）。
    /// </summary>
    public class OdinSaveSerializerTests
    {
        [Serializable]
        public class OdinAnimal
        {
            public string Name;
        }

        [Serializable]
        public class OdinDog : OdinAnimal
        {
            public bool GoodBoy;
        }

        [Serializable]
        public class OdinTestPayload
        {
            public Dictionary<string, int> Stats;
            public OdinAnimal Animal;
        }

        [Test]
        public void Odin_RoundTripsDictionaryAndPolymorphicField()
        {
            var serializer = new OdinSaveSerializer();
            var payload = new OdinTestPayload
            {
                Stats = new Dictionary<string, int> { ["kills"] = 3, ["deaths"] = 1 },
                Animal = new OdinDog { Name = "Rex", GoodBoy = true }
            };

            var restored = serializer.Deserialize<OdinTestPayload>(serializer.Serialize(payload));

            Assert.AreEqual(payload.Stats, restored.Stats, "Dictionary 应完整往返");
            Assert.IsInstanceOf<OdinDog>(restored.Animal, "多态派生类型信息应保留");
            Assert.AreEqual("Rex", restored.Animal.Name);
            Assert.IsTrue(((OdinDog)restored.Animal).GoodBoy);
        }

        [Test]
        public void Odin_Id_IsStableIdentifier()
        {
            Assert.AreEqual("odin", new OdinSaveSerializer().Id);
        }
    }
}