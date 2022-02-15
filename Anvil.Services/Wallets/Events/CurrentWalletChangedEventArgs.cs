using System;

namespace Anvil.Services.Wallets.Events
{
    /// <summary>
    /// The arguments of the event raised whenever the current wallet changes.
    /// </summary>
    public class CurrentWalletChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The new current wallet.
        /// </summary>
        public IWallet Wallet { get; init; }

        /// <summary>
        /// Initialize the <see cref="CurrentWalletChangedEventArgs"/> with the given <see cref="IWallet"/>.
        /// </summary>
        /// <param name="wallet">The new current wallet.</param>
        public CurrentWalletChangedEventArgs(IWallet wallet)
        {
            Wallet = wallet;
        }
    }
}
