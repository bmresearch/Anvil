using Anvil.Core.ViewModels;
using Anvil.Services;
using Anvil.ViewModels.Fields;
using ReactiveUI;
using Solnet.Programs;
using Solnet.Programs.Models;
using Solnet.Rpc;
using Solnet.Rpc.Models;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace Anvil.ViewModels.Crafter
{
    public class TransactionSendViewModel : ViewModelBase
    {
        public string Header => "Send Transaction";

        private IRpcClientProvider _rpcProvider;
        private IRpcClient _rpcClient => _rpcProvider.Client;

        public TransactionSendViewModel(IRpcClientProvider rpcProvider)
        {
            _rpcProvider = rpcProvider;

            RequiredSignatures = new();
            _observables = new();
        }

        public async void SendTransaction()
        {
            var msg = Message.Deserialize(_payload);

            var tx = Transaction.Populate(msg,
                RequiredSignatures.Select(x => Convert.FromBase64String(x.Signature)).ToList());

            var txSig = await _rpcClient.SendTransactionAsync(tx.Serialize());

        }

        private void DecodeMessageFromPayload()
        {
            RequiredSignatures = new();
            _observables = new();

            var msg = Message.Deserialize(Payload);
            // maybe do something about this
            if (msg == null) return;

            for (int i = 0; i < msg.Header.RequiredSignatures; i++)
            {
                var vm = new SignatureWrapperViewModel(msg.AccountKeys[i], Convert.FromBase64String(_payload));
                RequiredSignatures.Add(vm);
                vm.WhenAnyValue(x => x.Verified)
                    .Subscribe(x => CanSendTransaction = RequiredSignatures.All(x => x.Verified == true));
            }
        }

        private ObservableCollection<IObservable<bool>> _observables;

        private string _payload;
        public string Payload
        {
            get => _payload;
            set
            {
                this.RaiseAndSetIfChanged(ref _payload, value);
                DecodeMessageFromPayload();
            }
        }

        private ObservableCollection<SignatureWrapperViewModel> _requiredSignatures;
        public ObservableCollection<SignatureWrapperViewModel> RequiredSignatures
        {
            get => _requiredSignatures;
            set => this.RaiseAndSetIfChanged(ref _requiredSignatures, value);
        }

        private bool _canSendTransaction;
        public bool CanSendTransaction 
        { 
            get => _canSendTransaction; 
            set => this.RaiseAndSetIfChanged(ref _canSendTransaction, value); 
        }
    }
}
