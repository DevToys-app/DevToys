namespace DevToys.Api;

/// <summary>
/// Defines the internal name of this component. This name can be used to explicitly request this component to be invoked.
/// </summary>
[MetadataAttribute]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class AuthorAttribute : Attribute
{
    public string Author { get; }

    public AuthorAttribute(string author)
    {
        Guard.IsNotEmpty(author);
        Author = author;
    }
}
