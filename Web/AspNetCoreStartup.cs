using System;
using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Extensions.MsDependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using React.AspNet;
using unbooru.Web.OpenGraph;

namespace unbooru.Web
{
    public class AspNetCoreStartup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddControllersAsServices().AddNewtonsoftJson(
                options => options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            );
            services.AddSignalR(o =>
            {
                o.EnableDetailedErrors = true;
            });
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder
                            .AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
            });
            services.AddJsEngineSwitcher(options => options.DefaultEngineName = ChakraCoreJsEngine.EngineName)
                .AddChakraCore();
            services.AddReact();
            services.AddResponseCaching(options =>
            {
                options.SizeLimit = 1024L * 1024L * 1024L * 10L;
            });
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }
 
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseMiddleware<OpenGraphMiddleware>();
            // Caching Control
            app.Use(async (ctx, next) =>
            {
                var url = ctx.Request.Path.ToString().Trim('/');
                if (url.StartsWith("image", StringComparison.InvariantCultureIgnoreCase) ||
                    url.StartsWith("api/image", StringComparison.InvariantCultureIgnoreCase) ||
                    url.StartsWith("api/tag", StringComparison.InvariantCultureIgnoreCase) ||
                    url.StartsWith("api/artist", StringComparison.InvariantCultureIgnoreCase))
                {
                    ctx.Request.GetTypedHeaders().CacheControl = new CacheControlHeaderValue
                    {
                        Public = true,
                        MaxAge = TimeSpan.FromMinutes(60)
                    };
                }
                if (url.StartsWith("search", StringComparison.InvariantCultureIgnoreCase) ||
                    url.StartsWith("api/search", StringComparison.InvariantCultureIgnoreCase))
                {
                    ctx.Request.GetTypedHeaders().CacheControl = new CacheControlHeaderValue
                    {
                        Public = true,
                        MaxAge = TimeSpan.FromMinutes(15)
                    };
                }
                await next();
            });
            app.UseResponseCaching();
            // Initialise ReactJS.NET. Must be before static files.
            app.UseReact(config =>
            {
                config
                    .SetReuseJavaScriptEngines(true)
                    .SetLoadBabel(false)
                    .SetLoadReact(false)
                    .SetReactAppBuildPath("~/dist");
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(conf =>
            {
                conf.MapControllers();
            });
            app.Use(async (context, next) =>
            {
                await next();
                var path = context.Request.Path;
                if (path.HasValue && path.Value != null && !path.Value.StartsWith("/api") &&
                    context.Response.StatusCode == 404)
                {
                    context.Request.Path = "/NotFound";
                    await next();
                }
            });
        }
    }
}
