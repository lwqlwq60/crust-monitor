using System;
using System.Net.Http;
using Polly;
using Polly.Extensions.Http;

namespace Scheduler.Jobs
{
    public static class HttpClientPolicy
    {
        public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(int handledEventsAllowedBeforeBreaking,
            int durationOfBreak)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(handledEventsAllowedBeforeBreaking, TimeSpan.FromSeconds(durationOfBreak));
        }

        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
    }
}