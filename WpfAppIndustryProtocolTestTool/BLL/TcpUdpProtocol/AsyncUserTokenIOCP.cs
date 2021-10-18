using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WpfAppIndustryProtocolTestTool.Model;

namespace WpfAppIndustryProtocolTestTool.BLL.TcpUdpProtocol
{
    public class AsyncUserTokenIOCP
    {
        public Socket Socket { get; set; }
        public DateTime ConnectedTime { get; set; }
        //public List<byte> Buffer { get; set; }
        //public AsyncUserToken()
        //{
        //    Buffer = new List<byte>();
        //}
    }
}
