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
        /// Add a new wallet from private key.
        /// </summary>
        /// <param name="pkWallet">The private key based wallet.</param>
        void AddWallet(IPrivateKeyWallet pkWallet);

        /// <summary>
        /// Addd a new wallet from derivation index.
        /// </summary>
        /// <param name="derivationWallet">The derivation index based wallet.</param>
        void AddWallet(IDerivationPathWallet derivationWallet);

        /// <summary>
        /// An event raised whenever the current wallet changes.
        /// </summary>
        event EventHandler<CurrentWalletChangedEventArgs> OnCurrentWalletChanged;
    }
}
