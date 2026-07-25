using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// ProgressBar 特性案例。
    /// </summary>
    [AesirExample]
    internal class ProgressBarExampleSO : AttributeExampleSO<ProgressBarExampleSO>
    {
        [Title("No Parameters")]
        [ProgressBar(0, 100)]
        public int BasicProgressBar = 50;

        [Title("Parameter: ColorMember")]
        [ProgressBar(0, 1, ColorGetter = "GetColor")]
        public float ColoredProgressBar = 0.5f;

        [Title("Member Reference ($)")]
        [ProgressBar(0, "$MaxHealth", ColorGetter = "GetHealthColor")]
        public float HealthBar = 80;

        public float MaxHealth = 100;

        [Title("Parameter: Height")]
        [ProgressBar(-100, 100, Height = 30, ColorGetter = "CustomColor")]
        public int LargeProgressBar;

        Color CustomColor = new Color(0.2f, 0.6f, 1f);

        Color GetColor() => Color.Lerp(Color.red, Color.green, ColoredProgressBar);

        Color GetHealthColor() => Color.Lerp(Color.red, Color.green, HealthBar / MaxHealth);

        public override void AesirInspectorReset()
        {
            BasicProgressBar = 50;
            ColoredProgressBar = 0.5f;
            HealthBar = 80;
            MaxHealth = 100;
            LargeProgressBar = 0;
        }
    }
}
