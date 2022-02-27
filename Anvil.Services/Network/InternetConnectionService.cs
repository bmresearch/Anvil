using Anvil.Services.Network.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Anvil.Services.Network
{
    /// <summary>
    /// A service which checks for existing internet connection.
    /// </summary>
    public class InternetConnectionService
    {
        /// <summary>
        /// The host to ping.
        /// </summary>
        private static readonly string Host = "www.google.com";

        /// <summary>
        /// The cancellation token source for the periodic task.
        /// </summary>
        private readonly CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Initialize  the internet service.
        /// </summary>
        public InternetConnectionService()
        {
            _cancellationTokenSource = new();
        }

        /// <summary>
        /// Periodically checks for connection.
        /// </summary>
        /// <returns>A task which checks for the connection.</returns>
        private Task CheckConnection()
        {
            return Task.Run(() =>
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    IsConnected = TryPing();
                    Task.Delay(15000, _cancellationTokenSource.Token).Wait();
                }
            });
        }

        /// <summary>
        /// Attempts to ping the host.
        /// </summary>
        /// <returns>true if it succeeds, else false.</returns>
        private static bool TryPing()
        {
            bool result = false;
            Ping p = new ();
            try
            {
                PingReply reply = p.Send(Host, 443);
                if (reply.Status == IPStatus.Success)
                    return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
            return result;
        }

        /// <summary>
        /// Stops the service.
        /// </summary>
        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }

        /// <summary>
        /// Starts the service.
        /// </summary>
        public void Start()
        {
            CheckConnection();
        }

        /// <summary>
        /// Whether an internet connection exists or not.
        /// </summary>
        private bool _isConnected;

        /// <summary>
        /// Whether an internet connection exists or not.
        /// </summary>
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                _isConnected = value;
                OnNetworkConnectionChanged?.Invoke(this, new NetworkConnectionChangedEventArgs(value));
            }
        }

        /// <summary>
        /// The event raised whenever the connection changes.
        /// </summary>
        public event EventHandler<NetworkConnectionChangedEventArgs> OnNetworkConnectionChanged;
    }
}
