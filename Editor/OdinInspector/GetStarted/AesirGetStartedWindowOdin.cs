using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// Aesir Getting Started 窗口（Odin Inspector 版）— 框架示例导航主入口：概览页以包卡片展示本机安装的
    /// Aesir 包（未安装的已知包显示占位引导），包页按教学分组列出全部示例，点击卡片在 Project 窗口选中
    /// 示例文件夹、有场景的示例经「打开场景」按钮直达场景，动作结果经右下角 Toast 提示。
    /// <para>
    /// 结构与动效参照 Odin Inspector 自带 Getting Started 窗口：页面栈导航（顶部面包屑 + 底部返回）、
    /// 概览区随页面进出垂直收起为常驻条带（点击条带卡片可在包之间水平滑动切换）、页面内容入场渐显；
    /// 绘制全部使用 SirenixEditorGUI / SirenixGUIStyles / SdfIcons 官方基础设施，样式静态懒加载，
    /// OnGUI 期间零分配（显示文本均在扫描 / 进页时预计算）。
    /// </para>
    /// <para>
    /// 数据层与 IMGUI 兜底窗口共用 <see cref="AesirGetStartedService" />；菜单入口为
    /// <see cref="AesirGetStartedWindow" />，编辑器加载时经 [InitializeOnLoadMethod] 把打开方式注册进
    /// 其 OdinWindowOpener 委托——未安装 Odin Inspector 时本程序集整体不参与编译，菜单自动落回兜底窗口。
    /// </para>
    /// </summary>
    public class AesirGetStartedWindowOdin : OdinEditorWindow
    {
        #region 控件（照 Odin GettingStartedWindow 的官方状态机按钮）

        /// <summary>带 SDF 图标的 footer 按钮（hotControl 状态机：MouseDown 捕获 / MouseUp 命中才触发，支持键盘空格）。</summary>
        static bool Button(ref Rect rect,
            string text,
            SdfIconType icon,
            Direction takeDirection,
            Direction iconDirection)
        {
            if (Event.current.type == EventType.Layout)
            {
                return false;
            }

            var btnStyle = GUI.skin.button;
            var textStyle = SirenixGUIStyles.WhiteLabel;
            var content = GUIHelper.TempContent(text);
            var iconWidth = rect.height;
            var textWidth = textStyle.CalcSize(content).x;
            var btnWidth = textWidth + 5f + 20f + iconWidth;

            var r = rect.TakeFromDir(btnWidth, takeDirection);
            var btnRect = r;
            var id = GUIUtility.GetControlID(21345155, FocusType.Passive, rect);

            r.TakeFromDir(5f, iconDirection);
            var iconRect = r.TakeFromDir(iconWidth, iconDirection).AlignCenterY(r.height * 0.4f);
            r.TakeFromDir(0f, iconDirection);
            var textRect = r.TakeFromDir(textWidth, iconDirection);

            var current = Event.current;
            var hover = btnRect.Contains(Event.current.mousePosition);
            switch (current.type)
            {
                case EventType.Repaint:
                    btnStyle.Draw(btnRect, GUIContent.none, id, GUIUtility.hotControl == id, hover);
                    SdfIcons.DrawIcon(iconRect, icon, textStyle.normal.textColor);
                    GUI.Label(textRect, content, textStyle);
                    break;

                case EventType.MouseDown:
                    if (hover)
                    {
                        GUIUtility.hotControl = id;
                        current.Use();
                    }

                    break;

                case EventType.KeyDown:
                {
                    var modifierHeld = current.alt || current.shift || current.command || current.control;
                    if ((current.keyCode == KeyCode.Space || current.keyCode == KeyCode.Return ||
                         current.keyCode == KeyCode.KeypadEnter) && !modifierHeld &&
                        GUIUtility.keyboardControl == id)
                    {
                        current.Use();
                        GUI.changed = true;
                        return true;
                    }

                    break;
                }

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == id)
                    {
                        GUIHelper.RemoveFocusControl();
                        current.Use();
                        if (hover)
                        {
                            GUI.changed = true;
                            return true;
                        }
                    }

                    break;

                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == id)
                    {
                        current.Use();
                    }

                    break;
            }

            return false;
        }

        #endregion

        #region 概览卡片视图模型

        /// <summary>概览卡片视图模型（扫描时重建；显示文本预计算，OnGUI 零拼接）。</summary>
        class PackageCard
        {
            public string Description;

            public string DisplayName;
            public AesirGetStartedService.AesirPackageInfo Installed;

            /// <summary>状态行文本（扫描时预计算：已安装 · v0.23.0 / 未安装）。</summary>
            public string StatusText;
        }

        #endregion

        #region 常量

        const string WindowTitle = "Aesir Getting Started Window";

        /// <summary>在线文档站地址（工具条链接）。</summary>
        const string DocsUrl = "https://yuumixcode.github.io/AesirFramework-Docs/";

        /// <summary>GitHub 仓库地址。</summary>
        const string GitHubUrl = "https://github.com/yuumixcode/AesirFramework";

        /// <summary>包内更新器菜单路径（工具条「检查更新」路由，随更新器自身的 Odin 路由打开对应窗口）。</summary>
        const string UpdateMenuPath = "Tools/Aesir/Check for Updates";

        /// <summary>概览收起后的常驻条带高度（页面打开时仍可点击卡片在包之间切换）。</summary>
        const float OverviewStripHeight = 108f;

        /// <summary>滑动动画速度（MoveTowards 步进速率，单位：进度/秒）。</summary>
        const float SlideSpeed = 3f;

        /// <summary>页面 footer 高度。</summary>
        const float FooterHeight = 44f;

        /// <summary>Toast 显示时长（秒）。</summary>
        const float ToastDurationSeconds = 3f;

        /// <summary>同时显示的 Toast 数量上限（新 Toast 入队前把最早弹出的一条直接关闭，避免右下角持续堆叠）。</summary>
        const int MaxToasts = 3;

        #endregion

        #region 打开方式注册

        /// <summary>编辑器加载时把 Odin 窗口打开方式注册给菜单入口（域重载后静态委托清空，每次重载重新注册）。</summary>
        [InitializeOnLoadMethod]
        static void RegisterOpener() => AesirGetStartedWindow.RegisterOdinWindowOpener(OpenWindow);

        /// <summary>打开窗口（菜单路由到此）。</summary>
        public static void OpenWindow()
        {
            var window = GetWindow<AesirGetStartedWindowOdin>();
            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = new Vector2(760, 520);
            // 默认尺寸对齐 Odin GettingStartedWindow 的 900×900（近方形，概览三列卡片满高展开）
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(900, 900);
            window.Show();
        }

        #endregion

        #region 状态

        /// <summary>页面栈（面包屑导航；非序列化，域重载后回到概览）。</summary>
        [NonSerialized]
        readonly List<GetStartedPage> _pages = new List<GetStartedPage>();

        /// <summary>概览卡片视图模型（扫描时重建：已知包与安装实例的配对）。</summary>
        [NonSerialized]
        List<PackageCard> _cards = new List<PackageCard>();

        /// <summary>工具条版本号文本（扫描时预计算，OnGUI 零拼接）。</summary>
        [NonSerialized]
        string _versionLabel = "Aesir";

        // ── 滑动动画（照 Odin GettingStartedWindow：raw 序列化跨域重载，显示 T 每帧由 smoothstep² 现算）──
        // VerticalSlideT：1 = 概览完全展开，0 = 收起为条带；HorizontalSlideT：0→1 页面间水平过渡进度
        [SerializeField]
        float _verticalSlideRaw = 1f;

        [SerializeField]
        float _horizontalSlideRaw;

        [NonSerialized]
        GetStartedPage _slideToPage;

        [NonSerialized]
        GetStartedPage _slideFromPage;

        // 水平滑动方向（+1 新包在右 / -1 新包在左）——单页栈替换语义下页数不变，
        // 由 EnterPackagePage 按卡片序号显式判定，不再从页数变化推导
        [NonSerialized]
        int _horizontalSlideDirection = 1;

        public float VerticalSlideT { get; private set; } = 1f;
        public float HorizontalSlideT { get; private set; }

        /// <summary>Toast 动画活跃期截止时刻（<see cref="EditorApplication.timeSinceStartup" /> 秒）。</summary>
        [NonSerialized]
        double _toastDriveDeadline;

        #endregion

        #region 生命周期

        protected override void OnEnable()
        {
            base.OnEnable();
            wantsMouseMove = true;                       // 卡片 hover 高亮需要 MouseMove 事件
            WindowPadding = new Vector4(0f, 0f, 0f, 0f); // 全出血分区（工具条 / 条带 / 页面自绘）
            Scan();
        }

        /// <summary>
        /// 编辑器 tick 驱动：滑动动画与 Toast 活跃期间主动请求窗口重绘。IMGUI 窗口默认按需重绘
        /// ——鼠标静止时无事件、无 OnGUI，滑动动画与 Toast 的时长进度条（两者都只在 OnGUI 帧推进）会走走停停；
        /// Sirenix 的 GUIHelper.RequestRepaint 仅置静态标志、无实际重绘驱动，无法依赖。Repaint 请求会让
        /// 编辑器对窗口保持连续重绘，动画即平滑；动画结束（T 到端点、Toast 过期）后停止请求，不空转。
        /// EditorWindow.Update 为消息方法（非 virtual），由编辑器对可见窗口持续调用。
        /// </summary>
        protected void Update()
        {
            var verticalSliding = VerticalSlideT > 0.001f && VerticalSlideT < 0.999f;
            var horizontalSliding = HorizontalSlideT > 0f;
            if (verticalSliding || horizontalSliding ||
                EditorApplication.timeSinceStartup < _toastDriveDeadline)
            {
                Repaint();
            }
        }

        /// <summary>OdinEditorWindow.toasts 私有字段缓存（Odin 未公开 Toast 队列 API，上限机制须经反射访问）。</summary>
        static FieldInfo _toastsField;

        /// <summary>弹右下角 Toast：先按 <see cref="MaxToasts" /> 收敛队列，再入队并记录动画活跃期（时长 + 0.5s 淡出 + 滑入余量）。</summary>
        void ShowBottomRightToast(SdfIconType icon, string message)
        {
            DismissOldestToastsOverLimit();
            ShowToast(ToastPosition.BottomRight, icon, message, Color.green, ToastDurationSeconds);
            _toastDriveDeadline = EditorApplication.timeSinceStartup + ToastDurationSeconds + 1.5f;
        }

        /// <summary>
        /// 关闭超出 <see cref="MaxToasts" /> 的最早 Toast：toasts 列表新的在前、旧的在后，末尾即最早。
        /// 置 TimePassed 到已完全过期（与 HandleToastInput 点击关闭同语义）后必须同步 <c>RemoveAt</c>——
        /// Odin 的 alpha==0 自动移除发生在下一帧绘制，同步循环里 Count 不会变，缺了 RemoveAt 即死循环卡死编辑器。
        /// </summary>
        void DismissOldestToastsOverLimit()
        {
            if (_toastsField == null)
            {
                _toastsField = typeof(OdinEditorWindow).GetField("toasts",
                    BindingFlags.Instance | BindingFlags.NonPublic);
            }

            if (!(_toastsField?.GetValue(this) is IList toasts))
            {
                return;
            }

            while (toasts.Count >= MaxToasts)
            {
                var oldest = toasts[toasts.Count - 1];
                var toastType = oldest.GetType();
                var expiry = (float)toastType.GetField("ExpiryTime").GetValue(oldest);
                toastType.GetField("TimePassed").SetValue(oldest, expiry + 0.5f);
                toasts.RemoveAt(toasts.Count - 1);
            }
        }

        /// <summary>扫描包与示例并重建卡片视图模型（OnGUI 期间只读消费）。</summary>
        void Scan()
        {
            var packages = AesirGetStartedService.ScanPackages();

            _cards = new List<PackageCard>();
            // 已知包逐一配对安装实例（未安装 → 占位卡片引导去 Releases）
            foreach (var known in AesirGetStartedService.KnownPackages)
            {
                _cards.Add(new PackageCard
                {
                    DisplayName = known.DisplayName,
                    Description = known.Description,
                    Installed = packages.Find(p => p.Id == known.Id)
                });
            }

            // 未登记进 KnownPackages 的新包兜底追加（保证新包示例也可导航）
            foreach (var pkg in packages)
            {
                if (!_cards.Exists(c => c.Installed != null && c.Installed.Id == pkg.Id))
                {
                    _cards.Add(new PackageCard
                    {
                        DisplayName = pkg.DisplayName,
                        Description = pkg.Description,
                        Installed = pkg
                    });
                }
            }

            var architecture = packages.Find(p => p.Id.EndsWith("architecture", StringComparison.Ordinal));
            _versionLabel = architecture != null ? $"Aesir v{architecture.Version}" : "Aesir";

            // 状态行文本预计算（DrawPackageCard 每帧直接消费，OnGUI 零字符串拼接）
            foreach (var card in _cards)
            {
                card.StatusText = BuildStatusText(card.Installed);
            }
        }

        /// <summary>包卡片状态行文本（已安装附版本号；未安装为引导文案）。</summary>
        static string BuildStatusText(AesirGetStartedService.AesirPackageInfo installed) =>
            installed == null ? "未安装" : $"已安装 · v{installed.Version}";

        #endregion

        #region 主绘制流程

        protected override void OnImGUI()
        {
            if (Event.current.type == EventType.MouseMove)
            {
                GUIHelper.RequestRepaint(); // hover 高亮依赖
            }

            UpdateThings();

            var rect = position.ResetPosition();
            DrawToolbar(rect.TakeFromTop(EditorStyles.toolbarButton.fixedHeight + 4f));
            DrawOverviewSelector(ref rect);
            DrawPageStack(ref rect);

            this.RepaintIfRequested();
        }

        /// <summary>页面栈推进与滑动动画（照 Odin：Layout 事件推进 raw 进度，smoothstep² 求显示 T，未到位才请求重绘）。</summary>
        void UpdateThings()
        {
            if (Event.current.type != EventType.Layout)
            {
                return;
            }

            _slideToPage = _pages.Count == 0 ? null : _pages[_pages.Count - 1];

            var slideVertically = false;
            var slideHorizontally = false;
            if (_slideFromPage != _slideToPage)
            {
                if (_slideToPage == null || _slideFromPage == null)
                {
                    slideVertically = true;
                }
                else
                {
                    slideHorizontally = true;
                }
            }

            if (slideVertically)
            {
                var targetT = Mathf.MoveTowards(_verticalSlideRaw, _slideToPage == null ? 1f : 0f,
                    GUITimeHelper.LayoutDeltaTime * SlideSpeed);
                if (targetT != _verticalSlideRaw)
                {
                    _verticalSlideRaw = targetT;
                    GUIHelper.RequestRepaint();
                }
                else
                {
                    _slideFromPage = _slideToPage;
                }

                VerticalSlideT = Smoothstep(Smoothstep(_verticalSlideRaw));
            }

            if (slideHorizontally)
            {
                var targetT = Mathf.MoveTowards(_horizontalSlideRaw, 1f,
                    GUITimeHelper.LayoutDeltaTime * SlideSpeed);
                if (targetT != _horizontalSlideRaw)
                {
                    _horizontalSlideRaw = targetT;
                    GUIHelper.RequestRepaint();
                }
                else
                {
                    _slideFromPage = _slideToPage;
                    _horizontalSlideRaw = 1f;
                }

                HorizontalSlideT = Smoothstep(Smoothstep(_horizontalSlideRaw));
            }
            else
            {
                _horizontalSlideRaw = 0f;
                HorizontalSlideT = 0f;
            }
        }

        static float Smoothstep(float t) => t * t * (3f - 2f * t);

        #endregion

        #region 工具条（面包屑 + 外链）

        void DrawToolbar(Rect rect)
        {
            EditorGUI.DrawRect(rect.TakeFromBottom(2f), SirenixGUIStyles.BorderColor);
            rect = rect.AlignCenterY(EditorGUIUtility.singleLineHeight);

            // 右侧链接区（从右往左取）
            if (ToolbarButton(ref rect, SdfIconType.Github, "GitHub", false, null))
            {
                Application.OpenURL(GitHubUrl);
            }

            if (ToolbarButton(ref rect, SdfIconType.Globe, "文档", false, null))
            {
                Application.OpenURL(DocsUrl);
            }

            if (ToolbarButton(ref rect, SdfIconType.CloudArrowDownFill, "检查更新", false, null))
            {
                EditorApplication.ExecuteMenuItem(UpdateMenuPath);
            }

            if (ToolbarButton(ref rect, SdfIconType.None, _versionLabel, false,
                    SirenixGUIStyles.CenteredGreyMiniLabel))
            {
                Application.OpenURL(AesirUpdateService.LatestReleasePageUrl);
            }

            // 左侧面包屑：概览 + 页面栈
            if (ToolbarButtonFromLeft(ref rect, "概览", _pages.Count == 0, EditorStyles.toolbarButton))
            {
                _pages.Clear();
            }

            for (var i = 0; i < _pages.Count; i++)
            {
                var p = _pages[i];
                if (ToolbarButtonFromLeft(ref rect, p.Title, i == _pages.Count - 1,
                        EditorStyles.toolbarButton))
                {
                    _pages.SetLength(i + 1);
                    break;
                }
            }
        }

        /// <summary>工具条文字按钮（带可选 SDF 图标；照 Odin GettingStartedWindow 同名方法）。</summary>
        static bool ToolbarButton(ref Rect rect,
            SdfIconType icon,
            string text,
            bool fromLeft,
            GUIStyle textLabelStyle)
        {
            var textStyle = textLabelStyle ?? SirenixGUIStyles.Label;
            var content = GUIHelper.TempContent(text);
            var iconWidth = icon == SdfIconType.None ? 0f : rect.height;
            var iconPadding = icon == SdfIconType.None ? 0 : 5;
            const int btnPadding = 5;
            var textWidth = textStyle.CalcSize(content).x;
            var btnWidth = textWidth + iconPadding + btnPadding * 2 + iconWidth;

            var r = fromLeft ? rect.TakeFromLeft(btnWidth) : rect.TakeFromRight(btnWidth);
            var clicked = GUI.Button(r, GUIContent.none, EditorStyles.toolbarButton);
            r.TakeFromLeft(btnPadding);
            var iconRect = r.TakeFromLeft(iconWidth);
            r.TakeFromLeft(iconPadding);
            var textRect = r.TakeFromLeft(textWidth);
            if (icon != SdfIconType.None)
            {
                SdfIcons.DrawIcon(iconRect.AlignCenterY(rect.height - 4f), icon, textStyle.normal.textColor);
            }

            GUI.Label(textRect, content, textStyle);
            return clicked;
        }

        /// <summary>工具条面包屑按钮（整宽自适应 + 按下态；照 Odin GettingStartedWindow 同名方法）。</summary>
        static bool ToolbarButtonFromLeft(ref Rect rect, string text, bool isOn, GUIStyle style)
        {
            var content = GUIHelper.TempContent(text);
            var textWidth = style.CalcSize(content).x;
            const int btnPadding = 5;
            var btnWidth = textWidth + btnPadding * 2;

            var btnRect = rect.TakeFromLeft(btnWidth);
            var hover = btnRect.Contains(Event.current.mousePosition);
            if (Event.current.type == EventType.Repaint)
            {
                style.Draw(btnRect, content, false, hover, isOn, false);
            }

            return GUI.Button(btnRect, GUIContent.none, GUIStyle.none);
        }

        #endregion

        #region 概览（包卡片 ↔ 常驻条带）

        /// <summary>已安装状态行的绿色（对标 Odin 产品卡片 Installed 绿勾）。</summary>
        static readonly Color InstalledGreenColor = new Color(0.36f, 0.72f, 0.4f);

        /// <summary>
        /// 概览区：卡片无边框、无内边距，直接充满整个窗口（对标 Odin 满铺产品卡），
        /// 卡片之间仅以细分隔线区分；页面打开时垂直收起为顶部条带（常驻、可点击切换包）。
        /// </summary>
        void DrawOverviewSelector(ref Rect totalRect)
        {
            var selectorRect = totalRect.TakeFromTop(
                Mathf.Lerp(totalRect.height, OverviewStripHeight, 1f - VerticalSlideT));

            EditorGUI.DrawRect(selectorRect, SirenixGUIStyles.DarkEditorBackground);
            EditorGUI.DrawRect(selectorRect.AlignBottom(2f), SirenixGUIStyles.BorderColor);

            for (var i = 0; i < _cards.Count; i++)
                DrawPackageCard(selectorRect.Split(i, _cards.Count), _cards[i]);

            // 卡片之间的细分隔线（首尾不画，对标 Odin 竖向分隔线）
            for (var i = 1; i < _cards.Count; i++)
            {
                var x = selectorRect.xMin + selectorRect.width * (i / (float)_cards.Count);
                EditorGUI.DrawRect(Rect.MinMaxRect(x - 0.5f, selectorRect.yMin, x + 0.5f, selectorRect.yMax),
                    SirenixGUIStyles.BorderColor);
            }
        }

        /// <summary>
        /// 包卡片：概览态自上而下为 一级标题（大号）→ 描述（大一档）→ 安装状态行（绿勾）→ 分隔线 →
        /// 「开始使用」按钮——概览态仅按钮可点击（整卡不可点）；条带态压缩为 包名（大号）+ 状态行（中号）
        /// 纵向两行，整卡可点击（快速切换包）。两态随 VerticalSlideT 交叉淡入淡出。
        /// </summary>
        void DrawPackageCard(Rect rect, PackageCard card)
        {
            var installed = card.Installed;
            // 按包 Id 值比较判定选中（页面持有的包实例可能来自不同批次扫描，引用比较不可靠）
            var selected = installed != null && _pages.Count > 0 &&
                           _pages[_pages.Count - 1] is PackagePage top && top.Package.Id == installed.Id;

            // 无边框满铺：卡片不画底色与描边，直接融于概览背景（仅选中顶条做区分）
            if (selected)
            {
                EditorGUI.DrawRect(rect.AlignTop(2f), SirenixGUIStyles.HighlightedTextColor);
            }

            var prevColor = GUI.color;
            // 未安装整卡半显；两态内容在此基础上交叉淡入淡出
            var baseColor = installed == null ? prevColor * new Color(1f, 1f, 1f, 0.5f) : prevColor;
            GUI.color = baseColor;

            // 条带态（页面打开、概览收起过半）才提供整卡 hover 高亮——概览态整卡不可点，无 hover
            var stripMode = VerticalSlideT < 0.5f;
            var hover = installed != null && stripMode && rect.Contains(Event.current.mousePosition);
            if (hover)
            {
                EditorGUI.DrawRect(rect, SirenixGUIStyles.MouseOverBgOverlayColor);
            }

            // 概览态内容（条带态随 VerticalSlideT 渐隐）
            if (VerticalSlideT > 0.01f)
            {
                GUI.color = baseColor * new Color(1f, 1f, 1f, VerticalSlideT);
                DrawOverviewCardContent(rect, card, installed);
                GUI.color = baseColor;
            }

            // 条带态内容（概览态随 1 - VerticalSlideT 渐隐）
            if (VerticalSlideT < 0.99f)
            {
                GUI.color = baseColor * new Color(1f, 1f, 1f, 1f - VerticalSlideT);
                DrawStripCardContent(rect, card, installed);
                GUI.color = baseColor;
            }

            GUI.color = prevColor;

            // 条带态整卡点击：快速切换包（概览态仅「开始使用」按钮可点，不响应整卡）
            if (stripMode && GUI.Button(rect, GUIContent.none, GUIStyle.none))
            {
                if (installed != null)
                {
                    EnterPackagePage(installed);
                }
                else
                {
                    Application.OpenURL(AesirUpdateService.ReleasesPageUrl);
                }
            }
        }

        /// <summary>状态条带底色（比卡片底色略亮的半透明覆盖，对标 Odin 状态行条带）。</summary>
        static readonly Color StatusBandOverlayColor = new Color(1f, 1f, 1f, 0.035f);

        static GUIStyle _overviewTitleStyle;

        /// <summary>概览卡一级标题样式（大号居中加粗，撑起满高卡片的标题空间）。</summary>
        static GUIStyle OverviewTitleStyle =>
            _overviewTitleStyle ??= new GUIStyle(SirenixGUIStyles.BoldTitleCentered) { fontSize = 26 };

        static GUIStyle _overviewDescStyle;

        /// <summary>概览卡描述样式（比常规正文大一档，居中换行）。</summary>
        static GUIStyle OverviewDescStyle =>
            _overviewDescStyle ??= new GUIStyle(SirenixGUIStyles.MultiLineCenteredLabel) { fontSize = 15 };

        /// <summary>
        /// 概览态卡片内容（对标 Odin 满高产品卡的文本层级）：中部为 一级标题（大号）→ 描述（大一档，
        /// 整体垂直居中），底部为 状态行条带 → 分隔线 → 90% 宽「开始使用」按钮 → 底部留白（三段式）。
        /// </summary>
        void DrawOverviewCardContent(Rect rect,
            PackageCard card,
            AesirGetStartedService.AesirPackageInfo installed)
        {
            var sidePadding = rect.width * 0.05f;

            // 底部自下而上：底留白 → 按钮行（左右各 5% 留白）→ 间隔区（居中分隔线）→ 状态条带
            rect.TakeFromBottom(14f);
            var buttonRow = rect.TakeFromBottom(34f).HorizontalPadding(sidePadding);
            var gapRow = rect.TakeFromBottom(24f);
            EditorGUI.DrawRect(gapRow.AlignCenterY(1f), SirenixGUIStyles.BorderColor);
            var statusRow = rect.TakeFromBottom(46f);
            EditorGUI.DrawRect(statusRow, StatusBandOverlayColor);
            var statusContent = statusRow.Padding(sidePadding, 0f);

            // 中部：一级标题 → 描述，大字号、整体垂直居中
            var descHeight = 0f;
            if (!string.IsNullOrEmpty(card.Description))
            {
                descHeight =
                    OverviewDescStyle.CalcHeight(GUIHelper.TempContent(card.Description), rect.width - 72f) +
                    14f;
            }

            var blockHeight = Mathf.Min(46f + descHeight, rect.height);
            var blockRect = rect.AlignCenterY(blockHeight);

            var titleRect = blockRect.TakeFromTop(46f);
            GUI.Label(titleRect, card.DisplayName, OverviewTitleStyle);

            if (descHeight > 0f)
            {
                GUI.Label(blockRect.Padding(36f, 0f), card.Description, OverviewDescStyle);
            }

            // 状态行：绿勾圆图标 + 文字（未安装为灰圆叉），垂直居中于条带
            var statusIconRect = statusContent.TakeFromLeft(20f).AlignCenterY(16f);
            if (installed != null)
            {
                SdfIcons.DrawIcon(statusIconRect, SdfIconType.CheckCircleFill, InstalledGreenColor);
            }
            else
            {
                SdfIcons.DrawIcon(statusIconRect, SdfIconType.XCircleFill,
                    SirenixGUIStyles.Label.normal.textColor);
            }

            GUI.Label(statusContent, card.StatusText, SirenixGUIStyles.BoldLabel);

            // 底部按钮（概览态唯一可点击入口，对标 Odin 产品卡片的 Get Started 按钮：90% 宽、底边留白）
            if (GUI.Button(buttonRow, installed != null ? "开始使用" : "前往 GitHub 下载", SirenixGUIStyles.Button))
            {
                if (installed != null)
                {
                    EnterPackagePage(installed);
                }
                else
                {
                    Application.OpenURL(AesirUpdateService.ReleasesPageUrl);
                }
            }
        }

        static GUIStyle _stripTitleStyle;

        /// <summary>条带卡包名样式（大号加粗左对齐，约为常规 BoldLabel 的两倍）。</summary>
        static GUIStyle StripTitleStyle =>
            _stripTitleStyle ??= new GUIStyle(SirenixGUIStyles.BoldLabel) { fontSize = 24 };

        static GUIStyle _stripStatusStyle;

        /// <summary>条带卡状态行样式（中号左对齐，约为 MiniLabel 的两倍）。</summary>
        static GUIStyle StripStatusStyle =>
            _stripStatusStyle ??= new GUIStyle(SirenixGUIStyles.Label) { fontSize = 18 };

        /// <summary>
        /// 条带态卡片内容：包名（大号）在上、状态行（绿勾 + 版本，中号）在下，
        /// 左对齐纵向两行、上下留白均衡（不贴底、无图标）。
        /// </summary>
        void DrawStripCardContent(Rect rect,
            PackageCard card,
            AesirGetStartedService.AesirPackageInfo installed)
        {
            var contentRect = rect.Padding(20f, 14f);

            var nameRect = contentRect.TakeFromTop(38f);
            GUI.Label(nameRect, card.DisplayName, StripTitleStyle);

            contentRect.TakeFromTop(6f);
            var statusRow = contentRect.TakeFromTop(30f);

            var statusIconRect = statusRow.TakeFromLeft(22f).AlignCenterY(20f);
            if (installed != null)
            {
                SdfIcons.DrawIcon(statusIconRect, SdfIconType.CheckCircleFill, InstalledGreenColor);
            }
            else
            {
                SdfIcons.DrawIcon(statusIconRect, SdfIconType.XCircleFill,
                    SirenixGUIStyles.Label.normal.textColor);
            }

            statusRow.TakeFromLeft(8f);
            GUI.Label(statusRow, card.StatusText, StripStatusStyle);
        }

        /// <summary>
        /// 进入包页——单页栈替换语义：栈顶已是同包页时不重复进入（按包 Id 值比较），
        /// 切换包则替换栈内容（面包屑恒为「概览 + 当前包」，不会随点击增长）；
        /// 水平滑动方向按卡片序号显式判定（新包在右 → +1 向左推出，反之 -1）。
        /// </summary>
        void EnterPackagePage(AesirGetStartedService.AesirPackageInfo pkg)
        {
            if (_pages.Count > 0 && _pages[_pages.Count - 1] is PackagePage top && top.Package.Id == pkg.Id)
            {
                return;
            }

            if (_pages.Count > 0 && _pages[0] is PackagePage old)
            {
                _horizontalSlideDirection = GetCardIndex(pkg.Id) >= GetCardIndex(old.Package.Id) ? 1 : -1;
            }

            _pages.Clear();

            var page = new PackagePage
            {
                Title = pkg.DisplayName,
                TitleIcon = GetPackageIcon(pkg),
                Window = this,
                Package = pkg
            };
            page.EnterPage();
        }

        /// <summary>包在概览卡片中的序号（未找到返回 -1；序号只用于滑动方向判定）。</summary>
        int GetCardIndex(string packageId)
        {
            for (var i = 0; i < _cards.Count; i++)
            {
                if (_cards[i].Installed != null && _cards[i].Installed.Id == packageId)
                {
                    return i;
                }
            }

            return -1;
        }

        #endregion

        #region 页面栈绘制

        /// <summary>页面栈区域：单页直绘；页间切换时左右两页按水平进度交叉滑动（照 Odin GettingStartedWindow）。</summary>
        void DrawPageStack(ref Rect rect)
        {
            if ((_slideToPage ?? _slideFromPage) == null)
            {
                return;
            }

            if (_slideToPage == null || _slideFromPage == null)
            {
                var page = _slideToPage ?? _slideFromPage;
                DrawPage(ref rect, page, page.FooterSize);
                return;
            }

            if (_slideToPage == _slideFromPage)
            {
                DrawPage(ref rect, _slideToPage, _slideToPage.FooterSize);
                return;
            }

            var left = rect;
            var right = rect;
            var t = _horizontalSlideDirection == 1 ? HorizontalSlideT : 1f - HorizontalSlideT;
            left.x -= t * rect.width;
            right.x = left.xMax;

            var from = _slideFromPage;
            var to = _slideToPage;
            if (_horizontalSlideDirection < 0)
            {
                to = _slideFromPage;
                from = _slideToPage;
            }

            var footerSize = from.FooterSize * (1f - t) + to.FooterSize * t;
            var prevColor = GUI.color;
            GUI.color = prevColor * new Color(1f, 1f, 1f, 1f - t);
            DrawPage(ref left, from, footerSize);
            GUI.color = prevColor * new Color(1f, 1f, 1f, t);
            DrawPage(ref right, to, footerSize);
            GUI.color = prevColor;
            rect.TakeFromTop(Mathf.Max(left.height, right.height));
        }

        /// <summary>
        /// 单页绘制：标题栏（居中图标 + 标题）+ 页体 + footer（返回按钮 + 页脚提示），入场随 VerticalSlideT 渐显。
        /// 注意：绘制序列必须与事件类型/入场进度无关（不做任何早退门控）——控件序列随帧变化会导致
        /// Layout 与 Repaint 两次 pass 不一致，触发 IMGUI "Getting control N's position" 异常中止。
        /// </summary>
        void DrawPage(ref Rect rect, GetStartedPage page, float footerSize)
        {
            var verticalT = page.EntranceT;
            var prevColor = GUI.color;
            GUI.color *= new Color(1f, 1f, 1f, verticalT);

            // 标题栏
            var topRect = rect.TakeFromTop(50f * verticalT);
            var headerStyle = SirenixGUIStyles.SectionHeaderCentered;
            var textWidth = headerStyle.CalcSize(GUIHelper.TempContent(page.Title)).x;
            const int iconSize = 25;
            const int spacing = 5;
            var headerRow = topRect.AlignCenterX(iconSize + spacing + textWidth);

            var c = GUI.color;
            GUI.color = Color.white;
            EditorGUI.DrawRect(topRect, SirenixGUIStyles.HeaderBoxBackgroundColor);
            GUI.color = c;
            GUI.Label(headerRow.AlignRight(textWidth), page.Title, headerStyle);
            SdfIcons.DrawIcon(headerRow.AlignLeft(iconSize), page.TitleIcon, headerStyle.normal.textColor);

            // footer：返回按钮 + 页脚提示
            var bottomRect = rect.TakeFromBottom(footerSize * verticalT);
            c = GUI.color;
            GUI.color = Color.white;
            EditorGUI.DrawRect(bottomRect, SirenixGUIStyles.DarkEditorBackground);
            EditorGUI.DrawRect(bottomRect.AlignTop(1f), SirenixGUIStyles.BorderColor);
            GUI.color = c;
            bottomRect = bottomRect.HorizontalPadding(20f).AlignCenterY(25f);
            if (Button(ref bottomRect, "返回", SdfIconType.ChevronLeft, Direction.Left, Direction.Left))
            {
                page.GoBack();
            }

            page.DrawFooter(bottomRect);

            // 页体
            var bodyRect = rect;
            EditorGUI.DrawRect(bodyRect.AlignTop(1f), SirenixGUIStyles.BorderColor);
            page.DrawPage(bodyRect);

            GUI.color = prevColor;
        }

        #endregion

        #region 图标映射

        /// <summary>包卡片图标（Architecture 架构图 / Modules 拼图，用于包页标题栏）。</summary>
        static SdfIconType GetPackageIcon(AesirGetStartedService.AesirPackageInfo pkg) =>
            pkg != null && pkg.Id.EndsWith("modules", StringComparison.Ordinal)
                ? SdfIconType.PuzzleFill
                : SdfIconType.Diagram3Fill;

        /// <summary>示例卡片图标：分组优先，功能演示组内按示例细分。</summary>
        static SdfIconType GetSampleIcon(AesirGetStartedService.AesirSampleInfo sample)
        {
            switch (AesirGetStartedService.GetSampleGroup(sample))
            {
                case "MVC 计数器 · 渐进式":
                    return SdfIconType.Diagram3Fill;
                case "MVP 计数器 · 渐进式":
                    return SdfIconType.PersonWorkspace;
                case "实战示例":
                    return SdfIconType.Joystick;
                case "事件模块":
                    return SdfIconType.BroadcastPin;
                case "音频模块":
                    return SdfIconType.MusicNoteBeamed;
            }

            var dir = sample.RelativeDir ?? string.Empty;
            if (dir.StartsWith("MiniEvent", StringComparison.Ordinal))
            {
                return SdfIconType.LightningChargeFill;
            }

            if (dir.StartsWith("ObservableCollections", StringComparison.Ordinal))
            {
                return SdfIconType.Collection;
            }

            if (dir.StartsWith("ObservableValue", StringComparison.Ordinal))
            {
                return SdfIconType.EaselFill;
            }

            if (dir.StartsWith("RuntimeInitializeLoadType", StringComparison.Ordinal))
            {
                return SdfIconType.ClockHistory;
            }

            return SdfIconType.StarFill;
        }

        #endregion

        #region 页面

        /// <summary>Getting Started 页面基类（照 Odin GettingStartedPage：标题栏 / footer / 页面栈进出 / 滚动页包装）。</summary>
        abstract class GetStartedPage
        {
            static GUIStyle _pagePaddingStyle;
            public readonly float FooterSize = FooterHeight;

            protected Vector2 ScrollPosition;
            public string Title;
            public SdfIconType TitleIcon;

            [NonSerialized]
            public AesirGetStartedWindowOdin Window;

            /// <summary>入场进度（1 = 页面完全展开；概览收起时为 0，条目随其渐显增高）。</summary>
            public float EntranceT => 1f - Window.VerticalSlideT;

            protected static GUIStyle PagePaddingStyle =>
                _pagePaddingStyle ??= new GUIStyle { padding = new RectOffset(24, 24, 24, 24) };

            public virtual void DrawFooter(Rect rect) { }

            public abstract void DrawPage(Rect rect);

            public virtual void GoBack()
            {
                if (Window._pages.Count > 0)
                {
                    Window._pages.RemoveAt(Window._pages.Count - 1);
                }
            }

            public virtual void EnterPage()
            {
                Window._pages.Add(this);
            }

            protected void BeginScrollableLayoutPage(Rect rect, int paddingSize = 24)
            {
                // 复用静态 RectOffset 逐字段写入（照 Odin GettingStartedPage，避免每帧 new）
                PagePaddingStyle.padding.left = paddingSize;
                PagePaddingStyle.padding.right = paddingSize;
                PagePaddingStyle.padding.top = paddingSize;
                PagePaddingStyle.padding.bottom = paddingSize;
                GUILayout.BeginArea(rect);
                ScrollPosition = EditorGUILayout.BeginScrollView(ScrollPosition);
                GUILayout.BeginVertical(PagePaddingStyle);
            }

            protected void EndScrollableLayoutPage()
            {
                GUILayout.EndVertical();
                EditorGUILayout.EndScrollView();
                GUILayout.EndArea();
            }
        }

        /// <summary>包示例页：按教学分组列出示例卡片，点击卡片打开示例场景。</summary>
        sealed class PackagePage : GetStartedPage
        {
            static GUIStyle _footerHintStyle;

            static GUIStyle _cardDescStyle;
            public AesirGetStartedService.AesirPackageInfo Package;
            string _footerText;

            List<AesirGetStartedService.SampleGroup> _groups;
            float _prevWidth = 600f;

            /// <summary>页脚提示样式（右对齐灰字、比 MiniLabel 大一档——提示文本落在 footer 右下角）。</summary>
            static GUIStyle FooterHintStyle =>
                _footerHintStyle ??= new GUIStyle(SirenixGUIStyles.LeftAlignedGreyMiniLabel)
                {
                    alignment = TextAnchor.MiddleRight,
                    fontSize = 12
                };

            static GUIStyle CardDescStyle =>
                _cardDescStyle ??= new GUIStyle(SirenixGUIStyles.MultiLineLabel);

            public override void EnterPage()
            {
                _groups = AesirGetStartedService.GroupSamples(Package);
                _footerText = $"共 {Package.Samples.Count} 个示例 · 点击卡片在 Project 窗口选中示例文件夹 · 「打开场景」保存当前场景并进入示例";
                base.EnterPage();
            }

            public override void DrawFooter(Rect rect)
            {
                GUI.Label(rect, _footerText, FooterHintStyle);
            }

            public override void DrawPage(Rect pageRect)
            {
                if (Event.current.type == EventType.MouseMove)
                {
                    GUIHelper.RequestRepaint();
                }

                BeginScrollableLayoutPage(pageRect);

                foreach (var group in _groups)
                {
                    GUILayoutUtility.GetRect(0f, 8f * EntranceT);
                    SirenixEditorGUI.Title(group.Title, $"{group.Samples.Count} 个示例", TextAlignment.Left,
                        true);
                    GUILayoutUtility.GetRect(0f, 4f * EntranceT);

                    foreach (var sample in group.Samples)
                    {
                        DrawSampleCard(sample);
                    }

                    GUILayoutUtility.GetRect(0f, 6f);
                }

                // 下一帧取得滚动区实际宽度（供卡片描述换行高度预算；照 Odin TutorialPage 的 prevWidth 手法）
                var measured = GUILayoutUtility.GetRect(0f, 1f).width;
                if (Event.current.type == EventType.Repaint)
                {
                    _prevWidth = measured;
                }

                EndScrollableLayoutPage();
            }

            /// <summary>
            /// 示例卡片（照 Odin TutorialPage）：左图标 + 标题/描述 + 底部信息行（无场景提示 / 档位徽章）+
            /// 右侧动作按钮（有场景为「打开场景」、未导入为「去导入」、已导入无场景不占按钮列），整卡点击在
            /// Project 窗口选中示例文件夹。底部信息行有专属高度预算（不与描述叠字）；绘制序列恒定（不做任何早退门控）。
            /// </summary>
            void DrawSampleCard(AesirGetStartedService.AesirSampleInfo sample)
            {
                const int iconWidth = 50;
                const int textPadding = 14;
                const int buttonWidth = 80;
                const float buttonHeight = 36f;
                const float bottomLineHeight = 20f; // 底部信息行：无场景提示（左）+ 档位徽章（右）

                var badge = AesirGetStartedService.GetSampleBadge(sample);
                var hasSceneHint = sample.IsImported && !sample.HasScene;
                var hasBottomLine = badge != null || hasSceneHint;
                var bottomLine = hasBottomLine ? bottomLineHeight : 0f;

                // 右侧按钮列：有场景 →「打开场景」、未导入 →「去导入」；已导入无场景不占按钮列（描述全宽）
                var buttonColumnWidth = sample.IsImported && !sample.HasScene ? 0f : buttonWidth + 14f;

                // 描述换行高度预算：内容区实际宽度须扣掉图标、内边距与按钮列，否则预算行数偏少导致溢出
                var predicted = _prevWidth - textPadding * 2 - iconWidth - buttonColumnWidth;
                var descriptionSize = string.IsNullOrEmpty(sample.Description)
                    ? 0f
                    : CardDescStyle.CalcHeight(GUIHelper.TempContent(sample.Description), predicted);
                var rowHeight = Mathf.Max(66f, 22f + descriptionSize + 10f + bottomLine + 4f);

                var rect = GUILayoutUtility.GetRect(0f, rowHeight * EntranceT);

                // 未导入卡片半显（不用 GUI.enabled=false——「去导入」引导按钮仍需可点击）
                var prevColor = GUI.color;
                if (!sample.IsImported)
                {
                    GUI.color *= new Color(1f, 1f, 1f, 0.5f);
                }

                // 背景 + hover 高亮 + 边框
                EditorGUI.DrawRect(rect, SirenixGUIStyles.HeaderBoxBackgroundColor);
                var hover = rect.Contains(Event.current.mousePosition);
                if (hover && sample.IsImported)
                {
                    EditorGUI.DrawRect(rect, SirenixGUIStyles.MouseOverBgOverlayColor);
                }

                SirenixEditorGUI.DrawBorders(rect, 1);

                // 左图标（整卡垂直居中）
                var iconRect = rect.TakeFromLeft(iconWidth).AlignCenterY(26f);
                SdfIcons.DrawIcon(iconRect, GetSampleIcon(sample), SirenixGUIStyles.Label.normal.textColor);

                // 内容区（标题 + 描述），底部预留信息行高度——描述不会与提示/徽章叠字
                var contentRect = rect.Padding(textPadding, 7f);
                contentRect.width -= buttonColumnWidth;
                contentRect.yMax -= bottomLine;

                var titleRect = contentRect.TakeFromTop(22f);
                GUI.Label(titleRect, sample.DisplayName, SirenixGUIStyles.BoldLabel);

                if (!string.IsNullOrEmpty(sample.Description))
                {
                    GUI.Label(contentRect, sample.Description, CardDescStyle);
                }

                // 底部信息行：无场景提示（左）与档位徽章（右）同一行，各归其位
                // （rect 已被 TakeFromLeft 收窄过，提示行只补内边距、不再叠加图标宽度）
                if (hasBottomLine)
                {
                    var bottomRect = rect.AlignBottom(bottomLineHeight);

                    if (hasSceneHint)
                    {
                        bottomRect.xMin += textPadding;
                        GUI.Label(bottomRect, "代码示例 · 无独立场景，组件可挂到任意场景",
                            SirenixGUIStyles.LeftAlignedGreyMiniLabel);
                    }

                    if (badge != null)
                    {
                        var badgeRect = rect.AlignBottom(bottomLineHeight);
                        badgeRect.xMax -= textPadding + 4f;
                        GUI.Label(badgeRect, badge, SirenixGUIStyles.RightAlignedGreyMiniLabel);
                    }
                }

                // 行内按钮（先于整卡按钮绘制：重叠区域命中优先；垂直居中、比例与卡片协调）
                if (buttonColumnWidth > 0f)
                {
                    var buttonRect = rect.Padding(textPadding, 0f).AlignRight(buttonWidth)
                        .AlignCenterY(buttonHeight);
                    // 按钮样式用 Button 而非 MiniButton——MiniButton 的背景不随 rect 高度拉伸（恒 ~18px 高
                    // 且锚在 rect 顶部），加大 rect 只扩点击区不涨可见高度；Button 随 rect 完整渲染
                    if (GUI.Button(buttonRect, sample.IsImported ? "打开场景" : "去导入", SirenixGUIStyles.Button))
                    {
                        if (sample.IsImported)
                        {
                            if (AesirGetStartedService.OpenSampleScene(sample))
                            {
                                Window.ShowBottomRightToast(SdfIconType.CheckCircleFill,
                                    AesirGetStartedService.BuildOpenSceneToastMessage(sample));
                            }
                        }
                        else
                        {
                            AesirGetStartedService.OpenPackageManager();
                        }
                    }
                }

                // 整卡点击：在 Project 窗口选中示例文件夹并弹 Toast（未导入引导 Package Manager）
                if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
                {
                    if (sample.IsImported)
                    {
                        if (AesirGetStartedService.PingSample(sample))
                        {
                            Window.ShowBottomRightToast(SdfIconType.FolderFill,
                                AesirGetStartedService.BuildPingToastMessage(sample));
                        }
                    }
                    else
                    {
                        AesirGetStartedService.OpenPackageManager();
                    }
                }

                GUI.color = prevColor;
                GUILayoutUtility.GetRect(0f, 8f);
            }
        }

        #endregion
    }
}
