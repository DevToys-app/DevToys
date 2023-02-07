namespace DevToys.MauiBlazor.Components;

public class GridLayoutException : Exception
{
    public GridLayoutException(string data)
        : base($"'{data} is not a valid unit. Accepted values are *, auto or a number'")
    {
    }
}
