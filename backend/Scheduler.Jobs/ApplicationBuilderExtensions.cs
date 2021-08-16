using System;
using System.Net.Http;
using Docker.DotNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Scheduler.Jobs.BuiltInJobs;

namespace Scheduler.Jobs
{
    public static class ApplicationBuilderExtensions
    {
        public static IServiceCollection AddBuiltInJob(this IServiceCollection services)
        {
            //var provider = services.BuildServiceProvider();
            //var configuration = provider.GetRequiredService<IConfiguration>();
            //var settings = configuration.GetSection(nameof(JobSettings)).Get<JobSettings>();

            //var timeout = Policy.TimeoutAsync<HttpResponseMessage>(
            //    TimeSpan.FromSeconds(settings.HttpClientGetTimeout ?? 10));
            //var longTimeout = Policy.TimeoutAsync<HttpResponseMessage>(
            //    TimeSpan.FromSeconds(settings.HttpClientPostTimeout ?? 20));

            //var handledEventsAllowedBeforeBreaking = 5;
            //var durationOfBreak = 30;

            //if (settings.HandledEventsAllowedBeforeBreaking != null)
            //{
            //    handledEventsAllowedBeforeBreaking = settings.HandledEventsAllowedBeforeBreaking.Value;
            //}

            //if (settings.DurationOfBreak != null) durationOfBreak = settings.DurationOfBreak.Value;

            //services.AddHttpClient<OAuth2RestfulClientJob>().AddPolicyHandler(HttpClientPolicy.GetCircuitBreakerPolicy(
            //        handledEventsAllowedBeforeBreaking
            //        , durationOfBreak))
            //    .AddPolicyHandler(request => request.Method == HttpMethod.Get ? timeout : longTimeout)
            //    .SetHandlerLifetime(TimeSpan.FromMinutes(5));

            services.AddSingleton(provider => new DockerClientConfiguration(new Uri("http://host.docker.internal:4243")).CreateClient());
            services.AddScoped<CrustMonitorJob>();
            return services;
        }
    }
}