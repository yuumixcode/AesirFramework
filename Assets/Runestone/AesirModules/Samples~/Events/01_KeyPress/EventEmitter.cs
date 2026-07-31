using UnityEngine;

namespace Runestone.AesirModules.Samples.Events
{
    /// <summary>
    /// 按键事件发布者。按指定键发布 <see cref="OnKeyPressed"/> 事件。
    /// </summary>
    [AddComponentMenu("")]
    public class EventEmitter : MonoBehaviour
    {
        [SerializeField] KeyCode triggerKey = KeyCode.Space;

        void Update()
        {
            if (Input.GetKeyDown(triggerKey))
            {
                new OnKeyPressed { Key = triggerKey }.Invoke(this);
            }
        }
    }
}
