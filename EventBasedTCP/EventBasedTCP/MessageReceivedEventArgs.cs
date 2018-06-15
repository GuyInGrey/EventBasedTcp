using System;

namespace EventBasedTCP
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public string Message { get; set; }
        public DateTime TimeReceived { get; set; }

        public MessageReceivedEventArgs() : this("", DateTime.Now)
        {

        }

        public MessageReceivedEventArgs(string message) : this(message, DateTime.Now)
        {

        }

        public MessageReceivedEventArgs(string message, DateTime timeSent)
        {
            Message = message;
            TimeReceived = timeSent;
        }
    }
}
