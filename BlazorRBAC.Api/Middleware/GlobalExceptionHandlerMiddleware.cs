using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace BlazorRBAC.Api.Middleware;

/// <summary>
/// 全局异常处理中间件
/// 捕获所有未处理的异常，统一返回格式化的错误响应
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // 记录异常日志
        _logger.LogError(exception,
            "发生未处理的异常: {Message}\n请求路径: {Path}\n请求方法: {Method}",
            exception.Message,
            context.Request.Path,
            context.Request.Method);

        // 设置响应头
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)GetStatusCode(exception);

        // 构造错误响应
        var response = new ProblemDetails
        {
            Status = context.Response.StatusCode,
            Title = GetTitle(exception),
            Detail = GetDetail(exception),
            Instance = context.Request.Path
        };

        // 开发环境返回详细错误信息
        if (_env.IsDevelopment())
        {
            response.Extensions["traceId"] = context.TraceIdentifier;
            response.Extensions["exception"] = exception.GetType().Name;
            response.Extensions["stackTrace"] = exception.StackTrace;
        }

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _env.IsDevelopment()
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, options));
    }

    /// <summary>
    /// 根据异常类型获取HTTP状态码
    /// </summary>
    private static HttpStatusCode GetStatusCode(Exception exception) =>
        exception switch
        {
            ArgumentNullException => HttpStatusCode.BadRequest,
            ArgumentException => HttpStatusCode.BadRequest,
            KeyNotFoundException => HttpStatusCode.NotFound,
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            InvalidOperationException => HttpStatusCode.BadRequest,
            NotImplementedException => HttpStatusCode.NotImplemented,
            _ => HttpStatusCode.InternalServerError
        };

    /// <summary>
    /// 根据异常类型获取错误标题
    /// </summary>
    private static string GetTitle(Exception exception) =>
        exception switch
        {
            ArgumentNullException => "参数不能为空",
            ArgumentException => "参数错误",
            KeyNotFoundException => "资源未找到",
            UnauthorizedAccessException => "未授权访问",
            InvalidOperationException => "操作无效",
            NotImplementedException => "功能未实现",
            _ => "服务器内部错误"
        };

    /// <summary>
    /// 获取错误详情（生产环境隐藏敏感信息）
    /// </summary>
    private string GetDetail(Exception exception)
    {
        // 生产环境返回通用错误信息
        if (!_env.IsDevelopment())
        {
            return "处理请求时发生错误，请稍后重试或联系管理员";
        }

        // 开发环境返回详细错误
        return exception.Message;
    }
}