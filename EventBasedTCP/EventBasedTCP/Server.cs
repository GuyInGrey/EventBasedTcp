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

        public List<Client> ConnectedClients { get; set; } = new List<Client>();

        public string ListenAddress { get; }
        public int Port { get; }

        public bool IsDisposed { get; private set; }

        public event EventHandler<ClientToggleEventArgs> ClientConnected;
        public event EventHandler<ClientToggleEventArgs> ClientDisconnected;
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        public event EventHandler Disposed;
        public event EventHandler StartedListening;

        public object Tag { get; set; }

        public Server(string address, int port)
        {
            _listener = new TcpListener(IPAddress.Parse(address), port);
            _listener.Start();

            ListenAddress = address;
            Port = port;

            StartClientListening();
        }

        private void StartClientListening()
        {
            _clientListenerThread = new Thread(ListenForClients);
            _clientListenerThread.Start();
            StartedListening?.Invoke(this, null);
        }

        private void ListenForClients()
        {
            while (!IsDisposed)
            {
                try
                {
                    var connectedTCPClient = _listener.AcceptTcpClient();
                    var connectedClient = Client.FromServerClient(connectedTCPClient);

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
                MessageReceived?.Invoke(sender, e);
            }
        }

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