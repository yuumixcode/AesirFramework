#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
using UnityEngine;

namespace Runestone.AesirArchitecture.Samples.PlaneWarMono
{
    /// <summary>
    /// 飞机大战原生版示例 —— 敌机。
    /// </summary>
    /// <remarks>
    /// 原生写法：三种敌机（A / B / C）共用本脚本，速度与分值差异全部由预制体上的
    /// 序列化字段配置；被子弹命中时自行加分并销毁。
    /// <para>
    /// 移动方向为 -Y（自上方往下飞）；飞出画面底部即销毁。碰到玩家子弹会被消灭；
    /// 碰到玩家飞机（Kinematic Rigidbody2D）时玩家坠毁，游戏结束。
    /// </para>
    /// </remarks>
    public class SamplePlaneWarMonoEnemy : MonoBehaviour
    {
        /// <summary>
        /// 移动速度（世界单位 / 秒）。
        /// </summary>
        [SerializeField]
        float moveSpeed = 3f;

        /// <summary>
        /// 被击毁时的得分。
        /// </summary>
        [SerializeField]
        int scoreValue = 10;

        /// <summary>
        /// 飞出画面底部后销毁的 Y 阈值。
        /// </summary>
        [SerializeField]
        float destroyY = -7.5f;

        void Update()
        {
            transform.position += Vector3.down * (moveSpeed * Time.deltaTime);

            if (transform.position.y < destroyY)
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

            if (other.TryGetComponent<SamplePlaneWarMonoPlayer>(out SamplePlaneWarMonoPlayer player))
            {
                Destroy(player.gameObject);
                SamplePlaneWarMonoGameManager.Instance.SetGameOver();
            }
        }

        /// <summary>
        /// 被子弹击毁：加分并销毁自身。
        /// </summary>
        public void Kill()
        {
            SamplePlaneWarMonoGameManager.Instance.AddScore(scoreValue);
            Destroy(gameObject);
        }
    }
}
#endif
