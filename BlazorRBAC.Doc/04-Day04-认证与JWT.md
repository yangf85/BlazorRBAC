# Day 04 - 用户认证与 JWT

> ⏱️ 预计时间：2小时

## 🎯 今日目标

- [ ] 实现用户注册 API
- [ ] 实现用户登录并生成 JWT
- [ ] 配置 JWT 认证中间件
- [ ] 使用 Scalar 测试 API

---

## 💻 核心实现

### 1. JWT 配置类

**文件**：`src/BlazorRBAC.Infrastructure/Jwt/JwtSettings.cs`

```csharp
namespace BlazorRBAC.Infrastructure.Jwt;

public class JwtSettings
{
    public string SecretKey { get; set; } = "YourSuperSecretKey12345678901234567890"; // 至少32位
    public string Issuer { get; set; } = "BlazorRBAC";
    public string Audience { get; set; } = "BlazorRBACClient";
    public int ExpirationMinutes { get; set; } = 30;
}
```

### 2. JWT 服务

**文件**：`src/BlazorRBAC.Infrastructure/Jwt/JwtService.cs`

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BlazorRBAC.Infrastructure.Jwt;

public class JwtService
{
    private readonly JwtSettings _settings;

    public JwtService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    public string GenerateToken(long userId, string username, List<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // 添加角色
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

### 3. 认证服务

**文件**：`src/BlazorRBAC.Application/Services/AuthService.cs`

```csharp
using BlazorRBAC.Domain.Entities;
using BlazorRBAC.Infrastructure.Jwt;
using BCrypt.Net;

namespace BlazorRBAC.Application.Services;

public class AuthService
{
    private readonly IFreeSql _fsql;
    private readonly JwtService _jwtService;

    public AuthService(IFreeSql fsql, JwtService jwtService)
    {
        _fsql = fsql;
        _jwtService = jwtService;
    }

    // 注册
    public async Task<(bool Success, string Message)> RegisterAsync(
        string username, string password, string realName, string email)
    {
        // 检查用户名是否存在
        if (await _fsql.Select<SysUser>().AnyAsync(u => u.Username == username))
            return (false, "用户名已存在");

        // 创建用户
        var user = new SysUser
        {
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            RealName = realName,
            Email = email
        };

        var userId = await _fsql.Insert(user).ExecuteIdentityAsync();

        // 分配默认角色（普通用户，RoleId=3）
        await _fsql.Insert(new SysUserRole { UserId = userId, RoleId = 3 }).ExecuteAffrowsAsync();

        return (true, "注册成功");
    }

    // 登录
    public async Task<(bool Success, string Token, string Message)> LoginAsync(
        string username, string password)
    {
        // 查询用户及其角色
        var user = await _fsql.Select<SysUser>()
            .Where(u => u.Username == username && u.IsActive)
            .IncludeMany(u => u.Roles)
            .FirstAsync();

        if (user == null)
            return (false, string.Empty, "用户不存在或已禁用");

        // 验证密码
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return (false, string.Empty, "密码错误");

        // 生成 Token
        var roles = user.Roles.Select(r => r.RoleCode).ToList();
        var token = _jwtService.GenerateToken(user.Id, user.Username, roles);

        return (true, token, "登录成功");
    }
}
```

### 4. 认证控制器

**文件**：`src/BlazorRBAC.Api/Controllers/AuthController.cs`

```csharp
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

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var (success, message) = await _authService.RegisterAsync(
            dto.Username, dto.Password, dto.RealName, dto.Email);

        return success ? Ok(new { message }) : BadRequest(new { message });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var (success, token, message) = await _authService.LoginAsync(dto.Username, dto.Password);

        return success 
            ? Ok(new { token, message }) 
            : Unauthorized(new { message });
    }
}

public record RegisterDto(string Username, string Password, string RealName, string Email);
public record LoginDto(string Username, string Password);
```

### 5. 配置 JWT 认证

**文件**：`src/BlazorRBAC.Api/appsettings.json`

```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKey12345678901234567890",
    "Issuer": "BlazorRBAC",
    "Audience": "BlazorRBACClient",
    "ExpirationMinutes": 30
  }
}
```

**文件**：`src/BlazorRBAC.Api/Program.cs`

```csharp
using System.Text;
using BlazorRBAC.Application.Services;
using BlazorRBAC.Infrastructure.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// JWT 配置
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<AuthService>();

// JWT 认证
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };
    });

builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthentication();  // ⚠️ 必须在 UseAuthorization 之前
app.UseAuthorization();
app.MapControllers();

app.Run();
```

---

## 🧪 测试验证

### 1. 安装 Scalar

```bash
cd src/BlazorRBAC.Api
dotnet add package Scalar.AspNetCore
```

**更新 Program.cs**：

```csharp
app.MapScalarApiReference(); // 添加这一行
app.MapControllers();
```

### 2. 测试注册

```bash
curl -X POST https://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","password":"Test123!","realName":"测试用户","email":"test@example.com"}'
```

### 3. 测试登录

```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'

# 返回：
# { "token": "eyJhbGc...", "message": "登录成功" }
```

---

## 📝 今日总结

### ✅ 完成检查清单

- [ ] 实现了用户注册功能
- [ ] 实现了用户登录并生成 JWT
- [ ] 配置了 JWT 认证中间件
- [ ] 使用 Scalar 测试了 API

### 🎓 知识点

1. **BCrypt**：单向哈希，无法解密
2. **JWT**：无状态认证，包含用户信息
3. **Claims**：用户身份信息的键值对

---

[⬅️ 上一天](./03-Day03-实体与初始化.md) | [返回目录](./README.md) | [下一天 ➡️](./05-Day05-权限验证.md)
