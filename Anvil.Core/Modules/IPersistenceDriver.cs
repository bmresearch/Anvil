using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anvil.Core.Modules
{
    public interface IPersistenceDriver
    {
        T LoadState<T>();

        void SaveState<T>(T state);

        void MigrateState<T>(string newBaseDirectory);

        string FileName { get; }

        string BaseDirectory { get; }

        string DefaultBaseDirectory { get; }
    }
}
