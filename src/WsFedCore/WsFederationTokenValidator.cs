using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.IdentityModel.Protocols.WsFederation;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Saml;

namespace WsFedCore
{
    public class WsFederationTokenValidator 
    {
        public WsFederationTokenValidator(WsFederationOptions options, WsFederationConfiguration configuration)
        {
            this._wsFederationAuthenticationOptions = options;
            this._wsFederationConfiguration = configuration;
        }

        private readonly WsFederationOptions _wsFederationAuthenticationOptions;
        private readonly WsFederationConfiguration _wsFederationConfiguration;

        public ClaimsPrincipal Validate(string token)
        {

            var validationParameters = _wsFederationAuthenticationOptions.TokenValidationParameters.Clone();
            var issuers = new string[] { _wsFederationConfiguration.Issuer };
            validationParameters.ValidIssuers = (validationParameters.ValidIssuers == null)
                ? issuers
                : validationParameters.ValidIssuers.Concat(issuers);
            validationParameters.IssuerSigningKeys = (validationParameters.IssuerSigningKeys == null)
                ? _wsFederationConfiguration.SigningKeys
                : validationParameters.IssuerSigningKeys.Concat(_wsFederationConfiguration.SigningKeys);
            SecurityToken securityToken;
            var samlTokenHandler = _wsFederationAuthenticationOptions.SecurityTokenHandlers.Single(stv => stv.GetType() == typeof(SamlSecurityTokenHandler));
            //return _wsFederationAuthenticationOptions.SecurityTokenHandlers.ValidateToken(token, validationParameters, out securityToken);
            return samlTokenHandler.ValidateToken(token, validationParameters, out securityToken);
        }
    }
}
