using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartzmin;
using Scheduler.Core;
using Scheduler.Jobs.Core;

namespace Scheduler
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            _configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<QuartzOptions>(_configuration.GetSection("Quartz"));
            // services.AddSingleton(StdSchedulerFactory.GetDefaultScheduler().Result);
            services.AddQuartz();
            //1.register jobs middleware.
            services.AddQuartzJob();
            //2.create web page from scheduler.
            services.AddQuartzmin();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var scheduler = InitializeSchedulerAsync(app.ApplicationServices).Result;
            app.UseQuartzmin(new QuartzminOptions
            {
                Scheduler = scheduler,
                ProductName = "Crust",
                Logo = "data:image/png;base64,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"
            });
        }

        private static async Task<IScheduler> InitializeSchedulerAsync(IServiceProvider serviceProvider)
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var section = configuration.GetSection(nameof(SchedulerSettings));

            var scheduler = await serviceProvider.GetRequiredService<ISchedulerFactory>().GetScheduler();
            scheduler.JobFactory = new DependencyJobFactory(serviceProvider);

            if (section != null && !section.Get<SchedulerSettings>().AutoStartup)
                return scheduler;
            //if no config. by default is auto start.
            await scheduler.Start();
            await scheduler.ResumeAll();

            return scheduler;
        }
    }
}