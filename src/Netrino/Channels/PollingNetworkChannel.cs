using Extype;
using Netrino.Channels.Abstractions;
using Netrino.EventHandlers;
using Parallops;

namespace Netrino.Channels
{
    /// <summary>
    /// Repetitively polls the decorated network channel and raises data receive events to subscribers.
    /// </summary>
    public class PollingNetworkChannel : RepetitiveOperationBase, IPollingNetworkChannel
    {
        private readonly INetworkChannel _decoratedNetworkChannel;
        private readonly int _maxFailedReads;
        private int _failedReads;

        /// <inheritdoc/>
        public event CommunicationStartEventHandler CommunicationStarted;

        /// <inheritdoc/>
        public event CommunicationStopEventHandler CommunicationStopped;
        
        /// <inheritdoc/>
        public event DataReceiveEventHandler DataReceived;
        
        /// <summary>
        /// Instantiates the <see cref="PollingNetworkChannel"/> object.
        /// </summary>
        /// <param name="decoratedNetworkChannel">The decorated network channel.</param>
        /// <param name="pollingInterval">The interval in milliseconds the channel is checked for incoming data.</param>
        public PollingNetworkChannel(INetworkChannel decoratedNetworkChannel, int pollingInterval = 100)
            : base(pollingInterval)
        {
            _decoratedNetworkChannel = decoratedNetworkChannel.ThrowIfNull(nameof(decoratedNetworkChannel));
            _maxFailedReads = 10_000 / pollingInterval;
        }

        /// <inheritdoc/>
        protected override void PerformStart()
        {
            _decoratedNetworkChannel.CommunicationStarted += DecoratedNetworkChannel_CommunicationStarted;
            _decoratedNetworkChannel.CommunicationStopped += DecoratedNetworkChannel_CommunicationStopped;
            _decoratedNetworkChannel.Start();
            
            base.PerformStart();
        }

        private void DecoratedNetworkChannel_CommunicationStarted()
        {
            CommunicationStarted?.Invoke();
        }
        
        private void DecoratedNetworkChannel_CommunicationStopped()
        {
            CommunicationStopped?.Invoke();
        }

        /// <inheritdoc/>
        protected override void PerformStop()
        {
            base.PerformStop();
            
            _decoratedNetworkChannel.CommunicationStarted -= DecoratedNetworkChannel_CommunicationStarted;
            _decoratedNetworkChannel.CommunicationStopped -= DecoratedNetworkChannel_CommunicationStopped;
            _decoratedNetworkChannel.Stop();
        }

        /// <inheritdoc/>
        protected override void RepeatOperation()
        {
            if (Pull(out var data))
            {
                _failedReads = 0;
                DataReceived?.Invoke(this, data);
            }
            else
            {
                _failedReads++;
                if (_failedReads > _maxFailedReads)
                {
                    Reset();
                }
            }
        }

        /// <summary>
        /// Delegates the push operation to the decorated channel.
        /// </summary>
        /// <param name="data">The data to push.</param>
        /// <returns>true if data has been sent, false otherwise</returns>
        public bool Push(byte[] data)
        {
            return _decoratedNetworkChannel.Push(data);
        }
        
        /// <summary>
        /// Delegates the attempt to perform pull data to the decorated channel.
        /// </summary>
        /// <param name="data">The data to pull to.</param>
        /// <returns>true if data has been received, false otherwise</returns>
        public bool Pull(out byte[] data)
        {
            return _decoratedNetworkChannel.Pull(out data);
        }

        /// <inheritdoc/>
        public void Reset()
        {
            _decoratedNetworkChannel.Reset();
        }

        /// <inheritdoc/>
        protected override void PerformDisposal()
        {
            _decoratedNetworkChannel.Dispose();
            base.PerformDisposal();
        }
    }
}