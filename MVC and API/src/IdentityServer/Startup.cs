﻿using Host.Configuration;
using Host.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Host
{
    public class Startup
    {
        private readonly IHostingEnvironment _environment;

        public Startup(IHostingEnvironment env)
        {
            _environment = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var cert = new X509Certificate2(Path.Combine(_environment.ContentRootPath, "idsrv3test.pfx"), "idsrv3test");

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            var builder = services.AddIdentityServer(options =>
            {
                options.SigningCertificate = cert;
            });

            builder.AddInMemoryClients(Clients.Get());
            builder.AddInMemoryScopes(Scopes.Get());
            builder.AddInMemoryUsers(Users.Get());

            builder.AddCustomGrantValidator<CustomGrantValidator>();


            // for the UI
            services
                .AddMvc()
                .AddRazorOptions(razor =>
                {
                    razor.ViewLocationExpanders.Add(new UI.CustomViewLocationExpander());
                });
            services.AddTransient<UI.Login.LoginService>();
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(LogLevel.Trace);
            loggerFactory.AddDebug(LogLevel.Trace);

            app.UseDeveloperExceptionPage();

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationScheme = "Temp",
                AutomaticAuthenticate = false,
                AutomaticChallenge = false
            });

            app.UseGoogleAuthentication(new GoogleOptions
            {
                AuthenticationScheme = "Google",
                SignInScheme = "Temp",
                ClientId = "434483408261-55tc8n0cs4ff1fe21ea8df2o443v2iuc.apps.googleusercontent.com",
                ClientSecret = "3gcoTrEDPPJ0ukn_aYYT6PWo"
            });

            app.UseIdentityServer();

            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
    }
}