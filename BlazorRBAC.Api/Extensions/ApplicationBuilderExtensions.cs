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
}