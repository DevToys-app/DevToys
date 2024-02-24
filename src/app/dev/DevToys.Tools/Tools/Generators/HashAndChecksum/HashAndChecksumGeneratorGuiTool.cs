using System.Security.Authentication;
using System.Security.Cryptography;
using DevToys.Tools.Helpers;
using DevToys.Tools.Models;
using Microsoft.Extensions.Logging;
using OneOf;

namespace DevToys.Tools.Tools.Generators.HashAndChecksum;

[Export(typeof(IGuiTool))]
[Name("HashAndChecksumGenerator")]
[ToolDisplayInformation(
    IconFontName = "DevToys-Tools-Icons",
    IconGlyph = '\u0125',
    GroupName = PredefinedCommonToolGroupNames.Generators,
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Generators.HashAndChecksum.HashAndChecksumGenerator",
    ShortDisplayTitleResourceName = nameof(HashAndChecksumGenerator.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(HashAndChecksumGenerator.LongDisplayTitle),
    DescriptionResourceName = nameof(HashAndChecksumGenerator.Description),
    AccessibleNameResourceName = nameof(HashAndChecksumGenerator.AccessibleName))]
[AcceptedDataTypeName(PredefinedCommonDataTypeNames.Text)]
[AcceptedDataTypeName(PredefinedCommonDataTypeNames.File)]
internal sealed class HashAndChecksumGeneratorGuiTool : IGuiTool, IDisposable
{
    /// <summary>
    /// Whether the generated hash should be uppercase or lowercase.
    /// </summary>
    private static readonly SettingDefinition<bool> uppercase
        = new(
            name: $"{nameof(HashAndChecksumGeneratorGuiTool)}.{nameof(uppercase)}",
            defaultValue: false);

    /// <summary>
    /// The hashing algorithm to use.
    /// </summary>
    private static readonly SettingDefinition<HashAlgorithmType> hashAlgorithm
        = new(
            name: $"{nameof(HashAndChecksumGeneratorGuiTool)}.{nameof(hashAlgorithm)}",
            defaultValue: HashAlgorithmType.Md5);

    private enum GridRows
    {
        Stretch
    }

    private enum GridColumns
    {
        Left,
        Center,
        Right
    }

    private readonly DisposableSemaphore _semaphore = new();
    private readonly ILogger _logger;
    private readonly ISettingsProvider _settingsProvider;
    private readonly IUIStack _progressStack = Stack();
    private readonly IUIProgressBar _progressBar = ProgressBar();
    private readonly IUISingleLineTextInput _hmacSecretKeyInput = SingleLineTextInput("hash-checksum-generator-hmac-secret-key");
    private readonly IUIMultiLineTextInput _inputText = MultilineTextInput("hash-checksum-generator-input-text");
    private readonly IUIFileSelector _fileSelector = FileSelector("hash-checksum-generator-input-file");
    private readonly IUISingleLineTextInput _output = SingleLineTextInput("hash-checksum-generator-output");
    private readonly IUISingleLineTextInput _checksumVerification = SingleLineTextInput("hash-checksum-generator-checksum-verification");
    private readonly IUIInfoBar _infoBar = InfoBar("hash-checksum-generator-output-comparer-result");

    private CancellationTokenSource? _cancellationTokenSource;
    private OneOf<string, SandboxedFileReader>? _input;
    private bool _ignoreInputTextChange;
    private DateTime _lastStartTime;

    [ImportingConstructor]
    public HashAndChecksumGeneratorGuiTool(ISettingsProvider settingsProvider)
    {
        _logger = this.Log();
        _settingsProvider = settingsProvider;
    }

    // For unit tests.
    internal Task? WorkTask { get; private set; }

    public UIToolView View
        => new(
            isScrollable: true,
            Stack()
                .Vertical()
                .LargeSpacing()

                .WithChildren(
                    Stack()
                        .Vertical()
                        .SmallSpacing()

                        .WithChildren(
                            Label()
                                .Text(HashAndChecksumGenerator.ConfigurationTitle),

                            SettingGroup("hash-checksum-generator-hash-algorithm-setting")
                                .Icon("FluentSystemIcons", '\uF1EE')
                                .Title(HashAndChecksumGenerator.HashingAlgorithmTitle)
                                .Description(HashAndChecksumGenerator.HashingAlgorithmDescription)

                                .Handle(
                                    _settingsProvider,
                                    hashAlgorithm,
                                    onOptionSelected: OnHashingAlgorithmChanged,
                                    Item(HashAlgorithmName.MD5.Name, HashAlgorithmType.Md5),
                                    Item(HashAlgorithmName.SHA1.Name, HashAlgorithmType.Sha1),
                                    Item(HashAlgorithmName.SHA256.Name, HashAlgorithmType.Sha256),
                                    Item(HashAlgorithmName.SHA384.Name, HashAlgorithmType.Sha384),
                                    Item(HashAlgorithmName.SHA512.Name, HashAlgorithmType.Sha512))

                                .WithChildren(
                                    Setting("hash-checksum-generator-uppercase-setting")
                                        .Title(HashAndChecksumGenerator.UppercaseTitle)
                                        .Handle(_settingsProvider, uppercase, OnUppercaseChanged),

                                    _hmacSecretKeyInput
                                        .Title(HashAndChecksumGenerator.HmacSecret)
                                        .OnTextChanged(OnHmacSecretKeyChanged))),

                    Grid()
                        .ColumnMediumSpacing()

                        .Rows(
                            (GridRows.Stretch, new UIGridLength(1, UIGridUnitType.Fraction)))

                        .Columns(
                            (GridColumns.Left, new UIGridLength(1, UIGridUnitType.Fraction)),
                            (GridColumns.Center, Auto),
                            (GridColumns.Right, new UIGridLength(1, UIGridUnitType.Fraction)))

                        .Cells(
                            Cell(
                                GridRows.Stretch,
                                GridColumns.Left,

                                _inputText
                                    .Title(HashAndChecksumGenerator.InputTextTitle)
                                    .OnTextChanged(OnInputTextChanged)),

                            Cell(
                                GridRows.Stretch,
                                GridColumns.Center,

                                Label()
                                    .Text(HashAndChecksumGenerator.Or)
                                    .AlignVertically(UIVerticalAlignment.Center)),

                            Cell(
                                GridRows.Stretch,
                                GridColumns.Right,

                                _fileSelector
                                    .CanSelectOneFile()
                                    .OnFilesSelected(OnInputFileChanged))),

                    _progressStack
                        .AlignHorizontally(UIHorizontalAlignment.Center)
                        .Horizontal()
                        .SmallSpacing()
                        .Hide()

                        .WithChildren(
                            _progressBar,
                            Button()
                                .Icon("FluentSystemIcons", '\uF75A')
                                .OnClick(OnCancelButtonClicked)),

                    _output
                        .Title(HashAndChecksumGenerator.OutputTitle)
                        .ReadOnly()
                        .OnTextChanged(OnOutputOrOutputComparerTextChanged),

                    _checksumVerification
                        .Title(HashAndChecksumGenerator.ChecksumVerificationTitle)
                        .OnTextChanged(OnOutputOrOutputComparerTextChanged),

                    _infoBar.NonClosable()));

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
        if (dataTypeName == PredefinedCommonDataTypeNames.Text && parsedData is string text)
        {
            _inputText.Text(text); // This will trigger a hash generation.
        }
        else if (dataTypeName == PredefinedCommonDataTypeNames.File && parsedData is FileInfo fileInfo)
        {
            _fileSelector.WithFiles(SandboxedFileReader.FromFileInfo(fileInfo)); // This will trigger a hash generation.
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _semaphore.Dispose();
    }

    private void OnOutputOrOutputComparerTextChanged(string text)
    {
        if (!string.IsNullOrEmpty(_output.Text) && !string.IsNullOrEmpty(_checksumVerification.Text))
        {
            if (string.Equals(_output.Text, _checksumVerification.Text, StringComparison.OrdinalIgnoreCase))
            {
                _infoBar.Title(HashAndChecksumGenerator.HashesMatch).Success();
            }
            else
            {
                _infoBar.Title(HashAndChecksumGenerator.HashesMismatch).Error();
            }

            _infoBar.Open();
        }
        else
        {
            _infoBar.Close();
        }
    }

    private void OnUppercaseChanged(bool isUppercase)
    {
        if (isUppercase)
        {
            _output.Text(_output.Text.ToUpperInvariant());
        }
        else
        {
            _output.Text(_output.Text.ToLowerInvariant());
        }
    }

    private void OnHashingAlgorithmChanged(HashAlgorithmType hashAlgorithm)
    {
        StartComputeHash();
    }

    private void OnHmacSecretKeyChanged(string secretKey)
    {
        StartComputeHash();
    }

    private void OnInputTextChanged(string text)
    {
        if (!_ignoreInputTextChange)
        {
            if (_input.HasValue && _input.Value.TryPickT1(out SandboxedFileReader file, out _))
            {
                file.Dispose();
            }

            _input = text;

            StartComputeHash();
        }
    }

    private void OnInputFileChanged(SandboxedFileReader[] sandboxedFileReader)
    {
        if (_input.HasValue && _input.Value.TryPickT1(out SandboxedFileReader file, out _))
        {
            file.Dispose();
        }

        Guard.HasSizeEqualTo(sandboxedFileReader, 1);
        _input = sandboxedFileReader[0];

        _ignoreInputTextChange = true;
        _inputText.Text(string.Empty);
        _ignoreInputTextChange = false;

        StartComputeHash();
    }

    private void OnCancelButtonClicked()
    {
        _cancellationTokenSource?.Cancel();
        _progressStack.Hide();
    }

    private void StartComputeHash()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
        _lastStartTime = DateTime.UtcNow;

        if (_input.HasValue)
        {
            WorkTask
                = ComputeHashAsync(
                    _input.Value,
                    _settingsProvider.GetSetting(hashAlgorithm),
                    _settingsProvider.GetSetting(uppercase),
                    _hmacSecretKeyInput.Text,
                    _cancellationTokenSource.Token);
        }
        else
        {
            WorkTask = Task.CompletedTask;
        }
    }

    private async Task ComputeHashAsync(
        OneOf<string, SandboxedFileReader> input,
        HashAlgorithmType hashAlgorithm,
        bool uppercase,
        string hmacSecretKey,
        CancellationToken cancellationToken)
    {
        try
        {
            using (await _semaphore.WaitAsync(cancellationToken))
            {
                _output.Text(string.Empty);
                _progressBar.Progress(0);

                await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(cancellationToken);

                Stream? inputStream
                    = await input.Match(
                        async inputString =>
                        {
                            var stringStream = new MemoryStream();
                            var writer = new StreamWriter(stringStream);
                            if (inputString is not null)
                            {
                                await writer.WriteAsync(inputString.AsMemory(), cancellationToken);
                            }
                            writer.Flush();
                            stringStream.Position = 0;
                            return (Stream)stringStream;
                        },
                        inputFile => inputFile.GetNewAccessToFileContentAsync(cancellationToken));

                Guard.IsNotNull(inputStream);
                string hash
                    = await ComputeHashFromStreamAsync(
                        inputStream,
                        hashAlgorithm,
                        uppercase,
                        hmacSecretKey,
                        UpdateProgress,
                        cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                _output.Text(hash);
            }
        }
        finally
        {
            _progressStack.Hide();
        }
    }

    private static async Task<string> ComputeHashFromStreamAsync(
        Stream inputStream,
        HashAlgorithmType hashAlgorithm,
        bool uppercase,
        string hmacSecretKey,
        Action<HashingProgress> progressCallback,
        CancellationToken cancellationToken)
    {
        try
        {
            string fileHashString
                = await HashingHelper.ComputeHashAsync(
                    hashAlgorithm,
                    inputStream,
                    hmacSecretKey,
                    progressCallback,
                    cancellationToken);

            if (!uppercase)
            {
                return fileHashString.ToLowerInvariant();
            }

            return fileHashString;
        }
        catch (OperationCanceledException)
        {
            return string.Empty;
        }
        catch (Exception ex)
        {
            // Logger.LogFault("Checksum Generator", ex, $"Failed to calculate FileHash, algorithm used: {inputHashingAlgorithm}");
            return ex.Message;
        }
    }

    private void UpdateProgress(HashingProgress hashingProgress)
    {
        if (!hashingProgress.CancellationToken.IsCancellationRequested)
        {
            if (DateTime.UtcNow - _lastStartTime > TimeSpan.FromMilliseconds(250))
            {
                // Show progress bar only if the operation takes more than 250ms.
                // This avoids having the progress bar flickering as the user type the HMAC secret key or type in the text input form. 
                _progressStack.Show();
            }
            _progressBar.ProgressAsync(hashingProgress.GetPercentage()).Forget();
        }
    }
}
