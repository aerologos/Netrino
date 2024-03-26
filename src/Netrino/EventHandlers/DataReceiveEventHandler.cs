using Netrino.Channels.Abstractions;

namespace Netrino.EventHandlers
{
    /// <summary>
    /// The delegate used to signal the subscriber about data receive.
    /// </summary>
    /// <param name="networkChannel">The network channel initiating the event.</param>
    /// <param name="data">The received data.</param>
    public delegate void DataReceiveEventHandler(INetworkChannel networkChannel, byte[] data);
}