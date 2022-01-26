using Anvil.Core.Modules;
using Anvil.Services.Store.Config;
using Microsoft.Extensions.Logging;

namespace Anvil.Services.Store
{
    public abstract class Store
    {
        protected ILogger _logger;
        protected IPersistenceDriver _persistenceDriver;
        protected StoreConfig _config;

        public Store(ILogger logger, StoreConfig config)
        {
            _logger = logger;
            _config = config;
            _persistenceDriver = new PersistenceDriver(logger, config.Directory, config.Name);
        }
    }
}
