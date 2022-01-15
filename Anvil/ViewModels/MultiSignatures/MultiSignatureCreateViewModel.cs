using Anvil.Core.ViewModels;
using Anvil.Services;
using Anvil.ViewModels.Fields;
using ReactiveUI;
using Solnet.Programs;
using Solnet.Programs.Models;
using Solnet.Programs.Utilities;
using Solnet.Rpc;
using Solnet.Rpc.Builders;
using Solnet.Rpc.Models;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Anvil.ViewModels.MultiSignatures
{
    public class MultiSignatureCreateViewModel : ViewModelBase
    {
        public string Header => "Create MultiSig";

        private IRpcClientProvider _rpcProvider;
        private IRpcClient _rpcClient => _rpcProvider.Client;
        private IWalletService _walletService;
        private IMultiSignatureAccountMappingStore _multiSigAccountMappingStore;
        private ulong _rentExemptionLamports;

        public MultiSignatureCreateViewModel(IRpcClientProvider rpcClientProvider, IWalletService walletService, IMultiSignatureAccountMappingStore multiSignatureAccountMappingStore)
        {
            _rpcProvider = rpcClientProvider;
            _walletService = walletService;
            _multiSigAccountMappingStore = multiSignatureAccountMappingStore;

            Signers = new()
            {
                new RequiredPublicKeyViewModel(true),
                new RequiredPublicKeyViewModel(true),
            };
            MultiSigAccount = new();
            GetMultiSignatureRent();
        }

        private async void GetMultiSignatureRent()
        {
            var rentExemption = await _rpcClient.GetMinimumBalanceForRentExemptionAsync(MultiSignatureAccount.Layout.Length);

            if (rentExemption.WasSuccessful)
            {
                _rentExemptionLamports = rentExemption.Result;
                MultiSigRent = (double) rentExemption.Result / SolHelper.LAMPORTS_PER_SOL;
            }
        }

        public async void CreateMultiSigAccount()
        {
            var blockHash = await _rpcClient.GetRecentBlockHashAsync();

            var success = int.TryParse(RequiredSigners, out int minSigners);
            if (!success) return;

            var tx = new TransactionBuilder()
                .SetRecentBlockHash(blockHash.Result.Value.Blockhash)
                .SetFeePayer(_walletService.CurrentWallet.Wallet.Account.PublicKey)
                .AddInstruction(SystemProgram.CreateAccount(
                    _walletService.CurrentWallet.Wallet.Account.PublicKey,
                    MultiSigAccount.PublicKey,
                    _rentExemptionLamports,
                    MultiSignatureAccount.Layout.Length,
                    TokenProgram.ProgramIdKey))
                .AddInstruction(TokenProgram.InitializeMultiSignature(
                    MultiSigAccount.PublicKey,
                    Signers.Select(x => x.PublicKey),
                    minSigners))
                .Build(new List<Account> { _walletService.CurrentWallet.Wallet.Account, MultiSigAccount });


            var txSig = await _rpcClient.SendTransactionAsync(tx);

            if (txSig.WasSuccessful)
            {
                if(txSig != null)
                {
                    var txMeta = await PollConfirmedTx(txSig.Result);
                }
            }

        }

        /// <summary>
        /// Polls the rpc client until a transaction signature has been confirmed.
        /// </summary>
        /// <param name="signature">The first transaction signature.</param>
        private async Task<TransactionMetaSlotInfo> PollConfirmedTx(string signature)
        {
            var txMeta = await _rpcClient.GetTransactionAsync(signature, Solnet.Rpc.Types.Commitment.Confirmed);
            if (txMeta.WasSuccessful) return txMeta.Result;
            while (!txMeta.WasSuccessful)
            {
                Thread.Sleep(5000);
                txMeta = await _rpcClient.GetTransactionAsync(signature);
                if (txMeta.WasSuccessful) return txMeta.Result;
            }
            return null;
        }

        public void AddSigner()
        {
            Signers.Add(new RequiredPublicKeyViewModel(false));
        }

        public void RemoveSigner(RequiredPublicKeyViewModel vm)
        {
            Signers.Remove(vm);
        }

        public void GenerateNewAccount()
        {
            MultiSigAccount = new ();
        }

        private double _multiSigRent;
        public double MultiSigRent
        {
            get => _multiSigRent;
            set => this.RaiseAndSetIfChanged(ref _multiSigRent, value);
        }

        private string _requiredSigners;
        public string RequiredSigners
        {
            get => _requiredSigners;
            set => this.RaiseAndSetIfChanged(ref _requiredSigners, value);
        }

        private Account _multiSigAccount;
        public Account MultiSigAccount
        {
            get => _multiSigAccount;
            set => this.RaiseAndSetIfChanged(ref _multiSigAccount, value);
        }

        public ObservableCollection<RequiredPublicKeyViewModel> Signers { get; }
    }
}
