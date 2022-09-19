using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using DevToys.Api.Core;
using DevToys.Api.Core.Settings;
using DevToys.Api.Tools;
using DevToys.Helpers;
using DevToys.UI.Controls;
using DevToys.ViewModels.Tools.GuidGenerator;
using DevToys.Views.Tools.PasswordGenerator;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace DevToys.ViewModels.Tools.Generators.PasswordGenerator
{
    [Export(typeof(PasswordGeneratorToolViewModel))]
    public sealed class PasswordGeneratorToolViewModel : ObservableRecipient, IToolViewModel
    {
        /// <summary>
        /// Whether the password should include uppercase characters.
        /// </summary>
        private static readonly SettingDefinition<bool> Uppercase
            = new(
                name: $"{nameof(PasswordGeneratorToolViewModel)}.{nameof(Uppercase)}",
                isRoaming: true,
                defaultValue: true);

        /// <summary>
        /// Whether the password should include lowercase characters.
        /// </summary>
        private static readonly SettingDefinition<bool> Lowercase
            = new(
                name: $"{nameof(PasswordGeneratorToolViewModel)}.{nameof(Lowercase)}",
                isRoaming: true,
                defaultValue: true);

        /// <summary>
        /// Whether the password should include numbers.
        /// </summary>
        private static readonly SettingDefinition<bool> Numbers
            = new(
                name: $"{nameof(PasswordGeneratorToolViewModel)}.{nameof(Numbers)}",
                isRoaming: true,
                defaultValue: true);

        /// <summary>
        /// Whether the password should include special characters.
        /// </summary>
        private static readonly SettingDefinition<bool> SpecialCharacters
            = new(
                name: $"{nameof(PasswordGeneratorToolViewModel)}.{nameof(SpecialCharacters)}",
                isRoaming: true,
                defaultValue: true);

        /// <summary>
        /// How long the password should be.
        /// </summary>
        private static readonly SettingDefinition<int> Length
            = new(
                name: $"{nameof(PasswordGeneratorToolViewModel)}.{nameof(Length)}",
                isRoaming: true,
                defaultValue: 30);

        /// <summary>
        /// How many GUIDs should be generated at once.
        /// </summary>
        private static readonly SettingDefinition<int> PasswordsToGenerate
            = new(
                name: $"{nameof(GuidGeneratorToolViewModel)}.{nameof(PasswordsToGenerate)}",
                isRoaming: true,
                defaultValue: 1);

        private readonly IMarketingService _marketingService;
        internal ISettingsProvider SettingsProvider { get; }
        internal ICustomTextBox? OutputTextBox { private get; set; }

        private string _output = string.Empty;

        public Type View => typeof(PasswordGeneratorPage);

        internal PasswordGeneratorStrings Strings => LanguageManager.Instance.PasswordGenerator;

        internal bool HasUppercase
        {
            get => SettingsProvider.GetSetting(Uppercase);
            set
            {
                if (SettingsProvider.GetSetting(Uppercase) == value)
                {
                    return;
                }

                SettingsProvider.SetSetting(Uppercase, value);
                OnPropertyChanged();
            }
        }

        internal bool HasLowercase
        {
            get => SettingsProvider.GetSetting(Lowercase);
            set
            {
                if (SettingsProvider.GetSetting(Lowercase) == value)
                {
                    return;
                }

                SettingsProvider.SetSetting(Lowercase, value);
                OnPropertyChanged();
            }
        }

        internal bool HasNumbers
        {
            get => SettingsProvider.GetSetting(Numbers);
            set
            {
                if (SettingsProvider.GetSetting(Numbers) == value)
                {
                    return;
                }

                SettingsProvider.SetSetting(Numbers, value);
                OnPropertyChanged();
            }
        }

        internal bool HasSpecialCharacters
        {
            get => SettingsProvider.GetSetting(SpecialCharacters);
            set
            {
                if (SettingsProvider.GetSetting(SpecialCharacters) == value)
                {
                    return;
                }

                SettingsProvider.SetSetting(SpecialCharacters, value);
                OnPropertyChanged();
            }
        }

        internal int LengthOfPasswordToGenerate
        {
            get => SettingsProvider.GetSetting(Length);
            set
            {
                if (SettingsProvider.GetSetting(Length) == value)
                {
                    return;
                }

                SettingsProvider.SetSetting(Length, value);
                OnPropertyChanged();
            }
        }

        internal int NumberOfPasswordsToGenerate
        {
            get => SettingsProvider.GetSetting(PasswordsToGenerate);
            set
            {
                if (SettingsProvider.GetSetting(PasswordsToGenerate) == value)
                {
                    return;
                }

                SettingsProvider.SetSetting(PasswordsToGenerate, value);
                OnPropertyChanged();
            }
        }

        internal string Output
        {
            get => _output;
            set => SetProperty(ref _output, value);
        }

        [ImportingConstructor]
        public PasswordGeneratorToolViewModel(ISettingsProvider settingsProvider, IMarketingService marketingService)
        {
            SettingsProvider = settingsProvider;
            _marketingService = marketingService;

            GenerateCommand = new RelayCommand(ExecuteGenerateCommand);
        }

        #region GenerateCommand

        internal IRelayCommand GenerateCommand { get; }

        private void ExecuteGenerateCommand()
        {
            string[] randomChars = new[] {
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty
            };
            
            var rand = new CryptoRandom();
            var chars = new List<char>();

            if (HasUppercase)
            {
                randomChars[0] = UppercaseLetters;
                chars.Insert(rand.Next(0, chars.Count), randomChars[0][rand.Next(0, randomChars[0].Length)]);
            }

            if (HasLowercase)
            {
                randomChars[1] = LowercaseLetters;
                chars.Insert(rand.Next(0, chars.Count), randomChars[1][rand.Next(0, randomChars[1].Length)]);
            }

            if (HasNumbers)
            {
                randomChars[2] = Digits;
                chars.Insert(rand.Next(0, chars.Count), randomChars[2][rand.Next(0, randomChars[2].Length)]);
            }

            if (HasSpecialCharacters)
            {
                randomChars[3] = NonAlphanumerics;
                chars.Insert(rand.Next(0, chars.Count), randomChars[3][rand.Next(0, randomChars[3].Length)]);
            }

            randomChars = randomChars.Where(r => r.Length > 0).ToArray();

            for (int i = chars.Count; i < LengthOfPasswordToGenerate; i++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count),
                    rcs[rand.Next(0, rcs.Length)]);
            }

            Output = new string(chars.ToArray());
            OutputTextBox?.ScrollToBottom();
        }

        #endregion


        /// <summary>
        /// All non-alphanumeric characters.
        /// </summary>
        public const string NonAlphanumerics = "!@#$%^&*";

        /// <summary>
        /// All lower case ASCII characters.
        /// </summary>
        public const string LowercaseLetters = "abcdefghijkmnopqrstuvwxyz";

        /// <summary>
        /// All upper case ASCII characters.
        /// </summary>
        public const string UppercaseLetters = "ABCDEFGHJKLMNOPQRSTUVWXYZ";

        /// <summary>
        /// All digits.
        /// </summary>
        public const string Digits = "0123456789";
    }
}
