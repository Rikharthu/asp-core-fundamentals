using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using ASP_Core_Fundamentals.Middleware;
using ASP_Core_Fundamentals.Services;

namespace ASP_Core_Fundamentals
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            // servies.AddDbContext()....

            // Dependency Injection Demo:
            // Transient lifetime services are created each time they are requested. This lifetime works best for lightweight, stateless services.
            services.AddTransient<IOperationTransient, Operation>();
            // Scoped lifetime services are created once per request
            services.AddScoped<IOperationScoped, Operation>();
            // Singleton lifetime services are created the first time they are requested and then every subsequent request will use the same instance
            services.AddSingleton<IOperationSingleton, Operation>();
            // add as implementation instance:
            services.AddSingleton<IOperationSingletonInstance>(new Operation(Guid.Empty));
            // add a class implementation
            services.AddTransient<OperationService, OperationService>();
            //services.AddSingleton<OperationService, OperationService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            // Speciy how the ASP.NET application will respond to HTTP requests
            // Configure the request pipeline by adding middleware components to an IApplicationBuilder instance
            // More info https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware

            // You configure the HTTP pipeline using Run, Map, and Use. 
            // The Run method short-circuits the pipeline(that is, it does not call a next request delegate). 
            // Run is a convention, and some middleware components may expose 
            // Run[Middleware] methods that run at the end of the pipeline.

            // You can chain multiple request delegates together with app.Use
            // Middleware delegates are executed in the order they are added to the IApplicationBuilder
            app.Use(async (context, next) =>
             {
                // context- HttpContext that you can edit
                // next - next delegate in the middleware request pipeline
                // Do work that doesn't write to the Response
                Console.WriteLine("Middleware 1, before calling next.Invoke()");
                 await next.Invoke();
                // Do logging or other work that doesn't write to the Response
                Console.WriteLine("Middleware 1, after calling next.Invoke()");
             });
            app.Use(async (context, next) =>
            {
                Console.WriteLine("Middleware 2, before calling next.Invoke()");
                await next.Invoke();
                Console.WriteLine("Middleware 2, after calling next.Invoke()");
            });
            app.Use(async (context, next) =>
            {
                Console.WriteLine("Middleware 3, before calling next.Invoke()");
                await next.Invoke();
                Console.WriteLine("Middleware 3, after calling next.Invoke()");
            });
            // Our custom middleware and it's extension method
            // http://localhost:53659/?culture=ru-RU
            app.UseRequestCulture();

            // Map* extensions are used as a convention for branching the pipeline.
            // Map branches the request pipeline based on matches of the given request path. 
            // If the request path starts with the given path, the branch is executed.
            // More info https://metanit.com/sharp/aspnet5/2.3.php
            // http://localhost:53659/apples
            app.Map("/apples", (appBuilder) =>
            {
                appBuilder.Run(async context =>
                {
                    await context.Response.WriteAsync("Hey apple!");
                });
            });
            // http://localhost:53659/oranges
            app.Map("/oranges", HandleMap2Request);

            // Add console middleware
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            // Simple possible ASP.NET COre app sets up a single request delegate that handles all requests
            // first app.Run delegate terminates the pipeline
            //  app.Run(async context =>
            //  {
            //      await context.Response.WriteAsync("Hello, World!");
            //  });
        }

        private static void HandleMap2Request(IApplicationBuilder app)
        {
            app.Run(async context =>
            {
                await context.Response.WriteAsync("Hey orange!");
            });
        }
    }
}
