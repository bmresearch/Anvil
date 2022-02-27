namespace Anvil.Services.Wallets.SubWallets
{
    /// <summary>
    /// Specifies functionality for an aliased wallet.
    /// </summary>
    public interface IAliasedWallet
    {
        /// <summary>
        /// The wallet's alias.
        /// </summary>
        public string Alias { get; set; }
    }
}
