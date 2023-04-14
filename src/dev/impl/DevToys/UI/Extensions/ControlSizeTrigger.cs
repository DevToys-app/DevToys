#nullable enable

using Windows.UI.Xaml;

namespace DevToys.UI.Extensions
{
    public sealed class ControlSizeTrigger : StateTriggerBase
    {
        private FrameworkElement? _targetElement;
        private double _minHeight = -1;
        private double _minWidth = -1;
        private double _currentHeight;
        private double _currentWidth;

        public double MinHeight
        {
            get => _minHeight;
            set => _minHeight = value;
        }

        public double MinWidth
        {
            get => _minWidth;
            set => _minWidth = value;
        }

        public FrameworkElement? TargetElement
        {
            get => _targetElement;
            set
            {
                if (_targetElement is not null)
                {
                    _targetElement.SizeChanged -= OnSizeChanged;
                }
                _targetElement = value;
                if (_targetElement is not null)
                {
                    _targetElement.SizeChanged += OnSizeChanged;
                }
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _currentHeight = e.NewSize.Height;
            _currentWidth = e.NewSize.Width;
            UpdateTrigger();
        }

        private void UpdateTrigger()
        {
            if (_targetElement != null && (_minWidth > 0 || _minHeight > 0))
            {
                if (_minHeight > 0 && _minWidth > 0)
                {
                    SetActive((_currentHeight >= _minHeight) && (_currentWidth >= _minWidth));
                }
                else if (_minHeight > 0)
                {
                    SetActive(_currentHeight >= _minHeight);
                }
                else
                {
                    SetActive(_currentWidth >= _minWidth);
                }
            }
            else
            {
                SetActive(false);
            }
        }
    }
}
