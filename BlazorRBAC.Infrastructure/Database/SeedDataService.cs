using BlazorRBAC.Domain.Entities;
using BlazorRBAC.Domain.Enums;
using BCrypt.Net;

namespace BlazorRBAC.Infrastructure.Database;

/// <summary>
/// 种子数据服务
/// </summary>
public class SeedDataService
{
    private readonly IFreeSql _fsql;

    public SeedDataService(IFreeSql fsql)
    {
        _fsql = fsql;
    }

    /// <summary>
    /// 清空所有数据（保留表结构）
    /// </summary>
    public async Task CleanDataAsync()
    {
        // 按照外键依赖顺序删除（先删除关联表，再删除主表）
        await _fsql.Delete<RoleMenu>().Where(x => true).ExecuteAffrowsAsync();
        await _fsql.Delete<UserRole>().Where(x => true).ExecuteAffrowsAsync();
        await _fsql.Delete<Menu>().Where(x => true).ExecuteAffrowsAsync();
        await _fsql.Delete<User>().Where(x => true).ExecuteAffrowsAsync();
        await _fsql.Delete<Role>().Where(x => true).ExecuteAffrowsAsync();
    }

    /// <summary>
    /// 重置数据（清空 + 初始化）
    /// </summary>
    public async Task ResetDataAsync()
    {
        await CleanDataAsync();
        await InitializeAsync();
    }

    /// <summary>
    /// 初始化种子数据
    /// </summary>
    public async Task InitializeAsync()
    {
        // 检查是否已有数据
        if (await _fsql.Select<User>().AnyAsync())
        {
            return;
        }

        // 插入角色
        var roles = new List<Role>
        {
            new() { RoleName = "超级管理员", RoleCode = "SuperAdmin", Description = "系统超级管理员，拥有所有权限", IsSystem = true },
            new() { RoleName = "管理员", RoleCode = "Admin", Description = "普通管理员，拥有部分管理权限", IsSystem = false },
            new() { RoleName = "普通用户", RoleCode = "User", Description = "普通用户，只能访问基础功能", IsSystem = false }
        };
        await _fsql.Insert(roles).ExecuteAffrowsAsync();

        // 插入一级菜单
        var menus = new List<Menu>
        {
            new() { MenuName = "系统管理", MenuCode = "system", Icon = "Settings", SortOrder = 1 },
            new() { MenuName = "个人中心", MenuCode = "profile", Icon = "Person", SortOrder = 2 }
        };
        await _fsql.Insert(menus).ExecuteAffrowsAsync();

        var systemMenu = menus[0];
        var profileMenu = menus[1];

        // 插入二级菜单
        var subMenus = new List<Menu>
        {
            new() { ParentId = systemMenu.Id, MenuName = "用户管理", MenuCode = "user-mgr", Icon = "People", RoutePath = "/admin/users", SortOrder = 1 },
            new() { ParentId = systemMenu.Id, MenuName = "角色管理", MenuCode = "role-mgr", Icon = "Shield", RoutePath = "/admin/roles", SortOrder = 2 },
            new() { ParentId = systemMenu.Id, MenuName = "菜单管理", MenuCode = "menu-mgr", Icon = "Menu", RoutePath = "/admin/menus", SortOrder = 3 },
            new() { ParentId = profileMenu.Id, MenuName = "个人信息", MenuCode = "profile-info", Icon = "AccountCircle", RoutePath = "/profile/info", SortOrder = 1 }
        };
        await _fsql.Insert(subMenus).ExecuteAffrowsAsync();

        // 插入用户
        var adminUser = new User
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123", 12),
            RealName = "系统管理员",
            Email = "admin@example.com",
            Phone = "13800138000",
            LoginType = LoginType.Local,
            IsActive = true
        };
        await _fsql.Insert(adminUser).ExecuteAffrowsAsync();

        // 分配角色
        await _fsql.Insert(new UserRole { UserId = adminUser.Id, RoleId = roles[0].Id }).ExecuteAffrowsAsync();

        // 分配菜单
        var allMenuIds = await _fsql.Select<Menu>().ToListAsync(m => m.Id);
        var roleMenus = allMenuIds.Select(menuId => new RoleMenu { RoleId = roles[0].Id, MenuId = menuId });
        await _fsql.Insert(roleMenus).ExecuteAffrowsAsync();
    }
}