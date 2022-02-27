using Anvil.Core.ViewModels;
using Anvil.Services;
using Anvil.Services.Network;
using Anvil.ViewModels.Fields;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using Solnet.Programs;
using Solnet.Programs.Models;
using Solnet.Rpc;
using Solnet.Rpc.Models;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace Anvil.ViewModels.Crafter
{
    public class TransactionSendViewModel : ViewModelBase
    {
        public string Header => "Send Transaction";
        private IClassicDesktopStyleApplicationLifetime _appLifetime;
        private IRpcClientProvider _rpcProvider;
        private IRpcClient _rpcClient => _rpcProvider.Client;

        private InternetConnectionService _internetConnectionService;

        public TransactionSendViewModel(IClassicDesktopStyleApplicationLifetime appLifetime,
            InternetConnectionService internetConnectionService, IRpcClientProvider rpcProvider)
        {
            _appLifetime = appLifetime;
            _rpcProvider = rpcProvider;
            _internetConnectionService = internetConnectionService;
            _internetConnectionService.OnNetworkConnectionChanged += OnNetworkConnectionChanged;

            RequiredSignatures = new();
            NoConnection = !_internetConnectionService.IsConnected;
        }

        private void OnNetworkConnectionChanged(object? sender, Services.Network.Events.NetworkConnectionChangedEventArgs e)
        {
            NoConnection = !e.Connected;
        }

        public async void LoadPayloadFromFile()
        {
            var ofd = new OpenFileDialog()
            {
                AllowMultiple = false,
                Title = "Select Payload File",
                Filters = new()
                {
                    new FileDialogFilter()
                    {
                        Name = "*",
                        Extensions = new() { "tx" }
                    }
                }
            };
            var selected = await ofd.ShowAsync(_appLifetime.MainWindow);
            if (selected == null) return;
            if (selected.Length > 0)
            {
                if (!File.Exists(selected[0])) return;

                Payload = await File.ReadAllTextAsync(selected[0]);
            }
        }

        public void CopyTransactionHashToClipboard()
        {
            Application.Current.Clipboard.SetTextAsync(TransactionHash);
        }

        public async void SendTransaction()
        {
            SubmittingTransaction = true;

            Progress = "Populating transaction with signatures.";
            var msg = Message.Deserialize(Payload);
            var tx = Transaction.Populate(msg,
                RequiredSignatures.Select(x => Convert.FromBase64String(x.Signature)).ToList());

            Progress = "Submitting transaction..";
            var txSig = await _rpcClient.SendTransactionAsync(tx.Serialize());

            if (txSig.WasSuccessful)
            {
                Progress = "Awaiting transaction confirmation...";
                var res = await _rpcProvider.PollTxAsync(txSig.Result, Solnet.Rpc.Types.Commitment.Confirmed);
                TransactionHash = txSig.Result;
                TransactionConfirmed = true;
                TransactionError = false;
                SubmittingTransaction = false;
            }
            else
            {
                TransactionError = true;
                TransactionConfirmed = false;
                TransactionErrorMessage = txSig.Reason;
                SubmittingTransaction = false;
            }
        }

        private void DecodeMessageFromPayload()
        {
            RequiredSignatures = new();
            TransactionConfirmed = false;
            TransactionHash = string.Empty;
            TransactionError = false;
            TransactionErrorMessage = string.Empty;

            Message? msg = null;
            try
            {
                msg = Message.Deserialize(Payload);
                InvalidPayload = false;
            }
            catch (Exception)
            {
                InvalidPayload = true;
                return;
            }
            if (msg == null) return;

            for (int i = 0; i < msg.Header.RequiredSignatures; i++)
            {
                var vm = new SignatureWrapperViewModel(msg.AccountKeys[i], Convert.FromBase64String(_payload));
                RequiredSignatures.Add(vm);
                vm.WhenAnyValue(x => x.Verified)
                    .Subscribe(x => CanSendTransaction = RequiredSignatures.All(x => x.Verified == true));
            }
        }

        private string _progress;
        public string Progress
        {
            get => _progress;
            set => this.RaiseAndSetIfChanged(ref _progress, value);
        }

        private bool _submittingTransaction;
        public bool SubmittingTransaction
        {
            get => _submittingTransaction;
            set => this.RaiseAndSetIfChanged(ref _submittingTransaction, value);
        }

        private bool _noConnection;
        public bool NoConnection
        {
            get => _noConnection;
            set => this.RaiseAndSetIfChanged(ref _noConnection, value);
        }

        private bool _transactionError;
        public bool TransactionError
        {
            get => _transactionError;
            set => this.RaiseAndSetIfChanged(ref _transactionError, value);
        }

        private string _transactionErrorMessage;
        public string TransactionErrorMessage
        {
            get => _transactionErrorMessage;
            set => this.RaiseAndSetIfChanged(ref _transactionErrorMessage, value);
        }

        private bool _transactionConfirmed;
        public bool TransactionConfirmed
        {
            get => _transactionConfirmed;
            set => this.RaiseAndSetIfChanged(ref _transactionConfirmed, value);
        }

        private string _transactionHash;
        public string TransactionHash
        {
            get => _transactionHash;
            set => this.RaiseAndSetIfChanged(ref _transactionHash, value);
        }

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

        private bool _invalidPayload;
        public bool InvalidPayload
        {
            get => _invalidPayload;
            set => this.RaiseAndSetIfChanged(ref _invalidPayload, value);
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
