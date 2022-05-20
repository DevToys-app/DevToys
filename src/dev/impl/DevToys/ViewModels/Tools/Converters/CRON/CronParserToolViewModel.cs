#nullable enable

using System;
using Cronos;
using System.Composition;
using DevToys.Api.Tools;
using DevToys.Views.Tools.CronParser;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Windows.ApplicationModel.DataTransfer;
using System.Collections.Generic;

namespace DevToys.ViewModels.Tools.CronParser
{
    [Export(typeof(CronParserToolViewModel))]
    public sealed class CronParserToolViewModel : ObservableRecipient, IToolViewModel
    {
        private bool _isInputInvalid;
        private string _cronExpression;        

        public Type View => typeof(CronParserToolPage);

        internal Base64EncoderDecoderStrings Strings => LanguageManager.Instance.Base64EncoderDecoder;

        internal bool IsInputInvalid
        {
            get => _isInputInvalid;
            set => SetProperty(ref _isInputInvalid, value);
        }

        internal string UserCronExpression
        {
            get => _cronExpression;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _cronExpression = string.Empty;
                }
                else
                {
                    _cronExpression = value;
                }

                ParseCronExpression();
            }
        }

        internal string OutputValue
        {
            get;
            set;
        }

        private void ParseCronExpression()
        {
            IList<DateTimeOffset> dateTimeOffsets = new List<DateTimeOffset>();

            var expression = CronExpression.Parse(UserCronExpression);

            DateTimeOffset? nextOccurence = expression.GetNextOccurrence(DateTimeOffset.Now, TimeZoneInfo.Local, true);

            if (nextOccurence == null)
            {
                return;
            }

            dateTimeOffsets.Add(nextOccurence.Value);

            for (int i = 0; i < 5; i++)
            {
                nextOccurence = expression.GetNextOccurrence(nextOccurence.Value, TimeZoneInfo.Local, true);

                if (nextOccurence == null)
                {
                    return;
                }

                dateTimeOffsets.Add(nextOccurence.Value);

                i++;
            }

            IEnumerable<DateTimeOffset>? occurences = expression.GetOccurrences(DateTimeOffset.Now, DateTimeOffset.Now.AddDays(1), TimeZoneInfo.Local, true, true);

            OutputValue = String.Join(Environment.NewLine, dateTimeOffsets);
        }

        public CronParserToolViewModel()
        {
            PasteCommand = new RelayCommand(ExecutePasteCommand);
            CopyCommand = new RelayCommand(ExecuteCopyCommand);

            // Set to the current epoch time.
            UserCronExpression = "* * * * *";
        }

        #region PasteCommand

        internal IRelayCommand PasteCommand { get; }

        private async void ExecutePasteCommand()
        {
            try
            {
                DataPackageView? dataPackageView = Clipboard.GetContent();
                if (!dataPackageView.Contains(StandardDataFormats.Text))
                {
                    return;
                }

                string text = await dataPackageView.GetTextAsync();

                UserCronExpression = text;
                
            }
            catch (Exception ex)
            {
                Core.Logger.LogFault("Failed to paste in numeric box", ex);
            }
        }

        #endregion

        #region CopyCommand

        internal IRelayCommand CopyCommand { get; }

        private void ExecuteCopyCommand()
        {
            try
            {
                var data = new DataPackage
                {
                    RequestedOperation = DataPackageOperation.Copy
                };
                data.SetText(UserCronExpression);

                Clipboard.SetContentWithOptions(data, new ClipboardContentOptions() { IsAllowedInHistory = true, IsRoamable = true });
                Clipboard.Flush(); // This method allows the content to remain available after the application shuts down.
            }
            catch (Exception ex)
            {
                Core.Logger.LogFault("Failed to copy from numeric box", ex);
            }
        }

        #endregion               
    }
}
