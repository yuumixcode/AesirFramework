// ----------------------------------------------------------------------------
// Aesir 统一代码风格指南
//
// 本文件不参与编译，仅作为三个包（Architecture / Modules / Inspector）的代码风格参考。
// 基于 Rider 默认推荐格式，结合项目实践整理。
//
// 各包差异在对应区域标注 [Architecture/Modules] 或 [Inspector]。
// 未标注的规则为三包通用。
// ----------------------------------------------------------------------------

using System;
using UnityEngine;

namespace Runestone.CodeStyle
{
    // =========================================================================
    // 命名规范
    // =========================================================================

    /// <summary>
    /// 接口：I 前缀 + PascalCase
    /// </summary>
    public interface IDamageable<in T>
    {
        void Damage(T damageTaken);
    }

    /// <summary>
    /// 抽象类：Abstract 前缀 + PascalCase
    /// </summary>
    public abstract class AbstractWorker
    {
        // 抽象方法仍使用 PascalCase
        public abstract void Execute();
    }

    // 类名：PascalCase
    // 命名空间：Runestone.Aesir{Architecture|Modules|Inspector}
    // 包 ID：cn.runestone.aesir.{architecture|modules} / cn.runestone.aesir-inspector

    // =========================================================================
    // 字段声明顺序与命名
    // =========================================================================

    public sealed class FieldOrderExample : MonoBehaviour, IDamageable<float>
    {
        // #region 分段：允许自由使用，无最低代码量要求

        #region 字段

        // 顺序：const → static → static readonly → [SerializeField] → private

        public const int MaxCount = 100;

        public static int SharedCount;

        static readonly int ColorPropertyId = Shader.PropertyToID("_BaseColor");

        // 序列化字段：camelCase（不带下划线），每个特性独占一行
        [Header("Stats Settings")]
        [SerializeField]
        [Range(0f, 100f)]
        float healthStat;

        // 非序列化私有字段：_camelCase
        readonly int _instanceId;
        int _maxHealth;

        // 常量 / 静态只读：PascalCase

        public FieldOrderExample() => _instanceId = GetHashCode();

        #endregion

        #region 属性

        // 优先级：表达式体 → 私有 Setter → 完整属性

        public int MaxHealthReadOnly => _maxHealth;

        public int CurrentCount { get; private set; }

        public int MaxHealth
        {
            get => _maxHealth;
            set => _maxHealth = value;
        }

        #endregion

        #region 方法

        // Internal_ 前缀：仅当私有/受保护/内部方法与公开方法重名时使用
        public void SetMaxHealth(int newMaxValue) => Internal_SetMaxHealth(newMaxValue);

        void Internal_SetMaxHealth(int value) => _maxHealth = value;

        public void Damage(float damageTaken)
        {
            _maxHealth -= (int)damageTaken;
        }

        #endregion

        #region 事件

        // 事件声明：无 On 前缀（DoorOpened，而非 OnDoorOpened）
        public event Action DoorOpened;

        public event Action<CustomEventArgs> ThingHappened;

        // 触发方法：Raise + 事件名
        public void RaiseDoorOpened()
        {
            DoorOpened?.Invoke();
        }

        // 订阅方法：On + 事件名
        public void OnDoorOpened()
        {
            Debug.Log("门已打开");
        }

        // 事件参数：参数较多时用结构体整合，命名以 EventArgs 结尾
        public struct CustomEventArgs
        {
            public int ObjectId { get; }
            public Color Color { get; }

            public CustomEventArgs(int objectId, Color color)
            {
                ObjectId = objectId;
                Color = color;
            }
        }

        #endregion

        #region 空检查

        void NullCheckExample()
        {
            // 严禁对 UnityEngine.Object 及其派生类使用 ?. 或 ??
            // Unity 对象的 null 检查是自定义的（处理 C++ 层面的销毁），
            // 原生 C# 运算符会绕过这种检查。必须使用 != null 或 == null。

            if (this != null)
            {
                Debug.Log("Unity Object != null");
            }
        }

        #endregion

        #region 枚举

        /// <summary>
        /// 普通枚举：显式赋值，必须包含 None = 0
        /// </summary>
        public enum WeaponType
        {
            None = 0,
            Sword = 1,
            Bow = 2,
            Staff = 3
        }

        /// <summary>
        /// Flags 枚举：[Flags] + 位移基础值 + 按位或组合值，必须包含 None = 0
        /// </summary>
        [Flags]
        public enum AttackModes
        {
            None = 0,
            Melee = 1 << 0,
            Ranged = 1 << 1,
            Special = 1 << 2,
            MeleeAndSpecial = Melee | Special
        }

        #endregion

        #region 条件编译

#if UNITY_EDITOR
        // 编辑器专用代码使用 #if UNITY_EDITOR 条件编译
        public void Reset()
        {
            _maxHealth = 100;
        }
#endif

        #endregion
    }

    // =========================================================================
    // [Architecture/Modules] 包特有规范
    // =========================================================================

    namespace ArchitectureModules
    {
        using System;
        using UnityEngine;

        /// <summary>
        /// XML 文档注释使用中文（摘要、参数说明、备注）
        /// </summary>
        /// <remarks>
        /// 数据类标记 <c>[Serializable]</c>。
        /// 显式接口实现上下文注入（<c>IContextHolder.Context</c>、<c>ICanSetContext.SetContext</c>）。
        /// 使用 <c>ResetStaticsAssistant.Register()</c> 保障 Domain Reload 安全。
        /// </remarks>
        [Serializable]
        public class ArchitectureExample
        {
        }
    }

    // =========================================================================
    // [Inspector] 包特有规范
    // =========================================================================

    namespace Inspector
    {
        using System;
        using UnityEngine;

        /// <summary>
        /// Utility 命名约定：
        /// - Runtime 层：XxxUtility（通用工具）、XxxSafeEditorUtility（编辑器安全封装，[Conditional] 剔除）
        /// - Editor-only 层：XxxEditorUtility
        ///
        /// Odin 依赖代码必须放在 Odin Integration/ 子目录，使用独立 asmdef。
        /// 核心程序集不允许直接引用 Odin API。
        /// AttributeProcessor：internal sealed，与目标类同文件定义。
        /// </summary>
        public class InspectorExample
        {
        }
    }
}
