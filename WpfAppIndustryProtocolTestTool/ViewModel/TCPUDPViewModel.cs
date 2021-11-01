using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Windows.Input;
using WpfAppIndustryProtocolTestTool.BLL;
using WpfAppIndustryProtocolTestTool.BLL.TcpUdpProtocol;
using WpfAppIndustryProtocolTestTool.DAL;
using WpfAppIndustryProtocolTestTool.Model;
using WpfAppIndustryProtocolTestTool.Model.Enum;
using WpfAppIndustryProtocolTestTool.Model.SerializedMessage;

namespace WpfAppIndustryProtocolTestTool.ViewModel
{
    public class TcpUdpViewModel : ViewModelBase
    {

        #region Private fields

        TcpServerHelperIOCP _tcpServer;
        TcpClientHelperIOCP _tcpClient;
        UdpHelperIOCP _udpHelper;
        IPAddress _castIPAddress;
        TcpUdpWorkRoleEnum _workRole;

        bool _castConfirm;
        bool _textValid;
        System.Timers.Timer _sendTimer;
        ushort _codeCount;

        int _remoteRcvBufferSize;

        string _toolWorkMode;
        string _gatewayMode;

        SqliteHelper _sqlitehelper;
        int _connectionID;

        #endregion

        #region UI -> Source

        public bool ClientOrServer { get; set; }
        public bool UDPOrTCP { get; set; }
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public int MaxiConnections { get; set; }
        public int ReceiveBufferSize { get; set; }
        public string Alias { get; set; }

        public bool JsonSerialized { get; set; }
        public bool WordWrap { get; set; }
        public bool DisplayDatetime { get; set; }
        public bool RxCountIncrement { get; set; }
        public bool TxCountIncrement { get; set; }
        public bool DisplayTxRxLog { get; set; }

        public bool FormatASCII { get; set; }
        public bool FormatHEX { get; set; }
        public bool FormatUTF8 { get; set; }

        public bool SaveToSQLite { get; set; }
        public bool SaveToTxtFile { get; set; }

        public bool Singlecast { get; set; }
        public bool Broadcast { get; set; }
        public bool Multicast { get; set; }

        public string CastIPAddress { get; set; }

        public bool AutoSend { get; set; }
        public string SendCycleTime { get; set; }


        #endregion

        #region Source -> UI

        public ObservableCollection<TcpClientModel> TcpClientViewModelCollection { get; set; }


        private string _startStop;
        public string StartStop
        {
            get { return _startStop; }
            set
            {
                if (_startStop == value) { return; }
                _startStop = value;
                RaisePropertyChanged();
            }
        }


        private bool _isRunning;
        public bool IsRunning
        {
            get { return _isRunning; }
            set
            {
                if (_isRunning == value) { return; }
                _isRunning = value;
                RaisePropertyChanged();
                Messenger.Default.Send<bool>(_isRunning, "TcpUdp");
            }
        }


        private bool _udpOperationEnable;
        public bool UdpOperationEnable
        {
            get { return _udpOperationEnable; }
            set
            {
                if (_udpOperationEnable == value) { return; }
                _udpOperationEnable = value;
                RaisePropertyChanged();
            }
        }


        private int _clientTotal;
        public int ClientTotal
        {
            get { return _clientTotal; }
            set
            {
                if (_clientTotal == value) { return; }
                _clientTotal = value;
                RaisePropertyChanged();
            }
        }


        private int _castPort;
        public int CastPort
        {
            get => _castPort;
            set
            {
                if (_castPort == value) { return; }
                _castPort = value;
                RaisePropertyChanged();
            }
        }


        private string _receivedText;
        public string ReceivedText
        {
            get { return _receivedText; }
            set
            {
                if (_receivedText == value) { return; }
                _receivedText = value;
                RaisePropertyChanged();

            }
        }


        private string _remoteName;
        public string RemoteName
        {
            get { return _remoteName; }
            set
            {
                if (_remoteName == value) { return; }
                _remoteName = value;
                RaisePropertyChanged();
            }
        }


        private string _remoteEndPoint;
        public string RemoteEndPoint
        {
            get { return _remoteEndPoint; }
            set
            {
                if (_remoteEndPoint == value) { return; }
                _remoteEndPoint = value;
                RaisePropertyChanged();
            }
        }


        private uint _rxPieces;
        public uint RxPieces
        {
            get { return _rxPieces; }
            set
            {
                if (_rxPieces == value) { return; }
                _rxPieces = value;
                RaisePropertyChanged();
            }
        }


        private uint _rxCount;
        public uint RxCount
        {
            get { return _rxCount; }
            set
            {
                if (_rxCount == value) { return; }
                _rxCount = value;
                RaisePropertyChanged();
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


        private string _sendingText;
        public string SendingText
        {
            get { return _sendingText; }
            set
            {
                if (_sendingText == value) { return; }
                _sendingText = value;
                RaisePropertyChanged();

            }
        }


        private uint _txPieces;
        public uint TxPieces
        {
            get { return _txPieces; }
            set
            {
                if (_txPieces == value) { return; }
                _txPieces = value;
                RaisePropertyChanged();
            }
        }


        private uint _txCount;
        public uint TxCount
        {
            get { return _txCount; }
            set
            {
                if (_txCount == value) { return; }
                _txCount = value;
                RaisePropertyChanged();
            }
        }


        private DataTable _rxDataTable;
        public DataTable RxDataTable
        {
            get => _rxDataTable;
            set
            {
                if (_rxDataTable == value) { return; }
                _rxDataTable = value;
                RaisePropertyChanged();
            }
        }

        private DataTable _infoDataTable;
        public DataTable InfoDataTable
        {
            get => _infoDataTable;
            set
            {
                if (_infoDataTable == value) { return; }
                _infoDataTable = value;
                RaisePropertyChanged();
            }
        }

        private DataTable _txDataTable;
        public DataTable TxDataTable
        {
            get => _txDataTable;
            set
            {
                if (_txDataTable == value) { return; }
                _txDataTable = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Command

        private RelayCommand _cmdStartOrStop;
        public RelayCommand CmdStartOrStop
        {
            get
            {
                if (_cmdStartOrStop == null)
                {
                    _cmdStartOrStop = new RelayCommand(() => StartOrStop(), () => CanStartOrStop());
                }
                return _cmdStartOrStop;
            }
        }


        private RelayCommand _cmdReviewConfirm;
        public RelayCommand CmdReviewConfirm
        {
            get
            {
                if (_cmdReviewConfirm == null)
                {
                    _cmdReviewConfirm = new RelayCommand(() => _castConfirm = false);
                }
                return _cmdReviewConfirm;
            }
        }


        private RelayCommand<string> _cmdSetCastMode;
        public RelayCommand<string> CmdSetCastMode
        {
            get
            {
                if (_cmdSetCastMode == null)
                {
                    _cmdSetCastMode = new RelayCommand<string>((castMode) => SetCastMode(castMode), (castMode) => CanSetCastMode(castMode));
                }
                return _cmdSetCastMode;
            }
        }


        private RelayCommand _cmdResetRxCount;
        public RelayCommand CmdResetRxCount
        {
            get
            {
                if (_cmdResetRxCount == null)
                {
                    _cmdResetRxCount = new RelayCommand(() => { RxCount = 0; RxPieces = 0; });
                }
                return _cmdResetRxCount;
            }
        }


        private RelayCommand _cmdClearReceiveArea;
        public RelayCommand CmdClearReceiveArea
        {
            get
            {
                if (_cmdClearReceiveArea == null)
                {
                    _cmdClearReceiveArea = new RelayCommand(() => { ReceivedText = ""; RemoteName = ""; RemoteEndPoint = ""; },
                                                                () => !String.IsNullOrEmpty(ReceivedText));
                }
                return _cmdClearReceiveArea;
            }
        }


        private RelayCommand _cmdClearInfoMessage;
        public RelayCommand CmdClearInfoMessage
        {
            get
            {
                if (_cmdClearInfoMessage == null)
                {
                    _cmdClearInfoMessage = new RelayCommand(() => InfoMessage = String.Empty);
                }
                return _cmdClearInfoMessage;
            }
        }


        private RelayCommand _cmdReviewText;
        public RelayCommand CmdReviewText
        {
            get
            {
                if (_cmdReviewText == null)
                {
                    _cmdReviewText = new RelayCommand(() => ReviewText());
                }
                return _cmdReviewText;
            }
        }



        private RelayCommand _cmdSendMessage;
        public RelayCommand CmdSendMessage
        {
            get
            {
                if (_cmdSendMessage == null)
                {
                    _cmdSendMessage = new RelayCommand(() => SendText(), () => CanSendText());
                }
                return _cmdSendMessage;
            }
        }


        private RelayCommand _cmdResetTxCount;
        public RelayCommand CmdResetTxCount
        {
            get
            {
                if (_cmdResetTxCount == null)
                {
                    _cmdResetTxCount = new RelayCommand(() => { TxCount = 0; TxPieces = 0; }, () => !AutoSend);
                }
                return _cmdResetTxCount;
            }
        }

        private RelayCommand _cmdClearSendArea;
        public RelayCommand CmdClearSendArea
        {
            get
            {
                if (_cmdClearSendArea == null)
                {
                    _cmdClearSendArea = new RelayCommand(() => SendingText = string.Empty, () => CanClearSendArea());
                }
                return _cmdClearSendArea;
            }
        }

        public ICommand CmdQueryRxLog { get => new RelayCommand(() => RxDataTable = _sqlitehelper.QueryEthernetPortMsg(_connectionID, $"{_workRole}", "Rx")); }

        public ICommand CmdClearRxLog { get => new RelayCommand(() => { RxDataTable.Clear(); _sqlitehelper.DeleteEthernetPortMsg(_connectionID, $"{_workRole}", "Rx"); }, () => CanClearRxLog()); }

        public ICommand CmdQueryInfoLog { get => new RelayCommand(() => InfoDataTable = _sqlitehelper.QueryInfoMsg("EthernetPort", _connectionID)); }

        public ICommand CmdClearInfoLog { get => new RelayCommand(() => { InfoDataTable.Clear(); _sqlitehelper.DeleteInfoMsg("EthernetPort", _connectionID); }, () => CanClearInfoLog()); }

        public ICommand CmdQueryTxLog { get => new RelayCommand(() => TxDataTable = _sqlitehelper.QueryEthernetPortMsg(_connectionID, $"{_workRole}", "Tx")); }

        public ICommand CmdClearTxLog { get => new RelayCommand(() => { TxDataTable.Clear(); _sqlitehelper.DeleteEthernetPortMsg(_connectionID, $"{_workRole}", "Tx"); }, () => CanClearTxLog()); }



        #endregion

        public TcpUdpViewModel()
        {
            StartStop = "START";
            IPAddress = string.Empty;
            Alias = string.Empty;

            ReceiveBufferSize = 1;
            TcpClientViewModelCollection = new ObservableCollection<TcpClientModel>();


            _sendTimer = new System.Timers.Timer();
            _sendTimer.Elapsed += (sender, e) =>
            {
                if (AutoSend)
                {
                    SendText();
                }
                else
                {
                    _sendTimer.Enabled = false;
                }

            };


            Messenger.Default.Register<string>(this, "GatewayMode", (msg) => _gatewayMode = msg);
            Messenger.Default.Register<string>(this, "WorkMode", (workMode) => _toolWorkMode = workMode);
            Messenger.Default.Register<string>(this, "SerialPortInput", (msg) =>
            {
                SendingText = msg;
                SendText();
            });

            Messenger.Default.Register<string>(this, "Close", (msg) =>
            {
                if (msg == "CloseConnection")
                {
                    if (IsRunning)
                    {
                        StartOrStop();
                    }
                }
            });

            _sqlitehelper = SqliteHelper.GetSqliteHelpeInstance();

        }

        public override void Cleanup()
        {
            base.Cleanup();
        }

        #region Command Methods

        private void StartOrStop()
        {
            try
            {
                _workRole = GetWorkRole();

                System.Net.IPAddress iPAddress = _workRole == TcpUdpWorkRoleEnum.UdpClient ? System.Net.IPAddress.None : System.Net.IPAddress.Parse(IPAddress.Trim());
                if (SaveToSQLite && !IsRunning)
                {
                    _connectionID = _sqlitehelper.InsertIntoTableEthernetPortInfo($"{_workRole}", IPAddress, $"{Port}", $"{MaxiConnections}", $"{ReceiveBufferSize} KB");
                }

                switch (_workRole)
                {

                    case TcpUdpWorkRoleEnum.TcpServer:

                        if (StartStop == "START")
                        {
                            _tcpServer = new TcpServerHelperIOCP(MaxiConnections, ReceiveBufferSize);
                            _tcpServer.Init();
                            _tcpServer.AcceptCompleted += _tcpServer_AcceptCompleted;
                            _tcpServer.DataReceived += _tcpServer_DataReceived;
                            _tcpServer.MessageInformed += MessageInformed;
                            _tcpServer.ClientNumberChanged += _tcpServer_ClientNumberChanged;
                            _tcpServer.SendCompleted += SendCompleted;
                            IsRunning = _tcpServer.Start(iPAddress, Port);

                            if (IsRunning)
                            {
                                StartStop = "STOP SERVER";
                            }

                        }
                        else if (StartStop == "STOP SERVER")
                        {
                            _tcpServer.DataReceived -= _tcpServer_DataReceived;
                            _tcpServer.ClientNumberChanged -= _tcpServer_ClientNumberChanged;
                            _tcpServer.SendCompleted -= SendCompleted;
                            StopRunning();

                            _tcpServer.Stop();
                            _codeCount = 0;

                            App.Current.Dispatcher.Invoke(() => TcpClientViewModelCollection.Clear());
                        }

                        break;
                    case TcpUdpWorkRoleEnum.TcpClient:

                        if (StartStop == "START")
                        {

                            _tcpClient = new TcpClientHelperIOCP(ReceiveBufferSize);
                            _tcpClient.ConnectCompleted += _tcpClient_ConnectCompleted;
                            _tcpClient.DisconnectCompleted += _tcpClient_DisconnectCompleted;
                            _tcpClient.ReceiveCompleted += ReceiveCompleted;
                            _tcpClient.MessageInformed += MessageInformed;
                            _tcpClient.SendCompleted += SendCompleted;
                            _tcpClient.Connect(iPAddress, Port);

                        }
                        else if (StartStop == "STOP CLIENT")
                        {
                            _tcpClient.ReceiveCompleted -= ReceiveCompleted;
                            _tcpClient.SendCompleted -= SendCompleted;
                            StopRunning();

                            _tcpClient.Disconnect();
                        }
                        break;
                    case TcpUdpWorkRoleEnum.UdpServer:
                        if (StartStop == "START")
                        {
                            _udpHelper = new UdpHelperIOCP(ReceiveBufferSize);
                            _udpHelper.MessageInformed += MessageInformed;
                            _udpHelper.ReceiveCompleted += ReceiveCompleted;
                            _udpHelper.SendCompleted += SendCompleted;

                            _udpHelper.StartServer(iPAddress, Port);

                            _castConfirm = false;
                            StartStop = "STOP SERVER";
                            UdpOperationEnable = true;
                            CastPort = Port;
                            IsRunning = true;

                        }
                        else if (StartStop == "STOP SERVER")
                        {
                            _udpHelper.ReceiveCompleted -= ReceiveCompleted;
                            _udpHelper.SendCompleted -= SendCompleted;
                            StopRunning();

                            _udpHelper.Stop();
                            UdpOperationEnable = false;
                        }

                        break;
                    case TcpUdpWorkRoleEnum.UdpClient:
                        if (StartStop == "START")
                        {

                            _udpHelper = new UdpHelperIOCP(ReceiveBufferSize);
                            _udpHelper.MessageInformed += MessageInformed;
                            _udpHelper.ReceiveCompleted += ReceiveCompleted;
                            _udpHelper.SendCompleted += SendCompleted;
                            _udpHelper.UdpClientReceived += _udpHelper_UdpClientReceived;

                            _udpHelper.StartClient();
                            StartStop = "STOP CLIENT";
                            _castConfirm = false;
                            UdpOperationEnable = true;
                            IsRunning = true;

                        }
                        else if (StartStop == "STOP CLIENT")
                        {
                            _udpHelper.ReceiveCompleted -= ReceiveCompleted;
                            _udpHelper.SendCompleted -= SendCompleted;
                            _udpHelper.UdpClientReceived -= _udpHelper_UdpClientReceived;
                            StopRunning();

                            _udpHelper.Stop();
                            UdpOperationEnable = false;
                        }

                        break;
                    default:

                        break;
                }
                if (!IsRunning)
                {
                    ResetCount();
                }

            }
            catch (Exception ex)
            {
                InfoMessage = "Error: " + ex.Message.Replace("\n", "");
            }
        }

        private bool CanStartOrStop()
        {
            _workRole = GetWorkRole();

            if (!string.IsNullOrEmpty(IPAddress) && !string.IsNullOrWhiteSpace(IPAddress) &&
                        Port > 0 && Port <= 65535 && ReceiveBufferSize > 0 && ReceiveBufferSize <= 500)
            {
                if (_workRole == TcpUdpWorkRoleEnum.TcpServer)
                {
                    return MaxiConnections > 0;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                if (_workRole == TcpUdpWorkRoleEnum.UdpClient)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }

        private void SetCastMode(string castMode)
        {
            try
            {
                _castIPAddress = System.Net.IPAddress.Parse(CastIPAddress.Trim());

                if (castMode.Equals("ExitMulticast"))
                {
                    _udpHelper?.ExitMulticastGroup(_castIPAddress, CastPort);
                    return;
                }
                else
                {
                    MethodInfo methodInfo = _udpHelper?.GetType().GetMethod("Set" + castMode + "Mode", new Type[] { typeof(IPAddress), typeof(int) });
                    methodInfo?.Invoke(_udpHelper, new object[] { _castIPAddress, CastPort });
                    _castConfirm = true;
                }

            }
            catch (Exception ex)
            {

                InfoMessage = "Error: " + ex.Message.Replace("\n", "");
            }
        }

        private bool CanSetCastMode(string castMode)
        {

            if (IsRunning && !string.IsNullOrEmpty(CastIPAddress) && !string.IsNullOrWhiteSpace(CastIPAddress) && CastPort > 0 && CastPort <= 65535)
            {

                return true;
            }
            else
            {
                return false;
            }
        }

        private void ReviewText()
        {
            try
            {
                if (!string.IsNullOrEmpty(SendingText) && !string.IsNullOrWhiteSpace(SendingText))
                {
                    if (JsonSerialized && SendingText.Length > _remoteRcvBufferSize)
                    {
                        InfoMessage = "Warning: Input Message length is longer than RemoteEndPoint ReceiveBufferSize !";
                        return;

                    }
                    if (GetDataFormatEnum() == DataFormatEnum.HEX)
                    {
                        _textValid = ToolHelper.ReviewHexString(SendingText);
                        if (!_textValid)
                        {
                            InfoMessage = "Warning: HEX String is invalid !";
                            return;
                        }
                    }
                    else
                    {
                        _textValid = true;
                    }
                }
                else
                {
                    _textValid = false;
                }

            }
            catch (Exception ex)
            {

                InfoMessage = "Error: " + ex.Message.Replace("\n", "");
            }



        }

        private void SendText()
        {
            try
            {
                if (_sendTimer.Enabled == false && AutoSend)
                {
                    _sendTimer.Interval = int.Parse(SendCycleTime);
                    _sendTimer.Enabled = true;

                }
                byte[] sendingMsg = ToolHelper.StringToByteArray(SendingText, GetDataFormatEnum());

                switch (_workRole)
                {
                    case TcpUdpWorkRoleEnum.TcpServer:
                        if (_toolWorkMode != "Gateway" && !(TcpClientViewModelCollection.ToList().Count(c => c.IsChecked) > 0))
                        {
                            InfoMessage = "Warning: Please Select at least one client for sending message!";
                            return;
                        }
                        if (TcpClientViewModelCollection.ToList().Exists(client => client.IsChecked))
                        {
                            List<TcpClientModel> sendingClientList = TcpClientViewModelCollection.ToList().FindAll(client => client.IsChecked);

                            foreach (var item in sendingClientList)
                            {
                                AsyncUserTokenIOCP sendingUserToken = _tcpServer.ClientList.Find(client => client.Socket.RemoteEndPoint.ToString().Equals(item.EndPoint));

                                if (JsonSerialized)
                                {
                                    _tcpServer?.SendAsync(sendingUserToken, SerializeText(sendingMsg));
                                }
                                else
                                {
                                    _tcpServer?.SendAsync(sendingUserToken, sendingMsg);
                                }

                            }

                        }

                        break;
                    case TcpUdpWorkRoleEnum.TcpClient:
                        if (JsonSerialized)
                        {
                            _tcpClient?.SendAsync(SerializeText(sendingMsg));
                        }
                        else
                        {
                            _tcpClient?.SendAsync(sendingMsg);
                        }
                        break;
                    case TcpUdpWorkRoleEnum.UdpServer:
                        if (JsonSerialized)
                        {
                            _udpHelper?.SendToAsync(SerializeText(sendingMsg));
                        }
                        else
                        {
                            _udpHelper?.SendToAsync(sendingMsg);
                        }
                        break;
                    case TcpUdpWorkRoleEnum.UdpClient:
                        if (JsonSerialized)
                        {
                            _udpHelper?.SendToAsync(SerializeText(sendingMsg));
                        }
                        else
                        {
                            _udpHelper?.SendToAsync(sendingMsg);
                        }

                        break;
                    default:

                        break;
                }

            }
            catch (Exception ex)
            {
                InfoMessage = "Error: " + ex.Message.Replace("\n", "");
            }

        }

        private bool CanSendText()
        {
            if (_textValid)
            {
                switch (_workRole)
                {
                    case TcpUdpWorkRoleEnum.TcpServer:
                    case TcpUdpWorkRoleEnum.TcpClient:

                        return IsRunning;
                    case TcpUdpWorkRoleEnum.UdpServer:
                    case TcpUdpWorkRoleEnum.UdpClient:

                        return IsRunning && _castConfirm;
                    default:

                        return false;
                }
            }
            else
            {
                return false;
            }

        }

        private bool CanClearSendArea()
        {

            if (!string.IsNullOrEmpty(SendingText) && !string.IsNullOrWhiteSpace(SendingText) && !AutoSend)
            {
                return true;
            }

            return false;
        }

        private bool CanClearRxLog()
        {
            if (RxDataTable != null)
            {
                return RxDataTable.Rows.Count > 0;
            }
            return false;
        }

        private bool CanClearInfoLog()
        {
            if (InfoDataTable != null)
            {
                return InfoDataTable.Rows.Count > 0;
            }
            return false;
        }

        private bool CanClearTxLog()
        {
            if (TxDataTable != null)
            {
                return TxDataTable.Rows.Count > 0;
            }
            return false;
        }


        #endregion

        #region EventHandler

        private void _tcpServer_AcceptCompleted(AsyncUserTokenIOCP token)
        {
            try
            {
                _codeCount++;

                //Get Client Name&EndPoint
                TcpClientModel clientViewModel = new TcpClientModel
                {
                    Name = $"Client {_codeCount}",
                    EndPoint = token.Socket.RemoteEndPoint.ToString(),
                    Code = _codeCount
                };
                App.Current.Dispatcher.Invoke(() => TcpClientViewModelCollection.Add(clientViewModel));


                //send : Connected to Server
                byte[] msg = ToolHelper.StringToByteArray($"Connected to Server ({token.Socket.LocalEndPoint}) \n", GetDataFormatEnum());
                if (JsonSerialized)
                {
                    if (string.IsNullOrEmpty(Alias))
                    {
                        Alias = "TCP Server01";
                    }

                    string msgString = JsonHelper.SerializeMessage($" <{Alias}>  ", ReceiveBufferSize, msg,
                                                    SerializedMsgTypeEnum.Object, SerializedMsgFunctionEnum.ConnectionData);
                    byte[] msgArray = ToolHelper.StringToByteArray(msgString, GetDataFormatEnum());
                    _tcpServer.SendAsync(token, msgArray);
                }
                else
                {
                    _tcpServer.SendAsync(token, msg);
                }

            }
            catch (Exception ex)
            {

                InfoMessage = "Error: " + ex.Message.Replace("\n", "");
            }


        }

        private void _tcpServer_ClientNumberChanged(int num, AsyncUserTokenIOCP token)
        {
            try
            {
                ClientTotal += num;
                if (num == -1)
                {
                    var tcpClient = TcpClientViewModelCollection.ToList().Find(c =>
                                        c.EndPoint == token.Socket.RemoteEndPoint.ToString());
                    App.Current.Dispatcher.Invoke(() => TcpClientViewModelCollection.Remove(tcpClient));
                }

            }
            catch (Exception ex)
            {
                InfoMessage = "Error: " + ex.Message.Replace("\n", "");

            }
        }

        private void _tcpServer_DataReceived(AsyncUserTokenIOCP token, byte[] buffer)
        {
            try
            {
                if (!IsRunning)
                {
                    return;
                }

                RxPieces++;
                if (SaveToSQLite)
                {
                    string receivedText = ToolHelper.ByteArrayToString(buffer, GetDataFormatEnum());
                    _sqlitehelper.InsertIntoTableEthernetPortMsg(_connectionID, "Rx", receivedText, $"{token.Socket.RemoteEndPoint}");
                }

                if (JsonSerialized)
                {
                    string msgString = ToolHelper.ByteArrayToString(buffer);
                    SerializedMessageModel message = JsonHelper.DeserializeMessage(msgString);
                    if (message != null)
                    {
                        App.Current.Dispatcher.Invoke(() => RemoteName = message.Name);
                        App.Current.Dispatcher.Invoke(() => RemoteEndPoint = token.Socket.RemoteEndPoint.ToString());

                        if (message.MessageFunction == SerializedMsgFunctionEnum.ConnectionData)
                        {
                            var tcpClient = TcpClientViewModelCollection.ToList().Find(c =>
                                       c.EndPoint == token.Socket.RemoteEndPoint.ToString());
                            App.Current.Dispatcher.Invoke(() => tcpClient.Name = message.Name);

                            _remoteRcvBufferSize = message.RcvBufferSize;
                        }
                        else if (message.MessageFunction == SerializedMsgFunctionEnum.ActualData)
                        {
                            if (message.MessageType == SerializedMsgTypeEnum.Text)
                            {
                                ReceiveText(message.Buffer);
                            }
                            else
                            {
                                return;
                            }
                        }
                        else
                        {
                            return;
                        }

                    }
                    return;
                }
                else
                {

                    App.Current.Dispatcher.Invoke(() => RemoteName = String.Empty);
                    App.Current.Dispatcher.Invoke(() => RemoteEndPoint = token.Socket.RemoteEndPoint.ToString());

                    ReceiveText(buffer);
                }


            }
            catch (Exception ex)
            {
                InfoMessage = "Error: " + ex.Message.Replace("\n", "");
            }

        }

        private void _tcpClient_ConnectCompleted(SocketAsyncEventArgs e)
        {
            try
            {
                IsRunning = true;
                StartStop = "STOP CLIENT";
                InfoMessage = $"Connected to TCP Server [{e.RemoteEndPoint}] !";

                if (SaveToSQLite)
                {
                    string[] localEndPoint = e.ConnectSocket.LocalEndPoint.ToString().Split(':');
                    _sqlitehelper.UpdateEthernetPortInfo(_connectionID, localEndPoint[0], localEndPoint[1]);
                }

                RemoteName = String.Empty;
                RemoteEndPoint = e.RemoteEndPoint.ToString();

                if (JsonSerialized)
                {
                    if (string.IsNullOrEmpty(Alias))
                    {
                        Alias = string.Empty;
                    }
                    string MsgString = JsonHelper.SerializeMessage(" <" + Alias + ">  ", ReceiveBufferSize, null,
                                                  SerializedMsgTypeEnum.Object, SerializedMsgFunctionEnum.ConnectionData);
                    byte[] MsgArray = ToolHelper.StringToByteArray(MsgString, GetDataFormatEnum());
                    _tcpClient.SendAsync(MsgArray);
                }
                else
                {

                    return;
                }
            }
            catch (Exception ex)
            {

                InfoMessage = "Error: " + ex.Message.Replace("\n", "");
            }


        }

        private void _tcpClient_DisconnectCompleted()
        {
            _tcpClient.Disconnect();
            IsRunning = false;
            StartStop = "START";
        }

        private void _udpHelper_UdpClientReceived(Socket socket)
        {
            if (SaveToSQLite && RxPieces < 2)
            {
                string[] localEndPoint = socket.LocalEndPoint.ToString().Split(':');
                _sqlitehelper.UpdateEthernetPortInfo(_connectionID, localEndPoint[0], localEndPoint[1]);
            }
        }

        private void ReceiveCompleted(SocketAsyncEventArgs e, byte[] buffer)
        {
            try
            {
                RxPieces++;

                if (JsonSerialized)
                {
                    ReceiveMessage(e, buffer);
                }
                else
                {
                    App.Current.Dispatcher.Invoke(() => RemoteName = String.Empty);
                    App.Current.Dispatcher.Invoke(() => RemoteEndPoint = e.RemoteEndPoint.ToString());
                    ReceiveText(buffer);
                }

                if (SaveToSQLite)
                {
                    string receivedText = ToolHelper.ByteArrayToString(buffer, GetDataFormatEnum());
                    _sqlitehelper.InsertIntoTableEthernetPortMsg(_connectionID, "Rx", receivedText, $"{e.RemoteEndPoint}");
                }

            }
            catch (Exception ex)
            {
                InfoMessage = "Error: " + ex.Message.Replace("\n", "");
            }
        }

        private void MessageInformed(string message)
        {
            try
            {
                InfoMessage = message;
                if (SaveToSQLite && _connectionID > 0)
                {
                    _sqlitehelper.InsertIntoTableInfoMsg("EthernetPort", message, _connectionID);
                }
            }
            catch (Exception ex)
            {
                InfoMessage = "Error: " + ex.Message.Replace("\n", "");
            }
        }

        private void SendCompleted(SocketAsyncEventArgs e)
        {
            try
            {
                TxPieces++;
                string sendedText = ToolHelper.ByteArrayToString(e.Buffer, GetDataFormatEnum());

                if (DisplayTxRxLog)
                {
                    ReceivedText += $"{ToolHelper.SetTime(true, false)}Tx {TxPieces} -> {sendedText}";
                }
                TxCount = ToolHelper.CalcCountBytes(e.Buffer, TxCountIncrement, TxCount);

                if (TxPieces == 1 && _workRole == TcpUdpWorkRoleEnum.UdpClient)
                {
                    _udpHelper?.ReceiveFromAsync(e.RemoteEndPoint);
                }

                if (SaveToSQLite)
                {
                    _sqlitehelper.InsertIntoTableEthernetPortMsg(_connectionID, "Tx", sendedText, $"{e.RemoteEndPoint}");
                }
            }
            catch (Exception ex)
            {
                InfoMessage = "Error: " + ex.Message.Replace("\n", "");
            }

        }


        #endregion

        #region Shared Methods

        private void ResetCount()
        {
            TxCount = 0;
            RxCount = 0;
            TxPieces = 0;
            RxPieces = 0;
        }

        private void StopRunning()
        {
            IsRunning = false;
            StartStop = "START";
            _sendTimer.Enabled = false;
        }

        private TcpUdpWorkRoleEnum GetWorkRole()
        {
            if (ClientOrServer)
            {
                if (UDPOrTCP)
                {
                    return TcpUdpWorkRoleEnum.TcpServer;
                }
                else
                {
                    return TcpUdpWorkRoleEnum.UdpServer;
                }
            }
            else
            {
                if (UDPOrTCP)
                {
                    return TcpUdpWorkRoleEnum.TcpClient;
                }
                else
                {
                    return TcpUdpWorkRoleEnum.UdpClient;
                }
            }
        }

        private DataFormatEnum GetDataFormatEnum()
        {
            if (FormatASCII)
            {
                return DataFormatEnum.ASCII;
            }
            else if (FormatHEX)
            {
                return DataFormatEnum.HEX;
            }
            else if (FormatUTF8)
            {
                return DataFormatEnum.UTF8;
            }
            else
            {
                return DataFormatEnum.DEFAULT;
            }
        }


        private void ReceiveMessage(SocketAsyncEventArgs e, byte[] buffer)
        {
            try
            {
                string msgString = ToolHelper.ByteArrayToString(buffer);
                SerializedMessageModel message = JsonHelper.DeserializeMessage(msgString);
                if (message != null)
                {
                    App.Current.Dispatcher.Invoke(() => RemoteName = message.Name);
                    App.Current.Dispatcher.Invoke(() => RemoteEndPoint = e.RemoteEndPoint.ToString());

                    if (message.MessageFunction == SerializedMsgFunctionEnum.ConnectionData)
                    {
                        ReceiveText(message.Buffer);

                        _remoteRcvBufferSize = message.RcvBufferSize;
                    }
                    else if (message.MessageFunction == SerializedMsgFunctionEnum.ActualData)
                    {
                        if (message.MessageType == SerializedMsgTypeEnum.Text)
                        {
                            ReceiveText(message.Buffer);
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }


                }
                return;
            }
            catch (Exception ex)
            {

                InfoMessage = "Error: " + ex.Message.Replace("\n", "");
            }

        }

        private byte[] SerializeText(byte[] buffer)
        {
            try
            {
                string msgString = JsonHelper.SerializeMessage(Alias, ReceiveBufferSize, buffer,
                                               SerializedMsgTypeEnum.Text, SerializedMsgFunctionEnum.ActualData);
                return ToolHelper.StringToByteArray(msgString, GetDataFormatEnum());

            }
            catch (Exception ex)
            {
                InfoMessage = "Error: " + ex.Message.Replace("\n", "");
                return null;
            }

        }

        private void ReceiveText(byte[] buffer)
        {
            try
            {
                if (DisplayTxRxLog)
                {
                    ReceivedText += $"{ToolHelper.SetTime(true, false)}Rx {RxPieces} -> {ToolHelper.ByteArrayToString(buffer, GetDataFormatEnum())}";
                }
                else
                {
                    ReceivedText += $"{ToolHelper.SetWordWrap(WordWrap)}{ToolHelper.SetTime(DisplayDatetime, WordWrap)}{ToolHelper.ByteArrayToString(buffer, GetDataFormatEnum())}";
                }
                RxCount = ToolHelper.CalcCountBytes(buffer, RxCountIncrement, RxCount);

                if (_toolWorkMode == "Gateway" && _gatewayMode == "TCP/UDP --> Serial Port")
                {
                    string newMessage = ToolHelper.ByteArrayToString(buffer, GetDataFormatEnum());
                    Messenger.Default.Send<string>(newMessage, "EthernetPortInput");
                }
            }
            catch (Exception ex)
            {

                InfoMessage = "Error: " + ex.Message.Replace("\n", "");
            }


        }





        #endregion
    }
}
