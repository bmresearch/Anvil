using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anvil.Services.Wallets
{
    /// <summary>
    /// Specifies functionality for a generic wallet.
    /// </summary>
    public interface IWallet
    {
        /// <summary>
        /// The wallet.
        /// </summary>
        Wallet Wallet { get; }
    }
}
