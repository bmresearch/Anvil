using Anvil.Core.ViewModels;
using Anvil.Services;
using Anvil.ViewModels.Fields;
using Avalonia.Controls;
using ReactiveUI;
using Solnet.Programs.Utilities;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anvil.ViewModels.Dialogs
{
    public class CreateMultiSignatureAccountDialogViewModel : ViewModelBase
    {
        /// <summary>
        /// The account being created.
        /// </summary>
        private Account _account = new();
        public Account Account
        {
            get => _account;
            set => this.RaiseAndSetIfChanged(ref _account, value);
        }

        /// <summary>
        /// The address of the account.
        /// </summary>
        public string Address => Account.PublicKey;

        /// <summary>
        /// The address book service.
        /// </summary>
        public AddressBookService AddressBookService { get; init; }

        /// <summary>
        /// Adds a new signer.
        /// </summary>
        public void AddSigner()
        {
            Signers.Add(new RequiredPublicKeyViewModel(false, AddressBookService));
            ValidateMinimumSigners(false);
        }

        /// <summary>
        /// Removes a signer.
        /// </summary>
        /// <param name="vm">The signer to remove.</param>
        public void RemoveSigner(RequiredPublicKeyViewModel vm)
        {
            Signers.Remove(vm);
            ValidateMinimumSigners(false);
        }

        /// <summary>
        /// The collection of signers.
        /// </summary>
        private ObservableCollection<RequiredPublicKeyViewModel> _signers;
        public ObservableCollection<RequiredPublicKeyViewModel> Signers
        {
            get => _signers;
            set => this.RaiseAndSetIfChanged(ref _signers, value);
        }

        /// <summary>
        /// The validated minimum signers value.
        /// </summary>
        public int MinimumSigners { get; private set; }

        /// <summary>
        /// Whether the amount is valid.
        /// </summary>
        private bool _isAmountValid;

        /// <summary>
        /// Whether all inputs are valid.
        /// </summary>
        public bool IsInputValid => _isAmountValid;

        /// <summary>
        /// Validate the minimum amount of signers.
        /// </summary>
        /// <exception cref="Avalonia.Data.DataValidationException">Exception thrown if the amount is invalid.</exception>
        private void ValidateMinimumSigners(bool throwException = false)
        {
            _isAmountValid = MinimumSigners != 0 && MinimumSigners < Signers.Count;
            this.RaisePropertyChanged(nameof(IsInputValid));
            if (!_isAmountValid && throwException)
                throw new Avalonia.Data.DataValidationException("Minimum required signers must be greater than zero and fewer than provided signers!");
        }

        /// <summary>
        /// The required signers input.
        /// </summary>
        private string _requiredSigners;
        public string RequiredSigners
        {
            get => _requiredSigners;
            set
            {
                if (SetRequiredSigners(value))
                {
                    this.RaiseAndSetIfChanged(ref _requiredSigners, value);
                }
                else
                {
                    MinimumSigners = 0;
                }
            }
        }

        /// <summary>
        /// Validates the required signers input string.
        /// </summary>
        /// <param name="value">The input value.</param>
        /// <returns>true if validated otherwise false.</returns>
        private bool SetRequiredSigners(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            var success = int.TryParse(value, out int requiredSigners);
            if (success)
            {
                MinimumSigners = requiredSigners;
                ValidateMinimumSigners(true);
            }
            return success;
        }

        /// <summary>
        /// The rent for UI display.
        /// </summary>
        public decimal Rent { get => SolHelper.ConvertToSol(NativeRent); }

        /// <summary>
        /// The rent in native lamports.
        /// </summary>
        public ulong NativeRent { get; init; }

        /// <summary>
        /// Whether the action is confirmed or not.
        /// </summary>
        public bool Confirmed { get; private set; }

        /// <summary>
        /// Cancels the action.
        /// </summary>
        /// <param name="window">The current window.</param>
        public void Cancel(Window window)
        {
            window.Close();
        }

        /// <summary>
        /// Confirms the action.
        /// </summary>
        /// <param name="window">The current window.</param>
        public void Confirm(Window window)
        {
            Confirmed = true;
            window.Close();
        }
    }
}
