using UnityEditor;

namespace Runestone.AesirModules.Editor.OdinIntegration
{
    /// <summary>
    /// Object Binder 编辑器持久化设置。
    /// 使用 <see cref="ScriptableSingleton{T}" /> 在编辑器会话间持久存储 Binder 相关配置。
    /// </summary>
    public class BinderEditorSettings : ScriptableSingleton<BinderEditorSettings> { }
}
