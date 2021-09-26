#nullable enable

namespace DevToys.UI.Controls.TextEditor
{
    public interface ICommandHandler<in T>
    {
        CommandHandlerResult Handle(T args);
    }
}