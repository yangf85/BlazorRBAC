using FreeSql.DataAnnotations;

namespace BlazorRBAC.Domain.Entities;

/// <summary>
/// 菜单实体（树形结构）
/// </summary>
[Index("uk_menu_code", nameof(MenuCode), true)]  // 唯一索引
[Index("idx_parent_id", nameof(ParentId), false)] // 普通索引
public class Menu : AuditEntity
{
    /// <summary>
    /// 父菜单ID（0表示根菜单）
    /// </summary>
    [Column(Position = 11)]
    public int ParentId { get; set; } = 0;

    /// <summary>
    /// 菜单名称
    /// </summary>
    [Column(Position = 12, StringLength = 50, IsNullable = false)]
    public string MenuName { get; set; } = string.Empty;

    /// <summary>
    /// 菜单编码（唯一标识）
    /// </summary>
    [Column(Position = 13, StringLength = 50, IsNullable = false)]
    public string MenuCode { get; set; } = string.Empty;

    /// <summary>
    /// 路由路径
    /// </summary>
    [Column(Position = 14, StringLength = 200)]
    public string? RoutePath { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    [Column(Position = 15, StringLength = 50)]
    public string? Icon { get; set; }

    /// <summary>
    /// 排序序号
    /// </summary>
    [Column(Position = 16)]
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// 是否可见
    /// </summary>
    [Column(Position = 17)]
    public bool IsVisible { get; set; } = true;

    // ==================== 导航属性 ====================

    /// <summary>
    /// 父菜单（导航属性）
    /// </summary>
    [Navigate(nameof(ParentId))]
    public Menu? Parent { get; set; }

    /// <summary>
    /// 子菜单列表（导航属性）
    /// </summary>
    [Navigate(nameof(ParentId))]
    public List<Menu> Children { get; set; } = new();
}