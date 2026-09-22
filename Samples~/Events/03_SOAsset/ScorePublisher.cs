#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
using UnityEngine;

namespace Runestone.AesirModules.Samples.Events.SOAsset
{
    /// <summary>
    /// 分数发布者。按 Space 触发 Inspector 指定的 <see cref="AesirEventArgsSO" /> 资产
    /// ——事件类型与载荷完全由资产配置（SubclassSelector 选型 + 字段填写），代码只负责调用 Raise。
    /// 演示"设计师在资产里配事件、程序里一行触发"的分工。
    /// <para>
    /// 同时用 <c>[AesirListener]</c> 订阅同一事件，展示同一 SO 事件的两种消费方式：
    /// UnityEvent 桥接（零代码，见 <see cref="ScoreBulb" />）与代码订阅（可读取载荷）。
    /// </para>
    /// </summary>
    [AddComponentMenu("")]
    public class ScorePublisher : MonoBehaviour
    {
        /// <summary>要触发的事件资产（Inspector 指定，本示例为 ScoreEventAsset）。</summary>
        [SerializeField]
        AesirEventArgsSO scoreEventAsset;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                PublishScore();
            }
        }

        void OnEnable() => EventModule.AddListener(this);
        void OnDisable() => EventModule.RemoveListener(this);

        /// <summary>
        /// 触发 Inspector 配置的事件资产（Space 的行为；公开方法便于教学与自动化验证）。
        /// </summary>
        public void PublishScore()
        {
            if (scoreEventAsset == null)
            {
                Debug.LogWarning("[ScorePublisher] 未指定 AesirEventArgsSO 资产。", this);
                return;
            }

            scoreEventAsset.Raise();
        }

        /// <summary>
        /// 代码订阅同一事件：可读取资产配置的载荷字段。
        /// </summary>
        [AesirListener]
        void OnScoreChanged(ScoreChangedEvent e)
        {
            Debug.Log($"[ScorePublisher] 代码订阅收到事件：NewScore = {e.NewScore}，发布者 = {e.Sender}", this);
        }
    }
}
#endif
