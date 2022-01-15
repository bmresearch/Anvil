using Solnet.Wallet;
using System;

namespace Anvil.Services.Wallets
{
    /// <summary>
    /// 
    /// </summary>
    public class PrivateKeyWallet : IPrivateKeyWallet
    {
        private Wallet _wallet;

        public PrivateKeyWallet(Wallet wallet)
        {
            _wallet = wallet;
        }

        public string Path { get; init; }

        public PublicKey Address 
        { 
            get => _wallet.Account.PublicKey;
        }

        public Wallet Wallet => _wallet;

        public byte[] Sign(byte[] data)
        {
            return _wallet.Account.Sign(data);
        }
    }
}
