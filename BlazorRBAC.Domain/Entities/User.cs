using BlazorRBAC.Domain.Enums;  // ← 引入枚举命名空间
using FreeSql.DataAnnotations;

namespace BlazorRBAC.Domain.Entities;

/// <summary>
/// 用户实体
/// </summary>
[Index("uk_username", nameof(Username), true)]  // 唯一索引
[Index("idx_email", nameof(Email), false)]      // 普通索引
public class User : AuditEntity
{
    /// <summary>
    /// 用户名（唯一）
    /// </summary>
    [Column(Position = 11, StringLength = 50, IsNullable = false)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 密码哈希值
    /// </summary>
    [Column(Position = 12, StringLength = 200, IsNullable = false)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// 真实姓名
    /// </summary>
    [Column(Position = 13, StringLength = 50)]
    public string? RealName { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    [Column(Position = 14, StringLength = 100)]
    public string? Email { get; set; }

    /// <summary>
    /// 手机号
    /// </summary>
    [Column(Position = 15, StringLength = 20)]
    public string? Phone { get; set; }

    /// <summary>
    /// 登录类型
    /// </summary>
    [Column(Position = 16, MapType = typeof(string))]  // ← 数据库存字符串
    public LoginType LoginType { get; set; } = LoginType.Local;

    /// <summary>
    /// 外部账号ID（如微信OpenID）
    /// </summary>
    [Column(Position = 17, StringLength = 200)]
    public string? ExternalId { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    [Column(Position = 18)]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 导航属性：用户的角色列表
    /// </summary>
    [Navigate(ManyToMany = typeof(UserRole))]
    public List<Role> Roles { get; set; } = new();
}