using System.Text;
using BlazorRBAC.Application.Services;
using BlazorRBAC.Infrastructure.Database;
using BlazorRBAC.Infrastructure.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;  // ← 新增

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
        services.AddFreeSqlSetup(configuration);
        return services;
    }

    /// <summary>
    /// 添加 API 文档服务
    /// </summary>
    public static IServiceCollection AddApiDocumentation(
        this IServiceCollection services)
    {
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

                // ========== JWT 安全方案 ==========
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes = new Dictionary<string, OpenApiSecurityScheme>
                {
                    ["Bearer"] = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "JWT Token（登录后获取，无需 Bearer 前缀）"
                    }
                };

                document.SecurityRequirements = new List<OpenApiSecurityRequirement>
                {
                    new()
                    {
                        [new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        }] = Array.Empty<string>()
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
        services.AddScoped<AuthService>();
        services.AddScoped<MenuService>();

        // TODO: 后续添加其他服务
        // services.AddScoped<UserService>();
        // services.AddScoped<RoleService>();

        return services;
    }

    /// <summary>
    /// 添加验证器
    /// </summary>
    public static IServiceCollection AddValidators(
        this IServiceCollection services)
    {
        // TODO: 后续实现 FluentValidation
        // services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
        return services;
    }

    /// <summary>
    /// 添加对象映射
    /// </summary>
    public static IServiceCollection AddMapster(
        this IServiceCollection services)
    {
        // TODO: 后续实现 Mapster 配置
        return services;
    }
}