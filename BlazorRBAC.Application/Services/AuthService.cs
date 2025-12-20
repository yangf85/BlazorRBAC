using BlazorRBAC.Application.Common;
using BlazorRBAC.Application.Dtos;
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

    // 注意：请将下面的 Result手动替换为 Result

    /// <summary>
    /// 用户注册
    /// </summary>
    public async Task<Result> RegisterAsync(RegisterRequest request)
    {
        // 检查用户名是否存在
        if (await _fsql.Select<User>().AnyAsync(u => u.Username == request.Username))
            return Result.Fail("用户名已存在");

        // 创建用户
        var user = new User
        {
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RealName = request.RealName,
            Email = request.Email
        };

        var userId = await _fsql.Insert(user).ExecuteIdentityAsync();

        // 分配默认角色（普通用户，RoleId=3）
        await _fsql.Insert(new UserRole { UserId = (int)userId, RoleId = 3 }).ExecuteAffrowsAsync();

        return Result.Ok("注册成功");
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
    {
        // 查询用户及其角色
        var user = await _fsql.Select<User>()
            .Where(u => u.Username == request.Username && u.IsActive)
            .IncludeMany(u => u.Roles)
            .FirstAsync();

        if (user == null)
            return Result<LoginResponse>.Fail("用户不存在或已禁用");

        // 验证密码
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Result<LoginResponse>.Fail("密码错误");

        // 生成 Token
        var roles = user.Roles.Select(r => r.RoleCode).ToList();
        var token = _jwtService.GenerateAccessToken(user.Id, user.Username, roles);

        // 返回登录数据
        var response = new LoginResponse(
            Token: token,
            Username: user.Username,
            Roles: roles
        );

        return Result<LoginResponse>.Ok(response, "登录成功");
    }
}