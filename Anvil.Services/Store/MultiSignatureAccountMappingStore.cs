using Anvil.Services.Store.Models;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anvil.Services.Store
{
    /// <summary>
    /// Implements a store for <see cref="MultiSignatureMapping"/>.
    /// </summary>
    public class MultiSignatureAccountMappingStore : IMultiSignatureAccountMappingStore
    {
        private List<MultiSignatureAccountMapping> _multiSignatureMappings;

        /// <summary>
        /// 
        /// </summary>
        public MultiSignatureAccountMappingStore()
        {
            _multiSignatureMappings = new();
        }

        /// <inheritdoc cref="IMultiSignatureAccountMappingStore.AddMapping(MultiSignatureAccountMapping)"
        public void AddMapping(MultiSignatureAccountMapping mapping)
        {
            _multiSignatureMappings.Add(mapping);
        }

        /// <inheritdoc cref="IMultiSignatureAccountMappingStore.GetMapping(PublicKey)"
        public MultiSignatureAccountMapping GetMapping(PublicKey account)
        {
            return _multiSignatureMappings.FirstOrDefault(x => x.MultiSignature.Key == account.Key);
        }

        /// <inheritdoc cref="IMultiSignatureAccountMappingStore.MultiSignatureAccountMappings"
        public List<MultiSignatureAccountMapping> MultiSignatureAccountMappings 
        { 
            get => _multiSignatureMappings; 
        }
    }
}
