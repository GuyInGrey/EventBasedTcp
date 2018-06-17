using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace EventBasedTCP
{
    public class Server
    {
        TcpListener _listener;
        Thread _clientListenerThread;

        /// <summary>
        /// A list of all the clients connected to this server.
        /// </summary>
        public List<Client> ConnectedClients { get; set; }

        /// <summary>
        /// A list of all the responses set up on this server.
        /// </summary>
        public List<ResponseEvent> Responses { get; set; }

        /// <summary>
        /// The address the server is listening on.
        /// </summary>
        public string Address { get; }
        
        /// <summary>
        /// The port the server is listening on.
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// Whether the server has been stopped and disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Occurs when a client connects to the server.
        /// </summary>
        public event EventHandler<ClientToggleEventArgs> ClientConnected;

        /// <summary>
        /// Occurs when a client disconnected from the server.
        /// </summary>
        public event EventHandler<ClientToggleEventArgs> ClientDisconnected;

        /// <summary>
        /// Occurs when any client sends a message to the server.
        /// </summary>
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// Occurs when the server is disconnected and disposed.
        /// </summary>
        public event EventHandler Disposed;

        /// <summary>
        /// Whether the client is listening or not.
        /// </summary>
        public bool HasStartedListening { get; private set; }

        /// <summary>
        /// If it's listening and has not been disposed.
        /// </summary>
        public bool IsReady => HasStartedListening && !IsDisposed;

        /// <summary>
        /// The tag attached to this object.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Constructor to instantiate and start the server for listening.
        /// </summary>
        /// <param name="address">The IP address the listen on.</param>
        /// <param name="port">The port to listen on.</param>
        public Server(string address, int port)
        {
            _listener = new TcpListener(IPAddress.Parse(address), port);
            _listener.Start();

            Address = address;
            Port = port;

            StartClientListening();
        }

        /// <summary>
        /// Starts the listening thread for the server. After this, the server has begun listening for connected from clients. 
        /// Private so that users do not call more than once.
        /// </summary>
        private void StartClientListening()
        {
            ConnectedClients = new List<Client>();
            Responses = new List<ResponseEvent>();
            _clientListenerThread = new Thread(ListenForClients);
            _clientListenerThread.Start();
            HasStartedListening = true;
        }

        /// <summary>
        /// The threaded method where the server listens for client connections. Only called from <see cref="StartClientListening"/>
        /// </summary>
        private void ListenForClients()
        {
            while (!IsDisposed)
            {
                try
                {
                    var connectedTCPClient = _listener.AcceptTcpClient();
                    var connectedClient = new Client(connectedTCPClient);

                    connectedClient.MessageReceived += ConnectedClient_MessageReceived;

                    ConnectedClients.Add(connectedClient);
                    var eventargs = new ClientToggleEventArgs
                    {
                        ConnectedClient = connectedClient,
                        Time = DateTime.Now
                    };
                    ClientConnected?.Invoke(this, eventargs);
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode == SocketError.Interrupted)
                    {
                        break;
                    }
                    else
                    {
                        throw e;
                    }
                }
            }
        }

        /// <summary>
        /// This is the event handler attached to every client that is connected's MessageReceive event.
        /// This is where it checks if a client has sent the disconnetion code, and if so, disposes of them.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectedClient_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.Message == TcpOptions.EndConnectionCode.ToString())
            {
                ConnectedClients.Remove(sender as Client);
                var eventargs = new ClientToggleEventArgs
                {
                    ConnectedClient = sender as Client,
                    Time = DateTime.Now
                };
                ClientDisconnected?.Invoke(this, eventargs);
            }
            else
            {
                foreach (var response in Responses)
                {
                    var willTrigger = false;

                    switch (response.Mode)
                    {
                        case ContentMode.Contains:
                            if (e.Message.Contains(response.Content))
                            {
                                willTrigger = true;
                            }
                            break;
                        case ContentMode.EndsWish:
                            if (e.Message.EndsWith(response.Content))
                            {
                                willTrigger = true;
                            }
                            break;
                        case ContentMode.StartsWith:
                            if (e.Message.StartsWith(response.Content))
                            {
                                willTrigger = true;
                            }
                            break;
                        case ContentMode.Equals:
                            if (e.Message == response.Content)
                            {
                                willTrigger = true;
                            }
                            break;
                    }

                    if (willTrigger)
                    {
                        response.Event?.Invoke(e);
                    }
                    else
                    {
                        MessageReceived?.Invoke(sender, e);
                    }
                }
            }
        }

        /// <summary>
        /// This disposes the server, also stopping the listening thread, and sending an
        /// <see cref="TcpOptions.EndConnectionCode"/> to every client connected.
        /// </summary>
        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                foreach (var client in ConnectedClients)
                {
                    client.SendMessage(TcpOptions.EndConnectionCode);
                    client.Dispose(false);
                }
                ConnectedClients = null;
                _listener.Stop();
                Disposed?.Invoke(this, null);
            }
        }

        /// <summary>
        /// Returns this machine's intranetwork IPv4 address. 
        /// Throws an exception if there are no connected network adapters on the system.
        /// </summary>
        /// <returns>The IPv4 address of this machine.</returns>
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}