#nullable enable

using System;
using System.Collections.Generic;
using Windows.System;

namespace DevToys.UI.Controls.TextEditor
{
    public class KeyboardCommand<T> : IKeyboardCommand<T>
    {
        private static readonly TimeSpan ConsecutiveHitsInterval = TimeSpan.FromMilliseconds(500);

        private readonly bool _ctrl;
        private readonly bool _alt;
        private readonly bool _shift;
        private readonly IList<VirtualKey> _keys;
        private readonly Action<T>? _action;
        private readonly bool _shouldHandle;
        private readonly bool _shouldSwallow;
        private readonly int _requiredHits;
        private int _hits;
        private DateTime _lastHitTimestamp;

        public KeyboardCommand(
            VirtualKey key,
            Action<T>? action,
            bool shouldHandle = true,
            bool shouldSwallow = true) :
            this(false, false, false, key, action, shouldHandle, shouldSwallow)
        {
        }

        public KeyboardCommand(
            bool ctrlDown,
            bool altDown,
            bool shiftDown,
            VirtualKey key,
            Action<T>? action,
            bool shouldHandle = true,
            bool shouldSwallow = true,
            int requiredHits = 1) :
            this(ctrlDown, altDown, shiftDown, new List<VirtualKey>() { key }, action, shouldHandle, shouldSwallow, requiredHits)
        {
        }

        public KeyboardCommand(
            bool ctrlDown,
            bool altDown,
            bool shiftDown,
            IList<VirtualKey> keys,
            Action<T>? action,
            bool shouldHandle,
            bool shouldSwallow,
            int requiredHits = 1)
        {
            _ctrl = ctrlDown;
            _alt = altDown;
            _shift = shiftDown;
            _keys = keys ?? new List<VirtualKey>();
            _action = action;
            _shouldHandle = shouldHandle;
            _shouldSwallow = shouldSwallow;
            _requiredHits = requiredHits;
            _hits = 0;
            _lastHitTimestamp = DateTime.MinValue;
        }

        public bool Hit(bool ctrlDown, bool altDown, bool shiftDown, VirtualKey key)
        {
            return _ctrl == ctrlDown && _alt == altDown && _shift == shiftDown && _keys.Contains(key);
        }

        public bool ShouldExecute(IKeyboardCommand<T>? lastCommand)
        {
            DateTime now = DateTime.UtcNow;

            if (lastCommand == this && now - _lastHitTimestamp < ConsecutiveHitsInterval)
            {
                _hits++;
            }
            else
            {
                _hits = 1;
            }

            _lastHitTimestamp = now;

            if (_hits >= _requiredHits)
            {
                _hits = 0;
                return true;
            }

            return false;
        }

        public bool ShouldHandleAfterExecution()
        {
            return _shouldHandle;
        }

        public bool ShouldSwallowAfterExecution()
        {
            return _shouldSwallow;
        }

        public void Execute(T args)
        {
            _action?.Invoke(args);
        }
    }
}