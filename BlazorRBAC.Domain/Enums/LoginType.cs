namespace BlazorRBAC.Domain.Enums;

/// <summary>
/// 登录类型
/// </summary>
public enum LoginType
{
    /// <summary>
    /// 本地账号密码登录
    /// </summary>
    Local,

    /// <summary>
    /// 微信第三方登录
    /// </summary>
    WeChat,

    /// <summary>
    /// GitHub第三方登录（预留）
    /// </summary>
    GitHub
}