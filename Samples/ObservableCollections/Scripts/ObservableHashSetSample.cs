#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
using UnityEngine;

namespace Runestone.AesirArchitecture.Samples.ObservableCollections
{
    /// <summary>
    /// ObservableHashSet&lt;T&gt; 演示组件（在线玩家场景）。
    /// <para>订阅 Added / Removed / Cleared 三类变更事件，通过 ContextMenu 触发增删与集合代数运算，在 Console 观察事件日志。</para>
    /// <para>无变更的操作不触发事件：Add 重复元素、Remove 不存在的元素、Clear 空集合。</para>
    /// <para>UnionWith / ExceptWith 逐项复用 Add / Remove，仅对实际变更的元素逐项触发事件。</para>
    /// </summary>
    public sealed class ObservableHashSetSample : MonoBehaviour
    {
        readonly ObservableHashSet<string> _onlinePlayers = new ObservableHashSet<string>();

        AutoRemoveListenerHandle _addedSub, _removedSub, _clearedSub;

        int _playerCounter;

        string _lastNameAdded;

        void Start()
        {
            PlayerOnline();
            PlayerOnline();
            PlayerOnline();
        }

        void OnEnable()
        {
            _addedSub = _onlinePlayers.AddAddedListener(
                player => Debug.Log($"[HashSet] Added → {player} 上线（当前 {_onlinePlayers.Count} 人）"));
            _removedSub = _onlinePlayers.AddRemovedListener(
                player => Debug.Log($"[HashSet] Removed → {player} 下线（当前 {_onlinePlayers.Count} 人）"));
            _clearedSub = _onlinePlayers.AddClearedListener(
                () => Debug.Log("[HashSet] Cleared → 在线列表已清空"));
        }

        void OnDisable()
        {
            _addedSub.Dispose();
            _removedSub.Dispose();
            _clearedSub.Dispose();
        }

        [ContextMenu("Add：玩家上线")]
        void PlayerOnline()
        {
            _lastNameAdded = $"玩家{_playerCounter++}";
            _onlinePlayers.Add(_lastNameAdded);
            DumpPlayers();
        }

        [ContextMenu("Add 重复玩家（不触发事件）")]
        void AddDuplicatePlayer()
        {
            if (_lastNameAdded == null)
            {
                Debug.LogWarning("[HashSet] 在线列表为空，先运行 Play Mode 或「Add：玩家上线」");
                return;
            }

            bool added = _onlinePlayers.Add(_lastNameAdded);
            Debug.Log($"[HashSet] Add(\"{_lastNameAdded}\")（重复）→ 返回 {added}，事件未触发");
        }

        [ContextMenu("Remove：刚上线的玩家下线")]
        void PlayerOffline()
        {
            if (_lastNameAdded == null || !_onlinePlayers.Remove(_lastNameAdded))
            {
                Debug.LogWarning("[HashSet] 没有可下线的玩家，先运行 Play Mode 或「Add：玩家上线」");
                return;
            }

            DumpPlayers();
        }

        [ContextMenu("Remove 不存在的玩家（不触发事件）")]
        void RemoveMissingPlayer()
        {
            bool removed = _onlinePlayers.Remove("不在线的玩家");
            Debug.Log($"[HashSet] Remove(\"不在线的玩家\") → 返回 {removed}，事件未触发");
        }

        [ContextMenu("UnionWith：批量上线（并集）")]
        void UnionPlayers()
        {
            _onlinePlayers.UnionWith(new[] { "Alice", "Bob" });
            DumpPlayers();
        }

        [ContextMenu("ExceptWith：批量下线（差集）")]
        void ExceptPlayers()
        {
            _onlinePlayers.ExceptWith(new[] { "Alice", "Bob" });
            DumpPlayers();
        }

        [ContextMenu("Clear：清空在线列表")]
        void ClearPlayers()
        {
            _onlinePlayers.Clear();
            Debug.Log($"[HashSet] Clear 完成，当前在线 {_onlinePlayers.Count} 人");
        }

        void DumpPlayers() =>
            Debug.Log($"[HashSet] 当前在线：[{string.Join(", ", _onlinePlayers)}]");
    }
}
#endif
