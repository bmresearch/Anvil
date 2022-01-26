using ReactiveUI;

namespace Anvil.Services.Store.State
{
    public abstract class State<T> : ReactiveObject
    {
        public T Value { get; set; }
    }
}
