using UnityEngine;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// 飞机大战原生版示例 —— 敌机生成器。
    /// </summary>
    /// <remarks>
    /// 原生写法：在 <c>Update</c> 里递减计时器，归零后从预制体数组随机取一种敌机、
    /// 在顶部生成区随机取横坐标实例化；生成节奏由两个间隔字段控制。
    /// <para>游戏进行时才生成；失败后停止生成。</para>
    /// </remarks>
    public class SamplePlaneWarMonoEnemySpawner : MonoBehaviour
    {
        /// <summary>
        /// 可生成的敌机预制体（等概率随机选取）。
        /// </summary>
        [SerializeField]
        SamplePlaneWarMonoEnemy[] enemyPrefabs;

        /// <summary>
        /// 生成间隔下限（秒）。
        /// </summary>
        [SerializeField]
        float minInterval = 0.4f;

        /// <summary>
        /// 生成间隔上限（秒）。
        /// </summary>
        [SerializeField]
        float maxInterval = 1.2f;

        /// <summary>
        /// 生成区半宽；横坐标在 ±半宽内随机。
        /// </summary>
        [SerializeField]
        float spawnHalfWidth = 2.5f;

        /// <summary>
        /// 生成纵坐标（画面顶部之外）。
        /// </summary>
        [SerializeField]
        float spawnY = 6.5f;

        float _timer;

        void Start()
        {
            _timer = Random.Range(minInterval, maxInterval);
        }

        void Update()
        {
            if (SamplePlaneWarMonoGameManager.Instance.IsGameOver)
            {
                return;
            }

            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                _timer = Random.Range(minInterval, maxInterval);
                Spawn();
            }
        }

        void Spawn()
        {
            SamplePlaneWarMonoEnemy prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            float x = Random.Range(-spawnHalfWidth, spawnHalfWidth);
            Instantiate(prefab, new Vector3(x, spawnY, 0f), Quaternion.identity);
        }
    }
}
