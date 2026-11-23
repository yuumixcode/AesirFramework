using UnityEngine;
using UnityEngine.UIElements;

namespace Runestone.AesirModules.Editor
{
    /// <summary>
    /// 事件诊断窗口的 UI 构件helper。集中标题、行文本、空态等零散样式，
    /// 供三个 UI Toolkit 窗口复用，避免各自复制样式代码。
    /// </summary>
    internal static class EventDiagnosticsWindowHelper
    {
        public static readonly Color HeaderColor = new Color(0.22f, 0.26f, 0.32f);
        public static readonly Color MutedColor = new Color(0.62f, 0.65f, 0.70f);
        public static readonly Color OkColor = new Color(0.40f, 0.85f, 0.55f);
        public static readonly Color WarnColor = new Color(0.95f, 0.72f, 0.30f);

        /// <summary>创建带样式的表头标签。</summary>
        public static Label Header(string text) =>
            new Label(text)
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    color = Color.white,
                    backgroundColor = HeaderColor,
                    paddingLeft = 6f,
                    paddingRight = 6f,
                    paddingTop = 4f,
                    paddingBottom = 4f,
                    marginTop = 4f
                }
            };

        /// <summary>创建普通信息行。</summary>
        public static Label Row(string text, Color? color = null) =>
            new Label(text)
            {
                style =
                {
                    whiteSpace = WhiteSpace.Normal,
                    color = color ?? Color.white,
                    paddingLeft = 6f,
                    paddingTop = 2f,
                    paddingBottom = 2f,
                    fontSize = 11f
                }
            };

        /// <summary>创建空态提示（居中灰字）。</summary>
        public static Label Empty(string text) =>
            new Label(text)
            {
                style =
                {
                    color = MutedColor,
                    alignSelf = Align.Center,
                    marginTop = 24f,
                    fontSize = 12f
                }
            };
    }
}
