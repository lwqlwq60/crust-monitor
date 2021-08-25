using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using NETCore.MailKit.Core;

namespace CrustMonitor.Data
{
    public class AlertService
    {
        private static IHttpClientFactory _factory;
        private static IEmailService _emailService;

        private const string SubScanEndpoint = "https://crust.api.subscan.io/api/scan/extrinsics";
        public static readonly List<Extrinsic> Extrinsics = new List<Extrinsic>();
        private const string AddressFile = "./addresses.txt";
        private const string MailFile = "./mails.txt";
        public static string Emails;

        public AlertService(IHttpClientFactory factory, IEmailService emailService)
        {
            _factory = factory;
            _emailService = emailService;
        }

        private static HttpClient CreateClient()
        {
            return _factory!.CreateClient("subscan");
        }

        static AlertService()
        {
            Task.Factory.StartNew(async () =>
            {
                var sb = new StringBuilder();
                while (true)
                {
                    try
                    {
                        sb.Clear();
                        await Task.Delay(TimeSpan.FromMinutes(10));
                        var emailService = _emailService;
                        for (var index = 0; index < Extrinsics.Count; index++)
                        {
                            var extrinsic = Extrinsics[index];
                            var result = await GetSWorkReportAsync(extrinsic.AccountDisplay.Address);
                            Extrinsics[index] = result;
                            if (!result.Success)
                            {
                                sb.AppendLine(
                                    $"地址： {result.AccountDisplay.Address} 工作量上报失败，交易ID：{result.ExtrinsicIndex}。");
                            }

                            if (DateTime.UtcNow - DateTime.UnixEpoch.AddSeconds(result.BlockTimestamp) >
                                TimeSpan.FromHours(2))
                            {
                                sb.AppendLine(
                                    $"地址： {result.AccountDisplay.Address} 超过2小时未上报工作量，交易ID：{result.ExtrinsicIndex}。");
                            }

                            await Task.Delay(2000);
                        }

                        if (emailService != null && !string.IsNullOrEmpty(Emails) && sb.Length > 0)
                            await emailService.SendAsync(Emails, "Crust Monitor", sb.ToString());
                    }
                    catch
                    {
                        // ignored
                    }
                }
            });
        }

        public static async Task<Extrinsic> GetSWorkReportAsync(string address)
        {
            try
            {
                var response = await CreateClient()
                    .PostAsJsonAsync(SubScanEndpoint, new SWorkRequest { Address = address });
                if (response.IsSuccessStatusCode)
                {
                    var model = await response.Content.ReadFromJsonAsync<ExtrinsicModel>();
                    if (model!.Code == 0)
                        return model.Data.Extrinsics[0];
                }
            }
            catch
            {
                // ignored
            }

            return new Extrinsic
                { ExtrinsicIndex = "Unknown", AccountDisplay = new AccountDisplay { Address = address } };
        }

        public static async Task InitAsync()
        {
            if (File.Exists(AddressFile))
            {
                Extrinsics.Clear();
                var addresses = await File.ReadAllLinesAsync(AddressFile);
                foreach (var address in addresses)
                {
                    Extrinsics.Add(new Extrinsic
                        { ExtrinsicIndex = "Unknown", AccountDisplay = new AccountDisplay { Address = address } });
                }

                for (var index = 0; index < Extrinsics.Count; index++)
                {
                    var extrinsic = Extrinsics[index];
                    var result = await GetSWorkReportAsync(extrinsic.AccountDisplay.Address);
                    Extrinsics[index] = result;
                    await Task.Delay(500);
                }
            }
        }

        public static async Task InitEmailsAsync()
        {
            if (File.Exists(MailFile))
            {
                Emails = await File.ReadAllTextAsync(MailFile);
            }
        }

        public static async Task AddAddressAsync(string address)
        {
            try
            {
                if (Extrinsics.All(_ => _.AccountDisplay.Address != address))
                {
                    var report = await GetSWorkReportAsync(address);
                    Extrinsics.Add(report);
                    await File.WriteAllLinesAsync(AddressFile, Extrinsics.Select(_ => _.AccountDisplay.Address));
                }
            }
            catch
            {
                // ignored
            }
        }

        public static async Task SetEmailsAsync(string email)
        {
            try
            {
                Emails = email;
                await File.WriteAllTextAsync(MailFile, email);
            }
            catch
            {
                // ignored
            }
        }


        public static async Task DeleteReportAsync(Extrinsic extrinsic)
        {
            if (Extrinsics.Contains(extrinsic))
            {
                Extrinsics.Remove(extrinsic);
                await File.WriteAllLinesAsync(AddressFile, Extrinsics.Select(_ => _.AccountDisplay.Address));
            }
        }
    }
}