﻿using BottApp.Database;
using BottApp.Host.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using BottApp.Host.Services.Handlers;

namespace BottApp.Host
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private IWebHostEnvironment CurrentEnvironment{ get; }

        
        public Startup(IConfiguration configuration, IWebHostEnvironment hostEnvironment)
        {
            Configuration = configuration;
            CurrentEnvironment = hostEnvironment;
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
                c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "BottApp.BotHost.Exp", Version = "v1"}); }
            );
        }

        private void ConfigureCoreServices(IServiceCollection services, IWebHostEnvironment env)
        {
            services.AddScoped<IDatabaseContainer, DatabaseContainer>();
            services.AddScoped<IHandlerContainer>(x => Factory.Create(x.GetRequiredService<IDatabaseContainer>()));

            TelegramBotStartup.ConfigureServices(services, Configuration);
            
            var typeOfContent = typeof(Startup);

            services.AddDbContext<PostgresContext>(
                opt => opt.UseNpgsql(
                    Configuration.GetConnectionString("PostgreSqlConnection"),
                    b => b.MigrationsAssembly(typeOfContent.Assembly.GetName().Name)
                )
            );
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BottApp.BotHost.Exp v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
        
        
    }
}