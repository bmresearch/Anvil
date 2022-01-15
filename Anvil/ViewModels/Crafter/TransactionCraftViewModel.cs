using Anvil.Core.ViewModels;
using Anvil.Services;
using Anvil.ViewModels.Fields;
using ReactiveUI;
using Solnet.Extensions;
using Solnet.Programs;
using Solnet.Programs.Models;
using Solnet.Rpc;
using Solnet.Rpc.Models;
using Solnet.Rpc.Utilities;
using Solnet.Wallet;
using Solnet.Wallet.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Anvil.ViewModels.Crafter
{
    public class TransactionCraftViewModel : ViewModelBase
    {
        public string Header => "Craft Transaction";

        private IRpcClientProvider _rpcProvider;
        private IRpcClient _rpcClient => _rpcProvider.Client;
        private INonceAccountMappingStore _nonceAccountMappingStore;
        private TokenWallet _tokenWallet;
        private TokenMintResolver _tokenMintResolver;

        private string _sourceAccountData;
        private MultiSignatureAccount _sourceMultiSigAccount;

        public static readonly List<string> Assets = new List<string>() { };

        public TransactionCraftViewModel(IRpcClientProvider rpcProvider, INonceAccountMappingStore nonceAccountMappingStore)
        {
            _rpcProvider = rpcProvider;
            _nonceAccountMappingStore = nonceAccountMappingStore;

            SourceAccount = new PublicKeyViewModel();
            DestinationAccount = new PublicKeyViewModel();
        }

        private async Task GetAccount()
        {
            var account = await GetAccountInfo();
            if(account != null)
            {
                _sourceAccountData = account.Data[0];
                var accountBytes = Convert.FromBase64String(_sourceAccountData);
                if(accountBytes.Length == MultiSignatureAccount.Layout.Length)
                {
                    _sourceMultiSigAccount = MultiSignatureAccount.Deserialize(accountBytes);
                    if (_sourceMultiSigAccount != null)
                        MultiSigSource = true;
                }
                var tokenResolver = await TokenMintResolver.LoadAsync();
                _tokenWallet = await TokenWallet.LoadAsync(_rpcClient, tokenResolver, new(SourceAccount.PublicKeyString));
            }
            var tokenAccounts = await GetTokenAccounts();
            if(tokenAccounts != null)
            {

            }
        }

        private async Task<ulong> GetBalance()
        {
            var account = await _rpcClient.GetBalanceAsync(SourceAccount.PublicKeyString);

            return account.WasSuccessful ? account.Result.Value : 0;
        }

        private async Task<AccountInfo> GetAccountInfo()
        {
            var account = await _rpcClient.GetAccountInfoAsync(SourceAccount.PublicKeyString);
           
            return account.WasSuccessful ? account.Result.Value : null;
        }

        private async Task<List<TokenAccount>> GetTokenAccounts()
        {
            var tokenAccounts = await _rpcClient.GetTokenAccountsByOwnerAsync(SourceAccount.PublicKeyString, tokenProgramId: TokenProgram.ProgramIdKey);

            return tokenAccounts.WasSuccessful ? tokenAccounts.Result.Value : null;
        }

        private bool _multiSigSource;
        private bool MultiSigSource
        {
            get => _multiSigSource;
            set => this.RaiseAndSetIfChanged(ref _multiSigSource, value);
        }

        private float _assetAmount;
        public float AssetAmount
        {
            get => _assetAmount;
            set => this.RaiseAndSetIfChanged(ref _assetAmount, value);
        }

        private PublicKeyViewModel _sourceAccount;
        public PublicKeyViewModel SourceAccount
        {
            get => _sourceAccount;
            set => this.RaiseAndSetIfChanged(ref _sourceAccount, value);
        }

        private PublicKeyViewModel _destinationAccount;
        public PublicKeyViewModel DestinationAccount
        {
            get => _destinationAccount;
            set => this.RaiseAndSetIfChanged(ref _destinationAccount, value);
        }

        public int SelectedAssetIndex
        {
            get; set;
        }

        private string _selectedAsset;
        public string SelectedAsset
        {
            get => _selectedAsset;
            set => this.RaiseAndSetIfChanged(ref _selectedAsset, value);
        }


        private string _payload;
        public string Payload
        {
            get => _payload;
            set => this.RaiseAndSetIfChanged(ref _payload, value);
        }
    }
}
