namespace WpfAppIndustryProtocolTestTool.BLL.SerialPortProtocol
{
    public delegate void OnReceiveComplete(byte[] rcvArray);
    public delegate void OnSendComplete(byte[] sndArray);
}
