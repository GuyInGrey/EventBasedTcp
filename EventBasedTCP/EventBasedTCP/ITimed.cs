using System;

namespace EventBasedTCP
{
    public interface ITimed
    {
        /// <summary>
        /// The time the event happened.
        /// </summary>
        DateTime Time { get; set; }
    }
}