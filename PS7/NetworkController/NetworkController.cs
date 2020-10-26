using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetworkUtil
{

    public static class Networking
    {
        /////////////////////////////////////////////////////////////////////////////////////////
        // Server-Side Code
        /////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Starts a TcpListener on the specified port and starts an event-loop to accept new clients.
        /// The event-loop is started with BeginAcceptSocket and uses AcceptNewClient as the callback.
        /// AcceptNewClient will continue the event-loop.
        /// </summary>
        /// <param name="toCall">The method to call when a new connection is made</param>
        /// <param name="port">The the port to listen on</param>
        public static TcpListener StartServer(Action<SocketState> toCall, int port)
        {

            try
            {
                //Starts TcpListner on the specified port
                TcpListener listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                //Create tuple to pass information
                Tuple<TcpListener, Action<SocketState>> serverInfo = new Tuple<TcpListener, Action<SocketState>>(listener, toCall);

                //Starts an event-loop to accept new clients
                listener.BeginAcceptSocket(AcceptNewClient, serverInfo);

                //Return the TcpListener
                return listener;
            }
            catch (Exception)
            {

                //DO SOMETHING WITH ERRORS HERE
                Console.WriteLine("StartServer caught some error.");
                return new TcpListener(IPAddress.Any, port);
            }

        }

        /// <summary>
        /// To be used as the callback for accepting a new client that was initiated by StartServer, and 
        /// continues an event-loop to accept additional clients.
        ///
        /// Uses EndAcceptSocket to finalize the connection and create a new SocketState. The SocketState's
        /// OnNetworkAction should be set to the delegate that was passed to StartServer.
        /// Then invokes the OnNetworkAction delegate with the new SocketState so the user can take action. 
        /// 
        /// If anything goes wrong during the connection process (such as the server being stopped externally), 
        /// the OnNetworkAction delegate should be invoked with a new SocketState with its ErrorOccured flag set to true 
        /// and an appropriate message placed in its ErrorMessage field. The event-loop should not continue if
        /// an error occurs.
        ///
        /// If an error does not occur, after invoking OnNetworkAction with the new SocketState, an event-loop to accept 
        /// new clients should be continued by calling BeginAcceptSocket again with this method as the callback.
        /// </summary>
        /// <param name="ar">The object asynchronously passed via BeginAcceptSocket. It must contain a tuple with 
        /// 1) a delegate so the user can take action (a SocketState Action), and 2) the TcpListener</param>
        private static void AcceptNewClient(IAsyncResult ar)
        {

            //Have listener to repeat event loop
            Tuple<TcpListener, Action<SocketState>> serverInfo = (Tuple<TcpListener, Action<SocketState>>)ar.AsyncState;
            try
            {
                //Stabilize accept using tcplistener in the tuple
                Socket newClient = serverInfo.Item1.EndAcceptSocket(ar);

                //Create new SocketState with info from tuple (Action delegate) and new socket
                SocketState state = new SocketState(serverInfo.Item2, newClient);

                //Invoke OnNetworkAction
                state.OnNetworkAction(state);

                //Trigger event-loop with TcpListener
                serverInfo.Item1.BeginAcceptSocket(AcceptNewClient, serverInfo);
            }
            catch (Exception)
            {
                //Create new socket
                Socket newClient = serverInfo.Item1.EndAcceptSocket(ar);
                //Create new SocketState
                SocketState errorState = new SocketState(serverInfo.Item2, newClient);
                //Set error flag to true
                errorState.ErrorOccured = true;
                // Add error message and display to console
                errorState.ErrorMessage = "Connection interrupted";
                //Invoke OnNetworkAction with error flag true
                errorState.OnNetworkAction(errorState);
            }
        }

        /// <summary>
        /// Stops the given TcpListener.
        /// </summary>
        public static void StopServer(TcpListener listener)
        {
            try
            {
                listener.Stop();
            }
            catch (Exception e)
            {
                string s = e.Message;
                Console.WriteLine(s);
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////
        // Client-Side Code
        /////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Begins the asynchronous process of connecting to a server via BeginConnect, 
        /// and using ConnectedCallback as the method to finalize the connection once it's made.
        /// 
        /// If anything goes wrong during the connection process, toCall should be invoked 
        /// with a new SocketState with its ErrorOccured flag set to true and an appropriate message 
        /// placed in its ErrorMessage field. Between this method and ConnectedCallback, toCall should 
        /// only be invoked once on error.
        ///
        /// This connection process should timeout and produce an error (as discussed above) 
        /// if a connection can't be established within 3 seconds of starting BeginConnect.
        /// 
        /// </summary>
        /// <param name="toCall">The action to take once the connection is open or an error occurs</param>
        /// <param name="hostName">The server to connect to</param>
        /// <param name="port">The port on which the server is listening</param>
        public static void ConnectToServer(Action<SocketState> toCall, string hostName, int port)
        {
            // TODO: This method is incomplete, but contains a starting point 
            //       for decoding a host address

            // Establish the remote endpoint for the socket.
            IPHostEntry ipHostInfo;
            IPAddress ipAddress = IPAddress.None;

            // Determine if the server address is a URL or an IP
            try
            {
                ipHostInfo = Dns.GetHostEntry(hostName);
                bool foundIPV4 = false;
                foreach (IPAddress addr in ipHostInfo.AddressList)
                    if (addr.AddressFamily != AddressFamily.InterNetworkV6)
                    {
                        foundIPV4 = true;
                        ipAddress = addr;
                        break;
                    }
                // Didn't find any IPV4 addresses
                if (!foundIPV4)
                {
                    // TODO: Indicate an error to the user, as specified in the documentation
                    SocketState errorState = new SocketState(toCall, new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp));
                    errorState.ErrorOccured = true;
                    errorState.ErrorMessage = "IPV4 addresses were not found";
                    errorState.OnNetworkAction(errorState);
                }
            }
            catch (Exception)
            {
                // see if host name is a valid ipaddress
                try
                {
                    ipAddress = IPAddress.Parse(hostName);
                }
                catch (Exception)
                {
                    SocketState invalidIpAddress = new SocketState(toCall, new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp));
                    invalidIpAddress.ErrorOccured = true;
                    invalidIpAddress.ErrorMessage = "The IP address entered is not valid";
                    invalidIpAddress.OnNetworkAction(invalidIpAddress);
                }
            }

            // Create a TCP/IP socket.
            Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Tuple<Socket, Action<SocketState>> info = new Tuple<Socket, Action<SocketState>>(socket, toCall);

            // This disables Nagle's algorithm (google if curious!)
            // Nagle's algorithm can cause problems for a latency-sensitive 
            // game like ours will be 
            socket.NoDelay = true;

            // Connect using a timeout (3 seconds)
            //Saves the result of the begin connect without invoking callback to verify timeout does not happen
            IAsyncResult result = socket.BeginConnect(ipAddress, port, null, info);
            // boolean flag to signal timeout, end connection if is not made after 3 seconds
            bool success = result.AsyncWaitHandle.WaitOne(3000, true);
            // Check if timeout condition has been met
            if (success)
            {
                //Call ConnectedCallBack with the info that was saved
                ConnectedCallback(result);
            }
            // Connection has timed out and we want to close the socket and produce an error
            else
            {
                socket.Close();
                SocketState timeoutState = new SocketState(toCall, new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp));
                timeoutState.ErrorOccured = true;
                timeoutState.ErrorMessage = "Connection timed out";
                timeoutState.OnNetworkAction(timeoutState);
            }
            // TODO: Finish the remainder of the connection process as specified.

        }

        /// <summary>
        /// To be used as the callback for finalizing a connection process that was initiated by ConnectToServer.
        ///
        /// Uses EndConnect to finalize the connection.
        /// 
        /// As stated in the ConnectToServer documentation, if an error occurs during the connection process,
        /// either this method or ConnectToServer (not both) should indicate the error appropriately.
        /// 
        /// If a connection is successfully established, invokes the toCall Action that was provided to ConnectToServer (above)
        /// with a new SocketState representing the new connection.
        /// 
        /// </summary>
        /// <param name="ar">The object asynchronously passed via BeginConnect</param>
        private static void ConnectedCallback(IAsyncResult ar)
        {
            //Decodes the argument into the tuple we passed in
            Tuple<Socket, Action<SocketState>> info = (Tuple<Socket, Action<SocketState>>)ar.AsyncState;
            //Tuple item 1 is the socket created in ConnectToServer
            Socket socket = info.Item1;
            //Create a new SocketState representing the connection
            SocketState newState = new SocketState(info.Item2, socket);
            try
            {
                //EndConnect to finalize connection
                socket.EndConnect(ar);
                //Invoke the toCall Action
                info.Item2(newState);
            }
            catch
            {
                newState.ErrorOccured = true;
                newState.ErrorMessage = "Connection timed out";
                newState.OnNetworkAction(newState);
            }

        }


        /////////////////////////////////////////////////////////////////////////////////////////
        // Server and Client Common Code
        /////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Begins the asynchronous process of receiving data via BeginReceive, using ReceiveCallback 
        /// as the callback to finalize the receive and store data once it has arrived.
        /// The object passed to ReceiveCallback via the AsyncResult should be the SocketState.
        /// 
        /// If anything goes wrong during the receive process, the SocketState's ErrorOccured flag should 
        /// be set to true, and an appropriate message placed in ErrorMessage, then the SocketState's
        /// OnNetworkAction should be invoked. Between this method and ReceiveCallback, OnNetworkAction should only be 
        /// invoked once on error.
        /// 
        /// </summary>
        /// <param name="state">The SocketState to begin receiving</param>
        public static void GetData(SocketState state)
        {
            try
            {
                // Use beginReceive to finalize the the receive and store the data
                state.TheSocket.BeginReceive(state.buffer, 0, state.buffer.Length, SocketFlags.None, ReceiveCallback, state);
            } 
            catch
            {
                // If anything goes wrong set socket state's errorOccured to true
                // and display appropriate message
                state.ErrorOccured = true;
                state.ErrorMessage = "Message cannot be received";
                state.OnNetworkAction(state);
            }
        }

        /// <summary>
        /// To be used as the callback for finalizing a receive operation that was initiated by GetData.
        /// 
        /// Uses EndReceive to finalize the receive.
        ///
        /// As stated in the GetData documentation, if an error occurs during the receive process,
        /// either this method or GetData (not both) should indicate the error appropriately.
        /// 
        /// If data is successfully received:
        ///  (1) Read the characters as UTF8 and put them in the SocketState's unprocessed data buffer (its string builder).
        ///      This must be done in a thread-safe manner with respect to the SocketState methods that access or modify its 
        ///      string builder.
        ///  (2) Call the saved delegate (OnNetworkAction) allowing the user to deal with this data.
        /// </summary>
        /// <param name="ar"> 
        /// This contains the SocketState that is stored with the callback when the initial BeginReceive is called.
        /// </param>
        private static void ReceiveCallback(IAsyncResult ar)
        {
            SocketState state = (SocketState)ar.AsyncState;

            try
            {
                int numBytes = state.TheSocket.EndReceive(ar);
                String text = Encoding.UTF8.GetString(state.buffer, 0, numBytes);
                state.data.Append(text);

            }
            catch
            {
                // If anything goes wrong set socket state's errorOccured to true
                // and display appropriate message
                state.ErrorOccured = true;
                state.ErrorMessage = "Message cannot be received";
                state.OnNetworkAction(state);
            }
            state.OnNetworkAction(state);
        }

        /// <summary>
        /// Begin the asynchronous process of sending data via BeginSend, using SendCallback to finalize the send process.
        /// 
        /// If the socket is closed, does not attempt to send.
        /// 
        /// If a send fails for any reason, this method ensures that the Socket is closed before returning.
        /// </summary>
        /// <param name="socket">The socket on which to send the data</param>
        /// <param name="data">The string to send</param>
        /// <returns>True if the send process was started, false if an error occurs or the socket is already closed</returns>
        public static bool Send(Socket socket, string data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// To be used as the callback for finalizing a send operation that was initiated by Send.
        ///
        /// Uses EndSend to finalize the send.
        /// 
        /// This method must not throw, even if an error occured during the Send operation.
        /// </summary>
        /// <param name="ar">
        /// This is the Socket (not SocketState) that is stored with the callback when
        /// the initial BeginSend is called.
        /// </param>
        private static void SendCallback(IAsyncResult ar)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Begin the asynchronous process of sending data via BeginSend, using SendAndCloseCallback to finalize the send process.
        /// This variant closes the socket in the callback once complete. This is useful for HTTP servers.
        /// 
        /// If the socket is closed, does not attempt to send.
        /// 
        /// If a send fails for any reason, this method ensures that the Socket is closed before returning.
        /// </summary>
        /// <param name="socket">The socket on which to send the data</param>
        /// <param name="data">The string to send</param>
        /// <returns>True if the send process was started, false if an error occurs or the socket is already closed</returns>
        public static bool SendAndClose(Socket socket, string data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// To be used as the callback for finalizing a send operation that was initiated by SendAndClose.
        ///
        /// Uses EndSend to finalize the send, then closes the socket.
        /// 
        /// This method must not throw, even if an error occured during the Send operation.
        /// 
        /// This method ensures that the socket is closed before returning.
        /// </summary>
        /// <param name="ar">
        /// This is the Socket (not SocketState) that is stored with the callback when
        /// the initial BeginSend is called.
        /// </param>
        private static void SendAndCloseCallback(IAsyncResult ar)
        {
            throw new NotImplementedException();
        }

    }
}
