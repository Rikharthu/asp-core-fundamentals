using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace URL_Rewriting
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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // More info https://github.com/aspnet/Docs/tree/master/aspnetcore/fundamentals/url-rewriting/sample
            // Configure and Add URL-Rewriting middleware
            var options = new RewriteOptions()
                .AddRedirect( // causes to return 302(Found) code
                    "redirect-rule/(.*)", // regex for matching on the path of the incoming URL 
                    "api/redirected/$1"   // replacement string
                    ) // replaces "/redirect-rule/1234/5678" with "/api/redirected/1234/5678"
                      // (.*) is a capture group (in parentheses). "1234/5678" is captured by that and is accessed by $<capture_group_index}, $1 for first
                .AddRewrite( // create a rule for rewriting URLS
                    @"^rewrite-rule/(\d+)/(\d+)", // incoming url regex for matching
                    "api/rewritten?var1=$1&var2=$2", // replacement string. example: http://localhost:64481/rewrite-rule/1234/5678 => http://localhost:64481/rewritten?var1=1234&var2=5678
                    skipRemainingRules: true // do not process any other rules
                ) // The rewrite rule, ^rewrite-rule/(\d+)/(\d+), will only match paths if they start with rewrite-rule/, it won't match "my-cool-rewrite-rule/"
                  //.AddApacheModRewrite(env.ContentRootFileProvider, "ApacheModRewrite.txt") // these are server-specific rewrite rules fetched from your files
                  //.AddIISUrlRewrite(env.ContentRootFileProvider, "IISUrlRewrite.xml");
                  // These are Method/Class-based rules
                .Add(RedirectXMLRequests) // register method-base redirect/rewrite rule
                // IRule-based rules
                .Add(new RedirectImageRequests(".png", "/png-images"))
                .Add(new RedirectImageRequests(".jpg", "/jpg-images"));

            // Register our rewrite middleware
            app.UseRewriter(options);

            app.UseMvc();
        }

        /// <summary>
        /// This Rewrite Rule REDIRECTS to /xmlfiles/<original_request_path> if XML file is requested (path ends with .xml)
        /// Thus http://localhost:64481/api/values.xml becomes http://localhost:64481/xmlfiles/api/values.xml
        /// </summary>
        /// <param name="context"></param>
        static void RedirectXMLRequests(RewriteContext context)
        {
            // extract the original request
            var request = context.HttpContext.Request;

            // Because we're redirecting back to the same app, stop processing if the request has already been redirected
            if (request.Path.StartsWithSegments(new PathString("/xmlfiles")))
            {
                return;
            }

            if (request.Path.Value.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            {
                var response = context.HttpContext.Response;
                response.StatusCode = StatusCodes.Status301MovedPermanently;
                // context.Result determines how additional pipeline is handled
                context.Result = RuleResult.EndResponse; // stop applying rules and send the response
                // add "Location" header. By looking at it, browsers will automatically use the new location at this header if returned code 302/301 and etc
                response.Headers[HeaderNames.Location] = "/xmlfiles" + request.Path + request.QueryString;
            }
        }
    }
}
