#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
using UnityEngine;
using Runestone.AesirModules;

namespace Runestone.AesirModules.Samples.Events.Filters
{
    /// <summary>
    /// 事件订阅者。注册 <c>[AesirListener]</c> 监听两种示例事件，收到后弹跳一次并变色，
    /// 同时向 <see cref="SampleHud" /> 输出日志。被过滤器拦截的订阅者保持静止，形成直观对照。
    /// </summary>
    [AddComponentMenu("")]
    public class AlarmReceiver : MonoBehaviour
    {
        /// <summary>收到事件后的反馈颜色。</summary>
        [SerializeField] Color flashColor = Color.yellow;

        Renderer _renderer;
        MaterialPropertyBlock _propertyBlock;
        Vector3 _baseScale;
        float _bounceTimer;
        int _alarmCount;
        int _orderCount;

        static readonly int ColorId = Shader.PropertyToID("_BaseColor");

        void Awake()
        {
            _renderer = GetComponentInChildren<Renderer>();
            _propertyBlock = new MaterialPropertyBlock();
            _baseScale = transform.localScale;
        }

        void Update()
        {
            if (_bounceTimer <= 0f)
            {
                return;
            }

            // 弹跳动画：0.3 秒内放大后回落
            _bounceTimer -= Time.deltaTime;
            var t = Mathf.Clamp01(_bounceTimer / 0.3f);
            transform.localScale = _baseScale * (1f + 0.4f * Mathf.Sin(t * Mathf.PI));
        }

        void OnEnable() => EventModule.AddListener(this);
        void OnDisable() => EventModule.RemoveListener(this);

        [AesirListener]
        void OnAlarm(AlarmEvent e)
        {
            _alarmCount++;
            Flash($"警报 ×{_alarmCount}：{e.Message}");
        }

        [AesirListener]
        void OnSquadOrder(SquadOrderEvent e)
        {
            _orderCount++;
            Flash($"命令 ×{_orderCount}：{e.Order}");
        }

        void Flash(string message)
        {
            _bounceTimer = 0.3f;

            if (_renderer != null)
            {
                _renderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor(ColorId, flashColor);
                _renderer.SetPropertyBlock(_propertyBlock);
            }

            SampleHud.Report(name, message);
        }
    }
}
#endif
