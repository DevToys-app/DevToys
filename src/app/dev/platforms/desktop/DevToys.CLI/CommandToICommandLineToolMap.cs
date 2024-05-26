using System.CommandLine;
using System.Reflection;
using System.Resources;
using DevToys.Api;
using Microsoft.Extensions.Logging;

namespace DevToys.CLI;

internal sealed partial class CommandToICommandLineToolMap
{
    private readonly ILogger _logger;

    internal CommandToICommandLineToolMap(ICommandLineTool commandLineTool, CommandLineToolMetadata metadata)
    {
        Guard.IsNotNull(commandLineTool);
        Guard.IsNotNull(metadata);

        _logger = this.Log();

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

    private string GetCommandDescription(ResourceManager? resourceManager, CommandLineToolMetadata metadata)
    {
        if (resourceManager is null)
        {
            LogGetMetadataStringFailed(metadata.DescriptionResourceName, metadata.InternalComponentName);
            return $"[Unable to get the text for '{metadata.DescriptionResourceName}', " +
                $"likely because we couldn't find a proper '{nameof(IResourceAssemblyIdentifier)}' " +
                $"for the tool '{metadata.InternalComponentName}'.]";
        }

        try
        {
            string? commandDescription
                = !string.IsNullOrWhiteSpace(metadata.DescriptionResourceName)
                ? resourceManager.GetString(metadata.DescriptionResourceName) ?? $"[Unable to find '{metadata.DescriptionResourceName}' in '{metadata.ResourceManagerBaseName}']"
                : string.Empty;
            return commandDescription;
        }
        catch
        {
            LogGetMetadataStringFailed(metadata.DescriptionResourceName, metadata.InternalComponentName);
            return $"[Unable to find '{metadata.DescriptionResourceName}' in '{metadata.ResourceManagerBaseName}']";
        }
    }

    [LoggerMessage(0, LogLevel.Error, "Unable to get the string for '{metadataName}' for the tool '{toolName}'.")]
    partial void LogGetMetadataStringFailed(string metadataName, string toolName);
}
