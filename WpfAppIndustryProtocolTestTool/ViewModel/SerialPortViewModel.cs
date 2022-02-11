using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Timers;
using WpfAppIndustryProtocolTestTool.BLL;
using WpfAppIndustryProtocolTestTool.BLL.SerialPortProtocol;
using WpfAppIndustryProtocolTestTool.DAL;
using WpfAppIndustryProtocolTestTool.Model;
using WpfAppIndustryProtocolTestTool.Model.Enum;
using System.Windows.Input;
using System.IO;
using Microsoft.Win32;
using System.Threading.Tasks;
using System.Threading;

namespace WpfAppIndustryProtocolTestTool.ViewModel
{
    public class SerialPortViewModel : ViewModelBase
    {
        #region Private Fields

        SerialPortCfgModel _nameCfg;
        SerialPortCfgModel _baudCfg;
        SerialPortCfgModel _parityCfg;
        SerialPortCfgModel _dataBitsCfg;
        SerialPortCfgModel _stopBitsCfg;
        SerialPortCfgModel _handshakeCfg;
        SerialPortCfgModel _thresholdCfg;
        SerialPortCfgModel _readTimeoutCfg;
        SerialPortCfgModel _writeTimeoutCfg;
        SerialPortCfgModel _readBufferSizeCfg;
        SerialPortCfgModel _writeBufferSizeCfg;

        SerialPortHelper _serialPortHelper;

        System.Timers.Timer _sendTimer;
        //bool _textValid;

        string _workMode;
        string _gatewayMode;

        SqliteHelper _sqlitehelper;
        int _portID;

        DirectoryInfo _dirInfo;
        FileInfo _fileInfo;

        CancellationTokenSource _cancellationTokenSource;
        #endregion

        #region UI --> Source

        public bool FormatASCII { get; set; }
        public bool FormatHEX { get; set; }
        public bool FormatUTF8 { get; set; }
        public bool CRC8 { get; set; }
        public bool CRC16 { get; set; }
        public bool CRC32 { get; set; }
        public bool SaveToSQLite { get; set; }
        public bool SaveToTxtFile { get; set; }
        public bool SendWithWordWrap { get; set; }
        public bool SendWithDateTime { get; set; }
        public bool TxCountIncrement { get; set; }
        public bool DisplayInRcvArea { get; set; }
        public bool ReceiveWordWrap { get; set; }
        public bool DisplayDateTime { get; set; }
        public bool RxCountIncrement { get; set; }
        public bool ReceivePause { get; set; }
        public bool AutoSend { get; set; }
        public string SendCycleTime { get; set; }





        #endregion

        #region Source --> UI
        public ObservableCollection<SerialPortCfgModel> SerialPortCfgCollection { get; set; }


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


        private bool _autoReceive;
        public bool AutoReceive
        {
            get { return _autoReceive; }
            set
            {
                if (_autoReceive == value) { return; }
                _autoReceive = value;
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
                if (SaveToSQLite && _portID > 0)
                {
                    _sqlitehelper.InsertIntoTableInfoMsg("SerialPort", _infoMessage, _portID);
                }
                if (SaveToTxtFile)
                {
                    AppendLogText($"{ToolHelper.SetTime(true, false)}Info -> {_infoMessage}");
                }
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


        private bool _isOpen;

        public bool IsOpen
        {
            get { return _isOpen; }
            set
            {
                if (_isOpen == value) { return; }
                _isOpen = value;
                RaisePropertyChanged();
                Messenger.Default.Send<bool>(_isOpen, "SerialPort");
            }
        }


        private string _portOperation;

        public string PortOperation
        {
            get { return _portOperation; }
            set
            {
                if (_portOperation == value) { return; }
                _portOperation = value;
                RaisePropertyChanged();
            }
        }


        private ushort _txPieces;
        public ushort TxPieces
        {
            get { return _txPieces; }
            set
            {
                if (_txPieces == value) { return; }
                _txPieces = value;
                RaisePropertyChanged();
            }
        }


        private ushort _rxCount;
        public ushort RxCount
        {
            get { return _rxCount; }
            set
            {
                if (_rxCount == value) { return; }
                _rxCount = value;
                RaisePropertyChanged();
            }
        }


        private ushort _rxPieces;
        public ushort RxPieces
        {
            get { return _rxPieces; }
            set
            {
                if (_rxPieces == value) { return; }
                _rxPieces = value;
                RaisePropertyChanged();
            }
        }


        private ushort _txCount;
        public ushort TxCount
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


        private string _filePath;
        public string FilePath
        {
            get => _filePath;
            set
            {
                if (_filePath == value) { return; }
                _filePath = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Command


        private RelayCommand _cmdloadDefaultPara;
        public RelayCommand CmdLoadDefaultPara
        {
            get
            {
                if (_cmdloadDefaultPara == null)
                {
                    _cmdloadDefaultPara = new RelayCommand(() => LoadDefaultPara(), () => !IsOpen);
                }
                return _cmdloadDefaultPara;
            }
        }



        private RelayCommand _cmdClearReceiveArea;
        public RelayCommand CmdClearReceiveArea
        {
            get
            {
                if (_cmdClearReceiveArea == null)
                {
                    _cmdClearReceiveArea = new RelayCommand(() => ReceivedText = string.Empty,
                                                            () => !string.IsNullOrWhiteSpace(ReceivedText) && !string.IsNullOrEmpty(ReceivedText));
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

        private RelayCommand _cmdClearSendArea;
        public RelayCommand CmdClearSendArea
        {
            get
            {
                if (_cmdClearSendArea == null)
                {
                    _cmdClearSendArea = new RelayCommand(() => SendingText = string.Empty, () => !AutoSend);

                }
                return _cmdClearSendArea;
            }
        }



        private RelayCommand _cmdOpenClosePort;
        public RelayCommand CmdOpenClosePort
        {
            get
            {
                if (_cmdOpenClosePort == null)
                {
                    _cmdOpenClosePort = new RelayCommand(() => OpenClosePort(), () => CanOpenClosePort());
                }
                return _cmdOpenClosePort;
            }
        }


        private RelayCommand _cmdSendText;
        public RelayCommand CmdSendText
        {
            get
            {
                if (_cmdSendText == null)
                {
                    _cmdSendText = new RelayCommand(() => SendText(), () => IsOpen);
                }
                return _cmdSendText;
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


        public ICommand CmdQueryRxLog { get => new RelayCommand(async () => RxDataTable = await _sqlitehelper.QuerySerialPortMsg(_portID, "Rx")); }

        public ICommand CmdClearRxLog { get => new RelayCommand(() => { RxDataTable.Clear(); _sqlitehelper.DeleteSerialPortMsg(_portID, "Rx"); }, () => CanClearRxLog()); }

        public ICommand CmdQueryInfoLog { get => new RelayCommand(async () => InfoDataTable = await _sqlitehelper.QueryInfoMsg("SerialPort", _portID)); }

        public ICommand CmdClearInfoLog { get => new RelayCommand(() => { InfoDataTable.Clear(); _sqlitehelper.DeleteInfoMsg("SerialPort", _portID); }, () => CanClearInfoLog()); }

        public ICommand CmdQueryTxLog { get => new RelayCommand(async () => TxDataTable = await _sqlitehelper.QuerySerialPortMsg(_portID, "Tx")); }

        public ICommand CmdClearTxLog { get => new RelayCommand(() => { TxDataTable.Clear(); _sqlitehelper.DeleteSerialPortMsg(_portID, "Tx"); }, () => CanClearTxLog()); }


        public ICommand CmdChangeDirectory { get => new RelayCommand(() => ChangeDirectory(), () => !IsOpen); }



        #endregion


        public SerialPortViewModel()
        {
            DisplayDateTime = true;
            PortOperation = "Open Port";

            InitSerialPortCfg();

            _serialPortHelper = SerialPortHelper.GetInstance();
            _serialPortHelper.ReceiveCompleted += _serialPortHelper_ReceiveCompleted;
            _serialPortHelper.SendCompleted += _serialPortHelper_SendCompleted;

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

            Messenger.Default.Register<string>(this, "WorkMode", (workMode) => _workMode = workMode);
            Messenger.Default.Register<string>(this, "GatewayMode", (gatewayMode) => _gatewayMode = gatewayMode);
            Messenger.Default.Register<string>(this, "EthernetPortInput", (msg) =>
           {
               SendingText = msg;
               SendText();
           });
            Messenger.Default.Register<string>(this, "Close", (msg) =>
            {
                if (msg == "CloseConnection")
                {
                    if (IsOpen)
                    {
                        OpenClosePort();
                    }
                }
            });

            _sqlitehelper = SqliteHelper.GetSqliteHelpeInstance();
            InitializeLogFile();

            _cancellationTokenSource = new CancellationTokenSource();
        }

        public override void Cleanup()
        {
            base.Cleanup();
            _cancellationTokenSource.Cancel();
        }

        #region Command Methods

        private async void OpenClosePort()
        {
            try
            {
                IsOpen = _serialPortHelper.SerialPort.IsOpen;
                if (!IsOpen)
                {
                    SetSerialPortPara();
                    if (SaveToSQLite)
                    {
                        _portID = await _sqlitehelper.InsertIntoTableSerialPortInfo(_nameCfg.SelectedValue, _baudCfg.SelectedValue, _parityCfg.SelectedValue,
                                                                               _dataBitsCfg.SelectedValue, _stopBitsCfg.SelectedValue, _handshakeCfg.SelectedValue);
                    }
                    _serialPortHelper.OpenPort();
                    PortOperation = "Close Port";
                    IsOpen = _serialPortHelper.SerialPort.IsOpen;
                    InfoMessage = "SerialPort " + _nameCfg.SelectedValue + " is Open !";

                }
                else if (IsOpen)
                {
                    _serialPortHelper?.ClosePort();
                    _sendTimer.Enabled = false;
                    _cancellationTokenSource?.Cancel();

                    PortOperation = "Open Port";
                    IsOpen = _serialPortHelper.SerialPort.IsOpen;
                    InfoMessage = "SerialPort " + _nameCfg.SelectedValue + " is Closed !";
                    ResetCount();
                }



            }
            catch (Exception ex)
            {
                InfoMessage = "Error: " + ex.Message.Replace("\n", "");
            }


        }

        private bool CanOpenClosePort()
        {
            bool result = false;
            foreach (var SerialPortCfg in SerialPortCfgCollection)
            {
                if (!string.IsNullOrEmpty(SerialPortCfg.SelectedValue) && !string.IsNullOrWhiteSpace(SerialPortCfg.SelectedValue))
                {
                    result = true;
                }

            }
            return result;

        }

        private void SendText()
        {

            try
            {
                //ReviewText();

                //if (!_textValid)
                //{
                //    return;
                //}

                if (_sendTimer.Enabled == false && AutoSend)
                {
                    _sendTimer.Interval = int.Parse(SendCycleTime);
                    _sendTimer.Enabled = true;
                }

                string messageStr = $"{ToolHelper.SetWordWrap(SendWithWordWrap)}{ToolHelper.SetTime(SendWithDateTime, SendWithWordWrap)}{SendingText}";
                byte[] sendingMsg = ToolHelper.StringToByteArray(messageStr, GetDataFormatEnum());

                _serialPortHelper?.SendData(sendingMsg, GetCRCEnum());

                if (SaveToSQLite)
                {
                    _sqlitehelper?.InsertIntoTableSerialPortMsg(_portID, "Tx", SendingText);
                }

            }
            catch (Exception ex)
            {
                InfoMessage = "Error: " + ex.Message.Replace("\n", "");
            }
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

        private void ChangeDirectory()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = _fileInfo.DirectoryName;
            ofd.Filter = "Text File(*.txt)|*.txt";
            ofd.DefaultExt = "*.txt";
            if (ofd.ShowDialog() == true && !string.IsNullOrWhiteSpace(ofd.FileName))
            {
                FilePath = ofd.FileName;
            }
        }

        #endregion

        #region Eventhandler

        private void _serialPortHelper_SendCompleted(byte[] sndArray)
        {
            try
            {
                if (TxPieces == ushort.MaxValue)
                {
                    TxPieces = 0;
                }
                if (TxCountIncrement)
                {
                    TxCount += Convert.ToUInt16(sndArray.Length);
                }
                else
                {
                    TxCount = Convert.ToUInt16(sndArray.Length);
                }
                TxPieces++;
                string TxString = ToolHelper.ByteArrayToString(sndArray, GetDataFormatEnum());

                if (DisplayInRcvArea || SaveToTxtFile)
                {
                    Task.Run(async () =>
                    {
                        App.Current.Dispatcher.Invoke(() => ReceivedText += $"{ToolHelper.SetTime(true, false)}Tx {TxPieces} -> {TxString}");
                        await Task.Delay(20);
                    }, _cancellationTokenSource.Token);

                    if (SaveToTxtFile)
                    {
                        AppendLogText($"{ToolHelper.SetTime(true, false)}Tx {TxPieces} -> {TxString}");
                    }
                }

            }
            catch (Exception ex)
            {

                InfoMessage = "Error: " + ex.Message.Replace("\n", "");

            }
        }

        private void _serialPortHelper_ReceiveCompleted(byte[] rcvArray)
        {
            try
            {
                if (!ReceivePause)
                {
                    if (TxPieces == ushort.MaxValue)
                    {
                        TxPieces = 0;
                    }

                    if (RxCountIncrement)
                    {
                        RxCount += Convert.ToUInt16(rcvArray.Length);
                    }
                    else
                    {
                        RxCount = Convert.ToUInt16(rcvArray.Length);
                    }
                    RxPieces++;
                    DataFormatEnum dataFormat = GetDataFormatEnum();
                    byte[]? actualArray = CRCHelper.ValidateCRC(rcvArray, GetCRCEnum());
                    string actualString = string.Empty;
                    if (actualArray != null)
                    {
                        actualString = ToolHelper.ByteArrayToString(actualArray, dataFormat);
                        if (SaveToSQLite)
                        {
                            _sqlitehelper.InsertIntoTableSerialPortMsg(_portID, "Rx", actualString);
                        }

                    }


                    if (DisplayInRcvArea || SaveToTxtFile)
                    {
                        Task.Run(async () =>
                        {
                            App.Current.Dispatcher.Invoke(() => ReceivedText += $"{ToolHelper.SetTime(true, false)}Rx {RxPieces} -> {ToolHelper.ByteArrayToString(rcvArray, dataFormat)}");
                            await Task.Delay(20);
                        }, _cancellationTokenSource.Token);

                        if (SaveToTxtFile)
                        {
                            AppendLogText($"{ToolHelper.SetTime(true, false)}Rx {RxPieces} -> {ToolHelper.ByteArrayToString(rcvArray, dataFormat)}");
                        }
                    }
                    else
                    {
                        ReceivedText += $"{ ToolHelper.SetWordWrap(ReceiveWordWrap)}{ ToolHelper.SetTime(DisplayDateTime, ReceiveWordWrap)}{actualString}";

                        if (_workMode == "Gateway" && _gatewayMode == "Serial Port --> TCP/UDP")
                        {
                            string newMessage = ToolHelper.ByteArrayToString(actualArray, dataFormat);
                            Messenger.Default.Send<string>(newMessage, "SerialPortInput");
                        }
                    }

                }
            }
            catch (Exception ex)
            {

                InfoMessage = "Error: " + ex.Message.Replace("\n", "");
            }
        }

        #endregion

        #region Private Methods        

        private void InitSerialPortCfg()
        {
            try
            {
                SerialPortParaModel serialPortParaEnum = new SerialPortParaModel();


                _nameCfg = new SerialPortCfgModel { ChipContent = "PortName", ChipIcon = "PN", ContentList = new List<string>(serialPortParaEnum.PortName) };
                _baudCfg = new SerialPortCfgModel { ChipContent = "BaudRate", ChipIcon = "BR", ContentList = new List<string>(serialPortParaEnum.BaudRate) };
                _parityCfg = new SerialPortCfgModel { ChipContent = "ParityWay", ChipIcon = "PW", ContentList = new List<string>(serialPortParaEnum.Parity) };
                _dataBitsCfg = new SerialPortCfgModel { ChipContent = "DataBits", ChipIcon = "DB", ContentList = new List<string>(serialPortParaEnum.DataBits) };
                _stopBitsCfg = new SerialPortCfgModel { ChipContent = "StopBits", ChipIcon = "SB", ContentList = new List<string>(serialPortParaEnum.StopBits) };
                _handshakeCfg = new SerialPortCfgModel { ChipContent = "HandShake", ChipIcon = "HS", ContentList = new List<string>(serialPortParaEnum.Handshake) };
                _thresholdCfg = new SerialPortCfgModel { ChipContent = "RcvBytesThreshold", ChipIcon = "TH", ContentList = new List<string>(serialPortParaEnum.ReceiveBytesThreshold) };
                _readTimeoutCfg = new SerialPortCfgModel { ChipContent = "ReadTimeout", ChipIcon = "RT", ContentList = new List<string>(serialPortParaEnum.ReadTimeout) };
                _writeTimeoutCfg = new SerialPortCfgModel { ChipContent = "WriteTimeout", ChipIcon = "WT", ContentList = new List<string>(serialPortParaEnum.WriteTimeout) };
                _readBufferSizeCfg = new SerialPortCfgModel { ChipContent = "ReadBufferSize", ChipIcon = "RS", ContentList = new List<string>(serialPortParaEnum.ReadBufferSize) };
                _writeBufferSizeCfg = new SerialPortCfgModel { ChipContent = "WriteBufferSize", ChipIcon = "WS", ContentList = new List<string>(serialPortParaEnum.WriteBufferSize) };

                SerialPortCfgCollection = new ObservableCollection<SerialPortCfgModel>
                {
                    _nameCfg,
                    _baudCfg,
                    _parityCfg,
                    _dataBitsCfg,
                    _stopBitsCfg,
                    _handshakeCfg,
                    _thresholdCfg,
                    _readTimeoutCfg,
                    _writeTimeoutCfg,
                    _readBufferSizeCfg,
                    _writeBufferSizeCfg
                };

            }
            catch (Exception ex)
            {

                InfoMessage = "Error: " + ex.Message.Replace("\n", "");

            }

        }

        private void SetSerialPortPara()
        {
            try
            {
                _serialPortHelper.SerialPort.PortName = _nameCfg.SelectedValue;
                _serialPortHelper.SerialPort.BaudRate = Convert.ToInt32(_baudCfg.SelectedValue);
                _serialPortHelper.SerialPort.Parity = (Parity)Enum.Parse(typeof(Parity), _parityCfg.SelectedValue);
                _serialPortHelper.SerialPort.DataBits = Convert.ToInt32(_dataBitsCfg.SelectedValue);
                _serialPortHelper.SerialPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), _stopBitsCfg.SelectedValue);
                _serialPortHelper.SerialPort.Handshake = (Handshake)Enum.Parse(typeof(Handshake), _handshakeCfg.SelectedValue);
                _serialPortHelper.SerialPort.ReceivedBytesThreshold = Convert.ToInt32(_thresholdCfg.SelectedValue);
                _serialPortHelper.SerialPort.ReadTimeout = Convert.ToInt32(_readTimeoutCfg.SelectedValue);
                _serialPortHelper.SerialPort.WriteTimeout = Convert.ToInt32(_writeTimeoutCfg.SelectedValue);
                _serialPortHelper.SerialPort.ReadBufferSize = Convert.ToInt32(_readBufferSizeCfg.SelectedValue);
                _serialPortHelper.SerialPort.WriteBufferSize = Convert.ToInt32(_writeBufferSizeCfg.SelectedValue);
            }
            catch (Exception ex)
            {

                InfoMessage = "Error: " + ex.Message.Replace("\n", "");
            }


        }

        private void LoadDefaultPara()
        {

            _nameCfg.SelectedValue = _serialPortHelper.SerialPort.PortName;
            _baudCfg.SelectedValue = _serialPortHelper.SerialPort.BaudRate.ToString();
            _parityCfg.SelectedValue = _serialPortHelper.SerialPort.Parity.ToString();
            _dataBitsCfg.SelectedValue = _serialPortHelper.SerialPort.DataBits.ToString();
            _stopBitsCfg.SelectedValue = _serialPortHelper.SerialPort.StopBits.ToString();
            _handshakeCfg.SelectedValue = _serialPortHelper.SerialPort.Handshake.ToString();
            _thresholdCfg.SelectedValue = _serialPortHelper.SerialPort.ReceivedBytesThreshold.ToString();
            _readTimeoutCfg.SelectedValue = _serialPortHelper.SerialPort.ReadTimeout.ToString();
            _writeTimeoutCfg.SelectedValue = _serialPortHelper.SerialPort.WriteTimeout.ToString();
            _readBufferSizeCfg.SelectedValue = _serialPortHelper.SerialPort.ReadBufferSize.ToString();
            _writeBufferSizeCfg.SelectedValue = _serialPortHelper.SerialPort.WriteBufferSize.ToString();
        }

        private CRCEnum GetCRCEnum()
        {
            if (CRC8)
            {
                return CRCEnum.CRC8;
            }
            else if (CRC16)
            {
                return CRCEnum.CRC16;

            }
            else if (CRC32)
            {
                return CRCEnum.CRC32;
            }
            else
            {
                return CRCEnum.None;
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

        private void ResetCount()
        {
            TxCount = 0;
            RxCount = 0;
            TxPieces = 0;
            RxPieces = 0;
        }

        private void InitializeLogFile()
        {
            _dirInfo = new DirectoryInfo("./Log/");
            if (!_dirInfo.Exists)
            {
                _dirInfo.Create();
            }

            _fileInfo = new FileInfo("./Log/SerialPortLog.txt");
            if (!_fileInfo.Exists)
            {
                _fileInfo.Create();
            }
            FilePath = _fileInfo.FullName;

        }

        private void AppendLogText(string message)
        {
            Task.Run(async () =>
            {
                using (StreamWriter writer = new StreamWriter(FilePath, true))
                {
                    writer.Write(message);
                }
                await Task.Delay(50);
            }, _cancellationTokenSource.Token);
        }

        //private void ReviewText()
        //{
        //    try
        //    {
        //        if (!string.IsNullOrEmpty(SendingText) && !string.IsNullOrWhiteSpace(SendingText))
        //        {
        //            if (GetDataFormatEnum() == DataFormatEnum.HEX)
        //            {
        //                _textValid = ToolHelper.ReviewHexString(SendingText);
        //                if (!_textValid)
        //                {
        //                    InfoMessage = "Warning: HEX String is invalid !";
        //                }
        //            }
        //            else
        //            {
        //                _textValid = true;
        //            }
        //        }
        //        else
        //        {
        //            _textValid = false;
        //        }

        //    }
        //    catch (Exception ex)
        //    {

        //        InfoMessage = "Error: " + ex.Message.Replace("\n", "");
        //    }

        //}


        #endregion
    }
}
