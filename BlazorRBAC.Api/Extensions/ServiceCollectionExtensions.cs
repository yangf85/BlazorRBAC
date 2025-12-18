using BlazorRBAC.Infrastructure.Database;

namespace BlazorRBAC.Api.Extensions;

/// <summary>
/// 服务注册扩展类
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加数据库服务
    /// </summary>
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 调用 Infrastructure 层的 FreeSql 配置
        services.AddFreeSqlSetup(configuration);

        return services;
    }

    /// <summary>
    /// 添加 API 文档服务
    /// </summary>
    public static IServiceCollection AddApiDocumentation(
        this IServiceCollection services)
    {
        // OpenAPI 文档生成
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info = new()
                {
                    Title = "BlazorRBAC API",
                    Version = "v1",
                    Description = "基于 Blazor Server 的 RBAC 权限管理系统 API",
                    Contact = new()
                    {
                        Name = "开发团队",
                        Email = "dev@blazorrbac.com"
                    }
                };

                return Task.CompletedTask;
            });
        });

        return services;
    }

    /// <summary>
    /// 添加认证服务（JWT）
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // TODO: Day 4 实现
        return services;
    }

    /// <summary>
    /// 添加业务服务
    /// </summary>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        // TODO: 注册 Application 层的服务
        // services.AddScoped<IUserService, UserService>();

        return services;
    }

    /// <summary>
    /// 添加验证器
    /// </summary>
    public static IServiceCollection AddValidators(
        this IServiceCollection services)
    {
        // TODO: Day 3 实现 FluentValidation
        return services;
    }

    /// <summary>
    /// 添加对象映射
    /// </summary>
    public static IServiceCollection AddMapster(
        this IServiceCollection services)
    {
        // TODO: Day 3 实现 Mapster 配置
        return services;
    }
}