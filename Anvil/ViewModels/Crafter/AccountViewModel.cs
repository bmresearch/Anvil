using Anvil.Core.ViewModels;
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

        private string _amount = "0";
        public string Amount
        {
            get => _amount;
            set
            {
                var success = float.TryParse(value, out float converted);
                if (success)
                {
                    this.RaiseAndSetIfChanged(ref _amount, value);
                    AssetAmount = converted;
                }
            }
        }

        private float _assetAmount = 0f;
        public float AssetAmount
        {
            get => _assetAmount;
            set
            {
                this.RaiseAndSetIfChanged(ref _assetAmount, value);
                this.RaisePropertyChanged("InsufficientBalance");
                this.RaisePropertyChanged("InputValidated");
            }
        }

        public bool InsufficientBalance
        {
            get
            {
                return (decimal)AssetAmount > SelectedAsset.Balance;
            }
        }

        public bool InputValidated
        {
            get
            {
                return AssetAmount > 0f && !InsufficientBalance;
            }
        }
    }
}
