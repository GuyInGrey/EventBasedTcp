using System;

namespace EventBasedTCP
{
    public class ClientToggleEventArgs : EventArgs, ITimed
    {
        /// <summary>
        /// The time the event happened.
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// The Client that connected.
        /// </summary>
        public Client ConnectedClient { get; set; }
    }
}