using Duende.IdentityServer.Models;

namespace Hearts.IdentityServer;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        [
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        ];

    public static IEnumerable<ApiScope> ApiScopes =>
        [
            new ApiScope("scope1"),
            new ApiScope("scope2"),
        ];

    public static IEnumerable<Client> Clients(IConfiguration configuration) =>
        [
            new Client
            {
                ClientId = "blazor",
                ClientName = "Hearts Blazor Application",

                AllowOfflineAccess = true,

                AllowedGrantTypes = GrantTypes.Code,
                
                ClientSecrets = { new Secret("secret".Sha256()) },

                RedirectUris = [$"{configuration["blazorEndpoint"]}/signin-oidc"],

                PostLogoutRedirectUris = [$"{configuration["blazorEndpoint"]}/signout-callback-oidc"],

                AllowedScopes = { "openid", "profile", "offline_access" },

                AlwaysIncludeUserClaimsInIdToken = true
            }
        ];
}
