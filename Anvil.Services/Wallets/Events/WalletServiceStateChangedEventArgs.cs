using Anvil.Services.Wallets.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anvil.Services.Wallets.Events
{
    /// <summary>
    /// The arguments of the event raised whenever the wallet service state changes.
    /// </summary>
    public class WalletServiceStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The type of state change.
        /// </summary>
        public WalletServiceStateChange StateChange;

        /// <summary>
        /// The wallet affected by the state change.
        /// </summary>
        public IWallet Wallet;

        /// <summary>
        /// Initialize the <see cref="WalletServiceStateChangedEventArgs"/> with the given <see cref="WalletServiceStateChange"/> and it's affected <see cref="IWallet"/>.
        /// </summary>
        /// <param name="stateChange">The type state change.</param>
        /// <param name="wallet">The affected wallet.</param>
        public WalletServiceStateChangedEventArgs(WalletServiceStateChange stateChange, IWallet wallet)
        {
            StateChange = stateChange;
            Wallet = wallet;
        }
    }
}
