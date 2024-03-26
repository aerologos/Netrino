using System.Threading;
using Netrino.Channels.Abstractions;

namespace Netrino.Extensions
{
    /// <summary>
    /// Extends the <see cref="INetworkChannel"/> interface.
    /// </summary>
    public static class INetworkChannelExtensions
    {
        /// <summary>
        /// Checks whether the channel has incoming data or not.
        /// </summary>
        /// <param name="channel">The channel to check.</param>
        /// <param name="numberOfChecks">The number of check required to perform before exiting.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>true if has, false otherwise</returns>
        public static bool HasIncomingData(this INetworkChannel channel, int numberOfChecks, CancellationToken token)
        {
            channel.Start();

            for (int i = 0; i < numberOfChecks; i++)
            {
                token.ThrowIfCancellationRequested();
                    
                Thread.Sleep(100); // wait for data to come

                if (channel.Pull(out _)) return true;
            }
            
            // Data is not coming.
            // This is not the channel you are looking for.
            channel.Stop();
            channel.Dispose();

            return false;
        }
    }
}