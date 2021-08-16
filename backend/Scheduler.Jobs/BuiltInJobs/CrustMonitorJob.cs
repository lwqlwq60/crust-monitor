using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Jobs.BuiltInJobs
{
    public class CrustMonitorJob : IJob
    {
        private const string CrustDir = "/opt/crust";

        private readonly ILogger<CrustMonitorJob> _logger;
        private readonly DockerClient _client;

        public CrustMonitorJob(ILogger<CrustMonitorJob> logger, DockerClient client)
        {
            _logger = logger;
            _client = client;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            var containers = await _client.Containers.ListContainersAsync(new ContainersListParameters { All = true });
            foreach (var container in containers)
            {
                _logger.LogInformation(container.Image);
            }

        }
    }
}
