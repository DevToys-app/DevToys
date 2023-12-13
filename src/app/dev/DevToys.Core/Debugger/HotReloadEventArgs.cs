namespace DevToys.Core.Debugger;

public sealed class HotReloadEventArgs : EventArgs
{
    internal HotReloadEventArgs(Type[]? types)
    {
        Types = types;
    }

    public Type[]? Types { get; }
}
