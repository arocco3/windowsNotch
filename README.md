# WindowsNotch

Un "notch" in stile Mac per Windows: una pillola nera in cima allo schermo che si espande al passaggio del mouse e mostra moduli intercambiabili.

## Avvio

Requisiti: Windows 10 (2004+) o 11 e [.NET SDK](https://dotnet.microsoft.com/download) (il progetto usa .NET 10).

```bash
dotnet run
```

## Uso

- **Passa il mouse** sul notch per espanderlo.
- **Rotella del mouse** per cambiare modulo.
- **Trascina file** sul notch: finiscono nell'appoggio. Trascinali fuori per usarli, clic destro per rimuoverli.
- **Chiudi l'app** dall'icona nell'area di notifica (voce "Esci").

## Moduli

| Modulo | Cosa fa |
| --- | --- |
| Orologio | Ora corrente |
| Musica | Brano in riproduzione (Spotify, browser, ecc.) con play/pausa/avanti/indietro |
| File | Zona di appoggio per file trascinati |

## Aggiungere un modulo

1. Crea una classe in `Modules/` che implementa `INotchModule` (e `IFileDropModule` se deve ricevere file).
2. Registrala nella lista `_modules` in `MainWindow.xaml.cs`.
3. Se i dati cambiano, solleva l'evento `Changed` per ridisegnare la vista.

Esempi: `Modules/ClockModule.cs`, `Modules/MusicModule.cs`.

## Roadmap

- [x] Orologio
- [x] Musica (controllo multimediale di Windows)
- [x] Drag and drop di file
- [x] Icona nel system tray
- [ ] Calendario
- [ ] Batteria e notifiche
- [ ] Impostazioni e avvio automatico

## Licenza

[MIT](LICENSE)
