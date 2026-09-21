using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace UnityTcp.Editor.Helpers
{
    /// <summary>
    /// Central compatibility surface for Unity's instance-id → <c>EntityId</c>
    /// migration (Unity 6000.5 / Unity 6.5).
    ///
    /// Unity 6.5 deprecated every <see cref="int"/>-based identity API with an
    /// error-level obsolete attribute (CS0619) in favour of 64-bit <c>EntityId</c>
    /// overloads that do not exist on earlier versions:
    ///   Object.GetInstanceID()                    -> Object.GetEntityId()
    ///   Selection.activeInstanceID                -> Selection.activeEntityId
    ///   EditorUtility.InstanceIDToObject(int)     -> EditorUtility.EntityIdToObject(EntityId)
    ///   InternalEditorUtility.GetObjectFromInstanceID(int)
    ///                                             -> InternalEditorUtility.GetObjectFromEntityId(EntityId)
    ///   AssetDatabase.GetAssetPath(int)           -> AssetDatabase.GetAssetPath(EntityId)
    ///
    /// These helpers speak <see cref="long"/> so the full 64-bit EntityId value is
    /// carried losslessly and can be reconstructed exactly (no 32-bit truncation).
    /// On pre-6.5 Unity the legacy <c>int</c> instance id widens implicitly to
    /// <c>long</c>, so call sites see a single consistent type across versions. The
    /// only version-specific code lives in the two conversion helpers at the bottom.
    /// </summary>
    public static class InstanceIdExtensions
    {
        // ---- producers: Object / Selection -> long id -------------------------

        /// <summary>
        /// Returns the object's stable instance id as a <see cref="long"/>, or 0 if
        /// the object is null/destroyed. Prefer this over <c>GetInstanceID()</c>.
        /// </summary>
        public static long GetStableInstanceId(this Object obj)
        {
            if (obj == null) return 0;
#if UNITY_6000_5_OR_NEWER
            return ToLong(obj.GetEntityId());
#else
            return obj.GetInstanceID();
#endif
        }

        /// <summary>Active selection instance id, or 0 if nothing is selected.</summary>
        public static long ActiveSelectionInstanceId()
        {
#if UNITY_6000_5_OR_NEWER
            return ToLong(Selection.activeEntityId);
#else
#pragma warning disable CS0618
            return Selection.activeInstanceID;
#pragma warning restore CS0618
#endif
        }

        // ---- consumers: long id -> Object / path ------------------------------

        /// <summary>Resolves an instance id to its <see cref="Object"/>, or null.</summary>
        public static Object InstanceIdToObject(long instanceId)
        {
#if UNITY_6000_5_OR_NEWER
            return EditorUtility.EntityIdToObject(ToEntityId(instanceId));
#else
#pragma warning disable CS0618
            return EditorUtility.InstanceIDToObject((int)instanceId);
#pragma warning restore CS0618
#endif
        }

        /// <summary>
        /// Resolves an instance id via <c>InternalEditorUtility</c> (used for objects
        /// that <see cref="EditorUtility"/> will not return, e.g. some scene objects).
        /// </summary>
        public static Object ObjectFromInstanceId(long instanceId)
        {
#if UNITY_6000_5_OR_NEWER
            return InternalEditorUtility.GetObjectFromEntityId(ToEntityId(instanceId));
#else
#pragma warning disable CS0618
            return InternalEditorUtility.GetObjectFromInstanceID((int)instanceId);
#pragma warning restore CS0618
#endif
        }

        /// <summary>Asset path for an instance id, or "" if it is not an asset.</summary>
        public static string AssetPathFromInstanceId(long instanceId)
        {
#if UNITY_6000_5_OR_NEWER
            return AssetDatabase.GetAssetPath(ToEntityId(instanceId));
#else
#pragma warning disable CS0618
            return AssetDatabase.GetAssetPath((int)instanceId);
#pragma warning restore CS0618
#endif
        }

#if UNITY_6000_5_OR_NEWER
        // ---- the ONLY version-specific conversions ----------------------------
        // Both are pure 64-bit bit reinterpretations of EntityId.ToULong (the
        // canonical numeric form of an EntityId, which this project already uses),
        // so ToEntityId is an exact inverse of ToLong with no loss of information.

        /// <summary>Reinterprets an <c>EntityId</c> as a 64-bit <see cref="long"/>.</summary>
        internal static long ToLong(EntityId entityId)
            => unchecked((long)EntityId.ToULong(entityId));

        /// <summary>
        /// Exact inverse of <see cref="ToLong"/>: rebuilds the <c>EntityId</c> from
        /// its 64-bit numeric form via <c>EntityId.FromULong</c> (the counterpart of
        /// <c>EntityId.ToULong</c>), so an id survives a round-trip through storage or
        /// the wire without loss.
        /// </summary>
        internal static EntityId ToEntityId(long instanceId)
            => EntityId.FromULong(unchecked((ulong)instanceId));
#endif
    }
}
