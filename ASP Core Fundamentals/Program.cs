using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace ASP_Core_Fundamentals
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // This is a simple Main() method that gets executed


            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>() // A Startup class
                .UseApplicationInsights()
                .Build();

            host.Run();
        }
    }
}
