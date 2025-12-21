using BlazorRBAC.Domain.Entities;
using BlazorRBAC.Domain.Enums;

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
        Console.WriteLine("🗑️  开始清空数据...");
        await _fsql.Delete<RoleMenu>().Where(x => true).ExecuteAffrowsAsync();
        await _fsql.Delete<UserRole>().Where(x => true).ExecuteAffrowsAsync();
        await _fsql.Delete<Menu>().Where(x => true).ExecuteAffrowsAsync();
        await _fsql.Delete<User>().Where(x => true).ExecuteAffrowsAsync();
        await _fsql.Delete<Role>().Where(x => true).ExecuteAffrowsAsync();
        Console.WriteLine("✅ 数据清空完成");
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
            Console.WriteLine("⚠️  数据已存在，跳过初始化");
            return;
        }

        Console.WriteLine("========== 🌱 开始初始化种子数据 ==========");

        // ========== 1. 插入 5 种角色 ==========
        var roles = new List<Role>
        {
            new()
            {
                RoleName = "超级管理员",
                RoleCode = "SuperAdmin",
                Description = "系统超级管理员，拥有所有权限，不可删除",
                IsSystem = true
            },
            new()
            {
                RoleName = "系统管理员",
                RoleCode = "Admin",
                Description = "系统管理员，拥有大部分管理权限",
                IsSystem = false
            },
            new()
            {
                RoleName = "部门经理",
                RoleCode = "Manager",
                Description = "部门经理，拥有部门内的管理权限",
                IsSystem = false
            },
            new()
            {
                RoleName = "普通用户",
                RoleCode = "User",
                Description = "普通用户，只能访问基础功能",
                IsSystem = false
            },
            new()
            {
                RoleName = "访客",
                RoleCode = "Guest",
                Description = "访客用户，只能查看个人信息",
                IsSystem = false
            }
        };

        roles = await _fsql.Insert(roles).ExecuteInsertedAsync();

        Console.WriteLine($"✅ 插入角色: {roles.Count} 条");
        foreach (var role in roles)
        {
            Console.WriteLine($"   - ID={role.Id}, Code={role.RoleCode}, Name={role.RoleName}");
        }

        // ========== 2. 插入菜单（3级结构）==========

        // 一级菜单
        var level1Menus = new List<Menu>
        {
            new() { MenuName = "系统管理", MenuCode = "system", Icon = "Settings", SortOrder = 1 },
            new() { MenuName = "业务管理", MenuCode = "business", Icon = "BusinessCenter", SortOrder = 2 },
            new() { MenuName = "个人中心", MenuCode = "profile", Icon = "Person", SortOrder = 3 }
        };

        level1Menus = await _fsql.Insert(level1Menus).ExecuteInsertedAsync();
        Console.WriteLine($"✅ 插入一级菜单: {level1Menus.Count} 条");

        var systemMenu = level1Menus[0];
        var businessMenu = level1Menus[1];
        var profileMenu = level1Menus[2];

        // 二级菜单
        var level2Menus = new List<Menu>
        {
            // 系统管理 - 二级
            new() { ParentId = systemMenu.Id, MenuName = "用户管理", MenuCode = "user-mgr", Icon = "People", SortOrder = 1 },
            new() { ParentId = systemMenu.Id, MenuName = "角色管理", MenuCode = "role-mgr", Icon = "Shield", SortOrder = 2 },
            new() { ParentId = systemMenu.Id, MenuName = "菜单管理", MenuCode = "menu-mgr", Icon = "Menu", SortOrder = 3 },
            new() { ParentId = systemMenu.Id, MenuName = "日志管理", MenuCode = "log-mgr", Icon = "Description", SortOrder = 4 },

            // 业务管理 - 二级
            new() { ParentId = businessMenu.Id, MenuName = "订单管理", MenuCode = "order-mgr", Icon = "ShoppingCart", SortOrder = 1 },
            new() { ParentId = businessMenu.Id, MenuName = "产品管理", MenuCode = "product-mgr", Icon = "Inventory", SortOrder = 2 },
            new() { ParentId = businessMenu.Id, MenuName = "客户管理", MenuCode = "customer-mgr", Icon = "Group", SortOrder = 3 },

            // 个人中心 - 二级
            new() { ParentId = profileMenu.Id, MenuName = "个人信息", MenuCode = "profile-info", Icon = "AccountCircle", SortOrder = 1 },
            new() { ParentId = profileMenu.Id, MenuName = "修改密码", MenuCode = "change-pwd", Icon = "Lock", SortOrder = 2 }
        };

        level2Menus = await _fsql.Insert(level2Menus).ExecuteInsertedAsync();
        Console.WriteLine($"✅ 插入二级菜单: {level2Menus.Count} 条");

        var userMgr = level2Menus[0];
        var roleMgr = level2Menus[1];
        var orderMgr = level2Menus[4];

        // 三级菜单
        var level3Menus = new List<Menu>
        {
            // 用户管理 - 三级
            new() { ParentId = userMgr.Id, MenuName = "用户列表", MenuCode = "user-list", RoutePath = "/admin/users/list", SortOrder = 1 },
            new() { ParentId = userMgr.Id, MenuName = "添加用户", MenuCode = "user-add", RoutePath = "/admin/users/add", SortOrder = 2 },

            // 角色管理 - 三级
            new() { ParentId = roleMgr.Id, MenuName = "角色列表", MenuCode = "role-list", RoutePath = "/admin/roles/list", SortOrder = 1 },
            new() { ParentId = roleMgr.Id, MenuName = "权限分配", MenuCode = "role-perm", RoutePath = "/admin/roles/permission", SortOrder = 2 },

            // 订单管理 - 三级
            new() { ParentId = orderMgr.Id, MenuName = "订单列表", MenuCode = "order-list", RoutePath = "/business/orders/list", SortOrder = 1 },
            new() { ParentId = orderMgr.Id, MenuName = "订单统计", MenuCode = "order-stats", RoutePath = "/business/orders/stats", SortOrder = 2 }
        };

        level3Menus = await _fsql.Insert(level3Menus).ExecuteInsertedAsync();
        Console.WriteLine($"✅ 插入三级菜单: {level3Menus.Count} 条");

        // ========== 3. 插入 10 个用户 ==========
        var users = new List<User>
        {
            // 1. 超级管理员
            new()
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123", 12),
                RealName = "系统管理员",
                Email = "admin@blazorrbac.com",
                Phone = "13800138000",
                LoginType = LoginType.Local,
                IsActive = true
            },

            // 2. 系统管理员
            new()
            {
                Username = "admin2",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123", 12),
                RealName = "张三",
                Email = "zhangsan@blazorrbac.com",
                Phone = "13800138001",
                LoginType = LoginType.Local,
                IsActive = true
            },

            // 3-4. 部门经理
            new()
            {
                Username = "manager",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123", 12),
                RealName = "李四",
                Email = "lisi@blazorrbac.com",
                Phone = "13800138002",
                LoginType = LoginType.Local,
                IsActive = true
            },
            new()
            {
                Username = "manager2",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123", 12),
                RealName = "王五",
                Email = "wangwu@blazorrbac.com",
                Phone = "13800138003",
                LoginType = LoginType.Local,
                IsActive = true
            },

            // 5-8. 普通用户
            new()
            {
                Username = "user1",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123", 12),
                RealName = "赵六",
                Email = "user1@blazorrbac.com",
                Phone = "13800138004",
                LoginType = LoginType.Local,
                IsActive = true
            },
            new()
            {
                Username = "user2",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123", 12),
                RealName = "钱七",
                Email = "user2@blazorrbac.com",
                Phone = "13800138005",
                LoginType = LoginType.Local,
                IsActive = true
            },
            new()
            {
                Username = "user3",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123", 12),
                RealName = "孙八",
                Email = "user3@blazorrbac.com",
                Phone = "13800138006",
                LoginType = LoginType.Local,
                IsActive = true
            },
            new()
            {
                Username = "test",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123", 12),
                RealName = "测试用户",
                Email = "test@blazorrbac.com",
                Phone = "13800138007",
                LoginType = LoginType.Local,
                IsActive = true
            },

            // 9-10. 访客
            new()
            {
                Username = "guest1",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123", 12),
                RealName = "访客一",
                Email = "guest1@blazorrbac.com",
                Phone = "13800138008",
                LoginType = LoginType.Local,
                IsActive = true
            },
            new()
            {
                Username = "guest2",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123", 12),
                RealName = "访客二",
                Email = "guest2@blazorrbac.com",
                Phone = "13800138009",
                LoginType = LoginType.Local,
                IsActive = false  // 禁用状态
            }
        };

        users = await _fsql.Insert(users).ExecuteInsertedAsync();

        Console.WriteLine($"✅ 插入用户: {users.Count} 条");
        foreach (var user in users)
        {
            Console.WriteLine($"   - ID={user.Id}, Username={user.Username}, RealName={user.RealName}, Active={user.IsActive}");
        }

        // ========== 4. 分配角色（用户-角色关联）==========
        var userRoles = new List<UserRole>
        {
            // admin → SuperAdmin
            new() { UserId = users[0].Id, RoleId = roles[0].Id },

            // admin2 → Admin
            new() { UserId = users[1].Id, RoleId = roles[1].Id },

            // manager, manager2 → Manager
            new() { UserId = users[2].Id, RoleId = roles[2].Id },
            new() { UserId = users[3].Id, RoleId = roles[2].Id },

            // user1, user2, user3, test → User
            new() { UserId = users[4].Id, RoleId = roles[3].Id },
            new() { UserId = users[5].Id, RoleId = roles[3].Id },
            new() { UserId = users[6].Id, RoleId = roles[3].Id },
            new() { UserId = users[7].Id, RoleId = roles[3].Id },

            // guest1, guest2 → Guest
            new() { UserId = users[8].Id, RoleId = roles[4].Id },
            new() { UserId = users[9].Id, RoleId = roles[4].Id }
        };

        await _fsql.Insert(userRoles).ExecuteAffrowsAsync();
        Console.WriteLine($"✅ 分配用户角色: {userRoles.Count} 条");

        // ========== 5. 分配菜单（角色-菜单关联）==========
        var allMenuIds = await _fsql.Select<Menu>().ToListAsync(m => m.Id);

        // SuperAdmin - 所有菜单
        var superAdminMenus = allMenuIds.Select(id => new RoleMenu
        {
            RoleId = roles[0].Id,
            MenuId = id
        });

        // Admin - 除日志管理外的所有菜单
        var adminMenus = allMenuIds
            .Where(id => !level2Menus.Any(m => m.MenuCode == "log-mgr" && m.Id == id))
            .Select(id => new RoleMenu { RoleId = roles[1].Id, MenuId = id });

        // Manager - 用户管理、角色管理、业务管理、个人中心
        var managerMenuIds = new List<int>();
        managerMenuIds.AddRange(new[] { systemMenu.Id, userMgr.Id, roleMgr.Id });
        managerMenuIds.AddRange(new[] { businessMenu.Id, orderMgr.Id, profileMenu.Id });
        managerMenuIds.AddRange(level2Menus.Where(m => m.ParentId == businessMenu.Id || m.ParentId == profileMenu.Id).Select(m => m.Id));
        managerMenuIds.AddRange(level3Menus.Where(m => new[] { userMgr.Id, roleMgr.Id, orderMgr.Id }.Contains(m.ParentId)).Select(m => m.Id));

        var managerMenus = managerMenuIds.Distinct().Select(id => new RoleMenu { RoleId = roles[2].Id, MenuId = id });

        // User - 业务管理、个人中心
        var userMenuIds = new List<int> { businessMenu.Id, profileMenu.Id };
        userMenuIds.AddRange(level2Menus.Where(m => m.ParentId == businessMenu.Id || m.ParentId == profileMenu.Id).Select(m => m.Id));
        userMenuIds.AddRange(level3Menus.Where(m => level2Menus.Any(l2 => (l2.ParentId == businessMenu.Id || l2.ParentId == profileMenu.Id) && l2.Id == m.ParentId)).Select(m => m.Id));

        var userMenus = userMenuIds.Distinct().Select(id => new RoleMenu { RoleId = roles[3].Id, MenuId = id });

        // Guest - 仅个人中心
        var guestMenuIds = new List<int> { profileMenu.Id };
        guestMenuIds.AddRange(level2Menus.Where(m => m.ParentId == profileMenu.Id && m.MenuCode == "profile-info").Select(m => m.Id));

        var guestMenus = guestMenuIds.Select(id => new RoleMenu { RoleId = roles[4].Id, MenuId = id });

        // 合并所有角色菜单
        var allRoleMenus = superAdminMenus
            .Concat(adminMenus)
            .Concat(managerMenus)
            .Concat(userMenus)
            .Concat(guestMenus)
            .ToList();

        await _fsql.Insert(allRoleMenus).ExecuteAffrowsAsync();
        Console.WriteLine($"✅ 分配角色菜单: {allRoleMenus.Count} 条");
        Console.WriteLine($"   - SuperAdmin: {superAdminMenus.Count()} 个菜单（所有）");
        Console.WriteLine($"   - Admin: {adminMenus.Count()} 个菜单");
        Console.WriteLine($"   - Manager: {managerMenus.Count()} 个菜单");
        Console.WriteLine($"   - User: {userMenus.Count()} 个菜单");
        Console.WriteLine($"   - Guest: {guestMenus.Count()} 个菜单");

        Console.WriteLine("========== ✅ 种子数据初始化完成 ==========");
        Console.WriteLine("\n📝 测试账号（密码统一为 123）:");
        Console.WriteLine("   超级管理员: admin");
        Console.WriteLine("   系统管理员: admin2");
        Console.WriteLine("   部门经理: manager, manager2");
        Console.WriteLine("   普通用户: user1, user2, user3, test");
        Console.WriteLine("   访客: guest1, guest2（guest2已禁用）");
    }
}