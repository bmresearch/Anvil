using Anvil.Services.Wallets.SubWallets;
using System.Text.Json.Serialization;

namespace Anvil.Services.Store.Models
{
    /// <summary>
    /// Represents a watch-only account.
    /// </summary>
    public class WatchOnlyAccount : IAliasedWallet
    {
        /// <summary>
        /// The alias of the watch-only account. Used for search features.
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// The account.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// The shortened address.
        /// </summary>
        [JsonIgnore]
        public string ShortenedAddress => Address[..6] + "..." + Address[^6..];
    }
}
