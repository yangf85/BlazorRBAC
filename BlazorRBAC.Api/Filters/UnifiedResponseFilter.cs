// BlazorRBAC.Api/Filters/UnifiedResponseFilter.cs
using BlazorRBAC.Application.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BlazorRBAC.Api.Filters;

/// <summary>
/// 统一 API 响应格式过滤器
/// 将所有返回值包装成 Result<T> 格式
/// </summary>
public class UnifiedResponseFilter : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        // 1. 如果已经是 Result 类型，直接返回（避免重复包装）
        if (context.Result is ObjectResult { Value: Result })
        {
            // ✅ 统一返回 200 状态码
            if (context.Result is ObjectResult objectResult)
            {
                objectResult.StatusCode = 200;
            }
            return;
        }

        // 2. 处理成功返回的数据
        if (context.Result is ObjectResult successResult)
        {
            var wrappedResult = successResult.Value switch
            {
                // 已经是 Result 类型，保持不变
                Result result => result,

                // 原始数据，包装成 Result<T>
                _ => CreateGenericResult(successResult.Value)
            };

            context.Result = new ObjectResult(wrappedResult)
            {
                StatusCode = 200  // ✅ 统一返回 200
            };
            return;
        }

        // 3. 处理其他 ActionResult（如 NotFound、Unauthorized 等）
        if (ShouldWrapResult(context.Result))
        {
            var (code, message) = GetCodeAndMessage(context.Result);

            context.Result = new ObjectResult(Result.Failure(message, code))
            {
                StatusCode = 200  // ✅ 即使是错误，也返回 200
            };
        }
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
        // 不需要实现
    }

    /// <summary>
    /// 动态创建 Result<T>
    /// </summary>
    private static object CreateGenericResult(object? data)
    {
        if (data == null)
        {
            return Result.Success();
        }

        var dataType = data.GetType();
        var resultType = typeof(Result<>).MakeGenericType(dataType);

        // 调用 Result<T>.Success(T data, string message) 方法
        var successMethod = resultType.GetMethod(
            "Success",
            [dataType, typeof(string)]
        );

        return successMethod!.Invoke(null, [data, "操作成功"])!;
    }

    /// <summary>
    /// 判断是否需要包装
    /// </summary>
    private static bool ShouldWrapResult(IActionResult result)
    {
        return result is StatusCodeResult
            or NotFoundResult
            or NotFoundObjectResult
            or UnauthorizedResult
            or UnauthorizedObjectResult
            or BadRequestResult
            or BadRequestObjectResult
            or NoContentResult
            or ForbidResult;
    }

    /// <summary>
    /// 根据 ActionResult 类型获取对应的业务码和消息
    /// </summary>
    private static (ResultCode Code, string Message) GetCodeAndMessage(IActionResult result)
    {
        return result switch
        {
            // ========== 404 Not Found ==========
            NotFoundResult =>
                (ResultCode.NotFound, "资源未找到"),

            NotFoundObjectResult notFoundObj =>
                (ResultCode.NotFound, notFoundObj.Value?.ToString() ?? "资源未找到"),

            // ========== 401 Unauthorized ==========
            UnauthorizedResult =>
                (ResultCode.Unauthorized, "未授权访问"),

            UnauthorizedObjectResult unauthorizedObj =>
                (ResultCode.Unauthorized, unauthorizedObj.Value?.ToString() ?? "未授权访问"),

            // ========== 403 Forbidden ==========
            ForbidResult =>
                (ResultCode.PermissionDenied, "权限不足"),

            // ========== 400 Bad Request ==========
            BadRequestResult =>
                (ResultCode.ValidationError, "请求参数错误"),

            BadRequestObjectResult badRequestObj =>
                (ResultCode.ValidationError, GetBadRequestMessage(badRequestObj)),

            // ========== 204 No Content ==========
            NoContentResult =>
                (ResultCode.Success, "操作成功"),

            // ========== 其他状态码 ==========
            StatusCodeResult statusCodeResult =>
                GetCodeAndMessageFromStatusCode(statusCodeResult.StatusCode),

            // ========== 默认 ==========
            _ => (ResultCode.Error, "操作失败")
        };
    }

    /// <summary>
    /// 从 BadRequestObjectResult 提取错误信息
    /// </summary>
    private static string GetBadRequestMessage(BadRequestObjectResult result)
    {
        // 处理 ModelState 验证错误
        if (result.Value is ValidationProblemDetails validationProblem)
        {
            var errors = validationProblem.Errors
                .SelectMany(e => e.Value)
                .FirstOrDefault();

            return errors ?? "请求参数错误";
        }

        // 处理 SerializableError
        if (result.Value is SerializableError serializableError)
        {
            var firstError = serializableError.FirstOrDefault();
            if (firstError.Value is string[] errorMessages && errorMessages.Any())
            {
                return errorMessages.First();
            }
        }

        return result.Value?.ToString() ?? "请求参数错误";
    }

    /// <summary>
    /// 根据 HTTP 状态码获取业务码和消息
    /// </summary>
    private static (ResultCode Code, string Message) GetCodeAndMessageFromStatusCode(int statusCode)
    {
        return statusCode switch
        {
            401 => (ResultCode.Unauthorized, "未授权访问"),
            403 => (ResultCode.PermissionDenied, "权限不足"),
            404 => (ResultCode.NotFound, "资源未找到"),
            400 => (ResultCode.ValidationError, "请求参数错误"),
            409 => (ResultCode.AlreadyExists, "资源已存在"),
            500 => (ResultCode.InternalError, "服务器内部错误"),
            _ => (ResultCode.Error, "操作失败")
        };
    }
}