using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using WsFedCore.SamlBearerAuthentication;

namespace WsFedCore.DeflatedSamlBearerAuthentication
{
    public class DeflatedSamlBearerAuthenticationHandler : AuthenticationHandler<DeflatedSamlBearerAuthenticationOptions>
    {
        public DeflatedSamlBearerAuthenticationHandler(IOptionsMonitor<DeflatedSamlBearerAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        private WsFederationTokenValidator WsFederationTokenValidator { get; }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            const string Authorization = "Authorization";

            StringValues headerValue;
            if (!Context.Request.Headers.TryGetValue(Authorization, out headerValue))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }
            if (headerValue.Count > 1)
            {
                return Task.FromResult(AuthenticateResult.Fail("Multiple Authorization headers"));
            }

            var authzHeaderValue = headerValue.Single();

            if (!authzHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(AuthenticateResult.Fail("Authorization header not prefixed with 'Bearer '"));
            }
            // TODO: Additional validation to avoid blowing up on other token types
            var encodedToken = authzHeaderValue.Split(' ', 2).Last();
            var token = new DeflatedSamlEncoder().Decode(encodedToken);
            var principal = this.Options.Validator.Validate(token);
            return Task.FromResult(
                 AuthenticateResult.Success(
                    new AuthenticationTicket(
                        principal,
                        new AuthenticationProperties(),
                        this.Scheme.Name)));
            
        }
    }


}
