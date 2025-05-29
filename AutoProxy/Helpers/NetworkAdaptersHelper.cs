using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.Win32;
using System.Windows.Threading;
using System.Net;
using System.Threading;


namespace AutoProxy.Helpers
{
    public class NetworkAdaptersHelper
    {
        public static IPAddress GetFirstOpenGateway(int port,CancellationToken token)
        {
            // Get all network interfaces
            var ips = GetListOfAvailableIpAddress();

            foreach (var ip in ips)
            {
                if(token.IsCancellationRequested)
                    return null;
                
                var ipString = ip.ToString();
                
                Console.WriteLine($"Testing ip: {ipString} : {port}");

                bool isPortOpen = SystemProxyHelper.IsPortOpen(ipString, port);

                if (isPortOpen)
                {
                    return ip;
                }

                Console.WriteLine($"Port Closed : {ipString}:{port}");
            }

            return null;
        }
        
        public static async Task<IPAddress> GetFirstOpenGatewayAsync(int port, CancellationToken token)
        {
            // Get all network interfaces
            var gatewayIps = GetListOfAvailableIpAddress();

            if (gatewayIps.Count == 0)
                return null;

            // Create a linked cancellation token source
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(token))
            {
                var testTasks = new List<Task<IPAddress>>();

                foreach (var ip in gatewayIps)
                {
                    testTasks.Add(CheckIpLoop(ip, port, cts.Token));
                }

                var completedTask = await Task.WhenAny(testTasks);
                var result = await completedTask;

                if (result != null)
                {
                    // Found an open port - cancel all other checks
                    cts.Cancel();
                    return result;
                }
            }
            
            return null;
        }

        private static List<IPAddress> GetListOfAvailableIpAddress()
        {
            var networkInterfaces = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(i => i.OperationalStatus == OperationalStatus.Up);

            var gatewayIps = networkInterfaces
                .SelectMany(i => i.GetIPProperties().GatewayAddresses
                    .Select(j => j.Address.MapToIPv4()))
                .Distinct()
                .ToList();
            return gatewayIps;
        }

        private static async Task<IPAddress> CheckIpLoop(IPAddress ip, int port, CancellationToken ctsToken)
        {
            while (!ctsToken.IsCancellationRequested)
            {
                var isOpen = await SystemProxyHelper.IsPortOpenAsync(ip, port);

                if (isOpen)
                {
                    return ip;
                }
                
                Thread.Sleep(500);
            }
            
            return null;
        }
    }


}
