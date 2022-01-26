using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anvil.Services.Store.Config
{
    /// <summary>
    /// The config of a store.
    /// </summary>
    public class StoreConfig
    {
        /// <summary>
        /// The name of the store.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// The directory of the store.
        /// </summary>
        public string Directory { get; init; }
    }
}
