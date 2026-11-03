namespace NotchApp;

/// <summary>Icona nell'area di notifica con la voce "Esci".</summary>
public sealed class TrayIcon : IDisposable
{
    private readonly System.Windows.Forms.NotifyIcon _icon;

    public TrayIcon(Action onExit)
    {
        var menu = new System.Windows.Forms.ContextMenuStrip();
        menu.Items.Add("Esci", null, (_, _) => onExit());

        _icon = new System.Windows.Forms.NotifyIcon
        {
            Icon = System.Drawing.SystemIcons.Application,
            Text = "WindowsNotch",
            ContextMenuStrip = menu,
            Visible = true
        };
    }

    public void Dispose()
    {
        _icon.Visible = false;
        _icon.Dispose();
    }
}
