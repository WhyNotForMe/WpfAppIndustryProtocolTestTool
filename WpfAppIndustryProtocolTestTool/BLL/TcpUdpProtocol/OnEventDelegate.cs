using System.Net.Sockets;

namespace WpfAppIndustryProtocolTestTool.BLL.TcpUdpProtocol
{
    public delegate void OnAcceptComplete(AsyncUserTokenIOCP token);
    public delegate void OnClientNumberChange(int num, AsyncUserTokenIOCP token);
    public delegate void OnDataReceive(AsyncUserTokenIOCP token, byte[] buff);
    public delegate void OnMessageInform(string message);
    public delegate void OnConnectComplete(SocketAsyncEventArgs e);
    public delegate void OnDisconnectComplete();
    public delegate void OnReceiveComplete(SocketAsyncEventArgs e, byte[] buffer);
    public delegate void OnSendComplete(SocketAsyncEventArgs e);

}
