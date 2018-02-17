using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;

namespace WsFedCore.DeflatedSamlBearerAuthentication
{
    public static class DeflatedSamlBearerAuthenticationExtensions
    {
        public static AuthenticationBuilder AddDeflatedSamlBearerAuthentication(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<WsFederationOptions> configureOptions)
        {
            // Adapted from
            // https://github.com/aspnet/Security/blob/rel/2.0.0-ws-rtm/src/Microsoft.AspNetCore.Authentication.WsFederation/WsFederationExtensions.cs
            // last AddWsFederation()
            // TODO: Does double-registering the service cause issues when both DeflatedSaml and WsFed authentication is used?
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<WsFederationOptions>, WsFederationPostConfigureOptions>());
            return builder.AddScheme<WsFederationOptions, DeflatedSamlBearerAuthenticationHandler>(authenticationScheme, displayName, configureOptions);
        }

        public static AuthenticationBuilder AddDeflatedSamlBearerAuthentication(this AuthenticationBuilder builder, Action<WsFederationOptions> configureOptions)
        {
            return AddDeflatedSamlBearerAuthentication(builder, DeflatedSamlBearerDefaults.AuthenticationScheme, DeflatedSamlBearerDefaults.DisplayName, configureOptions);
        }
    }


}
