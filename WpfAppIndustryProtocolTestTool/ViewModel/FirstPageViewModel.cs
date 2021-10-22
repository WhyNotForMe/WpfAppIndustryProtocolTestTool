using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace WpfAppIndustryProtocolTestTool.ViewModel
{
    public class FirstPageViewModel : ViewModelBase
    {

        bool _isRunning;
        #region UI ->Source

        public bool SingleModule { get; set; }
        public bool GatewayModule { get; set; }
        public int SelectedInputModule { get; set; }
        public int SelectedOutputModule { get; set; }



        #endregion


        #region Source -> UI

        public ObservableCollection<string> GatewayModuleCollection { get; set; }


        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive == value) { return; }
                _isActive = value;
                RaisePropertyChanged();
            }
        }


        private bool _resetMode;
        public bool ResetMode
        {
            get => _resetMode;
            set
            {
                if (_resetMode == value) { return; }
                _resetMode = value;
                RaisePropertyChanged();
            }
        }


        private string _startOrStop;
        public string StartOrStop
        {
            get => _startOrStop;
            set
            {
                if (_startOrStop == value) { return; }
                _startOrStop = value;
                RaisePropertyChanged();
            }
        }

       

        #endregion


        #region Command

        public ICommand CmdChangeMode { get => new RelayCommand(() => ResetMode = true); }

        public ICommand CmdStartStop { get => new RelayCommand(() => ExeStartStop(), () => CanStartStop()); }

        private bool CanStartStop()
        {
            if (SingleModule)
            {
                return true;
            }
            else if (GatewayModule)
            {
                return SelectedInputModule != SelectedOutputModule;
            }
            else
            {
                return false;
            }
        }

        private void ExeStartStop()
        {
            if (StartOrStop == "Start to Enjoy")
            {
                IsActive = false;
                ResetMode = false;

                if (GatewayModule)
                {
                    Messenger.Default.Send<string>("Gateway", "WorkMode");

                    switch (SelectedInputModule)
                    {
                        case 0:
                            Messenger.Default.Send<string>("Serial Port --> TCP/UDP", "GatewayMode");
                            break;
                        case 1:
                            Messenger.Default.Send<string>("TCP/UDP --> Serial Port", "GatewayMode");
                            break;

                        default:

                            break;
                    }
                    _isRunning = true;
                    StartOrStop = "Stop GatewayModule Mode";
                }
                else if (SingleModule)
                {
                    Messenger.Default.Send<string>("Single", "WorkMode");
                    StartOrStop = "Stop SingleModule Mode";
                }
            }
            else if (StartOrStop == "Stop SingleModule Mode" || StartOrStop == "Stop GatewayModule Mode")
            {
                Messenger.Default.Send<string>("CloseConnection", "Close");

                ResetMode = true;
                StartOrStop = "Start to Enjoy";
                _isRunning = false;
            }
            Messenger.Default.Send<bool>(_isRunning, "FirstPage");

        }

        #endregion


        public FirstPageViewModel()
        {
            SingleModule = true;
            IsActive = true;
            ResetMode = true;


            StartOrStop = "Start to Enjoy";

            GatewayModuleCollection = new ObservableCollection<string> { " Serial Port Module", " TCP/UDP Module" };


        }

        public override void Cleanup()
        {
            base.Cleanup();
        }
    }
}
