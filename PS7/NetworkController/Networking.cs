/* Authors : Kevin Pineda & Cristian Tapiero , November 2021
   networking library (part of final project) for CS 3500
   University of Utah
*/


using System;
using System.Collections.Generic;
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
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            
            //Object state to be passed to callback
            Object stateObj = Tuple.Create(toCall, listener);
            listener.BeginAcceptSocket(AcceptNewClient, stateObj);

            return listener;
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
        /// the OnNetworkAction delegate should be invoked with a new SocketState with its ErrorOccurred flag set to true 
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
            // using object stated passed by the function
            Tuple<Action<SocketState>, TcpListener> tuple = ar.AsyncState as Tuple<Action<SocketState>, TcpListener>;
            Action<SocketState> toCall = tuple.Item1;
            TcpListener listener = tuple.Item2;

            try
            {
                Socket newClient = listener.EndAcceptSocket(ar);
                SocketState state = new SocketState(toCall, newClient);

                state.OnNetworkAction = toCall;

                state.OnNetworkAction.Invoke(state);

                // continues an accept loop
                listener.BeginAcceptSocket(AcceptNewClient, new Tuple<Action<SocketState>, TcpListener>(tuple.Item1, tuple.Item2));
            }
            catch (Exception e)
            {
                ErrorHandler(toCall, e.Message);
            }
        }

        /// <summary>
        /// Stops the given TcpListener.
        /// </summary>
        public static void StopServer(TcpListener listener)
        {
            listener.Stop();
        }

        /////////////////////////////////////////////////////////////////////////////////////////
        // Client-Side Code
        /////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Begins the asynchronous process of connecting to a server via BeginConnect, 
        /// and using ConnectedCallback as the method to finalize the connection once it's made.
        /// 
        /// If anything goes wrong during the connection process, toCall should be invoked 
        /// with a new SocketState with its ErrorOccurred flag set to true and an appropriate message 
        /// placed in its ErrorMessage field. Depending on when the error occurs, this should happen either
        /// in this method or in ConnectedCallback.
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
                    ErrorHandler(toCall, "Did not find IPV6 or IPV4 address.");
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
                    ErrorHandler(toCall, hostName + " is not a valid IP address.");
                }
            }

            // Create a TCP/IP socket.
            Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            // This disables Nagle's algorithm (google if curious!)
            // Nagle's algorithm can cause problems for a latency-sensitive 
            // game like ours will be 

            socket.NoDelay = true;

            SocketState state = new SocketState(toCall, socket);
            try
            {
                IAsyncResult result = state.TheSocket.BeginConnect(ipAddress, port, ConnectedCallback, state);

                //Sets success to false if the wait takes longer than the specified 3 seconds
                bool success = result.AsyncWaitHandle.WaitOne(3000, true);

                if (!success)
                {
                    ErrorHandler(state, "Connection Timed Out.");
                    socket.Close();
                    return;
                }
            }
            catch (Exception)
            {
                ErrorHandler(state, "Unable to begin connection.");
            }
        }
    
        /// <summary>
        /// To be used as the callback for finalizing a connection process that was initiated by ConnectToServer.
        ///
        /// Uses EndConnect to finalize the connection.
        /// 
        /// As stated in the ConnectToServer documentation, if an error occurs during the connection process,
        /// either this method or ConnectToServer should indicate the error appropriately.
        /// 
        /// If a connection is successfully established, invokes the toCall Action that was provided to ConnectToServer (above)
        /// with a new SocketState representing the new connection.
        /// 
        /// </summary>
        /// <param name="ar">The object asynchronously passed via BeginConnect</param>
        private static void ConnectedCallback(IAsyncResult ar)
        {
            SocketState state = (SocketState)ar.AsyncState;

            try
            {
                if (!socketConnected(state.TheSocket))
                {
                    ErrorHandler(state, "Server does not exist.");
                    return;
                }
                state.TheSocket.EndConnect(ar);
                state.OnNetworkAction(state);
            }
            catch (Exception e)
            {
                ErrorHandler(state, "Failed to finalize connection.");

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
        /// If anything goes wrong during the receive process, the SocketState's ErrorOccurred flag should 
        /// be set to true, and an appropriate message placed in ErrorMessage, then the SocketState's
        /// OnNetworkAction should be invoked. Depending on when the error occurs, this should happen either
        /// in this method or in ReceiveCallback.
        /// </summary>
        /// <param name="state">The SocketState to begin receiving</param>
        public static void GetData(SocketState state)
        {
            if (!socketConnected(state.TheSocket))
            {
                ErrorHandler(state, "The client is no longer connected to the server.");
                return;
            }

            try
            {
                state.TheSocket.BeginReceive(state.buffer, 0, state.buffer.Length, SocketFlags.None, ReceiveCallback, state);
            }
            catch (Exception)
            {
                ErrorHandler(state, "Failed to receive data.");
            }
        }

        /// <summary>
        /// To be used as the callback for finalizing a receive operation that was initiated by GetData.
        /// 
        /// Uses EndReceive to finalize the receive.
        ///
        /// As stated in the GetData documentation, if an error occurs during the receive process,
        /// either this method or GetData should indicate the error appropriately.
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

                string data = Encoding.UTF8.GetString(state.buffer, 0, numBytes);

                // Buffer the data received (we may not have a full message yet)
                state.data.Append(data);

                state.OnNetworkAction.Invoke(state);
            }
            catch(Exception)
            {
                ErrorHandler(state, "Not data recieved");
            }

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
                if (!socketConnected(socket) || object.ReferenceEquals(socket, null))
                {
                    socket.Close();
                    return false;

                }
                    
            byte[] messageBytes = Encoding.UTF8.GetBytes(data);
            try
            {
                socket.BeginSend(messageBytes, 0, messageBytes.Length, SocketFlags.None, SendCallback, socket);
            }
            catch (Exception)
            {
                socket.Close();
                return false;
            }
            return true;
        }

        /// <summary>
        /// To be used as the callback for finalizing a send operation that was initiated by Send.
        ///
        /// Uses EndSend to finalize the send.
        /// 
        /// This method must not throw, even if an error occurred during the Send operation.
        /// </summary>
        /// <param name="ar">
        /// This is the Socket (not SocketState) that is stored with the callback when
        /// the initial BeginSend is called.
        /// </param>
        private static void SendCallback(IAsyncResult ar)
        {
            Socket sender = (Socket)ar.AsyncState;
            try
            {
                sender.EndSend(ar);
            }
            catch (Exception)
            {
                return;
            }
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
            if (!socketConnected(socket) || object.ReferenceEquals(socket, null))
                return false;

            byte[] messageBytes = Encoding.UTF8.GetBytes(data);
            try
            {
                socket.BeginSend(messageBytes, 0, messageBytes.Length, SocketFlags.None, SendAndCloseCallback, socket);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// To be used as the callback for finalizing a send operation that was initiated by SendAndClose.
        ///
        /// Uses EndSend to finalize the send, then closes the socket.
        /// 
        /// This method must not throw, even if an error occurred during the Send operation.
        /// 
        /// This method ensures that the socket is closed before returning.
        /// </summary>
        /// <param name="ar">
        /// This is the Socket (not SocketState) that is stored with the callback when
        /// the initial BeginSend is called.
        /// </param>
        private static void SendAndCloseCallback(IAsyncResult ar)
        {
            Socket sender = (Socket)ar.AsyncState;
            try
            {
                sender.EndSend(ar);
            }
            finally
            {
                sender.Close();
            }
        }


        /// <summary>
        /// Takes a Socket and checks if it is connected to the server.
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        private static bool socketConnected(Socket socket)
        {
            return !(socket.Poll(1000, SelectMode.SelectRead) && socket.Available == 0);
        }

        /// <summary>
        /// Creates a new SocketState with a null socket and sets the error flags with an appropriate message. 
        /// This overload is called when a SocketState is not previously specified within the method that the error occurs.
        /// </summary>
        /// <param name="toCall"></param>
        /// <param name="message"></param>
        private static void ErrorHandler(Action<SocketState> toCall, string message)
        {
            SocketState newState = new SocketState(toCall, null);
            newState.ErrorOccurred = true;
            newState.ErrorMessage = message;
            newState.OnNetworkAction(newState);
        }

        /// <summary>
        /// Takes passed SocketState and sets error flag to true and invokes the OnNetworkAction delegate with a appropriate error message.
        ///
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        private static void ErrorHandler(SocketState state, string message)
        {
            state.ErrorOccurred = true;
            state.ErrorMessage = message;
            state.OnNetworkAction(state);
        }

    }
}