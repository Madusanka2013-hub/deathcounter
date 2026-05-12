# DeathCounter

Ein anpassbarer DeathCounter für Streams, OBS und Chroma-Key-Setups.

GitHub-Repository:
[github.com/Madusanka2013-hub/deathcounter](https://github.com/Madusanka2013-hub/deathcounter)

## Download

Fertige Versionen findest du unter:

[GitHub Releases](https://github.com/Madusanka2013-hub/deathcounter/releases)

## Vorschau

<img src="img/1.png" alt="DeathCounter Hauptansicht" width="700">

<img src="img/2.png" alt="DeathCounter Einstellungen" width="700">

<img src="img/3.png" alt="DeathCounter About-Fenster" width="700">

<img src="img/4.png" alt="DeathCounter weitere Ansicht" width="700">

## Lizenz

Dieses Projekt ist Open Source und steht unter der **GNU General Public License v3.0**.

Die vollständigen Lizenzbedingungen findest du in der Datei [LICENSE](./LICENSE).

## Hinweis

Wenn du dieses Projekt nutzt, weitergibst oder als Grundlage für eigene Anpassungen verwendest, freue ich mich über eine Verlinkung auf dieses Repository:

[github.com/Madusanka2013-hub/deathcounter](https://github.com/Madusanka2013-hub/deathcounter)

## Sicherheitsprüfung

Die veröffentlichte Datei wurde zusätzlich über VirusTotal geprüft:

[VirusTotal-Scan öffnen](https://www.virustotal.com/gui/file/ab2f54220e73a0861ead50421fb07653d97a4a77b3ebac201c938dffd2cb886d?nocache=1)

## Credits

- Idee und Impulse: [Casjopaja_](https://www.twitch.tv/casjopaja_)
- Umsetzung und Entwicklung: [Shinkaiyo](https://www.twitch.tv/shinkaiyo)

## Features

- Counter mit globalen Hotkeys
- Unterstützung für Tastatur, Tastenkombinationen und Maustasten
- Mehrere Counter-Dateien
- Portabler Betrieb pro Ordner
- OBS- und Chroma-Key-tauglicher Hintergrund
- Zufällige Soundausgabe bei Counter-Erhöhung
- Unterstützung für MP3 und WAV
- Anpassbare Schrift, Farben, Outline und Animation

## Technik

- .NET 8
- Windows Forms
- NAudio

## Build

```powershell
dotnet build
```

## Release / Einzeldatei

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```
