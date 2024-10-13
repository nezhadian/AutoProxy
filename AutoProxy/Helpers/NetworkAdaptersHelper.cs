using System;
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
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface networkInterface in networkInterfaces)
            {
                if (token.IsCancellationRequested)
                    return null;

                // Display the network adapter name and description
                Console.WriteLine($"Testing adapter: {networkInterface.Name}");

                // Get the IP properties for the network interface
                IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();

                // Display the gateways and test the specified port
                foreach (GatewayIPAddressInformation gateway in ipProperties.GatewayAddresses)
                {
                    if (token.IsCancellationRequested)
                        return null;

                    Console.WriteLine($"Testing gateway: {gateway.Address}");

                    bool isPortOpen = SystemProxyHelper.IsPortOpen(gateway.Address.ToString(), port);
                    Console.WriteLine($"Port {port} is {(isPortOpen ? "open" : "closed")} on gateway {gateway.Address}");

                    // If the port is open, set the system proxy
                    if (isPortOpen)
                    {
                        return gateway.Address;
                    }
                }

                Console.WriteLine(); // Add a blank line for better readability
            }

            return null;
        }
    }


}
