using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NotchApp.Modules;

/// <summary>Zona di appoggio: trascina file sul notch, poi trascinali fuori dove ti servono.</summary>
public class FilesModule : IFileDropModule
{
    private const int MaxShown = 4;
    private readonly List<string> _files = new();

    public string Name => "File";
    public event Action? Changed;

    public void AddFiles(IEnumerable<string> paths)
    {
        foreach (var p in paths)
            if (!_files.Contains(p, StringComparer.OrdinalIgnoreCase))
                _files.Add(p);
        Changed?.Invoke();
    }

    private void Prune() => _files.RemoveAll(p => !File.Exists(p) && !Directory.Exists(p));

    // ---------- Viste ----------

    public UIElement CreateCompactView()
    {
        Prune();
        return new TextBlock
        {
            Text = _files.Count == 0 ? "Appoggio file" : $"{_files.Count} file",
            Foreground = Brushes.White,
            FontSize = 12
        };
    }

    public UIElement CreateExpandedView()
    {
        Prune();
        var root = new StackPanel { Width = 388 };

        var header = new Grid { Margin = new Thickness(0, 0, 0, 10) };
        header.Children.Add(Label(_files.Count == 0 ? "Appoggio file" : $"Appoggio · {_files.Count}", 13, FontWeights.SemiBold));
        if (_files.Count > 0)
        {
            var clear = Label("Svuota", 12, FontWeights.Normal);
            clear.HorizontalAlignment = HorizontalAlignment.Right;
            clear.Opacity = 0.7;
            clear.Cursor = Cursors.Hand;
            clear.MouseLeftButtonUp += (_, _) => { _files.Clear(); Changed?.Invoke(); };
            header.Children.Add(clear);
        }
        root.Children.Add(header);

        if (_files.Count == 0)
        {
            var hint = Label("Rilascia qui i file per tenerli a portata di mano", 12, FontWeights.Normal);
            hint.Foreground = Brushes.Gray;
            root.Children.Add(new Border
            {
                Height = 52,
                CornerRadius = new CornerRadius(10),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Color.FromRgb(70, 70, 70)),
                Child = hint
            });
            hint.HorizontalAlignment = HorizontalAlignment.Center;
            hint.VerticalAlignment = VerticalAlignment.Center;
            return root;
        }

        var row = new StackPanel { Orientation = Orientation.Horizontal };
        foreach (var path in _files.Take(MaxShown))
            row.Children.Add(Tile(path));
        if (_files.Count > MaxShown)
        {
            var more = Label($"+{_files.Count - MaxShown}", 13, FontWeights.Normal);
            more.VerticalAlignment = VerticalAlignment.Center;
            more.Margin = new Thickness(6, 0, 0, 0);
            row.Children.Add(more);
        }
        root.Children.Add(row);
        return root;
    }

    private UIElement Tile(string path)
    {
        var panel = new StackPanel
        {
            Width = 84,
            Background = Brushes.Transparent,
            Cursor = Cursors.Hand,
            ToolTip = $"{path}\nTrascina fuori per usarlo · clic destro per rimuovere"
        };

        var iconSource = IconFor(path);
        panel.Children.Add(iconSource != null
            ? new Image { Source = iconSource, Width = 32, Height = 32 }
            : new TextBlock
            {
                Text = "",
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                FontSize = 28,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center
            });

        var name = Path.GetFileName(path);
        var label = Label(string.IsNullOrEmpty(name) ? path : name, 11, FontWeights.Normal);
        label.MaxWidth = 78;
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.Margin = new Thickness(0, 4, 0, 0);
        panel.Children.Add(label);

        // Trascina il file fuori dal notch (dopo una piccola soglia di movimento)
        Point start = default;
        panel.PreviewMouseLeftButtonDown += (_, e) => start = e.GetPosition(null);
        panel.MouseMove += (_, e) =>
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            var d = e.GetPosition(null) - start;
            if (Math.Abs(d.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(d.Y) < SystemParameters.MinimumVerticalDragDistance) return;
            DragDrop.DoDragDrop(panel, new DataObject(DataFormats.FileDrop, new[] { path }), DragDropEffects.Copy);
        };

        panel.MouseRightButtonUp += (_, _) => { _files.Remove(path); Changed?.Invoke(); };
        return panel;
    }

    private static ImageSource? IconFor(string path)
    {
        try
        {
            using var icon = System.Drawing.Icon.ExtractAssociatedIcon(path);
            if (icon == null) return null;
            var src = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            src.Freeze();
            return src;
        }
        catch { return null; }
    }

    private static TextBlock Label(string text, double size, FontWeight weight) => new()
    {
        Text = text,
        FontSize = size,
        FontWeight = weight,
        Foreground = Brushes.White,
        TextTrimming = TextTrimming.CharacterEllipsis
    };
}
