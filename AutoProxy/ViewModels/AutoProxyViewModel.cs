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
                OnIsConnectedChanged(value);
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
            set { SetProperty(ref _port, value); }
        }

        private string _gateway;
        public string Gateway
        {
            get { return _gateway; }
            set { SetProperty(ref _gateway, value); }
        }


        Task proxyFinderTask;
		NetworkAdaptersHelper networkAdaptersHelper;

        public AutoProxyViewModel()
        {
            networkAdaptersHelper = new NetworkAdaptersHelper();
            networkAdaptersHelper.StatusChanged += NetworkAdaptersHelper_StatusChanged;
            networkAdaptersHelper.OnConnected += NetworkAdaptersHelper_OnConnected;
            networkAdaptersHelper.OnFailed += NetworkAdaptersHelper_OnFailed;
        }

        

        private void OnIsConnectedChanged(bool? value)
        {
            if (value == true)
            {
                Connect();
                return;
            }
            
            if( value == false)
            {
                Disconnect();
                return;
            }

            
        }
        private void Disconnect()
        {
            networkAdaptersHelper.ClearSystemProxy();
            Status = $"Disconnected";

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
            });

        }

        private void NetworkAdaptersHelper_OnFailed()
        {
            App.Current.Dispatcher.Invoke(() => {
                Status = $"Failed to Connect";
            });
        }

        private void NetworkAdaptersHelper_StatusChanged(string obj)
        {
            App.Current.Dispatcher.Invoke(() => Status = obj);
        }
    }
}
