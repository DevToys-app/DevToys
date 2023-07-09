using System.Runtime.Versioning;
using System.Windows;
using Windows.Win32;
using Windows.Win32.System.Threading;

namespace DevToys.Windows.Core;

[Export]
public sealed class EfficiencyModeService
{
    private readonly HashSet<Window> _activeWindows = new();

    internal event EventHandler? EfficiencyModeEnabled;

    internal event EventHandler? EfficiencyModeDisabled;

    internal void RegisterWindow(Window window)
    {
        Guard.IsNotNull(window);

        if (Environment.OSVersion.Version >= new Version(10, 0, 16299))
        {
#pragma warning disable CA1416 // Validate platform compatibility
            window.LostFocus += Window_LostFocus;

            if (window.IsFocused)
            {
                _activeWindows.Add(window);
            }

            window.GotFocus += Window_GotFocus;
            UpdateEfficiencyMode();
#pragma warning restore CA1416 // Validate platform compatibility
        }
    }

    internal void UnregisterWindow(Window window)
    {
        Guard.IsNotNull(window);

        if (Environment.OSVersion.Version >= new Version(10, 0, 16299))
        {
#pragma warning disable CA1416 // Validate platform compatibility
            window.LostFocus -= Window_LostFocus;
            window.GotFocus -= Window_GotFocus;
            _activeWindows.Remove(window);
            UpdateEfficiencyMode();
#pragma warning restore CA1416 // Validate platform compatibility
        }
    }

    [SupportedOSPlatform("windows10.0.16299.0")]
    private void Window_LostFocus(object sender, RoutedEventArgs e)
    {
        _activeWindows.Remove((Window)sender);
        UpdateEfficiencyMode();
    }

    [SupportedOSPlatform("windows10.0.16299.0")]
    private void Window_GotFocus(object sender, RoutedEventArgs e)
    {
        _activeWindows.Add((Window)sender);
        UpdateEfficiencyMode();
    }

    [SupportedOSPlatform("windows10.0.16299.0")]
    private void UpdateEfficiencyMode()
    {
        bool enableEfficiencyMode = _activeWindows.Count == 0;

        if (Environment.OSVersion.Platform == PlatformID.Win32NT &&
            Environment.OSVersion.Version >= new Version(10, 0, 16299))
        {
            SetEfficiencyMode(enableEfficiencyMode);
        }

        if (enableEfficiencyMode)
        {
            EfficiencyModeEnabled?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            EfficiencyModeDisabled?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Enables/disables efficient mode for process <br/>
    /// Based on: <see href="https://devblogs.microsoft.com/performance-diagnostics/reduce-process-interference-with-task-manager-efficiency-mode/"/> 
    /// </summary>
    /// <param name="value"></param>
    [SupportedOSPlatform("windows10.0.16299.0")]
    private static void SetEfficiencyMode(bool value)
    {
        bool isWindow11_OrGreater = Environment.OSVersion.Version >= new Version(10, 0, 22000);

#pragma warning disable CA1416 // Validate platform compatibility
        QualityOfServiceLevel ecoLevel = isWindow11_OrGreater ? QualityOfServiceLevel.Eco : QualityOfServiceLevel.Low;
#pragma warning restore CA1416 // Validate platform compatibility

        SetProcessQualityOfServiceLevel(value ? ecoLevel : QualityOfServiceLevel.Default);
        SetProcessPriorityClass(value ? ProcessPriorityClass.Idle : ProcessPriorityClass.Normal);
    }

    /// <summary>
    /// Based on <see href="https://docs.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-setprocessinformation"/>
    /// </summary>
    [SupportedOSPlatform("windows10.0.16299.0")]
    public static unsafe void SetProcessQualityOfServiceLevel(QualityOfServiceLevel level)
    {
        bool isWindow11_OrGreater = Environment.OSVersion.Version >= new Version(10, 0, 22000);

        var powerThrottling = new PROCESS_POWER_THROTTLING_STATE
        {
            Version = PInvoke.PROCESS_POWER_THROTTLING_CURRENT_VERSION
        };

        switch (level)
        {
            // Let system manage all power throttling. ControlMask is set to 0 as we don’t want 
            // to control any mechanisms.
            case QualityOfServiceLevel.Default:
                powerThrottling.ControlMask = 0;
                powerThrottling.StateMask = 0;
                break;

#pragma warning disable CA1416 // Validate platform compatibility
            // Turn EXECUTION_SPEED throttling on.
            // ControlMask selects the mechanism and StateMask declares which mechanism should be on or off.
            case QualityOfServiceLevel.Eco when isWindow11_OrGreater:
#pragma warning restore CA1416 // Validate platform compatibility
            case QualityOfServiceLevel.Low:
                powerThrottling.ControlMask = PInvoke.PROCESS_POWER_THROTTLING_EXECUTION_SPEED;
                powerThrottling.StateMask = PInvoke.PROCESS_POWER_THROTTLING_EXECUTION_SPEED;
                break;

            // Turn EXECUTION_SPEED throttling off. 
            // ControlMask selects the mechanism and StateMask is set to zero as mechanisms should be turned off.
            case QualityOfServiceLevel.High:
                powerThrottling.ControlMask = PInvoke.PROCESS_POWER_THROTTLING_EXECUTION_SPEED;
                powerThrottling.StateMask = 0;
                break;

            default:
                throw new NotImplementedException();
        }

        _ = PInvoke.SetProcessInformation(
            hProcess: PInvoke.GetCurrentProcess(),
            ProcessInformationClass: PROCESS_INFORMATION_CLASS.ProcessPowerThrottling,
            ProcessInformation: &powerThrottling,
            ProcessInformationSize: (uint)sizeof(PROCESS_POWER_THROTTLING_STATE));
    }

    /// <summary>
    /// Based on <see href="https://docs.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-setpriorityclass"/>
    /// </summary>
    [SupportedOSPlatform("windows5.1.2600")]
    public static unsafe void SetProcessPriorityClass(ProcessPriorityClass priorityClass)
    {
        PROCESS_CREATION_FLAGS flags = priorityClass switch
        {
            ProcessPriorityClass.Idle => PROCESS_CREATION_FLAGS.IDLE_PRIORITY_CLASS,
            ProcessPriorityClass.BelowNormal => PROCESS_CREATION_FLAGS.BELOW_NORMAL_PRIORITY_CLASS,
            ProcessPriorityClass.Normal => PROCESS_CREATION_FLAGS.NORMAL_PRIORITY_CLASS,
            ProcessPriorityClass.AboveNormal => PROCESS_CREATION_FLAGS.ABOVE_NORMAL_PRIORITY_CLASS,
            ProcessPriorityClass.High => PROCESS_CREATION_FLAGS.HIGH_PRIORITY_CLASS,
            ProcessPriorityClass.RealTime => PROCESS_CREATION_FLAGS.REALTIME_PRIORITY_CLASS,
            _ => throw new NotImplementedException(),
        };

        _ = PInvoke.SetPriorityClass(hProcess: PInvoke.GetCurrentProcess(), dwPriorityClass: flags);
    }
}
