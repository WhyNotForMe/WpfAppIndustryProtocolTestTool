using System.Collections.Generic;
using System.Linq;
using SerialPort = System.IO.Ports;


namespace WpfAppIndustryProtocolTestTool.Model
{
    public class SerialPortParaModel
    {
        public List<string> PortName { get; set; }
        public List<string> BaudRate { get; set; }
        public List<string> Parity { get; set; }
        public List<string> DataBits { get; set; }
        public List<string> StopBits { get; set; }
        public List<string> Handshake { get; set; }
        public List<string> ReceiveBytesThreshold { get; set; }
        public List<string> ReadTimeout { get; set; }
        public List<string> WriteTimeout { get; set; }
        public List<string> ReadBufferSize { get; set; }
        public List<string> WriteBufferSize { get; set; }




        public SerialPortParaModel()
        {
            try
            {
                PortName = SerialPort.SerialPort.GetPortNames().ToList();

                BaudRate = new List<string> { "110", "300", "600", "1200", "2400", "4800", "9600", "14400", "19200", "56000", "115200" };

                Parity = System.Enum.GetNames(typeof(SerialPort.Parity)).ToList();

                DataBits = new List<string> { "5", "6", "7", "8" };

                StopBits = System.Enum.GetNames(typeof(SerialPort.StopBits)).ToList();

                Handshake = System.Enum.GetNames(typeof(SerialPort.Handshake)).ToList();

                ReceiveBytesThreshold = new List<string> { "1", "2", "4", "8" };

                ReadTimeout = new List<string> { "-1", "10000", "60000" };

                WriteTimeout = new List<string> { "-1", "10000", "60000" };

                ReadBufferSize = new List<string> { "100", "255", "1024", "2048", "4096" };

                WriteBufferSize = new List<string> { "100", "255", "1024", "2048", "4096" };
            }
            catch (System.Exception)
            {

                throw;
            }


        }
    }
}
