// BlazorRBAC.Application/Common/ResultCode.cs
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlazorRBAC.Application.Common;

/// <summary>
/// 业务状态码枚举
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ResultCode
{
    /// <summary>
    /// 操作成功
    /// </summary>
    Success = 0,

    /// <summary>
    /// 通用错误
    /// </summary>
    Error = 1000,

    /// <summary>
    /// 参数验证错误
    /// </summary>
    ValidationError = 1001,

    /// <summary>
    /// 资源未找到
    /// </summary>
    NotFound = 1002,

    /// <summary>
    /// 资源已存在（如用户名重复）
    /// </summary>
    AlreadyExists = 1003,

    /// <summary>
    /// 操作被拒绝（业务规则限制）
    /// </summary>
    OperationDenied = 1004,

    // ========== 认证授权相关 2xxx ==========

    /// <summary>
    /// 未授权（未登录）
    /// </summary>
    Unauthorized = 2001,

    /// <summary>
    /// 权限不足（已登录但无权限）
    /// </summary>
    PermissionDenied = 2002,

    /// <summary>
    /// Token 无效或过期
    /// </summary>
    InvalidToken = 2003,

    /// <summary>
    /// 账号被禁用
    /// </summary>
    AccountDisabled = 2004,

    // ========== 业务错误 3xxx ==========

    /// <summary>
    /// 用户名或密码错误
    /// </summary>
    InvalidCredentials = 3001,

    /// <summary>
    /// 用户不存在
    /// </summary>
    UserNotFound = 3002,

    /// <summary>
    /// 角色不存在
    /// </summary>
    RoleNotFound = 3003,

    /// <summary>
    /// 菜单不存在
    /// </summary>
    MenuNotFound = 3004,

    /// <summary>
    /// 无法删除系统角色
    /// </summary>
    CannotDeleteSystemRole = 3005,

    /// <summary>
    /// 无法修改超级管理员
    /// </summary>
    CannotModifySuperAdmin = 3006,

    // ========== 系统错误 5xxx ==========

    /// <summary>
    /// 服务器内部错误
    /// </summary>
    InternalError = 5000,

    /// <summary>
    /// 数据库错误
    /// </summary>
    DatabaseError = 5001,

    /// <summary>
    /// 外部服务调用失败
    /// </summary>
    ExternalServiceError = 5002
}