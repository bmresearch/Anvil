using Anvil.Core.Modules;
using Anvil.Services.Store.Config;
using Microsoft.Extensions.Logging;

namespace Anvil.Services.Store.Abstract
{
    /// <summary>
    /// Represents an abstract store.
    /// </summary>
    public abstract class Store
    {
        /// <summary>
        /// The logger.
        /// </summary>
        protected ILogger _logger;

        /// <summary>
        /// The persistence driver.
        /// </summary>
        protected IPersistenceDriver _persistenceDriver;

        /// <summary>
        /// The store config.
        /// </summary>
        protected StoreConfig _config;

        /// <summary>
        /// Initialize the <see cref="Store"/> with the given <see cref="ILogger"/> and <see cref="StoreConfig"/>.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="config">The store config.</param>
        public Store(ILogger logger, StoreConfig config)
        {
            _logger = logger;
            _config = config;
            _persistenceDriver = new PersistenceDriver(logger, config.Directory, config.Name);
        }
    }
}
