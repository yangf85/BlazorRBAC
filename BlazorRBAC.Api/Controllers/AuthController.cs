using BlazorRBAC.Application.Common;
using BlazorRBAC.Application.Dtos;
using BlazorRBAC.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BlazorRBAC.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// 用户注册
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        return result.IsSuccess
            ? Ok(result)
            : BadRequest(result);
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        return result.IsSuccess
            ? Ok(result)
            : Unauthorized(result);
    }
}