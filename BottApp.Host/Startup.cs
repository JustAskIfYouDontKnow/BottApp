﻿using BottApp.Database;
using BottApp.Host.Configs;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace BottApp.Host
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private IWebHostEnvironment CurrentEnvironment{ get; set; }
        
        private ILogger _logger;
        private ILoggerFactory _loggerFactory;

        
        public Startup(IConfiguration configuration, IWebHostEnvironment hostEnvironment, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;
            CurrentEnvironment = hostEnvironment;
            _logger = loggerFactory.CreateLogger<Startup>();
            _loggerFactory = loggerFactory;
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddCors
            (
                options =>
                {
                    options.AddPolicy
                    (
                        "CorsPolicy",
                        policy =>
                            policy.WithOrigins(Configuration.GetSection("AllowedHosts").Value)
                                .WithMethods("POST", "GET", "DELETE", "OPTIONS")
                                .WithHeaders("*")
                    );
                    options.AddPolicy
                    (
                        "apiDocumentation",
                        policy =>
                            policy.WithOrigins("*")
                                .WithMethods("POST", "GET", "DELETE")
                                .WithHeaders("*")
                    );
                }
            );
            
            ConfigureCoreServices(services, CurrentEnvironment);

            
            services.AddSwaggerGen(
                c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "BottApp.Host", Version = "v1"}); }
            );
        }

        private void ConfigureCoreServices(IServiceCollection services, IWebHostEnvironment env)
        {
            var botConfig = ConfigValidator.GetConfig<BotConfig>(Configuration, "Bot");
            services.AddSingleton(botConfig);
            
            BotInit.InitReceiver(botConfig);

            Type typeOfContent = typeof(Startup);
            
            services.AddDbContext<PostgreSqlContext>(
                opt => opt.UseNpgsql(
                    Configuration.GetConnectionString("PostgreSqlConnection"),
                    b => b.MigrationsAssembly(typeOfContent.Assembly.GetName().Name)
                )
            );

            services.AddScoped<IDatabaseContainer, DatabaseContainer>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BottApp.Host v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}