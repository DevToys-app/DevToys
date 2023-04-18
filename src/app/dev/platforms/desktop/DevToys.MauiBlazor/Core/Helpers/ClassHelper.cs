using System.Text;

namespace DevToys.MauiBlazor.Core.Helpers;

public class ClassHelper
{
    private const char _delimiter = ' ';
    private bool _hasChanged = true;
    private string _classes = string.Empty;
    private readonly Action<ClassHelper> _classHelper;
    private readonly StringBuilder _builder = new();

    public ClassHelper(Action<ClassHelper> classHelper)
        => _classHelper = classHelper;

    public void Append(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        _builder.Append(value).Append(_delimiter);
    }

    public void Append(HashSet<string> values)
    {
        if (values.Count == 0)
        {
            return;
        }

        foreach (string value in values)
        {
            _builder.Append(value).Append(_delimiter);
        }
    }

    public void HasChanged()
        => _hasChanged = true;

    public string Classes
    {
        get
        {
            if (_hasChanged)
            {
                _builder.Clear();
                _classHelper(this);
                _classes = _builder.ToString().Trim();
                _hasChanged = false;
            }

            return _classes;
        }
    }
}
