#nullable enable

using System;
using System.Collections.Generic;
using DevToys.Api.Tools;
using DevToys.Messages;
using DevToys.Shared.Core;
using DevToys.Views.Tools;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace DevToys.ViewModels.Tools
{
    public sealed class GroupToolViewModel : ObservableRecipient, IToolViewModel
    {
        public Type View { get; } = typeof(GroupToolPage);

        internal IEnumerable<IToolProvider>? ToolProviders { get; }

        public GroupToolViewModel(IEnumerable<IToolProvider>? toolProviders)
        {
            ToolProviders = toolProviders;

            NavigateToToolCommand = new RelayCommand<IToolProvider>(ExecuteNavigateToToolCommand);
            OpenToolInNewWindowClickCommand = new RelayCommand<IToolProvider>(ExecuteOpenToolInNewWindowClickCommand);

            // Activate the view model's messenger.
            IsActive = true;
        }

        #region NavigateToToolCommand

        public IRelayCommand<IToolProvider> NavigateToToolCommand { get; }

        private void ExecuteNavigateToToolCommand(IToolProvider? metadata)
        {
            Arguments.NotNull(metadata, nameof(metadata));
            Messenger.Send(new ChangeSelectedMenuItemMessage(metadata!));
        }

        #endregion

        #region OpenToolInNewWindowClickCommand

        public IRelayCommand<IToolProvider> OpenToolInNewWindowClickCommand { get; }

        private void ExecuteOpenToolInNewWindowClickCommand(IToolProvider? metadata)
        {
            Arguments.NotNull(metadata, nameof(metadata));
            Messenger.Send(new OpenToolInNewWindowMessage(metadata!));
        }

        #endregion
    }
}
