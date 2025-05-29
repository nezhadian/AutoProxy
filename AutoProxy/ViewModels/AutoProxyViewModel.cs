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
            }
        }

        private string _gateway;
        public string Gateway
        {
            get { return _gateway; }
            set
            {
                SetProperty(ref _gateway, value);
                OnGatewayChanged();
            }
        }

        private void OnGatewayChanged()
        {
            SystemProxyHelper.SetSystemProxy(Gateway, Port);
        }

        public AutoSearchViewModel auto { get; set; }
        public ManualSearchViewModel manual { get; set; }

        public AutoProxyViewModel()
        {
            auto   = new AutoSearchViewModel(this);
            manual = new ManualSearchViewModel(this);

            Gateway = TryGetLastGateway();
        }

        private string TryGetLastGateway()
        {
            var systemProxyString = SystemProxyHelper.GetSystemProxy();
            
            var doubleCoutIndex = systemProxyString.IndexOf(':');

            if (doubleCoutIndex == -1)
                return string.Empty;
            
            return systemProxyString.Substring(0, doubleCoutIndex);

        }
    }
}
