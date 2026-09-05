using UnityEngine;
using UnityEngine.InputSystem.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Runestone.AesirModules
{
    /// <summary>
    /// 将 UIRoot 的输入模块替换为 InputSystemUIInputModule。
    /// 此类位于 Runestone.AesirModules.InputSystem 程序集中，
    /// 仅当 Player Settings 启用 Input System（ENABLE_INPUT_SYSTEM）时该程序集才会编译。
    /// 编辑器阶段通过 [InitializeOnLoad] 静态构造函数注册，确保菜单创建 UIRoot 时生效。
    /// 运行时通过 [RuntimeInitializeOnLoadMethod] 注册，确保运行时兜底创建 UIRoot 时生效。
    /// </summary>
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    internal static class InputSystemModuleHook
    {
        static InputSystemModuleHook()
        {
            Register();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void RuntimeInitialize()
        {
            Register();
        }

        static void Register()
        {
            UIRoot.CreateInputModule = go =>
            {
                go.AddComponent<InputSystemUIInputModule>();
            };
        }
    }
}
