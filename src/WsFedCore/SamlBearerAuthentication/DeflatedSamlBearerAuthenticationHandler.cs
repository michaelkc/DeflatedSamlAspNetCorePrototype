using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using WsFedCore.SamlBearerAuthentication;
using Microsoft.IdentityModel.Protocols.WsFederation;

namespace WsFedCore.DeflatedSamlBearerAuthentication
{
    public class DeflatedSamlBearerAuthenticationHandler : AuthenticationHandler<WsFederationOptions>
    {
        public DeflatedSamlBearerAuthenticationHandler(IOptionsMonitor<WsFederationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        private WsFederationConfiguration _configuration;

        // Code adapted from 
        // https://github.com/aspnet/Security/blob/rel/2.0.0-ws-rtm/src/Microsoft.AspNetCore.Authentication.WsFederation/WsFederationHandler.cs
        // HandleRemoteAuthenticateAsync()
        private async Task<ClaimsPrincipal> Validate(string token)
        {
            if (_configuration == null)
            {
                _configuration = await Options.ConfigurationManager.GetConfigurationAsync(Context.RequestAborted);
            }

            // Copy and augment to avoid cross request race conditions for updated configurations.
            var tvp = Options.TokenValidationParameters.Clone();
            var issuers = new[] { _configuration.Issuer };
            tvp.ValidIssuers = (tvp.ValidIssuers == null ? issuers : tvp.ValidIssuers.Concat(issuers));
            tvp.IssuerSigningKeys = (tvp.IssuerSigningKeys == null ? _configuration.SigningKeys : tvp.IssuerSigningKeys.Concat(_configuration.SigningKeys));

            ClaimsPrincipal principal = null;
            SecurityToken parsedToken = null;
            foreach (var validator in Options.SecurityTokenHandlers)
            {
                if (validator.CanReadToken(token))
                {
                    principal = validator.ValidateToken(token, tvp, out parsedToken);
                    break;
                }
            }

            if (principal == null)
            {
                throw new SecurityTokenException("NoTokenValidatorFound");
            }
            return principal;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            const string Authorization = "Authorization";

            StringValues headerValue;
            if (!Context.Request.Headers.TryGetValue(Authorization, out headerValue))
            {
                return AuthenticateResult.NoResult();
            }
            if (headerValue.Count > 1)
            {
                return AuthenticateResult.Fail("Multiple Authorization headers");
            }

            var authzHeaderValue = headerValue.Single();

            if (!authzHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return AuthenticateResult.Fail("Authorization header not prefixed with 'Bearer '");
            }
            // TODO: Additional validation to avoid blowing up on other token types
            var encodedToken = authzHeaderValue.Split(' ', 2).Last();
            var token = new DeflatedSamlEncoder().Decode(encodedToken);
            var principal = await this.Validate(token);
            return 
                 AuthenticateResult.Success(
                    new AuthenticationTicket(
                        principal,
                        new AuthenticationProperties(),
                        this.Scheme.Name));
            
        }
    }


}
