using UnityEngine;

namespace Runestone.AesirModules
{
    /// <summary>
    /// <see cref="AesirEventArgs" /> 的 ScriptableObject 包装，让事件可保存为 <c>.asset</c> 资源，
    /// 在 Inspector 中配置载荷并由非程序员触发。
    /// <para>
    /// 通过 Project 右键菜单 <c>Create → Aesir → Event Module → AesirEventArgsSO</c> 创建；
    /// 事件参数字段经 <see cref="SubclassSelectorAttribute" /> 下拉选择具体子类并配置其字段。
    /// </para>
    /// <para>
    /// 触发方式：<see cref="Raise" />（代码 / Inspector 运行时按钮）、
    /// 或经其他 Inspector 可绑定入口（如 UnityEvent 调用）触发。
    /// 触发时以本资产为发布者（<see cref="AesirEventArgs.Sender" />）。
    /// </para>
    /// <para>
    /// 注意：Raise 发布的是资产内配置的同一个参数实例，订阅者不应修改事件参数载荷；
    /// 参数实例上经 <see cref="AesirEventArgs.WithFilter" /> 添加的过滤器会跨次触发保留。
    /// </para>
    /// </summary>
    [CreateAssetMenu(menuName = "Aesir/Event Module/AesirEventArgsSO", fileName = "NewAesirEventArgsSO")]
    public class AesirEventArgsSO : ScriptableObject
    {
        /// <summary>
        /// 配置的事件参数实例。经 [SerializeReference] + <see cref="SubclassSelectorAttribute" />
        /// 在 Inspector 中选择具体子类；未选择时为 null。
        /// </summary>
        [SerializeReference]
        [SubclassSelector]
        AesirEventArgs eventArgs;

        /// <summary>
        /// 配置的事件参数实例（只读）。未选择类型时为 null。
        /// </summary>
        public AesirEventArgs EventArgs => eventArgs;

        /// <summary>
        /// 以本资产为发布者触发事件。
        /// 未配置事件参数时输出错误日志并跳过。
        /// </summary>
        public void Raise()
        {
            if (eventArgs == null)
            {
                AesirModulesDebug.LogError(AesirModulesDebug.EventModuleTag,
                    $"AesirEventArgsSO「{name}」未配置事件参数，无法触发。");
                return;
            }

            eventArgs.Invoke(this);
        }
    }
}
