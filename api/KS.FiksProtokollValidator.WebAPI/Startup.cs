using System;
using System.Threading.Tasks;
using KS.Fiks.IO.Client;
using KS.FiksProtokollValidator.WebAPI.Data;
using KS.FiksProtokollValidator.WebAPI.FiksIO;
using KS.FiksProtokollValidator.WebAPI.Health;
using KS.FiksProtokollValidator.WebAPI.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KS.FiksProtokollValidator.WebAPI
{
    public class Startup
    {
        private const string AllowedOrigins = "_allowedOrigins";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        
        public async Task ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(name: AllowedOrigins,
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:8081",
                            "http://localhost:8080",
                            "http://localhost:64558",
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
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.Lax;
                options.Secure = CookieSecurePolicy.None;
            });
            
            // get configuration from appsettings.json - use as singleton
            var appSettings = CreateAppSettings();
            services.AddSingleton(appSettings);
            var fiksIOClientService = new FiksIOClientConsumerService(appSettings);
            services.AddSingleton<IFiksIOClientConsumerService>(fiksIOClientService);
            services.AddControllers();
            services.AddHostedService<FiksResponseMessageService>();
            services.AddDbContext<FiksIOMessageDBContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            var fiksRequestMessageService = new FiksRequestMessageService(appSettings);
            services.AddSingleton<IFiksRequestMessageService>(fiksRequestMessageService);
            services.AddScoped<IFiksResponseValidator, FiksResponseValidator>();
            services.AddScoped<ITestSeeder, TestSeeder>();
        }
        
        public AppSettings CreateAppSettings()
        {
            var appSettings = Configuration.GetSection("AppSettings").Get<AppSettings>();
            
            // Available from maskinporten-config config-map in k8s cluster
            var maskinportenClientId = Environment.GetEnvironmentVariable("MASKINPORTEN_CLIENT_ID");
            if (!string.IsNullOrEmpty(maskinportenClientId))
            {
                appSettings.FiksIOConfig.MaskinPortenIssuer = maskinportenClientId;
            }
            
            return appSettings;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment() || IsDockerCompose(env))
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
    }
}
