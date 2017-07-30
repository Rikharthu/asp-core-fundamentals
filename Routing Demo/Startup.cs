using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Routing.Constraints;

namespace Routing_Demo
{
    // More info:   https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing#using-routing-middleware
    //              https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/routing

    // About URL-Rewriting and URL-Redirects: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/url-rewriting#when-to-use-url-rewriting-middleware

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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
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

            // Incoming requests enter the RouterMiddleware, which calls the RouteAsync method on each route in sequence
            // The IRouter instance chooses whether to handle the request by setting the RouteContext Handler to a non-null RequestDelegate
            /// <see cref="Microsoft.AspNetCore.Routing.IRouter"/> - chooses whether to handle a request
            /// <see cref="Microsoft.AspNetCore.Routing.RouteContext"/> - primary input for IRouter during routing process
            // If a route sets a handler for the request, route processing stops and the handler will be invoked to process the request
            // If all routes are tried and no handler is found for the request, the middleware calls next() middleware in the request pipeline
            // A match will also set the properties of the RouteContext.RouteData to appropriate values based on the request processing done so far
            /// <see cref="Microsoft.AspNetCore.Routing.RouteData"/> - primary input for IRouter during routing process
            /// RouteData.Values - dictionary of route values produced from the route. only strings
            /// RouteData.DataTokens - property bag of additinal data related to the matched route.developer defined, any value types
            /// RouteData.Routers - a list of the routes that took part in successfully matching the request. 
            ///     last item is the route handler that matched, first item is the route collectiom


            // Adds the routing middleware to the request pipeline and configures MVC as the default handler
            app.UseMvc(routes =>
            {
                // This example adds route constraints and data tokens
                // http://localhost:58355/en-US/Products/3
                routes.MapRoute(
                    name: "us_english_products",
                    template: "en-US/Products/{id:int}",
                    defaults: new { controller = "Products", action = "Details" },
                    constraints: new { id = new IntRouteConstraint() },
                    dataTokens: new { locale = "en-US", routeName = "us_english_products" });
                // dataTokens contain additional information that can be later extracted in a controller 
                // like: var routeName = (string)RouteData.DataTokens["routeName"];

                // Default route. Routes are processed in the order they are mapped here, thus more specific cases should be at the top
                // create route(s)
                routes.MapRoute(
                    name: "default", // doesnt affect request, used for generation
                    template: "{controller=Home}/{action=Index}/{id?}");
                // this template above will match a URL path like /Products/Details/17 and extract the route values
                // {controller = Products, aciton=Details, id=17}
                // URL path like / would produce {controller=Home, action=index} by usign specified default values
                // More info https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing#route-template-reference 

                // Additional overloads of MapRoute accept values for constraints, dataTokens and defaults, defined as type object
                // This would produce the same result as above:
                //  routes.MapRoute(
                //      name: "default_route",
                //      template: "{controller}/{action}/{id?}",
                //      defaults: new { controller = "Home", action = "Index"
                //  });

                routes.MapRoute(
                      name: "blog",
                      template: "Blog/{*article}",
                      defaults: new { controller = "Blog", action = "ReadArticle" });
                // This template will match a URL path like /Blog/All-About-Reading/Introduction and will extract the values
                // { controller = Blog, action = ReadArticle, article = All-About-Routing/Introduction }
                // * asterisk is used as catch-all parameter and extract it to a named parameter              
            });
        }
    }
}
