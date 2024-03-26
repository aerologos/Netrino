using System.IO;
using System.Text;
using Netrino.Channels.Abstractions;
using Netrino.EventHandlers;
using Parallops;

namespace Netrino.Channels.File
{
    /// <summary>
    /// Provides the functionality for the network channel based on file.
    /// </summary>
    public class FileChannel : OperationBase, INetworkChannel
    {
        private readonly StreamWriter _streamWriter;
        private readonly StreamReader _streamReader;
        private readonly bool _isReadOnly;

        /// <inheritdoc/>
        public event CommunicationStartEventHandler CommunicationStarted;

        /// <inheritdoc/>
        public event CommunicationStopEventHandler CommunicationStopped;

        /// <summary>
        /// Instantiates the new <see cref="FileChannel"/> object.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="isReadOnly">The flag indicating whether to make the channel readonly or not.</param>
        public FileChannel(string filePath, bool isReadOnly)
        {
            var stream = new FileStream(filePath, FileMode.OpenOrCreate);
            _streamWriter = new StreamWriter(stream);
            _streamReader = new StreamReader(stream);
            _isReadOnly = isReadOnly;
        }

        /// <inheritdoc/>
        protected override void PerformStart()
        {
            CommunicationStarted?.Invoke();
        }

        /// <inheritdoc/>
        public bool Push(byte[] data)
        {
            if (_isReadOnly) return false;
            if (data == null || data.Length == 0) return false;

            var bytes = Encoding.Unicode.GetString(data);
            _streamWriter.WriteLine(bytes);
            return true;
        }

        /// <inheritdoc/>
        public bool Pull(out byte[] data)
        {
            var line = _streamReader.ReadLine();
            if (line == null)
            {
                data = null;
                return false;
            }

            data = Encoding.Unicode.GetBytes(line);
            return true;
        }

        /// <summary>
        /// Does nothing but implements the interface.
        /// There is nothing to reset in file channel.
        /// </summary>
        public void Reset()
        {
        }

        /// <inheritdoc/>
        protected override void PerformStop()
        {
            CommunicationStopped?.Invoke();
        }

        /// <inheritdoc/>
        protected override void PerformDisposal()
        {
            _streamWriter.Dispose();
            _streamReader.Dispose();
        }
    }
}