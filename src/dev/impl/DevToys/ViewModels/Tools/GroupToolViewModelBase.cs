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
        private IEnumerable<ToolProviderViewItem>? _toolProviders;

        public IEnumerable<ToolProviderViewItem>? ToolProviders
        {
            get => _toolProviders;
            set => SetProperty(ref _toolProviders, value);
        }

        public bool IsToolProvidersEmpty
        {
            get
            {
                if (ToolProviders == null)
                {
                    return true;
                }

                using IEnumerator<ToolProviderViewItem>? enumerator = ToolProviders.GetEnumerator();
                return !enumerator.MoveNext();
            }
        }

        public GroupToolViewModelBase()
        {
            NavigateToToolCommand = new RelayCommand<IToolProvider>(ExecuteNavigateToToolCommand);
            OpenToolInNewWindowClickCommand = new RelayCommand<ToolProviderMetadata>(ExecuteOpenToolInNewWindowClickCommand);
            PinToolToStartClickCommand = new RelayCommand<ToolProviderMetadata>(ExecutePinToolToStartClickCommand);
            AddToFavoritesCommand = new RelayCommand<ToolProviderViewItem>(ExecuteAddToFavoritesCommand);
            RemoveFromFavoritesCommand = new RelayCommand<ToolProviderViewItem>(ExecuteRemoveFromFavoritesCommand);

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

        public IRelayCommand<ToolProviderMetadata> OpenToolInNewWindowClickCommand { get; }

        private void ExecuteOpenToolInNewWindowClickCommand(ToolProviderMetadata? metadata)
        {
            Arguments.NotNull(metadata, nameof(metadata));
            Messenger.Send(new OpenToolInNewWindowMessage(metadata!));
        }

        #endregion

        #region PinToolToStartClickCommand

        public IRelayCommand<ToolProviderMetadata> PinToolToStartClickCommand { get; }

        private void ExecutePinToolToStartClickCommand(ToolProviderMetadata? metadata)
        {
            Arguments.NotNull(metadata, nameof(metadata));
            Messenger.Send(new PinToolToStartMessage(metadata!));
        }

        #endregion

        #region AddToFavoritesCommand

        public IRelayCommand<ToolProviderViewItem> AddToFavoritesCommand { get; }

        private void ExecuteAddToFavoritesCommand(ToolProviderViewItem? tool)
        {
            Arguments.NotNull(tool, nameof(tool));
            Messenger.Send(new AddToFavoritesMessage(tool!));
        }

        #endregion

        #region RemoveFromFavoritesCommand

        public IRelayCommand<ToolProviderViewItem> RemoveFromFavoritesCommand { get; }

        private void ExecuteRemoveFromFavoritesCommand(ToolProviderViewItem? tool)
        {
            Arguments.NotNull(tool, nameof(tool));
            Messenger.Send(new RemoveFromFavoritesMessage(tool!));
        }

        #endregion
    }
}
