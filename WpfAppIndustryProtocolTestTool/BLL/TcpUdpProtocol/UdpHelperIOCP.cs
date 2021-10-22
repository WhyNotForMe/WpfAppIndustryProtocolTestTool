using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WpfAppIndustryProtocolTestTool.Model.Enum;

namespace WpfAppIndustryProtocolTestTool.BLL.TcpUdpProtocol
{
    public class UdpHelperIOCP
    {
        Socket _socket;
        SocketAsyncEventArgs _readEventArg;
        SocketAsyncEventArgs _writeEventArg;
        IPEndPoint _localEndPoint;
        IPEndPoint _destinationEP;


        public event OnReceiveComplete ReceiveCompleted;
        public event OnSendComplete SendCompleted;
        public event OnMessageInform MessageInformed;
        public event OnUdpClientReceive UdpClientReceived;

        public void SetSinglecastMode(IPAddress destHost, int destPort)
        {
            _destinationEP = new IPEndPoint(destHost, destPort);

            MessageInformed?.Invoke($"Warning: Singlecast to {_destinationEP} !");
        }

        public void SetBroadcastMode(IPAddress broadcastHost, int broadcastPort)
        {
            try
            {
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
                _destinationEP = new IPEndPoint(broadcastHost, broadcastPort);

                MessageInformed?.Invoke($"Warning: Broadcast via {_destinationEP} !");
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void SetMulticastMode(IPAddress multicastGroup, int multicastPort)
        {
            try
            {
                _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, true);
                _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership,
                                                                    new MulticastOption(multicastGroup, multicastPort));
                _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, true);

                _destinationEP = new IPEndPoint(multicastGroup, multicastPort);

                MessageInformed?.Invoke($"Warning: Multicast to Group({multicastGroup}:{multicastPort}) !");
            }
            catch (Exception)
            {

                throw;
            }

        }

        public void ExitMulticastGroup(IPAddress multicastGroup, int multicastPort)
        {
            _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership,
                                                            new MulticastOption(multicastGroup, multicastPort));
            MessageInformed?.Invoke($"Warning: Exit Multicast Group({multicastGroup}:{multicastPort})");
        }

        public void StartServer(IPAddress iPAddress, int port)
        {
            try
            {
                _localEndPoint = new IPEndPoint(iPAddress, port);
                _socket.Bind(_localEndPoint);

                _readEventArg.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                bool willRaiseEvent = _socket.ReceiveFromAsync(_readEventArg);
                if (!willRaiseEvent)
                {
                    ProcessReceive(_readEventArg);
                };
            }
            catch (Exception)
            {

                throw;
            }

        }

        public void Stop()
        {
            try
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Dispose();
                _socket.Close();
            }
            catch (Exception)
            {

                throw;
            }

        }




        public void SendToAsync(byte[] sndData)
        {
            try
            {
                byte[] buffer = new byte[sndData.Length];
                Array.Copy(sndData, buffer, sndData.Length);
                _writeEventArg.SetBuffer(buffer, 0, buffer.Length);
                _writeEventArg.RemoteEndPoint = _destinationEP;

                bool willRaiseEvent = _socket.SendToAsync(_writeEventArg);
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


        public void ReceiveFromAsync(EndPoint remoteEndPoint)
        {
            try
            {
                _readEventArg.RemoteEndPoint = remoteEndPoint;

                bool willRaiseEvent = _socket.ReceiveFromAsync(_readEventArg);
                if (!willRaiseEvent)
                {
                    ProcessReceive(_readEventArg);
                };
            }
            catch (Exception)
            {

                throw;
            }
        }

        public UdpHelperIOCP(int receiveBufferSize)
        {
            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

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


        void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            // determine which type of operation just completed and call the associated handler
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.ReceiveFrom:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.SendTo:
                    ProcessSend(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a ReceiveFrom or SendTo");
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
                    UdpClientReceived?.Invoke(_socket);
                    ReceiveCompleted?.Invoke(e, data);
                }
                else
                {
                    MessageInformed?.Invoke("Warning: " + e.SocketError.ToString());
                    return;
                }
                bool willRaiseEvent = _socket.ReceiveFromAsync(e);
                if (!willRaiseEvent)
                {
                    ProcessReceive(e);
                };


            }
            catch (Exception)
            {

                throw;
            }
        }

        private void ProcessSend(SocketAsyncEventArgs e)
        {
            try
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
            catch (Exception)
            {

                throw;
            }

        }








    }
}
