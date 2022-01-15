using Anvil.Services.Wallets;
using Solnet.KeyStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anvil.Services
{
    /// <summary>
    /// The keystore service.
    /// </summary>
    public class KeyStoreService
    {
        /// <summary>
        /// The secret keystore.
        /// </summary>
        private static readonly SecretKeyStoreService SecretKeyStore = new();

        /// <summary>
        /// The solana keystore.
        /// </summary>
        private static readonly SolanaKeyStoreService SolanaKeyStore = new();

        /// <summary>
        /// The wallet service
        /// </summary>
        private IWalletService _walletService;

        /// <summary>
        /// Initialize the keystore service.
        /// </summary>
        /// <param name="walletService">The wallet service.</param>
        public KeyStoreService(IWalletService walletService)
        {
            _walletService = walletService;
        }

        /// <summary>
        /// Import wallet from private key file.
        /// </summary>
        /// <param name="path">The path to the private key file.</param>
        /// <param name="passphrase">The passphrase used to generate the key pair.</param>
        public void ImportPrivateKeyFile(string path, string passphrase = "")
        {
            var wallet = SolanaKeyStore.RestoreKeystoreFromFile(path, passphrase);
            _walletService.AddWallet(new PrivateKeyWallet(wallet));
        }


        /// <summary>
        /// Encrypt the passed data and return the json string.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="data">The data to encrypt.</param>
        /// <param name="address">The address associated with the keystore.</param>
        /// <returns>The json string.</returns>
        public static string Encrypt(string password, byte[] data, string address)
        {
            return SecretKeyStore.EncryptAndGenerateDefaultKeyStoreAsJson(password, data, address);
        }

        /// <summary>
        /// Decrypt the passed json string,
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="data">The json string to decrypt.</param>
        /// <returns>The data.</returns>
        public static byte[] Decrypt(string password, string data)
        {
            try { return SecretKeyStore.DecryptKeyStoreFromJson(password, data); }
            catch (Exception) { return null; }
        }
    }
}
