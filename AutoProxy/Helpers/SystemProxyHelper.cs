using Microsoft.Win32;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace AutoProxy.Helpers
{
    public static class SystemProxyHelper
    {

        public static void ClearSystemProxy()
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
        public static void SetSystemProxy(string ipAddress, int port)
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
        public static bool IsPortOpen(string ipAddress, int port)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    // Set a timeout for the connection attempt
                    client.Connect(ipAddress, port);
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



    }
}