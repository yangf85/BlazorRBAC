using FreeSql.DataAnnotations;

namespace BlazorRBAC.Domain.Entities;

/// <summary>
/// 角色实体
/// </summary>
[Index("uk_role_code", nameof(RoleCode), true)]  // 唯一索引
public class Role : AuditEntity
{
    /// <summary>
    /// 角色名称
    /// </summary>
    [Column(Position = 11, StringLength = 50, IsNullable = false)]
    public string RoleName { get; set; } = string.Empty;

    /// <summary>
    /// 角色编码（唯一标识）
    /// </summary>
    [Column(Position = 12, StringLength = 50, IsNullable = false)]
    public string RoleCode { get; set; } = string.Empty;

    /// <summary>
    /// 角色描述
    /// </summary>
    [Column(Position = 13, StringLength = 200)]
    public string? Description { get; set; }

    /// <summary>
    /// 是否系统角色（超级管理员标识）
    /// </summary>
    [Column(Position = 14)]
    public bool IsSystem { get; set; } = false;

    /// <summary>
    /// 导航属性：角色拥有的菜单列表
    /// </summary>
    [Navigate(ManyToMany = typeof(RoleMenu))]
    public List<Menu> Menus { get; set; } = new();
}