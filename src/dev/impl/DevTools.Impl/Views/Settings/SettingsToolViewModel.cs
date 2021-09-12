#nullable enable

using DevTools.Providers;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Composition;

namespace DevTools.Impl.Views.Settings
{
    [Export(typeof(SettingsToolViewModel))]
    public sealed class SettingsToolViewModel : ObservableRecipient, IToolViewModel
    {
        public Type View { get; } = typeof(SettingsToolPage);
    }
}
