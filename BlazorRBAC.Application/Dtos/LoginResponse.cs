namespace BlazorRBAC.Application.Dtos;

/// <summary>
/// 登录响应
/// </summary>
public record LoginResponse(
    string Token,
    string Username,
    List<string> Roles
);