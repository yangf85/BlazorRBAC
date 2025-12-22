using BlazorRBAC.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace BlazorRBAC.Web.Extensions;

/// <summary>
/// 服务注册扩展类
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加 Cookie 认证服务
    /// </summary>
    public static IServiceCollection AddCookieAuthentication(
        this IServiceCollection services,
        IWebHostEnvironment? environment = null)
    {
        // Cookie 认证
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = "BlazorRBAC.Auth";
                options.LoginPath = "/login";
                options.AccessDeniedPath = "/access-denied";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.SlidingExpiration = true;  // 滑动过期
                options.Cookie.HttpOnly = true;    // 防止 JavaScript 访问

                // ✅ 开发环境允许 HTTP，生产环境强制 HTTPS
                options.Cookie.SecurePolicy = environment?.IsDevelopment() == true
                    ? CookieSecurePolicy.None      // HTTP 也可用（开发）
                    : CookieSecurePolicy.Always;   // 强制 HTTPS（生产）

                // ✅ Lax 模式：允许正常导航携带 Cookie，但防止 CSRF
                // Strict 太严格，会导致从外部链接跳转时丢失 Cookie
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

        // 授权服务
        services.AddAuthorizationCore();

        // ✅ 认证状态级联（使用 ASP.NET Core 默认的实现）
        // 这会自动从 Cookie 读取认证信息并级联到组件树
        services.AddCascadingAuthenticationState();

        // ✅ HttpContextAccessor（用于在 Blazor 组件中访问 HttpContext）
        services.AddHttpContextAccessor();

        return services;
    }

    /// <summary>
    /// 添加 API 客户端
    /// </summary>
    public static IServiceCollection AddApiClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var apiBaseUrl = configuration["ApiSettings:BaseUrl"]
            ?? "https://localhost:7129";

        services.AddHttpApi<IAuthApiClient>(options =>
        {
            options.HttpHost = new Uri(apiBaseUrl);
        });

        return services;
    }
}