using FreeSql.DataAnnotations;

namespace BlazorRBAC.Domain.Entities;

/// <summary>
/// 基础实体（仅主键）
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// 主键
    /// </summary>
    [Column(Position = 1, IsPrimary = true, IsIdentity = true)]
    public int Id { get; set; }
}