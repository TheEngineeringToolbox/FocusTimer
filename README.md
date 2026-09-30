# FocusTimer

Een compacte Pomodoro-app voor Windows 10/11, gebouwd met C#, .NET 8 en WPF.

- Betrouwbare timestamp-gebaseerde timer
- Focus-, korte-pauze- en lange-pauzefases
- System-traybediening met dynamische voortgangsring
- Windows-taakbalkvoortgang en meldingen
- Instelbare duur, automatische overgang en Windows-startup
- Dagelijkse Pomodoro- en focusstatistiek
- Self-contained single-file publicatie voor Windows x64

De volledige bouw-, gebruiks- en publicatie-instructies staan in [FocusTimer/README.md](FocusTimer/README.md).

## Projecten

- `FocusTimer` — WPF-desktopapp
- `FocusTimer.Tests` — packagevrije regressie- en resource-tests

## Snel starten

```powershell
dotnet restore
dotnet build -c Release
dotnet run --project .\FocusTimer\FocusTimer.csproj
```

Standalone publiceren:

```powershell
dotnet publish .\FocusTimer\FocusTimer.csproj -c Release -r win-x64 --self-contained true
```
