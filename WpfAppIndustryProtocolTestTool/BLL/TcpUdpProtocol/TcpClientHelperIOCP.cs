using System;
using System.Net;
using System.Net.Sockets;

namespace WpfAppIndustryProtocolTestTool.BLL.TcpUdpProtocol
{

    public class TcpClientHelperIOCP
    {
        Socket _socket;
        SocketAsyncEventArgs _connectEventArg;
        SocketAsyncEventArgs _readEventArg;
        SocketAsyncEventArgs _writeEventArg;
        IPEndPoint _remoteEndPoint;

        public event OnConnectComplete ConnectCompleted;
        public event OnDisconnectComplete DisconnectCompleted;
        public event OnReceiveComplete ReceiveCompleted;
        public event OnSendComplete SendCompleted;
        public event OnMessageInform MessageInformed;


        public void Connect(IPAddress iPAddress, int port)
        {

            try
            {

                _remoteEndPoint = new IPEndPoint(iPAddress, port);

                _connectEventArg = new SocketAsyncEventArgs { RemoteEndPoint = _remoteEndPoint };
                _connectEventArg.Completed += _connectArg_Completed;


                bool willRaiseEvent = _socket.ConnectAsync(_connectEventArg);
                if (!willRaiseEvent)
                {
                    ProcessConnected(_connectEventArg);
                }


            }
            catch (Exception)
            {
                throw;
            }
        }



        public void Disconnect()
        {
            try
            {
                _socket?.Shutdown(SocketShutdown.Both);

            }
            catch (Exception) { }
            finally
            {
                _connectEventArg?.Dispose();
                _readEventArg?.Dispose();
                _writeEventArg?.Dispose();
                _socket?.Close();
            }


        }

        public void SendAsync(byte[] sndData)
        {
            try
            {
                byte[] buffer = new byte[sndData.Length];
                Array.Copy(sndData, buffer, sndData.Length);

                _writeEventArg.RemoteEndPoint = _remoteEndPoint;
                _writeEventArg?.SetBuffer(buffer, 0, buffer.Length);

                bool willRaiseEvent = _socket.SendAsync(_writeEventArg);
                if (!willRaiseEvent)
                {
                    ProcessSend(_writeEventArg);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public TcpClientHelperIOCP(int receiveBufferSize)
        {
            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                _readEventArg = new SocketAsyncEventArgs();
                _readEventArg.SetBuffer(new byte[receiveBufferSize * 1024], 0, receiveBufferSize * 1024);
                _readEventArg.Completed += IO_Completed;

                _writeEventArg = new SocketAsyncEventArgs();
                _writeEventArg.Completed += IO_Completed;

            }
            catch (Exception)
            {

                throw;
            }

        }

        private void _connectArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessConnected(e);
        }

        private void ProcessConnected(SocketAsyncEventArgs e)
        {
            _readEventArg.RemoteEndPoint = _remoteEndPoint;
            MessageInformed?.Invoke($"Connected to TCP Server [{_remoteEndPoint}] at {DateTime.Now.ToLocalTime().ToString("yyyyy-M-dd HH:mm:ss.FFF")}");
            ConnectCompleted?.Invoke(e);

            bool willRaiseEvent = _socket.ReceiveAsync(_readEventArg);
            if (!willRaiseEvent)
            {
                ProcessReceive(_readEventArg);
            };
        }

        void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            // determine which type of operation just completed and call the associated handler
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }

        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            try
            {
                //if (e.SocketError == SocketError.OperationAborted)
                //{
                //    return;
                //}

                if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
                {
                    //Get Data From SocketAsyncEventArgs.Buffer
                    byte[] data = new byte[e.BytesTransferred];
                    Array.Copy(e.Buffer, 0, data, 0, e.BytesTransferred);
                    ReceiveCompleted?.Invoke(e, data);

                    bool willRaiseEvent = _socket.ReceiveAsync(e);
                    if (!willRaiseEvent)
                    {
                        ProcessReceive(e);
                    };

                }
                else if (e.SocketError == SocketError.Success && e.BytesTransferred == 0)
                {
                    MessageInformed?.Invoke($"Warning: Disconnected to TCP Server [{_remoteEndPoint}] at {DateTime.Now.ToLocalTime().ToString("yyyyy-M-dd HH:mm:ss.FFF")}");
                    DisconnectCompleted?.Invoke(e);
                    return;
                }
                else
                {
                    return;
                }




            }
            catch (Exception)
            {

                throw;
            }
        }

        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                SendCompleted?.Invoke(e);
            }
            else
            {
                MessageInformed?.Invoke("Warning: " + e.SocketError.ToString());
                return;
            }
        }



    }
}
