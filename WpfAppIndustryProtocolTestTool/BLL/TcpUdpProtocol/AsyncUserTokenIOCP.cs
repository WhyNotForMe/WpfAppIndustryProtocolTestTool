using System;
using System.Net.Sockets;

namespace WpfAppIndustryProtocolTestTool.BLL.TcpUdpProtocol
{
    public class AsyncUserTokenIOCP
    {
        public Socket? Socket { get; set; }
        public DateTime ConnectedTime { get; set; }
        //public List<byte> Buffer { get; set; }
        //public AsyncUserToken()
        //{
        //    Buffer = new List<byte>();
        //}
    }
}
