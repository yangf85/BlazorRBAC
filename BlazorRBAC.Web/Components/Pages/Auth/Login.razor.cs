using BlazorRBAC.Application.Dtos;
using BlazorRBAC.Web.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BlazorRBAC.Web.Pages;

public partial class Login
{
    [Inject] private IAuthApiClient AuthApi { get; set; } = default!;
    [Inject] private CustomAuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private MudForm? _form;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isLoading = false;

    private async Task HandleLoginAsync()
    {
        _errorMessage = string.Empty;

        await _form!.Validate();
        if (!_form.IsValid) return;

        _isLoading = true;

        try
        {
            var request = new LoginRequest(_username, _password);
            var result = await AuthApi.LoginAsync(request);

            if (!result.IsSuccess || result.Data == null)
            {
                _errorMessage = result.Message;
                return;
            }

            // ✅ 直接存储，简单优雅
            await AuthStateProvider.MarkUserAsAuthenticatedAsync(
                result.Data.Token,
                result.Data.Username,
                result.Data.Roles);

            Snackbar.Add("登录成功！", Severity.Success);

            // ✅ 不需要 forceLoad，状态已更新
            Navigation.NavigateTo("/");
        }
        catch (Exception ex)
        {
            _errorMessage = $"登录失败：{ex.Message}";
        }
        finally
        {
            _isLoading = false;
        }
    }
}