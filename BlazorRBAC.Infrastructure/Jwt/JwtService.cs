using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BlazorRBAC.Infrastructure.Jwt;

/// <summary>
/// JWT Token 服务
/// 负责生成、验证 AccessToken 和 RefreshToken
/// </summary>
public class JwtService
{
    private readonly JwtSettings _settings;

    public JwtService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    /// <summary>
    /// 生成 AccessToken（JWT 格式）
    /// 调用时机：用户登录成功后
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <param name="username">用户名</param>
    /// <param name="roles">角色列表</param>
    /// <returns>JWT Token 字符串（格式：Header.Payload.Signature）</returns>
    public string GenerateAccessToken(int id, string username, List<string> roles)
    {
        // ====== 第1步：创建声明（Claims）- 存储用户信息 ======
        var claims = new List<Claim>
        {
            // NameIdentifier：用户ID（最重要，用于识别用户身份）
            new(ClaimTypes.NameIdentifier, id.ToString()),

            // Name：用户名（可选，用于显示）
            new(ClaimTypes.Name, username),

            // Jti：Token 唯一标识符（用于 Token 撤销、追踪）
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // ====== 第2步：添加角色声明（支持多角色） ======
        // 每个角色生成一个独立的 Claim，ASP.NET Core 会自动识别
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        // ====== 第3步：创建签名密钥和凭据 ======
        // 使用对称加密（HMAC-SHA256），密钥必须保密
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // ====== 第4步：创建 JWT Token 对象 ======
        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,                // 发行者（谁生成的 Token）
            audience: _settings.Audience,            // 受众（Token 给谁用的）
            claims: claims,                          // 用户信息（Claims）
            expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes), // 过期时间
            signingCredentials: credentials          // 签名凭据
        );

        // ====== 第5步：序列化为 JWT 字符串 ======
        // 返回格式：eyJhbGc...（Base64编码的 Header.Payload.Signature）
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// 生成 RefreshToken（随机字符串）
    /// 调用时机：与 AccessToken 一起生成，用于刷新 AccessToken
    /// </summary>
    /// <returns>Base64 编码的随机字符串（需要存储到数据库）</returns>
    public string GenerateRefreshToken()
    {
        // ====== 第1步：生成 32 字节的随机数 ======
        var randomNumber = new byte[32];

        // ====== 第2步：使用加密级随机数生成器（安全） ======
        // 不要使用 Random 类，它不够安全
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        // ====== 第3步：转换为 Base64 字符串 ======
        // 返回示例：k7+9Qw3L/xR5tM8N2vB6yT1zP4eF0gH3jI5nC7aD9qS=
        return Convert.ToBase64String(randomNumber);
    }

    /// <summary>
    /// 验证 Token 并提取用户 ID
    /// 调用时机：手动验证场景（刷新 Token、敏感操作）
    /// 注意：大部分情况下不需要手动调用，ASP.NET Core 中间件会自动验证
    /// </summary>
    /// <param name="token">JWT Token 字符串</param>
    /// <returns>验证成功返回用户ID，失败返回 null</returns>
    public int? ValidateToken(string token)
    {
        try
        {
            // ====== 第1步：创建 Token 处理器和签名密钥 ======
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_settings.SecretKey);

            // ====== 第2步：验证 Token（签名、过期时间、发行者、受众） ======
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                // 验证签名密钥（防止 Token 被篡改）
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),

                // 验证发行者（确保 Token 来自我们的系统）
                ValidateIssuer = true,
                ValidIssuer = _settings.Issuer,

                // 验证受众（确保 Token 是给我们的系统用的）
                ValidateAudience = true,
                ValidAudience = _settings.Audience,

                // 验证过期时间
                ValidateLifetime = true,

                // 不允许时间偏差（严格模式）
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            // ====== 第3步：从验证后的 Token 中提取 Claims ======
            var jwtToken = (JwtSecurityToken)validatedToken;

            // ====== 第4步：查找用户 ID 声明 ======
            var userIdClaim = jwtToken.Claims
                .First(x => x.Type == ClaimTypes.NameIdentifier)
                .Value;

            // ====== 第5步：返回用户 ID ======
            return int.Parse(userIdClaim);
        }
        catch
        {
            // ====== Token 无效（签名错误、过期、格式错误等） ======
            // 返回 null 表示验证失败
            return null;
        }
    }
}