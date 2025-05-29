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


        private readonly AutoProxyViewModel AutoProxyViewModel;
        public ManualSearchViewModel(AutoProxyViewModel autoProxyViewModel)
        {
            AutoProxyViewModel = autoProxyViewModel;
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
            return !AutoProxyViewModel.auto.AutoMode;
        }

        CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private void Disconnect()
        {
            _cancellationTokenSource?.Cancel();
            SetStatus("Disconnected.");
            SystemProxyHelper.ClearSystemProxy();

        }
        private void Connect()
        {
            
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            SetStatus("Searching...");
            Task.Run(DoSearch,_cancellationTokenSource.Token);
        }
        private async Task DoSearch()
        {
            
            var token = _cancellationTokenSource.Token;
            
            var gateway = await NetworkAdaptersHelper.GetFirstOpenGatewayAsync(AutoProxyViewModel.Port,_cancellationTokenSource.Token);
            if (gateway is null) 
            {
                SetStatus("Not Found.");
                return;
            }

            AutoProxyViewModel.Gateway = gateway.ToString();
            SetStatus($"Connected.");

        }

        private void SetStatus(string v)
        {
            App.Current.Dispatcher.Invoke(() => AutoProxyViewModel.Status = v);
            Console.WriteLine($"[Manual] {v}");

        }
    }
}
