using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using LiveCharts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using WpfAppIndustryProtocolTestTool.BLL.OpcProtocol;
using WpfAppIndustryProtocolTestTool.Model;

namespace WpfAppIndustryProtocolTestTool.ViewModel
{
    public class OpcClientViewModel : ViewModelBase
    {
        #region fields

        Timer _dataRefreshTimer;
        string _selectedItemID;

        OpcDataAccessAutomationHelper _opcCalssicDAHelper;


        #endregion

        public string ServerState { get => _opcCalssicDAHelper.GetServerState(); }
        public string ServerName { get => _opcCalssicDAHelper.GetServerName(); }

        #region UI -> Source

        public string GroupName { get; set; }
        public float DefaultGroupDeadband { get; set; }
        public int UpdateRate { get; set; }
        public bool IsActive { get; set; }
        public bool IsSubscribed { get; set; }

        public bool SyncRead { get; set; }
        public bool SyncWrite { get; set; }
        public bool AsyncRead { get; set; }
        public bool AsyncWrite { get; set; }

        public string SelectedItemID01 { get; set; }
        public string SelectedItemID02 { get; set; }
        public ushort Chart01PointsSum { get; set; }
        public ushort Chart02PointsSum { get; set; }

        #endregion

        #region Source -> UI

        private string _connectOrDisconnect;
        public string ConnectOrDisconnect
        {
            get { return _connectOrDisconnect; }
            set
            {
                _connectOrDisconnect = value;
                this.RaisePropertyChanged();
            }
        }


        private string _selectedHost;
        public string SelectedHost
        {
            get { return _selectedHost; }
            set
            {
                _selectedHost = value;
                this.RaisePropertyChanged();
            }
        }


        private string _selectedServer;
        public string SelectedServer
        {
            get { return _selectedServer; }
            set
            {
                _selectedServer = value;
                this.RaisePropertyChanged();
            }
        }





        private string _startOrStop;
        public string StartOrStop
        {
            get { return _startOrStop; }
            set
            {
                _startOrStop = value;
                this.RaisePropertyChanged();

            }
        }


        private bool _isConnected;
        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                if (_isConnected == value) { return; }
                _isConnected = value;
                RaisePropertyChanged();
                Messenger.Default.Send<bool>(_isConnected, "OpcClient");
            }
        }


        private bool _isReadingOrWriting;
        public bool IsReadingOrWriting
        {
            get { return _isReadingOrWriting; }
            set
            {
                if (_isReadingOrWriting == value) { return; }
                _isReadingOrWriting = value;
                RaisePropertyChanged();
            }
        }


        private bool _exchangeEnable;
        public bool ExchangeEnable
        {
            get { return _exchangeEnable; }
            set
            {
                _exchangeEnable = value;
                this.RaisePropertyChanged();

            }
        }


        private string _infoMessage;
        public string InfoMessage
        {
            get { return _infoMessage; }
            set
            {
                if (_infoMessage == value) { return; }
                _infoMessage = value;
                RaisePropertyChanged();
            }
        }


        #endregion

        #region ObservableCollection

        public ObservableCollection<string> HostCollection { get; set; }

        public ObservableCollection<string> ServerCollection { get; set; }

        public ObservableCollection<OpcTagTreeNodeModel> ServerTagTree { get; set; }

        public ObservableCollection<OpcTagItemModel> SelectedTagCollection { get; set; }

        public ObservableCollection<string> ChartItemIDCollection { get; set; }

        #endregion

        #region Command


        private RelayCommand _cmdRefresh;
        public RelayCommand CmdRefresh
        {
            get
            {
                if (_cmdRefresh == null)
                {
                    _cmdRefresh = new RelayCommand(() => ExeRefresh(), () => ServerState != "Connected");
                }
                return _cmdRefresh;
            }
        }


        private RelayCommand _cmdChangeHost;
        public RelayCommand CmdChangeHost
        {
            get
            {
                if (_cmdChangeHost == null)
                {
                    _cmdChangeHost = new RelayCommand(() => ExeChangeHost());
                }
                return _cmdChangeHost;
            }
        }


        private RelayCommand _cmdChangeServer;
        public RelayCommand CmdChangeServer
        {
            get
            {
                if (_cmdChangeServer == null)
                {
                    _cmdChangeServer = new RelayCommand(() => ExeChangeServer(), () => !string.IsNullOrEmpty(SelectedHost) && ServerState != "Connected");
                }
                return _cmdChangeServer;
            }
        }



        private RelayCommand _cmdConnect;
        public RelayCommand CmdConnect
        {
            get
            {
                if (_cmdConnect == null)
                {
                    _cmdConnect = new RelayCommand(() => ExeConnectOrDisconnect(), () => CanConnectOrDisconnect());
                }
                return _cmdConnect;
            }
        }



        private RelayCommand<OpcTagTreeNodeModel> _cmdselectTag;
        public RelayCommand<OpcTagTreeNodeModel> CmdSelectTag
        {
            get
            {
                if (_cmdselectTag == null)
                {
                    _cmdselectTag = new RelayCommand<OpcTagTreeNodeModel>((node) => ExeSelectTag(node));
                }
                return _cmdselectTag;
            }
        }


        private RelayCommand _cmdAddItem;
        public RelayCommand CmdAddItem
        {
            get
            {
                if (_cmdAddItem == null)
                {
                    _cmdAddItem = new RelayCommand(() => ExeAddItemIntoGroup(), () => !string.IsNullOrEmpty(_selectedItemID));
                }
                return _cmdAddItem;
            }
        }



        private RelayCommand _cmdStartRW;
        public RelayCommand CmdStartRW
        {
            get
            {
                if (_cmdStartRW == null)
                {
                    _cmdStartRW = new RelayCommand(() => ExeStartRW(), () => SelectedTagCollection.Count > 0 && ServerState == "Connected");
                }
                return _cmdStartRW;
            }
        }


        private RelayCommand _cmdRemoveSelectedTag;
        public RelayCommand CmdRemoveSelectedTag
        {
            get
            {
                if (_cmdRemoveSelectedTag == null)
                {
                    _cmdRemoveSelectedTag = new RelayCommand(() => ExeRemoveSelectedTag(), () => CanRemoveSelectedTag());
                }
                return _cmdRemoveSelectedTag;
            }
        }


        private RelayCommand _cmdAddIntoCharts;
        public RelayCommand CmdAddIntoCharts
        {
            get
            {
                if (_cmdAddIntoCharts == null)
                {
                    _cmdAddIntoCharts = new RelayCommand(() => ExeAddIntoCharts(), () => CanAddIntoCharts());
                }
                return _cmdAddIntoCharts;
            }
        }



        private RelayCommand _cmdRemoveFromCharts;
        public RelayCommand CmdRemoveFromCharts
        {
            get
            {
                if (_cmdRemoveFromCharts == null)
                {
                    _cmdRemoveFromCharts = new RelayCommand(() => ExeRemoveFromCharts(), () => CanRemoveFromCharts());
                }
                return _cmdRemoveFromCharts;
            }
        }



        private RelayCommand _cmdClearInfoMessage;
        public RelayCommand CmdClearInfoMessage
        {
            get
            {
                if (_cmdClearInfoMessage == null)
                {
                    _cmdClearInfoMessage = new RelayCommand(() => InfoMessage = string.Empty);
                }
                return _cmdClearInfoMessage;
            }
        }


        private RelayCommand<string> _cmdSelectionChanged;
        public RelayCommand<string> CmdSelectionChanged
        {
            get
            {
                if (_cmdSelectionChanged == null)
                {
                    _cmdSelectionChanged = new RelayCommand<string>((chartNo) => ExeClearChartPoints(chartNo));
                }
                return _cmdSelectionChanged;
            }
        }



        #endregion

        #region LiveCharts

        public ChartValues<float> Chart01Values { get; set; }
        public ObservableCollection<string> Chart01XLabels { get; set; }
        public ChartValues<float> Chart02Values { get; set; }
        public ObservableCollection<string> Chart02XLabels { get; set; }

        #endregion


        public OpcClientViewModel()
        {
            DefaultGroupDeadband = 1;
            UpdateRate = 1000;
            IsActive = true;
            IsSubscribed = true;
            ExchangeEnable = true;
            SyncRead = true;

            Chart01PointsSum = 10;
            Chart02PointsSum = 10;

            HostCollection = new ObservableCollection<string>();
            ServerCollection = new ObservableCollection<string>();
            ServerTagTree = new ObservableCollection<OpcTagTreeNodeModel>();
            SelectedTagCollection = new ObservableCollection<OpcTagItemModel>();
            ChartItemIDCollection = new ObservableCollection<string>();

            Chart01Values = new ChartValues<float>();
            Chart02Values = new ChartValues<float>();
            Chart01XLabels = new ObservableCollection<string>();
            Chart02XLabels = new ObservableCollection<string>();

            _opcCalssicDAHelper = new OpcDataAccessAutomationHelper();
            _opcCalssicDAHelper.ConnectionChanged += _opcCalssicDAHelper_ConnectionChanged;

            Messenger.Default.Register<string>(this, "Close", (msg) =>
            {
                if (msg == "CloseConnection")
                {
                    if (IsConnected)
                    {
                        ExeConnectOrDisconnect();
                    }
                }
            });

            ExeRefresh();
        }

        public override void Cleanup()
        {
            base.Cleanup();
        }



        #region Command Methods


        private void ExeRefresh()
        {
            try
            {
                ConnectOrDisconnect = "Connect";
                StartOrStop = "Start";
                ExchangeEnable = true;

                HostCollection.Clear();
                ServerTagTree.Clear();
                SelectedTagCollection.Clear();
                ChartItemIDCollection.Clear();

                _opcCalssicDAHelper.GetHostNode(HostCollection);
                SelectedHost = HostCollection[0];

                _opcCalssicDAHelper.GetServerName(ServerCollection, SelectedHost);
                if (ServerCollection.Count > 1)
                {
                    SelectedServer = ServerCollection[0];
                }

            }
            catch (Exception ex)
            {
                InfoMessage = "Error: " + ex.Message.Replace("\n", "");
            }

        }

        private void ExeChangeHost()
        {
            try
            {
                ServerCollection.Clear();
                ServerTagTree.Clear();
                SelectedTagCollection.Clear();
                ChartItemIDCollection.Clear();
                _opcCalssicDAHelper.GetServerName(ServerCollection, SelectedHost);

            }
            catch (Exception ex)
            {
                InfoMessage = "Error: " + ex.Message.Replace("\n", "");
            }

        }

        private void ExeChangeServer()
        {
            ServerTagTree.Clear();
            SelectedTagCollection.Clear();
            ChartItemIDCollection.Clear();

        }

        private void ExeConnectOrDisconnect()
        {
            try
            {
                if (ConnectOrDisconnect == "Connect")
                {

                    OpcGroupPropertyModel opcGroupProperty = new OpcGroupPropertyModel
                    {
                        GroupName = GroupName,
                        DefaultGroupDeadband = DefaultGroupDeadband,
                        UpdateRate = UpdateRate,
                        IsActive = IsActive,
                        IsSubscribed = IsSubscribed
                    };


                    _opcCalssicDAHelper.ConnectServer(SelectedServer, ServerTagTree, opcGroupProperty);
                    _opcCalssicDAHelper.ItemValueChanged += _opcCalssicDAHelper_ItemValueChanged;

                    _dataRefreshTimer = new Timer(UpdateRate);
                    _dataRefreshTimer.Elapsed += _dataRefreshTimer_Elapsed;

                    ConnectOrDisconnect = "Disconnect";

                    InfoMessage = $"Warning: [Host Node]:  {SelectedHost} , [Server Name]:  {ServerName}";

                }
                else if (ConnectOrDisconnect == "Disconnect")
                {


                    _dataRefreshTimer.Enabled = false;

                    _opcCalssicDAHelper.DisconnectServer();

                    ServerTagTree.Clear();
                    SelectedTagCollection.Clear();
                    ChartItemIDCollection.Clear();

                    ConnectOrDisconnect = "Connect";
                    StartOrStop = "Start";

                }
            }
            catch (Exception ex)
            {
                InfoMessage = "Error: " + ex.Message.Replace("\n", "");
            }

        }

        private bool CanConnectOrDisconnect()
        {
            if (ConnectOrDisconnect == "Connect")
            {
                return !string.IsNullOrWhiteSpace(GroupName) && !string.IsNullOrEmpty(GroupName) && UpdateRate != 0;
            }
            else if (ConnectOrDisconnect == "Disconnect")
            {
                return !IsReadingOrWriting;
            }
            return true;
        }

        private void ExeSelectTag(OpcTagTreeNodeModel node)
        {
            if (node.Children.Count == 0)
            {
                _selectedItemID = node.ItemID;
                InfoMessage = $"Warning: You have Selected Tag [ {_selectedItemID} ]";
            }
        }

        private void ExeAddItemIntoGroup()
        {
            try
            {
                _opcCalssicDAHelper.AddOpcTagItem(_selectedItemID);
                foreach (var item in _opcCalssicDAHelper.OpcTagItemList)
                {
                    if (!SelectedTagCollection.ToList().Exists(i => i == item))
                    {
                        SelectedTagCollection.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {

                InfoMessage = "Error: " + ex.Message.Replace("\n", "");
            }

        }

        private void ExeStartRW()
        {
            try
            {
                if (StartOrStop == "Start")
                {
                    IsReadingOrWriting = true;

                    if (SyncRead)
                    {
                        _opcCalssicDAHelper.ReadItemValueSync();
                    }
                    else if (SyncWrite)
                    {
                        _opcCalssicDAHelper.WriteItemValueSync();
                    }
                    else if (AsyncRead || AsyncWrite)
                    {
                        _dataRefreshTimer.Enabled = true;
                        ExchangeEnable = false;
                        StartOrStop = "Stop";
                    }
                }
                else if (StartOrStop == "Stop")
                {
                    _dataRefreshTimer.Enabled = false;
                    ExchangeEnable = true;
                    IsReadingOrWriting = false;
                    StartOrStop = "Start";
                }
            }
            catch (Exception ex)
            {
                IsReadingOrWriting = false;

                InfoMessage = "Error: " + ex.Message.Replace("\n", "");
            }
        }



        private void ExeRemoveSelectedTag()
        {
            try
            {
                List<OpcTagItemModel> removeList = SelectedTagCollection.ToList().FindAll(item => item.IsSelected);
                int count = removeList.Count;
                _opcCalssicDAHelper.RemoveOpcTagItem(count, removeList);

                foreach (var item in removeList)
                {
                    _ = SelectedTagCollection.Remove(item);
                    ChartItemIDCollection.ToList().FindAll(i => i == item.ItemID).ForEach(i => ChartItemIDCollection.Remove(i));
                }
                InfoMessage = $"Warning: Remove {count} item(s) from DataGrid!";
            }
            catch (Exception ex)
            {
                InfoMessage = "Error: " + ex.Message.Replace("\n", "");
            }
        }

        private bool CanRemoveSelectedTag()
        {
            return SelectedTagCollection.Count(item => item.IsSelected) > 0 && !IsReadingOrWriting;
        }


        private void ExeAddIntoCharts()
        {
            try
            {
                var selectedTagList = SelectedTagCollection.ToList().FindAll(item => item.IsSelected);

                foreach (var item in selectedTagList)
                {
                    if (!ChartItemIDCollection.ToList().Exists(i => i == item.ItemID))
                    {
                        ChartItemIDCollection.Add(item.ItemID);
                    }
                }
            }
            catch (Exception ex)
            {
                InfoMessage = "Error: " + ex.Message.Replace("\n", "");
            }

        }

        private bool CanAddIntoCharts()
        {
            return SelectedTagCollection.Count(item => item.IsSelected) > 0;
        }

        private void ExeRemoveFromCharts()
        {
            try
            {
                SelectedTagCollection.ToList().FindAll(item => item.IsSelected).
                                               ForEach(item => ChartItemIDCollection.Remove(item.ItemID));
            }
            catch (Exception ex)
            {
                InfoMessage = "Error: " + ex.Message.Replace("\n", "");
            }

        }

        private bool CanRemoveFromCharts()
        {
            return ChartItemIDCollection.Count > 0 && SelectedTagCollection.Count(item => item.IsSelected) > 0;
        }

        private void ExeClearChartPoints(string chartNo)
        {
            if (chartNo == "chart01")
            {
                Chart01Values.Clear();
                Chart01XLabels.Clear();
            }
            else if (chartNo == "chart02")
            {
                Chart02Values.Clear();
                Chart02XLabels.Clear();
            }
        }
        #endregion


        #region EventHandler

        private void _dataRefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {

                if (!ExchangeEnable && AsyncRead)
                {
                    _opcCalssicDAHelper.ReadItemValueAsync();
                }
                else if (!ExchangeEnable && AsyncWrite)
                {
                    _opcCalssicDAHelper.WriteItemValueSync();
                }

            }
            catch (Exception ex)
            {
                InfoMessage = "Error: " + ex.Message.Replace("\n", "");
            }

        }

        private void _opcCalssicDAHelper_ItemValueChanged(string message)
        {
            try
            {
                if (SyncRead || SyncWrite)
                {
                    IsReadingOrWriting = false;
                }

                Task.Run(() =>
                  {
                      ShowChartPoints(SelectedItemID01, Chart01PointsSum, Chart01Values, Chart01XLabels);
                      ShowChartPoints(SelectedItemID02, Chart02PointsSum, Chart02Values, Chart02XLabels);
                  });

                InfoMessage = $"{message}";
            }
            catch (Exception ex)
            {
                InfoMessage = "Error: " + ex.Message.Replace("\n", "");
            }


        }

        private void _opcCalssicDAHelper_ConnectionChanged(string serverName)
        {
            try
            {
                IsConnected = _opcCalssicDAHelper.IsConnected;
                InfoMessage = $"Warning: Disconnected to OPC Server [{serverName}] !";
            }
            catch (Exception ex)
            {
                InfoMessage = "Error: " + ex.Message.Replace("\n", "");
            }

        }

        #endregion

        private void ShowChartPoints(string selectedItemID, ushort pointsSum, ChartValues<float> values, Collection<string> xlabels)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(selectedItemID) && ChartItemIDCollection.Count > 0)
                {
                    values.Add(SelectedTagCollection.ToList().First(i => i.ItemID == selectedItemID).ItemValue);
                    xlabels.Add(SelectedTagCollection.ToList().First(i => i.ItemID == selectedItemID).TimeStamp.Substring(11));

                    if (values.Count > pointsSum)
                    {
                        values.RemoveAt(0);
                        xlabels.RemoveAt(0);
                    }

                }
            }
            catch (Exception ex)
            {
                InfoMessage = "Error: " + ex.Message.Replace("\n", "");
            }

        }



    }
}
