using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class SceneObjectsOnlyExampleSO : AttributeExampleSO<SceneObjectsOnlyExampleSO>
    {
        [Title("No Parameters")]
        [SceneObjectsOnly]
        public List<GameObject> onlySceneObjects;

        [SceneObjectsOnly]
        public GameObject someSceneObject;

        [SceneObjectsOnly]
        public MeshRenderer someMeshRenderer;

        public override void AesirInspectorReset()
        {
            onlySceneObjects = new List<GameObject>();
            someSceneObject = null;
            someMeshRenderer = null;
        }
    }
}
