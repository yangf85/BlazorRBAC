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
    [Column(Position = 11, IsPrimary = true)]
    public int UserId { get; set; }

    /// <summary>
    /// 角色ID
    /// </summary>
    [Column(Position = 12, IsPrimary = true)]
    public int RoleId { get; set; }

    [Navigate(nameof(UserId))]
    public User User { get; set; }

    [Navigate(nameof(RoleId))]
    public Role Role { get; set; }
}