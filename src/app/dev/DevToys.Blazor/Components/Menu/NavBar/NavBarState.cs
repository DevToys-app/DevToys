namespace DevToys.Blazor.Components;

internal sealed class NavBarState
{
    private NavBarSidebarStates _widthBasedState = NavBarSidebarStates.Expanded;
    private int _width;

    internal string NavBarClassNames { get; private set; } = string.Empty;

    internal bool IsExpandedOverlay { get; private set; }

    internal bool IsHidden { get; private set; }

    internal bool IsCollapsed { get; private set; }

    internal NavBarSidebarStates UserPreferredState { get; set; } = NavBarSidebarStates.Expanded;

    internal event EventHandler? OnStateChanged;

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
        if ((UserPreferredState & NavBarSidebarStates.ExpandedOverlay) == NavBarSidebarStates.ExpandedOverlay)
        {
            stateChanged = true;
            useTransition = false;
            UserPreferredState &= ~NavBarSidebarStates.ExpandedOverlay;
        }

        UpdateCssToApply(useTransition);
        return stateChanged;
    }

    internal void ToggleSidebar()
    {
        Guard.IsNotEqualTo((int)UserPreferredState, (int)NavBarSidebarStates.Hidden);

        bool useTransition = true;
        bool userCurrentlyPreferExpanded = (UserPreferredState & NavBarSidebarStates.Expanded) == NavBarSidebarStates.Expanded;
        switch (_widthBasedState)
        {
            // Based on the control's width, the default / automatic mode of the sidebar should be either collapsed or hidden.
            case NavBarSidebarStates.Hidden:
            case NavBarSidebarStates.Collapsed:
                if ((UserPreferredState & NavBarSidebarStates.ExpandedOverlay) == NavBarSidebarStates.ExpandedOverlay)
                {
                    // It looks like the sidebar is currently expanded overlay.
                    // User clicked on the toggle button, so let's collapse the sidebar.
                    UserPreferredState = NavBarSidebarStates.Collapsed;
                    useTransition = false;
                }
                else if (userCurrentlyPreferExpanded)
                {
                    // User preferred sidebar mode is expanded. Currently, it's collapsed
                    // or hidden due to the small width of the control. Let's expand overlay the sidebar.
                    UserPreferredState |= NavBarSidebarStates.ExpandedOverlay;
                }
                else
                {
                    // User preferred sidebar mode is collapsed. Let's switch it to expanded, and let's show the overlay.
                    UserPreferredState = NavBarSidebarStates.Expanded | NavBarSidebarStates.ExpandedOverlay;
                }
                break;

            // Based on the control's width, the default / automatic mode of the sidebar should be expanded.
            case NavBarSidebarStates.Expanded:
                if (userCurrentlyPreferExpanded)
                {
                    // Currently, user's preferred mode is expanded. But user just clicked on the
                    // toggle button, so let's switch to collapsed mode.
                    UserPreferredState = NavBarSidebarStates.Collapsed;
                }
                else
                {
                    // Opposite scenario.
                    UserPreferredState = NavBarSidebarStates.Expanded;
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
        if ((UserPreferredState & NavBarSidebarStates.ExpandedOverlay) == NavBarSidebarStates.ExpandedOverlay)
        {
            UserPreferredState &= ~NavBarSidebarStates.ExpandedOverlay;
            UpdateCssToApply(useTransition: false);
        }
    }

    internal void ForceExpand()
    {
        if (UserPreferredState == NavBarSidebarStates.Collapsed
            || _widthBasedState == NavBarSidebarStates.Collapsed
            || _widthBasedState == NavBarSidebarStates.Hidden)
        {
            ToggleSidebar();
        }
    }

    private void UpdateCssToApply(bool useTransition = false)
    {
        bool userRequestExpandOverlay = (UserPreferredState & NavBarSidebarStates.ExpandedOverlay) == NavBarSidebarStates.ExpandedOverlay;
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
                cssBuilder.AddClass("collapsed", when: UserPreferredState == NavBarSidebarStates.Collapsed);
                isCollapsed = UserPreferredState == NavBarSidebarStates.Collapsed;
                break;

            default:
                ThrowHelper.ThrowNotSupportedException();
                break;
        }

        NavBarClassNames = cssBuilder.ToString();
        IsExpandedOverlay = userRequestExpandOverlay;
        IsCollapsed = isCollapsed;
        IsHidden = _widthBasedState == NavBarSidebarStates.Hidden;
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }
}
