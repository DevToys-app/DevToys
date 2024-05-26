using System.Reflection;
using DevToys.Blazor.Components;
using Markdig;
using Microsoft.AspNetCore.Components.Web;

namespace DevToys.Blazor.Pages.Dialogs;

public partial class WhatsNewDialog : StyledComponentBase
{
    private Dialog _dialog = default!;

#pragma warning disable CA1822 // Mark members as static
    internal string Version
    {
        get
        {
            var assemblyInformationalVersion = (AssemblyInformationalVersionAttribute)Assembly.GetExecutingAssembly().GetCustomAttribute(typeof(AssemblyInformationalVersionAttribute))!;
            return assemblyInformationalVersion.InformationalVersion;
        }
    }

    internal string ReleaseNotesHtml
    {
        get
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = "DevToys.Blazor.Assets.changelog.md";

            using Stream stream = assembly.GetManifestResourceStream(resourceName)!;
            using var reader = new StreamReader(stream);
            string markdown = reader.ReadToEnd();
            string[] parts = markdown.Split(new[] { "---" }, StringSplitOptions.RemoveEmptyEntries);
            return Markdown.ToHtml(parts[1]);
        }
    }
#pragma warning restore CA1822 // Mark members as static

    internal Task OpenAsync()
    {
        _dialog.TryOpen();
        return _dialog.WaitUntilClosedAsync();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);
    }

    private void OnContinueButtonClick(MouseEventArgs ev)
    {
        _dialog.Close();
    }
}
