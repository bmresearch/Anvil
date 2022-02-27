using Anvil.Core.ViewModels;
using Anvil.Services;
using Avalonia;
using ReactiveUI;
using System.Threading.Tasks;

namespace Anvil.ViewModels.Common
{
    /// <summary>
    /// Represents the view model of a transaction submission to the network.
    /// <remarks>
    /// TODO: Add a new store for user actions and store these locally with appropriate types for historical wallet activity view.
    /// </remarks>
    /// </summary>
    public class TransactionSubmissionViewModel : ViewModelBase
    {
        private IRpcClientProvider _rpcClientProvider;

        private bool _submittingTransaction = true;
        private bool _transactionError = false;
        private bool _transactionConfirmed = false;
        private string _transactionHash = string.Empty;
        private string _transactionErrorMessage = string.Empty;
        private string _progress = "Crafting transaction.";

        /// <summary>
        /// Copies the transaction hash to the clipboard.
        /// </summary>
        public async void CopyTransactionHashToClipboard()
        {
            await Application.Current.Clipboard.SetTextAsync(TransactionHash);
        }

        /// <summary>
        /// Initialize a no-show transaction submission view model.
        /// </summary>
        /// <returns>The view model.</returns>
        public static TransactionSubmissionViewModel NoShow() => new(null)
        {
            SubmittingTransaction = false,
        };

        /// <summary>
        /// Initialize the transaction submission.
        /// </summary>
        /// <param name="rpcClientProvider">The rpc client provider.</param>
        public TransactionSubmissionViewModel(IRpcClientProvider rpcClientProvider)
        {
            _rpcClientProvider = rpcClientProvider;
        }

        /// <summary>
        /// Submit a transaction and return a boolean that represents whether it was a successful submission or not.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <returns>A task that performs the action and may return a boolean which represents whether it was a successful submission or not.</returns>
        public async Task<bool> SubmitTransaction(byte[] transaction)
        {
            Progress = "Submitting transaction..";
            var txSig = await _rpcClientProvider.Client.SendTransactionAsync(transaction,
                commitment: Solnet.Rpc.Types.Commitment.Confirmed);

            if (txSig.WasSuccessful)
            {
                TransactionHash = txSig.Result;

                return true;
            }
            else
            {
                SubmittingTransaction = false;
                TransactionError = true;
                TransactionErrorMessage = txSig.Reason;

                return false;
            }
        }

        /// <summary>
        /// Polls the RPC until the transaction has been confirmed.
        /// </summary>
        /// <returns>A task which performs the action.</returns>
        public async Task PollConfirmation()
        {
            Progress = "Awaiting transaction confirmation...";
            _ = await _rpcClientProvider.PollTxAsync(TransactionHash, Solnet.Rpc.Types.Commitment.Confirmed);
            TransactionConfirmed = true;
            SubmittingTransaction = false;
        }

        /// <summary>
        /// Triggers an error while crafting the transaction.
        /// </summary>
        public void CraftingError()
        {
            TransactionError = true;
            TransactionErrorMessage = "Something went wrong, please try again.";
            SubmittingTransaction = false;
        }

        public bool SubmittingTransaction
        {
            get => _submittingTransaction;
            set => this.RaiseAndSetIfChanged(ref _submittingTransaction, value);
        }

        public bool TransactionError
        {
            get => _transactionError;
            set => this.RaiseAndSetIfChanged(ref _transactionError, value);
        }

        public bool TransactionConfirmed
        {
            get => _transactionConfirmed;
            set => this.RaiseAndSetIfChanged(ref _transactionConfirmed, value);
        }

        public string TransactionHash
        {
            get => _transactionHash;
            set => this.RaiseAndSetIfChanged(ref _transactionHash, value);
        }

        public string Progress
        {
            get => _progress;
            set => this.RaiseAndSetIfChanged(ref _progress, value);
        }

        public string TransactionErrorMessage
        {
            get => _transactionErrorMessage;
            set => this.RaiseAndSetIfChanged(ref _transactionErrorMessage, value);
        }
    }
}
