using Anvil.Services.Wallets;
using Anvil.Services.Wallets.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anvil.Services
{
    /// <summary>
    /// Specifies functionality for a wallet service.
    /// </summary>
    public interface IWalletService
    {
        /// <summary>
        /// The current wallet.
        /// </summary>
        IWallet CurrentWallet { get; }

        /// <summary>
        /// The list of available wallets.
        /// </summary>
        List<IWallet> Wallets { get; }

        /// <summary>
        /// Changes the current wallet.
        /// </summary>
        /// <param name="wallet">The wallet to change to.</param>
        void ChangeWallet(IWallet wallet);

        /// <summary>
        /// Add the main wallet.
        /// </summary>
        /// <param name="wallet">The main wallet.</param>
        IWallet AddWallet(IWalletStore wallet);

        /// <summary>
        /// Add a new wallet from private key.
        /// </summary>
        /// <param name="pkWallet">The private key based wallet.</param>
        IWallet AddWallet(PrivateKeyWallet pkWallet);

        /// <summary>
        /// Addd a new wallet from derivation index.
        /// </summary>
        /// <param name="derivationWallet">The derivation index based wallet.</param>
        IWallet AddWallet(DerivationIndexWallet derivationWallet);

        /// <summary>
        /// An event raised whenever the current wallet changes.
        /// </summary>
        event EventHandler<CurrentWalletChangedEventArgs> OnCurrentWalletChanged;

        /// <summary>
        /// An event raised whenever the wallet service state changes.
        /// </summary>
        event EventHandler<WalletServiceStateChangedEventArgs> OnWalletServiceStateChanged;
    }
}
