using System;

namespace EventBasedTCP
{
    public class MessageReceivedEventArgs : EventArgs, ITimed
    {
        /// <summary>
        /// The time the event happened.
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// The message that was sent.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The Client that sent the message.
        /// </summary>
        public Client Client { get; set; }
    }
}