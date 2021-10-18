using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppIndustryProtocolTestTool.BLL.SerialPortProtocol
{
    public delegate void OnReceiveComplete(byte[] rcvArray);
    public delegate void OnSendComplete(byte[] sndArray);
}
