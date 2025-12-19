using BlazorRBAC.Domain.Entities;
using FreeSql;
using FreeSql.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorRBAC.Infrastructure.Database;

public static class FreeSqlSetup
{
    public static IServiceCollection AddFreeSqlSetup(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 获取连接字符串
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new Exception("未配置数据库连接字符串");

        // 创建 FreeSql 实例
        var freeSql = new FreeSqlBuilder()
            .UseConnectionString(DataType.PostgreSQL, connectionString)
            .UseAutoSyncStructure(true)  // 自动同步实体结构到数据库
            .UseMonitorCommand(cmd =>
            {
                // 开发环境输出 SQL（可选）
                Console.WriteLine($"[SQL] {cmd.CommandText}");
            })
            .Build();

        // 注册为单例
        services.AddSingleton<IFreeSql>(freeSql);
        services.AddSingleton<SeedDataService>();

        return services;
    }

    /// <summary>
    /// 同步所有实体的表结构
    /// </summary>
    public static void SyncStructure(this IFreeSql fsql)
    {
        fsql.CodeFirst.SyncStructure<User>();
        fsql.CodeFirst.SyncStructure<Role>();
        fsql.CodeFirst.SyncStructure<Menu>();
        fsql.CodeFirst.SyncStructure<UserRole>();
        fsql.CodeFirst.SyncStructure<RoleMenu>();
    }
}