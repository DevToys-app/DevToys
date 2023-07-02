namespace DevToys.Blazor.Components;

internal interface IFocusable
{
    ValueTask<bool> FocusAsync();
}
