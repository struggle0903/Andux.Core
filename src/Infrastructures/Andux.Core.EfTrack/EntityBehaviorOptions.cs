// =======================================
// 作者：andy.hu
// 文件：AuditOptions.cs
// 描述：审计配置选项，用于控制是否启用审计拦截器以及指定用户标识字段
// =======================================

namespace Andux.Core.EfTrack
{
    /// <summary>
    /// 实体行为选项，包含审计和软删除等功能的配置。
    /// </summary>
    public class EntityBehaviorOptions
    {
        /// <summary>
        /// 是否启用数据隔离。隔离实体需继承 IProject 接口
        /// 若为 true，则启用IProject接口字段的数据隔离；
        /// 若为 false，则不进行任何数据隔离。
        /// </summary>
        public bool EnableProject { get; set; } = false;

        /// <summary>
        /// 启用数据隔离后需要的字段，必须准确，否则数据隔离失败
        /// 该字段用于从 IHttpContextAccessor.HttpContext.User.Claims 中提取当前登录用户所拥有的 IProject 的字段唯一标识，
        /// </summary>
        public string ProjectClaimsType { get; set; } = "projectId";

        /// <summary>
        /// 是否启用审计功能。
        /// 若为 true，则启用拦截器自动设置创建时间、修改时间、创建人、修改人等字段；
        /// 若为 false，则不进行任何审计字段处理。
        /// </summary>
        public bool EnableAuditing { get; set; } = false;

        /// <summary>
        /// 用户标识在 JWT 或 Claims 中的字段名。
        /// 该字段用于从 IHttpContextAccessor.HttpContext.User.Claims 中提取当前用户的唯一标识，
        /// 并作为审计字段（CreatedBy / UpdatedBy）的值。
        /// 常见值如： "id"、"sub"、"userId"。
        /// </summary>
        public string UserClaimsType { get; set; } = "id";

        /// <summary>
        /// 是否启用软删除功能（开启后自动添加全局查询过滤）
        /// </summary>
        public bool EnableSoftDelete { get; set; } = false;

    }
}
