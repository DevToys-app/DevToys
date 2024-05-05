using System.ComponentModel;
using DevToys.Blazor.Components;

namespace DevToys.UnitTests.Blazor.Components.NavBar;

public class NavBarStateTests
{
    private const int NavBarWidthSidebarHiddenThreshold = 640;
    private const int NavBarWidthSidebarCollapseThreshold = 1008;
    private readonly NavBarState _state = new();
    private int _onStateChangedInvokeCount;

    public NavBarStateTests()
    {
        _state.OnStateChanged += NavBarState_OnStateChanged;
    }

    [Fact]
    [Description("By default, the nav bar is expanded.")]
    public void Default()
    {
        _state.IsExpandedOverlay.Should().BeFalse();
        _state.IsHidden.Should().BeFalse();
        _state.IsCollapsed.Should().BeFalse();
        _onStateChangedInvokeCount.Should().Be(0);
    }

    [Fact]
    [Description("When the nav bar is hidden, making the window wider will turn the nav bar into collapsed mode.")]
    public void FromHiddenToCollapsed()
    {
        _state.WidthUpdated(NavBarWidthSidebarHiddenThreshold - 1, NavBarWidthSidebarHiddenThreshold, NavBarWidthSidebarCollapseThreshold);
        _onStateChangedInvokeCount.Should().Be(1);
        _state.IsExpandedOverlay.Should().BeFalse();
        _state.IsHidden.Should().BeTrue();
        _state.IsCollapsed.Should().BeTrue();

        _state.WidthUpdated(NavBarWidthSidebarHiddenThreshold, NavBarWidthSidebarHiddenThreshold, NavBarWidthSidebarCollapseThreshold);
        _onStateChangedInvokeCount.Should().Be(2);
        _state.IsExpandedOverlay.Should().BeFalse();
        _state.IsHidden.Should().BeTrue();
        _state.IsCollapsed.Should().BeTrue();

        _state.WidthUpdated(NavBarWidthSidebarHiddenThreshold + 1, NavBarWidthSidebarHiddenThreshold, NavBarWidthSidebarCollapseThreshold);
        _onStateChangedInvokeCount.Should().Be(3);
        _state.IsExpandedOverlay.Should().BeFalse();
        _state.IsHidden.Should().BeFalse();
        _state.IsCollapsed.Should().BeTrue();
    }

    [Fact]
    [Description("When the nav bar is collapsed, making the window smaller will turn the nav bar into hidden mode.")]
    public void FromCollapsedToHidden()
    {
        _state.WidthUpdated(NavBarWidthSidebarHiddenThreshold + 1, NavBarWidthSidebarHiddenThreshold, NavBarWidthSidebarCollapseThreshold);
        _onStateChangedInvokeCount.Should().Be(1);
        _state.IsExpandedOverlay.Should().BeFalse();
        _state.IsHidden.Should().BeFalse();
        _state.IsCollapsed.Should().BeTrue();

        _state.WidthUpdated(NavBarWidthSidebarHiddenThreshold, NavBarWidthSidebarHiddenThreshold, NavBarWidthSidebarCollapseThreshold);
        _onStateChangedInvokeCount.Should().Be(2);
        _state.IsExpandedOverlay.Should().BeFalse();
        _state.IsHidden.Should().BeTrue();
        _state.IsCollapsed.Should().BeTrue();

        _state.WidthUpdated(NavBarWidthSidebarHiddenThreshold - 1, NavBarWidthSidebarHiddenThreshold, NavBarWidthSidebarCollapseThreshold);
        _onStateChangedInvokeCount.Should().Be(3);
        _state.IsExpandedOverlay.Should().BeFalse();
        _state.IsHidden.Should().BeTrue();
        _state.IsCollapsed.Should().BeTrue();
    }

    [Fact]
    [Description("When the nav bar is collapsed, making the window wider will turn the nav bar into expanded mode.")]
    public void FromCollapsedToExpanded()
    {
        _state.WidthUpdated(NavBarWidthSidebarCollapseThreshold - 1, NavBarWidthSidebarHiddenThreshold, NavBarWidthSidebarCollapseThreshold);
        _onStateChangedInvokeCount.Should().Be(1);
        _state.IsExpandedOverlay.Should().BeFalse();
        _state.IsHidden.Should().BeFalse();
        _state.IsCollapsed.Should().BeTrue();

        _state.WidthUpdated(NavBarWidthSidebarCollapseThreshold, NavBarWidthSidebarHiddenThreshold, NavBarWidthSidebarCollapseThreshold);
        _onStateChangedInvokeCount.Should().Be(2);
        _state.IsExpandedOverlay.Should().BeFalse();
        _state.IsHidden.Should().BeFalse();
        _state.IsCollapsed.Should().BeTrue();

        _state.WidthUpdated(NavBarWidthSidebarCollapseThreshold + 1, NavBarWidthSidebarHiddenThreshold, NavBarWidthSidebarCollapseThreshold);
        _onStateChangedInvokeCount.Should().Be(3);
        _state.IsExpandedOverlay.Should().BeFalse();
        _state.IsHidden.Should().BeFalse();
        _state.IsCollapsed.Should().BeFalse();
    }

    [Fact]
    [Description("When the nav bar is expanded, making the window smaller will turn the nav bar into collapsed mode.")]
    public void FromExpandedToCollapsed()
    {
        _state.WidthUpdated(NavBarWidthSidebarCollapseThreshold + 1, NavBarWidthSidebarHiddenThreshold, NavBarWidthSidebarCollapseThreshold);
        _onStateChangedInvokeCount.Should().Be(1);
        _state.IsExpandedOverlay.Should().BeFalse();
        _state.IsHidden.Should().BeFalse();
        _state.IsCollapsed.Should().BeFalse();

        _state.WidthUpdated(NavBarWidthSidebarCollapseThreshold, NavBarWidthSidebarHiddenThreshold, NavBarWidthSidebarCollapseThreshold);
        _onStateChangedInvokeCount.Should().Be(2);
        _state.IsExpandedOverlay.Should().BeFalse();
        _state.IsHidden.Should().BeFalse();
        _state.IsCollapsed.Should().BeTrue();

        _state.WidthUpdated(NavBarWidthSidebarCollapseThreshold - 1, NavBarWidthSidebarHiddenThreshold, NavBarWidthSidebarCollapseThreshold);
        _onStateChangedInvokeCount.Should().Be(3);
        _state.IsExpandedOverlay.Should().BeFalse();
        _state.IsHidden.Should().BeFalse();
        _state.IsCollapsed.Should().BeTrue();
    }

    [Fact]
    [Description("When the nav bar is expanded, clicking the hamburger menu will collapse the nav bar. Clicking it again will expand it.")]
    public void FromExpandedToCollapsedExplicitly()
    {
        _state.WidthUpdated(NavBarWidthSidebarCollapseThreshold + 1, NavBarWidthSidebarHiddenThreshold, NavBarWidthSidebarCollapseThreshold);
        _onStateChangedInvokeCount.Should().Be(1);
        _state.IsExpandedOverlay.Should().BeFalse();
        _state.IsHidden.Should().BeFalse();
        _state.IsCollapsed.Should().BeFalse();

        _state.ToggleSidebar();
        _onStateChangedInvokeCount.Should().Be(2);
        _state.IsExpandedOverlay.Should().BeFalse();
        _state.IsHidden.Should().BeFalse();
        _state.IsCollapsed.Should().BeTrue();

        _state.ToggleSidebar();
        _onStateChangedInvokeCount.Should().Be(3);
        _state.IsExpandedOverlay.Should().BeFalse();
        _state.IsHidden.Should().BeFalse();
        _state.IsCollapsed.Should().BeFalse();
    }

    [Fact]
    [Description("When the nav bar is expanded, clicking the hamburger menu will collapse the nav bar. Making the window smaller or larger will keep the nav bar in hidden or collapsed mode")]
    public void FromExpandedToCollapsedExplicitlyResizeKeepItCollapsed()
    {
        _state.WidthUpdated(NavBarWidthSidebarCollapseThreshold + 1, NavBarWidthSidebarHiddenThreshold, NavBarWidthSidebarCollapseThreshold);
        _state.ToggleSidebar();
        _onStateChangedInvokeCount.Should().Be(2);
        _state.IsExpandedOverlay.Should().BeFalse();
        _state.IsHidden.Should().BeFalse();
        _state.IsCollapsed.Should().BeTrue();

        _state.WidthUpdated(NavBarWidthSidebarCollapseThreshold - 1, NavBarWidthSidebarHiddenThreshold, NavBarWidthSidebarCollapseThreshold);
        _state.WidthUpdated(NavBarWidthSidebarCollapseThreshold + 1, NavBarWidthSidebarHiddenThreshold, NavBarWidthSidebarCollapseThreshold);
        _onStateChangedInvokeCount.Should().Be(4);
        _state.IsExpandedOverlay.Should().BeFalse();
        _state.IsHidden.Should().BeFalse();
        _state.IsCollapsed.Should().BeTrue();
    }

    [Fact]
    [Description("When the nav bar is collapsed, clicking the hamburger menu will open the expanded overlay experience.")]
    public void FromCollapsedToExpandedOverlay()
    {
        _state.WidthUpdated(NavBarWidthSidebarCollapseThreshold - 1, NavBarWidthSidebarHiddenThreshold, NavBarWidthSidebarCollapseThreshold);
        _onStateChangedInvokeCount.Should().Be(1);
        _state.IsExpandedOverlay.Should().BeFalse();
        _state.IsHidden.Should().BeFalse();
        _state.IsCollapsed.Should().BeTrue();

        _state.ToggleSidebar();
        _onStateChangedInvokeCount.Should().Be(2);
        _state.IsExpandedOverlay.Should().BeTrue();
        _state.IsHidden.Should().BeFalse();
        _state.IsCollapsed.Should().BeFalse();

        _state.ToggleSidebar();
        _onStateChangedInvokeCount.Should().Be(3);
        _state.IsExpandedOverlay.Should().BeFalse();
        _state.IsHidden.Should().BeFalse();
        _state.IsCollapsed.Should().BeTrue();
    }

    [Fact]
    [Description("When the nav bar has the expanded overlay opened, resizing the window will close the overlay and switch back to collapsed mode. Making the window larger will switch back to regular expanded mode.")]
    public void ExpandedOverlayClosesBackToCollapsedWhenResizingWindow()
    {
        _state.WidthUpdated(NavBarWidthSidebarCollapseThreshold - 1, NavBarWidthSidebarHiddenThreshold, NavBarWidthSidebarCollapseThreshold);
        _state.ToggleSidebar();
        _onStateChangedInvokeCount.Should().Be(2);
        _state.IsExpandedOverlay.Should().BeTrue();
        _state.IsHidden.Should().BeFalse();
        _state.IsCollapsed.Should().BeFalse();

        _state.WidthUpdated(NavBarWidthSidebarCollapseThreshold - 2, NavBarWidthSidebarHiddenThreshold, NavBarWidthSidebarCollapseThreshold);
        _onStateChangedInvokeCount.Should().Be(3);
        _state.IsExpandedOverlay.Should().BeFalse();
        _state.IsHidden.Should().BeFalse();
        _state.IsCollapsed.Should().BeTrue();

        _state.WidthUpdated(NavBarWidthSidebarCollapseThreshold + 1, NavBarWidthSidebarHiddenThreshold, NavBarWidthSidebarCollapseThreshold);
        _onStateChangedInvokeCount.Should().Be(4);
        _state.IsExpandedOverlay.Should().BeFalse();
        _state.IsHidden.Should().BeFalse();
        _state.IsCollapsed.Should().BeFalse();
    }

    [Fact]
    [Description("When the nav bar has the expanded overlay opened, resizing the window will close the overlay and switch back to hidden mode. Making the window larger will switch back to regular collapsed mode.")]
    public void ExpandedOverlayClosesBackToHiddenWhenResizingWindow()
    {
        _state.WidthUpdated(NavBarWidthSidebarHiddenThreshold - 1, NavBarWidthSidebarHiddenThreshold, NavBarWidthSidebarCollapseThreshold);
        _state.ToggleSidebar();
        _onStateChangedInvokeCount.Should().Be(2);
        _state.IsExpandedOverlay.Should().BeTrue();
        _state.IsHidden.Should().BeTrue();
        _state.IsCollapsed.Should().BeFalse();

        _state.WidthUpdated(NavBarWidthSidebarHiddenThreshold - 2, NavBarWidthSidebarHiddenThreshold, NavBarWidthSidebarCollapseThreshold);
        _onStateChangedInvokeCount.Should().Be(3);
        _state.IsExpandedOverlay.Should().BeFalse();
        _state.IsHidden.Should().BeTrue();
        _state.IsCollapsed.Should().BeTrue();

        _state.WidthUpdated(NavBarWidthSidebarHiddenThreshold + 1, NavBarWidthSidebarHiddenThreshold, NavBarWidthSidebarCollapseThreshold);
        _onStateChangedInvokeCount.Should().Be(4);
        _state.IsExpandedOverlay.Should().BeFalse();
        _state.IsHidden.Should().BeFalse();
        _state.IsCollapsed.Should().BeTrue();
    }

    [Fact]
    [Description("When the nav bar has the expanded overlay opened, clicking the hamburger menu again is equivalent to collapsing the nav bar. Making the window larger won't make it switch to expanded mode..")]
    public void ExpandedOverlayForceClosedCauseNotToExpandWhenEnlargingWindow()
    {
        Default();
        _state.WidthUpdated(NavBarWidthSidebarCollapseThreshold - 1, NavBarWidthSidebarHiddenThreshold, NavBarWidthSidebarCollapseThreshold);
        _state.ToggleSidebar();
        _onStateChangedInvokeCount.Should().Be(2);
        _state.IsExpandedOverlay.Should().BeTrue();
        _state.IsHidden.Should().BeFalse();
        _state.IsCollapsed.Should().BeFalse();

        _state.ToggleSidebar();
        _state.IsExpandedOverlay.Should().BeFalse();
        _state.IsHidden.Should().BeFalse();
        _state.IsCollapsed.Should().BeTrue();

        _state.WidthUpdated(NavBarWidthSidebarCollapseThreshold + 1, NavBarWidthSidebarHiddenThreshold, NavBarWidthSidebarCollapseThreshold);
        _onStateChangedInvokeCount.Should().Be(4);
        _state.IsExpandedOverlay.Should().BeFalse();
        _state.IsHidden.Should().BeFalse();
        _state.IsCollapsed.Should().BeTrue();
    }

    [Fact]
    [Description("When the nav bar has the expanded overlay opened, clicking outside of the overlay area will close the overlay. Making the window larger will switch back to regular expanded mode.")]
    public void ExpandedOverlayClickOutsideOfNavBar()
    {
        _state.WidthUpdated(NavBarWidthSidebarCollapseThreshold - 1, NavBarWidthSidebarHiddenThreshold, NavBarWidthSidebarCollapseThreshold);
        _state.ToggleSidebar();
        _onStateChangedInvokeCount.Should().Be(2);
        _state.IsExpandedOverlay.Should().BeTrue();
        _state.IsHidden.Should().BeFalse();
        _state.IsCollapsed.Should().BeFalse();

        _state.CloseExpandedOverlay();
        _state.IsExpandedOverlay.Should().BeFalse();
        _state.IsHidden.Should().BeFalse();
        _state.IsCollapsed.Should().BeTrue();

        _state.WidthUpdated(NavBarWidthSidebarCollapseThreshold + 1, NavBarWidthSidebarHiddenThreshold, NavBarWidthSidebarCollapseThreshold);
        _onStateChangedInvokeCount.Should().Be(4);
        _state.IsExpandedOverlay.Should().BeFalse();
        _state.IsHidden.Should().BeFalse();
        _state.IsCollapsed.Should().BeFalse();
    }

    private void NavBarState_OnStateChanged(object sender, EventArgs e)
    {
        _onStateChangedInvokeCount++;
    }
}
