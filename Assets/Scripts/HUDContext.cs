using Runestone.AesirArchitecture;

namespace Game
{
    /// <summary>
    /// HUD 面板关联的 Context：聚合 HUD 相关的 Model 与 Service，作为面板与业务逻辑之间的数据中转站。
    /// </summary>
    public class HUDContext : AbstractContext<HUDContext>
    {
        /// <summary>
        /// 注册 HUD 面板所需的 Model 与 Service。
        /// </summary>
        protected override void Configure()
        {
            // 在此注册 HUD 所需的模块，例如:
            // RegisterModel(new HUDModel());
        }
    }
}
