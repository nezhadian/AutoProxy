using AutoProxy.Helpers;
using ExtendedControls.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoProxy.ViewModels
{
    public class AutoSearchViewModel : BindableBase
    {
        private bool _autoSwitching = false;
        public bool AutoSwitching
        {
            get { return _autoSwitching; }
            set
            {
                SetProperty(ref _autoSwitching, value);
                OnAutoSwitchingChanged();
            }
        }

        private int _delay = 15;
        public int Delay
        {
            get { return _delay; }
            set
            {
                SetProperty(ref _delay, value);
                OnAutoSwitchingChanged();
            }
        }


        public readonly AutoProxyViewModel autoProxyViewModel;
        public AutoSearchViewModel(AutoProxyViewModel autoProxyViewModel)
        {
            this.autoProxyViewModel = autoProxyViewModel;
            autoProxyViewModel.PropertyChanged += OnPropertiesChanged;
        }

        private void OnPropertiesChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(autoProxyViewModel.Gateway):
                case nameof(autoProxyViewModel.Port):
                    SystemProxyHelper.SetSystemProxy(autoProxyViewModel.Gateway, autoProxyViewModel.Port);
                    OnAutoSwitchingChanged();
                    break;
            }
        }

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();


        private void OnAutoSwitchingChanged()
        {
            if (!AutoSwitching) 
            {
                cancellationTokenSource?.Cancel();
                return;
            }

            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();
            Task.Run(DoSearching);
        }

        private async Task DoSearching()
        {
            var token = cancellationTokenSource.Token;
            while (!token.IsCancellationRequested)
            {
                SetStatus($"Checking {autoProxyViewModel.Gateway}{autoProxyViewModel.Port}");
                var isOpen = SystemProxyHelper.IsPortOpen(autoProxyViewModel.Gateway, autoProxyViewModel.Port);

                if (isOpen)
                {
                    SetStatus("Still Connected.");
                }
                else
                {
                    SystemProxyHelper.ClearSystemProxy();
                    SetStatus("Disconnected. Searching...");
                    var gateway = NetworkAdaptersHelper.GetFirstOpenGateway(autoProxyViewModel.Port, token);
                    if(gateway is null)
                    {
                        SetStatus("Failed to find.");
                    }
                    else
                    {
                        autoProxyViewModel.Gateway = gateway.ToString();
                        SetStatus($"Reconnected to {autoProxyViewModel.Gateway}:{autoProxyViewModel.Port}");
                        SystemProxyHelper.SetSystemProxy(autoProxyViewModel.Gateway, autoProxyViewModel.Port);

                    }
                }
                await Task.Delay(TimeSpan.FromSeconds(Delay),token);
            }
        }

        private void SetStatus(string v)
        {
            App.Current.Dispatcher.Invoke(() => autoProxyViewModel.Status = v);
            Console.WriteLine($"[Auto] {v}");
        }
    }
}
