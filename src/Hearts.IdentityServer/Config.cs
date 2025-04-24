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

                AllowedGrantTypes = GrantTypes.Code,

                RequireClientSecret = false,
                //ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },

                RedirectUris = [$"{configuration["blazorEndpoint"]}/authentication/login-callback"],

                AllowedScopes = { "openid", "profile" }
            }
        ];
}
