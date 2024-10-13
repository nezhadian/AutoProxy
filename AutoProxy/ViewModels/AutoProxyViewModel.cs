using AutoProxy.Helpers;
using ExtendedControls.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AutoProxy.ViewModels
{
    public class AutoProxyViewModel : BindableBase
    {
		private bool? _isConnected = false;
		public bool? IsConnected
		{
			get { return _isConnected; }
			set { 
				SetProperty(ref _isConnected, value);
			}
		}

        private string _status;
		public string Status
		{
			get { return _status; }
			set { SetProperty(ref _status, value); }
		}

        private int _port = 10809;
        public int Port
        {
            get { return _port; }
            set { 
                SetProperty(ref _port, value);
                ResetProxyMonitor();
            }
        }

        private string _gateway;
        public string Gateway
        {
            get { return _gateway; }
            set { 
                SetProperty(ref _gateway, value);
                ResetProxyMonitor();
            }
        }

        private bool _autoSwitching = false;
        public bool AutoSwitching
        {
            get { return _autoSwitching; }
            set
            {
                SetProperty(ref _autoSwitching, value);
                ResetProxyMonitor();
            }
        }

        private int _delay = 15;
        public int Delay
        {
            get { return _delay; }
            set { SetProperty(ref _delay, value);
                ResetProxyMonitor();
            }
        }

        Task proxyFinderTask;
		NetworkAdaptersHelper networkAdaptersHelper;

        public ICommand ConnectCommand { get; set; }
        public ICommand DisconnectCommand { get; set; }


        public AutoProxyViewModel()
        {
            networkAdaptersHelper = new NetworkAdaptersHelper();
            networkAdaptersHelper.StatusChanged += NetworkAdaptersHelper_StatusChanged;
            networkAdaptersHelper.OnConnected += NetworkAdaptersHelper_OnConnected;
            networkAdaptersHelper.OnFailed += NetworkAdaptersHelper_OnFailed;

            ConnectCommand = new Command(Connect);
            DisconnectCommand = new Command(Disconnect);
        }

        
        private void Disconnect()
        {
            networkAdaptersHelper.ClearSystemProxy();
            Status = $"Disconnected";
            IsConnected = false;

        }
        private async void Connect()
        {
            if (!(proxyFinderTask is null) && proxyFinderTask.Status == TaskStatus.Running)
            {
                return;
            }

            proxyFinderTask = networkAdaptersHelper.TestGatewaysPortAndSetProxyAsync(Port, App.Current.Dispatcher);
            await proxyFinderTask;
        }

        private void NetworkAdaptersHelper_OnConnected(string obj)
        {
            App.Current.Dispatcher.Invoke(() => {
                Status = $"Connected to {obj}";
                Gateway = obj;
                IsConnected = true;

            });

        }
        private void NetworkAdaptersHelper_OnFailed()
        {
            App.Current.Dispatcher.Invoke(() => {
                Status = $"Failed to Connect";
                IsConnected = false;
            });
        }
        private void NetworkAdaptersHelper_StatusChanged(string obj)
        {
            App.Current.Dispatcher.Invoke(() => Status = obj);
        }



        ProxyMonitor proxyMonitor;

        private async Task ResetProxyMonitorAsync()
        {
            if (!(proxyMonitor is null))
            {
                proxyMonitor.StopMonitoring();
            }

            proxyMonitor = new ProxyMonitor(Gateway, Port, Delay);
            proxyMonitor.ProxyDisabled += NetworkAdaptersHelper_OnFailed;
            await proxyMonitor.StartMonitoringAsync();
        }
        public void ResetProxyMonitor()
        {
            if(!AutoSwitching)
            {
                proxyMonitor?.StopMonitoring();
                proxyMonitor = null;
                return;
            }

            Task.Run(ResetProxyMonitorAsync);
        }
    }
}
