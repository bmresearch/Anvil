namespace Anvil.Services.Enums
{
    /// <summary>
    /// Enumerates the different kinds of state changes for the wallet service.
    /// </summary>
    public enum WalletServiceStateChange
    {
        /// <summary>
        /// The addition of a new wallet.
        /// </summary>
        Addition,

        /// <summary>
        /// The removal of a new wallet.
        /// </summary>
        Removal,

        /// <summary>
        /// The alias was edited.
        /// </summary>
        AliasChanged
    }
}
