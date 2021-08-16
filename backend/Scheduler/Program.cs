using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Quartz.Util;
using Serilog;
using Serilog.Exceptions;

namespace Scheduler
{
    public class Program
    {
        private static readonly Serilog.ILogger Logger = new LoggerConfiguration()
            .Enrich.WithExceptionDetails()
            .WriteTo.Console()
            .CreateLogger();

        public static void Main(string[] args)
        {
            FixQuartzLinuxTimezoneMapping();
 
            try
            {
                Logger.Information("Initializing in Scheduler:Main...");

                CreateWebHostBuilder(args).Build().Run();
            
                Logger.Information("Exiting with code 0.");
                
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "Host terminated unexpectedly");
                
                Environment.Exit(1);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        //https://en.wikipedia.org/wiki/List_of_tz_database_time_zones
        private static void FixQuartzLinuxTimezoneMapping()
        {
            var isLinux = RuntimeInformation
                .IsOSPlatform(OSPlatform.Linux);
            if (isLinux)
            {
                var timeZoneIdAliases =
                    (Dictionary<string, string>) typeof(TimeZoneUtil).GetField("timeZoneIdAliases",
                        BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null);

                if (timeZoneIdAliases != null)
                    timeZoneIdAliases["China Standard Time"] = "Asia/Shanghai";
            }
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Trace);
                })
                .UseSerilog((hostingContext, loggerConfiguration) => {
                    loggerConfiguration
                        .ReadFrom.Configuration(hostingContext.Configuration)
                        .Enrich.FromLogContext()
                        .Enrich.WithProperty("ApplicationName", typeof(Program).Assembly.GetName().Name)
                        .Enrich.WithProperty("Environment", hostingContext.HostingEnvironment);

#if DEBUG
                    // Used to filter out potentially bad data due debugging.
                    // Very useful when doing Seq dashboards and want to remove logs under debugging session.
                    loggerConfiguration.Enrich.WithProperty("DebuggerAttached", Debugger.IsAttached);
#endif
                });
        }
    }
}