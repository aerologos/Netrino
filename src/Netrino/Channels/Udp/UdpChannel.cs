using System;
using System.Net;
using System.Net.Sockets;
using NLog;
using Extype;
using Netrino.Channels.Abstractions;
using Netrino.EventHandlers;
using Parallops;

namespace Netrino.Channels.Udp
{
    /// <summary>
    /// Provides the functionality for the network channel based on UDP.
    /// </summary>
    public class UdpChannel : OperationBase, INetworkChannel
    {
        private readonly string _ip;
        private readonly ushort _pushPort;
        private readonly ushort _pullPort;
        private readonly UdpClient _pushUdpClient;
        private readonly UdpClient _pullUdpClient;

        /// <inheritdoc/>
        public event CommunicationStartEventHandler CommunicationStarted;

        /// <inheritdoc/>
        public event CommunicationStopEventHandler CommunicationStopped;

        /// <summary>
        /// Instantiates the <see cref="UdpChannel"/> object.
        /// </summary>
        /// <param name="ip">The name of the remote DNS host to which you intend to connect.</param>
        /// <param name="pushPort">The remote port number to which the data is pushed.</param>
        /// <param name="pullPort">The remote port number from which the data is pulled.</param>
        /// <param name="logger">The instance of logger.</param>
        public UdpChannel(string ip, ushort pushPort, ushort pullPort, ILogger logger)
            : base(logger)
        {
            _ip = ip.ThrowIfNull(nameof(ip));
            _pushPort = pushPort;
            _pullPort = pullPort;

            _pushUdpClient = new UdpClient();
            _pullUdpClient = new UdpClient(pullPort);
        }

        /// <summary>
        /// Starts the UDP channel using provided ip and port.
        /// </summary>
        protected override void PerformStart()
        {
            _pushUdpClient.Connect(_ip, _pushPort);
            CommunicationStarted?.Invoke();
        }

        /// <summary>
        /// Stops the UDP channel if established.
        /// </summary>
        protected override void PerformStop()
        {
            _pushUdpClient.Client.Disconnect(true);
            SendDummyPacketToReleaseUdpListener();
            CommunicationStopped?.Invoke();
        }

        private void SendDummyPacketToReleaseUdpListener()
        {
            using (var udpSender = new UdpClient())
            {
                var dummyPacket = new byte[] { 0xFF };
                udpSender.Connect("localhost", _pullPort);
                udpSender.Send(dummyPacket, dummyPacket.Length);
                udpSender.Dispose();
            }
        }

        /// <inheritdoc/>
        public bool Push(byte[] data)
        {
            if (data == null) return false;

            try
            {
                return _pushUdpClient.Send(data, data.Length) > 0;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return false;
        }

        /// <inheritdoc/>
        public bool Pull(out byte[] data)
        {
            try
            {
                if (_pullUdpClient.Available > 0)
                {
                    IPEndPoint ipEndPoint = null;
                    data = _pullUdpClient.Receive(ref ipEndPoint);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            data = null;
            return false;
        }

        /// <summary>
        /// Does nothing but implements the interface.
        /// </summary>
        public void Reset()
        {
        }

        /// <inheritdoc/>
        protected override void PerformDisposal()
        {
            _pushUdpClient.Dispose();
            _pullUdpClient.Dispose();
        }
    }
}