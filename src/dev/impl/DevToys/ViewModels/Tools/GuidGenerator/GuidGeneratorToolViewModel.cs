#nullable enable

using System;
using System.Composition;
using System.Text;
using DevToys.Api.Core;
using DevToys.Api.Core.Settings;
using DevToys.Api.Tools;
using DevToys.UI.Controls;
using DevToys.Views.Tools.GuidGenerator;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace DevToys.ViewModels.Tools.GuidGenerator
{
    [Export(typeof(GuidGeneratorToolViewModel))]
    public sealed class GuidGeneratorToolViewModel : ObservableRecipient, IToolViewModel
    {
        /// <summary>
        /// Whether the generated UUID should be uppercase or lowercase.
        /// </summary>
        private static readonly SettingDefinition<bool> Uppercase
            = new(
                name: $"{nameof(GuidGeneratorToolViewModel)}.{nameof(Uppercase)}",
                isRoaming: true,
                defaultValue: false);

        /// <summary>
        /// Whether the generated UUID should have hyphens or not.
        /// </summary>
        private static readonly SettingDefinition<bool> Hyphens
            = new(
                name: $"{nameof(GuidGeneratorToolViewModel)}.{nameof(Hyphens)}",
                isRoaming: true,
                defaultValue: true);

        /// <summary>
        /// The UUID Version to generate.
        /// </summary>
        private static readonly SettingDefinition<string> Version
            = new(
                name: $"{nameof(GuidGeneratorToolViewModel)}.{nameof(Version)}",
                isRoaming: true,
                defaultValue: DefaultVersion);

        /// <summary>
        /// How many GUIDs should be generated at once.
        /// </summary>
        private static readonly SettingDefinition<int> GuidsToGenerate
            = new(
                name: $"{nameof(GuidGeneratorToolViewModel)}.{nameof(GuidsToGenerate)}",
                isRoaming: true,
                defaultValue: 1);

        private static readonly Random Random = new Random();

        private static readonly DateTimeOffset GregorianCalendarStart = new(1582, 10, 15, 0, 0, 0, TimeSpan.Zero);
        private const int VariantByte = 8;
        private const int VariantByteMask = 0x3f;
        private const int VariantByteShift = 0x80;
        private const int VersionByte = 7;
        private const int VersionByteMask = 0x0f;
        private const int VersionByteShift = 4;
        private const int ByteArraySize = 16;
        private const byte TimestampByte = 0;
        private const byte NodeByte = 10;
        private const byte GuidClockSequenceByte = 8;
        private const string DefaultVersion = "Four";
        internal const string VersionOne = "One";

        private readonly IMarketingService _marketingService;
        private readonly ISettingsProvider _settingsProvider;

        private string _output = string.Empty;
        private bool _toolSuccessfullyWorked;

        public Type View { get; } = typeof(GuidGeneratorToolPage);

        internal GuidGeneratorStrings Strings => LanguageManager.Instance.GuidGenerator;

        internal bool IsUppercase
        {
            get => _settingsProvider.GetSetting(Uppercase);
            set
            {
                if (_settingsProvider.GetSetting(Uppercase) != value)
                {
                    _settingsProvider.SetSetting(Uppercase, value);
                    OnPropertyChanged();
                }
            }
        }

        internal bool IncludeHyphens
        {
            get => _settingsProvider.GetSetting(Hyphens);
            set
            {
                if (_settingsProvider.GetSetting(Hyphens) != value)
                {
                    _settingsProvider.SetSetting(Hyphens, value);
                    OnPropertyChanged();
                }
            }
        }

        internal string UuidVersion
        {
            get => _settingsProvider.GetSetting(Version);
            set
            {
                if (_settingsProvider.GetSetting(Version) != value)
                {
                    _settingsProvider.SetSetting(Version, value);
                    OnPropertyChanged();
                }
            }
        }

        internal int NumberOfGuidsToGenerate
        {
            get => _settingsProvider.GetSetting(GuidsToGenerate);
            set
            {
                if (_settingsProvider.GetSetting(GuidsToGenerate) != value)
                {
                    _settingsProvider.SetSetting(GuidsToGenerate, value);
                    OnPropertyChanged();
                }
            }
        }

        internal string Output
        {
            get => _output;
            set => SetProperty(ref _output, value);
        }

        internal ICustomTextBox? OutputTextBox { private get; set; }

        [ImportingConstructor]
        public GuidGeneratorToolViewModel(ISettingsProvider settingsProvider, IMarketingService marketingService)
        {
            _settingsProvider = settingsProvider;
            _marketingService = marketingService;

            GenerateCommand = new RelayCommand(ExecuteGenerateCommand);
        }

        #region GenerateCommand

        internal IRelayCommand GenerateCommand { get; }

        private void ExecuteGenerateCommand()
        {
            string guidStringFormat;
            if (IncludeHyphens)
            {
                guidStringFormat = "D";
            }
            else
            {
                guidStringFormat = "N";
            }

            var newGuids = new StringBuilder();
            for (int i = 0; i < NumberOfGuidsToGenerate; i++)
            {
                string? guid;
                if (string.Equals(VersionOne, UuidVersion, StringComparison.Ordinal))
                {
                    guid = GenerateTimeBasedGuid().ToString(guidStringFormat);
                }
                else
                {
                    guid = Guid.NewGuid().ToString(guidStringFormat);
                }

                if (IsUppercase)
                {
                    guid = guid.ToUpperInvariant();
                }

                newGuids.AppendLine(guid);
            }

            Output += newGuids.ToString();
            OutputTextBox?.ScrollToBottom();

            if (!_toolSuccessfullyWorked)
            {
                _toolSuccessfullyWorked = true;
                _marketingService.NotifyToolSuccessfullyWorked();
            }
        }

        #endregion

        private Guid GenerateTimeBasedGuid()
        {
            DateTime dateTime = DateTime.UtcNow;
            long ticks = dateTime.Ticks - GregorianCalendarStart.Ticks;

            byte[] guid = new byte[ByteArraySize];
            byte[] timestamp = BitConverter.GetBytes(ticks);

            // copy node
            byte[]? nodes = GenerateNodeBytes();
            Array.Copy(nodes, 0, guid, NodeByte, Math.Min(6, nodes.Length));

            // copy clock sequence
            byte[]? clockSequence = GenerateClockSequenceBytes(dateTime);
            Array.Copy(clockSequence, 0, guid, GuidClockSequenceByte, Math.Min(2, clockSequence.Length));

            // copy timestamp
            Array.Copy(timestamp, 0, guid, TimestampByte, Math.Min(8, timestamp.Length));

            // set the variant
            guid[VariantByte] &= (byte)VariantByteMask;
            guid[VariantByte] |= (byte)VariantByteShift;

            // set the version
            guid[VersionByte] &= (byte)VersionByteMask;
            guid[VersionByte] |= (byte)((byte)GuidVersion.TimeBased << VersionByteShift);

            return new Guid(guid);
        }

        public byte[] GenerateNodeBytes()
        {
            byte[]? node = new byte[6];

            Random.NextBytes(node);
            return node;
        }

        public byte[] GenerateClockSequenceBytes(DateTime dt)
        {
            DateTime utc = dt.ToUniversalTime();

            byte[]? bytes = BitConverter.GetBytes(utc.Ticks);

            if (bytes.Length == 0)
            {
                return new byte[] { 0x0, 0x0 };
            }

            if (bytes.Length == 1)
            {
                return new byte[] { 0x0, bytes[0] };
            }

            return new byte[] { bytes[0], bytes[1] };
        }

        private enum GuidVersion
        {
            TimeBased = 0x01,
            Reserved = 0x02,
            NameBased = 0x03,
            Random = 0x04
        }
    }
}
