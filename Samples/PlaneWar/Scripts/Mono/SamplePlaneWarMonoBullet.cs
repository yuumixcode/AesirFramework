#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
using UnityEngine;

namespace Runestone.AesirArchitecture.Samples.PlaneWarMono
{
    /// <summary>
    /// 飞机大战原生版示例 —— 玩家子弹。
    /// </summary>
    /// <remarks>
    /// 原生写法：每帧在 <c>Update</c> 里推进位移，命中检测依赖 Physics2D 触发器——
    /// 子弹挂 Kinematic Rigidbody2D + Trigger 碰撞体，敌机挂静态 Trigger 碰撞体，
    /// 两者重叠即触发 <c>OnTriggerEnter2D</c>（两个静态碰撞体之间不会产生回调）。
    /// <para>游戏进行时玩家朝上射击；游戏结束后所有子弹清空。</para>
    /// </remarks>
    public class SamplePlaneWarMonoBullet : MonoBehaviour
    {
        /// <summary>
        /// 飞行速度（世界单位 / 秒），朝 +Y（画面上方）飞行。
        /// </summary>
        [SerializeField]
        float moveSpeed = 12f;

        /// <summary>
        /// 飞出画面顶部后销毁的 Y 阈值。
        /// </summary>
        [SerializeField]
        float destroyY = 7.5f;

        void Update()
        {
            transform.position += Vector3.up * (moveSpeed * Time.deltaTime);

            if (transform.position.y > destroyY)
            {
                Destroy(gameObject);
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (SamplePlaneWarMonoGameManager.Instance.IsGameOver)
            {
                return;
            }

            if (other.TryGetComponent<SamplePlaneWarMonoEnemy>(out SamplePlaneWarMonoEnemy enemy))
            {
                enemy.Kill();
                Destroy(gameObject);
            }
        }
    }
}
#endif
