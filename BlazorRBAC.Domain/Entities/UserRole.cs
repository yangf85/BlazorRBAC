using FreeSql.DataAnnotations;

namespace BlazorRBAC.Domain.Entities;

/// <summary>
/// 用户角色关联表（多对多中间表）
/// </summary>
[Index("uk_user_role", "UserId,RoleId", true)]   // 组合唯一索引
[Index("idx_role_id", nameof(RoleId), false)]    // RoleId 单独索引
public class UserRole : BaseEntity
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [Column(Position = 11)]
    public int UserId { get; set; }

    /// <summary>
    /// 角色ID
    /// </summary>
    [Column(Position = 12)]
    public int RoleId { get; set; }
}