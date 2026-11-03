using System.Windows;

namespace NotchApp.Modules;

/// <summary>Contratto che ogni modulo del notch deve implementare.</summary>
public interface INotchModule
{
    string Name { get; }

    /// <summary>Da sollevare quando i dati cambiano e la vista va ridisegnata.</summary>
    event Action? Changed { add { } remove { } }

    UIElement CreateCompactView();   // vista a notch chiuso
    UIElement CreateExpandedView();  // vista a notch aperto
}

/// <summary>Modulo che può ricevere file trascinati sul notch.</summary>
public interface IFileDropModule : INotchModule
{
    void AddFiles(IEnumerable<string> paths);
}
