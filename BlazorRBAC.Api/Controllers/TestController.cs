using Microsoft.AspNetCore.Mvc;

namespace BlazorRBAC.Api.Controllers;

/// <summary>
/// 测试控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly IFreeSql _fsql;
    private readonly ILogger<TestController> _logger;

    public TestController(IFreeSql fsql, ILogger<TestController> logger)
    {
        _fsql = fsql;
        _logger = logger;
    }

    /// <summary>
    /// 测试数据库连接
    /// </summary>
    /// <returns></returns>
    [HttpGet("db-connection")]
    public IActionResult TestDatabaseConnection()
    {
        try
        {
            _logger.LogInformation("开始测试数据库连接...");

            // 执行简单的查询获取 PostgreSQL 版本
            var version = _fsql.Ado.ExecuteScalar("SELECT version()");

            _logger.LogInformation("数据库连接成功！版本: {Version}", version);

            return Ok(new
            {
                success = true,
                message = "数据库连接成功！",
                database = "PostgreSQL",
                version = version?.ToString(),
                timestamp = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "数据库连接失败");

            return StatusCode(500, new
            {
                success = false,
                message = "数据库连接失败",
                error = ex.Message,
                timestamp = DateTime.Now
            });
        }
    }

    /// <summary>
    /// 测试 API 是否正常运行
    /// </summary>
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new
        {
            success = true,
            message = "API 运行正常",
            timestamp = DateTime.Now
        });
    }
}