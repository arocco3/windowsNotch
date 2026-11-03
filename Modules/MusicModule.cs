using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.Media.Control;

namespace NotchApp.Modules;

/// <summary>Mostra il brano in riproduzione (Spotify, browser, ecc.) e controlla play/pausa/avanti/indietro.</summary>
public class MusicModule : INotchModule
{
    public string Name => "Musica";
    public event Action? Changed;

    private GlobalSystemMediaTransportControlsSessionManager? _manager;
    private GlobalSystemMediaTransportControlsSession? _session;
    private string _title = "Nessun brano";
    private string _artist = "";
    private bool _playing;
    private BitmapImage? _cover;

    public MusicModule() => _ = InitAsync();

    private async Task InitAsync()
    {
        try
        {
            _manager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
            _manager.CurrentSessionChanged += (_, _) => Attach();
            Attach();
        }
        catch { /* controlli multimediali non disponibili */ }
    }

    private void Attach()
    {
        if (_session != null)
        {
            _session.MediaPropertiesChanged -= OnMediaChanged;
            _session.PlaybackInfoChanged -= OnPlaybackChanged;
        }

        _session = _manager?.GetCurrentSession();

        if (_session != null)
        {
            _session.MediaPropertiesChanged += OnMediaChanged;
            _session.PlaybackInfoChanged += OnPlaybackChanged;
        }
        _ = RefreshAsync();
    }

    private void OnMediaChanged(GlobalSystemMediaTransportControlsSession s, MediaPropertiesChangedEventArgs e) => _ = RefreshAsync();
    private void OnPlaybackChanged(GlobalSystemMediaTransportControlsSession s, PlaybackInfoChangedEventArgs e) => _ = RefreshAsync();

    private async Task RefreshAsync()
    {
        var s = _session;
        if (s == null)
        {
            Update("", "", false, null);
            return;
        }

        try
        {
            var props = await s.TryGetMediaPropertiesAsync();
            var playing = s.GetPlaybackInfo()?.PlaybackStatus ==
                          GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
            Update(props.Title, props.Artist, playing, await LoadCoverAsync(props));
        }
        catch { /* la sessione può sparire mentre leggiamo */ }
    }

    private static async Task<BitmapImage?> LoadCoverAsync(GlobalSystemMediaTransportControlsSessionMediaProperties props)
    {
        if (props.Thumbnail == null) return null;
        try
        {
            using var stream = (await props.Thumbnail.OpenReadAsync()).AsStreamForRead();
            var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            ms.Position = 0;

            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.StreamSource = ms;
            bmp.EndInit();
            bmp.Freeze(); // utilizzabile dal thread della UI
            return bmp;
        }
        catch { return null; }
    }

    private void Update(string title, string artist, bool playing, BitmapImage? cover)
    {
        _title = string.IsNullOrWhiteSpace(title) ? "Nessun brano" : title;
        _artist = artist;
        _playing = playing;
        _cover = cover;
        Application.Current?.Dispatcher.BeginInvoke(new Action(() => Changed?.Invoke()));
    }

    // ---------- Viste ----------

    public UIElement CreateCompactView() => new TextBlock
    {
        Text = (_playing ? "♪ " : "") + _title,
        Foreground = Brushes.White,
        FontSize = 12,
        MaxWidth = 150,
        TextTrimming = TextTrimming.CharacterEllipsis
    };

    public UIElement CreateExpandedView()
    {
        var grid = new Grid { Width = 388 };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        grid.Children.Add(new Border
        {
            Width = 80,
            Height = 80,
            CornerRadius = new CornerRadius(12),
            Background = _cover != null
                ? new ImageBrush(_cover) { Stretch = Stretch.UniformToFill }
                : new SolidColorBrush(Color.FromRgb(38, 38, 38))
        });

        var info = new StackPanel { Margin = new Thickness(14, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
        Grid.SetColumn(info, 1);
        info.Children.Add(Label(_title, 15, Brushes.White, FontWeights.SemiBold));
        info.Children.Add(Label(_artist, 12, Brushes.Gray, FontWeights.Normal));

        var controls = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0) };
        controls.Children.Add(Icon("", 16, () => _ = _session?.TrySkipPreviousAsync()));
        controls.Children.Add(Icon(_playing ? "" : "", 20, () => _ = _session?.TryTogglePlayPauseAsync()));
        controls.Children.Add(Icon("", 16, () => _ = _session?.TrySkipNextAsync()));
        info.Children.Add(controls);

        grid.Children.Add(info);
        return grid;
    }

    private static TextBlock Label(string text, double size, Brush color, FontWeight weight) => new()
    {
        Text = text,
        FontSize = size,
        Foreground = color,
        FontWeight = weight,
        TextTrimming = TextTrimming.CharacterEllipsis
    };

    private static TextBlock Icon(string glyph, double size, Action onClick)
    {
        var t = new TextBlock
        {
            Text = glyph,
            FontFamily = new FontFamily("Segoe MDL2 Assets"),
            FontSize = size,
            Foreground = Brushes.White,
            Opacity = 0.85,
            Cursor = Cursors.Hand,
            Margin = new Thickness(0, 0, 24, 0)
        };
        t.MouseEnter += (_, _) => t.Opacity = 1;
        t.MouseLeave += (_, _) => t.Opacity = 0.85;
        t.MouseLeftButtonUp += (_, _) => onClick();
        return t;
    }
}
