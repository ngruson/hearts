using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using Hearts.IdentityServer.Models;
using Hearts.IdentityServer.Pages.Login;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Hearts.IdentityServer.Pages.Account.Login;

[SecurityHeaders]
[AllowAnonymous]
public class Index(
    IIdentityServerInteractionService interaction,
    IAuthenticationSchemeProvider schemeProvider,
    IIdentityProviderStore identityProviderStore,
    IEventService events,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly IIdentityServerInteractionService _interaction = interaction;
    private readonly IEventService _events = events;
    private readonly IAuthenticationSchemeProvider _schemeProvider = schemeProvider;
    private readonly IIdentityProviderStore _identityProviderStore = identityProviderStore;

    public ViewModel View { get; set; } = default!;

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public async Task<IActionResult> OnGet(string? returnUrl)
    {
        await this.BuildModelAsync(returnUrl);

        if (this.View.IsExternalLoginOnly)
        {
            // we only have one option for logging in and it's an external provider
            return this.RedirectToPage("/ExternalLogin/Challenge", new { scheme = this.View.ExternalLoginScheme, returnUrl });
        }

        return this.Page();
    }

    public async Task<IActionResult> OnPost()
    {
        // check if we are in the context of an authorization request
        AuthorizationRequest? context = await this._interaction.GetAuthorizationContextAsync(this.Input.ReturnUrl);

        // the user clicked the "cancel" button
        if (this.Input.Button != "login")
        {
            if (context != null)
            {
                // This "can't happen", because if the ReturnUrl was null, then the context would be null
                ArgumentNullException.ThrowIfNull(this.Input.ReturnUrl, nameof(this.Input.ReturnUrl));

                // if the user cancels, send a result back into IdentityServer as if they 
                // denied the consent (even if this client does not require consent).
                // this will send back an access denied OIDC error response to the client.
                await this._interaction.DenyAuthorizationAsync(context, AuthorizationError.AccessDenied);

                // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                if (context.IsNativeClient())
                {
                    // The client is native, so this change in how to
                    // return the response is for better UX for the end user.
                    return this.LoadingPage(this.Input.ReturnUrl);
                }

                return this.Redirect(this.Input.ReturnUrl ?? "~/");
            }
            else
            {
                // since we don't have a valid context, then we just go back to the home page
                return this.Redirect("~/");
            }
        }

        if (this.ModelState.IsValid)
        {
            // Only remember login if allowed
            bool rememberLogin = LoginOptions.AllowRememberLogin && this.Input.RememberLogin;

            Microsoft.AspNetCore.Identity.SignInResult result = await this._signInManager.PasswordSignInAsync(this.Input.Username!, this.Input.Password!, isPersistent: rememberLogin, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                ApplicationUser? user = await this._userManager.FindByNameAsync(this.Input.Username!);
                await this._events.RaiseAsync(new UserLoginSuccessEvent(user!.UserName, user.Id, user.UserName, clientId: context?.Client.ClientId));
                Telemetry.Metrics.UserLogin(context?.Client.ClientId, IdentityServerConstants.LocalIdentityProvider);

                if (context != null)
                {
                    // This "can't happen", because if the ReturnUrl was null, then the context would be null
                    ArgumentNullException.ThrowIfNull(this.Input.ReturnUrl, nameof(this.Input.ReturnUrl));

                    if (context.IsNativeClient())
                    {
                        // The client is native, so this change in how to
                        // return the response is for better UX for the end user.
                        return this.LoadingPage(this.Input.ReturnUrl);
                    }

                    // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                    return this.Redirect(this.Input.ReturnUrl ?? "~/");
                }

                // request for a local page
                if (this.Url.IsLocalUrl(this.Input.ReturnUrl))
                {
                    return this.Redirect(this.Input.ReturnUrl);
                }
                else if (string.IsNullOrEmpty(this.Input.ReturnUrl))
                {
                    return this.Redirect("~/");
                }
                else
                {
                    // user might have clicked on a malicious link - should be logged
                    throw new ArgumentException("invalid return URL");
                }
            }

            const string error = "invalid credentials";
            await this._events.RaiseAsync(new UserLoginFailureEvent(this.Input.Username, error, clientId: context?.Client.ClientId));
            Telemetry.Metrics.UserLoginFailure(context?.Client.ClientId, IdentityServerConstants.LocalIdentityProvider, error);
            this.ModelState.AddModelError(string.Empty, LoginOptions.InvalidCredentialsErrorMessage);
        }

        // something went wrong, show form with error
        await this.BuildModelAsync(this.Input.ReturnUrl);
        return this.Page();
    }

    private async Task BuildModelAsync(string? returnUrl)
    {
        this.Input = new InputModel
        {
            ReturnUrl = returnUrl
        };

        AuthorizationRequest? context = await this._interaction.GetAuthorizationContextAsync(returnUrl);
        if (context?.IdP != null)
        {
            AuthenticationScheme? scheme = await this._schemeProvider.GetSchemeAsync(context.IdP);
            if (scheme != null)
            {
                bool local = context.IdP == IdentityServerConstants.LocalIdentityProvider;

                // this is meant to short circuit the UI and only trigger the one external IdP
                this.View = new ViewModel
                {
                    EnableLocalLogin = local,
                };

                this.Input.Username = context.LoginHint;

                if (!local)
                {
                    this.View.ExternalProviders = [new ViewModel.ExternalProvider(authenticationScheme: context.IdP, displayName: scheme.DisplayName)];
                }
            }

            return;
        }

        IEnumerable<AuthenticationScheme> schemes = await this._schemeProvider.GetAllSchemesAsync();

        List<ViewModel.ExternalProvider> providers = [.. schemes
            .Where(x => x.DisplayName != null)
            .Select(x => new ViewModel.ExternalProvider
            (
                authenticationScheme: x.Name,
                displayName: x.DisplayName ?? x.Name
            ))];

        IEnumerable<ViewModel.ExternalProvider> dynamicSchemes = (await this._identityProviderStore.GetAllSchemeNamesAsync())
            .Where(x => x.Enabled)
            .Select(x => new ViewModel.ExternalProvider
            (
                authenticationScheme: x.Scheme,
                displayName: x.DisplayName ?? x.Scheme
            ));
        providers.AddRange(dynamicSchemes);


        bool allowLocal = true;
        Client? client = context?.Client;
        if (client != null)
        {
            allowLocal = client.EnableLocalLogin;
            if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Count != 0)
            {
                providers = [.. providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme))];
            }
        }

        this.View = new ViewModel
        {
            AllowRememberLogin = LoginOptions.AllowRememberLogin,
            EnableLocalLogin = allowLocal && LoginOptions.AllowLocalLogin,
            ExternalProviders = [.. providers]
        };
    }
}
