// BlazorRBAC.Application/Common/Result.cs
namespace BlazorRBAC.Application.Common;

/// <summary>
/// 统一 API 响应结果
/// </summary>
public class Result
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 业务状态码
    /// </summary>
    public ResultCode Code { get; set; }

    /// <summary>
    /// 响应消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    // ========== 成功方法 ==========

    public static Result Success(string message = "操作成功")
        => new()
        {
            IsSuccess = true,
            Code = ResultCode.Success,
            Message = message
        };

    // ========== 失败方法（通用）==========

    public static Result Failure(string message, ResultCode code = ResultCode.Error)
        => new()
        {
            IsSuccess = false,
            Code = code,
            Message = message
        };

    // ========== 便捷失败方法 ==========

    public static Result NotFound(string message = "资源不存在")
        => Failure(message, ResultCode.NotFound);

    public static Result ValidationError(string message)
        => Failure(message, ResultCode.ValidationError);

    public static Result Unauthorized(string message = "未授权访问")
        => Failure(message, ResultCode.Unauthorized);

    public static Result PermissionDenied(string message = "权限不足")
        => Failure(message, ResultCode.PermissionDenied);

    public static Result AlreadyExists(string message)
        => Failure(message, ResultCode.AlreadyExists);

    public static Result OperationDenied(string message)
        => Failure(message, ResultCode.OperationDenied);
}

/// <summary>
/// 带数据的响应结果
/// </summary>
public class Result<T> : Result
{
    /// <summary>
    /// 响应数据
    /// </summary>
    public T? Data { get; set; }

    // ========== 成功方法 ==========

    public static Result<T> Success(T data, string message = "操作成功")
        => new()
        {
            IsSuccess = true,
            Code = ResultCode.Success,
            Data = data,
            Message = message
        };

    // ========== 失败方法（通用）==========

    public static new Result<T> Failure(string message, ResultCode code = ResultCode.Error)
        => new()
        {
            IsSuccess = false,
            Code = code,
            Message = message
        };

    // ========== 便捷失败方法 ==========

    public static new Result<T> NotFound(string message = "资源不存在")
        => Failure(message, ResultCode.NotFound);

    public static new Result<T> ValidationError(string message)
        => Failure(message, ResultCode.ValidationError);

    public static new Result<T> Unauthorized(string message = "未授权访问")
        => Failure(message, ResultCode.Unauthorized);

    public static new Result<T> PermissionDenied(string message = "权限不足")
        => Failure(message, ResultCode.PermissionDenied);

    public static new Result<T> AlreadyExists(string message)
        => Failure(message, ResultCode.AlreadyExists);

    public static new Result<T> OperationDenied(string message)
        => Failure(message, ResultCode.OperationDenied);
}