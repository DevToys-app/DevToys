#nullable enable

using System;
using System.Collections.Generic;
using System.Composition;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using DevToys.Api.Core;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Shared.Core.Threading;
using DevToys.Models;
using DevToys.Views.Tools.JwtDecoderEncoder;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using DevToys.Helpers.JsonYaml;

namespace DevToys.ViewModels.Tools.JwtDecoderEncoder
{
    [Export(typeof(JwtDecoderEncoderToolViewModel))]
    public sealed class JwtDecoderEncoderToolViewModel : ObservableRecipient, IToolViewModel
    {
        private readonly IMarketingService _marketingService;
        private readonly Queue<string> _decodingQueue = new();

        private bool _toolSuccessfullyWorked;
        private bool _decodingInProgress;
        private string? _jwtToken;
        private string? _jwtHeader;
        private string? _jwtPayload;

        public Type View { get; } = typeof(JwtDecoderEncoderToolPage);

        internal JwtDecoderEncoderStrings Strings => LanguageManager.Instance.JwtDecoderEncoder;

        /// <summary>
        /// Gets or sets the jwt token.
        /// </summary>
        internal string? JwtToken
        {
            get => _jwtToken;
            set
            {
                SetProperty(ref _jwtToken, value?.Trim());
                QueueConversion();
            }
        }

        /// <summary>
        /// Gets or sets the jwt header.
        /// </summary>
        internal string? JwtHeader
        {
            get => _jwtHeader;
            set => SetProperty(ref _jwtHeader, value);
        }

        /// <summary>
        /// Gets or sets the jwt payload.
        /// </summary>
        internal string? JwtPayload
        {
            get => _jwtPayload;
            set => SetProperty(ref _jwtPayload, value);
        }

        [ImportingConstructor]
        public JwtDecoderEncoderToolViewModel(IMarketingService marketingService)
        {
            _marketingService = marketingService;
        }

        private void QueueConversion()
        {
            _decodingQueue.Enqueue(JwtToken ?? string.Empty);
            TreatQueueAsync().Forget();
        }

        private async Task TreatQueueAsync()
        {
            if (_decodingInProgress)
            {
                return;
            }

            _decodingInProgress = true;

            await TaskScheduler.Default;

            while (_decodingQueue.TryDequeue(out string? text))
            {
                JwtDecode(text, out string? header, out string? payload);
                ThreadHelper.RunOnUIThreadAsync(ThreadPriority.Low, () =>
                {
                    JwtHeader = header;
                    JwtPayload = payload;

                    if (!_toolSuccessfullyWorked)
                    {
                        _toolSuccessfullyWorked = true;
                        _marketingService.NotifyToolSuccessfullyWorked();
                    }
                }).ForgetSafely();
            }

            _decodingInProgress = false;
        }

        private void JwtDecode(string input, out string header, out string payload)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                header = string.Empty;
                payload = string.Empty;
                return;
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwtSecurityToken = handler.ReadJwtToken(input);
                header = JsonHelper.Format(jwtSecurityToken.Header.SerializeToJson(), Indentation.TwoSpaces);
                payload = JsonHelper.Format(jwtSecurityToken.Payload.SerializeToJson(), Indentation.TwoSpaces);
            }
            catch (Exception ex)
            {
                header = ex.Message;
                payload = ex.Message;
            }
        }
    }
}
