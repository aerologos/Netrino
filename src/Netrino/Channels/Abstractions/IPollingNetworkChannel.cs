using Netrino.EventHandlers;

namespace Netrino.Channels.Abstractions
{
    /// <summary>
    /// Describes the contract for polling network channel.
    /// Polling means that it repeatedly checks for the incoming packets.
    /// </summary>
    public interface IPollingNetworkChannel : INetworkChannel
    {
        /// <summary>
        /// The event that the client can subscribe to to receive packets from the channel.
        /// </summary>
        event DataReceiveEventHandler DataReceived;
    }
}