using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anvil.Services.Wallets.Events
{
    /// <summary>
    /// The arguments of the event raised whenever the current wallet changes.
    /// </summary>
    public class CurrentWalletChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The new current wallet.
        /// </summary>
        public IWallet Wallet { get; init; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wallet"></param>
        public CurrentWalletChangedEventArgs(IWallet wallet)
        {
            Wallet = wallet;
        }
    }
}
