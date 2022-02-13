#nullable enable

using System.Collections.Generic;
using DevToys.Api.Tools;
using DevToys.Messages;
using DevToys.Shared.Core;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace DevToys.ViewModels.Tools
{
    public abstract class GroupToolViewModelBase : ObservableRecipient
    {
        public IEnumerable<IToolProvider>? ToolProviders { get; protected set; }

        public bool IsToolProvidersEmpty => !ToolProviders?.GetEnumerator().MoveNext() ?? true;

        public GroupToolViewModelBase()
        {
            NavigateToToolCommand = new RelayCommand<IToolProvider>(ExecuteNavigateToToolCommand);
            OpenToolInNewWindowClickCommand = new RelayCommand<IToolProvider>(ExecuteOpenToolInNewWindowClickCommand);
            PinToolToStartClickCommand = new RelayCommand<IToolProvider>(ExecutePinToolToStartClickCommand);

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

        #region PinToolToStartClickCommand

        public IRelayCommand<IToolProvider> PinToolToStartClickCommand { get; }

        private void ExecutePinToolToStartClickCommand(IToolProvider? metadata)
        {
            Arguments.NotNull(metadata, nameof(metadata));
            Messenger.Send(new PinToolToStartMessage(metadata!));
        }

        #endregion
    }
}
