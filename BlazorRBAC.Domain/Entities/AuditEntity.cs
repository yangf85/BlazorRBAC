using FreeSql.DataAnnotations;

namespace BlazorRBAC.Domain.Entities;

/// <summary>
/// 审计实体（时间戳 + 乐观锁）
/// </summary>
public abstract class AuditEntity : BaseEntity
{
    /// <summary>
    /// 创建时间
    /// </summary>
    [Column(Position = 2)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新时间
    /// </summary>
    [Column(Position = 3)]
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 乐观锁版本号
    /// </summary>
    [Column(Position = 4, IsVersion = true)]
    public int Version { get; set; }
}