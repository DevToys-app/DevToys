namespace DevToys.Blazor.Components.UIElements;

internal static class UIMultilineTextInputHelper
{
    private static readonly Dictionary<IUIMultiLineTextInput, UIMultiLineTextInputPresenter> allMultiLineTextInputToPresenterMap = new();
    private static readonly Dictionary<string, UIMultiLineTextInputPresenter> allTextModelNameToPresenterMap = new();

    internal static void RegisterMultiLineTextInputPresenter(UIMultiLineTextInputPresenter presenter)
    {
        lock (allMultiLineTextInputToPresenterMap)
        {
            Guard.IsNotNull(presenter);

            allMultiLineTextInputToPresenterMap[presenter.UIMultiLineTextInput] = presenter;
            allTextModelNameToPresenterMap[presenter.TextModelName] = presenter;
        }
    }

    internal static void UnregisterMultiLineTextInputPresenter(UIMultiLineTextInputPresenter presenter)
    {
        lock (allMultiLineTextInputToPresenterMap)
        {
            Guard.IsNotNull(presenter);

            allMultiLineTextInputToPresenterMap.Remove(presenter.UIMultiLineTextInput);
            allTextModelNameToPresenterMap.Remove(presenter.TextModelName);
        }
    }

    internal static UIMultiLineTextInputPresenter? GetPresenter(IUIMultiLineTextInput multiLineTextInput)
    {
        lock (allMultiLineTextInputToPresenterMap)
        {
            Guard.IsNotNull(multiLineTextInput);

            if (allMultiLineTextInputToPresenterMap.TryGetValue(multiLineTextInput, out UIMultiLineTextInputPresenter? presenter))
            {
                return presenter;
            }

            return null;
        }
    }

    internal static UIMultiLineTextInputPresenter? GetPresenter(string textModelName)
    {
        lock (allMultiLineTextInputToPresenterMap)
        {
            Guard.IsNotNullOrWhiteSpace(textModelName);

            if (allTextModelNameToPresenterMap.TryGetValue(textModelName, out UIMultiLineTextInputPresenter? presenter))
            {
                return presenter;
            }

            return null;
        }
    }
}
