using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anvil.Services.Wallets
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
