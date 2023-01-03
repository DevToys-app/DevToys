using System.CommandLine;
using System.CommandLine.Invocation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using DevToys.Api;
using DevToys.Core.Mef;
using Microsoft.Extensions.Logging;

namespace DevToys.CLI;

internal class Program
{
    private static readonly ILoggerFactory loggerFactory;

    static Program()
    {
        loggerFactory
            = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("DevToys", LogLevel.Debug)
                    .AddDebug();
            });
    }

    private static void Main(string[] args)
    {
        // Enable support for multiple encodings, especially in .NET Core
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        MainAsync(args).GetAwaiter().GetResult();

        loggerFactory.Dispose();
    }

    private static async Task MainAsync(string[] args)
    {
        var rootCommand = new RootCommand("DevToys");

        // Initialize MEF.
        var mefComposer
            = new MefComposer(new[] { typeof(Program).Assembly, Assembly.Load("DevToys.Tools") });

        // Get all the command line tools.
        IEnumerable<Lazy<ICommandLineTool, CommandLineToolMetadata>> commandLineTools
            = mefComposer.Provider.ImportMany<ICommandLineTool, CommandLineToolMetadata>();

        var commands = new List<CommandToICommandLineToolMap>();

        // For each tool, try to create a System.CommandLine.Command and register it.
        foreach (Lazy<ICommandLineTool, CommandLineToolMetadata> commandLineTool in commandLineTools)
        {
            CommandToICommandLineToolMap? command = CreateCommandForTool(commandLineTool);

            if (command is not null)
            {
                commands.Add(command);
                rootCommand.AddCommand(command.CommandDefinition);
            }
        }

        // Parse the command prompt arguments and run the appropriate command, if possible.
        int exitCode = await rootCommand.InvokeAsync(args);
        Environment.ExitCode = exitCode;
    }

    private static CommandToICommandLineToolMap? CreateCommandForTool(Lazy<ICommandLineTool, CommandLineToolMetadata> commandLineTool)
    {
        // Make sure the tool is supported by the current OS. If no platform is precised by the tool,
        // it means it's supported by every OS.
        if (!IsOsSupported(commandLineTool.Metadata))
        {
            Debug.WriteLine($"Ignoring '{commandLineTool.Metadata.InternalComponentName}' tool as it isn't supported by the current OS.");
            return null;
        }

        // Create a System.CommandLine.Command based on the information provided by the ICommandLineTool.
        var command = new CommandToICommandLineToolMap(commandLineTool.Value, commandLineTool.Metadata);

        // Create the command handler
        command.CommandDefinition.SetHandler(async (InvocationContext context) =>
        {
            // For each option, try to get its value from the command prompt arguments
            // and set the values to the command line tool instance.
            for (int i = 0; i < command.Options.Count; i++)
            {
                OptionToICommandLineToolMap options = command.Options[i];
                object? optionValue = context.ParseResult.GetValueForOption(options.OptionDefinition);
                options.SetPropertyValue(optionValue);
            }

            // Invoke the command line tool.
            ILogger logger = loggerFactory.CreateLogger(commandLineTool.Value.GetType());
            int exitCode = await commandLineTool.Value.InvokeAsync(logger, context.GetCancellationToken());
            context.ExitCode = exitCode;
        });

        return command;
    }

    private static bool IsOsSupported(CommandLineToolMetadata metadata)
    {
        if (metadata.TargetPlatforms.Count > 0)
        {
            Platform currentPlatform;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                currentPlatform = Platform.MacOS;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                currentPlatform = Platform.Windows;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
            {
                currentPlatform = Platform.Linux;
            }
            else
            {
                throw new NotSupportedException();
            }

            if (!metadata.TargetPlatforms.Contains(currentPlatform))
            {
                return false;
            }
        }

        return true;
    }
}
