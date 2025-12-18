# Day 07 - MudBlazor 动态菜单 UI

> ⏱️ 预计时间：2小时

## 🎯 今日目标

- [ ] 安装配置 MudBlazor
- [ ] 创建主布局
- [ ] 实现动态侧边菜单
- [ ] 添加路由守卫

---

## 💻 核心实现（简化）

### 1. 主布局

**文件**：`Shared/MainLayout.razor`

```razor
@inherits LayoutComponentBase
@inject HttpClient Http
@inject AuthenticationStateProvider AuthStateProvider

<MudThemeProvider />
<MudDialogProvider />
<MudSnackbarProvider />

<AuthorizeView>
    <Authorized>
        <MudLayout>
            <MudAppBar>
                <MudText Typo="Typo.h6">BlazorRBAC</MudText>
                <MudSpacer />
                <MudText>@context.User.Identity?.Name</MudText>
            </MudAppBar>
            
            <MudDrawer Open="true">
                <NavMenu Menus="@menus" />
            </MudDrawer>

            <MudMainContent>
                @Body
            </MudMainContent>
        </MudLayout>
    </Authorized>
    <NotAuthorized>
        <RedirectToLogin />
    </NotAuthorized>
</AuthorizeView>

@code {
    private List<MenuDto> menus = new();

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        if (authState.User.Identity?.IsAuthenticated == true)
        {
            menus = await Http.GetFromJsonAsync<List<MenuDto>>("api/menu/user-menus") ?? new();
        }
    }
}
```

---

## 📝 今日总结

实现了动态菜单渲染、路由守卫和用户友好的UI布局。

---

[⬅️ 上一天](./06-Day06-Blazor认证.md) | [返回目录](./README.md) | [下一天 ➡️](./08-Day08-后台管理.md)
