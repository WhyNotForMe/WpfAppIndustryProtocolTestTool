using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Net;
using EasyModbus;
using WpfAppIndustryProtocolTestTool.BLL;
using WpfAppIndustryProtocolTestTool.BLL.ModbusProtocol;
using WpfAppIndustryProtocolTestTool.Model;
using WpfAppIndustryProtocolTestTool.Model.Enum;
using System.Timers;
using GalaSoft.MvvmLight.Messaging;
using System.Threading.Tasks;
using System.Threading;

namespace WpfAppIndustryProtocolTestTool.ViewModel
{
    public class ModbusViewModel : ViewModelBase
    {

        #region Fields
        SerialPortCfgModel? _nameCfg;
        SerialPortCfgModel? _baudCfg;
        SerialPortCfgModel? _parityCfg;
        SerialPortCfgModel? _dataBitsCfg;
        SerialPortCfgModel? _stopBitsCfg;
        SerialPort? _serialPort;

        ModbusClient? _masterClient;
        ModbusSlaveServer? _slaveServer;

        ushort _rxCount;
        ushort _txCount;

        System.Timers.Timer _sendTimer;

        CancellationTokenSource _cancellationTokenSource;

        #endregion

        #region UI-> Source
        public bool MasterOrClientMode { get; set; }
        public bool SlaveOrServerMode { get; set; }
        public bool ModbusRTU { get; set; }
        public bool ModbusTCP { get; set; }

        public string IpAddress { get; set; }
        public int Port { get; set; }
        public int ConnectionTimeout { get; set; }
        public string RemoteIpAddress { get; set; }
        public int RemotePort { get; set; }

        public byte SlaveID { get; set; }
        public List<string> SlaveFunctionList { get; set; }
        public int SelectedSlaveFunction { get; set; }
        public ushort StartAddress { get; set; }
        public List<string> MasterDataTypeList { get; set; }
        public List<string> SlaveDataTypeList { get; set; }
        public int SelectedDataType { get; set; }
        public List<string> MasterFunctionList { get; set; }
        public int SelectedMasterFunction { get; set; }
        public List<string> DataFormatList { get; set; }
        public int SelectedDataFormat { get; set; }

        public bool RegisterOrderSwap { get; set; }
        public bool DisplayTime { get; set; }
        public bool AutoSend { get; set; }
        public string SendCycleTime { get; set; }

        #endregion

        #region Source -> UI

        public ObservableCollection<SerialPortCfgModel> SerialPortCfgCollection { get; set; }
        public ObservableCollection<ModbusRegisterModel> RegisterDataCollection { get; set; }

        private ushort _registerQuantity;
        public ushort RegisterQuantity
        {
            get { return _registerQuantity; }
            set
            {
                if (_registerQuantity == value) { return; }
                _registerQuantity = value;
                RaisePropertyChanged();
            }
        }

        private string _openOrClosePort;
        public string OpenOrClosePort
        {
            get { return _openOrClosePort; }
            set
            {
                if (_openOrClosePort == value) { return; }
                _openOrClosePort = value;
                RaisePropertyChanged();
            }
        }


        private string _startOrStop;
        public string StartOrStop
        {
            get { return _startOrStop; }
            set
            {
                if (_startOrStop == value) { return; }
                _startOrStop = value;
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
                Messenger.Default.Send<bool>(_isRunning, "Modbus");
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


        private string _rawTelegraph;
        public string RawTelegraph
        {
            get { return _rawTelegraph; }
            set
            {
                if (_rawTelegraph == value) { return; }
                _rawTelegraph = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Command


        private RelayCommand _cmdLoadDefaultPara;
        public RelayCommand CmdLoadDefaultPara
        {
            get
            {
                if (_cmdLoadDefaultPara == null)
                {
                    _cmdLoadDefaultPara = new RelayCommand(() => ExeLoadDefaultPara());
                }
                return _cmdLoadDefaultPara;
            }
        }


        private RelayCommand _cmdOpenOrClosePort;
        public RelayCommand CmdOpenOrClosePort
        {
            get
            {
                if (_cmdOpenOrClosePort == null)
                {
                    _cmdOpenOrClosePort = new RelayCommand(() => ExeOpenOrClosePort(), () => CanOpenOrClosePort());
                }
                return _cmdOpenOrClosePort;
            }
        }



        private RelayCommand _cmdStartOrStop;
        public RelayCommand CmdStartOrStop
        {
            get
            {
                if (_cmdStartOrStop == null)
                {
                    _cmdStartOrStop = new RelayCommand(() => ExeStartOrStop(), () => CanStartOrStop());
                }
                return _cmdStartOrStop;
            }
        }


        private RelayCommand _cmdClearDataGrid;
        public RelayCommand CmdClearDataGrid
        {
            get
            {
                if (_cmdClearDataGrid == null)
                {
                    _cmdClearDataGrid = new RelayCommand(() => RegisterDataCollection.Clear());
                }
                return _cmdClearDataGrid;
            }
        }


        private RelayCommand _cmdInitDataGrid;
        public RelayCommand CmdInitDataGrid
        {
            get
            {
                if (_cmdInitDataGrid == null)
                {
                    _cmdInitDataGrid = new RelayCommand(() => ExeInitDataGrid(), () => CanInitDataGrid());
                }
                return _cmdInitDataGrid;
            }
        }


        private RelayCommand _cmdUpdateRegister;
        public RelayCommand CmdUpdateRegister
        {
            get
            {
                if (_cmdUpdateRegister == null)
                {
                    _cmdUpdateRegister = new RelayCommand(() => ExeUpdateRegister());
                }
                return _cmdUpdateRegister;
            }
        }



        private RelayCommand _cmdClearTelegraph;
        public RelayCommand CmdClearTelegraph
        {
            get
            {
                if (_cmdClearTelegraph == null)
                {
                    _cmdClearTelegraph = new RelayCommand(() => RawTelegraph = string.Empty, () => !AutoSend);
                }
                return _cmdClearTelegraph;
            }
        }


        private RelayCommand _cmdSendCommand;
        public RelayCommand CmdSendCommand
        {
            get
            {
                if (_cmdSendCommand == null)
                {
                    _cmdSendCommand = new RelayCommand(() => ExeSendCommand(), () => CanSendCommand());
                }
                return _cmdSendCommand;
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

        #endregion


        public ModbusViewModel()
        {

            MasterOrClientMode = true;
            ModbusRTU = true;

            OpenOrClosePort = "Open Port";
            StartOrStop = "Start";

            Port = 502;
            ConnectionTimeout = 6000;

            SlaveID = 1;
            RegisterQuantity = 8;

            InitSerialPortPara();

            InitSlaveFunction();
            InitMasterFunction();
            InitDataType();

            RegisterDataCollection = new ObservableCollection<ModbusRegisterModel>();

            _sendTimer = new System.Timers.Timer();
            _sendTimer.Elapsed += (sender, e) =>
            {
                if (AutoSend)
                {
                    ExeSendCommand();
                }
                else
                {
                    _sendTimer.Enabled = false;
                }
            };

            Messenger.Default.Register<string>(this, "Close", (msg) =>
            {
                if (msg == "CloseConnection")
                {
                    if (ModbusRTU && IsRunning)
                    {
                        ExeOpenOrClosePort();
                    }
                    else if (ModbusTCP && IsRunning)
                    {
                        ExeStartOrStop();
                    }
                }
            });
            _cancellationTokenSource = new CancellationTokenSource();

        }

        public override void Cleanup()
        {
            _cancellationTokenSource.Cancel();
            base.Cleanup();
        }

        #region Command Methods


        private void ExeLoadDefaultPara()
        {

            _nameCfg.SelectedValue = _serialPort.PortName;
            _baudCfg.SelectedValue = _serialPort.BaudRate.ToString();
            _parityCfg.SelectedValue = _serialPort.Parity.ToString();
            _dataBitsCfg.SelectedValue = _serialPort.DataBits.ToString();
            _stopBitsCfg.SelectedValue = _serialPort.StopBits.ToString();
        }

        private void ExeOpenOrClosePort()
        {
            try
            {

                if (MasterOrClientMode)
                {
                    if (OpenOrClosePort == "Open Port")
                    {
                        _masterClient = new ModbusClient(_nameCfg.SelectedValue)
                        {
                            Baudrate = Convert.ToInt32(_baudCfg.SelectedValue),
                            Parity = (Parity)Enum.Parse(typeof(Parity), _parityCfg.SelectedValue),
                            StopBits = (StopBits)Enum.Parse(typeof(StopBits), _stopBitsCfg.SelectedValue)
                        };
                        _masterClient.ConnectedChanged += _masterClient_ConnectedChanged;
                        _masterClient.ReceiveDataChanged += _masterClient_ReceiveDataChanged;
                        _masterClient.SendDataChanged += _masterClient_SendDataChanged;

                        _masterClient.Connect();
                        OpenOrClosePort = "Close Port";
                    }
                    else if (OpenOrClosePort == "Close Port")
                    {
                        _masterClient.Disconnect();
                        OpenOrClosePort = "Open Port";

                    }
                }
                else if (SlaveOrServerMode)
                {
                    if (OpenOrClosePort == "Open Port")
                    {
                        _slaveServer = new ModbusSlaveServer()
                        {
                            SerialPort = _nameCfg.SelectedValue,
                            Baudrate = Convert.ToInt32(_baudCfg.SelectedValue),
                            Parity = (Parity)Enum.Parse(typeof(Parity), _parityCfg.SelectedValue),
                            StopBits = (StopBits)Enum.Parse(typeof(StopBits), _stopBitsCfg.SelectedValue),
                            SerialFlag = true
                        };
                        _slaveServer.CoilsChanged += _slaveServer_CoilsChanged;
                        _slaveServer.HoldingRegistersChanged += _slaveServer_HoldingRegistersChanged;
                        _slaveServer.ReceiveCompleted += _slaveServer_ReceiveCompleted;
                        _slaveServer.SendCompleted += _slaveServer_SendCompleted;

                        _slaveServer.Listen();
                        IsRunning = true;
                        OpenOrClosePort = "Close Port";
                    }
                    else if (OpenOrClosePort == "Close Port")
                    {
                        _slaveServer.StopListening();
                        IsRunning = false;
                        OpenOrClosePort = "Open Port";

                    }
                }


            }
            catch (Exception ex)
            {

                InfoMessage = "Error: " + ex.Message.Replace("\n", "");

            }
        }

        private bool CanOpenOrClosePort()
        {

            if (OpenOrClosePort == "Open Port")
            {
                if (!IsRunning)
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
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }



        }

        private void ExeStartOrStop()
        {
            try
            {
                if (MasterOrClientMode)
                {
                    if (StartOrStop == "Start")
                    {
                        _masterClient = new ModbusClient(IpAddress, Port);
                        _masterClient.ConnectedChanged += _masterClient_ConnectedChanged;
                        _masterClient.ReceiveDataChanged += _masterClient_ReceiveDataChanged;
                        _masterClient.SendDataChanged += _masterClient_SendDataChanged;

                        _masterClient.Connect();
                        StartOrStop = "Stop Client";
                    }
                    else if (StartOrStop == "Stop Client")
                    {
                        _masterClient.Disconnect();
                        StartOrStop = "Start";

                    }

                }
                else if (SlaveOrServerMode)
                {
                    if (StartOrStop == "Start")
                    {
                        _slaveServer = new ModbusSlaveServer()
                        {
                            LocalIPAddress = IPAddress.Parse(IpAddress),
                            Port = Port,
                            UDPFlag = false,
                            FunctionCode23Disabled = true
                        };
                        _slaveServer.CoilsChanged += _slaveServer_CoilsChanged;
                        _slaveServer.HoldingRegistersChanged += _slaveServer_HoldingRegistersChanged;
                        _slaveServer.ReceiveCompleted += _slaveServer_ReceiveCompleted;
                        _slaveServer.SendCompleted += _slaveServer_SendCompleted;

                        _slaveServer.Listen();
                        IsRunning = true;
                        StartOrStop = "Stop Server";
                    }
                    else if (StartOrStop == "Stop Server")
                    {
                        _slaveServer.StopListening();
                        IsRunning = false;
                        StartOrStop = "Start";

                    }
                }

            }
            catch (Exception ex)
            {
                InfoMessage = "Error: " + ex.Message.Replace("\n", "");
            }

        }

        private bool CanStartOrStop()
        {
            if (StartOrStop == "Start")
            {
                if (!IsRunning)
                {
                    if (!string.IsNullOrEmpty(IpAddress) && !string.IsNullOrWhiteSpace(IpAddress) && Port > 0 && ConnectionTimeout > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }

        }

        private void ExeInitDataGrid()
        {
            RegisterDataCollection.Clear();
            if (SlaveOrServerMode)
            {

                Array.Clear(_slaveServer.coils.localArray, 0, 65535);
                Array.Clear(_slaveServer.discreteInputs.localArray, 0, 65535);
                Array.Clear(_slaveServer.inputRegisters.localArray, 0, 65535);
                Array.Clear(_slaveServer.holdingRegisters.localArray, 0, 65535);

            }

            if (MasterOrClientMode && (SelectedMasterFunction == 4 || SelectedMasterFunction == 5))
            {
                RegisterQuantity = 1;
                RegisterDataCollection.Add(new ModbusRegisterModel
                {
                    Address = StartAddress,
                    AddressRange = StartAddress.ToString(),
                    RegisterSize = 1,
                    Value = SelectedDataType == 0 ? "False" : "0"

                });
            }
            else
            {
                byte size = MasterOrClientMode ? GetMasterRegisterSize() : GetSlaveRegisterSize();

                for (ushort i = StartAddress; i < StartAddress + RegisterQuantity; i += size)
                {
                    RegisterDataCollection.Add(new ModbusRegisterModel
                    {
                        Address = i,
                        AddressRange = size == 1 ? i.ToString() : $"{i} -- {i + size - 1}",
                        RegisterSize = size,
                        Value = SelectedDataType == 0 ? "False" : "0"

                    });

                }


            }


            InfoMessage = "Notice: Initialize Done!";
        }

        private bool CanInitDataGrid()
        {
            if (IsRunning)
            {
                if (((SlaveOrServerMode && (SelectedSlaveFunction == 0 || SelectedSlaveFunction == 1)) ||
                    (MasterOrClientMode && (SelectedMasterFunction == 0 || SelectedMasterFunction == 1 ||
                            SelectedMasterFunction == 4 || SelectedMasterFunction == 6))) && SelectedDataType > 0)
                {
                    InfoMessage = "Warning: The Data Type must be Bool !";
                    return false;
                }
                else if (MasterOrClientMode && SelectedMasterFunction == 5 && SelectedDataType > 2)
                {
                    InfoMessage = "Warning: The Data Size must be 16bits !";
                    return false;
                }
                else if (SlaveID < 1 || StartAddress < 0 || RegisterQuantity < 1)
                {
                    InfoMessage = "Warning: Please input valid parameter !";
                    return false;
                }
                else if (StartAddress + RegisterQuantity - 1 > ushort.MaxValue)
                {
                    InfoMessage = "Warning: The last Register Address must be less than 65536 !";
                    return false;
                }
                else if (StartAddress > ushort.MaxValue)
                {
                    InfoMessage = "Warning: The Start Address must be less than 65536 !";
                    return false;
                }
                else if ((MasterOrClientMode && (RegisterQuantity % GetMasterRegisterSize() != 0)) ||
                                (SlaveOrServerMode && (RegisterQuantity % GetSlaveRegisterSize() != 0)))
                {
                    //InfoMessage = "Warning: The Register Quantity is invalid because of selected Data Type !";
                    return false;
                }
                else if (RegisterQuantity > 2000 && (SelectedSlaveFunction <= 1 || SelectedMasterFunction <= 1))
                {
                    InfoMessage = "Warning: The Register Quantity must be less than 2001 !";
                    return false;
                }
                else if (RegisterQuantity > 125 && (SelectedSlaveFunction > 1 || SelectedMasterFunction > 1))
                {
                    InfoMessage = "Warning: The Register Quantity must be less than 126 !";
                    return false;
                }
                else if (RegisterQuantity > 1968 && SelectedMasterFunction == 6)
                {
                    InfoMessage = "Warning: The Register Quantity must be less than 1969 !";
                    return false;
                }
                else if (RegisterQuantity > 123 && SelectedMasterFunction == 7)
                {
                    InfoMessage = "Warning: The Register Quantity must be less than 124 !";
                    return false;
                }
                else if (SlaveOrServerMode && SelectedSlaveFunction > 1 && SelectedDataType == 0)
                {
                    InfoMessage = "Warning: The Register can not be used for Bool Type  !";
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        private void ExeSendCommand()
        {
            try
            {
                if (_sendTimer.Enabled == false && AutoSend)
                {
                    _sendTimer.Interval = int.Parse(SendCycleTime);
                    _sendTimer.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                InfoMessage = "Error: " + ex.Message.Replace("\n", "");
            }


            bool[] boolValueArray;
            int[] registerValueArray;
            bool newBoolValue;
            int[] intValueArray;
            ModbusClient.RegisterOrder registerOrder = GetRegisterOrder();
            Task.Run(() =>
            {
                try
                {
                    switch (SelectedMasterFunction)
                    {
                        //0 : Fn01_ReadCoils_0x,
                        case (int)ModbusMasterFunctionEnum.Fn01_ReadCoils_0x:
                            boolValueArray = _masterClient?.ReadCoils(StartAddress, RegisterQuantity);
                            for (int i = 0; i < boolValueArray.Length; i++)
                            {
                                App.Current.Dispatcher.Invoke(() => RegisterDataCollection[i].Value = boolValueArray[i].ToString());
                            }
                            break;
                        //1 : Fn02_ReadDiscreteInputs_1x,
                        case (int)ModbusMasterFunctionEnum.Fn02_ReadDiscreteInputs_1x:
                            boolValueArray = _masterClient?.ReadDiscreteInputs(StartAddress, RegisterQuantity);
                            for (int i = 0; i < boolValueArray.Length; i++)
                            {
                                App.Current.Dispatcher.Invoke(() => RegisterDataCollection[i].Value = boolValueArray[i].ToString());
                            }
                            break;
                        //2 : Fn03_ReadHoldingRegisters_4x,
                        case (int)ModbusMasterFunctionEnum.Fn03_ReadHoldingRegisters_4x:
                            registerValueArray = _masterClient.ReadHoldingRegisters(StartAddress, RegisterQuantity);
                            ConvertRegisterValue(registerValueArray, registerOrder);
                            break;
                        //3 : Fn04_ReadInputRegisters_3x,
                        case (int)ModbusMasterFunctionEnum.Fn04_ReadInputRegisters_3x:
                            registerValueArray = _masterClient.ReadInputRegisters(StartAddress, RegisterQuantity);
                            ConvertRegisterValue(registerValueArray, registerOrder);
                            break;
                        //4 : Fn05_WriteSingleCoil_0x,
                        case (int)ModbusMasterFunctionEnum.Fn05_WriteSingleCoil_0x:
                            newBoolValue = Convert.ToBoolean(RegisterDataCollection[0].Value);
                            _masterClient.WriteSingleCoil(StartAddress, newBoolValue);
                            break;
                        //5 : Fn06_WriteSingleRegister_4x,
                        case (int)ModbusMasterFunctionEnum.Fn06_WriteSingleRegister_4x:
                            switch (SelectedDataType)
                            {
                                case (int)ModbusMasterDataTypeEnum.Bool_TrueFalse:
                                    short boolValue = Convert.ToInt16(Convert.ToBoolean(RegisterDataCollection[0].Value));
                                    _masterClient.WriteSingleRegister(StartAddress, boolValue);
                                    break;

                                case (int)ModbusMasterDataTypeEnum.Signed_16bits:
                                    short shortValue = Convert.ToInt16(RegisterDataCollection[0].Value);
                                    _masterClient.WriteSingleRegister(StartAddress, shortValue);
                                    break;
                                case (int)ModbusMasterDataTypeEnum.Unsigned_16bits:
                                    ushort ushortValue = Convert.ToUInt16(RegisterDataCollection[0].Value);
                                    _masterClient.WriteSingleRegister(StartAddress, ushortValue);
                                    break;
                                default:

                                    break;
                            }
                            break;
                        //6 : Fn0F_WriteMultipleCoils_0x,
                        case (int)ModbusMasterFunctionEnum.Fn15_WriteMultipleCoils_0x:
                            boolValueArray = new bool[RegisterDataCollection.Count];
                            for (int i = 0; i < RegisterDataCollection.Count; i++)
                            {
                                boolValueArray[i] = Convert.ToBoolean(RegisterDataCollection[i]);
                            }
                            _masterClient.WriteMultipleCoils(StartAddress, boolValueArray);
                            break;
                        //7 : Fn10_WriteMultipleRegisters_4x
                        case (int)ModbusMasterFunctionEnum.Fn16_WriteMultipleRegisters_4x:
                            intValueArray = new int[RegisterQuantity];
                            int[] rcvIntArray;
                            switch (SelectedDataType)
                            {
                                //0: Bool_TrueFalse,
                                case (int)ModbusMasterDataTypeEnum.Bool_TrueFalse:
                                    for (int i = 0; i < RegisterQuantity; i++)
                                    {
                                        intValueArray[i] = Convert.ToInt16(Convert.ToBoolean(RegisterDataCollection[i].Value));
                                    }
                                    _masterClient.WriteMultipleRegisters(StartAddress, intValueArray);
                                    break;
                                //1: Signed_16bits,
                                case (int)ModbusMasterDataTypeEnum.Signed_16bits:
                                    for (int i = 0; i < RegisterQuantity; i++)
                                    {
                                        intValueArray[i] = Convert.ToInt16(RegisterDataCollection[i].Value);
                                    }
                                    _masterClient.WriteMultipleRegisters(StartAddress, intValueArray);
                                    break;
                                //2 :Unsigned_16bits,
                                case (int)ModbusMasterDataTypeEnum.Unsigned_16bits:
                                    for (int i = 0; i < RegisterQuantity; i++)
                                    {
                                        intValueArray[i] = Convert.ToUInt16(RegisterDataCollection[i].Value);
                                    }
                                    _masterClient.WriteMultipleRegisters(StartAddress, intValueArray);
                                    break;
                                //3 :Signed_32bits,
                                case (int)ModbusMasterDataTypeEnum.Signed_32bits:
                                    for (int i = 0; i < RegisterQuantity; i += 2)
                                    {
                                        rcvIntArray = ModbusClient.ConvertIntToRegisters(Convert.ToInt32(RegisterDataCollection[i / 2].Value), registerOrder);
                                        intValueArray[i] = rcvIntArray[0];
                                        intValueArray[i + 1] = rcvIntArray[1];
                                    }
                                    _masterClient.WriteMultipleRegisters(StartAddress, intValueArray);

                                    break;
                                //4: Unsigned_32bits,
                                case (int)ModbusMasterDataTypeEnum.Unsigned_32bits:
                                    for (int i = 0; i < RegisterQuantity; i += 2)
                                    {
                                        rcvIntArray = ModbusClient.ConvertLongToRegisters(Convert.ToInt64(RegisterDataCollection[i / 2].Value), registerOrder);
                                        intValueArray[i] = rcvIntArray[0];
                                        intValueArray[i + 1] = rcvIntArray[1];
                                    }
                                    _masterClient.WriteMultipleRegisters(StartAddress, intValueArray);

                                    break;
                                //5: Float_32bits,
                                case (int)ModbusMasterDataTypeEnum.Float_32bits:
                                    for (int i = 0; i < RegisterQuantity; i += 2)
                                    {
                                        rcvIntArray = ModbusClient.ConvertFloatToRegisters(Convert.ToSingle(RegisterDataCollection[i / 2].Value), registerOrder);
                                        intValueArray[i] = rcvIntArray[0];
                                        intValueArray[i + 1] = rcvIntArray[1];
                                    }
                                    _masterClient.WriteMultipleRegisters(StartAddress, intValueArray);

                                    break;
                                //6: Signed_64bits,
                                case (int)ModbusMasterDataTypeEnum.Signed_64bits:
                                    for (int i = 0; i < RegisterQuantity; i += 4)
                                    {
                                        rcvIntArray = ModbusClient.ConvertLongToRegisters(Convert.ToInt64(RegisterDataCollection[i / 4].Value), registerOrder);
                                        intValueArray[i] = rcvIntArray[0];
                                        intValueArray[i + 1] = rcvIntArray[1];
                                        intValueArray[i + 2] = rcvIntArray[2];
                                        intValueArray[i + 3] = rcvIntArray[3];
                                    }
                                    _masterClient.WriteMultipleRegisters(StartAddress, intValueArray);

                                    break;
                                //7: Double_64bits
                                case (int)ModbusMasterDataTypeEnum.Double_64bits:
                                    for (int i = 0; i < RegisterQuantity; i += 4)
                                    {
                                        rcvIntArray = ModbusClient.ConvertDoubleToRegisters(Convert.ToDouble(RegisterDataCollection[i / 4].Value), registerOrder);
                                        intValueArray[i] = rcvIntArray[0];
                                        intValueArray[i + 1] = rcvIntArray[1];
                                        intValueArray[i + 2] = rcvIntArray[2];
                                        intValueArray[i + 3] = rcvIntArray[3];
                                    }
                                    _masterClient.WriteMultipleRegisters(StartAddress, intValueArray);

                                    break;
                                //None Operation !
                                default:
                                    break;
                            }
                            break;
                    }

                }
                catch (Exception ex)
                {
                    InfoMessage = "Error: " + ex.Message.Replace("\n", "");
                }

            }, _cancellationTokenSource.Token);

        }

        private bool CanSendCommand()
        {
            return RegisterDataCollection.Count > 0;
        }

        private void ExeUpdateRegister()
        {

            _slaveServer.UnitIdentifier = SlaveID;

            int[] rcvIntArray;
            ModbusClient.RegisterOrder registerOrder = GetRegisterOrder();

            Task.Run(() =>
            {
                try
                {
                    switch (SelectedSlaveFunction)
                    {
                        //0: Coils_0x,
                        case (int)ModbusSlaveFunctionEnum.Coils_0x:
                            for (int i = 0; i < RegisterQuantity; i++)
                            {
                                _slaveServer.coils[StartAddress + i + 1] = Convert.ToBoolean(RegisterDataCollection[i].Value);
                            }
                            break;

                        //1: DiscreteInputs_1x,
                        case (int)ModbusSlaveFunctionEnum.DiscreteInputs_1x:
                            for (int i = 0; i < RegisterQuantity; i++)
                            {
                                _slaveServer.discreteInputs[StartAddress + i + 1] = Convert.ToBoolean(RegisterDataCollection[i].Value);
                            }
                            break;

                        //2: HoldingRegisters_4x,
                        case (int)ModbusSlaveFunctionEnum.HoldingRegisters_4x:

                            switch (SelectedDataType)
                            {

                                //1: Signed_16bits,
                                case (int)ModbusSlaveDataTypeEnum.Signed_16bits:
                                    for (int i = 0; i < RegisterQuantity; i++)
                                    {
                                        _slaveServer.holdingRegisters[StartAddress + i + 1] = Convert.ToInt16(RegisterDataCollection[i].Value);
                                    }
                                    break;

                                //2 :Signed_32bits,
                                case (int)ModbusSlaveDataTypeEnum.Signed_32bits:
                                    for (int i = 0; i < RegisterQuantity; i += 2)
                                    {
                                        rcvIntArray = ModbusClient.ConvertIntToRegisters(Convert.ToInt32(RegisterDataCollection[i / 2].Value), registerOrder);
                                        _slaveServer.holdingRegisters[StartAddress + i + 1] = Convert.ToInt16(rcvIntArray[0]);
                                        _slaveServer.holdingRegisters[StartAddress + i + 2] = Convert.ToInt16(rcvIntArray[1]);
                                    }
                                    break;

                                //3: Float_32bits,
                                case (int)ModbusSlaveDataTypeEnum.Float_32bits:
                                    for (int i = 0; i < RegisterQuantity; i += 2)
                                    {
                                        rcvIntArray = ModbusClient.ConvertFloatToRegisters(Convert.ToSingle(RegisterDataCollection[i / 2].Value), registerOrder);
                                        _slaveServer.holdingRegisters[StartAddress + i + 1] = ConvertInt32ToInt16(rcvIntArray[0]);
                                        _slaveServer.holdingRegisters[StartAddress + i + 2] = ConvertInt32ToInt16(rcvIntArray[1]);

                                    }
                                    break;

                                //4: Signed_64bits,
                                case (int)ModbusSlaveDataTypeEnum.Signed_64bits:
                                    for (int i = 0; i < RegisterQuantity; i += 4)
                                    {
                                        rcvIntArray = ModbusClient.ConvertLongToRegisters(Convert.ToInt64(RegisterDataCollection[i / 4].Value), registerOrder);
                                        _slaveServer.holdingRegisters[StartAddress + i + 1] = ConvertInt32ToInt16(rcvIntArray[0]);
                                        _slaveServer.holdingRegisters[StartAddress + i + 2] = ConvertInt32ToInt16(rcvIntArray[1]);
                                        _slaveServer.holdingRegisters[StartAddress + i + 3] = ConvertInt32ToInt16(rcvIntArray[2]);
                                        _slaveServer.holdingRegisters[StartAddress + i + 4] = ConvertInt32ToInt16(rcvIntArray[3]);
                                    }
                                    break;

                                //5: Double_64bits
                                case (int)ModbusSlaveDataTypeEnum.Double_64bits:
                                    for (int i = 0; i < RegisterQuantity; i += 4)
                                    {
                                        rcvIntArray = ModbusClient.ConvertDoubleToRegisters(Convert.ToDouble(RegisterDataCollection[i / 4].Value), registerOrder);
                                        _slaveServer.holdingRegisters[StartAddress + i + 1] = ConvertInt32ToInt16(rcvIntArray[0]);
                                        _slaveServer.holdingRegisters[StartAddress + i + 2] = ConvertInt32ToInt16(rcvIntArray[1]);
                                        _slaveServer.holdingRegisters[StartAddress + i + 3] = ConvertInt32ToInt16(rcvIntArray[2]);
                                        _slaveServer.holdingRegisters[StartAddress + i + 4] = ConvertInt32ToInt16(rcvIntArray[3]);
                                    }
                                    break;
                                //None Operation !
                                default:

                                    break;
                            }
                            break;

                        //3: InputRegisters_3x
                        case (int)ModbusSlaveFunctionEnum.InputRegisters_3x:

                            switch (SelectedDataType)
                            {
                                //1: Signed_16bits,
                                case (int)ModbusSlaveDataTypeEnum.Signed_16bits:
                                    for (int i = 0; i < RegisterQuantity; i++)
                                    {
                                        _slaveServer.inputRegisters[StartAddress + i + 1] = Convert.ToInt16(RegisterDataCollection[i].Value);
                                    }
                                    break;

                                //2 :Signed_32bits,
                                case (int)ModbusSlaveDataTypeEnum.Signed_32bits:
                                    for (int i = 0; i < RegisterQuantity; i += 2)
                                    {
                                        rcvIntArray = ModbusClient.ConvertIntToRegisters(Convert.ToInt32(RegisterDataCollection[i / 2].Value), registerOrder);
                                        _slaveServer.inputRegisters[StartAddress + i + 1] = ConvertInt32ToInt16(rcvIntArray[0]);
                                        _slaveServer.inputRegisters[StartAddress + i + 2] = ConvertInt32ToInt16(rcvIntArray[1]);
                                    }
                                    break;

                                //3: Float_32bits,
                                case (int)ModbusSlaveDataTypeEnum.Float_32bits:
                                    for (int i = 0; i < RegisterQuantity; i += 2)
                                    {
                                        rcvIntArray = ModbusClient.ConvertFloatToRegisters(Convert.ToSingle(RegisterDataCollection[i / 2].Value), registerOrder);
                                        _slaveServer.inputRegisters[StartAddress + i + 1] = ConvertInt32ToInt16(rcvIntArray[0]);
                                        _slaveServer.inputRegisters[StartAddress + i + 2] = ConvertInt32ToInt16(rcvIntArray[1]);
                                    }

                                    break;

                                //4: Signed_64bits,
                                case (int)ModbusSlaveDataTypeEnum.Signed_64bits:
                                    for (int i = 0; i < RegisterQuantity; i += 4)
                                    {
                                        rcvIntArray = ModbusClient.ConvertLongToRegisters(Convert.ToInt64(RegisterDataCollection[i / 4].Value), registerOrder);
                                        _slaveServer.inputRegisters[StartAddress + i + 1] = ConvertInt32ToInt16(rcvIntArray[0]);
                                        _slaveServer.inputRegisters[StartAddress + i + 2] = ConvertInt32ToInt16(rcvIntArray[1]);
                                        _slaveServer.inputRegisters[StartAddress + i + 3] = ConvertInt32ToInt16(rcvIntArray[2]);
                                        _slaveServer.inputRegisters[StartAddress + i + 4] = ConvertInt32ToInt16(rcvIntArray[3]);
                                    }
                                    break;

                                //5: Double_64bits
                                case (int)ModbusSlaveDataTypeEnum.Double_64bits:
                                    for (int i = 0; i < RegisterQuantity; i += 4)
                                    {
                                        rcvIntArray = ModbusClient.ConvertDoubleToRegisters(Convert.ToDouble(RegisterDataCollection[i / 4].Value), registerOrder);
                                        _slaveServer.inputRegisters[StartAddress + i + 1] = ConvertInt32ToInt16(rcvIntArray[0]);
                                        _slaveServer.inputRegisters[StartAddress + i + 2] = ConvertInt32ToInt16(rcvIntArray[1]);
                                        _slaveServer.inputRegisters[StartAddress + i + 3] = ConvertInt32ToInt16(rcvIntArray[2]);
                                        _slaveServer.inputRegisters[StartAddress + i + 4] = ConvertInt32ToInt16(rcvIntArray[3]);
                                    }
                                    break;

                                //None Operation !
                                default:
                                    break;
                            }
                            break;

                        //None
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    InfoMessage = "Error: " + ex.Message.Replace("\n", "");
                }

            }, _cancellationTokenSource.Token);

        }


        #endregion

        #region Shared Methods

        private short ConvertInt32ToInt16(int rcvInt)
        {
            byte[] rcvBytes = BitConverter.GetBytes(rcvInt);
            byte[] shortBytes = new byte[2] { rcvBytes[0], rcvBytes[1] };
            return BitConverter.ToInt16(shortBytes, 0);
        }

        private ModbusClient.RegisterOrder GetRegisterOrder()
        {
            return RegisterOrderSwap ? ModbusClient.RegisterOrder.LowHigh : ModbusClient.RegisterOrder.HighLow;
        }

        private void ConvertRegisterValue(int[] registerValueArray, ModbusClient.RegisterOrder registerOrder)
        {

            int[] intValueArray;

            switch (SelectedDataType)
            {
                case (int)ModbusMasterDataTypeEnum.Signed_16bits:
                case (int)ModbusMasterDataTypeEnum.Unsigned_16bits:
                    for (int i = 0; i < registerValueArray.Length; i++)
                    {
                        App.Current.Dispatcher.Invoke(() => RegisterDataCollection[i].Value = registerValueArray[i].ToString());
                    }
                    break;

                case (int)ModbusMasterDataTypeEnum.Signed_32bits:
                    for (int i = 0; i < registerValueArray.Length; i += 2)
                    {
                        intValueArray = new int[2] { registerValueArray[i], registerValueArray[i + 1] };
                        App.Current.Dispatcher.Invoke(() => RegisterDataCollection[i / 2].Value = ModbusClient.ConvertRegistersToInt(intValueArray, registerOrder).ToString());
                    }
                    break;

                case (int)ModbusMasterDataTypeEnum.Unsigned_32bits:
                    for (int i = 0; i < registerValueArray.Length; i += 2)
                    {
                        intValueArray = new int[2] { registerValueArray[i], registerValueArray[i + 1] };
                        App.Current.Dispatcher.Invoke(() => RegisterDataCollection[i / 2].Value = Convert.ToUInt32(ModbusClient.ConvertRegistersToLong(intValueArray, registerOrder)).ToString());
                    }
                    break;

                case (int)ModbusMasterDataTypeEnum.Float_32bits:
                    for (int i = 0; i < registerValueArray.Length; i += 2)
                    {
                        intValueArray = new int[2] { registerValueArray[i], registerValueArray[i + 1] };
                        App.Current.Dispatcher.Invoke(() => RegisterDataCollection[i / 2].Value = ModbusClient.ConvertRegistersToFloat(intValueArray, registerOrder).ToString());
                    }
                    break;

                case (int)ModbusMasterDataTypeEnum.Signed_64bits:
                    for (int i = 0; i < registerValueArray.Length; i += 4)
                    {
                        intValueArray = new int[4]
                        {
                           registerValueArray[i],
                           registerValueArray[i + 1],
                           registerValueArray[i + 2],
                           registerValueArray[i + 3]
                        };
                        App.Current.Dispatcher.Invoke(() => RegisterDataCollection[i / 4].Value = ModbusClient.ConvertRegistersToFloat(intValueArray, registerOrder).ToString());
                    }
                    break;

                case (int)ModbusMasterDataTypeEnum.Double_64bits:
                    for (int i = 0; i < registerValueArray.Length; i += 4)
                    {
                        intValueArray = new int[4]
                        {
                           registerValueArray[i],
                           registerValueArray[i + 1],
                           registerValueArray[i + 2],
                           registerValueArray[i + 3]
                        };
                        App.Current.Dispatcher.Invoke(() => RegisterDataCollection[i / 4].Value = ModbusClient.ConvertRegistersToDouble(intValueArray, registerOrder).ToString());
                    }
                    break;

                default:
                    break;
            }
        }


        #endregion

        #region Init Methods

        private byte GetSlaveRegisterSize()
        {
            if (SelectedDataType < 4 && SelectedDataType >= 2)
            {
                return 2;
            }
            else if (SelectedDataType < 6 && SelectedDataType >= 4)
            {
                return 4;
            }
            else
            {
                return 1;
            }
        }

        private byte GetMasterRegisterSize()
        {
            if (SelectedDataType < 6 && SelectedDataType >= 3)
            {
                return 2;
            }
            else if (SelectedDataType < 8 && SelectedDataType >= 6)
            {
                return 4;
            }
            else
            {
                return 1;
            }
        }

        private void InitMasterFunction()
        {
            try
            {
                if (MasterFunctionList == null)
                {
                    MasterFunctionList = Enum.GetNames(typeof(ModbusMasterFunctionEnum)).ToList();

                }
            }
            catch (Exception ex)
            {
                InfoMessage = "Error: " + ex.Message.Replace("\n", "");
            }
        }

        private void InitSlaveFunction()
        {
            try
            {
                if (SlaveFunctionList == null)
                {
                    SlaveFunctionList = Enum.GetNames(typeof(ModbusSlaveFunctionEnum)).ToList();
                }

            }
            catch (Exception ex)
            {
                InfoMessage = "Error: " + ex.Message.Replace("\n", "");
            }
        }

        private void InitDataType()
        {
            try
            {
                if (MasterDataTypeList == null)
                {
                    MasterDataTypeList = Enum.GetNames(typeof(ModbusMasterDataTypeEnum)).ToList();
                }
                if (SlaveDataTypeList == null)
                {
                    SlaveDataTypeList = Enum.GetNames(typeof(ModbusSlaveDataTypeEnum)).ToList();
                }
            }
            catch (Exception ex)
            {

                InfoMessage = "Error: " + ex.Message.Replace("\n", "");

            }
        }

        private void InitSerialPortPara()
        {
            _serialPort = new SerialPort();

            SerialPortParaModel serialPortParaEnum = new SerialPortParaModel();

            _nameCfg = new SerialPortCfgModel { ChipContent = "PortName", ContentList = new List<string>(serialPortParaEnum.PortName) };
            _baudCfg = new SerialPortCfgModel { ChipContent = "BaudRate", ContentList = new List<string>(serialPortParaEnum.BaudRate) };
            _parityCfg = new SerialPortCfgModel { ChipContent = "ParityWay", ContentList = new List<string>(serialPortParaEnum.Parity) };
            _dataBitsCfg = new SerialPortCfgModel { ChipContent = "DataBits", ContentList = new List<string> { "8" } };
            _stopBitsCfg = new SerialPortCfgModel { ChipContent = "StopBits", ContentList = new List<string>(serialPortParaEnum.StopBits) };

            SerialPortCfgCollection = new ObservableCollection<SerialPortCfgModel> { _nameCfg, _baudCfg, _parityCfg, _dataBitsCfg, _stopBitsCfg };

        }

        #endregion

        #region EventHandler

        private void _masterClient_ConnectedChanged(object sender)
        {
            try
            {
                IsRunning = _masterClient.Connected;
                if (IsRunning)
                {
                    if (ModbusRTU)
                    {
                        InfoMessage = $"Notice: Serial Port {_nameCfg.SelectedValue} is Opened for easyModbus Application !";
                    }
                    else if (ModbusTCP)
                    {
                        if (MasterOrClientMode)
                        {
                            InfoMessage = $"Notice: Connected to ModbusTCP Server [{IpAddress}:{Port}] for easyModbus Application !";
                        }
                        else if (SlaveOrServerMode)
                        {
                            InfoMessage = $"Notice: Bind [{IpAddress}:{Port}] as ModbusTCP Server for easyModbus Application !";
                        }
                    }

                }
                else
                {
                    if (ModbusRTU)
                    {
                        InfoMessage = $"Notice: Serial Port {_nameCfg.SelectedValue} is Closed !";
                    }
                    else if (ModbusTCP)
                    {
                        if (MasterOrClientMode)
                        {
                            InfoMessage = $"Notice: Disonnected to ModbusTCP Server [{IpAddress}:{Port}] !";
                        }
                        else if (SlaveOrServerMode)
                        {
                            InfoMessage = $"Notice: Close ModbusTCP Server [{IpAddress}:{Port}] !";
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                InfoMessage = "Error: " + ex.Message.Replace("\n", "");
            }



        }

        private void _masterClient_SendDataChanged(object sender)
        {
            Task.Run(() =>
            {
                try
                {
                    _txCount++;
                    string sndMsg = ToolHelper.byteArrToHexStr(_masterClient.sendData);
                    App.Current.Dispatcher.Invoke(() => RawTelegraph += ToolHelper.SetTime(DisplayTime, true) + $"Tx {_txCount}-> {sndMsg}\n");
                    if (_txCount == ushort.MaxValue)
                    {
                        _txCount = 0;
                    }
                }
                catch (Exception ex)
                {
                    InfoMessage = "Error: " + ex.Message.Replace("\n", "");
                }

            }, _cancellationTokenSource.Token);

        }

        private void _masterClient_ReceiveDataChanged(object sender)
        {
            Task.Run(() =>
            {
                try
                {
                    _rxCount++;
                    string rcvMsg = ToolHelper.byteArrToHexStr(_masterClient.receiveData);
                    App.Current.Dispatcher.Invoke(() => RawTelegraph += ToolHelper.SetTime(DisplayTime, true) + $"Rx {_rxCount}-> {rcvMsg}\n");
                    if (_rxCount == ushort.MaxValue)
                    {
                        _rxCount = 0;
                    }
                }
                catch (Exception ex)
                {
                    InfoMessage = "Error: " + ex.Message.Replace("\n", "");
                }

            }, _cancellationTokenSource.Token);

        }

        private void _slaveServer_CoilsChanged(int startCoil, int numberOfCoils)
        {
            try
            {
                if (startCoil < StartAddress + 1 || startCoil >= StartAddress + RegisterQuantity || RegisterQuantity < numberOfCoils)
                {
                    InfoMessage = $"Warning: [StartAddress] should be {startCoil} , [RegisterQuantity] can't be less than {numberOfCoils} !";
                    return;
                }
                else
                {
                    InfoMessage = $"Notice: Received {numberOfCoils} Coil Value(s) starting from Register[{startCoil - 1}] !";

                }
                foreach (var item in RegisterDataCollection)
                {
                    ushort index = Convert.ToUInt16(item.Address);
                    item.Value = Convert.ToBoolean(_slaveServer.coils[index + 1]).ToString();
                }
            }
            catch (Exception ex)
            {

                InfoMessage = "Error: " + ex.Message.Replace("\n", "");

            }

        }

        private void _slaveServer_HoldingRegistersChanged(int startRegister, int numberOfRegisters)
        {
            try
            {
                if (startRegister < StartAddress + 1 || startRegister >= StartAddress + RegisterQuantity || RegisterQuantity < numberOfRegisters)
                {
                    InfoMessage = $"Warning: [StartAddress] should be {startRegister} , [RegisterQuantity] can't be less than {numberOfRegisters} !";
                    return;
                }
                else
                {
                    InfoMessage = $"Notice: Received {numberOfRegisters} Register Value(s) starting from Register[{startRegister - 1}] !";

                }
                ModbusClient.RegisterOrder registerOrder = GetRegisterOrder();

                switch (SelectedDataType)
                {
                    //1: Signed_16bits,
                    case (int)ModbusSlaveDataTypeEnum.Signed_16bits:
                        foreach (var item in RegisterDataCollection)
                        {
                            ushort index = Convert.ToUInt16(item.Address);
                            item.Value = _slaveServer.holdingRegisters[index + 1].ToString();
                        }
                        break;

                    //2: Signed_32bits,
                    case (int)ModbusSlaveDataTypeEnum.Signed_32bits:
                        for (int i = 0; i < RegisterDataCollection.Count; i++)
                        {
                            ushort index = Convert.ToUInt16(RegisterDataCollection[i].Address);
                            int[] registerInt = new int[2] { _slaveServer.holdingRegisters[index + 1], _slaveServer.holdingRegisters[index + 2] };

                            RegisterDataCollection[i].Value = ModbusClient.ConvertRegistersToInt(registerInt, registerOrder).ToString();
                        }
                        break;

                    //3: Float_32bits,
                    case (int)ModbusSlaveDataTypeEnum.Float_32bits:
                        for (int i = 0; i < RegisterDataCollection.Count; i++)
                        {
                            ushort index = Convert.ToUInt16(RegisterDataCollection[i].Address);
                            int[] registerInt = new int[2] { _slaveServer.holdingRegisters[index + 1], _slaveServer.holdingRegisters[index + 2] };

                            RegisterDataCollection[i].Value = ModbusClient.ConvertRegistersToFloat(registerInt, registerOrder).ToString();
                        }
                        break;


                    //4: Signed_64bits,
                    case (int)ModbusSlaveDataTypeEnum.Signed_64bits:
                        for (int i = 0; i < RegisterDataCollection.Count; i++)
                        {
                            ushort index = Convert.ToUInt16(RegisterDataCollection[i].Address);
                            int[] registerInt = new int[4]
                            {
                            _slaveServer.holdingRegisters[index + 1],
                            _slaveServer.holdingRegisters[index + 2],
                            _slaveServer.holdingRegisters[index + 3] ,
                            _slaveServer.holdingRegisters[index + 4]
                            };
                            RegisterDataCollection[i].Value = ModbusClient.ConvertRegistersToLong(registerInt, registerOrder).ToString();
                        }
                        break;

                    //5: Double_64bits
                    case (int)ModbusSlaveDataTypeEnum.Double_64bits:
                        for (int i = 0; i < RegisterDataCollection.Count; i++)
                        {
                            ushort index = Convert.ToUInt16(RegisterDataCollection[i].Address);
                            int[] registerInt = new int[4]
                            {
                            _slaveServer.holdingRegisters[index + 1],
                            _slaveServer.holdingRegisters[index + 2],
                            _slaveServer.holdingRegisters[index + 3] ,
                            _slaveServer.holdingRegisters[index + 4]
                            };
                            RegisterDataCollection[i].Value = ModbusClient.ConvertRegistersToDouble(registerInt, registerOrder).ToString();
                        }
                        break;

                    // None
                    default:

                        break;
                }
            }
            catch (Exception ex)
            {

                InfoMessage = "Error: " + ex.Message.Replace("\n", "");

            }


        }

        private void _slaveServer_ReceiveCompleted(byte[] rcvArray)
        {
            Task.Run(() =>
            {
                try
                {
                    _rxCount++;
                    string rcvMsg = ToolHelper.byteArrToHexStr(rcvArray);
                    App.Current.Dispatcher.Invoke(() => RawTelegraph += ToolHelper.SetTime(DisplayTime, true) + $"Rx {_rxCount}-> {rcvMsg}\n");
                    if (_rxCount == ushort.MaxValue)
                    {
                        _rxCount = 0;
                    }
                }
                catch (Exception ex)
                {
                    InfoMessage = "Error: " + ex.Message.Replace("\n", "");
                }
            }, _cancellationTokenSource.Token);
        }

        private void _slaveServer_SendCompleted(byte[] sndArray)
        {
            Task.Run(() =>
            {
                try
                {
                    _txCount++;
                    string sndMsg = ToolHelper.byteArrToHexStr(sndArray);
                    App.Current.Dispatcher.Invoke(() => RawTelegraph += ToolHelper.SetTime(DisplayTime, true) + $"Tx {_txCount}-> {sndMsg}\n");
                    if (_txCount == ushort.MaxValue)
                    {
                        _txCount = 0;
                    }
                }
                catch (Exception ex)
                {
                    InfoMessage = "Error: " + ex.Message.Replace("\n", "");
                }

            });
        }

        #endregion



    }





}
