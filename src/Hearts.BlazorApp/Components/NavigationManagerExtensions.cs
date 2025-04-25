using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace Hearts.BlazorApp.Components;

public static class NavigationManagerExtensions
{
    public static void NavigateToLogout(this NavigationManager manager, [StringSyntax(StringSyntaxAttribute.Uri, UriKind.Relative)] string logoutPath) =>
        manager.NavigateToLogout(logoutPath, null);

    public static void NavigateToLogout(this NavigationManager manager, [StringSyntax(StringSyntaxAttribute.Uri, UriKind.Relative)] string logoutPath, [StringSyntax(StringSyntaxAttribute.Uri, UriKind.Relative)] string? returnUrl)
    {
        manager.NavigateTo(logoutPath, new NavigationOptions
        {
            HistoryEntryState = new InteractiveRequestOptions
            {
                Interaction = InteractionType.SignOut,
                ReturnUrl = returnUrl!
            }.ToState()
        });
    }

    public static void NavigateToLogin(this NavigationManager manager, [StringSyntax(StringSyntaxAttribute.Uri, UriKind.Relative)] string loginPath, InteractiveRequestOptions request)
    {
        manager.NavigateTo(loginPath, new NavigationOptions
        {
            HistoryEntryState = request.ToState(),
        });
    }

    public static void NavigateToLogin(this NavigationManager manager, [StringSyntax(StringSyntaxAttribute.Uri, UriKind.Relative)] string loginPath)
    {
        manager.NavigateToLogin(
            loginPath,
            new InteractiveRequestOptions
            {
                Interaction = InteractionType.SignIn,
                ReturnUrl = manager.Uri
            });
    }
}
