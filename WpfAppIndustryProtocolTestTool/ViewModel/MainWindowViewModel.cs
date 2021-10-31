using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.ObjectModel;
using WpfAppIndustryProtocolTestTool.Model;
using WpfAppIndustryProtocolTestTool.BLL;
using WpfAppIndustryProtocolTestTool.DAL;
using GalaSoft.MvvmLight.Messaging;

namespace WpfAppIndustryProtocolTestTool.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {

        #region Fields

        MainWindowNaviBarModel _firstPageBtn;
        MainWindowNaviBarModel _serialPortBtn;
        MainWindowNaviBarModel _TcpUdpBtn;
        MainWindowNaviBarModel _ModbusBtn;
        MainWindowNaviBarModel _OpcClientBtn;

        SqliteHelper _sqlitehelper;
        #endregion

        #region Source -> UI

        public ObservableCollection<MainWindowNaviBarModel> NaviBarCollection { get; set; }


        private string _time;
        public string Time
        {
            get { return _time; }
            set
            {
                if (_time == value) { return; }
                _time = value;
                RaisePropertyChanged();
            }
        }



        private Control _mainContent;
        public Control MainContent
        {
            get { return _mainContent; }
            set
            {
                if (_mainContent == value) { return; }
                _mainContent = value;
                RaisePropertyChanged();
            }
        }


        private string _workMode;
        public string WorkMode
        {
            get => _workMode;
            set
            {
                if (_workMode == value) { return; }
                _workMode = value;
                RaisePropertyChanged();
            }
        }


        private string _subModuleState;
        public string SubModuleState
        {
            get => _subModuleState;
            set
            {
                if (_subModuleState == value) { return; }
                _subModuleState = value;
                RaisePropertyChanged();
            }
        }



        private bool _isEnable;
        public bool IsEnable
        {
            get => _isEnable;
            set
            {
                if (_isEnable == value) { return; }
                _isEnable = value;
                RaisePropertyChanged();
            }
        }



        private string _gatewayMode;
        public string GatewayMode
        {
            get => _gatewayMode;
            set
            {
                if (_gatewayMode == value) { return; }
                _gatewayMode = value;
                RaisePropertyChanged();
            }
        }


        #endregion

        #region UI -> Source

        public bool NaviBarTransformed { get; set; }

        #endregion

        #region Command


        public ICommand CmdShowMainContent { get => new RelayCommand<string>((moduleName) => ExeShowMainContent(moduleName)); }

        public ICommand CmdNaviBarTransform { get => new RelayCommand(() => ExeTransformNaviBar()); }

        public ICommand CmdCloseWindow { get => new RelayCommand(() => ExeCloseWindow()); }



        #endregion

        public MainWindowViewModel()
        {
            ExeShowMainContent("FirstPage");

            InitNaviBar();

            InitStatusBar();

            Messenger.Default.Register<string>(this, "Close", (msg) =>
            {
                if (msg == "CloseConnection")
                {
                    if (!_serialPortBtn.LanVisibility)
                    {
                        GlobalViewManager.RemoveView("SerialPort");
                        ViewModelLocator.Cleanup<SerialPortViewModel>();
                    }
                    if (!_TcpUdpBtn.LanVisibility)
                    {
                        GlobalViewManager.RemoveView("TcpUdp");
                        ViewModelLocator.Cleanup<TcpUdpViewModel>();
                    }
                    if (!_ModbusBtn.LanVisibility)
                    {
                        GlobalViewManager.RemoveView("Modbus");
                        ViewModelLocator.Cleanup<ModbusViewModel>();
                    }
                    if (!_OpcClientBtn.LanVisibility)
                    {
                        GlobalViewManager.RemoveView("OpcClient");
                        ViewModelLocator.Cleanup<OpcClientViewModel>();
                    }
                    IsEnable = false;
                }
            });

            _sqlitehelper = SqliteHelper.GetSqliteHelpeInstance();
            _sqlitehelper.InitializeSqliteDB();

        }


        public override void Cleanup()
        {
            base.Cleanup();
        }

        #region Command Methods

        private void ExeShowMainContent(string moduleName)
        {
            //1 :Need to modify partial calss by singleInstanceMode
            //Type type = Type.GetType("WpfAppIndustryProtocolTestTool.View." + str + "View");
            //MainContent = Activator.CreateInstance(type) as Control;

            //2: HashTable Ioc
            MainContent = GlobalViewManager.GetView(moduleName) as UserControl;

            MainContent.Tag = moduleName;

        }
        private void ExeTransformNaviBar()
        {
            _firstPageBtn.HorVertical = NaviBarTransformed;
            _serialPortBtn.HorVertical = NaviBarTransformed;
            _TcpUdpBtn.HorVertical = NaviBarTransformed;
            _ModbusBtn.HorVertical = NaviBarTransformed;
            _OpcClientBtn.HorVertical = NaviBarTransformed;


            UpdateCollectionItem();

        }
        private void ExeCloseWindow()
        {
            Messenger.Default.Send<string>("CloseWindow", "Close");
        }

        #endregion

        private void UpdateCollectionItem()
        {
            NaviBarCollection.Clear();

            if (WorkMode == "Single Module Mode")
            {
                //_firstPageBtn
                NaviBarCollection.Add(_firstPageBtn);
                //_serialPortBtn
                if (_serialPortBtn.LanVisibility || NaviBarTransformed)
                {
                    NaviBarCollection.Add(_serialPortBtn);
                }
                //_TcpUdpBtn
                if (_TcpUdpBtn.LanVisibility || NaviBarTransformed)
                {
                    NaviBarCollection.Add(_TcpUdpBtn);
                }
                //_ModbusBtn
                if (_ModbusBtn.LanVisibility || NaviBarTransformed)
                {
                    NaviBarCollection.Add(_ModbusBtn);
                }
                //_OpcClientBtn
                if (_OpcClientBtn.LanVisibility || NaviBarTransformed)
                {
                    NaviBarCollection.Add(_OpcClientBtn);
                }
            }
            else if (WorkMode == "Gateway Module Mode")
            {
                //_firstPageBtn
                NaviBarCollection.Add(_firstPageBtn);
                //_serialPortBtn
                if (_serialPortBtn.LanVisibility || NaviBarTransformed)
                {
                    NaviBarCollection.Add(_serialPortBtn);
                }
                //_TcpUdpBtn
                if (_TcpUdpBtn.LanVisibility || NaviBarTransformed)
                {
                    NaviBarCollection.Add(_TcpUdpBtn);
                }

            }
        }

        #region EventHandler

        private void StatusTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Time = DateTime.Now.ToLocalTime().ToString();

        }

        #endregion

        #region Init Methods

        private void InitNaviBar()
        {
            NaviBarTransformed = true;

            _firstPageBtn = new MainWindowNaviBarModel { BtnName = "First Page", PackIconNum = "Numeric0BoxMultipleOutline", CmdParameter = "FirstPage", HorVertical = NaviBarTransformed };
            _serialPortBtn = new MainWindowNaviBarModel { BtnName = "Serial Port", PackIconNum = "Numeric1BoxMultipleOutline", CmdParameter = "SerialPort", HorVertical = NaviBarTransformed };
            _TcpUdpBtn = new MainWindowNaviBarModel { BtnName = "TCP / UDP", PackIconNum = "Numeric2BoxMultipleOutline", CmdParameter = "TcpUdp", HorVertical = NaviBarTransformed };
            _ModbusBtn = new MainWindowNaviBarModel { BtnName = "Modbus", PackIconNum = "Numeric3BoxMultipleOutline", CmdParameter = "Modbus", HorVertical = NaviBarTransformed };
            _OpcClientBtn = new MainWindowNaviBarModel { BtnName = "OPC Client", PackIconNum = "Numeric4BoxMultipleOutline", CmdParameter = "OpcClient", HorVertical = NaviBarTransformed };

            NaviBarCollection = new ObservableCollection<MainWindowNaviBarModel> { _firstPageBtn, _serialPortBtn, _TcpUdpBtn, _ModbusBtn, _OpcClientBtn };

            Messenger.Default.Register<bool>(_serialPortBtn, "SerialPort", (isRunning) =>
            {
                _serialPortBtn.LanVisibility = isRunning;
                SubModuleState = isRunning ? "Serial Port Module Run" : "Serial Port Module Stop";

                if (WorkMode == "Gateway Module Mode")
                {
                    Messenger.Default.Send<string>("Gateway", "WorkMode");
                    Messenger.Default.Send<string>(GatewayMode, "GatewayMode");
                }
            });
            Messenger.Default.Register<bool>(_TcpUdpBtn, "TcpUdp", (isRunning) =>
            {
                _TcpUdpBtn.LanVisibility = isRunning;
                SubModuleState = isRunning ? "TCP/UDP Module Run" : "TCP/UDP Module Stop";

                if (WorkMode == "Gateway Module Mode")
                {
                    Messenger.Default.Send<string>("Gateway", "WorkMode");
                    Messenger.Default.Send<string>(GatewayMode, "GatewayMode");
                }
            });
            Messenger.Default.Register<bool>(_ModbusBtn, "Modbus", (isRunning) =>
            {
                _ModbusBtn.LanVisibility = isRunning;
                SubModuleState = isRunning ? "Modbus Module Run" : "Modbus Module Stop";

            });
            Messenger.Default.Register<bool>(_OpcClientBtn, "OpcClient", (isRunning) =>
            {
                _OpcClientBtn.LanVisibility = isRunning;
                SubModuleState = isRunning ? "OPC Client Module Run" : "OPC Client Module Stop";
            });

            Messenger.Default.Register<bool>(_firstPageBtn, "FirstPage", (isRunning) =>
            {
                _firstPageBtn.LanVisibility = isRunning;
                SubModuleState = isRunning ? "Gateway Module Run" : "Gateway Module Stop";
                if (WorkMode == "Gateway Module Mode" && !isRunning)
                {
                    IsEnable = false;
                }
            });

        }


        private void InitStatusBar()
        {
            WorkMode = "Work Mode";
            SubModuleState = "SubModule State";

            Messenger.Default.Register<string>(this, "WorkMode", (workMode) =>
            {
                WorkMode = $"{workMode} Module Mode";
                IsEnable = true;

                App.Current.Dispatcher.Invoke(() => UpdateCollectionItem());
            });

            Messenger.Default.Register<string>(this, "GatewayMode", (msg) => GatewayMode = msg);

            var StatusTimer = new Timer { Interval = 100, Enabled = true };
            StatusTimer.Elapsed += StatusTimer_Elapsed;
        }


        #endregion
    }
}