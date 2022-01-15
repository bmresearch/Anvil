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
    /// Implements a store for <see cref="NonceAccountMapping"/>.
    /// </summary>
    public class NonceAccountMappingStore : INonceAccountMappingStore
    {
        private List<NonceAccountMapping> _nonceAccountMappings;

        /// <summary>
        /// 
        /// </summary>
        public NonceAccountMappingStore()
        {
            _nonceAccountMappings = new();
        }

        /// <inheritdoc cref="INonceAccountMappingStore.AddMapping(NonceAccountMapping)"></inheritdoc>
        public void AddMapping(NonceAccountMapping mapping)
        {
            _nonceAccountMappings.Add(mapping);
        }

        /// <inheritdoc cref="INonceAccountMappingStore.GetMapping(PublicKey)"></inheritdoc>
        public NonceAccountMapping GetMapping(PublicKey authority)
        {
            return _nonceAccountMappings.FirstOrDefault(x => x.Authority.Key == authority.Key);
        }

        /// <inheritdoc cref="INonceAccountMappingStore.GetMappings(PublicKey)"></inheritdoc>
        public List<NonceAccountMapping> GetMappings(PublicKey authority)
        {
            return _nonceAccountMappings.Where(x => x.Authority.Key == authority.Key).ToList();
        }

        /// <inheritdoc cref="INonceAccountMappingStore.NonceAccountMappings"></inheritdoc>
        public List<NonceAccountMapping> NonceAccountMappings 
        { 
            get => _nonceAccountMappings;
        }
    }
}
