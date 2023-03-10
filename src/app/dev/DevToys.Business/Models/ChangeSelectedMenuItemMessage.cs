using CommunityToolkit.Mvvm.Messaging.Messages;

namespace DevToys.Business.Models;

internal sealed class ChangeSelectedMenuItemMessage : ValueChangedMessage<object>
{
    internal ChangeSelectedMenuItemMessage(object value)
        : base(value)
    {
    }
}
