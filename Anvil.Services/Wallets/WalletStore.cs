using Anvil.Services.Wallets.SubWallets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anvil.Services.Wallets
{
    /// <summary>
    /// Represents the main wallet.
    /// </summary>
    public class WalletStore : IWalletStore
    {
        /// <summary>
        /// Initialize the keystore.
        /// </summary>
        public WalletStore()
        {
            DerivationIndexWallets = new List<DerivationIndexWallet>();
            PrivateKeyWallets = new List<PrivateKeyWallet>();
        }

        /// <summary>
        /// Initialize the keystore with the given mnemonic.
        /// </summary>
        /// <param name="mnemonic">The wallet's mnemonic seed.</param>
        public WalletStore(string mnemonic)
        {
            DerivationIndexWallets = new List<DerivationIndexWallet>();
            PrivateKeyWallets = new List<PrivateKeyWallet>();

            Mnemonic = mnemonic;
        }

        /// <inheritdoc cref="IWalletStore.Mnemonic"/>
        public string Mnemonic { get; set; }

        /// <inheritdoc cref="IWalletStore.DerivationIndexWallets"/>
        public List<DerivationIndexWallet> DerivationIndexWallets { get; set; }

        /// <inheritdoc cref="IWalletStore.PrivateKeyWallets"/>
        public List<PrivateKeyWallet> PrivateKeyWallets { get; set; }

        /// <inheritdoc cref="IWalletStore.AddWallet(DerivationIndexWallet)"/>
        public void AddWallet(DerivationIndexWallet derivationIndexWallet)
        {
            DerivationIndexWallets.Add(derivationIndexWallet);
        }

        /// <inheritdoc cref="IWalletStore.AddWallet(PrivateKeyWallet)"/>
        public void AddWallet(PrivateKeyWallet privateKeyWallet)
        {
            PrivateKeyWallets.Add(privateKeyWallet);
        }

        /// <inheritdoc cref="IWalletStore.RemoveWallets(List{PrivateKeyWallet})"/>
        public void RemoveWallets(List<PrivateKeyWallet> privateKeyWallets)
        {
            foreach (var w in privateKeyWallets)
            {
                PrivateKeyWallets.Remove(w);
            }
        }

        /// <inheritdoc cref="IWalletStore.AddWallet(string)"/>
        public void AddWallet(string mnemonic)
        {
            Mnemonic = mnemonic;
        }

        /// <inheritdoc cref="IWalletStore.EditAlias(DerivationIndexWallet, string)"/>
        public void EditAlias(DerivationIndexWallet derivationIndexWallet, string newAlias)
        {
            var drvw = DerivationIndexWallets.FirstOrDefault(x => x == derivationIndexWallet);
            if (drvw == null) return;

            drvw.Alias = newAlias;
        }

        /// <inheritdoc cref="IWalletStore.AddWallet(PrivateKeyWallet, string)"/>
        public void EditAlias(PrivateKeyWallet privateKeyWallet, string newAlias)
        {
            var pkw = PrivateKeyWallets.FirstOrDefault(x => x == privateKeyWallet);
            if (pkw == null) return;

            pkw.Alias = newAlias;
        }
    }
}
