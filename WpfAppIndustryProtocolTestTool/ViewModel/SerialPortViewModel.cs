﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Timers;
using System.Windows;
using WpfAppIndustryProtocolTestTool.BLL;
using WpfAppIndustryProtocolTestTool.BLL.SerialPortProtocol;
using WpfAppIndustryProtocolTestTool.Model;
using WpfAppIndustryProtocolTestTool.Model.Enum;

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

        Timer _sendTimer;
        bool _textValid;

        string _workMode;
        string _gatewayMode;

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


        private string _receivedMessage;
        public string ReceivedMessage
        {
            get { return _receivedMessage; }
            set
            {
                if (_receivedMessage == value) { return; }
                _receivedMessage = value;
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
            }
        }

        private string _sendingMessage;
        public string SendingMessage
        {
            get { return _sendingMessage; }
            set
            {
                if (_sendingMessage == value) { return; }
                _sendingMessage = value;
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

        #endregion

        #region Command


        private RelayCommand _cmdloadDefaultPara;
        public RelayCommand CmdLoadDefaultPara
        {
            get
            {
                if (_cmdloadDefaultPara == null)
                {
                    _cmdloadDefaultPara = new RelayCommand(() => LoadDefaultPara());
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
                    _cmdClearReceiveArea = new RelayCommand(() => ReceivedMessage = string.Empty,
                                                            () => !string.IsNullOrWhiteSpace(ReceivedMessage) && !string.IsNullOrEmpty(ReceivedMessage));
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
                    _cmdClearSendArea = new RelayCommand(() => SendingMessage = string.Empty, () => !AutoSend);

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
                    _cmdSendMessage = new RelayCommand(() => SendMessage(), () => _textValid && IsOpen ? true : false);
                }
                return _cmdSendMessage;
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


        #endregion

        public SerialPortViewModel()
        {
            DisplayDateTime = true;
            PortOperation = "Open Port";

            InitSerialPortCfg();

            _serialPortHelper = SerialPortHelper.GetInstance();
            _serialPortHelper.ReceiveCompleted += _serialPortHelper_ReceiveCompleted;
            _serialPortHelper.SendCompleted += _serialPortHelper_SendCompleted;

            _sendTimer = new Timer();
            _sendTimer.Elapsed += (sender, e) =>
            {
                if (AutoSend)
                {
                    SendMessage();
                }
                else
                {
                    _sendTimer.Enabled = false;
                }

            };

            Messenger.Default.Register<string>(this, "WorkMode", (workMode) => _workMode = workMode);
            Messenger.Default.Register<string>(this, "GatewayMode", (msg) => _gatewayMode = msg);
            Messenger.Default.Register<string>(this, "EthernetPortInput", (msg) =>
            {
                SendingMessage = msg;
                SendMessage();
            });
            Messenger.Default.Register<string>(this, "Close", (msg) =>
            {
                if (IsOpen)
                {
                    OpenClosePort();
                }
            });

        }

        public override void Cleanup()
        {
            if (!IsOpen)
            {
                base.Cleanup();

            }
        }

        #region Command Methods

        private void OpenClosePort()
        {
            try
            {

                SetSerialPortPara();

                IsOpen = _serialPortHelper.SerialPort.IsOpen;

                if (!IsOpen)
                {

                    bool openResult = _serialPortHelper.OpenPort();
                    if (openResult)
                    {
                        PortOperation = "Close Port";
                        IsOpen = _serialPortHelper.SerialPort.IsOpen;
                        InfoMessage = "SerialPort " + _nameCfg.SelectedValue + "is Open !";
                    }
                }
                else if (IsOpen)
                {
                    bool closeResult = _serialPortHelper.ClosePort();
                    if (closeResult)
                    {
                        PortOperation = "Open Port";
                        IsOpen = _serialPortHelper.SerialPort.IsOpen;
                        InfoMessage = "SerialPort " + _nameCfg.SelectedValue + "is Closed !";
                    }
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

        private void ReviewText()
        {
            try
            {
                if (!string.IsNullOrEmpty(SendingMessage) && !string.IsNullOrWhiteSpace(SendingMessage))
                {
                    if (GetDataFormatEnum() == DataFormatEnum.HEX)
                    {
                        _textValid = ToolHelper.ReviewHexString(SendingMessage);
                        if (!_textValid)
                        {
                            InfoMessage = "Warning: HEX String is invalid !";
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

        private void SendMessage()
        {

            try
            {
                if (_sendTimer.Enabled == false && AutoSend)
                {
                    _sendTimer.Interval = int.Parse(SendCycleTime);
                    _sendTimer.Enabled = true;
                }

                string messageStr = ToolHelper.SetWordWrap(SendWithWordWrap) + ToolHelper.SetTime(SendWithDateTime, SendWithWordWrap) + SendingMessage;
                byte[] sendingMsg = ToolHelper.StringToByteArray(messageStr, GetDataFormatEnum());

                _serialPortHelper.SendData(sendingMsg, GetCRCEnum());

            }
            catch (Exception ex)
            {

                InfoMessage = "Error: " + ex.Message.Replace("\n", "");
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

                if (DisplayInRcvArea)
                {
                    ReceivedMessage += $"{ToolHelper.SetTime(true, false)}Tx {TxPieces} -> {ToolHelper.ByteArrayToString(sndArray, GetDataFormatEnum())}";
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

                    if (DisplayInRcvArea)
                    {
                        ReceivedMessage += $"{ToolHelper.SetTime(true, false)}Rx {RxPieces} -> {ToolHelper.ByteArrayToString(rcvArray, GetDataFormatEnum())}";
                    }
                    else
                    {
                        byte[] actualData = CRCHelper.ValidateCRC(rcvArray, GetCRCEnum());

                        ReceivedMessage += $"{ ToolHelper.SetWordWrap(ReceiveWordWrap)}{ ToolHelper.SetTime(DisplayDateTime, ReceiveWordWrap)}{ ToolHelper.ByteArrayToString(actualData, GetDataFormatEnum())}";

                        if (_workMode == "Gateway" && _gatewayMode == "Serial Port --> TCP/UDP")
                        {
                            string newMessage = ToolHelper.ByteArrayToString(actualData, GetDataFormatEnum());
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
                _readBufferSizeCfg = new SerialPortCfgModel { ChipContent = "ReadBufferSizeCfg", ChipIcon = "RS", ContentList = new List<string>(serialPortParaEnum.ReadBufferSize) };
                _writeBufferSizeCfg = new SerialPortCfgModel { ChipContent = "WriteBufferSizeCfg", ChipIcon = "WS", ContentList = new List<string>(serialPortParaEnum.WriteBufferSize) };

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

        #endregion
    }
}