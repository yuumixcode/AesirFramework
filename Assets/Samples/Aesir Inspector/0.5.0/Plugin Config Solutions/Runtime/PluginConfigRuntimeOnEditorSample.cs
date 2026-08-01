using Runestone.AesirInspector.OdinIntegration;
using UnityEngine;
#if UNITY_EDITOR
#endif

namespace Runestone.AesirInspector.Samples.PluginConfig
{
    [Summary("在编辑器阶段使用且可以被 Runtime 程序集读取，构建后不使用的配置类案例")]
    public class PluginConfigRuntimeOnEditorSample : ScriptableObject
    {
        static PluginConfigRuntimeOnEditorSample _instance;

        [BilingualTitle("可配置数据", "Configurable Data")]
        public string runtimeName = "Runtime Name";

        static string ConfigName =>
            OdinBridgeLocator.Bridge.GetFriendlyFullName(typeof(PluginConfigRuntimeOnEditorSample));

        public static PluginConfigRuntimeOnEditorSample Instance
        {
            get
            {
#if UNITY_EDITOR
                return ScriptableObjectSafeEditorUtility
                    .GetOrCreateEditorScriptableObject<PluginConfigRuntimeOnEditorSample>(ConfigName,
                        AesirInspectorPaths.EditorDefaultResourcesPath + "/Samples",
                        nameof(PluginConfigRuntimeOnEditorSample));
#else
                return null;
#endif
            }
        }

        [BilingualButton("重置配置", "Reset Config")]
        public void ResetConfig()
        {
            runtimeName = "Runtime Name";
        }

        [BilingualTitle("调试", "Debug")]
        [BilingualButton("选择项目中的配置资产", "Select Config Asset In Project")]
        public void PingAsset()
        {
#if UNITY_EDITOR
            ProjectSafeEditorUtility.PingAndSelectAsset(AesirInspectorPaths.EditorDefaultResourcesPath +
                                                        "/Samples/" +
                                                        nameof(PluginConfigRuntimeOnEditorSample) + ".asset");
#endif
        }
    }
}
