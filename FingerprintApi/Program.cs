using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FingerprintApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
#if (!DEBUG)
            Console.WriteLine("Release Version");
#endif
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
#if (!DEBUG)
            .UseKestrel(o =>
            {
                o.ListenAnyIP(1235); // default http pipeline
            })
#endif
                .UseStartup<Startup>();
    }
}
