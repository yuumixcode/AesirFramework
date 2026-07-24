using System;
using System.Text;
using UnityEngine.LowLevel;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// PlayerLoop 操作的静态工具类，提供子系统的插入、查询与描述功能。
    /// <para>
    /// 供框架内部和外部用户扩展 PlayerLoop，不局限于 <see cref="AesirArchitectureLifeCyclePhase" /> 预定义阶段。
    /// </para>
    /// </summary>
    public static class PlayerLoopUtility
    {
        /// <summary>
        /// 在 PlayerLoop 中指定子系统前插入自定义系统
        /// </summary>
        /// <typeparam name="TTarget">目标子系统的 type 标识（如 typeof(UnityEngine.PlayerLoop.Update)）</typeparam>
        /// <param name="system">要插入的自定义子系统</param>
        /// <returns>是否成功找到目标并插入</returns>
        public static bool InsertSystemBefore<TTarget>(PlayerLoopSystem system)
        {
            var loop = PlayerLoop.GetCurrentPlayerLoop();
            if (!InsertBeforeInternal(ref loop, typeof(TTarget), system))
            {
                return false;
            }

            PlayerLoop.SetPlayerLoop(loop);
            return true;
        }

        /// <summary>
        /// 在 PlayerLoop 中指定子系统后插入自定义系统
        /// </summary>
        /// <typeparam name="TTarget">目标子系统的 type 标识（如 typeof(UnityEngine.PlayerLoop.Update)）</typeparam>
        /// <param name="system">要插入的自定义子系统</param>
        /// <returns>是否成功找到目标并插入</returns>
        public static bool InsertSystemAfter<TTarget>(PlayerLoopSystem system)
        {
            var loop = PlayerLoop.GetCurrentPlayerLoop();
            if (!InsertAfterInternal(ref loop, typeof(TTarget), system))
            {
                return false;
            }

            PlayerLoop.SetPlayerLoop(loop);
            return true;
        }

        /// <summary>
        /// 检测 PlayerLoop 中是否包含指定类型的子系统
        /// </summary>
        /// <typeparam name="TTarget">目标子系统 type 标识</typeparam>
        /// <returns>是否包含目标子系统</returns>
        public static bool ContainsSystem<TTarget>()
        {
            var loop = PlayerLoop.GetCurrentPlayerLoop();
            return ContainsSystemInternal(ref loop, typeof(TTarget));
        }

        /// <summary>
        /// 将当前 PlayerLoop 所有子系统按执行顺序输出为字符串。
        /// <para>
        /// Aesir Architecture 注入的子系统会以 [Aesir Architecture] 前缀标注。
        /// </para>
        /// </summary>
        /// <returns>格式化的 PlayerLoop 描述字符串</returns>
        public static string GetCurrentPlayerLoopDescription()
        {
            var sb = new StringBuilder();
            var loop = PlayerLoop.GetCurrentPlayerLoop();
            AppendPlayerLoopSystemDescription(loop, 0, sb);
            return sb.ToString();
        }

        static bool InsertBeforeInternal(ref PlayerLoopSystem system,
            Type targetType,
            PlayerLoopSystem newSystem)
        {
            if (system.subSystemList == null)
            {
                return false;
            }

            for (var i = 0; i < system.subSystemList.Length; i++)
            {
                if (system.subSystemList[i].type == targetType)
                {
                    var newList = new PlayerLoopSystem[system.subSystemList.Length + 1];
                    Array.Copy(system.subSystemList, 0, newList, 0, i);
                    newList[i] = newSystem;
                    Array.Copy(system.subSystemList, i, newList, i + 1, system.subSystemList.Length - i);
                    system.subSystemList = newList;
                    return true;
                }

                if (InsertBeforeInternal(ref system.subSystemList[i], targetType, newSystem))
                {
                    return true;
                }
            }

            return false;
        }

        static bool InsertAfterInternal(ref PlayerLoopSystem system,
            Type targetType,
            PlayerLoopSystem newSystem)
        {
            if (system.subSystemList == null)
            {
                return false;
            }

            for (var i = 0; i < system.subSystemList.Length; i++)
            {
                if (system.subSystemList[i].type == targetType)
                {
                    var newList = new PlayerLoopSystem[system.subSystemList.Length + 1];
                    Array.Copy(system.subSystemList, 0, newList, 0, i + 1);
                    newList[i + 1] = newSystem;
                    Array.Copy(system.subSystemList, i + 1, newList, i + 2,
                        system.subSystemList.Length - i - 1);
                    system.subSystemList = newList;
                    return true;
                }

                if (InsertAfterInternal(ref system.subSystemList[i], targetType, newSystem))
                {
                    return true;
                }
            }

            return false;
        }

        static bool ContainsSystemInternal(ref PlayerLoopSystem system, Type targetType)
        {
            if (system.type == targetType)
            {
                return true;
            }

            if (system.subSystemList == null)
            {
                return false;
            }

            for (var i = 0; i < system.subSystemList.Length; i++)
            {
                if (ContainsSystemInternal(ref system.subSystemList[i], targetType))
                {
                    return true;
                }
            }

            return false;
        }

        static void AppendPlayerLoopSystemDescription(PlayerLoopSystem system, int depth, StringBuilder sb)
        {
            if (system.type != null)
            {
                var indent = new string(' ', depth * 4);
                var isAesir = system.type.Name.StartsWith("AesirArchitecture");
                var name = system.type.Name;
                var prefix = isAesir ? "[Aesir Architecture] " : string.Empty;
                sb.AppendLine($"{indent}{prefix}{name}");
            }

            if (system.subSystemList == null)
            {
                return;
            }

            for (var i = 0; i < system.subSystemList.Length; i++)
            {
                AppendPlayerLoopSystemDescription(system.subSystemList[i], depth + 1, sb);
            }
        }
    }
}
