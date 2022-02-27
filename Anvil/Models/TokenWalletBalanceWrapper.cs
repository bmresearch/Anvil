using Solnet.Extensions;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anvil.Models
{
    /// <summary>
    /// A wrapper for <see cref="Solnet.Extensions.TokenWalletBalance"/>.
    /// </summary>
    public class TokenWalletBalanceWrapper
    {
        private TokenWalletBalance _tokenWalletBalance;
        private PublicKey _mint;
        private string _name;
        private ulong _rawBalance;
        private decimal _balance;
        private int _decimals;

        public TokenWalletBalanceWrapper(TokenWalletBalance walletBalance)
        {
            _tokenWalletBalance = walletBalance;
        }

        public TokenWalletBalanceWrapper(string name, ulong rawBalance, decimal balance, int decimals, PublicKey mint)
        {
            _name = name;
            _rawBalance = rawBalance;
            _balance = balance;
            _decimals = decimals;
            _mint = mint;
        }

        public int Decimals
        {
            get
            {
                return _tokenWalletBalance != null ? _tokenWalletBalance.DecimalPlaces : _decimals;
            }
        }

        public ulong RawBalance
        {
            get
            {
                return _tokenWalletBalance != null ? _tokenWalletBalance.QuantityRaw : _rawBalance;
            }
        }

        public decimal Balance
        {
            get
            {
                return _tokenWalletBalance != null ? _tokenWalletBalance.QuantityDecimal : _balance;
            }
        }

        public string TokenName
        {
            get
            {
                return _tokenWalletBalance != null ? _tokenWalletBalance.TokenName : _name;
            }
        }

        public string TokenMint
        {
            get
            {
                return _tokenWalletBalance != null ? _tokenWalletBalance.TokenMint : _mint;
            }
        }

        public TokenWalletBalance TokenWalletBalance { get => _tokenWalletBalance; }
    }
}
