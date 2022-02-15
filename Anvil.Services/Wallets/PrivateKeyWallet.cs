namespace Anvil.Services.Wallets
{
    /// <summary>
    /// Represents a private key file based wallet.
    /// </summary>
    public class PrivateKeyWallet : IPrivateKeyWallet
    {
        /// <summary>
        /// Initialize the private key file based wallet with the given path.
        /// </summary>
        /// <param name="path">The path to the private key file.</param>
        public PrivateKeyWallet(string path)
        {
            Path = path;
        }

        /// <inheritdoc cref="IPrivateKeyWallet.Path"/>
        public string Path { get; init; }

        /// <inheritdoc cref="IAliasedWallet.Alias"/>
        public string Alias { get; set; } = string.Empty;
    }
}
