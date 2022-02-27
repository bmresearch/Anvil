using Solnet.KeyStore;
using Solnet.Wallet;

namespace Anvil.Services
{
    /// <summary>
    /// A private key loader which exposes the <see cref="SolanaKeyStoreService"/> methods.
    /// </summary>
    public static class SolanaPrivateKeyLoader
    {
        /// <summary>
        /// The solana keystore.
        /// </summary>
        private static readonly SolanaKeyStoreService SolanaKeyStore = new();

        /// <summary>
        /// Import wallet from private key file.
        /// </summary>
        /// <param name="path">The path to the private key file.</param>
        public static Wallet Load(string path)
        {
            return SolanaKeyStore.RestoreKeystoreFromFile(path);
        }
    }
}
