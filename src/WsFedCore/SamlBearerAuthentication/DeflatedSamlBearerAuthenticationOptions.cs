using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace WsFedCore.DeflatedSamlBearerAuthentication
{
    public class DeflatedSamlBearerAuthenticationOptions : AuthenticationSchemeOptions
    {
        public DeflatedSamlBearerAuthenticationOptions()
        {
        }

        public string MetadataAddress { get; set; }
        public string Wtrealm { get; set; }
        public WsFederationTokenValidator Validator { get; internal set; }
    }


}
