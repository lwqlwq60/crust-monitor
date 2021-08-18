using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace CrustMonitor.Data
{
    public class CrustService
    {
        private static IHttpClientFactory _factory;
        private const string NodesFile = "./nodes.txt";
        public static readonly List<CrustNode> Nodes = new List<CrustNode>();

        public CrustService(IHttpClientFactory factory)
        {
            _factory = factory;
        }

        private static HttpClient CreateClient()
        {
            return _factory!.CreateClient("crust-monitor");
        }

        static CrustService()
        {
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    try
                    {
                        await Task.Delay(5000);
                        await UpdateNodeStatusAsync();
                    }
                    catch
                    {
                        // ignored
                    }
                }
            });
        }

        private static async Task UpdateNodeStatusAsync()
        {
            foreach (var node in Nodes)
            {
                node.Online = await CheckHealthAsync(node.Ip);
                await GetNodeStatusAsync(node);
            }
        }

        private static async Task<bool> CheckHealthAsync(string nodeIp)
        {
            try
            {
                var result = await CreateClient().GetAsync($"http://{nodeIp}:51888/api/crust-monitor/health");
                if (result.IsSuccessStatusCode)
                    return true;
            }
            catch
            {
                // ignored
            }

            return false;
        }

        public static async Task DeleteNodeAsync(string ip)
        {
            if (Nodes.Select(_ => _.Ip).Contains(ip))
            {
                var node = Nodes.FirstOrDefault(_ => _.Ip == ip);
                Nodes.Remove(node);
                await File.WriteAllLinesAsync(NodesFile, Nodes.Select(_ => _.Ip));
            }
        }

        public static async Task InitAsync()
        {
            if (File.Exists(NodesFile))
            {
                Nodes.Clear();
                var ips = await File.ReadAllLinesAsync(NodesFile);
                var tasks = new List<Task<bool>>();
                foreach (var ip in ips)
                {
                    var task = CheckHealthAsync(ip);
                    tasks.Add(task);
                }

                var results = await Task.WhenAll(tasks.ToArray());

                for (var i = 0; i < results.Length; i++)
                {
                    if (results[i])
                        Nodes.Add(new CrustNode { Ip = ips[i], Online = true });
                    else
                        Nodes.Add(new CrustNode { Ip = ips[i] });
                }

                foreach (var node in Nodes)
                {
                    await GetNodeStatusAsync(node);
                }
            }
        }

        public static async Task SearchLocalNodesAsync(string ip)
        {
            if (!Nodes.Select(_ => _.Ip).Contains(ip))
            {
                var changed = false;
                var ipv4 = ip.Split('.');
                if (ipv4.Length == 4)
                {
                    if (await CheckHealthAsync(ip))
                    {
                        changed = true;
                        Nodes.Add(new CrustNode { Ip = ip, Online = true });
                    }
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
                        {
                            changed = true;
                            Nodes.Add(new CrustNode { Ip = $"{ipv4[0]}.{ipv4[1]}.{ipv4[2]}.{i + 2}", Online = true });
                        }
                    }
                }

                if (changed)
                {
                    await File.WriteAllLinesAsync(NodesFile, Nodes.Select(_ => _.Ip));

                    foreach (var node in Nodes)
                    {
                        await GetNodeStatusAsync(node);
                    }
                }
            }
        }

        public static async Task<string> GetLogAsync(string ip, string type, int limit)
        {
            try
            {
                return await CreateClient().GetStringAsync($"http://{ip}:51888/api/crust-monitor/{type}-logs/{limit}");
            }
            catch
            {
                // ignored
            }

            return "Request error!";
        }

        private static async Task GetNodeStatusAsync(CrustNode node)
        {
            if (node.Online)
            {
                var status = await CreateClient().GetStringAsync($"http://{node.Ip}:51888/api/crust-monitor/status");
                var statusDict = JsonSerializer.Deserialize<IDictionary<string, ContainerStatus>>(status,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                node.ChainStatus = statusDict!["chain"].Status;
                node.ApiStatus = statusDict["api"].Status;
                node.SWorkerStatus = statusDict["sworker"].Status;
                node.SWorkerAStatus = statusDict["sworker-a"].Status;
                node.SWorkerBStatus = statusDict["sworker-b"].Status;
                node.SManagerStatus = statusDict["smanager"].Status;
                node.IpfsStatus = statusDict["ipfs"].Status;
            }
            else
            {
                node.ChainStatus = "Unknown";
                node.ApiStatus ="Unknown";
                node.SWorkerStatus ="Unknown";
                node.SWorkerAStatus = "Unknown";
                node.SWorkerBStatus = "Unknown";
                node.SManagerStatus = "Unknown";
                node.IpfsStatus = "Unknown"; 
            }
        }

        public static async Task<string> GetWorkloadAsync(string ip)
        {
            try
            {
                return await CreateClient().GetStringAsync($"http://{ip}:51888/api/crust-monitor/workload");
            }
            catch
            {
                // ignored
            }

            return "Request error!";
        }
    }
}