using FreeSql.DataAnnotations;

namespace BlazorRBAC.Domain.Entities;

/// <summary>
/// 用户角色关联表（多对多中间表）
/// </summary>
[Index("uk_user_role", "UserId,RoleId", true)]   // 组合唯一索引
public class UserRole
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

    // ====== 🔧 修复：添加导航属性 ======
    /// <summary>
    /// 导航属性：关联的用户
    /// </summary>
    public User User { get; set; }

    /// <summary>
    /// 导航属性：关联的角色
    /// </summary>
    public Role Role { get; set; }
}