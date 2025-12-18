using FreeSql;
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
            .Build();

        // 注册为单例
        services.AddSingleton<IFreeSql>(freeSql);

        return services;
    }
}