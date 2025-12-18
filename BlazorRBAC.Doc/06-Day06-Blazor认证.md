# Day 06 - Blazor Server 认证集成

> ⏱️ 预计时间：2小时

## 🎯 今日目标

- [ ] 创建 Blazor Server 项目
- [ ] 实现 AuthenticationStateProvider
- [ ] 创建登录页面
- [ ] 配置认证组件

---

## 💻 核心实现

### 1. 自定义认证提供者

**文件**：`src/BlazorRBAC.Web/Auth/CustomAuthStateProvider.cs`

```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace BlazorRBAC.Web.Auth;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedLocalStorage _localStorage;
    private readonly HttpClient _httpClient;

    public CustomAuthStateProvider(ProtectedLocalStorage localStorage, HttpClient httpClient)
    {
        _localStorage = localStorage;
        _httpClient = httpClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _localStorage.GetAsync<string>("authToken");
            
            if (string.IsNullOrEmpty(token.Value))
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

            // 解析 Token 中的 Claims
            var claims = ParseClaimsFromJwt(token.Value);
            var identity = new ClaimsIdentity(claims, "jwt");
            
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public async Task LoginAsync(string token)
    {
        await _localStorage.SetAsync("authToken", token);
        
        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);
        
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public async Task LogoutAsync()
    {
        await _localStorage.DeleteAsync("authToken");
        
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = System.Text.Json.JsonSerializer
            .Deserialize<Dictionary<string, object>>(jsonBytes);

        return keyValuePairs!.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()!));
    }

    private byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}
```

### 2. 登录页面

**文件**：`src/BlazorRBAC.Web/Pages/Login.razor`

```razor
@page "/login"
@inject CustomAuthStateProvider AuthStateProvider
@inject NavigationManager Navigation
@inject HttpClient Http

<MudContainer MaxWidth="MaxWidth.Small">
    <MudPaper Elevation="3" Class="pa-6 mt-10">
        <MudText Typo="Typo.h4" Align="Align.Center" GutterBottom="true">
            BlazorRBAC 登录
        </MudText>

        <EditForm Model="loginModel" OnValidSubmit="HandleLogin">
            <MudTextField @bind-Value="loginModel.Username" Label="用户名" />
            <MudTextField @bind-Value="loginModel.Password" Label="密码" 
                          InputType="InputType.Password" />
            
            <MudButton ButtonType="ButtonType.Submit" Variant="Variant.Filled" 
                       Color="Color.Primary" FullWidth="true" Class="mt-4">
                登录
            </MudButton>
        </EditForm>

        @if (!string.IsNullOrEmpty(errorMessage))
        {
            <MudAlert Severity="Severity.Error" Class="mt-4">@errorMessage</MudAlert>
        }
    </MudPaper>
</MudContainer>

@code {
    private LoginModel loginModel = new();
    private string errorMessage = string.Empty;

    private async Task HandleLogin()
    {
        try
        {
            var response = await Http.PostAsJsonAsync("api/auth/login", loginModel);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                await AuthStateProvider.LoginAsync(result!.Token);
                Navigation.NavigateTo("/");
            }
            else
            {
                errorMessage = "登录失败";
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"错误：{ex.Message}";
        }
    }

    class LoginModel
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }

    class LoginResponse
    {
        public string Token { get; set; } = "";
    }
}
```

### 3. 配置服务

**文件**：`src/BlazorRBAC.Web/Program.cs`

```csharp
using BlazorRBAC.Web.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();

// 认证
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<CustomAuthStateProvider>();

// HttpClient
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri("https://localhost:5001");
});
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
```

---

## 📝 今日总结

### ✅ 完成检查清单

- [ ] 实现了 AuthenticationStateProvider
- [ ] 创建了登录页面
- [ ] 配置了认证服务
- [ ] 测试了登录功能

---

[⬅️ 上一天](./05-Day05-权限验证.md) | [返回目录](./README.md) | [下一天 ➡️](./07-Day07-动态菜单UI.md)
