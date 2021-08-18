using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace CrustMonitor.Data
{
    public class CrustService
    {
        private readonly HttpClient _client;

        public CrustService(HttpClient client)
        {
            _client = client;
        }

        private async Task<bool> CheckHealthAsync(string nodeIp)
        {
            try
            {
                var result = await _client.GetAsync($"http://{nodeIp}:51888/api/crust-node/health");
                if (result.IsSuccessStatusCode)
                    return true;
            }
            catch
            {
                // ignored
            }

            return false;
        }

        public async Task<IEnumerable<CrustNode>> SearchLocalNodesAsync(string ip)
        {
            var nodes = new List<CrustNode>();

            var ipv4 = ip.Split('.');
            if (ipv4.Length == 4)
            {
                if (await CheckHealthAsync(ip))
                    nodes.Add(new CrustNode { Ip = ip });
            }
            else if (ipv4.Length == 3)
            {
                var tasks = new List<Task<bool>>();
                for (var i = 2; i < 255; i++)
                {
                    var localIp = $"{ipv4[0]}.{ipv4[1]}.{ipv4[2]}.{i}";
                    var task = CheckHealthAsync(localIp);
                    tasks.Add(task);
                }

                var results = await Task.WhenAll(tasks.ToArray());
                for (var i = 0; i < results.Length; i++)
                {
                    if (results[i])
                        nodes.Add(new CrustNode { Ip = $"{ipv4[0]}.{ipv4[1]}.{ipv4[2]}.{i + 2}" });
                }
            }

            return nodes;
        }
    }
}