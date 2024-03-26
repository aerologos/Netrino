using System;
using System.IO.Ports;
using System.Threading;
using Netrino.EventHandlers;
using NLog;
using Parallops;

namespace Netrino.Channels.Serial
{
    /// <summary>
    /// Provides the functionality for the network channel based on serial port.
    /// </summary>
    public class SerialPortChannel : OperationBase, ISerialPortChannel
    {
        private readonly object _syncLock = new object();
        
        /// <inheritdoc/>
        public event CommunicationStartEventHandler CommunicationStarted;

        /// <inheritdoc/>
        public event CommunicationStopEventHandler CommunicationStopped;

        /// <inheritdoc/>
        public SerialPort Port { get; }

        /// <inheritdoc/>
        public int BaudRate
        {
            get => Port.BaudRate;
            set => Port.BaudRate = value;
        }
        
        /// <summary>
        /// Instantiates the new <see cref="SerialPortChannel"/> object.
        /// </summary>
        /// <param name="portName">The port name.</param>
        /// <param name="baudRate">The baud rate.</param>
        public SerialPortChannel(
            string portName,
            int baudRate)
            : this(portName, baudRate, LogManager.CreateNullLogger())
        {
        }
        
        /// <summary>
        /// Instantiates the new <see cref="SerialPortChannel"/> object.
        /// </summary>
        /// <param name="portName">The port name.</param>
        /// <param name="baudRate">The baud rate.</param>
        /// <param name="logger">The logger.</param>
        public SerialPortChannel(
            string portName,
            int baudRate,
            ILogger logger)
            : base(logger)
        {
            Port = new SerialPort(portName, baudRate);
        }

        /// <inheritdoc/>
        protected override void PerformStart()
        {
            Port.WriteTimeout = 500;
            Port.Open();
            
            Reset();

            CommunicationStarted?.Invoke();
        }

        /// <inheritdoc/>
        public bool Push(byte[] data)
        {
            if (!Port.IsOpen) return false;
            
            try
            {
                lock (_syncLock)
                {
                    Port.BaseStream.Write(data, 0, data.Length);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }

        private readonly byte[] _readBuffer = new byte[1024];
        /// <inheritdoc/>
        public bool Pull(out byte[] data)
        {
            if (Port.IsOpen && Port.BytesToRead > 0)
            {
                // think twice before editing provided method of read.
                // Though it is not ideal, it cost some time to get to it.
                var read = Port.BaseStream
                    .ReadAsync(_readBuffer, 0, _readBuffer.Length)
                    .GetAwaiter()
                    .GetResult();
                if (read <= 0)
                {
                    data = null;
                    return false;
                }
                
                data = new byte[read];
                Buffer.BlockCopy(_readBuffer, 0, data, 0, read);
                return true;
            }

            data = null;
            return false;
        }

        /// <inheritdoc/>
        public void Reset()
        {
            Port.DtrEnable = false;
            Port.RtsEnable = false;

            Thread.Sleep(50);

            Port.DtrEnable = true;
            Port.RtsEnable = true;

            Thread.Sleep(50);
        }

        /// <inheritdoc/>
        protected override void PerformStop()
        {
            Port.Close();
            
            CommunicationStopped?.Invoke();
        }

        /// <inheritdoc/>
        protected override void PerformDisposal()
        {
            Port.Dispose();
        }
    }
}