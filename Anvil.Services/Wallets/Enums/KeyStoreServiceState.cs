namespace Anvil.Services.Wallets.Enums
{
    /// <summary>
    /// Represents the <see cref="KeyStoreService"/> state.
    /// </summary>
    public enum KeyStoreServiceState
    {
        /// <summary>
        /// The <see cref="KeyStoreService"/> is initializing.
        /// </summary>
        Initializing,

        /// <summary>
        /// The <see cref="KeyStoreService"/> is decoding the stored wallet.
        /// </summary>
        Decoding,

        /// <summary>
        /// The <see cref="KeyStoreService"/> has finished initializing.
        /// </summary>
        Done
    }

}
