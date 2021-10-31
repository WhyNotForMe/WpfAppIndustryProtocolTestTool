using System;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using EasyModbus;
using EasyModbus.Exceptions;

namespace WpfAppIndustryProtocolTestTool.BLL.ModbusProtocol
{
    public class HoldingRegisters
    {
        public short[] localArray = new short[65535];

        private ModbusSlaveServer modbusServer;

        public short this[int x]
        {
            get
            {
                return localArray[x];
            }
            set
            {
                localArray[x] = value;
            }
        }

        public HoldingRegisters(ModbusSlaveServer modbusServer)
        {
            this.modbusServer = modbusServer;
        }
    }

    public class InputRegisters
    {
        public short[] localArray = new short[65535];

        private ModbusSlaveServer modbusServer;

        public short this[int x]
        {
            get
            {
                return localArray[x];
            }
            set
            {
                localArray[x] = value;
            }
        }

        public InputRegisters(ModbusSlaveServer modbusServer)
        {
            this.modbusServer = modbusServer;
        }
    }

    public class Coils
    {
        public bool[] localArray = new bool[65535];

        private ModbusSlaveServer modbusServer;

        public bool this[int x]
        {
            get
            {
                return localArray[x];
            }
            set
            {
                localArray[x] = value;
            }
        }

        public Coils(ModbusSlaveServer modbusServer)
        {
            this.modbusServer = modbusServer;
        }
    }

    public class DiscreteInputs
    {
        public bool[] localArray = new bool[65535];

        private ModbusSlaveServer modbusServer;

        public bool this[int x]
        {
            get
            {
                return localArray[x];
            }
            set
            {
                localArray[x] = value;
            }
        }

        public DiscreteInputs(ModbusSlaveServer modbusServer)
        {
            this.modbusServer = modbusServer;
        }
    }

    public class ModbusSlaveServer
    {

        private bool debug = false;

        private int port = 502;

        private ModbusProtocol receiveData;

        private ModbusProtocol sendData = new ModbusProtocol();

        private byte[] bytes = new byte[2100];

        private int numberOfConnections = 0;

        private bool udpFlag;

        private bool serialFlag;

        private int baudrate = 9600;

        private Parity parity = Parity.Even;

        private StopBits stopBits = StopBits.One;

        private string serialPort = "COM1";

        private SerialPort serialport;

        private byte unitIdentifier = 1;

        private int portIn;

        private IPAddress ipAddressIn;

        private UdpClient udpClient;

        private IPEndPoint iPEndPoint;

        private TCPHandler tcpHandler;

        private Thread listenerThread;

        private Thread clientConnectionThread;

        private ModbusProtocol[] modbusLogData = new ModbusProtocol[100];

        private object lockCoils = new object();

        private object lockHoldingRegisters = new object();

        private volatile bool shouldStop;

        private IPAddress localIPAddress = IPAddress.Any;

        private bool dataReceived = false;

        private byte[] readBuffer = new byte[2094];

        private DateTime lastReceive;

        private int nextSign = 0;

        private object lockProcessReceivedData = new object();


        public HoldingRegisters holdingRegisters;

        public InputRegisters inputRegisters;

        public Coils coils;

        public DiscreteInputs discreteInputs;

        public bool FunctionCode1Disabled
        {
            get;
            set;
        }

        public bool FunctionCode2Disabled
        {
            get;
            set;
        }

        public bool FunctionCode3Disabled
        {
            get;
            set;
        }

        public bool FunctionCode4Disabled
        {
            get;
            set;
        }

        public bool FunctionCode5Disabled
        {
            get;
            set;
        }

        public bool FunctionCode6Disabled
        {
            get;
            set;
        }

        public bool FunctionCode15Disabled
        {
            get;
            set;
        }

        public bool FunctionCode16Disabled
        {
            get;
            set;
        }

        public bool FunctionCode23Disabled
        {
            get;
            set;
        }

        public bool PortChanged
        {
            get;
            set;
        }

        //
        // Summary:
        //     When creating a TCP or UDP socket, the local IP address to attach to.
        public IPAddress LocalIPAddress
        {
            get
            {
                return localIPAddress;
            }
            set
            {
                if (listenerThread == null)
                {
                    localIPAddress = value;
                }
            }
        }

        public int NumberOfConnections => numberOfConnections;

        internal ModbusProtocol[] ModbusLogData => modbusLogData;

        public int Port
        {
            get
            {
                return port;
            }
            set
            {
                port = value;
            }
        }

        public bool UDPFlag
        {
            get
            {
                return udpFlag;
            }
            set
            {
                udpFlag = value;
            }
        }

        public bool SerialFlag
        {
            get
            {
                return serialFlag;
            }
            set
            {
                serialFlag = value;
            }
        }

        public int Baudrate
        {
            get
            {
                return baudrate;
            }
            set
            {
                baudrate = value;
            }
        }

        public Parity Parity
        {
            get
            {
                return parity;
            }
            set
            {
                parity = value;
            }
        }

        public StopBits StopBits
        {
            get
            {
                return stopBits;
            }
            set
            {
                stopBits = value;
            }
        }

        public string SerialPort
        {
            get
            {
                return serialPort;
            }
            set
            {
                serialPort = value;
                if (serialPort != null)
                {
                    serialFlag = true;
                }
                else
                {
                    serialFlag = false;
                }
            }
        }

        public byte UnitIdentifier
        {
            get
            {
                return unitIdentifier;
            }
            set
            {
                unitIdentifier = value;
            }
        }

        //
        // Summary:
        //     Gets or Sets the Filename for the LogFile
        public string LogFileFilename
        {
            get
            {
                return StoreLogData.Instance.Filename;
            }
            set
            {
                StoreLogData.Instance.Filename = value;
                if (StoreLogData.Instance.Filename != null)
                {
                    debug = true;
                }
                else
                {
                    debug = false;
                }
            }
        }


        public event CoilsChangedHandler CoilsChanged;

        public event HoldingRegistersChangedHandler HoldingRegistersChanged;

        public event NumberOfConnectedClientsChangedHandler NumberOfConnectedClientsChanged;

        public event LogDataChangedHandler LogDataChanged;

        public event OnReceiveComplete ReceiveCompleted;

        public event OnSendComplete SendCompleted;

        public ModbusSlaveServer()
        {
            holdingRegisters = new HoldingRegisters(this);
            inputRegisters = new InputRegisters(this);
            coils = new Coils(this);
            discreteInputs = new DiscreteInputs(this);
        }

        public void Listen()
        {
            listenerThread = new Thread(ListenerThread);
            listenerThread.Start();
        }

        public void StopListening()
        {
            if (SerialFlag)
            {
                if (serialport.IsOpen)
                {
                    serialport?.Close();
                }

                shouldStop = true;
            }

            try
            {
                tcpHandler?.Disconnect();
                //listenerThread?.Interrupt();
            }
            catch (Exception)
            {
                throw;
            }

            try
            {
                listenerThread?.Join();
                //clientConnectionThread?.Abort();
            }
            catch (Exception)
            {
                throw;
            }



        }

        private void ListenerThread()
        {
            if (!udpFlag & !serialFlag)
            {
                try
                {
                    udpClient?.Close();
                }
                catch (Exception)
                {
                    throw;
                }

                tcpHandler = new TCPHandler(LocalIPAddress, port);
                if (debug)
                {
                    StoreLogData.Instance.Store($"EasyModbus Server listing for incomming data at Port {port}, local IP {LocalIPAddress}", DateTime.Now);
                }

                tcpHandler.dataChanged += ProcessReceivedData;
                tcpHandler.numberOfClientsChanged += numberOfClientsChanged;
                return;
            }

            if (serialFlag)
            {
                if (serialport == null)
                {
                    if (debug)
                    {
                        StoreLogData.Instance.Store("EasyModbus RTU-Server listing for incomming data at Serial Port " + serialPort, DateTime.Now);
                    }
                    try
                    {
                        serialport = new SerialPort();
                        serialport.PortName = serialPort;
                        serialport.BaudRate = baudrate;
                        serialport.Parity = parity;
                        serialport.StopBits = stopBits;
                        serialport.WriteTimeout = 10000;
                        serialport.ReadTimeout = 1000;
                        serialport.DataReceived += DataReceivedHandler;
                        serialport.Open();
                    }
                    catch (Exception)
                    {

                        throw;
                    }


                }

                return;
            }

            while (!shouldStop)
            {
                if (!udpFlag)
                {
                    continue;
                }

                if ((udpClient == null) | PortChanged)
                {
                    IPEndPoint localEP = new IPEndPoint(LocalIPAddress, port);
                    udpClient = new UdpClient(localEP);
                    if (debug)
                    {
                        StoreLogData.Instance.Store($"EasyModbus Server listing for incomming data at Port {port}, local IP {LocalIPAddress}", DateTime.Now);
                    }

                    udpClient.Client.ReceiveTimeout = 1000;
                    iPEndPoint = new IPEndPoint(IPAddress.Any, port);
                    PortChanged = false;
                }

                if (tcpHandler != null)
                {
                    tcpHandler.Disconnect();
                }

                try
                {
                    bytes = udpClient.Receive(ref iPEndPoint);
                    portIn = iPEndPoint.Port;
                    NetworkConnectionParameter networkConnectionParameter = default(NetworkConnectionParameter);
                    networkConnectionParameter.bytes = bytes;
                    ipAddressIn = iPEndPoint.Address;
                    networkConnectionParameter.portIn = portIn;
                    networkConnectionParameter.ipAddressIn = ipAddressIn;
                    ParameterizedThreadStart start = ProcessReceivedData;
                    Thread thread = new Thread(start);
                    thread.Start(networkConnectionParameter);

                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            int num = 4000 / baudrate;
            checked
            {
                if (DateTime.Now.Ticks - lastReceive.Ticks > 10000L * unchecked((long)num))
                {
                    nextSign = 0;
                }

                SerialPort serialPort = (SerialPort)sender;
                int bytesToRead = serialPort.BytesToRead;
                byte[] array = new byte[bytesToRead];
                serialPort.Read(array, 0, bytesToRead);
                //Telegraph
                ReceiveCompleted?.Invoke(array);

                Array.Copy(array, 0, readBuffer, nextSign, array.Length);
                lastReceive = DateTime.Now;
                nextSign = bytesToRead + nextSign;
                if (ModbusClient.DetectValidModbusFrame(readBuffer, nextSign))
                {
                    try
                    {
                        dataReceived = true;
                        nextSign = 0;
                        NetworkConnectionParameter networkConnectionParameter = default(NetworkConnectionParameter);
                        networkConnectionParameter.bytes = readBuffer;
                        ParameterizedThreadStart start = ProcessReceivedData;
                        Thread thread = new Thread(start);
                        thread.Start(networkConnectionParameter);
                        dataReceived = false;
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                else
                {
                    dataReceived = false;
                }
            }
        }

        private void numberOfClientsChanged()
        {
            numberOfConnections = tcpHandler.NumberOfConnectedClients;
            if (this.NumberOfConnectedClientsChanged != null)
            {
                this.NumberOfConnectedClientsChanged();
            }
        }

        private void ProcessReceivedData(object networkConnectionParameter)
        {
            checked
            {
                lock (lockProcessReceivedData)
                {
                    byte[] array = new byte[((NetworkConnectionParameter)networkConnectionParameter).bytes.Length];

                    if (debug)
                    {
                        StoreLogData.Instance.Store("Received Data: " + BitConverter.ToString(array), DateTime.Now);
                    }

                    NetworkStream stream = ((NetworkConnectionParameter)networkConnectionParameter).stream;
                    int num = ((NetworkConnectionParameter)networkConnectionParameter).portIn;
                    IPAddress iPAddress = ((NetworkConnectionParameter)networkConnectionParameter).ipAddressIn;
                    Array.Copy(((NetworkConnectionParameter)networkConnectionParameter).bytes, 0, array, 0, ((NetworkConnectionParameter)networkConnectionParameter).bytes.Length);

                    //Telegraph
                    if (!UDPFlag && !SerialFlag)
                    {
                        ReceiveCompleted?.Invoke(array);
                    }
                    ModbusProtocol modbusProtocol = new ModbusProtocol();
                    ModbusProtocol modbusProtocol2 = new ModbusProtocol();
                    try
                    {
                        ushort[] array2 = new ushort[1];
                        byte[] array3 = new byte[2];
                        modbusProtocol.timeStamp = DateTime.Now;
                        modbusProtocol.request = true;
                        if (!serialFlag)
                        {
                            array3[1] = array[0];
                            array3[0] = array[1];
                            Buffer.BlockCopy(array3, 0, array2, 0, 2);
                            modbusProtocol.transactionIdentifier = array2[0];
                            array3[1] = array[2];
                            array3[0] = array[3];
                            Buffer.BlockCopy(array3, 0, array2, 0, 2);
                            modbusProtocol.protocolIdentifier = array2[0];
                            array3[1] = array[4];
                            array3[0] = array[5];
                            Buffer.BlockCopy(array3, 0, array2, 0, 2);
                            modbusProtocol.length = array2[0];
                        }

                        modbusProtocol.unitIdentifier = array[6 - 6 * Convert.ToInt32(serialFlag)];
                        if ((modbusProtocol.unitIdentifier != unitIdentifier) & (modbusProtocol.unitIdentifier != 0))
                        {
                            return;
                        }

                        modbusProtocol.functionCode = array[7 - 6 * Convert.ToInt32(serialFlag)];
                        array3[1] = array[8 - 6 * Convert.ToInt32(serialFlag)];
                        array3[0] = array[9 - 6 * Convert.ToInt32(serialFlag)];
                        Buffer.BlockCopy(array3, 0, array2, 0, 2);
                        modbusProtocol.startingAdress = array2[0];
                        if (modbusProtocol.functionCode <= 4)
                        {
                            array3[1] = array[10 - 6 * Convert.ToInt32(serialFlag)];
                            array3[0] = array[11 - 6 * Convert.ToInt32(serialFlag)];
                            Buffer.BlockCopy(array3, 0, array2, 0, 2);
                            modbusProtocol.quantity = array2[0];
                        }

                        if (modbusProtocol.functionCode == 5)
                        {
                            modbusProtocol.receiveCoilValues = new ushort[1];
                            array3[1] = array[10 - 6 * Convert.ToInt32(serialFlag)];
                            array3[0] = array[11 - 6 * Convert.ToInt32(serialFlag)];
                            Buffer.BlockCopy(array3, 0, modbusProtocol.receiveCoilValues, 0, 2);
                        }

                        if (modbusProtocol.functionCode == 6)
                        {
                            modbusProtocol.receiveRegisterValues = new ushort[1];
                            array3[1] = array[10 - 6 * Convert.ToInt32(serialFlag)];
                            array3[0] = array[11 - 6 * Convert.ToInt32(serialFlag)];
                            Buffer.BlockCopy(array3, 0, modbusProtocol.receiveRegisterValues, 0, 2);
                        }

                        if (modbusProtocol.functionCode == 15)
                        {
                            array3[1] = array[10 - 6 * Convert.ToInt32(serialFlag)];
                            array3[0] = array[11 - 6 * Convert.ToInt32(serialFlag)];
                            Buffer.BlockCopy(array3, 0, array2, 0, 2);
                            modbusProtocol.quantity = array2[0];
                            modbusProtocol.byteCount = array[12 - 6 * Convert.ToInt32(serialFlag)];
                            if (unchecked((int)modbusProtocol.byteCount % 2) != 0)
                            {
                                modbusProtocol.receiveCoilValues = new ushort[unchecked((int)modbusProtocol.byteCount / 2) + 1];
                            }
                            else
                            {
                                modbusProtocol.receiveCoilValues = new ushort[unchecked((int)modbusProtocol.byteCount / 2)];
                            }

                            Buffer.BlockCopy(array, 13 - 6 * Convert.ToInt32(serialFlag), modbusProtocol.receiveCoilValues, 0, modbusProtocol.byteCount);
                        }

                        if (modbusProtocol.functionCode == 16)
                        {
                            array3[1] = array[10 - 6 * Convert.ToInt32(serialFlag)];
                            array3[0] = array[11 - 6 * Convert.ToInt32(serialFlag)];
                            Buffer.BlockCopy(array3, 0, array2, 0, 2);
                            modbusProtocol.quantity = array2[0];
                            modbusProtocol.byteCount = array[12 - 6 * Convert.ToInt32(serialFlag)];
                            modbusProtocol.receiveRegisterValues = new ushort[modbusProtocol.quantity];
                            for (int i = 0; i < modbusProtocol.quantity; i++)
                            {
                                array3[1] = array[13 + i * 2 - 6 * Convert.ToInt32(serialFlag)];
                                array3[0] = array[14 + i * 2 - 6 * Convert.ToInt32(serialFlag)];
                                Buffer.BlockCopy(array3, 0, modbusProtocol.receiveRegisterValues, i * 2, 2);
                            }
                        }

                        if (modbusProtocol.functionCode == 23)
                        {
                            array3[1] = array[8 - 6 * Convert.ToInt32(serialFlag)];
                            array3[0] = array[9 - 6 * Convert.ToInt32(serialFlag)];
                            Buffer.BlockCopy(array3, 0, array2, 0, 2);
                            modbusProtocol.startingAddressRead = array2[0];
                            array3[1] = array[10 - 6 * Convert.ToInt32(serialFlag)];
                            array3[0] = array[11 - 6 * Convert.ToInt32(serialFlag)];
                            Buffer.BlockCopy(array3, 0, array2, 0, 2);
                            modbusProtocol.quantityRead = array2[0];
                            array3[1] = array[12 - 6 * Convert.ToInt32(serialFlag)];
                            array3[0] = array[13 - 6 * Convert.ToInt32(serialFlag)];
                            Buffer.BlockCopy(array3, 0, array2, 0, 2);
                            modbusProtocol.startingAddressWrite = array2[0];
                            array3[1] = array[14 - 6 * Convert.ToInt32(serialFlag)];
                            array3[0] = array[15 - 6 * Convert.ToInt32(serialFlag)];
                            Buffer.BlockCopy(array3, 0, array2, 0, 2);
                            modbusProtocol.quantityWrite = array2[0];
                            modbusProtocol.byteCount = array[16 - 6 * Convert.ToInt32(serialFlag)];
                            modbusProtocol.receiveRegisterValues = new ushort[modbusProtocol.quantityWrite];
                            for (int j = 0; j < modbusProtocol.quantityWrite; j++)
                            {
                                array3[1] = array[17 + j * 2 - 6 * Convert.ToInt32(serialFlag)];
                                array3[0] = array[18 + j * 2 - 6 * Convert.ToInt32(serialFlag)];
                                Buffer.BlockCopy(array3, 0, modbusProtocol.receiveRegisterValues, j * 2, 2);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }

                    CreateAnswer(modbusProtocol, modbusProtocol2, stream, num, iPAddress);
                    CreateLogData(modbusProtocol, modbusProtocol2);
                    if (this.LogDataChanged != null)
                    {
                        this.LogDataChanged();
                    }
                }
            }
        }

        private void CreateAnswer(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream, int portIn, IPAddress ipAddressIn)
        {
            checked
            {
                switch (receiveData.functionCode)
                {
                    case 1:
                        if (!FunctionCode1Disabled)
                        {
                            ReadCoils(receiveData, sendData, stream, portIn, ipAddressIn);
                            break;
                        }

                        sendData.errorCode = (byte)(unchecked((int)receiveData.functionCode) + 128);
                        sendData.exceptionCode = 1;
                        sendException(sendData.errorCode, sendData.exceptionCode, receiveData, sendData, stream, portIn, ipAddressIn);
                        break;
                    case 2:
                        if (!FunctionCode2Disabled)
                        {
                            ReadDiscreteInputs(receiveData, sendData, stream, portIn, ipAddressIn);
                            break;
                        }

                        sendData.errorCode = (byte)(unchecked((int)receiveData.functionCode) + 128);
                        sendData.exceptionCode = 1;
                        sendException(sendData.errorCode, sendData.exceptionCode, receiveData, sendData, stream, portIn, ipAddressIn);
                        break;
                    case 3:
                        if (!FunctionCode3Disabled)
                        {
                            ReadHoldingRegisters(receiveData, sendData, stream, portIn, ipAddressIn);
                            break;
                        }

                        sendData.errorCode = (byte)(unchecked((int)receiveData.functionCode) + 128);
                        sendData.exceptionCode = 1;
                        sendException(sendData.errorCode, sendData.exceptionCode, receiveData, sendData, stream, portIn, ipAddressIn);
                        break;
                    case 4:
                        if (!FunctionCode4Disabled)
                        {
                            ReadInputRegisters(receiveData, sendData, stream, portIn, ipAddressIn);
                            break;
                        }

                        sendData.errorCode = (byte)(unchecked((int)receiveData.functionCode) + 128);
                        sendData.exceptionCode = 1;
                        sendException(sendData.errorCode, sendData.exceptionCode, receiveData, sendData, stream, portIn, ipAddressIn);
                        break;
                    case 5:
                        if (!FunctionCode5Disabled)
                        {
                            WriteSingleCoil(receiveData, sendData, stream, portIn, ipAddressIn);
                            break;
                        }

                        sendData.errorCode = (byte)(unchecked((int)receiveData.functionCode) + 128);
                        sendData.exceptionCode = 1;
                        sendException(sendData.errorCode, sendData.exceptionCode, receiveData, sendData, stream, portIn, ipAddressIn);
                        break;
                    case 6:
                        if (!FunctionCode6Disabled)
                        {
                            WriteSingleRegister(receiveData, sendData, stream, portIn, ipAddressIn);
                            break;
                        }

                        sendData.errorCode = (byte)(unchecked((int)receiveData.functionCode) + 128);
                        sendData.exceptionCode = 1;
                        sendException(sendData.errorCode, sendData.exceptionCode, receiveData, sendData, stream, portIn, ipAddressIn);
                        break;
                    case 15:
                        if (!FunctionCode15Disabled)
                        {
                            WriteMultipleCoils(receiveData, sendData, stream, portIn, ipAddressIn);
                            break;
                        }

                        sendData.errorCode = (byte)(unchecked((int)receiveData.functionCode) + 128);
                        sendData.exceptionCode = 1;
                        sendException(sendData.errorCode, sendData.exceptionCode, receiveData, sendData, stream, portIn, ipAddressIn);
                        break;
                    case 16:
                        if (!FunctionCode16Disabled)
                        {
                            WriteMultipleRegisters(receiveData, sendData, stream, portIn, ipAddressIn);
                            break;
                        }

                        sendData.errorCode = (byte)(unchecked((int)receiveData.functionCode) + 128);
                        sendData.exceptionCode = 1;
                        sendException(sendData.errorCode, sendData.exceptionCode, receiveData, sendData, stream, portIn, ipAddressIn);
                        break;
                    case 23:
                        if (!FunctionCode23Disabled)
                        {
                            ReadWriteMultipleRegisters(receiveData, sendData, stream, portIn, ipAddressIn);
                            break;
                        }

                        sendData.errorCode = (byte)(unchecked((int)receiveData.functionCode) + 128);
                        sendData.exceptionCode = 1;
                        sendException(sendData.errorCode, sendData.exceptionCode, receiveData, sendData, stream, portIn, ipAddressIn);
                        break;
                    default:
                        sendData.errorCode = (byte)(unchecked((int)receiveData.functionCode) + 128);
                        sendData.exceptionCode = 1;
                        sendException(sendData.errorCode, sendData.exceptionCode, receiveData, sendData, stream, portIn, ipAddressIn);
                        break;
                }

                sendData.timeStamp = DateTime.Now;
            }
        }

        private void ReadCoils(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream, int portIn, IPAddress ipAddressIn)
        {
            sendData.response = true;
            sendData.transactionIdentifier = receiveData.transactionIdentifier;
            sendData.protocolIdentifier = receiveData.protocolIdentifier;
            sendData.unitIdentifier = unitIdentifier;
            sendData.functionCode = receiveData.functionCode;
            byte[] array;
            byte[] array2;
            checked
            {
                if ((receiveData.quantity < 1) | (receiveData.quantity > 2000))
                {
                    sendData.errorCode = (byte)(unchecked((int)receiveData.functionCode) + 128);
                    sendData.exceptionCode = 3;
                }

                if ((unchecked((int)receiveData.startingAdress) + 1 + unchecked((int)receiveData.quantity) > 65535) | (receiveData.startingAdress < 0))
                {
                    sendData.errorCode = (byte)(unchecked((int)receiveData.functionCode) + 128);
                    sendData.exceptionCode = 2;
                }

                if (sendData.exceptionCode == 0)
                {
                    if (unchecked((int)receiveData.quantity % 8) == 0)
                    {
                        sendData.byteCount = (byte)unchecked((int)receiveData.quantity / 8);
                    }
                    else
                    {
                        sendData.byteCount = (byte)(unchecked((int)receiveData.quantity / 8) + 1);
                    }

                    sendData.sendCoilValues = new bool[receiveData.quantity];
                    lock (lockCoils)
                    {
                        Array.Copy(coils.localArray, unchecked((int)receiveData.startingAdress) + 1, sendData.sendCoilValues, 0, receiveData.quantity);
                    }
                }

                bool flag = true;
                array = ((sendData.exceptionCode <= 0) ? new byte[9 + unchecked((int)sendData.byteCount) + 2 * Convert.ToInt32(serialFlag)] : new byte[9 + 2 * Convert.ToInt32(serialFlag)]);
                array2 = new byte[2];
                sendData.length = (byte)(array.Length - 6);
            }

            array2 = BitConverter.GetBytes((int)sendData.transactionIdentifier);
            array[0] = array2[1];
            array[1] = array2[0];
            array2 = BitConverter.GetBytes((int)sendData.protocolIdentifier);
            array[2] = array2[1];
            array[3] = array2[0];
            array2 = BitConverter.GetBytes((int)sendData.length);
            array[4] = array2[1];
            array[5] = array2[0];
            array[6] = sendData.unitIdentifier;
            array[7] = sendData.functionCode;
            array[8] = sendData.byteCount;
            if (sendData.exceptionCode > 0)
            {
                array[7] = sendData.errorCode;
                array[8] = sendData.exceptionCode;
                sendData.sendCoilValues = null;
            }

            checked
            {
                if (sendData.sendCoilValues != null)
                {
                    for (int i = 0; i < sendData.byteCount; i++)
                    {
                        array2 = new byte[2];
                        for (int j = 0; j < 8; j++)
                        {
                            byte b;
                            unchecked
                            {
                                b = (byte)(sendData.sendCoilValues[checked(i * 8 + j)] ? 1 : 0);
                            }

                            array2[1] = (byte)(array2[1] | (b << j));
                            if (i * 8 + j + 1 >= sendData.sendCoilValues.Length)
                            {
                                break;
                            }
                        }

                        array[9 + i] = array2[1];
                    }
                }

                try
                {
                    if (serialFlag)
                    {
                        if (!serialport.IsOpen)
                        {
                            throw new SerialPortNotOpenedException("serial port not opened");
                        }

                        sendData.crc = ModbusClient.calculateCRC(array, Convert.ToUInt16(array.Length - 8), 6);
                        array2 = BitConverter.GetBytes(unchecked((int)sendData.crc));
                        array[array.Length - 2] = array2[0];
                        array[array.Length - 1] = array2[1];
                        serialport.Write(array, 6, array.Length - 6);

                        byte[] array3 = new byte[array.Length - 6];
                        Array.Copy(array, 6, array3, 0, array.Length - 6);
                        //Telegraph
                        SendCompleted?.Invoke(array3);
                        if (debug)
                        {
                            StoreLogData.Instance.Store("Send Serial-Data: " + BitConverter.ToString(array3), DateTime.Now);
                        }
                    }
                    else if (udpFlag)
                    {
                        IPEndPoint endPoint = new IPEndPoint(ipAddressIn, portIn);
                        if (debug)
                        {
                            StoreLogData.Instance.Store("Send Data: " + BitConverter.ToString(array), DateTime.Now);
                        }

                        udpClient.Send(array, array.Length, endPoint);
                    }
                    else
                    {
                        stream.Write(array, 0, array.Length);
                        //Telegraph
                        SendCompleted?.Invoke(array);
                        if (debug)
                        {
                            StoreLogData.Instance.Store("Send Data: " + BitConverter.ToString(array), DateTime.Now);
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        private void ReadDiscreteInputs(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream, int portIn, IPAddress ipAddressIn)
        {
            sendData.response = true;
            sendData.transactionIdentifier = receiveData.transactionIdentifier;
            sendData.protocolIdentifier = receiveData.protocolIdentifier;
            sendData.unitIdentifier = unitIdentifier;
            sendData.functionCode = receiveData.functionCode;
            byte[] array;
            byte[] array2;
            checked
            {
                if ((receiveData.quantity < 1) | (receiveData.quantity > 2000))
                {
                    sendData.errorCode = (byte)(unchecked((int)receiveData.functionCode) + 128);
                    sendData.exceptionCode = 3;
                }

                if ((unchecked((int)receiveData.startingAdress) + 1 + unchecked((int)receiveData.quantity) > 65535) | (receiveData.startingAdress < 0))
                {
                    sendData.errorCode = (byte)(unchecked((int)receiveData.functionCode) + 128);
                    sendData.exceptionCode = 2;
                }

                if (sendData.exceptionCode == 0)
                {
                    if (unchecked((int)receiveData.quantity % 8) == 0)
                    {
                        sendData.byteCount = (byte)unchecked((int)receiveData.quantity / 8);
                    }
                    else
                    {
                        sendData.byteCount = (byte)(unchecked((int)receiveData.quantity / 8) + 1);
                    }

                    sendData.sendCoilValues = new bool[receiveData.quantity];
                    try
                    {
                        Array.Copy(discreteInputs.localArray, unchecked((int)receiveData.startingAdress) + 1, sendData.sendCoilValues, 0, receiveData.quantity);
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }

                bool flag = true;
                array = ((sendData.exceptionCode <= 0) ? new byte[9 + unchecked((int)sendData.byteCount) + 2 * Convert.ToInt32(serialFlag)] : new byte[9 + 2 * Convert.ToInt32(serialFlag)]);
                array2 = new byte[2];
                sendData.length = (byte)(array.Length - 6);
            }

            array2 = BitConverter.GetBytes((int)sendData.transactionIdentifier);
            array[0] = array2[1];
            array[1] = array2[0];
            array2 = BitConverter.GetBytes((int)sendData.protocolIdentifier);
            array[2] = array2[1];
            array[3] = array2[0];
            array2 = BitConverter.GetBytes((int)sendData.length);
            array[4] = array2[1];
            array[5] = array2[0];
            array[6] = sendData.unitIdentifier;
            array[7] = sendData.functionCode;
            array[8] = sendData.byteCount;
            if (sendData.exceptionCode > 0)
            {
                array[7] = sendData.errorCode;
                array[8] = sendData.exceptionCode;
                sendData.sendCoilValues = null;
            }

            checked
            {
                if (sendData.sendCoilValues != null)
                {
                    for (int i = 0; i < sendData.byteCount; i++)
                    {
                        array2 = new byte[2];
                        for (int j = 0; j < 8; j++)
                        {
                            byte b;
                            unchecked
                            {
                                b = (byte)(sendData.sendCoilValues[checked(i * 8 + j)] ? 1 : 0);
                            }

                            array2[1] = (byte)(array2[1] | (b << j));
                            if (i * 8 + j + 1 >= sendData.sendCoilValues.Length)
                            {
                                break;
                            }
                        }

                        array[9 + i] = array2[1];
                    }
                }

                try
                {
                    if (serialFlag)
                    {
                        if (!serialport.IsOpen)
                        {
                            throw new SerialPortNotOpenedException("serial port not opened");
                        }

                        sendData.crc = ModbusClient.calculateCRC(array, Convert.ToUInt16(array.Length - 8), 6);
                        array2 = BitConverter.GetBytes(unchecked((int)sendData.crc));
                        array[array.Length - 2] = array2[0];
                        array[array.Length - 1] = array2[1];
                        serialport.Write(array, 6, array.Length - 6);

                        byte[] array3 = new byte[array.Length - 6];
                        Array.Copy(array, 6, array3, 0, array.Length - 6);
                        //Telegraph
                        SendCompleted?.Invoke(array3);
                        if (debug)
                        {
                            StoreLogData.Instance.Store("Send Serial-Data: " + BitConverter.ToString(array3), DateTime.Now);
                        }
                    }
                    else if (udpFlag)
                    {
                        IPEndPoint endPoint = new IPEndPoint(ipAddressIn, portIn);
                        udpClient.Send(array, array.Length, endPoint);
                    }
                    else
                    {
                        stream.Write(array, 0, array.Length);
                        //Telegraph
                        SendCompleted?.Invoke(array);
                        if (debug)
                        {
                            StoreLogData.Instance.Store("Send Data: " + BitConverter.ToString(array), DateTime.Now);
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        private void ReadHoldingRegisters(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream, int portIn, IPAddress ipAddressIn)
        {
            sendData.response = true;
            sendData.transactionIdentifier = receiveData.transactionIdentifier;
            sendData.protocolIdentifier = receiveData.protocolIdentifier;
            sendData.unitIdentifier = unitIdentifier;
            sendData.functionCode = receiveData.functionCode;
            byte[] array;
            byte[] array2;
            checked
            {
                if ((receiveData.quantity < 1) | (receiveData.quantity > 125))
                {
                    sendData.errorCode = (byte)(unchecked((int)receiveData.functionCode) + 128);
                    sendData.exceptionCode = 3;
                }

                if ((unchecked((int)receiveData.startingAdress) + 1 + unchecked((int)receiveData.quantity) > 65535) | (receiveData.startingAdress < 0))
                {
                    sendData.errorCode = (byte)(unchecked((int)receiveData.functionCode) + 128);
                    sendData.exceptionCode = 2;
                }

                if (sendData.exceptionCode == 0)
                {
                    sendData.byteCount = (byte)(2 * unchecked((int)receiveData.quantity));
                    sendData.sendRegisterValues = new short[receiveData.quantity];
                    lock (lockHoldingRegisters)
                    {
                        Buffer.BlockCopy(holdingRegisters.localArray, unchecked((int)receiveData.startingAdress) * 2 + 2, sendData.sendRegisterValues, 0, unchecked((int)receiveData.quantity) * 2);
                    }
                }

                if (sendData.exceptionCode > 0)
                {
                    sendData.length = 3;
                }
                else
                {
                    sendData.length = (ushort)(3 + unchecked((int)sendData.byteCount));
                }

                bool flag = true;
                array = ((sendData.exceptionCode <= 0) ? new byte[9 + unchecked((int)sendData.byteCount) + 2 * Convert.ToInt32(serialFlag)] : new byte[9 + 2 * Convert.ToInt32(serialFlag)]);
                array2 = new byte[2];
                sendData.length = (byte)(array.Length - 6);
            }

            array2 = BitConverter.GetBytes((int)sendData.transactionIdentifier);
            array[0] = array2[1];
            array[1] = array2[0];
            array2 = BitConverter.GetBytes((int)sendData.protocolIdentifier);
            array[2] = array2[1];
            array[3] = array2[0];
            array2 = BitConverter.GetBytes((int)sendData.length);
            array[4] = array2[1];
            array[5] = array2[0];
            array[6] = sendData.unitIdentifier;
            array[7] = sendData.functionCode;
            array[8] = sendData.byteCount;
            if (sendData.exceptionCode > 0)
            {
                array[7] = sendData.errorCode;
                array[8] = sendData.exceptionCode;
                sendData.sendRegisterValues = null;
            }

            checked
            {
                if (sendData.sendRegisterValues != null)
                {
                    for (int i = 0; i < unchecked((int)sendData.byteCount / 2); i++)
                    {
                        array2 = BitConverter.GetBytes(sendData.sendRegisterValues[i]);
                        array[9 + i * 2] = array2[1];
                        array[10 + i * 2] = array2[0];
                    }
                }

                try
                {
                    if (serialFlag)
                    {
                        if (!serialport.IsOpen)
                        {
                            throw new SerialPortNotOpenedException("serial port not opened");
                        }

                        sendData.crc = ModbusClient.calculateCRC(array, Convert.ToUInt16(array.Length - 8), 6);
                        array2 = BitConverter.GetBytes(unchecked((int)sendData.crc));
                        array[array.Length - 2] = array2[0];
                        array[array.Length - 1] = array2[1];
                        serialport.Write(array, 6, array.Length - 6);

                        byte[] array3 = new byte[array.Length - 6];
                        Array.Copy(array, 6, array3, 0, array.Length - 6);
                        //Telegraph
                        SendCompleted?.Invoke(array3);
                        if (debug)
                        {
                            StoreLogData.Instance.Store("Send Serial-Data: " + BitConverter.ToString(array3), DateTime.Now);
                        }

                    }
                    else if (udpFlag)
                    {
                        IPEndPoint endPoint = new IPEndPoint(ipAddressIn, portIn);
                        udpClient.Send(array, array.Length, endPoint);
                    }
                    else
                    {
                        stream.Write(array, 0, array.Length);
                        //Telegraph
                        SendCompleted?.Invoke(array);
                        if (debug)
                        {
                            StoreLogData.Instance.Store("Send Data: " + BitConverter.ToString(array), DateTime.Now);
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        private void ReadInputRegisters(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream, int portIn, IPAddress ipAddressIn)
        {
            sendData.response = true;
            sendData.transactionIdentifier = receiveData.transactionIdentifier;
            sendData.protocolIdentifier = receiveData.protocolIdentifier;
            sendData.unitIdentifier = unitIdentifier;
            sendData.functionCode = receiveData.functionCode;
            byte[] array;
            byte[] array2;
            checked
            {
                if ((receiveData.quantity < 1) | (receiveData.quantity > 125))
                {
                    sendData.errorCode = (byte)(unchecked((int)receiveData.functionCode) + 128);
                    sendData.exceptionCode = 3;
                }

                if ((unchecked((int)receiveData.startingAdress) + 1 + unchecked((int)receiveData.quantity) > 65535) | (receiveData.startingAdress < 0))
                {
                    sendData.errorCode = (byte)(unchecked((int)receiveData.functionCode) + 128);
                    sendData.exceptionCode = 2;
                }

                if (sendData.exceptionCode == 0)
                {
                    sendData.byteCount = (byte)(2 * unchecked((int)receiveData.quantity));
                    sendData.sendRegisterValues = new short[receiveData.quantity];
                    Buffer.BlockCopy(inputRegisters.localArray, unchecked((int)receiveData.startingAdress) * 2 + 2, sendData.sendRegisterValues, 0, unchecked((int)receiveData.quantity) * 2);
                }

                if (sendData.exceptionCode > 0)
                {
                    sendData.length = 3;
                }
                else
                {
                    sendData.length = (ushort)(3 + unchecked((int)sendData.byteCount));
                }

                bool flag = true;
                array = ((sendData.exceptionCode <= 0) ? new byte[9 + unchecked((int)sendData.byteCount) + 2 * Convert.ToInt32(serialFlag)] : new byte[9 + 2 * Convert.ToInt32(serialFlag)]);
                array2 = new byte[2];
                sendData.length = (byte)(array.Length - 6);
            }

            array2 = BitConverter.GetBytes((int)sendData.transactionIdentifier);
            array[0] = array2[1];
            array[1] = array2[0];
            array2 = BitConverter.GetBytes((int)sendData.protocolIdentifier);
            array[2] = array2[1];
            array[3] = array2[0];
            array2 = BitConverter.GetBytes((int)sendData.length);
            array[4] = array2[1];
            array[5] = array2[0];
            array[6] = sendData.unitIdentifier;
            array[7] = sendData.functionCode;
            array[8] = sendData.byteCount;
            if (sendData.exceptionCode > 0)
            {
                array[7] = sendData.errorCode;
                array[8] = sendData.exceptionCode;
                sendData.sendRegisterValues = null;
            }

            checked
            {
                if (sendData.sendRegisterValues != null)
                {
                    for (int i = 0; i < unchecked((int)sendData.byteCount / 2); i++)
                    {
                        array2 = BitConverter.GetBytes(sendData.sendRegisterValues[i]);
                        array[9 + i * 2] = array2[1];
                        array[10 + i * 2] = array2[0];
                    }
                }

                try
                {
                    if (serialFlag)
                    {
                        if (!serialport.IsOpen)
                        {
                            throw new SerialPortNotOpenedException("serial port not opened");
                        }

                        sendData.crc = ModbusClient.calculateCRC(array, Convert.ToUInt16(array.Length - 8), 6);
                        array2 = BitConverter.GetBytes(unchecked((int)sendData.crc));
                        array[array.Length - 2] = array2[0];
                        array[array.Length - 1] = array2[1];
                        serialport.Write(array, 6, array.Length - 6);

                        byte[] array3 = new byte[array.Length - 6];
                        Array.Copy(array, 6, array3, 0, array.Length - 6);
                        //Telegraph
                        SendCompleted?.Invoke(array3);
                        if (debug)
                        {
                            StoreLogData.Instance.Store("Send Serial-Data: " + BitConverter.ToString(array3), DateTime.Now);
                        }
                    }
                    else if (udpFlag)
                    {
                        IPEndPoint endPoint = new IPEndPoint(ipAddressIn, portIn);
                        udpClient.Send(array, array.Length, endPoint);
                    }
                    else
                    {
                        stream.Write(array, 0, array.Length);
                        //Telegraph
                        SendCompleted?.Invoke(array);
                        if (debug)
                        {
                            StoreLogData.Instance.Store("Send Data: " + BitConverter.ToString(array), DateTime.Now);
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        private void WriteSingleCoil(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream, int portIn, IPAddress ipAddressIn)
        {
            sendData.response = true;
            sendData.transactionIdentifier = receiveData.transactionIdentifier;
            sendData.protocolIdentifier = receiveData.protocolIdentifier;
            sendData.unitIdentifier = unitIdentifier;
            sendData.functionCode = receiveData.functionCode;
            sendData.startingAdress = receiveData.startingAdress;
            sendData.receiveCoilValues = receiveData.receiveCoilValues;
            byte[] array;
            byte[] array2;
            checked
            {
                if ((receiveData.receiveCoilValues[0] != 0) & (receiveData.receiveCoilValues[0] != 65280))
                {
                    sendData.errorCode = (byte)(unchecked((int)receiveData.functionCode) + 128);
                    sendData.exceptionCode = 3;
                }

                if ((unchecked((int)receiveData.startingAdress) + 1 > 65535) | (receiveData.startingAdress < 0))
                {
                    sendData.errorCode = (byte)(unchecked((int)receiveData.functionCode) + 128);
                    sendData.exceptionCode = 2;
                }

                if (sendData.exceptionCode == 0)
                {
                    if (receiveData.receiveCoilValues[0] == 65280)
                    {
                        lock (lockCoils)
                        {
                            coils[unchecked((int)receiveData.startingAdress) + 1] = true;
                        }
                    }

                    if (receiveData.receiveCoilValues[0] == 0)
                    {
                        lock (lockCoils)
                        {
                            coils[unchecked((int)receiveData.startingAdress) + 1] = false;
                        }
                    }
                }

                if (sendData.exceptionCode > 0)
                {
                    sendData.length = 3;
                }
                else
                {
                    sendData.length = 6;
                }

                bool flag = true;
                array = ((sendData.exceptionCode <= 0) ? new byte[12 + 2 * Convert.ToInt32(serialFlag)] : new byte[9 + 2 * Convert.ToInt32(serialFlag)]);
                array2 = new byte[2];
                sendData.length = (byte)(array.Length - 6);
            }

            array2 = BitConverter.GetBytes((int)sendData.transactionIdentifier);
            array[0] = array2[1];
            array[1] = array2[0];
            array2 = BitConverter.GetBytes((int)sendData.protocolIdentifier);
            array[2] = array2[1];
            array[3] = array2[0];
            array2 = BitConverter.GetBytes((int)sendData.length);
            array[4] = array2[1];
            array[5] = array2[0];
            array[6] = sendData.unitIdentifier;
            array[7] = sendData.functionCode;
            if (sendData.exceptionCode > 0)
            {
                array[7] = sendData.errorCode;
                array[8] = sendData.exceptionCode;
                sendData.sendRegisterValues = null;
            }
            else
            {
                array2 = BitConverter.GetBytes((int)receiveData.startingAdress);
                array[8] = array2[1];
                array[9] = array2[0];
                array2 = BitConverter.GetBytes((int)receiveData.receiveCoilValues[0]);
                array[10] = array2[1];
                array[11] = array2[0];
            }

            checked
            {
                try
                {
                    if (serialFlag)
                    {
                        if (!serialport.IsOpen)
                        {
                            throw new SerialPortNotOpenedException("serial port not opened");
                        }

                        sendData.crc = ModbusClient.calculateCRC(array, Convert.ToUInt16(array.Length - 8), 6);
                        array2 = BitConverter.GetBytes(unchecked((int)sendData.crc));
                        array[array.Length - 2] = array2[0];
                        array[array.Length - 1] = array2[1];
                        serialport.Write(array, 6, array.Length - 6);


                        byte[] array3 = new byte[array.Length - 6];
                        Array.Copy(array, 6, array3, 0, array.Length - 6);
                        //Telegraph
                        SendCompleted?.Invoke(array3);
                        if (debug)
                        {
                            StoreLogData.Instance.Store("Send Serial-Data: " + BitConverter.ToString(array3), DateTime.Now);
                        }

                    }
                    else if (udpFlag)
                    {
                        IPEndPoint endPoint = new IPEndPoint(ipAddressIn, portIn);
                        udpClient.Send(array, array.Length, endPoint);
                    }
                    else
                    {
                        stream.Write(array, 0, array.Length);
                        //Telegraph
                        SendCompleted?.Invoke(array);
                        if (debug)
                        {
                            StoreLogData.Instance.Store("Send Data: " + BitConverter.ToString(array), DateTime.Now);
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }

                if (this.CoilsChanged != null)
                {
                    this.CoilsChanged(unchecked((int)receiveData.startingAdress) + 1, 1);
                }
            }
        }

        private void WriteSingleRegister(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream, int portIn, IPAddress ipAddressIn)
        {
            sendData.response = true;
            sendData.transactionIdentifier = receiveData.transactionIdentifier;
            sendData.protocolIdentifier = receiveData.protocolIdentifier;
            sendData.unitIdentifier = unitIdentifier;
            sendData.functionCode = receiveData.functionCode;
            sendData.startingAdress = receiveData.startingAdress;
            sendData.receiveRegisterValues = receiveData.receiveRegisterValues;
            byte[] array;
            byte[] array2;
            checked
            {
                if ((receiveData.receiveRegisterValues[0] < 0) | (receiveData.receiveRegisterValues[0] > ushort.MaxValue))
                {
                    sendData.errorCode = (byte)(unchecked((int)receiveData.functionCode) + 128);
                    sendData.exceptionCode = 3;
                }

                if ((unchecked((int)receiveData.startingAdress) + 1 > 65535) | (receiveData.startingAdress < 0))
                {
                    sendData.errorCode = (byte)(unchecked((int)receiveData.functionCode) + 128);
                    sendData.exceptionCode = 2;
                }

                if (sendData.exceptionCode == 0)
                {
                    lock (lockHoldingRegisters)
                    {
                        holdingRegisters[unchecked((int)receiveData.startingAdress) + 1] = unchecked((short)receiveData.receiveRegisterValues[0]);
                    }
                }

                if (sendData.exceptionCode > 0)
                {
                    sendData.length = 3;
                }
                else
                {
                    sendData.length = 6;
                }

                bool flag = true;
                array = ((sendData.exceptionCode <= 0) ? new byte[12 + 2 * Convert.ToInt32(serialFlag)] : new byte[9 + 2 * Convert.ToInt32(serialFlag)]);
                array2 = new byte[2];
                sendData.length = (byte)(array.Length - 6);
            }

            array2 = BitConverter.GetBytes((int)sendData.transactionIdentifier);
            array[0] = array2[1];
            array[1] = array2[0];
            array2 = BitConverter.GetBytes((int)sendData.protocolIdentifier);
            array[2] = array2[1];
            array[3] = array2[0];
            array2 = BitConverter.GetBytes((int)sendData.length);
            array[4] = array2[1];
            array[5] = array2[0];
            array[6] = sendData.unitIdentifier;
            array[7] = sendData.functionCode;
            if (sendData.exceptionCode > 0)
            {
                array[7] = sendData.errorCode;
                array[8] = sendData.exceptionCode;
                sendData.sendRegisterValues = null;
            }
            else
            {
                array2 = BitConverter.GetBytes((int)receiveData.startingAdress);
                array[8] = array2[1];
                array[9] = array2[0];
                array2 = BitConverter.GetBytes((int)receiveData.receiveRegisterValues[0]);
                array[10] = array2[1];
                array[11] = array2[0];
            }

            checked
            {
                try
                {
                    if (serialFlag)
                    {
                        if (!serialport.IsOpen)
                        {
                            throw new SerialPortNotOpenedException("serial port not opened");
                        }

                        sendData.crc = ModbusClient.calculateCRC(array, Convert.ToUInt16(array.Length - 8), 6);
                        array2 = BitConverter.GetBytes(unchecked((int)sendData.crc));
                        array[array.Length - 2] = array2[0];
                        array[array.Length - 1] = array2[1];
                        serialport.Write(array, 6, array.Length - 6);

                        byte[] array3 = new byte[array.Length - 6];
                        Array.Copy(array, 6, array3, 0, array.Length - 6);
                        //Telegraph
                        SendCompleted?.Invoke(array3);
                        if (debug)
                        {
                            StoreLogData.Instance.Store("Send Serial-Data: " + BitConverter.ToString(array3), DateTime.Now);
                        }

                    }
                    else if (udpFlag)
                    {
                        IPEndPoint endPoint = new IPEndPoint(ipAddressIn, portIn);
                        udpClient.Send(array, array.Length, endPoint);
                    }
                    else
                    {
                        stream.Write(array, 0, array.Length);
                        //Telegraph
                        SendCompleted?.Invoke(array);
                        if (debug)
                        {
                            StoreLogData.Instance.Store("Send Data: " + BitConverter.ToString(array), DateTime.Now);
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }

                if (this.HoldingRegistersChanged != null)
                {
                    this.HoldingRegistersChanged(unchecked((int)receiveData.startingAdress) + 1, 1);
                }
            }
        }

        private void WriteMultipleCoils(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream, int portIn, IPAddress ipAddressIn)
        {
            sendData.response = true;
            sendData.transactionIdentifier = receiveData.transactionIdentifier;
            sendData.protocolIdentifier = receiveData.protocolIdentifier;
            sendData.unitIdentifier = unitIdentifier;
            sendData.functionCode = receiveData.functionCode;
            sendData.startingAdress = receiveData.startingAdress;
            sendData.quantity = receiveData.quantity;
            byte[] array;
            byte[] array2;
            checked
            {
                if ((receiveData.quantity == 0) | (receiveData.quantity > 1968))
                {
                    sendData.errorCode = (byte)(unchecked((int)receiveData.functionCode) + 128);
                    sendData.exceptionCode = 3;
                }

                if ((unchecked((int)receiveData.startingAdress) + 1 + unchecked((int)receiveData.quantity) > 65535) | (receiveData.startingAdress < 0))
                {
                    sendData.errorCode = (byte)(unchecked((int)receiveData.functionCode) + 128);
                    sendData.exceptionCode = 2;
                }

                if (sendData.exceptionCode == 0)
                {
                    lock (lockCoils)
                    {
                        for (int i = 0; i < receiveData.quantity; i++)
                        {
                            int num = unchecked(i % 16);
                            int num2 = 1;
                            num2 <<= num;
                            if ((receiveData.receiveCoilValues[unchecked(i / 16)] & (ushort)num2) == 0)
                            {
                                coils[unchecked((int)receiveData.startingAdress) + i + 1] = false;
                            }
                            else
                            {
                                coils[unchecked((int)receiveData.startingAdress) + i + 1] = true;
                            }
                        }
                    }
                }

                if (sendData.exceptionCode > 0)
                {
                    sendData.length = 3;
                }
                else
                {
                    sendData.length = 6;
                }

                bool flag = true;
                array = ((sendData.exceptionCode <= 0) ? new byte[12 + 2 * Convert.ToInt32(serialFlag)] : new byte[9 + 2 * Convert.ToInt32(serialFlag)]);
                array2 = new byte[2];
                sendData.length = (byte)(array.Length - 6);
            }

            array2 = BitConverter.GetBytes((int)sendData.transactionIdentifier);
            array[0] = array2[1];
            array[1] = array2[0];
            array2 = BitConverter.GetBytes((int)sendData.protocolIdentifier);
            array[2] = array2[1];
            array[3] = array2[0];
            array2 = BitConverter.GetBytes((int)sendData.length);
            array[4] = array2[1];
            array[5] = array2[0];
            array[6] = sendData.unitIdentifier;
            array[7] = sendData.functionCode;
            if (sendData.exceptionCode > 0)
            {
                array[7] = sendData.errorCode;
                array[8] = sendData.exceptionCode;
                sendData.sendRegisterValues = null;
            }
            else
            {
                array2 = BitConverter.GetBytes((int)receiveData.startingAdress);
                array[8] = array2[1];
                array[9] = array2[0];
                array2 = BitConverter.GetBytes((int)receiveData.quantity);
                array[10] = array2[1];
                array[11] = array2[0];
            }

            checked
            {
                try
                {
                    if (serialFlag)
                    {
                        if (!serialport.IsOpen)
                        {
                            throw new SerialPortNotOpenedException("serial port not opened");
                        }

                        sendData.crc = ModbusClient.calculateCRC(array, Convert.ToUInt16(array.Length - 8), 6);
                        array2 = BitConverter.GetBytes(unchecked((int)sendData.crc));
                        array[array.Length - 2] = array2[0];
                        array[array.Length - 1] = array2[1];
                        serialport.Write(array, 6, array.Length - 6);

                        byte[] array3 = new byte[array.Length - 6];
                        Array.Copy(array, 6, array3, 0, array.Length - 6);
                        //Telegraph
                        SendCompleted?.Invoke(array3);
                        if (debug)
                        {
                            StoreLogData.Instance.Store("Send Serial-Data: " + BitConverter.ToString(array3), DateTime.Now);
                        }

                    }
                    else if (udpFlag)
                    {
                        IPEndPoint endPoint = new IPEndPoint(ipAddressIn, portIn);
                        udpClient.Send(array, array.Length, endPoint);
                    }
                    else
                    {
                        stream.Write(array, 0, array.Length);
                        //Telegraph
                        SendCompleted?.Invoke(array);
                        if (debug)
                        {
                            StoreLogData.Instance.Store("Send Data: " + BitConverter.ToString(array), DateTime.Now);
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }

                if (this.CoilsChanged != null)
                {
                    this.CoilsChanged(unchecked((int)receiveData.startingAdress) + 1, receiveData.quantity);
                }
            }
        }

        private void WriteMultipleRegisters(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream, int portIn, IPAddress ipAddressIn)
        {
            sendData.response = true;
            sendData.transactionIdentifier = receiveData.transactionIdentifier;
            sendData.protocolIdentifier = receiveData.protocolIdentifier;
            sendData.unitIdentifier = unitIdentifier;
            sendData.functionCode = receiveData.functionCode;
            sendData.startingAdress = receiveData.startingAdress;
            sendData.quantity = receiveData.quantity;
            byte[] array;
            byte[] array2;
            checked
            {
                if ((receiveData.quantity == 0) | (receiveData.quantity > 1968))
                {
                    sendData.errorCode = (byte)(unchecked((int)receiveData.functionCode) + 128);
                    sendData.exceptionCode = 3;
                }

                if ((unchecked((int)receiveData.startingAdress) + 1 + unchecked((int)receiveData.quantity) > 65535) | (receiveData.startingAdress < 0))
                {
                    sendData.errorCode = (byte)(unchecked((int)receiveData.functionCode) + 128);
                    sendData.exceptionCode = 2;
                }

                if (sendData.exceptionCode == 0)
                {
                    lock (lockHoldingRegisters)
                    {
                        for (int i = 0; i < receiveData.quantity; i++)
                        {
                            holdingRegisters[unchecked((int)receiveData.startingAdress) + i + 1] = unchecked((short)receiveData.receiveRegisterValues[i]);
                        }
                    }
                }

                if (sendData.exceptionCode > 0)
                {
                    sendData.length = 3;
                }
                else
                {
                    sendData.length = 6;
                }

                bool flag = true;
                array = ((sendData.exceptionCode <= 0) ? new byte[12 + 2 * Convert.ToInt32(serialFlag)] : new byte[9 + 2 * Convert.ToInt32(serialFlag)]);
                array2 = new byte[2];
                sendData.length = (byte)(array.Length - 6);
            }

            array2 = BitConverter.GetBytes((int)sendData.transactionIdentifier);
            array[0] = array2[1];
            array[1] = array2[0];
            array2 = BitConverter.GetBytes((int)sendData.protocolIdentifier);
            array[2] = array2[1];
            array[3] = array2[0];
            array2 = BitConverter.GetBytes((int)sendData.length);
            array[4] = array2[1];
            array[5] = array2[0];
            array[6] = sendData.unitIdentifier;
            array[7] = sendData.functionCode;
            if (sendData.exceptionCode > 0)
            {
                array[7] = sendData.errorCode;
                array[8] = sendData.exceptionCode;
                sendData.sendRegisterValues = null;
            }
            else
            {
                array2 = BitConverter.GetBytes((int)receiveData.startingAdress);
                array[8] = array2[1];
                array[9] = array2[0];
                array2 = BitConverter.GetBytes((int)receiveData.quantity);
                array[10] = array2[1];
                array[11] = array2[0];
            }

            checked
            {
                try
                {
                    if (serialFlag)
                    {
                        if (!serialport.IsOpen)
                        {
                            throw new SerialPortNotOpenedException("serial port not opened");
                        }

                        sendData.crc = ModbusClient.calculateCRC(array, Convert.ToUInt16(array.Length - 8), 6);
                        array2 = BitConverter.GetBytes(unchecked((int)sendData.crc));
                        array[array.Length - 2] = array2[0];
                        array[array.Length - 1] = array2[1];
                        serialport.Write(array, 6, array.Length - 6);

                        byte[] array3 = new byte[array.Length - 6];
                        Array.Copy(array, 6, array3, 0, array.Length - 6);
                        //Telegraph
                        SendCompleted?.Invoke(array3);
                        if (debug)
                        {
                            StoreLogData.Instance.Store("Send Serial-Data: " + BitConverter.ToString(array3), DateTime.Now);
                        }

                    }
                    else if (udpFlag)
                    {
                        IPEndPoint endPoint = new IPEndPoint(ipAddressIn, portIn);
                        udpClient.Send(array, array.Length, endPoint);
                    }
                    else
                    {
                        stream.Write(array, 0, array.Length);
                        //Telegraph
                        SendCompleted?.Invoke(array);
                        if (debug)
                        {
                            StoreLogData.Instance.Store("Send Data: " + BitConverter.ToString(array), DateTime.Now);
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }

                if (this.HoldingRegistersChanged != null)
                {
                    this.HoldingRegistersChanged(unchecked((int)receiveData.startingAdress) + 1, receiveData.quantity);
                }
            }
        }

        private void ReadWriteMultipleRegisters(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream, int portIn, IPAddress ipAddressIn)
        {
            sendData.response = true;
            sendData.transactionIdentifier = receiveData.transactionIdentifier;
            sendData.protocolIdentifier = receiveData.protocolIdentifier;
            sendData.unitIdentifier = unitIdentifier;
            sendData.functionCode = receiveData.functionCode;
            byte[] array;
            byte[] array2;
            checked
            {
                if ((receiveData.quantityRead < 1) | (receiveData.quantityRead > 125) | (receiveData.quantityWrite < 1) | (receiveData.quantityWrite > 121) | (receiveData.byteCount != unchecked((int)receiveData.quantityWrite) * 2))
                {
                    sendData.errorCode = (byte)(unchecked((int)receiveData.functionCode) + 128);
                    sendData.exceptionCode = 3;
                }

                if ((unchecked((int)receiveData.startingAddressRead) + 1 + unchecked((int)receiveData.quantityRead) > 65535) | (unchecked((int)receiveData.startingAddressWrite) + 1 + unchecked((int)receiveData.quantityWrite) > 65535) | (receiveData.quantityWrite < 0) | (receiveData.quantityRead < 0))
                {
                    sendData.errorCode = (byte)(unchecked((int)receiveData.functionCode) + 128);
                    sendData.exceptionCode = 2;
                }

                if (sendData.exceptionCode == 0)
                {
                    sendData.sendRegisterValues = new short[receiveData.quantityRead];
                    lock (lockHoldingRegisters)
                    {
                        Buffer.BlockCopy(holdingRegisters.localArray, unchecked((int)receiveData.startingAddressRead) * 2 + 2, sendData.sendRegisterValues, 0, unchecked((int)receiveData.quantityRead) * 2);
                    }

                    lock (holdingRegisters)
                    {
                        for (int i = 0; i < receiveData.quantityWrite; i++)
                        {
                            holdingRegisters[unchecked((int)receiveData.startingAddressWrite) + i + 1] = unchecked((short)receiveData.receiveRegisterValues[i]);
                        }
                    }

                    sendData.byteCount = (byte)(2 * unchecked((int)receiveData.quantityRead));
                }

                if (sendData.exceptionCode > 0)
                {
                    sendData.length = 3;
                }
                else
                {
                    sendData.length = Convert.ToUInt16(3 + 2 * unchecked((int)receiveData.quantityRead));
                }

                bool flag = true;
                array = ((sendData.exceptionCode <= 0) ? new byte[9 + unchecked((int)sendData.byteCount) + 2 * Convert.ToInt32(serialFlag)] : new byte[9 + 2 * Convert.ToInt32(serialFlag)]);
                array2 = new byte[2];
            }

            array2 = BitConverter.GetBytes((int)sendData.transactionIdentifier);
            array[0] = array2[1];
            array[1] = array2[0];
            array2 = BitConverter.GetBytes((int)sendData.protocolIdentifier);
            array[2] = array2[1];
            array[3] = array2[0];
            array2 = BitConverter.GetBytes((int)sendData.length);
            array[4] = array2[1];
            array[5] = array2[0];
            array[6] = sendData.unitIdentifier;
            array[7] = sendData.functionCode;
            array[8] = sendData.byteCount;
            checked
            {
                if (sendData.exceptionCode > 0)
                {
                    array[7] = sendData.errorCode;
                    array[8] = sendData.exceptionCode;
                    sendData.sendRegisterValues = null;
                }
                else if (sendData.sendRegisterValues != null)
                {
                    for (int j = 0; j < unchecked((int)sendData.byteCount / 2); j++)
                    {
                        array2 = BitConverter.GetBytes(sendData.sendRegisterValues[j]);
                        array[9 + j * 2] = array2[1];
                        array[10 + j * 2] = array2[0];
                    }
                }

                try
                {
                    if (serialFlag)
                    {
                        if (!serialport.IsOpen)
                        {
                            throw new SerialPortNotOpenedException("serial port not opened");
                        }

                        sendData.crc = ModbusClient.calculateCRC(array, Convert.ToUInt16(array.Length - 8), 6);
                        array2 = BitConverter.GetBytes(unchecked((int)sendData.crc));
                        array[array.Length - 2] = array2[0];
                        array[array.Length - 1] = array2[1];
                        serialport.Write(array, 6, array.Length - 6);

                        byte[] array3 = new byte[array.Length - 6];
                        Array.Copy(array, 6, array3, 0, array.Length - 6);
                        //Telegraph
                        SendCompleted?.Invoke(array3);
                        if (debug)
                        {
                            StoreLogData.Instance.Store("Send Serial-Data: " + BitConverter.ToString(array3), DateTime.Now);
                        }

                    }
                    else if (udpFlag)
                    {
                        IPEndPoint endPoint = new IPEndPoint(ipAddressIn, portIn);
                        udpClient.Send(array, array.Length, endPoint);
                    }
                    else
                    {
                        stream.Write(array, 0, array.Length);
                        if (debug)
                        {
                            StoreLogData.Instance.Store("Send Data: " + BitConverter.ToString(array), DateTime.Now);
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }

                if (this.HoldingRegistersChanged != null)
                {
                    this.HoldingRegistersChanged(unchecked((int)receiveData.startingAddressWrite) + 1, receiveData.quantityWrite);
                }
            }
        }

        private void sendException(int errorCode, int exceptionCode, ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream, int portIn, IPAddress ipAddressIn)
        {
            sendData.response = true;
            sendData.transactionIdentifier = receiveData.transactionIdentifier;
            sendData.protocolIdentifier = receiveData.protocolIdentifier;
            sendData.unitIdentifier = receiveData.unitIdentifier;
            byte[] array;
            byte[] array2;
            checked
            {
                sendData.errorCode = (byte)errorCode;
                sendData.exceptionCode = (byte)exceptionCode;
                if (sendData.exceptionCode > 0)
                {
                    sendData.length = 3;
                }
                else
                {
                    sendData.length = (ushort)(3 + unchecked((int)sendData.byteCount));
                }

                bool flag = true;
                array = ((sendData.exceptionCode <= 0) ? new byte[9 + unchecked((int)sendData.byteCount) + 2 * Convert.ToInt32(serialFlag)] : new byte[9 + 2 * Convert.ToInt32(serialFlag)]);
                array2 = new byte[2];
                sendData.length = (byte)(array.Length - 6);
            }

            array2 = BitConverter.GetBytes((int)sendData.transactionIdentifier);
            array[0] = array2[1];
            array[1] = array2[0];
            array2 = BitConverter.GetBytes((int)sendData.protocolIdentifier);
            array[2] = array2[1];
            array[3] = array2[0];
            array2 = BitConverter.GetBytes((int)sendData.length);
            array[4] = array2[1];
            array[5] = array2[0];
            array[6] = sendData.unitIdentifier;
            array[7] = sendData.errorCode;
            array[8] = sendData.exceptionCode;
            checked
            {
                try
                {
                    if (serialFlag)
                    {
                        if (!serialport.IsOpen)
                        {
                            throw new SerialPortNotOpenedException("serial port not opened");
                        }

                        sendData.crc = ModbusClient.calculateCRC(array, Convert.ToUInt16(array.Length - 8), 6);
                        array2 = BitConverter.GetBytes(unchecked((int)sendData.crc));
                        array[array.Length - 2] = array2[0];
                        array[array.Length - 1] = array2[1];
                        serialport.Write(array, 6, array.Length - 6);

                        byte[] array3 = new byte[array.Length - 6];
                        Array.Copy(array, 6, array3, 0, array.Length - 6);
                        //Telegraph
                        SendCompleted?.Invoke(array3);
                        if (debug)
                        {
                            StoreLogData.Instance.Store("Send Serial-Data: " + BitConverter.ToString(array3), DateTime.Now);
                        }

                    }
                    else if (udpFlag)
                    {
                        IPEndPoint endPoint = new IPEndPoint(ipAddressIn, portIn);
                        udpClient.Send(array, array.Length, endPoint);
                    }
                    else
                    {
                        stream.Write(array, 0, array.Length);
                        if (debug)
                        {
                            StoreLogData.Instance.Store("Send Data: " + BitConverter.ToString(array), DateTime.Now);
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        private void CreateLogData(ModbusProtocol receiveData, ModbusProtocol sendData)
        {
            checked
            {
                for (int i = 0; i < 98; i++)
                {
                    modbusLogData[99 - i] = modbusLogData[99 - i - 2];
                }

                modbusLogData[0] = receiveData;
                modbusLogData[1] = sendData;
            }
        }
    }

}
