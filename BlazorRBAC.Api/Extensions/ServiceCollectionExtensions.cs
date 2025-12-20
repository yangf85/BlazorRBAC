using System.Text;
using BlazorRBAC.Application.Services;
using BlazorRBAC.Infrastructure.Database;
using BlazorRBAC.Infrastructure.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

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
        // 1. 注册 JWT 配置
        services.Configure<JwtSettings>(
            configuration.GetSection(JwtSettings.SectionName)
        );

        // 2. 注册 JWT 服务
        services.AddScoped<JwtService>();

        // 3. 配置 JWT 认证
        var jwtSettings = configuration
            .GetSection(JwtSettings.SectionName)
            .Get<JwtSettings>()!;

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        return services;
    }

    /// <summary>
    /// 添加业务服务
    /// </summary>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        // 注册 Application 层的服务
        services.AddScoped<AuthService>();

        // TODO: 后续添加其他服务
        // services.AddScoped<UserService>();
        // services.AddScoped<RoleService>();
        // services.AddScoped<MenuService>();

        return services;
    }

    /// <summary>
    /// 添加验证器
    /// </summary>
    public static IServiceCollection AddValidators(
        this IServiceCollection services)
    {
        // TODO: Day 后续实现 FluentValidation
        // services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
        return services;
    }

    /// <summary>
    /// 添加对象映射
    /// </summary>
    public static IServiceCollection AddMapster(
        this IServiceCollection services)
    {
        // TODO: Day 后续实现 Mapster 配置
        // services.AddMapster();
        return services;
    }
}