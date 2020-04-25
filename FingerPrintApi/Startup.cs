using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FingerPrintApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FingerPrintApi
{
    public class Startup
    {
        public static readonly string CORS_ALLOW_ALL = "allowAllOrigins";
        public static readonly string CORS_ALLOW_SPECIFIC = "allowSpecificOrigins";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(CORS_ALLOW_ALL,
                    builder =>
                    {
                        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                    });
            });
            services.AddCors(options =>
            {
                options.AddPolicy(CORS_ALLOW_SPECIFIC,
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:4200");
                    });
            });

            services.AddSingleton<IFingerprintService, FingerprintService>(c =>
            {
                var fps = new  FingerprintService();
                fps.Fingerprint.SerialPortName = "/dev/serial0";
                return fps;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseCors(CORS_ALLOW_ALL); // Add this line here or inside if (env.IsDevelopment()) block

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
