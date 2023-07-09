namespace DevToys.Blazor.Core.Helpers;

public struct CssBuilder
{
    private string? _stringBuffer;
    private string _prefix;

    /// <summary>
    /// Sets the prefix value to be appended to all classes added following the this statement. When SetPrefix is called it will overwrite any previous prefix set for this instance. Prefixes are not applied when using AddValue.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>CssBuilder</returns>
    public CssBuilder SetPrefix(string value)
    {
        _prefix = value;
        return this;
    }

    /// <summary>
    /// Creates a CssBuilder used to define conditional CSS classes used in a component.
    /// Call Build() to return the completed CSS Classes as a string. 
    /// </summary>
    /// <param name="value"></param>
    public static CssBuilder Default(string value) => new(value);

    /// <summary>
    /// Creates an Empty CssBuilder used to define conditional CSS classes used in a component.
    /// Call Build() to return the completed CSS Classes as a string. 
    /// </summary>
    public static CssBuilder Empty() => new();

    /// <summary>
    /// Creates a CssBuilder used to define conditional CSS classes used in a component.
    /// Call Build() to return the completed CSS Classes as a string. 
    /// </summary>
    /// <param name="value"></param>
    public CssBuilder(string value)
    {
        _stringBuffer = value;
        _prefix = string.Empty;
    }

    /// <summary>
    /// Adds a raw string to the builder that will be concatenated with the next class or value added to the builder.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>CssBuilder</returns>
    public CssBuilder AddValue(string value)
    {
        _stringBuffer += value;
        return this;
    }

    /// <summary>
    /// Adds a CSS Class to the builder with space separator.
    /// </summary>
    /// <param name="value">CSS Class to add</param>
    /// <returns>CssBuilder</returns>
    public CssBuilder AddClass(string value) => AddValue(" " + _prefix + value);

    /// <summary>
    /// Adds a conditional CSS Class to the builder with space separator.
    /// </summary>
    /// <param name="value">CSS Class to conditionally add.</param>
    /// <param name="when">Condition in which the CSS Class is added.</param>
    /// <returns>CssBuilder</returns>
    public CssBuilder AddClass(string value, bool when = true) => when ? this.AddClass(value) : this;

    /// <summary>
    /// Adds a conditional CSS Class to the builder with space separator.
    /// </summary>
    /// <param name="value">CSS Class to conditionally add.</param>
    /// <param name="when">Condition in which the CSS Class is added.</param>
    /// <returns>CssBuilder</returns>
    public CssBuilder AddClass(string value, Func<bool> when) => this.AddClass(value, when());

    /// <summary>
    /// Adds a conditional CSS Class to the builder with space separator.
    /// </summary>
    /// <param name="value">Function that returns a CSS Class to conditionally add.</param>
    /// <param name="when">Condition in which the CSS Class is added.</param>
    /// <returns>CssBuilder</returns>
    public CssBuilder AddClass(Func<string> value, bool when = true) => when ? this.AddClass(value()) : this;

    /// <summary>
    /// Adds a conditional CSS Class to the builder with space separator.
    /// </summary>
    /// <param name="value">Function that returns a CSS Class to conditionally add.</param>
    /// <param name="when">Condition in which the CSS Class is added.</param>
    /// <returns>CssBuilder</returns>
    public CssBuilder AddClass(Func<string> value, Func<bool> when) => this.AddClass(value, when());

    /// <summary>
    /// Adds a conditional nested CssBuilder to the builder with space separator.
    /// </summary>
    /// <param name="value">CSS Class to conditionally add.</param>
    /// <param name="when">Condition in which the CSS Class is added.</param>
    /// <returns>CssBuilder</returns>
    public CssBuilder AddClass(CssBuilder builder, bool when = true) => when ? this.AddClass(builder.Build()) : this;

    /// <summary>
    /// Adds a conditional CSS Class to the builder with space separator.
    /// </summary>
    /// <param name="value">CSS Class to conditionally add.</param>
    /// <param name="when">Condition in which the CSS Class is added.</param>
    /// <returns>CssBuilder</returns>
    public CssBuilder AddClass(CssBuilder builder, Func<bool> when) => this.AddClass(builder, when());

    /// <summary>
    /// Adds a conditional CSS Class when it exists in a dictionary to the builder with space separator.
    /// Null safe operation.
    /// </summary>
    /// <param name="additionalAttributes">Additional Attribute splat parameters</param>
    /// <returns>CssBuilder</returns>
    public CssBuilder AddClassFromAttributes(IDictionary<string, object> additionalAttributes) =>
        additionalAttributes == null ? this :
        additionalAttributes.TryGetValue("class", out object? c) && c != null ? AddClass(c.ToString() ?? string.Empty) : this;

    /// <summary>
    /// Finalize the completed CSS Classes as a string.
    /// </summary>
    /// <returns>string</returns>
    public readonly string Build()
    {
        // String buffer finalization code
        return _stringBuffer != null ? _stringBuffer.Trim() : string.Empty;
    }

    // ToString should only and always call Build to finalize the rendered string.
    public override string ToString() => Build();

}
