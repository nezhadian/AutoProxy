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
        public event Action<string> StatusChanged; // Event to notify status changes
        public event Action<string> OnConnected; // Event to notify status changes
        public event Action OnFailed; // Event to notify status changes

        private string _status;
        public string Status
        {
            get => _status;
            private set
            {
                _status = value;
                OnStatusChanged(value); // Invoke the event when status changes
            }
        }

        public async Task TestGatewaysPortAndSetProxyAsync(int port, Dispatcher dispatcher)
        {
            // Get all network interfaces
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface networkInterface in networkInterfaces)
            {
                // Display the network adapter name and description
                Status = $"Testing adapter: {networkInterface.Name}";
                Console.WriteLine(Status);

                // Get the IP properties for the network interface
                IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();

                // Display the gateways and test the specified port
                foreach (GatewayIPAddressInformation gateway in ipProperties.GatewayAddresses)
                {
                    Status = $"Testing gateway: {gateway.Address}";
                    Console.WriteLine(Status);

                    bool isPortOpen = await IsPortOpen(gateway.Address.ToString(), port);
                    Status = $"Port {port} is {(isPortOpen ? "open" : "closed")} on gateway {gateway.Address}";
                    Console.WriteLine(Status);

                    // If the port is open, set the system proxy
                    if (isPortOpen)
                    {
                        var gatewayString = gateway.Address.ToString();
                        SetSystemProxy(gatewayString, port);
                        Status = $"Proxy set to {gateway.Address}:{port}";
                        Console.WriteLine(Status);
                        OnConnectedToGateway(gatewayString);
                        return; // Exit after setting the proxy to the first open port
                    }
                }

                Console.WriteLine(); // Add a blank line for better readability
            }

            OnFailedToConnect();
        }
        private async Task<bool> IsPortOpen(string ipAddress, int port)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    // Set a timeout for the connection attempt
                    await client.ConnectAsync(ipAddress, port);
                    return client.Connected; // If connection is successful, return true
                }
            }
            catch (SocketException)
            {
                return false; // If there is a socket exception, the port is closed
            }
            catch (Exception ex)
            {
                Status = $"An error occurred: {ex.Message}";
                Console.WriteLine(Status);
                return false; // Handle other exceptions
            }
        }

        protected virtual void OnStatusChanged(string newStatus)
        {
            // Invoke the event on the UI thread if a dispatcher is provided
            StatusChanged?.Invoke(newStatus);
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
