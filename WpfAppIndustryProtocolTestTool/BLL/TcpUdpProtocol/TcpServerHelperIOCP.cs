using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace WpfAppIndustryProtocolTestTool.BLL.TcpUdpProtocol
{


    public class TcpServerHelperIOCP
    {
        private int m_numConnections;       // the maximum number of connections the sample is designed to handle simultaneously
        private int m_receiveBufferSize;    // buffer size to use for each socket I/O operation
        BufferManagerIOCP m_bufferManager;      // represents a large reusable set of buffers for all socket operations
        const int opsToPreAlloc = 2;        // read, write (don't alloc buffer space for accepts)
        Socket? listenSocket;                // the socket used to listen for incoming connection requests
        EventArgsPoolIOCP m_readEventArgsPool;    // pool of reusable SocketAsyncEventArgs objects for write, read and accept socket operations

        int m_totalBytesRead;               // counter of the total # bytes received by the server
        int m_numConnectedSockets;          // the total number of clients connected to the server
        Semaphore m_maxNumberAcceptedClients;

        List<AsyncUserTokenIOCP>? m_clients;

        public event OnAcceptComplete? AcceptCompleted;
        public event OnClientNumberChange? ClientNumberChanged;
        public event OnDataReceive? DataReceived;
        public event OnMessageInform? MessageInformed;
        public event OnSendComplete? SendCompleted;

        public List<AsyncUserTokenIOCP> ClientList { get { return m_clients; } }



        // 1.Create an uninitialized server instance.
        // 2.To start the server listening for connection requests
        // 3.call the Init method followed by Start method

        // <param name="numConnections">the maximum number of connections the sample is designed to handle simultaneously</param>
        // <param name="receiveBufferSize">buffer size to use for each socket I/O operation</param>
        public TcpServerHelperIOCP(int numConnections, int receiveBufferSize)
        {
            try
            {
                m_totalBytesRead = 0;
                m_numConnectedSockets = 0;
                m_numConnections = numConnections;
                m_receiveBufferSize = Math.Abs(receiveBufferSize) * 1024;
                // allocate buffers such that the maximum number of sockets can have one outstanding read and
                //write posted to the socket simultaneously
                m_bufferManager = new BufferManagerIOCP(m_receiveBufferSize * m_numConnections * opsToPreAlloc, m_receiveBufferSize);

                m_readEventArgsPool = new EventArgsPoolIOCP(m_numConnections);
                m_maxNumberAcceptedClients = new Semaphore(m_numConnections, m_numConnections);
            }
            catch (Exception)
            {

                throw;
            }

        }

        /* Initializes the server by preallocating reusable buffers and context objects.
           These objects do not need to be preallocated or reused, 
           but it is done this way to illustrate how the API can easily be used to 
           create reusable objects to increase server performance.
        */
        public void Init()
        {
            try
            {
                // Allocates one large byte buffer which all I/O operations use a piece of.
                // This gaurds against memory fragmentation
                m_bufferManager.InitBuffer();
                m_clients = new List<AsyncUserTokenIOCP>();

                // preallocate pool of SocketAsyncEventArgs objects 
                SocketAsyncEventArgs readEventArg;

                for (int i = 0; i < m_numConnections; i++)
                {
                    //Pre-allocate a set of reusable SocketAsyncEventArgs
                    readEventArg = new SocketAsyncEventArgs();
                    readEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                    readEventArg.UserToken = new AsyncUserTokenIOCP();

                    // assign a byte buffer from the buffer pool to the SocketAsyncEventArg object
                    m_bufferManager.SetBuffer(readEventArg);

                    // add SocketAsyncEventArg to the pool
                    m_readEventArgsPool.Push(readEventArg);
                }
            }
            catch (Exception)
            {

                throw;
            }

        }

        // Starts the server such that it is listening for incoming connection requests.
        // <param name="localEndPoint">The endpoint which the server will listening for connection requests on</param>
        public bool Start(IPAddress iPAddress, int port)
        {
            try
            {

                m_clients.Clear();


                // create the socket which listens for incoming connections
                IPEndPoint localEndPoint = new IPEndPoint(iPAddress, port);
                listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listenSocket.Bind(localEndPoint);
                // start the server with a listen backlog of 100 connections
                //listenSocket.Listen(100);
                listenSocket.Listen(m_numConnections);

                // post accepts on the listening socket
                StartAccept(null);

                //Console.WriteLine("Press any key to terminate the server process....");
                //Console.ReadKey()
                MessageInformed?.Invoke($"TCP Server [{localEndPoint}] is Running");
                return true;
            }
            catch (Exception)
            {
                throw;
            }
           ;
        }


        #region Stop

        public void Stop()
        {
            foreach (AsyncUserTokenIOCP token in m_clients)
            {
                try
                {
                    token.Socket.Shutdown(SocketShutdown.Both);
                    token.Socket.Dispose();
                }
                catch (Exception) { }
            }

            try
            {
                if (m_numConnectedSockets == 0)
                {
                    listenSocket?.Shutdown(SocketShutdown.Both);
                }

            }
            catch (Exception) { }

            listenSocket?.Dispose();
            listenSocket?.Close();

            int c_count = m_clients.Count;
            lock (m_clients)
            {
                m_clients.Clear();
            }
            //ClientNumberChanged?.Invoke(-c_count, null);
        }

        public void CloseClient(AsyncUserTokenIOCP token)
        {
            try
            {
                token.Socket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception) { }
        }

        private void CloseClientSocket(SocketAsyncEventArgs e)
        {
            AsyncUserTokenIOCP? token = e.UserToken as AsyncUserTokenIOCP;

            if (token != null)
            {
                lock (m_clients)
                {
                    m_clients.Remove(token);
                }
                ClientNumberChanged?.Invoke(-1, token);
            }

            // close the socket associated with the client
            try
            {
                token.Socket.Shutdown(SocketShutdown.Both);
            }
            // throws if client process has already closed
            catch (Exception) { }

            token.Socket.Close();

            // decrement the counter keeping track of the total number of clients connected to the server
            Interlocked.Decrement(ref m_numConnectedSockets);
            m_maxNumberAcceptedClients.Release();

            // Free the SocketAsyncEventArg so they can be reused by another client
            e.UserToken = new AsyncUserTokenIOCP();
            m_readEventArgsPool.Push(e);


            //Console.WriteLine("A client has been disconnected from the server.
            //                   There are {0} clients connected to the server", m_numConnectedSockets);
            MessageInformed?.Invoke($"Warning: A client has been disconnected from the server!");
        }

        #endregion

        #region Accept

        // Begins an operation to accept a connection request from the client
        //
        // <param name="acceptEventArg">The context object to use when issuing the accept operation on the server's listening socket</param>
        private void StartAccept(SocketAsyncEventArgs? acceptEventArg)
        {
            try
            {
                if (acceptEventArg == null)
                {
                    acceptEventArg = new SocketAsyncEventArgs();
                    acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);
                }
                else
                {
                    // socket must be cleared since the context object is being reused
                    acceptEventArg.AcceptSocket = null;
                }

                m_maxNumberAcceptedClients.WaitOne();
                bool willRaiseEvent = listenSocket.AcceptAsync(acceptEventArg);
                if (!willRaiseEvent)
                {
                    ProcessAccept(acceptEventArg);
                }
            }
            catch (Exception)
            {

                throw;
            }

        }

        // This method is the callback method associated with Socket.AcceptAsync operations
        // and is invoked when an accept operation is complete
        void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                MessageInformed?.Invoke(e.SocketError.ToString());
                return;
            }

            try
            {
                Interlocked.Increment(ref m_numConnectedSockets);

                // Get the socket for the accepted client connection and put it into the ReadEventArg object user token
                SocketAsyncEventArgs readEventArg = m_readEventArgsPool.Pop();

                //((AsyncUserToken)readEventArgs.UserToken).Socket = e.AcceptSocket;
                AsyncUserTokenIOCP? userToken = readEventArg.UserToken as AsyncUserTokenIOCP;

                if (userToken != null)
                {
                    userToken.Socket = e.AcceptSocket;          //Socket for Session
                    userToken.ConnectedTime = DateTime.Now;

                    MessageInformed?.Invoke($"Client connection accepted.There are {m_numConnectedSockets} clients connected to server");
                    AcceptCompleted?.Invoke(userToken);

                    lock (m_clients)
                    {
                        m_clients.Add(userToken);
                    }

                    ClientNumberChanged?.Invoke(1, userToken);
                }


                // As soon as the client is connected, post a receive to the connection
                bool willRaiseEvent = e.AcceptSocket.ReceiveAsync(readEventArg);
                if (!willRaiseEvent)
                {
                    ProcessReceive(readEventArg);
                }
            }
            catch (Exception)
            {
                throw;
            }


            // Accept the next connection request
            StartAccept(e);
        }


        #endregion

        #region IO Completed

        // This method is called whenever a receive or send operation is completed on a socket
        //
        // <param name="e">SocketAsyncEventArg associated with the completed receive operation</param>
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

        // This method is invoked when an asynchronous receive operation completes.
        // If the remote host closed the connection, then the socket is closed.
        // If data was received then the data is echoed back to the client.
        //
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            try
            {
                // check if the remote host closed the connection
                AsyncUserTokenIOCP? token = e.UserToken as AsyncUserTokenIOCP;

                if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
                {
                    //increment the count of the total bytes receive by the server
                    Interlocked.Add(ref m_totalBytesRead, e.BytesTransferred);
                    //Console.WriteLine("The server has read a total of {0} bytes", m_totalBytesRead);


                    byte[] data = new byte[e.BytesTransferred];
                    //Get Data From SocketAsyncEventArgs.Buffer
                    Array.Copy(e.Buffer, e.Offset, data, 0, e.BytesTransferred);
                    //lock (token.Buffer)
                    //{
                    //    token.Buffer.AddRange(data);
                    //}

                    DataReceived?.Invoke(token, data);

                    bool willRaiseEvent = token.Socket.ReceiveAsync(e);
                    if (!willRaiseEvent)
                    {
                        ProcessReceive(e);

                    }


                }
                else if (e.BytesTransferred == 0 && e.SocketError == SocketError.Success)
                {
                    MessageInformed?.Invoke($"Warning: This Client [{e.RemoteEndPoint}] is Disconnected ! ");
                    CloseClientSocket(e);
                }
                else
                {
                    MessageInformed?.Invoke("Warning: " + e.SocketError.ToString());
                    CloseClientSocket(e);
                }
            }
            catch (Exception)
            {

                throw;
            }

        }

        // This method is invoked when an asynchronous send operation completes.
        // The method issues another receive on the socket to read any additional data sent from the client
        //
        // <param name="e"></param>
        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                // done echoing data back to the client
                AsyncUserTokenIOCP? token = e.UserToken as AsyncUserTokenIOCP;

                SendCompleted?.Invoke(e);
            }
            else
            {
                MessageInformed?.Invoke($"Warning: This Client [{e.RemoteEndPoint}] is Disconnected ! ");
                CloseClientSocket(e);
            }
        }

        #endregion

        public void SendAsync(AsyncUserTokenIOCP token, byte[]? buffer)
        {
            if (token == null || token.Socket == null || !token.Socket.Connected || buffer == null)
            {
                return;
            }

            try
            {
                SocketAsyncEventArgs sendEventArg = new SocketAsyncEventArgs();
                sendEventArg.UserToken = token;
                sendEventArg.RemoteEndPoint = token.Socket.RemoteEndPoint;
                sendEventArg.Completed += IO_Completed;
                sendEventArg.SetBuffer(buffer, 0, buffer.Length);

                bool willRaiseEvent = token.Socket.SendAsync(sendEventArg);
                if (!willRaiseEvent)
                {
                    ProcessSend(sendEventArg);
                }

            }
            catch (Exception)
            {
                throw;
            }
        }



    }



}
