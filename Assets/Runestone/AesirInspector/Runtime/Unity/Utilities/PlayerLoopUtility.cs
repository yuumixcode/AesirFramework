using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.LowLevel;

namespace Runestone.AesirInspector
{
    /// <summary>
    /// 玩家循环工具类，提供自定义 Unity PlayerLoop 的相关方法
    /// </summary>
    public static class PlayerLoopUtility
    {
        /// <summary>
        /// 从指定的循环系统中移除特定的系统
        /// </summary>
        public static void RemoveSystem<T>(ref PlayerLoopSystem loop, PlayerLoopSystem systemToRemove)
        {
            if (loop.subSystemList == null || loop.subSystemList.Length == 0)
            {
                return;
            }

            var playerLoopSystemList = new List<PlayerLoopSystem>(loop.subSystemList);
            for (var i = 0; i < playerLoopSystemList.Count; ++i)
            {
                if (playerLoopSystemList[i].type == systemToRemove.type &&
                    playerLoopSystemList[i].updateDelegate == systemToRemove.updateDelegate)
                {
                    playerLoopSystemList.RemoveAt(i);
                    loop.subSystemList = playerLoopSystemList.ToArray();
                }
            }

            HandleSubSystemLoopForRemoval<T>(ref loop, systemToRemove);
        }

        /// <summary>
        /// 在指定的循环系统中插入特定的系统
        /// </summary>
        public static bool InsertSystem<T>(ref PlayerLoopSystem loop,
            PlayerLoopSystem systemToInsert,
            int index)
        {
            if (loop.type != typeof(T))
            {
                return HandleSubSystemLoop<T>(ref loop, systemToInsert, index);
            }

            var playerLoopSystemList = new List<PlayerLoopSystem>();
            if (loop.subSystemList != null)
            {
                playerLoopSystemList.AddRange(loop.subSystemList);
            }

            playerLoopSystemList.Insert(index, systemToInsert);
            loop.subSystemList = playerLoopSystemList.ToArray();
            return true;
        }

        /// <summary>
        /// 打印当前的 PlayerLoop 结构到控制台
        /// </summary>
        public static void PrintPlayerLoop(PlayerLoopSystem loop)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Unity Player Loop:");
            foreach (var subSystem in loop.subSystemList)
            {
                PrintSubSystem(subSystem, sb, 0);
            }

            Debug.Log(sb.ToString());
        }

        /// <summary>
        /// 获取一个新的自定义 PlayerLoopSystem 实例
        /// </summary>
        public static PlayerLoopSystem GetNewCustomPlayerLoopSystem(Type target,
            PlayerLoopSystem.UpdateFunction update = null,
            IntPtr loopCondition = default,
            PlayerLoopSystem[] subSystems = null) =>
            new PlayerLoopSystem
            {
                type = target,
                updateDelegate = update,
                loopConditionFunction = loopCondition,
                subSystemList = subSystems
            };

        #region Internal

        static void HandleSubSystemLoopForRemoval<T>(ref PlayerLoopSystem loop,
            PlayerLoopSystem systemToRemove)
        {
            if (loop.subSystemList == null || loop.subSystemList.Length == 0)
            {
                return;
            }

            for (var i = 0; i < loop.subSystemList.Length; ++i)
            {
                RemoveSystem<T>(ref loop.subSystemList[i], systemToRemove);
            }
        }

        static bool HandleSubSystemLoop<T>(ref PlayerLoopSystem loop,
            PlayerLoopSystem systemToInsert,
            int index)
        {
            if (loop.subSystemList == null || loop.subSystemList.Length == 0)
            {
                return false;
            }

            for (var i = 0; i < loop.subSystemList.Length; ++i)
            {
                if (!InsertSystem<T>(ref loop.subSystemList[i], systemToInsert, index))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        static void PrintSubSystem(PlayerLoopSystem system, StringBuilder sb, int depth)
        {
            sb.Append(' ', depth * 2).AppendLine(system.type.ToString());
            if (system.subSystemList == null || system.subSystemList.Length == 0)
            {
                return;
            }

            foreach (var subSubSystem in system.subSystemList)
            {
                PrintSubSystem(subSubSystem, sb, depth + 1);
            }
        }

        #endregion
    }
}
