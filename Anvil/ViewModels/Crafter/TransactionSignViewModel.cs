using Anvil.Core.ViewModels;
using Anvil.Services;
using Avalonia;
using ReactiveUI;
using Solnet.Programs;
using Solnet.Rpc.Models;
using System;
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

        public void SignPayload()
        {
            var msgBytes = Convert.FromBase64String(Payload);
            var signature = _walletService.CurrentWallet.Wallet.Account.Sign(msgBytes);
            Signature = Convert.ToBase64String(signature);
            Signed = true;
        }

        public void CopySignatureToClipboard()
        {
            Application.Current.Clipboard.SetTextAsync(Signature);
        }

        private void DecodeMessageFromPayload()
        {
            Message msg = null;
            Signature = string.Empty;

            try
            {
                msg = Message.Deserialize(Payload);
                InvalidPayload = false;
            } catch(Exception ex)
            {
                InvalidPayload = true;
                DecodedInstructions = new();
                Signed = false;
                return;
            }

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
                    Signed = false;
                }else
                {
                    DecodeMessageFromPayload();
                }
            }
        }

        private bool _invalidPayload;
        public bool InvalidPayload
        {
            get => _invalidPayload;
            set => this.RaiseAndSetIfChanged(ref _invalidPayload, value);
        }

        private bool _signed;
        public bool Signed
        {
            get => _signed;
            set => this.RaiseAndSetIfChanged(ref _signed, value);
        }

        private string _signature;
        public string Signature 
        { 
            get => _signature; 
            set => this.RaiseAndSetIfChanged(ref _signature, value); 
        }
    }
}
