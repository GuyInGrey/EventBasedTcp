using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace EventBasedTCP
{
    public class Client
    {
        TcpClient _client;
        NetworkStream _stream => _client.GetStream();
        Thread _listenThread;

        public bool IsDisposed { get; private set; }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        public event EventHandler ServerDisconnected;
        public event EventHandler Disposed;
        public event EventHandler StartedListening;

        public bool IsServerClient { get; }
        public bool FailedConnect { get; }

        public object Tag { get; set; }

        public string ConnectAddress => _client.Client.RemoteEndPoint.ToString();

        private Client(TcpClient client)
        {
            _client = client;
            IsServerClient = true;

            StartListening();
        }

        public Client(string address, int port)
        {
            try
            {
                _client = new TcpClient(address, port);

                StartListening();
            }
            catch
            {
                FailedConnect = true;
            }
        }

        private void StartListening()
        {
            _listenThread = new Thread(ListenForMessages);
            _listenThread.Start();
            StartedListening?.Invoke(this, null);
        }

        public void SendMessage(object content)
        {
            var outContent = content.ToString();
            outContent += TcpOptions.EndMessageCode.ToString();
            var data = outContent.GetBytes();
            _stream.Write(data, 0, data.Length);
        }

        private void ListenForMessages()
        {
            var bytes = new List<byte>();

            while (!IsDisposed)
            {
                var i = -1;

                try
                {
                    i = _stream.ReadByte();
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode == SocketError.Interrupted)
                    {
                        break;
                    }
                }

                if (i == -1)
                {
                    break;
                }
                else if (i == TcpOptions.EndMessageCode)
                {
                    if (bytes.Count > 0)
                    {
                        var message = bytes.ToArray().GetString();
                        MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));
                        bytes.Clear();
                    }
                }
                else if (i == TcpOptions.EndConnectionCode && !IsServerClient)
                {
                    ServerDisconnected?.Invoke(this, null);
                    Dispose(true);
                    break;
                }
                else
                {
                    bytes.Add(Convert.ToByte(i));
                }
            }
        }

        public void Dispose(bool fromServer)
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                if (!fromServer)
                {
                    SendMessage(TcpOptions.EndConnectionCode.ToString());
                }
                _client.Close();
                _client.Dispose();
                _listenThread.Abort();
                Disposed?.Invoke(this, null);
            }
        }

        public static Client FromServerClient(TcpClient serverClient) =>
            new Client(serverClient);
    }
}
