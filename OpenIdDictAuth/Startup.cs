using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Validation;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;
using OpenIdDictAuth.Models;
using OpenIdDictAuth.Services;
using Swashbuckle.AspNetCore.Swagger;

namespace OpenIdDictAuth
{
    public sealed class Startup
    {
        private IConfiguration _configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            // allow overriding configuration values from user secrets/environment
            if (env.IsDevelopment())
                builder.AddUserSecrets("aspnet-oidapi-04cf693a-d29e-4986-8721-351b6f7d5627");
            builder.AddEnvironmentVariables();

            _configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // setup options with DI
            services.AddOptions();
            // these options are used for building and sending email messages
            //services.Configure<MessagingOptions>(_configuration.GetSection("Messaging"));
            //services.Configure<MailingOptions>(_configuration.GetSection("Mailing"));

            // CORS (note: if using Azure, remember to enable CORS in the portal, too!)
            services.AddCors();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                // Configure the context to use an in-memory store.
               // options.UseInMemoryDatabase(nameof(ApplicationDbContext));
                // or use a real db:
                 options.UseSqlServer(_configuration.GetConnectionString("OpenIdDictAuthDb"));

                // Register the entity sets needed by OpenIddict.
                // Note: use the generic overload if you need
                // to replace the default OpenIddict entities.
                options.UseOpenIddict();
            });

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // configure Identity to use the same JWT claims as OpenIddict instead
            // of the legacy WS-Federation claims it uses by default (ClaimTypes),
            // which saves you from doing the mapping in your authorization controller.
            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
            });

            // Register the OpenIddict services.
            services.AddOpenIddict()
                .AddCore(options =>
                {
                    // Configure OpenIddict to use the Entity Framework Core stores and entities.
                    options.UseEntityFrameworkCore()
                                   .UseDbContext<ApplicationDbContext>();
                })

                .AddServer(options =>
                {
                    // Register the ASP.NET Core MVC binder used by OpenIddict.
                    // Note: if you don't call this method, you won't be able to
                    // bind OpenIdConnectRequest or OpenIdConnectResponse parameters.
                    options.UseMvc();

                    // Enable the token endpoint (required to use the password flow).
                    options.EnableTokenEndpoint("/connect/token");
                    options.EnableLogoutEndpoint("/connect/logout");
                    options.EnableUserinfoEndpoint("/connect/userinfo");

                    // Allow client applications to use the grant_type=password flow.
                    options.AllowPasswordFlow();
                    options.AllowRefreshTokenFlow();
                    // During development, you can disable the HTTPS requirement.
                    options.DisableHttpsRequirement();

                    // Accept token requests that don't specify a client_id.
                    options.AcceptAnonymousClients();
                })

                .AddValidation();

            // Register the validation handler that is used to decrypt the tokens
            //services.AddAuthentication(options =>
            //{
            //    options.DefaultScheme = OAuthValidationDefaults.AuthenticationScheme;
            //})
            //    .AddOAuthValidation();

            // register MVC framework services
            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver =
                        new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
                });

            // register the database initializer service
            services.AddTransient<IDatabaseInitializer, DatabaseInitializer>();
            // register the sample messaging services
            services.AddTransient<IMessageBuilderService, FileMessageBuilderService>();
            services.AddTransient<IMailerService, DotNetMailerService>();

            // register the Swagger service
            // https://github.com/domaindrivendev/Swashbuckle.AspNetCore
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info
                {
                    Title = "Oidapi API",
                    Version = "v1"
                });
                options.DescribeAllParametersInCamelCase();

                string basePath = PlatformServices.Default.Application.ApplicationBasePath;
                string xmlPath = Path.Combine(basePath, "Oidapi.xml");
                options.IncludeXmlComments(xmlPath);
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
            IDatabaseInitializer databaseInitializer)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseCors(builder =>
                builder.WithOrigins("http://localhost:4200",
                        "http://www.somesite.com")
                    .AllowAnyHeader()
                    .AllowAnyMethod());

            app.UseAuthentication();
            app.UseMvc();

            databaseInitializer.Seed().GetAwaiter().GetResult();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                // options.BooleanValues(new object[] { 0, 1 });
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "V1 Docs");
                // options.ShowJsonEditor();
            });
        }
    }
}
