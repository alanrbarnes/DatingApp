using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using API.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //CreateHostBuilder(args).Build().Run();
            //Changes here will make automatic migration making auto changes to database
            var host = CreateHostBuilder(args).Build();

            /*
            WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .UseKestrel(options =>
            {
                options.Listen(IPAddress.Loopback, 5000, listenOptions =>
                {
                    listenOptions.UseHttps("localhost.pfx", "yourPassword");
                });
            })
            .UseUrls("https://localhost:5000")
            .Build();
            */

            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<DataContext>();
                await context.Database.MigrateAsync();
                await Seed.SeedUsers(context);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred during migration");
            }

            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>{
                    HostConfig.CertPath = context.Configuration["CertPath"];
                    HostConfig.CertPassword = context.Configuration["CertPassword"];
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var host = Dns.GetHostEntry("datingapp.io");
                    webBuilder.ConfigureKestrel(opt =>{
                        //opt.ListenAnyIP(5000);
                        opt.Listen(host.AddressList[0], 5000);
                        opt.Listen(host.AddressList[0], 5001, listOpt =>
                        //opt.ListenAnyIP(5001, listOpt =>
                        {
                            listOpt.UseHttps(HostConfig.CertPath, HostConfig.CertPassword);
                        });
                    });
                webBuilder.UseStartup<Startup>();
                });

                
    }

    public static class HostConfig
    {
        public static string CertPath { get; set; } 
        public static string CertPassword { get; set; }
    }
}

/*
.UseKestrel(options =>
                {
                    options.Listen(IPAddress.Loopback, 5001, listenOptions =>
                    {
                        listenOptions.UseHttps("client/ssl/server.pfx", "BigAlsDevelopment");
                    });
                })
                .UseUrls("https://localhost:5001")
                */
/*
var host = new WebHostBuilder()
    .UseConfiguration(config)
    .UseKestrel(options => {
        options.UseHttps("localhost.pfx", "password");
    })
    .UseContentRoot(Directory.GetCurrentDirectory())
    .UseIISIntegration()
    .UseUrls("https://*:4430")
    .UseStartup<Startup>()
    .Build();
    */