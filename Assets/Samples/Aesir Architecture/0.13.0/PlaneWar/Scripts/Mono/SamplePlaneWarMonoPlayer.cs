using UnityEngine;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// 飞机大战原生版示例 —— 玩家飞机。
    /// </summary>
    /// <remarks>
    /// 原生写法：一个组件同时负责读输入、移动、开火三件事；移动边界以序列化字段配置在组件上。
    /// <para>
    /// 操作：方向键 / WASD 移动，按住 Space 连发（受 <see cref="fireCooldown" /> 节流）。
    /// 本示例中玩家位于画面下方朝上射击，敌机自上方往下飞。
    /// </para>
    /// <para>移动与开火只在游戏进行中生效；失败后由 <see cref="SamplePlaneWarMonoGameManager.SetGameOver" /> 接管。</para>
    /// </remarks>
    public class SamplePlaneWarMonoPlayer : MonoBehaviour
    {
        /// <summary>
        /// 移动速度（世界单位 / 秒）。
        /// </summary>
        [SerializeField]
        float moveSpeed = 8f;

        /// <summary>
        /// 开火最小间隔（秒）；按住开火键时按此间隔连发。
        /// </summary>
        [SerializeField]
        float fireCooldown = 0.15f;

        /// <summary>
        /// 移动边界（x / y 各自的半宽半高），玩家被限制在背景区域内。
        /// </summary>
        [SerializeField]
        Vector2 moveBounds = new Vector2(2.5f, 5.5f);

        /// <summary>
        /// 子弹预制体；在机头上方生成。
        /// </summary>
        [SerializeField]
        SamplePlaneWarMonoBullet bulletPrefab;

        /// <summary>
        /// 枪口相对机身的垂直偏移（机头朝上，为正值）。
        /// </summary>
        [SerializeField]
        float muzzleOffsetY = 0.6f;

        float _fireTimer;

        void Update()
        {
            if (SamplePlaneWarMonoGameManager.Instance.IsGameOver)
            {
                return;
            }

            Move();
            Fire();
        }

        void Move()
        {
            // 归一化保证斜向移动不快于正向
            Vector3 direction = new Vector3(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical"),
                0f).normalized;

            Vector3 position = transform.position + direction * (moveSpeed * Time.deltaTime);
            position.x = Mathf.Clamp(position.x, -moveBounds.x, moveBounds.x);
            position.y = Mathf.Clamp(position.y, -moveBounds.y, moveBounds.y);
            transform.position = position;
        }

        void Fire()
        {
            _fireTimer += Time.deltaTime;
            if (Input.GetKey(KeyCode.Space) && _fireTimer >= fireCooldown)
            {
                _fireTimer = 0f;
                Instantiate(bulletPrefab, transform.position + Vector3.up * muzzleOffsetY,
                    Quaternion.identity);
            }
        }
    }
}
