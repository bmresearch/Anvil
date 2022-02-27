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
            var signer = new RequiredPublicKeyViewModel(false, AddressBookService);
            signer.WhenAnyValue(x => x.PublicKey)
                .Subscribe(x =>
                {
                    CheckDuplicateSigners();
                });
            Signers.Add(signer);
            ValidateMinimumSigners();
        }

        private void ValidateSigners()
        {
            MissingSigners = false;
            foreach (var signer in Signers)
            {
                if (signer.PublicKey == null) MissingSigners = true;
            }
        }

        /// <summary>
        /// Checks for duplicate keys in the signers.
        /// </summary>
        public void CheckDuplicateSigners()
        {
            ValidateSigners();
            this.RaisePropertyChanged(nameof(IsInputValid));
            if (MissingSigners) return;
            var dup = new List<bool>();
            for (int i = 0; i < Signers.Count; i++)
            {
                var signer = Signers[i];
                if (signer.PublicKey != null)
                {
                    for (int j = i + 1; j < Signers.Count; j++)
                    {
                        if (Signers[j].PublicKey == null) continue;
                        if (signer.PublicKey.Equals(Signers[j].PublicKey)) dup.Add(true);
                        continue;
                    }
                    if (dup.Count == i + 1) continue;
                    dup.Add(false);
                }
                else
                {
                    dup.Add(false);
                    continue;
                }
            }
            if (dup.Contains(true))
            {
                DuplicateSigners = true;
            }
            else
            {
                DuplicateSigners = false;
            }
            this.RaisePropertyChanged(nameof(IsInputValid));
        }

        /// <summary>
        /// Removes a signer.
        /// </summary>
        /// <param name="vm">The signer to remove.</param>
        public void RemoveSigner(RequiredPublicKeyViewModel vm)
        {
            Signers.Remove(vm);
            ValidateMinimumSigners();
            CheckDuplicateSigners();
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
        public bool IsInputValid => !InvalidAlias && !MissingSigners && !InvalidSigners && !DuplicateSigners;

        /// <summary>
        /// Validate the minimum amount of signers.
        /// </summary>
        /// <exception cref="Avalonia.Data.DataValidationException">Exception thrown if the amount is invalid.</exception>
        private void ValidateMinimumSigners()
        {
            _isAmountValid = MinimumSigners != 0 && MinimumSigners < Signers.Count;
            if (!_isAmountValid)
            {
                InvalidSigners = true;
            }
            else
            {
                InvalidSigners = false;
            }
            this.RaisePropertyChanged(nameof(IsInputValid));
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
        /// The required signers input.
        /// </summary>
        private string _alias;
        public string Alias
        {
            get => _alias;
            set
            {
                this.RaiseAndSetIfChanged(ref _alias, value);
                ValidateAlias();
            }
        }

        private bool _invalidAlias;
        public bool InvalidAlias
        {
            get => _invalidAlias;
            set => this.RaiseAndSetIfChanged(ref _invalidAlias, value);
        }

        private void ValidateAlias()
        {
            if (string.IsNullOrWhiteSpace(Alias))
            {
                InvalidAlias = true;
            }
            else
            {
                InvalidAlias = false;
            }
            this.RaisePropertyChanged(nameof(IsInputValid));
        }

        /// <summary>
        /// Validates the required signers input string.
        /// </summary>
        /// <param name="value">The input value.</param>
        /// <returns>true if validated otherwise false.</returns>
        private bool SetRequiredSigners(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            var success = int.TryParse(value, out int requiredSigners);
            if (success)
            {
                MinimumSigners = requiredSigners;
                ValidateMinimumSigners();
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
        /// Whether the number of signers is valid.
        /// </summary>
        private bool _invalidSigners;
        public bool InvalidSigners
        {
            get => _invalidSigners;
            set => this.RaiseAndSetIfChanged(ref _invalidSigners, value);
        }

        /// <summary>
        /// Whether the number of signers is valid.
        /// </summary>
        private bool _duplicateSigners;
        public bool DuplicateSigners
        {
            get => _duplicateSigners;
            set => this.RaiseAndSetIfChanged(ref _duplicateSigners, value);
        }

        /// <summary>
        /// Whether the number of signers is valid.
        /// </summary>
        private bool _missingSigners;
        public bool MissingSigners
        {
            get => _missingSigners;
            set => this.RaiseAndSetIfChanged(ref _missingSigners, value);
        }

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
