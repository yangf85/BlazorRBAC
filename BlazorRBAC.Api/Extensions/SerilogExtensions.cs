using Serilog;
using Serilog.Events;

namespace BlazorRBAC.Api.Extensions;

/// <summary>
/// Serilog 日志扩展类
/// </summary>
public static class SerilogExtensions
{
    /// <summary>
    /// 配置 Serilog 日志
    /// </summary>
    public static void ConfigureSerilog()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: "logs/app-.log",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                retainedFileCountLimit: 30)
            .CreateLogger();
    }

    /// <summary>
    /// 添加 Serilog 到 WebApplicationBuilder
    /// </summary>
    public static WebApplicationBuilder AddSerilog(
        this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog();
        return builder;
    }

    /// <summary>
    /// 配置请求日志中间件
    /// </summary>
    public static WebApplication UseRequestLogging(  // ← 改名！
        this WebApplication app)
    {
        // 调用 Serilog 提供的扩展方法
        app.UseSerilogRequestLogging();
        return app;
    }
}