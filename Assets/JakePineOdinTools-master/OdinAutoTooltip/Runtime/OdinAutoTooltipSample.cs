// Copyright (c) 2026 Jake Pine
// SPDX-License-Identifier: MIT
// This software is provided "as is", without warranty of any kind. Use at your own risk.

using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Demonstrates OdinAutoTooltip patterns from the plugin README. Hover inspector labels to see tooltips
/// applied from XML summaries or member-name fallback. Add this component to a GameObject and inspect it in the editor.
/// </summary>
public class OdinAutoTooltipSample : MonoBehaviour
{
    // ── How it works: XML summary ───────────────────────────────────────────────────────────────

    /// <summary>How fast the unit moves in world units per second.</summary>
    public float moveSpeed = 5f;

    // ── How it works: member name fallback (label differs from field name) ────────────────────

    [LabelText("Move Speed")]
    public float playerMoveSpeed = 5f;

    // ── Basic usage ───────────────────────────────────────────────────────────────────────────

    /// <summary>When enabled, the object loops its animation.</summary>
    public bool loop;

    /// <summary>Resets all runtime state to defaults.</summary>
    [Button]
    private void ResetState()
    {
    }

    // ── Multi-line summary ────────────────────────────────────────────────────────────────────

    /// <summary>Minimum health floor for this unit.</summary>
    public int minHealth = 1;

    /// <summary>
    /// Maximum health for this unit. Clamped at runtime by
    /// <see cref="minHealth"/> and upgrade modifiers.
    /// </summary>
    public int maxHealth = 100;

    // ── Overriding auto-tooltips ────────────────────────────────────────────────────────────────

    /// <summary>This summary is ignored for the tooltip.</summary>
    [Tooltip("Shown in the inspector instead.")]
    public float damage = 10f;

    /// <summary>Internal tuning value; documented in code only.</summary>
    [Tooltip(null)]
    public float hiddenTuning = 1f;

    // ── Summary and attributes (either order) ─────────────────────────────────────────────────

    /// <summary>Spawn offset from the anchor transform.</summary>
    [FoldoutGroup("Spawn")]
    [Range(0f, 10f)]
    public Vector3 spawnOffset;

    [FoldoutGroup("Stats")]
    /// <summary>Current health points.</summary>
    public int health;

    // ── Nested types  ─────

    [FoldoutGroup("Nested Types")]
    public Outer nestedOuter = new Outer();

    // ── Enum field (not per-value auto-tooltip) ───────────────────────────────────────────────

    /// <summary>Playback direction for this clip.</summary>
    public Mode mode;

    /// <summary>
    /// The summary on a class does not auto generate any tooltips.
    /// </summary>
    [System.Serializable]
    public class Outer
    {
        /// <summary>Tooltip for outerField inside Outer.</summary>
        public int outerField;

        public Inner inner = new Inner();

        [System.Serializable]
        public class Inner
        {
            /// <summary>Inner field tooltip.</summary>
            public int innerField;
        }
    }

    public enum Mode
    {
        /// <summary>Play forward from start to end.</summary>
        [Tooltip("Play forward from start to end.")]
        Forward,

        /// <summary>Play backward from end to start.</summary>
        [Tooltip("Play backward from end to start.")]
        Reverse,
    }
}
