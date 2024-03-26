using System;
using System.Net.Sockets;
using Extype;
using Netrino.Channels.Abstractions;
using Netrino.EventHandlers;
using Parallops;

namespace Netrino.Channels.Tcp
{
    /// <summary>
    /// Provides the functionality for the network channel based on TCP.
    /// </summary>
    public class TcpChannel : OperationBase, INetworkChannel
    {
        private readonly string _ip;
        private readonly ushort _port;

        private TcpClient _tcpClient;

        /// <inheritdoc/>
        public event CommunicationStartEventHandler CommunicationStarted;

        /// <inheritdoc/>
        public event CommunicationStopEventHandler CommunicationStopped;

        /// <summary>
        /// Instantiates the new <see cref="TcpChannel"/> object.
        /// </summary>
        /// <param name="ip">The IP address.</param>
        /// <param name="port">The port number.</param>
        public TcpChannel(string ip, ushort port)
        {
            _ip = ip.ThrowIfNull(nameof(ip));
            _port = port;
        }

        /// <inheritdoc/>
        protected override void PerformStart()
        {
            _tcpClient = new TcpClient(_ip, _port);
            if (_tcpClient.Connected)
            {
                CommunicationStarted?.Invoke();
            }
        }

        /// <inheritdoc/>
        public bool Push(byte[] data)
        {
            if (data == null) return false;
            if (!_tcpClient?.Connected ?? false) return false;

            try
            {
                _tcpClient.GetStream().Write(data, 0, data.Length);
                return true;
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
            data = null;
            
            if (_tcpClient?.Connected ?? false) return false;
            if (_tcpClient.Available == 0) return false;

            try
            {
                var buffer = new byte[1024];
                var read = _tcpClient.GetStream().Read(buffer, 0, buffer.Length);
                if (read <= 0) return false;

                data = new byte[read];
                Array.Copy(buffer, 0, data, 0, read);

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return false;
        }

        /// <inheritdoc/>
        public void Reset()
        {
            PerformStop();
            PerformStart();
        }

        /// <inheritdoc/>
        protected override void PerformStop()
        {
            _tcpClient.Client.Disconnect(true);
            _tcpClient.Dispose();
            
            CommunicationStopped?.Invoke();
        }

        /// <inheritdoc/>
        protected override void PerformDisposal()
        {
            _tcpClient?.Dispose();
        }
    }
}