using System;
using System.IO.Ports;
using WpfAppIndustryProtocolTestTool.BLL;
using WpfAppIndustryProtocolTestTool.Model.Enum;

namespace WpfAppIndustryProtocolTestTool.BLL.SerialPortProtocol
{
    public class SerialPortHelper
    {
        #region Single Instance

        private static SerialPortHelper _instance;
        private static object _locker = new object();

        private SerialPortHelper()
        {
            SerialPort = new SerialPort();
            SerialPort.DataReceived += SerialPort_DataReceived;

        }

        public static SerialPortHelper GetInstance()
        {
            if (_instance == null)
            {
                lock (_locker)
                {
                    if (_instance == null)
                    {
                        _instance = new SerialPortHelper();
                    }
                }
            }
            return _instance;
        }

        #endregion


        #region Fields

        byte[] _receivedTelegraph;

        #endregion

        public event OnReceiveComplete ReceiveCompleted;
        public event OnSendComplete SendCompleted;


        public SerialPort SerialPort { get; }


        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (SerialPort.IsOpen)
                {
                    _receivedTelegraph = new byte[SerialPort.BytesToRead];
                    int count = SerialPort.Read(_receivedTelegraph, 0, SerialPort.BytesToRead);

                    if (count > 0)
                    {
                        ReceiveCompleted?.Invoke(_receivedTelegraph);
                        SerialPort.DiscardInBuffer();
                    }
                }

            }
            catch (Exception)
            {
                throw;
            }
        }
        public void OpenPort()
        {
            try
            {
                if (SerialPort.IsOpen)
                {
                    SerialPort.Close();
                }

                SerialPort.Open();
            }
            catch (Exception )
            {

                throw ;
            }



        }

        public void ClosePort()
        {
            try
            {
                if (SerialPort.IsOpen)
                {
                    SerialPort.Close();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }




        public void SendData(byte[] sndBuffer, CRCEnum check = CRCEnum.None)
        {
            if (!SerialPort.IsOpen)
            {
                throw new Exception("Serial Port is not Open!");
            }
            else
            {
                try
                {
                    byte[] newBuffer = CRCHelper.AppendCRC(sndBuffer, check);
                    SerialPort.Write(newBuffer, 0, sndBuffer.Length);
                    SendCompleted?.Invoke(newBuffer);
                    SerialPort.DiscardOutBuffer();
                }
                catch (Exception)
                {
                    throw;
                }

            }
        }


    }
}
