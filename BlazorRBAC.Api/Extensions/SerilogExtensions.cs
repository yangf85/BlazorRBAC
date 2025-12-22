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
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} | IP: {ClientIP} | User: {UserName} ({UserId}){NewLine}{Exception}")
            .WriteTo.File(
                path: "logs/app-.log",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} | IP: {ClientIP} | User: {UserName} ({UserId}) | RequestId: {RequestId}{NewLine}{Exception}",
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
    /// 配置请求日志中间件（增强版）
    /// </summary>
    /// <remarks>
    /// 自动记录：
    /// - 请求方法、路径、状态码、耗时
    /// - 客户端 IP（支持代理）
    /// - 已登录用户的 ID 和用户名
    /// - 根据状态码和响应时间自动调整日志级别
    /// </remarks>
    public static WebApplication UseRequestLogging(
        this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            // ========== 自定义日志消息模板 ==========
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";

            // ========== 根据状态码和响应时间设置日志级别 ==========
            options.GetLevel = (httpContext, elapsed, ex) =>
            {
                // 1. 异常 → Error
                if (ex != null)
                {
                    return LogEventLevel.Error;
                }

                // 2. 5xx 错误 → Error
                if (httpContext.Response.StatusCode >= 500)
                {
                    return LogEventLevel.Error;
                }

                // 3. 4xx 错误 → Warning
                if (httpContext.Response.StatusCode >= 400)
                {
                    return LogEventLevel.Warning;
                }

                // 4. 慢请求（超过 5 秒）→ Warning
                if (elapsed > 5000)
                {
                    return LogEventLevel.Warning;
                }

                // 5. 其他 → Information
                return LogEventLevel.Information;
            };

            // ========== 丰富日志上下文信息 ==========
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                // 1. 记录客户端 IP（支持代理场景）
                diagnosticContext.Set("ClientIP", GetClientIpAddress(httpContext));

                // 2. 记录请求 ID（用于链路追踪）
                diagnosticContext.Set("RequestId", httpContext.TraceIdentifier);

                // 3. 记录请求头信息
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());

                // 4. 记录已登录用户信息
                if (httpContext.User?.Identity?.IsAuthenticated == true)
                {
                    var userId = httpContext.User.FindFirst(
                        System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                    var userName = httpContext.User.FindFirst(
                        System.Security.Claims.ClaimTypes.Name)?.Value;

                    diagnosticContext.Set("UserId", userId);
                    diagnosticContext.Set("UserName", userName);
                }
            };
        });

        return app;
    }

    /// <summary>
    /// 获取客户端 IP 地址
    /// </summary>
    /// <remarks>
    /// 优先级：
    /// 1. X-Forwarded-For（代理/负载均衡场景）
    /// 2. X-Real-IP（Nginx 代理场景）
    /// 3. RemoteIpAddress（直连场景）
    /// </remarks>
    private static string GetClientIpAddress(HttpContext context)
    {
        // 优先从 X-Forwarded-For 获取（代理/负载均衡场景）
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // X-Forwarded-For 可能包含多个 IP，格式：client, proxy1, proxy2
            // 取第一个 IP（真实客户端 IP）
            return forwardedFor.Split(',').First().Trim();
        }

        // 其次从 X-Real-IP 获取（Nginx 代理场景）
        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // 最后使用远程 IP（直连场景）
        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}