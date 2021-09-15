#nullable enable

using System.Collections.Generic;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace DevTools.Common.UI.Controls.TextEditor
{
    public class KeyboardCommandHandler : ICommandHandler<KeyRoutedEventArgs>
    {
        public readonly ICollection<IKeyboardCommand<KeyRoutedEventArgs>> Commands;

        private IKeyboardCommand<KeyRoutedEventArgs>? _lastCommand;

        public KeyboardCommandHandler(ICollection<IKeyboardCommand<KeyRoutedEventArgs>> commands)
        {
            Commands = commands;
        }

        public CommandHandlerResult Handle(KeyRoutedEventArgs args)
        {
            var ctrlDown = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down);
            var altDown = Window.Current.CoreWindow.GetKeyState(VirtualKey.Menu).HasFlag(CoreVirtualKeyStates.Down);
            var shiftDown = Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down);
            var shouldHandle = false;
            var shouldSwallow = false;

            foreach (var command in Commands)
            {
                if (command.Hit(ctrlDown, altDown, shiftDown, args.Key))
                {
                    if (command.ShouldExecute(_lastCommand))
                    {
                        command.Execute(args);
                    }

                    if (command.ShouldSwallowAfterExecution())
                    {
                        shouldSwallow = true;
                    }

                    if (command.ShouldHandleAfterExecution())
                    {
                        shouldHandle = true;
                    }

                    _lastCommand = command;
                    break;
                }
            }

            if (!shouldHandle)
            {
                _lastCommand = null;
            }

            return new CommandHandlerResult(shouldHandle, shouldSwallow);
        }
    }
}