using System.IO.Ports;
using Netrino.Channels.Abstractions;

namespace Netrino.Channels.Serial
{
    /// <summary>
    /// Describes the contract for pushing/pulling data through the serial port.
    /// </summary>
    public interface ISerialPortChannel : INetworkChannel
    {
        /// <summary>
        /// Gets the port to the serial port channel.
        /// </summary>
        SerialPort Port { get; }
        
        /// <summary>
        /// Gets or sets the serial port baud rate.
        /// </summary>
        int BaudRate { get; set; }
    }
}