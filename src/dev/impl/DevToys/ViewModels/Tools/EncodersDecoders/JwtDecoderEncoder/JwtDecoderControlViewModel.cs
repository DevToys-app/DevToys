﻿#nullable enable

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
                ClearPayload();

                return;
            }

            DecoderParameters decoderParamters = new();
            if (ValidateSignature)
            {
                decoderParamters.ValidateSignature = ValidateSignature;
                decoderParamters.ValidateIssuerSigningKey = ValidateIssuerSigningKey;
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

            TokenResult? result = _decoder.DecodeToken(decoderParamters, tokenParameters, TokenErrorCallBack, out JwtAlgorithm? jwtAlgorithm);

            ThreadHelper.RunOnUIThreadAsync(ThreadPriority.Low, () =>
            {
                if (ValidateSignature)
                {
                    RequireSignature = true;
                    if (jwtAlgorithm is
                        not null and
                        not JwtAlgorithm.HS256 and
                        not JwtAlgorithm.HS384 and
                        not JwtAlgorithm.HS512)
                    {
                        RequireSignature = false;
                    }
                }

                if (result is null)
                {
                    return;
                }

                Header = result.Header;
                Payload = result.Payload;

                DisplayValidationInfoBar(decoderParamters);

                if (ToolSuccessfullyWorked)
                {
                    ToolSuccessfullyWorked = true;
                    MarketingService.NotifyToolSuccessfullyWorked();
                }
            }).ForgetSafely();
        }

        private void TokenErrorCallBack(TokenResultErrorEventArgs e)
        {
            JwtValidation.IsValid = false;
            JwtValidation.ErrorMessage = e.Message;
            ThreadHelper.RunOnUIThreadAsync(ThreadPriority.Low, () =>
            {
                Header = string.Empty;
                ClearPayload();
                DisplayValidationInfoBar();
            }).ForgetSafely();
        }

        private void ClearPayload()
        {
            Payload = string.Empty;
        }
    }
}
