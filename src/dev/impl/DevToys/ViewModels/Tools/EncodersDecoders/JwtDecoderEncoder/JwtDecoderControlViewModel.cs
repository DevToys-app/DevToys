#nullable enable

using System;
using System.Composition;
using System.Linq;
using DevToys.Api.Core;
using DevToys.Api.Core.Settings;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Models;
using DevToys.Models.JwtDecoderEncoder;
using DevToys.Shared.Core.Threading;
using DevToys.Views.Tools.EncodersDecoders.JwtDecoderEncoder;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace DevToys.ViewModels.Tools.EncodersDecoders.JwtDecoderEncoder
{
    [Export(typeof(JwtDecoderControlViewModel))]
    public sealed class JwtDecoderControlViewModel : JwtDecoderEncoderViewModelBase, IToolViewModel, IRecipient<JwtJobAddedMessage>
    {
        private readonly JwtDecoder _decoder;

        public Type View { get; } = typeof(JwtDecoderControl);

        [ImportingConstructor]
        public JwtDecoderControlViewModel(
           ISettingsProvider settingsProvider,
           IMarketingService marketingService,
           JwtDecoder decoder)
           : base(settingsProvider, marketingService)
        {
            IsActive = true;
            _decoder = decoder;
            ShowValidation = ValidateSignature;
        }

        public void Receive(JwtJobAddedMessage message)
        {
            if (string.IsNullOrWhiteSpace(Token))
            {
                return;
            }

            DecoderParameters decoderParamters = new();
            if (ValidateSignature)
            {
                decoderParamters.ValidateSignature = ValidateSignature;
                decoderParamters.ValidateAudience = ValidateAudience;
                decoderParamters.ValidateLifetime = ValidateLifetime;
                decoderParamters.ValidateIssuer = ValidateIssuer;
                decoderParamters.ValidateActor = ValidateActor;
            }

            TokenParameters tokenParameters = new()
            {
                Token = Token,
                Signature = Signature,
                PublicKey = PublicKey,
            };

            if (!string.IsNullOrEmpty(ValidIssuers))
            {
                tokenParameters.ValidIssuers = ValidIssuers!.Split(',').ToHashSet();
            }

            if (!string.IsNullOrEmpty(ValidAudiences))
            {
                tokenParameters.ValidAudiences = ValidAudiences!.Split(',').ToHashSet();
            }

            TokenResult? result = _decoder.DecodeToken(decoderParamters, tokenParameters, TokenErrorCallBack);

            ThreadHelper.RunOnUIThreadAsync(ThreadPriority.Low, () =>
            {
                if (result is null)
                {
                    return;
                }

                Header = result.Header;
                Payload = result.Payload;

                if (ValidateSignature)
                {
                    RequireSignature = true;
                    if (result.TokenAlgorithm is
                        not JwtAlgorithm.HS256 and
                        not JwtAlgorithm.HS384 and
                        not JwtAlgorithm.HS512)
                    {
                        RequireSignature = false;
                    }

                }

                DisplayValidationInfoBar();


                if (ToolSuccessfullyWorked)
                {
                    ToolSuccessfullyWorked = true;
                    MarketingService.NotifyToolSuccessfullyWorked();
                }
            }).ForgetSafely();
        }
    }
}
