using Anvil.Core.ViewModels;
using ReactiveUI;
using Solnet.Programs.Models;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anvil.ViewModels.Crafter
{
    public class MultiSignatureAccountViewModel : AccountViewModel
    {

        public MultiSignatureAccountViewModel(ObservableCollection<TokenWalletBalanceWrapper> assets, MultiSignatureAccount multiSig) 
            : base(assets)
        {
            SelectedSigners = new();
            SelectedSigners.CollectionChanged += SelectedSigners_CollectionChanged;
            MinimumSigners = multiSig.MinimumSigners;
            Signers = new(multiSig.Signers);
        }

        private void SelectedSigners_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.RaisePropertyChanged("Validated");
        }

        private int _mininumSigners;
        public int MinimumSigners
        {
            get => _mininumSigners;
            set => this.RaiseAndSetIfChanged(ref _mininumSigners, value);
        }

        private ObservableCollection<PublicKey> _signers;
        public ObservableCollection<PublicKey> Signers
        {
            get => _signers;
            set => this.RaiseAndSetIfChanged(ref _signers, value);
        }

        private ObservableCollection<PublicKey> _selectedSigners;
        public ObservableCollection<PublicKey> SelectedSigners
        {
            get => _selectedSigners;
            set => this.RaiseAndSetIfChanged(ref _selectedSigners, value);
        }

        public bool Validated
        {
            get
            {
                return base.InputValidated && SelectedSigners.Count == MinimumSigners;
            }
        }
    }
}
