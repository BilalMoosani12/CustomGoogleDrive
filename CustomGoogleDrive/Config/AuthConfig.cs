using System.Collections.Generic;
using IdentityServer4.Models;
using static IdentityServer4.IdentityServerConstants;

namespace CustomGoogleDrive.Config
{
    public class AuthConfig
    {
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("scoreBoardApi", "Score Board Api")
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "MvcClient",
                    ClientName = "Google Custom Client",
                    AllowedGrantTypes = new[] {GrantType.ResourceOwnerPassword,"external"},
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes = {
                        "Test.WebApi",
                        StandardScopes.Email,
                        StandardScopes.OpenId,
                        StandardScopes.Profile
                    },
                    AccessTokenType = AccessTokenType.Jwt,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    AccessTokenLifetime = 86400,
                    AllowOfflineAccess = true,
                    IdentityTokenLifetime = 86400,
                    AlwaysSendClientClaims = true,
                    Enabled = true,
                }
            };
        }

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email()
            };
        }
    }
}
