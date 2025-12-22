using BlazorRBAC.Application.Common;
using BlazorRBAC.Application.Dtos;
using BlazorRBAC.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorRBAC.Application.Services;

public class MenuService
{
    private readonly IFreeSql _fsql;

    public MenuService(IFreeSql fsql)
    {
        _fsql = fsql;
    }

    public async Task<Result<List<MenuDto>>> GetUserMenusAsync(int userId)
    {
        try
        {
            // ========== 第1步：判断是否超级管理员 ==========
            var isSuperAdmin = await _fsql.Select<UserRole, Role>()
                .InnerJoin((ur, r) => ur.RoleId == r.Id)
                .Where((ur, r) => ur.UserId == userId && r.IsSystem)
                .AnyAsync();

            List<Menu> menuTree;

            if (isSuperAdmin)
            {
                // ========== 超级管理员：获取所有菜单树 ==========
                menuTree = await _fsql.Select<Menu>()
                    .Where(m => m.IsVisible)
                    .AsTreeCte()  // ✅ 不需要参数，自动识别 Parent 导航属性
                    .OrderBy(m => m.SortOrder)
                    .ToTreeListAsync();
            }
            else
            {
                // ========== 第2步：获取用户拥有的菜单ID列表 ==========
                var menuIds = await _fsql.Select<UserRole, RoleMenu>()
                    .InnerJoin((ur, rm) => ur.RoleId == rm.RoleId)
                    .Where((ur, rm) => ur.UserId == userId)
                    .Distinct()
                    .ToListAsync((ur, rm) => rm.MenuId);

                if (!menuIds.Any())
                {
                    return Result<List<MenuDto>>.Failure("未获取任何菜单");
                }

                // ========== 第3步：构建菜单树 ==========
                menuTree = await _fsql.Select<Menu>()
                    .Where(m => m.IsVisible && menuIds.Contains(m.Id))
                    .AsTreeCte()  // ✅ 正确用法
                    .OrderBy(m => m.SortOrder)
                    .ToTreeListAsync();
            }

            var menuDtos = MapToDto(menuTree);

            return Result<List<MenuDto>>.Success(menuDtos, "查询菜单成功");
        }
        catch (Exception ex)
        {
            return Result<List<MenuDto>>.Failure($"查询菜单失败: {ex.Message}");
        }
    }

    private List<MenuDto> MapToDto(List<Menu> menus)
    {
        return menus.Select(m => new MenuDto
        {
            Id = m.Id,
            MenuName = m.MenuName,
            MenuCode = m.MenuCode,
            RoutePath = m.RoutePath,
            Icon = m.Icon,
            SortOrder = m.SortOrder,
            Children = m.Children?.Any() == true ? MapToDto(m.Children) : null
        }).ToList();
    }
}