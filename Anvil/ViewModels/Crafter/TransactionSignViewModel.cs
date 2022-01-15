using Anvil.Core.ViewModels;
using Anvil.Services;
using ReactiveUI;
using Solnet.Programs;
using Solnet.Rpc.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Anvil.ViewModels.Crafter
{
    public class TransactionSignViewModel : ViewModelBase
    {
        private IWalletService _walletService;
        public string Header => "Sign Transaction";


        public TransactionSignViewModel(IWalletService walletService)
        {
            _walletService = walletService;
        }

        private void DecodeMessageFromPayload()
        {
            var msg = Message.Deserialize(Payload);

            var ixs = InstructionDecoder.DecodeInstructions(msg);

            DecodedInstructions = new();
            foreach(var ix in ixs)
            {
                DecodedInstructions.Add(ix);
            }
        }

        private ObservableCollection<DecodedInstruction> _decodedInstructions;
        public ObservableCollection<DecodedInstruction> DecodedInstructions
        {
            get => _decodedInstructions;
            set => this.RaiseAndSetIfChanged(ref _decodedInstructions, value);
        }

        private string _payload;
        public string Payload
        {
            get => _payload;
            set
            {
                this.RaiseAndSetIfChanged(ref _payload, value);
                if (string.IsNullOrEmpty(_payload))
                {
                    DecodedInstructions = new();
                }else
                {
                    DecodeMessageFromPayload();
                }
            }
        }

        private string _signature;
        public string Signature 
        { 
            get => _signature; 
            set => this.RaiseAndSetIfChanged(ref _signature, value); 
        }
    }
}
