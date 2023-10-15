using System.CommandLine;
using System.CommandLine.Parsing;
using System.Reflection;
using System.Resources;
using DevToys.Api;
using DevToys.CLI.Core;

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

        OptionDefinition = CreateOption(commandLineOptionAttribute, property, optionName.ToLowerInvariant(), optionDescription);

        // Normalize option alias, if any.
        string? optionAlias = commandLineOptionAttribute.Alias?.Trim('-');
        if (!string.IsNullOrWhiteSpace(optionAlias))
        {
            optionAlias = "-" + optionAlias;
            OptionDefinition.AddAlias(optionAlias.ToLowerInvariant());
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

    private static Option CreateOption(
        CommandLineOptionAttribute commandLineOptionAttribute,
        PropertyInfo property,
        string optionName,
        string? optionDescription)
    {
        // Creates a new instance of Option<T> where T is the type of the property we found.
        Type nonNullablePropertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        bool isArray = nonNullablePropertyType.IsEnumerable();
        Type arrayValueType = isArray ? nonNullablePropertyType.GetElementTypeIfEnumerable()! : nonNullablePropertyType;
        nonNullablePropertyType = Nullable.GetUnderlyingType(arrayValueType) ?? arrayValueType;

        bool isAnyType
            = nonNullablePropertyType.GUID == AnyTypeIdentifiers.AnyTypeT2Guid
            || nonNullablePropertyType.GUID == AnyTypeIdentifiers.AnyTypeT3Guid
            || nonNullablePropertyType.GUID == AnyTypeIdentifiers.AnyTypeT4Guid;

        Option option;
        if (isAnyType)
        {
            // The option is of type AnyType<T1, T2>, or AnyType<T1, T2, T3>, or AnyType<T1, T2, T3, T4>.
            // We have a special code path to try to parse this type.
            option
                = CreateAnyTypeOption(
                    commandLineOptionAttribute,
                    optionName,
                    optionDescription,
                    nonNullablePropertyType,
                    isArray);
        }
        else
        {
            // Instantiate a new Option<T> object.
            Type[] typeArgs = { property.PropertyType };
            Type genericOptionType = optionType.MakeGenericType(typeArgs);
            object? optionObject
                = Activator.CreateInstance(
                    genericOptionType,
                    optionName,
                    optionDescription);
            Guard.IsNotNull(optionObject);
            option = (Option)optionObject;
        }

        // Set whether the option is required.
        if (commandLineOptionAttribute.IsRequired)
        {
            option.IsRequired = true;
        }

        return option;
    }

    private static Option CreateAnyTypeOption(
        CommandLineOptionAttribute commandLineOptionAttribute,
        string optionName,
        string? optionDescription,
        Type anyTypeDefinition,
        bool isArray)
    {
        var option = new AnyTypeOption(optionName, optionDescription, anyTypeDefinition, ParseArgument);

        // Set the option arity.
        if (commandLineOptionAttribute.IsRequired)
        {
            if (isArray)
            {
                option.Arity = ArgumentArity.OneOrMore;
                option.AllowMultipleArgumentsPerToken = true;
            }
            else
            {
                option.Arity = ArgumentArity.ExactlyOne;
            }
        }
        else
        {
            if (isArray)
            {
                option.Arity = ArgumentArity.ZeroOrMore;
                option.AllowMultipleArgumentsPerToken = true;
            }
            else
            {
                option.Arity = ArgumentArity.ZeroOrOne;
            }
        }

        return option;
    }

    private static object? ParseArgument(ArgumentResult result)
    {
        if (result.Parent is OptionResult optionResult && optionResult.Option is AnyTypeOption anyTypeOption)
        {
            // Create an array of AnyType<T1, T2>, or AnyType<T1, T2, T3>, or AnyType<T1, T2, T3, T4>.
            var arguments = Array.CreateInstance(anyTypeOption.AnyTypeDefinition, result.Tokens.Count);

            // Parse each argument.
            for (int i = 0; i < result.Tokens.Count; i++)
            {
                object? parsedArgument = AnyTypeParser.ParseAnyType(anyTypeOption, result.Tokens[i].Value);

                if (parsedArgument is null)
                {
                    result.ErrorMessage = result.LocalizationResources.ArgumentConversionCannotParse(result.ToString(), anyTypeOption.AnyTypeDefinition);
                    return null;
                }

                arguments.SetValue(parsedArgument, i);
            }

            // Return the proper value based on the option arity.
            if (anyTypeOption.Arity.Equals(ArgumentArity.ZeroOrOne))
            {
                if (arguments.Length == 0)
                {
                    return default!;
                }
                else if (arguments.Length == 1)
                {
                    return arguments.GetValue(0)!;
                }
            }
            else if (anyTypeOption.Arity.Equals(ArgumentArity.ExactlyOne))
            {
                if (arguments.Length == 0)
                {
                    result.ErrorMessage = result.LocalizationResources.ExpectsOneArgument(result);
                    return default!;
                }
                else if (arguments.Length == 1)
                {
                    return arguments.GetValue(0)!;
                }
            }

            return arguments;
        }

        result.ErrorMessage = result.LocalizationResources.ArgumentConversionCannotParse(result.ToString(), typeof(AnyTypeOption));
        return null;
    }
}
