using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorRBAC.Application.Dtos;

public record MenuDto
{
    public int Id { get; init; }
    public string MenuName { get; init; } = string.Empty;
    public string MenuCode { get; init; } = string.Empty;
    public string? RoutePath { get; init; }
    public string? Icon { get; init; }
    public int SortOrder { get; init; }
    public List<MenuDto>? Children { get; init; }

    // ✅ 自定义相等性（避免递归比较 Children）
    public virtual bool Equals(MenuDto? other)
    {
        if (other is null) return false;
        return Id == other.Id && MenuCode == other.MenuCode;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, MenuCode);
    }
}