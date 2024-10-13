using AutoProxy.Helpers;
using ExtendedControls.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AutoProxy.ViewModels
{
    public class ManualSearchViewModel : BindableBase
    {
        

        public Command ConnectCommand { get; set; }
        public Command DisconnectCommand { get; set; }


        public readonly AutoProxyViewModel autoProxyViewModel;
        public ManualSearchViewModel(AutoProxyViewModel autoProxyViewModel)
        {
            this.autoProxyViewModel = autoProxyViewModel;
            autoProxyViewModel.auto.PropertyChanged += OnAutoModeChanged;
            ConnectCommand = new Command(Connect,CanUseManual);
            DisconnectCommand = new Command(Disconnect, CanUseManual);
        }

        private void OnAutoModeChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ConnectCommand.ChangeCanExecute();
            DisconnectCommand.ChangeCanExecute();   
        }

        private bool CanUseManual()
        {
            return !autoProxyViewModel.auto.AutoMode;
        }

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private void Disconnect()
        {
            cancellationTokenSource?.Cancel();
            SetStatus("Disconnected.");
            SystemProxyHelper.ClearSystemProxy();

        }
        private async void Connect()
        {
            
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();
            SetStatus("Searching...");
            await Task.Run(DoSearch);
        }
        private void DoSearch()
        {
            var gateway = NetworkAdaptersHelper.GetFirstOpenGateway(autoProxyViewModel.Port,cancellationTokenSource.Token);
            if (gateway is null) 
            {
                SetStatus("Not finded.");
                return;
            }

            autoProxyViewModel.Gateway = gateway.ToString();
            SystemProxyHelper.SetSystemProxy(autoProxyViewModel.Gateway, autoProxyViewModel.Port);
            SetStatus($"Connected.");

        }

        private void SetStatus(string v)
        {
            App.Current.Dispatcher.Invoke(() => autoProxyViewModel.Status = v);
            Console.WriteLine($"[Manual] {v}");

        }
    }
}
