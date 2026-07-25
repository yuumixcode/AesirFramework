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
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Runestone.AesirInspector
{
    /// <summary>
    /// 水平横向分割线控件，用于在 Inspector 中绘制视觉分隔线。推荐使用 OnEnable 赋值。
    /// </summary>
    [Summary("水平横向分割线控件")]
    [Serializable]
    public class HorizontalSeparateControl
    {
        [Summary("深色横线高度")]
        readonly int _darkLineHeight;

        [Summary("浅色横线高度，构造函数中未设置则默认为深色横线高度 - 1")]
        readonly int _lightLineHeight;

        [Summary("分割线下方高度")]
        readonly float _spaceAfter;

        [Summary("分割线上方高度")]
        readonly float _spaceBefore;

        public HorizontalSeparateControl()
        {
            _darkLineHeight = 2;
            _lightLineHeight = _darkLineHeight - 1;
            _spaceBefore = 5;
            _spaceAfter = 5;
        }

        /// <summary>
        /// 创建自定义水平分割线控件，浅色横线高度默认为深色横线高度 - 1。
        /// </summary>
        [Summary("创建自定义水平分割线控件，浅色横线高度默认为深色横线高度 - 1。")]
        public HorizontalSeparateControl(int darkLineHeight, float spaceBefore, float spaceAfter)
        {
            _darkLineHeight = darkLineHeight;
            _lightLineHeight = _darkLineHeight - 1;
            _spaceBefore = spaceBefore;
            _spaceAfter = spaceAfter;
        }

        public HorizontalSeparateControl(int darkLineHeight,
            int lightLineHeight,
            float spaceAfter,
            float spaceBefore)
        {
            _darkLineHeight = darkLineHeight;
            _lightLineHeight = lightLineHeight;
            _spaceAfter = spaceAfter;
            _spaceBefore = spaceBefore;
        }

#if UNITY_EDITOR

        /// <summary>
        /// 深色线条颜色
        /// </summary>
        [Summary("深色线条颜色")]
        static Color DarkLineColor => EditorGUIUtility.isProSkin
            ? new Color(0.1f, 0.1f, 0.1f, 0.6f)
            : new Color(0f, 0f, 0f, 0.2f);

        /// <summary>
        /// 浅色线条颜色
        /// </summary>
        [Summary("浅色线条颜色")]
        static Color LightLineColor => EditorGUIUtility.isProSkin
            ? new Color(1f, 1f, 1f, 0.1f)
            : new Color(1f, 1f, 1f, 1f);

        public void Separate()
        {
            EditorGUILayout.BeginVertical();
            GUILayout.Space(_spaceBefore);
            float totalHeight = _darkLineHeight + _lightLineHeight;
            // 只申请一次布局空间，这保证了内部不会被插入自动间距
            var rect = EditorGUILayout.GetControlRect(false, totalHeight);
            // 基于申请到的 rect 手动计算两个子区域的坐标
            var darkRect = new Rect(rect.x, rect.y, rect.width, _darkLineHeight);
            var lightRect = new Rect(rect.x, rect.y + _darkLineHeight, rect.width, _lightLineHeight);
            EditorGUI.DrawRect(darkRect, DarkLineColor);
            EditorGUI.DrawRect(lightRect, LightLineColor);
            GUILayout.Space(_spaceAfter);
            EditorGUILayout.EndVertical();
        }

#endif
    }
}
