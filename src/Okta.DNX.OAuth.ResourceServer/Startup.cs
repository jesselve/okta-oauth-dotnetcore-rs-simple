using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Okta.DNX.OAuth.ResourceServer.Models;
using Microsoft.IdentityModel.Tokens;

namespace Okta.DNX.OAuth.ResourceServer
{
    public class Startup
    {
        readonly string clientId = string.Empty;
        readonly string issuer = string.Empty;
        readonly string authorizationServerIssuer = string.Empty;
        readonly string audience = string.Empty;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile($"config.{env.EnvironmentName}.json", optional: true)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            builder.AddEnvironmentVariables();

            Configuration = builder.Build();

            clientId = Configuration["okta:clientId"] as string;
            issuer = Configuration["okta:organizationUrl"];
            authorizationServerIssuer = Configuration["okta:authorizationServerIssuer"];
            audience = Configuration["okta:audience"] as string;
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication();
            services.AddAuthorization(options =>
                {
                    options.AddPolicy("todo.read",
                        policy =>
                        {
                            policy
                            .RequireClaim("cid", clientId)
                           .RequireClaim("http://schemas.microsoft.com/identity/claims/scope", "todolist.read");
                        }
                    );
                    options.AddPolicy("todo.write",
                        policy =>
                        {
                            policy
                            .RequireClaim("cid", clientId)
                           .RequireClaim("http://schemas.microsoft.com/identity/claims/scope", "todolist.write");
                        }
                    );
                    options.AddPolicy("todo.delete",
                        policy =>
                        {
                            policy
                            .RequireClaim("cid", clientId)
                           .RequireClaim("http://schemas.microsoft.com/identity/claims/scope", "todolist.delete");
                        }
                    );
                }
            );
            services.AddMvc();
            services.AddSingleton<ITodoRepository, TodoRepository>();
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(LogLevel.Trace);
            loggerFactory.AddDebug();

            TokenValidationParameters tvps = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidAudience = audience,

                ValidateIssuer = true,
                ValidIssuer = authorizationServerIssuer,

                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            // Configure the app to use Jwt Bearer Authentication
            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                //MetadataAddress is critical for the JwtBearerAuthentication middleware to retrieve the OIDC metadata and be able to perform signing key validation
                MetadataAddress = authorizationServerIssuer + "/.well-known/openid-configuration",
                TokenValidationParameters = tvps
            });

            app.UseMvc();
        }
    }
}
