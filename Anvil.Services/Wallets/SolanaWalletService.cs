using Solnet.Wallet;
using Solnet.Wallet.Bip39;
using System;
using System.Linq;

namespace Anvil.Services.Wallets
{
    /// <summary>
    /// Implements the common interface for all Solana based wallets.
    /// </summary>
    public class SolanaWalletService : IWallet
    {
        /// <summary>
        /// The main wallet.
        /// </summary>
        private IWalletStore _walletStore;

        /// <summary>
        /// The aliased wallet.
        /// </summary>
        private IAliasedWallet _aliasedWallet;

        /// <summary>
        /// The wallet.
        /// </summary>
        private Wallet _wallet;

        /// <summary>
        /// The primary account provided by this wallet.
        /// </summary>
        private Account _account;

        /// <summary>
        /// Initialize the <see cref="SolanaWalletService"/> with the given <see cref="IWalletStore"/>.
        /// </summary>
        /// <param name="mainWallet">The main wallet.</param>
        public SolanaWalletService(IWalletStore mainWallet)
        {
            _walletStore = mainWallet;
            _aliasedWallet = new DerivationIndexWallet { DerivationIndex = 0 };
            _wallet = new Wallet(mainWallet.Mnemonic, WordList.AutoDetect(mainWallet.Mnemonic));
            SubWalletType = SubWalletType.DerivationIndex;

            InitializeAccount();
        }

        /// <summary>
        /// Initialize the <see cref="SolanaWalletService"/> with the given <see cref="IWalletStore"/> and <see cref="IDerivationIndexWallet"/>.
        /// </summary>
        /// <param name="mainWallet">The main wallet.</param>
        /// <param name="derivationIndexWallet">The derivation index wallet.</param>
        public SolanaWalletService(IWalletStore mainWallet, IDerivationIndexWallet derivationIndexWallet)
        {
            _walletStore = mainWallet;
            _aliasedWallet = derivationIndexWallet;
            _wallet = new Wallet(mainWallet.Mnemonic, WordList.AutoDetect(mainWallet.Mnemonic));
            SubWalletType = SubWalletType.DerivationIndex;

            InitializeAccount();
        }

        /// <summary>
        /// Initialize the <see cref="SolanaWalletService"/> with the given <see cref="IWalletStore"/> and <see cref="IPrivateKeyWallet"/>.
        /// </summary>
        /// <param name="mainWallet">The main wallet.</param>
        /// <param name="privateKeyWallet">The private key file based wallet.</param>
        public SolanaWalletService(IPrivateKeyWallet privateKeyWallet)
        {
            _wallet = KeyStoreService.LoadPrivateKeyFile(privateKeyWallet.Path);
            _aliasedWallet = privateKeyWallet;
            SubWalletType = SubWalletType.PrivateKey;

            InitializeAccount();
        }

        /// <inheritdoc cref="IWallet.GenerateNewWallet"/>
        public IWallet GenerateNewWallet()
        {
            if (_walletStore != null)
            {
                int idx = _walletStore.DerivationIndexWallets.Select(w => w.DerivationIndex).DefaultIfEmpty(0).Max() + 1;

                _wallet.GetAccount(idx);

                var derivationWallet = new DerivationIndexWallet()
                {
                    DerivationIndex = idx,
                };

                _walletStore.DerivationIndexWallets.Add(derivationWallet);

                return new SolanaWalletService(_walletStore, derivationWallet);
            }

            throw new Exception("Unable to derive from non-main wallet.");
        }

        /// <summary>
        /// Initialize the underlying account provided by the wallet.
        /// </summary>
        private void InitializeAccount()
        {
            if (_aliasedWallet is IDerivationIndexWallet derivationIndexWallet)
            {
                _account = _wallet.GetAccount(derivationIndexWallet.DerivationIndex);
            } else if (_aliasedWallet is IPrivateKeyWallet)
            {
                _account = _wallet.Account;
            } else
            {
                _account = _wallet.Account;
            }
        }

        /// <inheritdoc cref="IWallet.ShortenedAddress"/>
        public string ShortenedAddress => _account.PublicKey.Key[..6] + "..." + _account.PublicKey.Key[^6..];

        /// <inheritdoc cref="IWallet.SubWalletType"/>
        public SubWalletType SubWalletType { get; init; }

        /// <inheritdoc cref="IWallet.Address"/>
        public PublicKey Address => _account;

        /// <inheritdoc cref="IWallet.Sign(byte[])"/>
        public byte[] Sign(byte[] data) => _account.Sign(data);
    }
}
