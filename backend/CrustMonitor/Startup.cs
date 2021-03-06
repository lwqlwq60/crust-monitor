using System;
using System.Net.Http;
using Blazored.Modal;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CrustMonitor.Data;
using NETCore.MailKit.Extensions;
using NETCore.MailKit.Infrastructure.Internal;
using Polly;

namespace CrustMonitor
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddBlazoredModal();
            services.AddSingleton<CrustService>();
            services.AddScoped<AlertService>();
            services.AddMailKit(config =>
            {
                var options = Configuration.GetSection(nameof(MailKitOptions)).Get<MailKitOptions>();
                config.UseMailKit(options);
            });
            services.AddHttpClient("crust-monitor")
                .AddPolicyHandler(request => request.RequestUri.ToString().Contains("health")
                    ? Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(2))
                    : Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10)))
                .SetHandlerLifetime(TimeSpan.FromMinutes(5));

            services.AddHttpClient("subscan")
                .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(20)))
                .SetHandlerLifetime(TimeSpan.FromMinutes(5));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}