using BlazorRBAC.Api.Middleware;
using Scalar.AspNetCore;

namespace BlazorRBAC.Api.Extensions;

/// <summary>
/// 应用程序中间件扩展类
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// 配置 API 文档（开发环境）
    /// </summary>
    public static WebApplication UseApiDocumentation(
        this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            // 映射 OpenAPI 端点
            app.MapOpenApi();

            // 映射 Scalar API 文档界面
            app.MapScalarApiReference(options =>
            {
                options
                    .WithTitle("BlazorRBAC API 文档")
                    .WithTheme(ScalarTheme.Default)
                    .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
            });
        }

        return app;
    }

    /// <summary>
    /// 添加全局异常处理中间件
    /// </summary>
    /// <remarks>
    /// 必须在其他中间件之前注册，以捕获所有未处理的异常
    /// </remarks>
    public static IApplicationBuilder UseGlobalExceptionHandler(
        this IApplicationBuilder app)
    {
        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
        return app;
    }

    /// <summary>
    /// 配置认证和授权中间件
    /// 注意：必须在 UseRouting 之后、MapControllers 之前
    /// </summary>
    public static WebApplication UseAuthenticationAndAuthorization(
        this WebApplication app)
    {
        // ⚠️ 顺序很重要：Authentication 必须在 Authorization 之前
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}