using System.Collections.Concurrent;
using DevToys.UI.Framework.Helpers;

namespace DevToys.UI.Framework.Controls;

/// <summary>
/// A pool of <see cref="IMonacoEditor"/>. You can use this class to get a new instance of the Monaco Editor, or a recycled one.
/// </summary>
internal sealed class MonacoEditorPool
{
    private readonly IMonacoEditorFactory _monacoEditorFactory = Parts.MefProvider.Import<IMonacoEditorFactory>();
    private readonly ConcurrentBag<IMonacoEditor> _instances = new ConcurrentBag<IMonacoEditor>();

    public void Recycle(IMonacoEditor item)
    {
        _instances.Add(item);
    }

    public IMonacoEditor Get()
    {
        if (_instances.TryTake(out IMonacoEditor? item) && item is not null)
        {
            return item;
        }
        else
        {
            return _monacoEditorFactory.CreateMonacoEditorInstance();
        }
    }
}
