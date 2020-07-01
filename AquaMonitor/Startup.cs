using System;
using AquaMonitor.Data.Context;
using AquaMonitor.Data.Models;
using AquaMonitor.Web.Global;
using AquaMonitor.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AquaMonitor.Web
{
    /// <summary>
    /// Startup
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Main entry
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration, IHostEnvironment env)
        {
            Configuration = configuration;
            this.env = env;
        }
        
        /// <summary>
        /// Configuration
        /// </summary>
        public IConfiguration Configuration { get; }
        private IHostEnvironment env { get; }

        /// <summary>
        /// Service Build up
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AquaMonitor.Data.Context.AquaServiceDbContext>(ServiceLifetime.Singleton);
            services.AddDbContext<AquaMonitor.Data.Context.AquaDbContext>(ServiceLifetime.Scoped);
            services.AddSingleton<IGlobalState, GlobalData>();
            services.AddSingleton(this.Configuration);
            services.AddSingleton<IPowerRelayService, PowerRelayService>();
            services.AddHostedService<AtmosphereService>();
            services.AddHostedService<WaterLevelService>();
            services.AddHostedService<SystemOperationsService>();            
            services.AddHostedService<NetworkHealthService>();
            services.AddHostedService<WeatherServices>();
            services.AddHostedService<CameraService>();
            //services.AddSingleton<SkyService>(new SkyService(env));
            services.AddSingleton<SkyService>();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("InMemoryDb"));

            services.AddDefaultIdentity<AppUser>(config =>
                {
                    // User defined password policy settings.  
                    config.Password.RequiredLength = 4;
                    config.Password.RequireDigit = false;
                    config.Password.RequireNonAlphanumeric = false;
                    config.Password.RequireUppercase = false;
                }).AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(config =>
            {
                config.Cookie.Name = "AquaMonitorToken";
                config.LoginPath = "/Home/Login"; // User defined login path  
                config.ExpireTimeSpan = TimeSpan.FromMinutes(5);
            });

            services.AddControllersWithViews();
#if debug
            services.AddRazorPages();
#else
            services.AddRazorPages().AddRazorRuntimeCompilation();
#endif
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
        }

        /// <summary>
        /// HTTP Pipeline
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="userManager"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, UserManager<AppUser> userManager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //app.UseHsts();
            }
            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseAuthentication();


            var globalSettings = app.ApplicationServices.GetService<IGlobalState>();
            var internalUser =
                new AppUser()
                {
                    UserName = "admin",
                    Id = "8d4e3fe7-9018-4ed5-b5ee-646beb2f64bf",
                    LockoutEnabled = false,
                    Password = globalSettings.AdminPassword
                };
            //var userManager = app.ApplicationServices.GetService<UserManager<IdentityUser>>();
            userManager.CreateAsync(internalUser, internalUser.Password);


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "defaultApi",
                    pattern: "{controller}/{id?}");
            });
        }
    }
}
