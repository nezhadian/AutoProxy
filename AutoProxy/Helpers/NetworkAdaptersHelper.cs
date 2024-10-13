using System;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.Win32;
using System.Windows.Threading;
using System.Net;


namespace AutoProxy.Helpers
{
    public class NetworkAdaptersHelper : SystemProxyHelper
    {
        public event Action<string> OnConnected; // Event to notify status changes
        public event Action OnFailed; // Event to notify status changes

        public async Task TestGatewaysPortAndSetProxyAsync(int port, Dispatcher dispatcher)
        {
            // Get all network interfaces
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface networkInterface in networkInterfaces)
            {
                // Display the network adapter name and description
                Console.WriteLine($"Testing adapter: {networkInterface.Name}");

                // Get the IP properties for the network interface
                IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();

                // Display the gateways and test the specified port
                foreach (GatewayIPAddressInformation gateway in ipProperties.GatewayAddresses)
                {
                    Console.WriteLine($"Testing gateway: {gateway.Address}");

                    bool isPortOpen = await IsPortOpen(gateway.Address.ToString(), port);
                    Console.WriteLine($"Port {port} is {(isPortOpen ? "open" : "closed")} on gateway {gateway.Address}");

                    // If the port is open, set the system proxy
                    if (isPortOpen)
                    {
                        var gatewayString = gateway.Address.ToString();
                        SetSystemProxy(gatewayString, port);
                        Console.WriteLine($"Proxy set to {gateway.Address}:{port}");
                        OnConnectedToGateway(gatewayString);
                        return; // Exit after setting the proxy to the first open port
                    }
                }

                Console.WriteLine(); // Add a blank line for better readability
            }

            OnFailedToConnect();
        }

        protected virtual void OnConnectedToGateway(string gateway)
        {
            // Invoke the event on the UI thread if a dispatcher is provided
            OnConnected?.Invoke(gateway);
        }

        protected virtual void OnFailedToConnect()
        {
            // Invoke the event on the UI thread if a dispatcher is provided
            OnFailed?.Invoke();
        }
    }


}
