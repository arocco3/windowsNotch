using System.Windows;

namespace NotchApp.Modules;

/// <summary>Contratto che ogni modulo del notch deve implementare.</summary>
public interface INotchModule
{
    string Name { get; }
    UIElement CreateCompactView();   // vista a notch chiuso
    UIElement CreateExpandedView();  // vista a notch aperto
}
