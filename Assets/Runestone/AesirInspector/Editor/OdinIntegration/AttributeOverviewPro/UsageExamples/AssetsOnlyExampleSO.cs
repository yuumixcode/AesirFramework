using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class AssetsOnlyExampleSO : AttributeExampleSO<AssetsOnlyExampleSO>
    {
        [Title("No Parameters")]
        [AssetsOnly]
        public GameObject somePrefab;

        [AssetsOnly]
        public Material materialAsset;

        [AssetsOnly]
        public MeshRenderer someMeshRendererOnPrefab;

        [AssetsOnly]
        public List<GameObject> prefabs;

        public override void AesirInspectorReset()
        {
            somePrefab = null;
            materialAsset = null;
            someMeshRendererOnPrefab = null;
        }
    }
}
