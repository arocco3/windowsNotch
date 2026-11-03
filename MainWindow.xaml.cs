using System.Windows;
using System.Windows.Animation;
using System.Windows.Input;
using NotchApp.Modules;

namespace NotchApp;

public partial class MainWindow : Window
{
    // Per aggiungere un modulo: crea la classe e inseriscila qui.
    private readonly List<INotchModule> _modules = new() { new ClockModule() };
    private int _current;
    private bool _expanded;

    public MainWindow()
    {
        InitializeComponent();
        Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
        Top = 0;
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

    private void OnEnter(object sender, MouseEventArgs e)
    {
        _expanded = true;
        Animate(420, 120);
        Render();
    }

    private void OnLeave(object sender, MouseEventArgs e)
    {
        _expanded = false;
        Animate(180, 32);
        Render();
    }

    // Rotella del mouse: cambia modulo
    private void OnWheel(object sender, MouseWheelEventArgs e)
    {
        _current = (_current + (e.Delta > 0 ? 1 : -1) + _modules.Count) % _modules.Count;
        Render();
    }
}
