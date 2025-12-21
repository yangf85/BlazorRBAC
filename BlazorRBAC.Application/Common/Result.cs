namespace BlazorRBAC.Application.Common;

/// <summary>
/// 统一 API 响应结果
/// </summary>
public class Result
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;

    public static Result Success(string message = "操作成功")
        => new() { IsSuccess = true, Message = message };

    public static Result Failure(string message)
        => new() { IsSuccess = false, Message = message };
}

/// <summary>
/// 带数据的响应结果
/// </summary>
public class Result<T> : Result
{
    public T? Data { get; set; }

    public static Result<T> Success(T data, string message = "操作成功")
        => new() { IsSuccess = true, Data = data, Message = message };

    public static new Result<T> Failure(string message)
        => new() { IsSuccess = false, Message = message };
}