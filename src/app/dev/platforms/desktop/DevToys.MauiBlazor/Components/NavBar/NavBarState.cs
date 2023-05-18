namespace DevToys.MauiBlazor.Components;

// TODO: Write unit tests
internal sealed class NavBarState
{
    // TODO: Save state in settings and restore it, along with window size and location.
    private NavBarSidebarStates _widthBasedState = NavBarSidebarStates.Expanded;
    private NavBarSidebarStates _userPreferredState = NavBarSidebarStates.Expanded;
    private int _width;

    internal string NavBarClassNames { get; private set; } = string.Empty;

    internal bool IsExpandedOverlay { get; private set; }

    internal bool IsHidden { get; private set; }

    internal bool IsCollapsed { get; private set; }

    internal bool WidthUpdated(int width, int hiddenThreshold, int collapsedThreshold)
    {
        bool stateChanged;
        bool useTransition = false;
        _width = width;

        if (_width <= hiddenThreshold)
        {
            stateChanged = _widthBasedState != NavBarSidebarStates.Hidden;
            _widthBasedState = NavBarSidebarStates.Hidden;
        }
        else if (_width <= collapsedThreshold)
        {
            stateChanged = _widthBasedState != NavBarSidebarStates.Collapsed;
            if (_widthBasedState == NavBarSidebarStates.Expanded)
            {
                useTransition = true;
            }
            _widthBasedState = NavBarSidebarStates.Collapsed;
        }
        else
        {
            stateChanged = _widthBasedState != NavBarSidebarStates.Expanded;
            useTransition = true;
            _widthBasedState = NavBarSidebarStates.Expanded;
        }

        // When resizing the window, let's automatically hide the expanded overlay, if opened.
        if ((_userPreferredState & NavBarSidebarStates.ExpandedOverlay) == NavBarSidebarStates.ExpandedOverlay)
        {
            stateChanged = true;
            useTransition = false;
            _userPreferredState &= ~NavBarSidebarStates.ExpandedOverlay;
        }

        UpdateCssToApply(useTransition);
        return stateChanged;
    }

    internal void ToggleSidebar()
    {
        Guard.IsNotEqualTo((int)_userPreferredState, (int)NavBarSidebarStates.Hidden);

        bool useTransition = true;
        bool userCurrentlyPreferExpanded = (_userPreferredState & NavBarSidebarStates.Expanded) == NavBarSidebarStates.Expanded;
        switch (_widthBasedState)
        {
            // Based on the control's width, the default / automatic mode of the sidebar should be either collapsed or hidden.
            case NavBarSidebarStates.Hidden:
            case NavBarSidebarStates.Collapsed:
                if ((_userPreferredState & NavBarSidebarStates.ExpandedOverlay) == NavBarSidebarStates.ExpandedOverlay)
                {
                    // It looks like the sidebar is currently expanded overlay.
                    // User clicked on the toggle button, so let's collapse the sidebar.
                    _userPreferredState = NavBarSidebarStates.Collapsed;
                    useTransition = false;
                }
                else if (userCurrentlyPreferExpanded)
                {
                    // User preferred sidebar mode is expanded. Currently, it's collapsed
                    // or hidden due to the small width of the control. Let's expand overlay the sidebar.
                    _userPreferredState |= NavBarSidebarStates.ExpandedOverlay;
                }
                else
                {
                    // User preferred sidebar mode is collapsed. Let's switch it to expanded, and let's show the overlay.
                    _userPreferredState = NavBarSidebarStates.Expanded | NavBarSidebarStates.ExpandedOverlay;
                }
                break;

            // Based on the control's width, the default / automatic mode of the sidebar should be expanded.
            case NavBarSidebarStates.Expanded:
                if (userCurrentlyPreferExpanded)
                {
                    // Currently, user's preferred mode is expanded. But user just clicked on the
                    // toggle button, so let's switch to collapsed mode.
                    _userPreferredState = NavBarSidebarStates.Collapsed;
                }
                else
                {
                    // Opposite scenario.
                    _userPreferredState = NavBarSidebarStates.Expanded;
                }
                break;

            default:
                ThrowHelper.ThrowNotSupportedException();
                break;
        }

        UpdateCssToApply(useTransition);
    }

    internal void CloseExpandedOverlay()
    {
        if ((_userPreferredState & NavBarSidebarStates.ExpandedOverlay) == NavBarSidebarStates.ExpandedOverlay)
        {
            _userPreferredState &= ~NavBarSidebarStates.ExpandedOverlay;
            UpdateCssToApply(useTransition: false);
        }
    }

    internal void ForceExpand()
    {
        if (_userPreferredState == NavBarSidebarStates.Collapsed
            || _widthBasedState == NavBarSidebarStates.Collapsed
            || _widthBasedState == NavBarSidebarStates.Hidden)
        {
            ToggleSidebar();
        }
    }

    private void UpdateCssToApply(bool useTransition = false)
    {
        bool userRequestExpandOverlay = (_userPreferredState & NavBarSidebarStates.ExpandedOverlay) == NavBarSidebarStates.ExpandedOverlay;
        bool isCollapsed = !userRequestExpandOverlay;
        var cssBuilder = new CssBuilder();

        cssBuilder.AddClass("transition", when: useTransition);

        switch (_widthBasedState)
        {
            case NavBarSidebarStates.Hidden:
                cssBuilder.AddClass("hidden");
                cssBuilder.AddClass("expanded-overlay", when: userRequestExpandOverlay);
                break;

            case NavBarSidebarStates.Collapsed:
                cssBuilder.AddClass("collapsed");
                cssBuilder.AddClass("expanded-overlay", when: userRequestExpandOverlay);
                break;

            case NavBarSidebarStates.Expanded:
                cssBuilder.AddClass("collapsed", when: _userPreferredState == NavBarSidebarStates.Collapsed);
                isCollapsed = _userPreferredState == NavBarSidebarStates.Collapsed;
                break;

            default:
                ThrowHelper.ThrowNotSupportedException();
                break;
        }

        NavBarClassNames = cssBuilder.ToString();
        IsExpandedOverlay = userRequestExpandOverlay;
        IsHidden = _widthBasedState == NavBarSidebarStates.Hidden;
        IsCollapsed = isCollapsed;
    }
}
