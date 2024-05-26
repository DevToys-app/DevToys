using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text;
using DevToys.Api;
using DevToys.CLI.Core;
using DevToys.CLI.Core.FileStorage;
using DevToys.Core;
using DevToys.Core.Logging;
using DevToys.Core.Mef;
using Microsoft.Extensions.Logging;

namespace DevToys.CLI;

internal partial class Program
{
    private static readonly ILoggerFactory loggerFactory;
    private static readonly ILogger logger;

    static Program()
    {
        DateTime startTime = DateTime.Now;
        loggerFactory
            = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFile(new FileStorage()); // To save logs on local hard drive.

                // Exclude logs below this level
#if DEBUG
                builder.AddDebug();
                builder.SetMinimumLevel(LogLevel.Trace);
#else
                builder.SetMinimumLevel(LogLevel.Information);
#endif
            });

        LoggingExtensions.LoggerFactory = loggerFactory;

        logger = new Logger<Program>(loggerFactory);
        LogLoggerInitialization(logger, (DateTime.Now - startTime).TotalMilliseconds);
        LogAppStarting(logger);
    }

    private static void Main(string[] args)
    {
        // Enable support for multiple encodings, especially in .NET Core
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        MainAsync(args).GetAwaiter().GetResult();

        LogAppShuttingDown(logger);
        loggerFactory.Dispose();

        if (Debugger.IsAttached)
        {
            Console.ReadKey();
        }
    }

    private static async Task MainAsync(string[] args)
    {
        try
        {
            FileHelper.ClearTempFiles(Constants.AppTempFolder);

            var rootCommand = new RootCommand("DevToys");

            // Initialize MEF.
            var mefComposer
                = new MefComposer(
                    assemblies: new[]
                    {
                        typeof(Program).Assembly
                    },
                    pluginFolders: new[]
                    {
                        Path.Combine(AppContext.BaseDirectory, "Plugins")
                    });

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

            // Read the pipeline input, if any.
            if (Console.IsInputRedirected)
            {
                string pipelineInput = await Console.In.ReadToEndAsync(CancellationToken.None);

                // Remove the last new line, if any.
                if (pipelineInput.EndsWith(Environment.NewLine))
                {
                    pipelineInput = pipelineInput.Substring(0, pipelineInput.Length - Environment.NewLine.Length);
                }

                // Remove the BOM character, if any.
                pipelineInput = pipelineInput.Trim(new char[] { '\uFEFF', '\u200B' });

                args = args.Append(pipelineInput).ToArray();
            }

            // Parse the command prompt arguments and run the appropriate command, if possible.
            int exitCode = await rootCommand.InvokeAsync(args);
            Environment.ExitCode = exitCode;
        }
        catch (Exception ex)
        {
            LogUnhandledException(logger, ex);
        }
        finally
        {
            FileHelper.ClearTempFiles(Constants.AppTempFolder);
        }
    }

    private static CommandToICommandLineToolMap? CreateCommandForTool(Lazy<ICommandLineTool, CommandLineToolMetadata> commandLineTool)
    {
        // Make sure the tool is supported by the current OS. If no platform is precise by the tool,
        // it means it's supported by every OS.
        if (!OSHelper.IsOsSupported(commandLineTool.Metadata.TargetPlatforms))
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
            ILogger logger = loggerFactory.CreateLogger(commandLineTool.Value.GetType().FullName!);
            LogInvokingCommand(logger, command.CommandDefinition.Name);

            int exitCode = await commandLineTool.Value.InvokeAsync(logger, context.GetCancellationToken());
            LogExitCode(logger, exitCode);

            context.ExitCode = exitCode;
        });

        return command;
    }

    [LoggerMessage(0, LogLevel.Information, "Logger initialized in {duration} ms")]
    static partial void LogLoggerInitialization(ILogger logger, double duration);

    [LoggerMessage(1, LogLevel.Information, "App is starting...")]
    static partial void LogAppStarting(ILogger logger);

    [LoggerMessage(2, LogLevel.Information, "App is shutting down...")]
    static partial void LogAppShuttingDown(ILogger logger);

    [LoggerMessage(3, LogLevel.Information, "Invoking '{CommandName}' command...")]
    static partial void LogInvokingCommand(ILogger logger, string commandName);

    [LoggerMessage(4, LogLevel.Information, "Exit code: {ExitCode}")]
    static partial void LogExitCode(ILogger logger, int exitCode);

    [LoggerMessage(5, LogLevel.Critical, "Unhandled exception !!!    (╯°□°）╯︵ ┻━┻")]
    static partial void LogUnhandledException(ILogger logger, Exception exception);
}
