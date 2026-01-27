using System;
using System.Configuration;
using KS.FiksProtokollValidator.WebAPI.Data;
using KS.FiksProtokollValidator.WebAPI.FiksIO;
using KS.FiksProtokollValidator.WebAPI.FiksIO.Connection;
using KS.FiksProtokollValidator.WebAPI.TjenerValidator.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KS.FiksProtokollValidator.WebAPI
{
    public class Startup
    {
        private const string AllowedOrigins = "_allowedOrigins";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }
        
        public void ConfigureServices(IServiceCollection services, ILoggerFactory loggerFactory)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(name: AllowedOrigins,
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:8081",
                            "http://localhost:8080",
                            "http://localhost:64558",
                            "http://localhost:5173",
                            "https://forvaltning.fiks.dev.ks.no",
                            "https://forvaltning.fiks.test.ks.no",
                            "https://forvaltning.fiks.ks.no")
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials(); 
                    });
            });
            
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = _ => false;
                options.MinimumSameSitePolicy = SameSiteMode.Lax;
                options.Secure = CookieSecurePolicy.None;
            });
            
            // get configuration from appsettings.json - use as singleton
            var appSettings = CreateAppSettings();
            services.AddSingleton(appSettings);

            services.AddControllers();

            if (appSettings.StandaloneMode)
            {
                services.AddDbContext<FiksIOMessageDBContext>(options =>
                    options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));
                services.AddSingleton<ISendMessageService, MockSendMessageService>();
            }
            else
            {
                var fiksProtokollServicesManager = new FiksIOConnectionManager(appSettings, loggerFactory);
                services.AddSingleton(fiksProtokollServicesManager);
                services.AddHostedService<TjenerMessagesSubscriber>();
                services.AddHostedService<KlientMessagesSubscriber>();
                services.AddDbContext<FiksIOMessageDBContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
                services.AddSingleton<ISendMessageService, SendMessageService>();
            }

            services.AddScoped<IFiksResponseValidator, FiksResponseValidator>();
            services.AddScoped<ITestSeeder, TestSeeder>();
        }

        private AppSettings CreateAppSettings()
        {
            var appSettings = Configuration.GetSection("AppSettings").Get<AppSettings>();
            
            // Available from maskinporten-config config-map in k8s cluster
            var maskinportenClientId = Environment.GetEnvironmentVariable("MASKINPORTEN_CLIENT_ID");
            if (!string.IsNullOrEmpty(maskinportenClientId))
            {
                appSettings.TjenerValidatorFiksIOConfig.MaskinPortenIssuer = maskinportenClientId;
            }
            
            return appSettings;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment() || IsDockerCompose(env) || IsStandalone(env))
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                serviceScope.ServiceProvider.GetService<ITestSeeder>();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors(AllowedOrigins);
            
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static bool IsDockerCompose(IHostEnvironment hostEnvironment)
        {
            if (hostEnvironment == null)
            {
                throw new ArgumentNullException(nameof(hostEnvironment));
            }

            return hostEnvironment.IsEnvironment("DockerCompose");
        }

        private static bool IsStandalone(IHostEnvironment hostEnvironment)
        {
            ArgumentNullException.ThrowIfNull(hostEnvironment);

            return hostEnvironment.IsEnvironment("Standalone");
        }
    }
}
