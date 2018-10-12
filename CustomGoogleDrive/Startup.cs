using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CustomGoogleDrive.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Reflection;
//using IdentityServer4.EntityFramework.DbContexts;
//using CustomGoogleDrive.Config;
//using IdentityServer4;
//using IdentityServer4.EntityFramework.Mappers;

namespace CustomGoogleDrive
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
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options
                    .UseSqlServer(connectionString)
                    .EnableSensitiveDataLogging()
                    .ConfigureWarnings(i => i.Throw())
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

            services.AddIdentity<ApplicationUser, IdentityRole<int>>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            //services.AddIdentityServer(options =>
            //{
            //    options.Events.RaiseErrorEvents = true;
            //    options.Events.RaiseInformationEvents = true;
            //    options.Events.RaiseFailureEvents = true;
            //    options.Events.RaiseSuccessEvents = true;
            //})
            //    .AddConfigurationStore(options =>
            //    {
            //        options.ConfigureDbContext = b =>
            //            b.UseSqlServer(connectionString,
            //                sql => sql.MigrationsAssembly(migrationsAssembly));
            //    })
            //    // this adds the operational data from DB (codes, tokens, consents)
            //    .AddOperationalStore(options =>
            //    {
            //        options.ConfigureDbContext = b =>
            //            b.UseSqlServer(connectionString,
            //                sql => sql.MigrationsAssembly(migrationsAssembly));

            //        // this enables automatic token cleanup. this is optional.
            //        options.EnableTokenCleanup = true;
            //        // options.TokenCleanupInterval = 15; // frequency in seconds to cleanup stale grants. 15 is useful during debugging
            //    })
            //    .AddAspNetIdentity<ApplicationUser>();

            services
                .AddAuthentication()
                .AddGoogle(googleOptions =>
                {
                    //googleOptions.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    googleOptions.AccessType = "offline";
                    googleOptions.ClientId = Configuration["Authentication:Google:ClientId"];
                    googleOptions.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
                    googleOptions.Scope.Add("https://www.googleapis.com/auth/plus.login");
                    googleOptions.SaveTokens = true;
                    googleOptions.Events.OnCreatingTicket = ctx =>
                    {
                        if (ctx.Properties.GetTokens() is List<AuthenticationToken> tokens)
                        {
                            tokens.Add(new AuthenticationToken()
                            {
                                Name = "TicketCreated",
                                Value = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)
                            });
                            ctx.Properties.StoreTokens(tokens);
                        }

                        return Task.CompletedTask;
                    };
                })
                .AddFacebook(facebookOptions =>
                {
                    facebookOptions.AppId = Configuration["Authentication:Facebook:AppId"];
                    facebookOptions.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
                    facebookOptions.SaveTokens = true;
                    facebookOptions.Scope.Add("email");
                }); 

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc();
        }

        //private static void InitializeDatabase(IApplicationBuilder app)
        //{
        //    using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
        //    {
        //        serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

        //        var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
        //        context.Database.Migrate();
        //        if (!context.Clients.Any())
        //        {
        //            foreach (var client in AuthConfig.GetClients())
        //            {
        //                context.Clients.Add(client.ToEntity());
        //            }
        //            context.SaveChanges();
        //        }

        //        if (!context.IdentityResources.Any())
        //        {
        //            foreach (var resource in AuthConfig.GetIdentityResources())
        //            {
        //                context.IdentityResources.Add(resource.ToEntity());
        //            }
        //            context.SaveChanges();
        //        }

        //        if (!context.ApiResources.Any())
        //        {
        //            foreach (var resource in AuthConfig.GetApiResources())
        //            {
        //                context.ApiResources.Add(resource.ToEntity());
        //            }
        //            context.SaveChanges();
        //        }
        //    }
        //}
    }
}
