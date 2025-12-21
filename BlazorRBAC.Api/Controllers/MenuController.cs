using BlazorRBAC.Application.Common;
using BlazorRBAC.Application.Services;
using BlazorRBAC.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlazorRBAC.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MenuController : ControllerBase
{
    private readonly MenuService _service;

    public MenuController(MenuService service)
    {
        _service = service;
    }

    /// <summary>
    /// 获取当前用户的菜单树
    /// </summary>
    [HttpGet("my-menus")]  // GET /api/menu/my-menus
    [ProducesResponseType(typeof(Result<List<Menu>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyMenus()
    {
        // ✅ 使用空条件运算符避免 null 异常
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null)
        {
            return Unauthorized("用户未登录");
        }

        var userId = int.Parse(userIdClaim);
        var result = await _service.GetUserMenusAsync(userId);

        return result.IsSuccess
            ? Ok(result)
            : BadRequest(result);
    }

    /// <summary>
    /// 获取指定用户的菜单（管理员功能）
    /// </summary>
    [HttpGet("{userId:int}/menus")]  // GET /api/menu/123/menus
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetUserMenusById(int userId)
    {
        var result = await _service.GetUserMenusAsync(userId);
        return result.IsSuccess
            ? Ok(result)
            : NotFound(result);
    }
}