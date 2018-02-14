﻿using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.WsFederation;
using System;
using WsFedCore.DeflatedSamlBearerAuthentication;

namespace WsFedCore
{

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            WsFederationTokenValidator validator = null;
            services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultChallengeScheme = WsFederationDefaults.AuthenticationScheme;
            }).AddWsFederation(options =>
              {
                  options.Wtrealm = Configuration["wsfed:realm"];
                  options.MetadataAddress = Configuration["wsfed:metadata"];

                  var configuration = FetchWsFederationConfiguration(options);
                  validator = new WsFederationTokenValidator(options, configuration);
              })
              .AddDeflatedSamlBearerAuthentication(options => 
              {
                  options.Wtrealm = Configuration["wsfed:realm"];
                  options.MetadataAddress = Configuration["wsfed:metadata"];
                  options.Validator = validator ?? throw new Exception("Null validator");
              })
              .AddCookie();

            services.AddMvc();
        }

        private WsFederationConfiguration FetchWsFederationConfiguration(WsFederationOptions options)
        {
            using (var httpClient = new System.Net.Http.HttpClient())
            {
                IConfigurationRetriever<WsFederationConfiguration> retriever = new WsFederationConfigurationRetriever();
                var configurationManager = new ConfigurationManager<WsFederationConfiguration>(options.MetadataAddress, retriever, httpClient);
                return configurationManager.GetConfigurationAsync().Result;
            }
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseAuthentication();
            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
