// * ------------------------------------------------------------------
// * 本文件由 Aesir Modules Binder 以「同一脚本增量」模式创建
// * 之后重新生成时仅替换「绑定字段（自动生成）」region 内的内容（含 BindComponents 方法），
// * region 外的内容归开发者所有
// * 
// * 面板对象: HUD
// * 生成时间: 2026-09-06 19:38:21
// * 
// * 使用说明:
// * 1. 业务逻辑直接写在本文件 region 外的区域
// * 2. 更新绑定: 在 BinderAssistant 的 Inspector 中先「构建绑定单元」再「生成脚本」
// * 3. region 内的类型与特性均为全限定名，不依赖文件头部的 using 指令
// * ------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    /// <summary>
    /// 由 Binder 自动生成的绑定部分，与 HUDPanel.cs 中的 partial 合并为同一类。
    /// </summary>
    public partial class HUDPanel : Runestone.AesirModules.AesirBasePanelViewController<Game.HUDContext>,
        Runestone.AesirModules.IComponentBinder
    {
    #region 绑定字段（自动生成）

    [Sirenix.OdinInspector.TitleGroup("绑定字段（自动生成）")]
    [UnityEngine.SerializeField]
    private UnityEngine.UI.Text scoreText;

    [Sirenix.OdinInspector.TitleGroup("绑定字段（自动生成）")]
    [UnityEngine.SerializeField]
    private UnityEngine.UI.Text timeText;

    [Sirenix.OdinInspector.TitleGroup("绑定字段（自动生成）")]
    [UnityEngine.SerializeField]
    private UnityEngine.UI.Text gameOverText;

    /// <summary>
    /// 绑定引用: 按 BinderAssistant 中配置的层级路径查找组件并赋值到绑定字段。
    /// </summary>
    [UnityEngine.ContextMenu("绑定引用")]
    public void BindComponents()
    {
        scoreText = transform.Find("ScoreText").GetComponent<UnityEngine.UI.Text>();
        timeText = transform.Find("TimeText").GetComponent<UnityEngine.UI.Text>();
        gameOverText = transform.Find("GameOverText").GetComponent<UnityEngine.UI.Text>();
    }
    #endregion
    }
}
