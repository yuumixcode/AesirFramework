using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// 可观察集合浏览器 —— 集中查看编辑器下存活的可观察集合与同步视图。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 数据源是 <see cref="ObservableCollectionRegistry" />（集合构造函数与 <c>CreateView</c> 自动登记，
    /// 只持弱引用）。面板为只读调试工具：显示类型、元素数、<c>CollectionChanged</c> 订阅者数、
    /// 轻量事件监听者数、同步视图数量与元素预览。
    /// </para>
    /// <para>
    /// 行视图模型在刷新时一次性重建并预计算显示文本，OnGUI 期间零反射、零 LINQ。
    /// 「自动刷新」为可选项（默认关闭）——开启后按固定间隔重建行模型，用于观察运行时变化。
    /// </para>
    /// </remarks>
    public class ObservableCollectionBrowserWindow : OdinEditorWindow
    {
        #region 常量

        const string WindowTitle = "Observable Collections";

        const string InfoText =
            "列出编辑器下存活的可观察集合与同步视图（弱引用登记，不阻止 GC）。\n" +
            "集合在构造时、视图在 CreateView 时自动登记；玩家构建中登记调用被编译器整体移除，运行时零开销。\n" +
            "面板为只读调试视图，不修改集合内容。";

        const string EmptyText =
            "当前没有存活的可观察集合。集合创建后会自动出现在这里（例如在 Play Mode 中创建 ObservableList）。";

        /// <summary>自动刷新间隔（秒）。</summary>
        const double AutoRefreshInterval = 0.5;

        #endregion

        #region 打开入口

        /// <summary>
        /// 打开窗口。
        /// </summary>
        [MenuItem("Tools/Aesir/Observable Collections")]
        public static void OpenWindow()
        {
            var window = GetWindow<ObservableCollectionBrowserWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = new Vector2(720, 420);
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(860, 560);
            window.Show();
        }

        #endregion

        #region 序列化状态（跨域重载保留）

        [SerializeField, HideInInspector]
        List<CollectionRow> _rows = new List<CollectionRow>();

        #endregion

        #region 会话状态（域重载后重建）

        [HideInInspector]
        string _lastRefreshTime = "尚未刷新";

        [HideInInspector]
        bool _autoRefreshSubscribed;

        double _nextAutoRefreshTime;

        #endregion

        #region 面板内容

        [Title("可观察集合浏览器", "ObservableList / Dictionary / HashSet / Queue / Stack / RingBuffer / 同步视图")]
        [InfoBox(InfoText, InfoMessageType.Info)]
        [InfoBox(EmptyText, InfoMessageType.Warning, VisibleIf = nameof(IsEmpty))]
        [HorizontalGroup("Toolbar")]
        [LabelText("过滤"), LabelWidth(40), PropertySpace(0, 4)]
        [OnValueChanged(nameof(Refresh))]
        [SerializeField]
        string _filter = "";

        [HorizontalGroup("Toolbar", Width = 120)]
        [LabelText("自动刷新"), LabelWidth(60), PropertySpace(0, 4)]
        [OnValueChanged(nameof(OnAutoRefreshChanged))]
        [SerializeField]
        bool _autoRefresh;

        [HorizontalGroup("Toolbar")]
        [Button("立即刷新", ButtonSizes.Medium)]
        void RefreshButton() => Refresh();

        [ShowInInspector, ReadOnly, HideLabel, DisplayAsString(false), PropertySpace(4, 4)]
        string LastRefreshLabel => $"最近刷新：{_lastRefreshTime}　|　条目：{_rows.Count}";

        [ShowInInspector, ReadOnly, LabelText("集合与视图")]
        [ListDrawerSettings(ShowFoldout = false, DraggableItems = false, IsReadOnly = true,
            ShowItemCount = false, ShowPaging = false, ShowIndexLabels = false)]
        List<CollectionRow> Rows => _rows;

        #endregion

        #region 生命周期

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();
            Refresh();
            ApplyAutoRefreshSubscription();
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
            base.OnDisable();
            EditorApplication.update -= OnEditorUpdate;
            _autoRefreshSubscribed = false;
        }

        /// <inheritdoc />
        protected override void OnDestroy()
        {
            base.OnDestroy();
            EditorApplication.update -= OnEditorUpdate;
            _autoRefreshSubscribed = false;
        }

        #endregion

        #region 刷新

        /// <summary>
        /// 重建行模型（含过滤）。
        /// </summary>
        void Refresh()
        {
            var collections = ObservableCollectionRegistry.GetLiveCollections();
            var views = ObservableCollectionRegistry.GetLiveViews();

            _rows.Clear();
            var filter = string.IsNullOrEmpty(_filter) ? null : _filter;

            for (var i = 0; i < collections.Count; i++)
            {
                var collection = collections[i];
                if (collection == null || !ObservableCollectionInspectorUtility.IsObservableCollection(collection))
                {
                    continue;
                }

                var row = CollectionRow.CreateCollectionRow(
                    collection,
                    ObservableCollectionInspectorUtility.CountViewsOf(collection, views));
                if (row.Matches(filter))
                {
                    _rows.Add(row);
                }
            }

            for (var i = 0; i < views.Count; i++)
            {
                var view = views[i].View;
                if (view == null)
                {
                    continue;
                }

                var row = CollectionRow.CreateViewRow(views[i].Source, view);
                if (row.Matches(filter))
                {
                    _rows.Add(row);
                }
            }

            _lastRefreshTime = DateTime.Now.ToString("HH:mm:ss");
        }

        /// <summary>
        /// 切换自动刷新订阅。
        /// </summary>
        void OnAutoRefreshChanged() => ApplyAutoRefreshSubscription();

        /// <summary>
        /// 按当前开关状态订阅 / 退订编辑器更新回调。
        /// </summary>
        void ApplyAutoRefreshSubscription()
        {
            if (_autoRefresh && !_autoRefreshSubscribed)
            {
                EditorApplication.update += OnEditorUpdate;
                _autoRefreshSubscribed = true;
                _nextAutoRefreshTime = EditorApplication.timeSinceStartup + AutoRefreshInterval;
            }
            else if (!_autoRefresh && _autoRefreshSubscribed)
            {
                EditorApplication.update -= OnEditorUpdate;
                _autoRefreshSubscribed = false;
            }
        }

        /// <summary>
        /// 编辑器更新回调：按固定间隔重建行模型。
        /// </summary>
        void OnEditorUpdate()
        {
            if (!_autoRefresh)
            {
                return;
            }

            var now = EditorApplication.timeSinceStartup;
            if (now < _nextAutoRefreshTime)
            {
                return;
            }

            _nextAutoRefreshTime = now + AutoRefreshInterval;
            Refresh();
            Repaint();
        }

        /// <summary>
        /// 当前是否没有任何条目（用于空态提示）。
        /// </summary>
        bool IsEmpty() => _rows.Count == 0;

        #endregion

        #region 行视图模型

        /// <summary>
        /// 单行视图模型 —— 显示文本与颜色在创建时预计算，绘制期间不再做任何计算。
        /// </summary>
        [Serializable]
        public sealed class CollectionRow
        {
            /// <summary>行类型着色。</summary>
            [HideInInspector]
            public Color RowColor;

            [HorizontalGroup("Row", 0.26f), DisplayAsString(false, 13), HideLabel, GUIColor(nameof(RowColor))]
            public string TypeName;

            [HorizontalGroup("Row", Width = 92), DisplayAsString(false, 13), HideLabel]
            public string CountText;

            [HorizontalGroup("Row", Width = 120), DisplayAsString(false, 13), HideLabel]
            public string SubscriberText;

            [HorizontalGroup("Row", Width = 92), DisplayAsString(false, 13), HideLabel]
            public string ViewText;

            [HorizontalGroup("Row"), DisplayAsString(false, 13), HideLabel]
            public string Preview;

            /// <summary>创建集合行。</summary>
            /// <param name="collection">集合实例。</param>
            /// <param name="viewCount">关联的同步视图数量。</param>
            /// <returns>行视图模型。</returns>
            public static CollectionRow CreateCollectionRow(object collection, int viewCount)
            {
                var count = ObservableCollectionInspectorUtility.GetCount(collection);
                return new CollectionRow
                {
                    RowColor = new Color(0.62f, 0.86f, 1f),
                    TypeName = ObservableCollectionInspectorUtility.GetDisplayTypeName(collection.GetType()),
                    CountText = "元素 " + count,
                    SubscriberText = "变更订阅 " + ObservableCollectionInspectorUtility.GetCollectionChangedSubscriberCount(collection)
                                     + "　轻量监听 " + ObservableCollectionInspectorUtility.GetLightEventListenerCount(collection),
                    ViewText = "视图 " + viewCount,
                    Preview = ObservableCollectionInspectorUtility.GetItemPreview(collection)
                };
            }

            /// <summary>创建同步视图行。</summary>
            /// <param name="source">视图所属的源集合。</param>
            /// <param name="view">同步视图实例。</param>
            /// <returns>行视图模型。</returns>
            public static CollectionRow CreateViewRow(object source, object view)
            {
                var filtered = ObservableCollectionInspectorUtility.GetCount(view);
                var unfiltered = ObservableCollectionInspectorUtility.GetUnfilteredCount(view);
                var sourceName = source == null
                    ? "<已回收>"
                    : ObservableCollectionInspectorUtility.GetDisplayTypeName(source.GetType());

                return new CollectionRow
                {
                    RowColor = new Color(0.75f, 0.9f, 0.7f),
                    TypeName = "└ 视图 ← " + sourceName,
                    CountText = "视图项 " + filtered,
                    SubscriberText = "未过滤 " + unfiltered + "　过滤器 " + ObservableCollectionInspectorUtility.GetFilterDescription(view),
                    ViewText = "",
                    Preview = ObservableCollectionInspectorUtility.GetItemPreview(view)
                };
            }

            /// <summary>
            /// 判断本行是否命中过滤词（匹配类型名与预览文本）。
            /// </summary>
            /// <param name="filter">过滤词；为 null 时全部命中。</param>
            /// <returns>命中返回 <c>true</c>。</returns>
            public bool Matches(string filter)
            {
                if (string.IsNullOrEmpty(filter))
                {
                    return true;
                }

                return TypeName != null && TypeName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
                       || Preview != null && Preview.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
            }
        }

        #endregion
    }
}
