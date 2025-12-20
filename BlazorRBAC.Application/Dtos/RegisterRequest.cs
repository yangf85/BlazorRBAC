namespace BlazorRBAC.Application.Dtos;

/// <summary>
/// 注册请求
/// </summary>
public record RegisterRequest(
    string Username,
    string Password,
    string RealName,
    string Email
);