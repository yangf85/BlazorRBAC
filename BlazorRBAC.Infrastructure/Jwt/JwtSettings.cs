namespace BlazorRBAC.Infrastructure.Jwt;

public class JwtSettings
{
    /// <summary>
    /// appsettings.json 中的配置节点名称
    /// </summary>
    public const string SectionName = "JwtSettings";

    /// <summary>
    /// JWT 签名密钥（至少32位，生产环境使用环境变量）
    /// </summary>
    public string SecretKey { get; set; } = "YourSuperSecretKey12345678901234567890";

    /// <summary>
    /// 发行者
    /// </summary>
    public string Issuer { get; set; } = "BlazorRBAC";

    /// <summary>
    /// 受众
    /// </summary>
    public string Audience { get; set; } = "BlazorRBACClient";

    /// <summary>
    /// AccessToken 过期时间（分钟）
    /// </summary>
    public int AccessTokenExpirationMinutes { get; set; } = 30;

    /// <summary>
    /// RefreshToken 过期时间（天）
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;
}