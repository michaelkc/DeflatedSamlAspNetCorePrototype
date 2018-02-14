using Microsoft.AspNetCore.Authentication;
using System;

namespace WsFedCore.DeflatedSamlBearerAuthentication
{
    public static class DeflatedSamlBearerAuthenticationExtensions
    {
        public static AuthenticationBuilder AddDeflatedSamlBearerAuthentication(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<DeflatedSamlBearerAuthenticationOptions> configureOptions)
        {
            return builder.AddScheme<DeflatedSamlBearerAuthenticationOptions, DeflatedSamlBearerAuthenticationHandler>(authenticationScheme, displayName, configureOptions);
        }

        public static AuthenticationBuilder AddDeflatedSamlBearerAuthentication(this AuthenticationBuilder builder, Action<DeflatedSamlBearerAuthenticationOptions> configureOptions)
        {
            return builder.AddScheme<DeflatedSamlBearerAuthenticationOptions, DeflatedSamlBearerAuthenticationHandler>(DeflatedSamlBearerDefaults.AuthenticationScheme, DeflatedSamlBearerDefaults.DisplayName, configureOptions);
        }
    }


}
