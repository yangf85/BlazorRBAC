using BlazorRBAC.Application.Common;
using BlazorRBAC.Application.Dtos;
using WebApiClientCore;
using WebApiClientCore.Attributes;

namespace BlazorRBAC.Web.Services;

/// <summary>
/// 认证 API 客户端接口
/// 使用 WebApiClientCore 自动生成实现
/// </summary>
[HttpHost("https://localhost:7129")]  // API 地址
public interface IAuthApiClient : IHttpApi
{
    /// <summary>
    /// 用户登录
    /// </summary>
    [HttpPost("/api/auth/login")]
    Task<Result<LoginResponse>> LoginAsync([JsonContent] LoginRequest request);

    /// <summary>
    /// 用户注册
    /// </summary>
    [HttpPost("/api/auth/register")]
    Task<Result<object>> RegisterAsync([JsonContent] RegisterRequest request);
}