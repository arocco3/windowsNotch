using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace NotchApp.Modules;

/// <summary>Modulo di esempio: orologio.</summary>
public class ClockModule : INotchModule
{
    public string Name => "Orologio";

    public UIElement CreateCompactView() => Build(14);
    public UIElement CreateExpandedView() => Build(36);

    private static TextBlock Build(double size)
    {
        var tb = new TextBlock
        {
            Foreground = Brushes.White,
            FontSize = size,
            FontWeight = FontWeights.SemiBold,
            Text = DateTime.Now.ToString("HH:mm")
        };
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        timer.Tick += (_, _) => tb.Text = DateTime.Now.ToString("HH:mm");
        timer.Start();
        tb.Unloaded += (_, _) => timer.Stop();
        return tb;
    }
}
