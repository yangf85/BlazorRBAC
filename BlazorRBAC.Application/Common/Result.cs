namespace BlazorRBAC.Application.Common;

/// <summary>
/// 统一 API 响应结果
/// </summary>
public class Result
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public static Result Ok(string message = "操作成功")
        => new() { Success = true, Message = message };

    public static Result Fail(string message)
        => new() { Success = false, Message = message };
}

/// <summary>
/// 带数据的响应结果
/// </summary>
public class Result<T> : Result
{
    public T? Data { get; set; }

    public static Result<T> Ok(T data, string message = "操作成功")
        => new() { Success = true, Data = data, Message = message };

    public static new Result<T> Fail(string message)
        => new() { Success = false, Message = message };
}