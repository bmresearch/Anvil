namespace Anvil.Services.Wallets
{
    /// <summary>
    /// A derivation index based wallet.
    /// </summary>
    public interface IDerivationIndexWallet : IAliasedWallet
    {
        /// <summary>
        /// The derivation index.
        /// </summary>
        int DerivationIndex { get; }
    }
}
