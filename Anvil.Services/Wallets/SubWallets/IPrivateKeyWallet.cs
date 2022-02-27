namespace Anvil.Services.Wallets.SubWallets
{
    /// <summary>
    /// A private key based wallet.
    /// </summary>
    public interface IPrivateKeyWallet : IAliasedWallet
    {
        /// <summary>
        /// The path to the keystore.
        /// </summary>
        string Path { get; }
    }
}
