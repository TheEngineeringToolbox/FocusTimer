# Focus Timer

Een compacte Pomodoro-app voor Windows 10/11, gebouwd met C#, .NET 8 en WPF. De timer rekent met absolute UTC-timestamps en blijft daardoor correct na tijdelijke belasting of slaapstand.

## Vereisten

- Windows 10 versie 1809 of nieuwer, of Windows 11
- .NET 8 SDK om te bouwen (een nieuwere SDK die .NET 8 ondersteunt werkt ook)
- Internettoegang tot `nuget.org` bij de eerste self-contained restore/publish

Er zijn geen externe applicatiepackages. NuGet wordt alleen gebruikt voor de officiële Microsoft runtime-, Windows Desktop-, Windows SDK- en linkerpakken die een self-contained publicatie vereist.

## Herstellen, bouwen en starten

Voer vanuit de projectmap uit:

```powershell
dotnet restore
dotnet build
dotnet run
```

Release-build:

```powershell
dotnet build -c Release
```

Geautomatiseerde timer-tests:

```powershell
dotnet run --project ..\FocusTimer.Tests\FocusTimer.Tests.csproj
```

## Standalone publiceren

```powershell
dotnet publish -c Release -r win-x64 --self-contained true
```

De single-file executable staat in:

`bin\Release\net8.0-windows10.0.17763.0\win-x64\publish\FocusTimer.exe`

## Gebruik

- Space: starten of pauzeren
- R: huidige fase resetten
- N: naar de volgende fase
- Het tandwiel opent de instellingen.
- Minimaliseren houdt het venster zichtbaar in de Windows-taakbalk.
- Sluiten verbergt het venster alleen in de system tray wanneer “Sluiten minimaliseert naar system tray” actief is.
- Het tray-menu bevat Start/Pauze, Volgende fase, Open timer, Instellingen en Afsluiten.
- De tooltip toont fase en resterende tijd. De taakbalkknop toont voortgang zolang het venster zichtbaar is.
- Het tomaat-icoon wordt gebruikt voor de EXE, titelbalk, taakbalk en system tray.
- De ring om het tray-icoon toont de resterende fase: blauw voor focus, groen voor korte pauze en paars voor lange pauze. Tijdens pauze wordt de ring gedimd.

## Instellingen en statistiek

Instellingen: `%LOCALAPPDATA%\FocusTimer\settings.json`

Dagstatistiek: `%LOCALAPPDATA%\FocusTimer\daily-stats.json`

“Starten met Windows” gebruikt de sleutel `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` en vereist geen administratorrechten.

## Windows-meldingen

De app gebruikt `NotifyIcon.ShowBalloonTip`. Op Windows 10/11 presenteert de Windows-shell dit als een systeemmelding wanneer meldingen voor desktopapps zijn toegestaan. Deze aanpak is bewust gekozen omdat hij betrouwbaar werkt voor een unpackaged WPF-app en geen package identity, installer of externe notification-library nodig heeft.

Bekende beperking: Windows kan meldingen onderdrukken als Niet storen/Focus is ingeschakeld of meldingen voor desktopapps zijn uitgeschakeld. De melding krijgt zonder geïnstalleerde package identity geen eigen geavanceerde toast-acties.
