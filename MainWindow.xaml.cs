using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using NotchApp.Modules;

namespace NotchApp;

public partial class MainWindow : Window
{
    // Per aggiungere un modulo: crea la classe e inseriscila qui.
    private readonly List<INotchModule> _modules = new()
    {
        new ClockModule(),
        new MusicModule(),
        new FilesModule(),
    };

    private readonly TrayIcon _tray;
    private int _current;
    private bool _expanded;

    public MainWindow()
    {
        InitializeComponent();
        Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
        Top = 0;

        // Quando un modulo cambia dati, ridisegna (solo se è quello visibile)
        foreach (var m in _modules)
        {
            var module = m;
            module.Changed += () => { if (_modules[_current] == module) Render(); };
        }

        _tray = new TrayIcon(() => Application.Current.Shutdown());
        Closed += (_, _) => _tray.Dispose();

        Render();
    }

    private void Render()
    {
        var m = _modules[_current];
        Host.Content = _expanded ? m.CreateExpandedView() : m.CreateCompactView();
    }

    private void Animate(double w, double h)
    {
        var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
        var d = TimeSpan.FromMilliseconds(220);
        Pill.BeginAnimation(WidthProperty, new DoubleAnimation(w, d) { EasingFunction = ease });
        Pill.BeginAnimation(HeightProperty, new DoubleAnimation(h, d) { EasingFunction = ease });
    }

    private void Expand()
    {
        _expanded = true;
        Animate(420, 120);
        Render();
    }

    private void Collapse()
    {
        _expanded = false;
        Animate(70, 10);
        Render();
    }

    private void OnEnter(object sender, MouseEventArgs e) => Expand();
    private void OnLeave(object sender, MouseEventArgs e) => Collapse();

    // Rotella del mouse: cambia modulo
    private void OnWheel(object sender, MouseWheelEventArgs e)
    {
        _current = (_current + (e.Delta > 0 ? 1 : -1) + _modules.Count) % _modules.Count;
        Render();
    }

    // ---------- Drag and drop di file ----------

    private void OnDragEnter(object sender, DragEventArgs e)
    {
        e.Handled = true;
        if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
        e.Effects = DragDropEffects.Copy;

        // DragEnter risale anche dagli elementi figli: se già pronto, non ridisegnare
        if (_expanded && _modules[_current] is IFileDropModule) return;

        var i = _modules.FindIndex(m => m is IFileDropModule);
        if (i >= 0) _current = i;
        Expand();
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private void OnDragLeave(object sender, DragEventArgs e)
    {
        // Chiudi solo se il cursore è davvero uscito dal notch
        var p = e.GetPosition(Pill);
        if (p.X < 0 || p.Y < 0 || p.X > Pill.ActualWidth || p.Y > Pill.ActualHeight)
            Collapse();
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(DataFormats.FileDrop) is string[] paths &&
            _modules[_current] is IFileDropModule target)
            target.AddFiles(paths);
        e.Handled = true;
    }
}
