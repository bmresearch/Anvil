using Anvil.Services.Events;
using Anvil.Services.Wallets;
using Anvil.Services.Wallets.Events;
using Anvil.Services.Wallets.SubWallets;
using System;
using System.Collections.Generic;

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
        /// Edits the alias of the given wallet.
        /// </summary>
        /// <param name="address">The wallet to edit the alias.</param>
        /// <param name="newAlias">The new alias.</param>
        void EditAlias(string address, string newAlias);

        /// <summary>
        /// Add the main wallet's mnemonic.
        /// </summary>
        /// <param name="mnemonic">The mnemonic.</param>
        void AddWallet(string mnemonic);

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
        /// Whether a mnemonic has been imported.
        /// </summary>
        public bool MnemonicImported { get; }

        /// <summary>
        /// Generate a new wallet from the mnemonic.
        /// </summary>
        /// <returns>The generated <see cref="IWallet"/>.</returns>
        public IWallet GenerateNewWallet();

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
