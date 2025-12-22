using BlazorRBAC.Api.Extensions;
using Serilog;

// ==================== 配置 Serilog 日志 ====================
SerilogExtensions.ConfigureSerilog();

try
{
    Log.Information("启动 BlazorRBAC API...");

    var builder = WebApplication.CreateBuilder(args);

    // ==================== 服务注册 ====================

    // 日志
    builder.AddSerilog();

    // 控制器
    builder.Services.AddControllersWithFilters();

    // 数据库
    builder.Services.AddDatabase(builder.Configuration);

    // API 文档
    builder.Services.AddApiDocumentation();

    // JWT 认证
    builder.Services.AddJwtAuthentication(builder.Configuration);

    // ? 新增：CORS 配置
    builder.Services.AddCorsPolicy(builder.Configuration);

    // 业务服务
    builder.Services.AddApplicationServices();

    // ==================== 中间件配置 ====================

    var app = builder.Build();

    app.UseGlobalExceptionHandler();

    // API 文档（仅开发环境）
    app.UseApiDocumentation();

    // HTTPS 重定向
    app.UseHttpsRedirection();

    // ? 新增：CORS 中间件（必须在 UseAuthentication 之前）
    app.UseCorsPolicy();

    // 请求日志
    app.UseRequestLogging();

    // 认证和授权
    app.UseAuthenticationAndAuthorization();

    // 路由
    app.MapControllers();

    // ==================== 启动应用 ====================

    Log.Information("BlazorRBAC API 启动成功！");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "应用启动失败");
    throw;
}
finally
{
    Log.CloseAndFlush();
}