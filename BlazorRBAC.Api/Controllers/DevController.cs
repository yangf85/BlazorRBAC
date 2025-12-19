using BlazorRBAC.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;

namespace BlazorRBAC.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevController : ControllerBase
{
    private readonly IFreeSql _fsql;
    private readonly SeedDataService _seedService;

    public DevController(IFreeSql fsql, SeedDataService seedService)
    {
        _fsql = fsql;
        _seedService = seedService;
    }

    /// <summary>
    /// 初始化种子数据
    /// </summary>
    [HttpPost("initial")]
    public async Task<IActionResult> Initialize()
    {
        await _seedService.InitializeAsync();
        return Ok(new { message = "种子数据初始化完成" });
    }

    /// <summary>
    /// 清空所有数据
    /// </summary>
    [HttpPost("clean")]
    public async Task<IActionResult> CleanData()
    {
        await _seedService.CleanDataAsync();
        return Ok(new { message = "数据清空完成" });
    }

    /// <summary>
    /// 重置数据（清空 + 初始化）
    /// </summary>
    [HttpPost("reset")]
    public async Task<IActionResult> ResetData()
    {
        await _seedService.ResetDataAsync();
        return Ok(new { message = "数据重置完成" });
    }
}