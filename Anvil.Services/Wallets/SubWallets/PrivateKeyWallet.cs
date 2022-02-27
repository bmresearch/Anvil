namespace Anvil.Services.Wallets.SubWallets
{
    /// <summary>
    /// Represents a private key file based wallet.
    /// </summary>
    public class PrivateKeyWallet : IPrivateKeyWallet
    {
        /// <inheritdoc cref="IPrivateKeyWallet.Path"/>
        public string Path { get; init; }

        /// <inheritdoc cref="IAliasedWallet.Alias"/>
        public string Alias { get; set; } = string.Empty;
    }
}
