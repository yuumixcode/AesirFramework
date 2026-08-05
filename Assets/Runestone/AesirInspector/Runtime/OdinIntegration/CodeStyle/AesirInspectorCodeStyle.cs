// ----------------------------------------------------------------------------
// MIT License
// 
// Copyright (c) 2026 Runestone - Yuumix
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ----------------------------------------------------------------------------
// 方法与区域规范 (Methods & Regions):
// 1. Internal_ 前缀的方法名，必须是一个私有或者受保护或者内部的方法和一个公开方法重名，才能使用 Internal_ 前缀。
// Odin Inspector 规范 (Odin Inspector Integration):
// 特别注意：本文的 Odin Inspector 规范只针对 Aesir Inspector 插件。
// 1. 优先使用 Odin Attribute 来构建 UI，而非编写原始的 Editor 代码。优先选择使用 OdinAttributeProcessor 的方式去动态添加特性。
// 2. 类型名称格式化请直接使用 Sirenix.Utilities 扩展方法（type.GetNiceName() 等）；反射工具方法请使用 ReflectionUtility。
// ----------------------------------------------------------------------------

using System;
using UnityEngine;

// ReSharper disable ConvertToAutoPropertyWithPrivateSetter 
// ReSharper disable ConvertToAutoPropertyWhenPossible 
#pragma warning disable CS0067

namespace Runestone.AesirInspector
{
    /// <summary>
    /// Flags 枚举规范：必须使用 [Flags]；基础值用位移操作；复合值用按位或组合；必须包含 0 值成员。
    /// 普通枚举同理：显式赋值连续整数，必须包含 0 值成员（如 None）。
    /// </summary>
    [Flags]
    public enum AttackModes
    {
        None = 0,
        Melee = 1 << 0,                   // 1
        Ranged = 1 << 1,                  // 2
        Special = 1 << 2,                 // 4
        MeleeAndSpecial = Melee | Special // 5 (1 | 4)
    }

    /// <summary>
    /// 接口规范：I 前缀 + 帕斯卡命名法
    /// </summary>
    public interface IDamageable<in T>
    {
        void Damage(T damageTaken);
    }

    /// <summary>
    /// Aesir Inspector 代码风格示例，展示本项目的规范和最佳实践，基于 Rider 默认推荐格式
    /// </summary>
    public sealed class AesirInspectorCodeStyle : MonoBehaviour, IDamageable<float>
    {
        // 字段声明顺序：const → static → static readonly → [SerializeField] → private
        public const int MaxCount = 100;

        public static int SharedCount;

        static readonly int ColorPropertyId = Shader.PropertyToID("_BaseColor");

        // 单个特性独占一行，提升可读性
        [Header("Stats Settings")]
        [SerializeField]
        [Range(0f, 100f)]
        float anotherStat;

        // 私有字段：_camelCase 命名
        readonly int _instanceId;
        int _maxHealth;

        public AesirInspectorCodeStyle() => _instanceId = GetHashCode();

        // 属性风格：表达式体 → 私有 Setter → 完整属性
        public int MaxHealthReadOnly => _maxHealth;

        public int CurrentCount { get; private set; }

        public int MaxHealth
        {
            get => _maxHealth;
            set => _maxHealth = value;
        }

#if UNITY_EDITOR
        // 编辑器专用代码：使用条件编译确保不会被包含进构建包
        public void Reset()
        {
            _maxHealth = 100;
        }
#endif

        public void Damage(float damageTaken)
        {
            _maxHealth -= (int)damageTaken;
        }

        // Internal_ 前缀的方法名，必须是一个私有或者受保护或者内部的方法和一个公开方法重名，才能使用 Internal_ 前缀
        public void SetMaxHealth(int newMaxValue) => Internal_SetMaxHealth(newMaxValue);

        void Internal_SetMaxHealth(int value) => _maxHealth = value;

        // 事件声明
        public event Action DoorOpened;

        public event Action<CustomEventArgs> ThingHappened;

        // 事件触发方法命名：Raise + 事件名
        public void RaiseDoorOpened()
        {
            DoorOpened?.Invoke();
        }

        // 事件订阅方法命名：On + 事件名
        public void OnDoorOpened()
        {
            Debug.Log("门已打开");
        }

        void FormatExamples()
        {
            // 严禁对 UnityEngine.Object 及其派生类使用 ?. 或 ??
            // 因为 Unity 对象的 null 检查是自定义的（处理 C++ 层面的销毁），原生 C# 运算符会绕过这种检查。
            // 必须使用 != null 或 == null
            if (this != null)
            {
                Debug.Log("Unity Object != null");
            }
        }

        // 事件参数结构体：参数较多时用结构体整合
        public struct CustomEventArgs
        {
            public int ObjectID { get; }

            public Color Color { get; }

            public CustomEventArgs(int objectId, Color color)
            {
                ObjectID = objectId;
                Color = color;
            }
        }
    }
}
