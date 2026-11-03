# WindowsNotch

Un "notch" in stile Mac per Windows: una pillola nera in cima allo schermo che si espande al passaggio del mouse e mostra moduli intercambiabili.

## Avvio

Requisiti: Windows 10/11 e [.NET 8 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet run
```

## Uso

- **Passa il mouse** sul notch per espanderlo.
- **Rotella del mouse** per cambiare modulo.

## Aggiungere un modulo

1. Crea una classe in `Modules/` che implementa `INotchModule`.
2. Registrala nella lista `_modules` in `MainWindow.xaml.cs`.

Esempio: vedi `Modules/ClockModule.cs`.

## Roadmap

- [ ] Musica (controllo multimediale di Windows)
- [ ] Calendario
- [ ] Drag and drop di file
- [ ] Batteria e notifiche
- [ ] Impostazioni e avvio automatico

## Licenza

[MIT](LICENSE)
