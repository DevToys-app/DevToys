#nullable enable

namespace DevTools.Common.UI.Controls.TextEditor
{
    public interface ICommandHandler<in T>
    {
        CommandHandlerResult Handle(T args);
    }
}