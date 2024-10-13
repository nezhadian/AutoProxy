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
        private bool _autoMode = false;
        public bool AutoMode
        {
            get { return _autoMode; }
            set
            {
                SetProperty(ref _autoMode, value);
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
                    OnAutoSwitchingChanged();
                    break;
            }
        }

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();


        private void OnAutoSwitchingChanged()
        {
            if (!AutoMode) 
            {
                cancellationTokenSource?.Cancel();
                SetStatus("Disconnected.");
                SystemProxyHelper.ClearSystemProxy();
                return;
            }

            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();
            SystemProxyHelper.SetSystemProxy(autoProxyViewModel.Gateway, autoProxyViewModel.Port);
            Task.Run(DoSearching);
        }

        private async Task DoSearching()
        {
            var token = cancellationTokenSource.Token;
            while (!token.IsCancellationRequested)
            {
                SetStatus($"Checking...");
                var isOpen = SystemProxyHelper.IsPortOpen(autoProxyViewModel.Gateway, autoProxyViewModel.Port);

                if (isOpen)
                {
                    SetStatus("Connected.");
                    await Task.Delay(TimeSpan.FromSeconds(Delay), token);

                }
                else
                {
                    SetStatus("Disconnected. Searching...");
                    var gateway = NetworkAdaptersHelper.GetFirstOpenGateway(autoProxyViewModel.Port, token);
                    if(gateway is null)
                    {
                        SetStatus("Failed.");
                        SystemProxyHelper.ClearSystemProxy();
                        await Task.Delay(TimeSpan.FromSeconds(1), token);

                    }
                    else
                    {
                        autoProxyViewModel.Gateway = gateway.ToString();
                        SetStatus($"Reconnected.");
                        SystemProxyHelper.SetSystemProxy(autoProxyViewModel.Gateway, autoProxyViewModel.Port);
                        await Task.Delay(TimeSpan.FromSeconds(Delay), token);

                    }
                }
            }
        }

        private void SetStatus(string v)
        {
            App.Current.Dispatcher.Invoke(() => autoProxyViewModel.Status = v);
            Console.WriteLine($"[Auto] {v}");
        }
    }
}
