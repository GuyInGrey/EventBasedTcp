using System;

namespace EventBasedTCP
{
    public class ClientToggleEventArgs : EventArgs
    {
        public Client ConnectedClient { get; set; }
        public DateTime TimeConnected { get; set; }

        public ClientToggleEventArgs(Client client) : this(client, DateTime.Now)
        {

        }

        public ClientToggleEventArgs(Client client, DateTime timeConnected)
        {
            ConnectedClient = client;
            TimeConnected = timeConnected;
        }
    }
}
