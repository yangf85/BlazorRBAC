using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;
using System.Text.Json;

namespace BlazorRBAC.Web.Services;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedLocalStorage _localStorage;
    private readonly ILogger<CustomAuthenticationStateProvider> _logger;
    private ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public CustomAuthenticationStateProvider(
        ProtectedLocalStorage localStorage,
        ILogger<CustomAuthenticationStateProvider> logger)
    {
        _localStorage = localStorage;
        _logger = logger;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var tokenResult = await _localStorage.GetAsync<string>("authToken");

            if (!tokenResult.Success || string.IsNullOrEmpty(tokenResult.Value))
            {
                return new AuthenticationState(_anonymous);
            }

            var claims = ParseClaimsFromJwt(tokenResult.Value);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            return new AuthenticationState(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取认证状态失败");
            return new AuthenticationState(_anonymous);
        }
    }

    public async Task MarkUserAsAuthenticatedAsync(string token, string userName, List<string> roles)
    {
        // 存储 Token
        await _localStorage.SetAsync("authToken", token);

        // 创建 Claims
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, userName),
            new("AccessToken", token)
        };

        // 从 Token 解析更多 Claims（可选）
        var tokenClaims = ParseClaimsFromJwt(token);
        claims.AddRange(tokenClaims);

        // 添加角色
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(user)));
    }

    public async Task MarkUserAsLoggedOutAsync()
    {
        await _localStorage.DeleteAsync("authToken");

        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(_anonymous)));
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();

        try
        {
            var parts = jwt.Split('.');
            if (parts.Length != 3) return claims;

            var payload = parts[1];
            var paddedPayload = payload.PadRight(
                payload.Length + (4 - payload.Length % 4) % 4,
                '=');

            var payloadBytes = Convert.FromBase64String(paddedPayload);
            var payloadJson = System.Text.Encoding.UTF8.GetString(payloadBytes);

            var doc = JsonDocument.Parse(payloadJson);

            foreach (var property in doc.RootElement.EnumerateObject())
            {
                claims.Add(new Claim(property.Name, property.Value.ToString()));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "解析 JWT 失败");
        }

        return claims;
    }
}