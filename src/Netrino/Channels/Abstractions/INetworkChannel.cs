using Netrino.EventHandlers;
using Parallops;

namespace Netrino.Channels.Abstractions
{
    /// <summary>
    /// Describes the contract for pushing/pulling data through the network.
    /// </summary>
    public interface INetworkChannel : IOperation
    {
        /// <summary>
        /// The event that the client can subscribe to to receive notifications about the started communication.
        /// </summary>
        event CommunicationStartEventHandler CommunicationStarted;
        
        /// <summary>
        /// The event that the client can subscribe to to receive notifications about the stopped communication.
        /// </summary>
        event CommunicationStopEventHandler CommunicationStopped;

        /// <summary>
        /// Pushes the data to the channel.
        /// </summary>
        /// <param name="data">The data been pushed.</param>
        /// <returns>true if data has been pushed, false otherwise</returns>
        bool Push(byte[] data);

        /// <summary>
        /// Tries to pull the data from the network channel.
        /// </summary>
        /// <param name="data">The packet data.</param>
        /// <returns>true if data has been received, false otherwise</returns>
        bool Pull(out byte[] data);

        /// <summary>
        /// Resets the network channel.
        /// </summary>
        void Reset();
    }
}