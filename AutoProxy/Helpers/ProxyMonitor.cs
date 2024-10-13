using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoProxy.Helpers
{
    public class ProxyMonitor : SystemProxyHelper
    {
        private readonly string _gatewayIpAddress;
        private readonly int _port;
        private readonly int _delay;
        private readonly CancellationTokenSource _cancellationTokenSource;

        // Event to notify when the proxy is disabled
        public event Action ProxyDisabled;

        public ProxyMonitor(string gatewayIpAddress, int port, int delay)
        {
            _gatewayIpAddress = gatewayIpAddress;
            _port = port;
            _delay = delay;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task StartMonitoringAsync()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                bool isPortOpen = await IsPortOpen(_gatewayIpAddress, _port);
                Console.WriteLine($"Checking gateway {_gatewayIpAddress} on port {_port}: {(isPortOpen ? "open" : "closed")}");

                if (!isPortOpen)
                {
                    ClearSystemProxy();
                    Console.WriteLine("Proxy disabled due to closed port.");
                    OnProxyDisabled(); // Invoke the event when the proxy is disabled
                }

                // Wait for 15 seconds before the next check
                await Task.Delay(TimeSpan.FromSeconds(_delay), _cancellationTokenSource.Token);
            }
        }
        protected virtual void OnProxyDisabled()
        {
            ProxyDisabled?.Invoke(); // Invoke the event
        }

        public void StopMonitoring()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
