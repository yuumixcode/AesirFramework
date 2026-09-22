#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Runestone.AesirArchitecture.Samples.PlaneWarMono
{
    /// <summary>
    /// 飞机大战原生版示例 —— 游戏管理器（经典 MonoBehaviour 单例）。
    /// </summary>
    /// <remarks>
    /// 原生写法：静态 <c>Instance</c> 单例 + 场景预放置；得分与失败状态集中在此，
    /// 命中逻辑经 <c>Instance</c> 直加，HUD 每帧轮询 <see cref="Score" /> 取值刷新。
    /// <para>
    /// 玩家与敌机碰撞即判定失败：<see cref="SetGameOver" /> 置位后，
    /// 玩家移动/开火、敌机生成、子弹碰撞全部停止，按 Space 重开当前场景。
    /// </para>
    /// <para>
    /// 与 RAA 版的对比点：原生版中"游戏状态"依附于场景里的一个组件；
    /// RAA 版中得分属于纯 C# 的 Model，组件只是无状态的表现层。
    /// </para>
    /// <para>
    /// 注意：<see cref="Score" /> 与 <see cref="IsGameOver" /> 使用非序列化字段，
    /// 防止编辑器中反复 Play 时残留上次的运行状态。
    /// </para>
    /// </remarks>
    public class SamplePlaneWarMonoGameManager : MonoBehaviour
    {
        /// <summary>
        /// 全局单例访问器；由 Awake 赋值，场景中必须预放置一个实例。
        /// </summary>
        public static SamplePlaneWarMonoGameManager Instance { get; private set; }

        // 非序列化字段：防止 Unity 将运行状态保存到场景
        int _score;
        bool _isGameOver;

        /// <summary>
        /// 当前得分（只读暴露，写入走 <see cref="AddScore" />）。
        /// </summary>
        public int Score => _score;

        /// <summary>
        /// 是否已失败（玩家坠毁后置位）。
        /// </summary>
        public bool IsGameOver => _isGameOver;

        void Awake()
        {
            Instance = this;
            _isGameOver = false;
            _score = 0;
        }

        void Update()
        {
            if (_isGameOver && Input.GetKeyDown(KeyCode.Space))
            {
                Restart();
            }
        }

        /// <summary>
        /// 增加得分；子弹命中敌机时由 <see cref="SamplePlaneWarMonoEnemy.Kill" /> 调用。
        /// </summary>
        public void AddScore(int value)
        {
            _score += value;
        }

        /// <summary>
        /// 标记游戏结束（玩家飞机与敌机碰撞）。
        /// </summary>
        public void SetGameOver()
        {
            _isGameOver = true;
        }

        void Restart()
        {
            _isGameOver = false;
            _score = 0;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
#endif
