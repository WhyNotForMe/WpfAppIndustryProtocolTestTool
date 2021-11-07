using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace WpfAppIndustryProtocolTestTool.BLL.ModbusProtocol
{
    internal class TCPHandler
    {
        public delegate void DataChanged(object networkConnectionParameter);

        public delegate void NumberOfClientsChanged();

        internal class Client
        {
            private readonly TcpClient tcpClient;

            private readonly byte[] buffer;

            public long Ticks
            {
                get;
                set;
            }

            public TcpClient TcpClient => tcpClient;

            public byte[] Buffer => buffer;

            public NetworkStream NetworkStream => tcpClient.GetStream();

            public Client(TcpClient tcpClient)
            {
                this.tcpClient = tcpClient;
                int receiveBufferSize = tcpClient.ReceiveBufferSize;
                buffer = new byte[receiveBufferSize];
            }
        }

        private TcpListener server ;

        private List<Client> tcpClientLastRequestList = new List<Client>();

        public string ipAddress ;

        private IPAddress localIPAddress = IPAddress.Any;

        public int NumberOfConnectedClients
        {
            get;
            set;
        }

        public IPAddress LocalIPAddress => localIPAddress;

        public event DataChanged? dataChanged;

        public event NumberOfClientsChanged? numberOfClientsChanged;

        //
        // Summary:
        //     Listen to all network interfaces.
        //
        // Parameters:
        //   port:
        //     TCP port to listen
        public TCPHandler(int port)
        {
            server = new TcpListener(LocalIPAddress, port);
            server.Start();
            server.BeginAcceptTcpClient(AcceptTcpClientCallback, null);
        }

        //
        // Summary:
        //     Listen to a specific network interface.
        //
        // Parameters:
        //   localIPAddress:
        //     IP address of network interface to listen
        //
        //   port:
        //     TCP port to listen
        public TCPHandler(IPAddress localIPAddress, int port)
        {
            this.localIPAddress = localIPAddress;
            server = new TcpListener(LocalIPAddress, port);
            server.Start();
            server.BeginAcceptTcpClient(AcceptTcpClientCallback, null);
        }

        private void AcceptTcpClientCallback(IAsyncResult asyncResult)
        {
            TcpClient tcpClient = new TcpClient();
            try
            {
                tcpClient = server.EndAcceptTcpClient(asyncResult);
                tcpClient.ReceiveTimeout = 4000;
                if (ipAddress != null)
                {
                    string? text = tcpClient.Client.RemoteEndPoint.ToString();
                    text = text?.Split(':')[0];
                    if (text != ipAddress)
                    {
                        tcpClient.Client.Disconnect(reuseSocket: false);
                        return;
                    }
                }
            }
            catch (Exception)
            {
            }

            try
            {
                server.BeginAcceptTcpClient(AcceptTcpClientCallback, null);
                Client client = new Client(tcpClient);
                NetworkStream networkStream = client.NetworkStream;
                networkStream.ReadTimeout = 4000;
                networkStream.BeginRead(client.Buffer, 0, client.Buffer.Length, ReadCallback, client);
            }
            catch (Exception)
            {
            }
        }

        private int GetAndCleanNumberOfConnectedClients(Client client)
        {
            lock (this)
            {
                int num = 0;
                bool flag = false;
                foreach (Client tcpClientLastRequest in tcpClientLastRequestList)
                {
                    if (client.Equals(tcpClientLastRequest))
                    {
                        flag = true;
                    }
                }

                try
                {
                    tcpClientLastRequestList.RemoveAll((Client c) => checked(DateTime.Now.Ticks - c.Ticks) > 40000000);
                }
                catch (Exception)
                {
                }

                if (!flag)
                {
                    tcpClientLastRequestList.Add(client);
                }

                return tcpClientLastRequestList.Count;
            }
        }

        private void ReadCallback(IAsyncResult asyncResult)
        {
            NetworkConnectionParameter networkConnectionParameter = default(NetworkConnectionParameter);
            Client? client = asyncResult.AsyncState as Client;
            if (client!=null)
            {
                client.Ticks = DateTime.Now.Ticks;
                NumberOfConnectedClients = GetAndCleanNumberOfConnectedClients(client);
            }
            if (this.numberOfClientsChanged != null)
            {
                this.numberOfClientsChanged();
            }

            if (client == null)
            {
                return;
            }

            NetworkStream? networkStream = null;
            int num;
            try
            {
                networkStream = client.NetworkStream;
                num = networkStream.EndRead(asyncResult);
            }
            catch (Exception)
            {
                return;
            }

            if (num != 0)
            {
                byte[] array = new byte[num];
                Buffer.BlockCopy(client.Buffer, 0, array, 0, num);
                networkConnectionParameter.bytes = array;
                networkConnectionParameter.stream = networkStream;
                if (this.dataChanged != null)
                {
                    this.dataChanged(networkConnectionParameter);
                }

                try
                {
                    networkStream.BeginRead(client.Buffer, 0, client.Buffer.Length, ReadCallback, client);
                }
                catch (Exception)
                {
                }
            }
        }

        public void Disconnect()
        {
            try
            {
                foreach (Client tcpClientLastRequest in tcpClientLastRequestList)
                {
                    tcpClientLastRequest.NetworkStream.Close(0);
                }
            }
            catch (Exception)
            {
            }

            server.Stop();
        }
    }
}
