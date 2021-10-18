using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppIndustryProtocolTestTool.BLL.ModbusProtocol
{
    public delegate void CoilsChangedHandler(int startCoil, int numberOfCoils);
    public delegate void HoldingRegistersChangedHandler(int startRegister, int numberOfRegisters);
    public delegate void NumberOfConnectedClientsChangedHandler();
    public delegate void LogDataChangedHandler();

    public delegate void OnReceiveComplete(byte[] rcvArray);
    public delegate void OnSendComplete(byte[] sndArray);
}
