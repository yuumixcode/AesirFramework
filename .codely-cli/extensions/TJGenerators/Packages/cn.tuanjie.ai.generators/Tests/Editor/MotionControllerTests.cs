using System;
using NUnit.Framework;
using TJGenerators.PostProcessing;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace TJGenerators.Editor.Tests
{
    public class MotionControllerTests
    {
        private string _folder;
        private string _motion;

        [SetUp]
        public void SetUp()
        {
            string name = "MotionControllerTest_" + Guid.NewGuid().ToString("N");
            _folder = "Assets/" + name;
            AssetDatabase.CreateFolder("Assets", name);
            _motion = _folder + "/Motion.anim";
            var clip = new AnimationClip { name = "Wave" };
            AnimationUtility.SetEditorCurve(clip,
                EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.x"),
                AnimationCurve.Linear(0, 0, 1, 1));
            AssetDatabase.CreateAsset(clip, _motion);
        }

        [TearDown]
        public void TearDown() => AssetDatabase.DeleteAsset(_folder);

        private void AssertController(string path, bool loop)
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            Assert.NotNull(controller);
            var state = controller.layers[0].stateMachine.defaultState;
            Assert.AreEqual(AssetDatabase.LoadAssetAtPath<AnimationClip>(_motion), state.motion);
            Assert.AreEqual(loop ? 1 : 0, state.transitions.Length);
            if (loop)
            {
                Assert.AreEqual(state, state.transitions[0].destinationState);
                Assert.IsTrue(state.transitions[0].hasExitTime);
                Assert.AreEqual(1f, state.transitions[0].exitTime);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Controller_RespectsLoopFlag(bool loop)
        {
            string path = RiggedModelPostProcess.CreateSingleClipLoopAnimatorControllerFromMotionClip(
                _folder, "Character", _motion, loop);
            AssertController(path, loop);
        }

        [Test]
        public void LegacyOverload_KeepsLoopingDefault()
        {
            string path = RiggedModelPostProcess.CreateSingleClipLoopAnimatorControllerFromMotionClip(
                _folder, "Character", _motion);
            AssertController(path, true);
        }
    }
}
