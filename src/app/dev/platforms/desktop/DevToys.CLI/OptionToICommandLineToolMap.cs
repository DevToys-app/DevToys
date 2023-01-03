using System.CommandLine;
using System.Reflection;
using System.Resources;
using DevToys.Api;

namespace DevToys.CLI;

internal sealed class OptionToICommandLineToolMap
{
    private static readonly Type optionType = typeof(Option<>);

    private readonly ICommandLineTool _commandLineTool;
    private readonly PropertyInfo _property;

    internal OptionToICommandLineToolMap(
        ICommandLineTool commandLineTool,
        PropertyInfo property,
        CommandLineOptionAttribute commandLineOptionAttribute,
        ResourceManager? parentResourceManager)
    {
        Guard.IsNotNull(commandLineTool);
        Guard.IsNotNull(property);
        Guard.IsNotNull(commandLineOptionAttribute);

        _property = property;
        _commandLineTool = commandLineTool;

        // Get option description, if possible.
        string? optionDescription = GetOptionDescription(commandLineTool, commandLineOptionAttribute, parentResourceManager);

        // Normalize option name.
        string optionName = commandLineOptionAttribute.Name.Trim('-');
        optionName = "--" + optionName;

        OptionDefinition = CreateOption(property, optionName.ToLowerInvariant(), optionDescription);

        // Normalize option alias, if any.
        string? optionAlias = commandLineOptionAttribute.Alias?.Trim('-');
        if (!string.IsNullOrWhiteSpace(optionAlias))
        {
            optionAlias = "-" + optionAlias;
            OptionDefinition.AddAlias(optionAlias.ToLowerInvariant());
        }

        // Set whether the option is required.
        if (commandLineOptionAttribute.IsRequired)
        {
            OptionDefinition.IsRequired = true;
        }

        // Set option default value.
        object? defaultValue = property.GetValue(commandLineTool);
        if (defaultValue is not null)
        {
            OptionDefinition.SetDefaultValue(defaultValue);
        }
    }

    internal Option OptionDefinition { get; }

    internal void SetPropertyValue(object? value)
    {
        _property.SetValue(_commandLineTool, value);
    }

    private static string? GetOptionDescription(ICommandLineTool commandLineTool, CommandLineOptionAttribute commandLineOptionAttribute, ResourceManager? parentResourceManager)
    {
        string? optionDescription = null;
        if (!string.IsNullOrWhiteSpace(commandLineOptionAttribute.DescriptionResourceName))
        {
            ResourceManager? optionResourceManager;
            if (!string.IsNullOrWhiteSpace(commandLineOptionAttribute.ResourceManagerBaseName))
            {
                optionResourceManager = new ResourceManager(commandLineOptionAttribute.ResourceManagerBaseName, commandLineTool.GetType().Assembly);
            }
            else
            {
                optionResourceManager = parentResourceManager;
            }

            if (optionResourceManager is not null)
            {
                optionDescription = optionResourceManager.GetString(commandLineOptionAttribute.DescriptionResourceName);
            }
        }

        return optionDescription;
    }

    private static Option CreateOption(PropertyInfo property, string optionName, string? optionDescription)
    {
        // Creates a new instance of Option<T> where T is the type of the property we found.
        Type[] typeArgs = { property.PropertyType };
        Type makeme = optionType.MakeGenericType(typeArgs);
        object? optionObject = Activator.CreateInstance(makeme, optionName, optionDescription);
        Guard.IsNotNull(optionObject);
        return (Option)optionObject;
    }
}
