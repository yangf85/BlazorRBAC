namespace BlazorRBAC.Application.Dtos;

/// <summary>
/// 登录请求
/// </summary>
public record LoginRequest(
    string Username,
    string Password
);