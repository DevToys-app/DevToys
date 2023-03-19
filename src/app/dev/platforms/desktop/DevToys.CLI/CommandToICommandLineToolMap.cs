using System.CommandLine;
using System.Reflection;
using System.Resources;
using DevToys.Api;

namespace DevToys.CLI;

internal sealed class CommandToICommandLineToolMap
{
    internal CommandToICommandLineToolMap(ICommandLineTool commandLineTool, CommandLineToolMetadata metadata)
    {
        Guard.IsNotNull(commandLineTool);
        Guard.IsNotNull(metadata);

        // Get the resource manager, if possible.
        ResourceManager? resourceManager = GetResourceManager(commandLineTool, metadata);

        // Get command description, if possible.
        string? commandDescription = GetCommandDescription(resourceManager, metadata);

        // Create the command.
        var command = new Command(metadata.Name.ToLowerInvariant(), commandDescription);

        // Set the alias, if any.
        if (!string.IsNullOrWhiteSpace(metadata.Alias))
        {
            command.AddAlias(metadata.Alias.ToLowerInvariant());
        }

        // Creates the command's options.
        var options = new List<OptionToICommandLineToolMap>();
        PropertyInfo[] properties = commandLineTool.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        for (int i = 0; i < properties.Length; i++)
        {
            PropertyInfo property = properties[i];
            CommandLineOptionAttribute? commandLineOptionAttribute = property.GetCustomAttribute<CommandLineOptionAttribute>();
            if (commandLineOptionAttribute is not null)
            {
                var option = new OptionToICommandLineToolMap(commandLineTool, property, commandLineOptionAttribute, resourceManager);
                options.Add(option);
                command.AddOption(option.OptionDefinition);
            }
        }

        CommandDefinition = command;
        Options = options;
    }

    internal Command CommandDefinition { get; }

    internal IReadOnlyList<OptionToICommandLineToolMap> Options { get; }

    private static ResourceManager? GetResourceManager(ICommandLineTool commandLineTool, CommandLineToolMetadata metadata)
    {
        // Load resource manager, if needed.
        ResourceManager? resourceManager
            = !string.IsNullOrWhiteSpace(metadata.ResourceManagerBaseName)
            ? new ResourceManager(metadata.ResourceManagerBaseName, commandLineTool.GetType().Assembly)
            : null;

        return resourceManager;
    }

    private static string GetCommandDescription(ResourceManager? resourceManager, CommandLineToolMetadata metadata)
    {
        string? commandDescription
            = resourceManager is not null && !string.IsNullOrWhiteSpace(metadata.DescriptionResourceName)
            ? resourceManager.GetString(metadata.DescriptionResourceName)
            : string.Empty;

        return commandDescription;
    }
}
