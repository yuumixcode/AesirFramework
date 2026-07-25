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
    [Summary("玩家循环工具类，提供自定义 Unity PlayerLoop 的相关方法")]
    public static class PlayerLoopUtility
    {
        /// <summary>
        /// 从指定的循环系统中移除特定的系统
        /// </summary>
        [Summary("从指定的循环系统中移除特定的系统")]
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
        [Summary("在指定的循环系统中插入特定的系统")]
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
        [Summary("打印当前的 PlayerLoop 结构到控制台")]
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
        [Summary("获取一个新的自定义 PlayerLoopSystem 实例")]
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
