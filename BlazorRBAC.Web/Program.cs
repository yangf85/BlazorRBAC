using BlazorRBAC.Web.Components;
using BlazorRBAC.Web.Extensions;
using BlazorRBAC.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;

namespace BlazorRBAC.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ==================== 服务注册 ====================

            // MudBlazor
            builder.Services.AddMudServices();

            // Razor Components
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            // ? Cookie 认证（关键：这行被漏掉了！）
            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
            builder.Services.AddScoped<CustomAuthenticationStateProvider>();

            // API 客户端
            builder.Services.AddApiClients(builder.Configuration);

            // Controllers（用于登录 API）
            builder.Services.AddControllers();

            // ==================== 中间件配置 ====================

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseAntiforgery();

            // 认证和授权（必须在 MapRazorComponents 之前）
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllers();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}