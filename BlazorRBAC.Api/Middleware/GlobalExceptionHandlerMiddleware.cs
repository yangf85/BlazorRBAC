// BlazorRBAC.Api/Middleware/GlobalExceptionHandlerMiddleware.cs
using System.Text.Json;
using System.Text.Json.Serialization;
using BlazorRBAC.Application.Common;

namespace BlazorRBAC.Api.Middleware;

/// <summary>
/// 全局异常处理中间件
/// 捕获所有未处理的异常，统一返回 Result 格式
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    // ✅ 静态字段缓存 JSON 配置（只创建一次，所有请求复用）
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,  // 生产环境不格式化（节省带宽）
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

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

    /// <summary>
    /// 处理异常并返回统一格式的响应
    /// </summary>
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
        context.Response.StatusCode = 200;  // 所有响应都返回 200

        // 根据异常类型获取业务码和消息
        var (code, message) = GetCodeAndMessage(exception);

        // 构造 Result 响应
        Result result;

        if (_env.IsDevelopment())
        {
            // 开发环境：返回详细错误信息
            result = new DevelopmentErrorResult
            {
                IsSuccess = false,
                Code = code,
                Message = message,
                ExceptionType = exception.GetType().Name,
                StackTrace = exception.StackTrace,
                InnerException = exception.InnerException?.Message
            };
        }
        else
        {
            // 生产环境：返回简化错误信息
            result = Result.Failure(message, code);
        }

        // ✅ 使用静态字段配置序列化
        await context.Response.WriteAsync(
            JsonSerializer.Serialize(result, JsonOptions));
    }

    /// <summary>
    /// 根据异常类型获取业务码和错误消息
    /// </summary>
    private (ResultCode Code, string Message) GetCodeAndMessage(Exception exception)
    {
        // 根据环境决定消息内容
        var message = _env.IsDevelopment()
            ? exception.Message
            : GetProductionMessage(exception);

        // 映射异常类型到业务码
        var code = exception switch
        {
            // ========== 参数相关 ==========
            ArgumentNullException => ResultCode.ValidationError,
            ArgumentException => ResultCode.ValidationError,

            // ========== 资源相关 ==========
            KeyNotFoundException => ResultCode.NotFound,

            // ========== 授权相关 ==========
            UnauthorizedAccessException => ResultCode.Unauthorized,

            // ========== 业务相关 ==========
            InvalidOperationException => ResultCode.OperationDenied,

            // ========== 系统相关 ==========
            NotImplementedException => ResultCode.InternalError,
            TimeoutException => ResultCode.InternalError,

            // ========== 数据库相关（可选，需要引用对应的异常类型）==========
            // FreeSql.Common.Exception => ResultCode.DatabaseError,
            // System.Data.Common.DbException => ResultCode.DatabaseError,

            // ========== 默认 ==========
            _ => ResultCode.InternalError
        };

        return (code, message);
    }

    /// <summary>
    /// 获取生产环境的通用错误消息（隐藏敏感信息）
    /// </summary>
    private static string GetProductionMessage(Exception exception)
    {
        return exception switch
        {
            ArgumentNullException => "参数不能为空",
            ArgumentException => "参数错误",
            KeyNotFoundException => "资源未找到",
            UnauthorizedAccessException => "未授权访问",
            InvalidOperationException => "操作无效",
            NotImplementedException => "功能暂未实现",
            TimeoutException => "请求超时，请稍后重试",
            _ => "服务器内部错误，请稍后重试或联系管理员"
        };
    }

    /// <summary>
    /// 开发环境错误响应（包含详细调试信息）
    /// </summary>
    private class DevelopmentErrorResult : Result
    {
        /// <summary>
        /// 异常类型名称
        /// </summary>
        public string? ExceptionType { get; set; }

        /// <summary>
        /// 堆栈跟踪
        /// </summary>
        public string? StackTrace { get; set; }

        /// <summary>
        /// 内部异常消息
        /// </summary>
        public string? InnerException { get; set; }
    }
}