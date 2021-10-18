using System.Net;
using System.Net.Sockets;

namespace WpfAppIndustryProtocolTestTool.BLL.ModbusProtocol
{
    internal struct NetworkConnectionParameter
    {
        public NetworkStream stream;

        public byte[] bytes;

        public int portIn;

        public IPAddress ipAddressIn;
    }
}
