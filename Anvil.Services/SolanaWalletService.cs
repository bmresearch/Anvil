using Anvil.Services.Wallets;
using Anvil.Services.Wallets.Enums;
using Anvil.Services.Wallets.SubWallets;
using Solnet.Wallet;
using Solnet.Wallet.Bip39;

namespace Anvil.Services
{
    /// <summary>
    /// Implements the common interface for all Solana based wallets.
    /// </summary>
    public class SolanaWalletService : IWallet
    {
        /// <summary>
        /// The wallet.
        /// </summary>
        private Wallet _wallet;

        /// <summary>
        /// The primary account provided by this wallet.
        /// </summary>
        private Account _account;

        /// <summary>
        /// Initialize the <see cref="SolanaWalletService"/> with the given <see cref="IWalletStore"/> and <see cref="IDerivationIndexWallet"/>.
        /// </summary>
        /// <param name="mnemonic">The mnemonic.</param>
        /// <param name="derivationIndexWallet">The derivation index wallet.</param>
        public SolanaWalletService(string mnemonic, IDerivationIndexWallet derivationIndexWallet)
        {
            //_walletStore = mainWallet;
            AliasedWallet = derivationIndexWallet;
            _wallet = new Wallet(mnemonic, WordList.AutoDetect(mnemonic));
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
            _wallet = SolanaPrivateKeyLoader.Load(privateKeyWallet.Path);
            AliasedWallet = privateKeyWallet;
            SubWalletType = SubWalletType.PrivateKey;

            InitializeAccount();
        }

        /// <summary>
        /// Initialize the underlying account provided by the wallet.
        /// </summary>
        private void InitializeAccount()
        {
            if (AliasedWallet is IDerivationIndexWallet derivationIndexWallet)
            {
                _account = _wallet.GetAccount(derivationIndexWallet.DerivationIndex);
            }
            else if (AliasedWallet is IPrivateKeyWallet)
            {
                _account = _wallet.Account;
            }
            else
            {
                _account = _wallet.Account;
            }
        }

        /// <inheritdoc cref="IWallet.AliasedWallet"/>
        public IAliasedWallet AliasedWallet { get; }

        /// <inheritdoc cref="IWallet.Alias"/>
        public string Alias
        {
            get => AliasedWallet.Alias;
            set => AliasedWallet.Alias = value;
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
