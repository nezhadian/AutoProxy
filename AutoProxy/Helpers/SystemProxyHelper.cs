using Microsoft.Win32;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace AutoProxy.Helpers
{
    public class SystemProxyHelper
    {

        private string _status;
        public string Status
        {
            get => _status;
             set
            {
                _status = value;
                OnStatusChanged(value); // Invoke the event when status changes
            }
        }
        public event Action<string> StatusChanged;

        public void ClearSystemProxy()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings", true))
            {
                if (key != null)
                {
                    key.SetValue("ProxyEnable", 0); // Enable the proxy
                    key.Flush(); // Save changes
                }
            }
        }

        public void SetSystemProxy(string ipAddress, int port)
        {
            // Set the proxy settings in the registry
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings", true))
            {
                if (key != null)
                {
                    key.SetValue("ProxyEnable", 1); // Enable the proxy
                    key.SetValue("ProxyServer", $"{ipAddress}:{port}"); // Set the proxy server
                    key.SetValue("ProxyOverride", "<local>"); // Bypass local addresses
                    key.Flush(); // Save changes
                }
            }
        }

        public async Task<bool> IsPortOpen(string ipAddress, int port)
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
                return false; // Handle other exceptions
            }
        }

        protected virtual void OnStatusChanged(string newStatus)
        {
            // Invoke the event on the UI thread if a dispatcher is provided
            StatusChanged?.Invoke(newStatus);
        }


    }
}