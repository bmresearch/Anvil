using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anvil.Services.Store.Models
{
    /// <summary>
    /// A mapping between a <see cref="NonceAccount"/>'s <see cref="PublicKey"/> and it's authority's.
    /// </summary>
    public class NonceAccountMapping
    {
        /// <summary>
        /// The <see cref="PublicKey"/> of the <see cref="NonceAccount"/>'s authority.
        /// </summary>
        public string Authority { get; set; }


        /// <summary>
        /// The <see cref="PublicKey"/> of the <see cref="NonceAccount"/>.
        /// </summary>
        public string Account { get; set; }
    }
}
