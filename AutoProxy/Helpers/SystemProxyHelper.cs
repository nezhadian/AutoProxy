using Microsoft.Win32;

namespace AutoProxy.Helpers
{
    public class SystemProxyHelper
    {
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
    }
}