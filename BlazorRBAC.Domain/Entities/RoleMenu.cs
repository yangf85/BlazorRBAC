using FreeSql.DataAnnotations;

namespace BlazorRBAC.Domain.Entities;

/// <summary>
/// 角色菜单关联表（多对多中间表）
/// </summary>
[Index("uk_role_menu", "RoleId,MenuId", true)]   // 组合唯一索引
public class RoleMenu
{
    /// <summary>
    /// 角色ID
    /// </summary>
    [Column(Position = 11)]
    public int RoleId { get; set; }

    /// <summary>
    /// 菜单ID
    /// </summary>
    [Column(Position = 12)]
    public int MenuId { get; set; }

    public Role Role { get; set; }

    public Menu Menu { get; set; }
}