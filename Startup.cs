using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using LibraryApi2.Services;
using LibraryApi2.Domain;
using Microsoft.EntityFrameworkCore;
using LibraryApi.Services;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.IO;

namespace LibraryApi2
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
            services.AddControllers().AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new StringToIntConverter())
            );

            // Every time an instance of the interface is requested, a new instance of the EnrollmentIdGenerator will be created
            services.AddTransient<IGenerateEnrollmentIds, EnrollmentIdGenerator>();

            services.AddDbContext<LibraryDataContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("LibraryDatabase"))

            );

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Library Api",
                    Version = "1.0",
                    Contact = new OpenApiContact
                    {
                        Name = "Mat Partin",
                        Email = "partinmathew@gmailcom"
                    },
                    Description = @"This is the API for our library. Make sure you pay your late fees!"
                });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Library API");
                c.RoutePrefix = string.Empty;
            });

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
