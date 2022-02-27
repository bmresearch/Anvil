using Anvil.Core.ViewModels;
using Anvil.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anvil.ViewModels.Crafter
{
    public class AccountViewModel : ViewModelBase
    {
        public AccountViewModel(ObservableCollection<TokenWalletBalanceWrapper> assets)
        {
            Assets = assets;
            SelectedAsset = assets.First();
        }

        private ObservableCollection<TokenWalletBalanceWrapper> _assets;
        public ObservableCollection<TokenWalletBalanceWrapper> Assets
        {
            get => _assets;
            set => this.RaiseAndSetIfChanged(ref _assets, value);
        }

        private TokenWalletBalanceWrapper _selectedAsset;
        public TokenWalletBalanceWrapper SelectedAsset
        {
            get => _selectedAsset;
            set => this.RaiseAndSetIfChanged(ref _selectedAsset, value);
        }

        private decimal _amount = 0m;
        public decimal Amount
        {
            get => _amount;
            set
            {
                this.RaiseAndSetIfChanged(ref _amount, value);
                this.RaisePropertyChanged(nameof(InsufficientBalance));
                this.RaisePropertyChanged(nameof(InputValidated));
            }
        }

        public bool InsufficientBalance
        {
            get
            {
                return Amount > SelectedAsset.Balance;
            }
        }

        public bool InputValidated
        {
            get
            {
                return Amount > 0m && !InsufficientBalance;
            }
        }
    }
}
