using BlazorRBAC.Api.Extensions;
using Serilog;

namespace BlazorRBAC.Api;

public class Program
{
    public static void Main(string[] args)
    {
        // 配置 Serilog
        SerilogExtensions.ConfigureSerilog();

        try
        {
            Log.Information("===== 应用程序启动 =====");

            var builder = WebApplication.CreateBuilder(args);

            // ===== 添加服务 =====
            builder.AddSerilog();
            builder.Services.AddDatabase(builder.Configuration);
            builder.Services.AddApiDocumentation();
            builder.Services.AddControllers();

            // ===== 构建应用 =====
            var app = builder.Build();

            // ===== 配置中间件管道 =====
            app.UseRequestLogging();  // ← 改名！
            app.UseApiDocumentation();
            app.UseAuthorization();
            app.MapControllers();

            Log.Information("应用程序配置完成，准备运行");

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "应用程序启动失败");
            throw;
        }
        finally
        {
            Log.Information("===== 应用程序关闭 =====");
            Log.CloseAndFlush();
        }
    }
}