<!-- markdownlint-disable MD033 MD041 -->

[![Crowdin](https://badges.crowdin.net/playnite-extensions/localized.svg)](https://crowdin.com/project/playnite-extensions)
[![GitHub release](https://img.shields.io/github/v/release/Lacro59/playnite-checkdlc-plugin?logo=github&color=8A2BE2)](https://github.com/Lacro59/playnite-checkdlc-plugin/releases/latest)
[![GitHub Release Date](https://img.shields.io/github/release-date/Lacro59/playnite-checkdlc-plugin?logo=github)](https://github.com/Lacro59/playnite-checkdlc-plugin/releases/latest)
[![GitHub downloads](https://img.shields.io/github/downloads/Lacro59/playnite-checkdlc-plugin/total?logo=github)](https://github.com/Lacro59/playnite-checkdlc-plugin/releases)
[![GitHub commit activity](https://img.shields.io/github/commit-activity/m/Lacro59/playnite-checkdlc-plugin/devel?logo=github)](https://github.com/Lacro59/playnite-checkdlc-plugin/graphs/commit-activity)
[![GitHub contributors](https://img.shields.io/github/contributors/Lacro59/playnite-checkdlc-plugin?logo=github)](https://github.com/Lacro59/playnite-checkdlc-plugin/graphs/contributors)
[![GitHub license](https://img.shields.io/github/license/Lacro59/playnite-checkdlc-plugin?logo=github)](https://github.com/Lacro59/playnite-checkdlc-plugin/blob/main/LICENSE)

# CheckDlc for Playnite

Discover DLC for your library games, track what you own, and surface missing add-ons from supported digital stores directly inside [Playnite](https://playnite.link).

## ✨ Features

- **DLC catalog & ownership**: fetches DLC lists per game and marks owned vs. not owned, with prices where the store provides them.
- **Supported stores**: works with games imported through these Playnite library integrations:
  - **Steam** (Steam library)
  - **GOG** (GOG and GOG OSS libraries)
  - **Epic Games** (Epic and Legendary libraries)
  - **Origin / EA app** (Origin library)
  - **PlayStation Store** (PSN library, experimental)
  - **Nintendo eShop** (Nintendo library, Europe only, experimental)
- **Dedicated DLC view**: open a game's DLC panel from the game menu; filter by owned, free, hidden, or maximum price.
- **Library-wide tools**: bulk download DLC data, browse free DLC you do not own, export owned DLC to CSV, and refresh or clear cached data from the main menu.
- **Tags & notifications**: optionally tag games when you own all DLC; optional alerts when DLC prices change.
- **Manual control**: add DLC via Steam search, flag DLC as manually owned, maintain ignored titles, and attach DLC as a custom game feature.
- **Theme integration**: custom theme elements (button, full/owned/not-owned DLC lists) for game details and list views.

## 📸 Screenshots

### Main interface

<a href="https://github.com/Lacro59/playnite-checkdlc-plugin/blob/main/forum/main_01.jpg?raw=true">
  <picture>
    <img alt="CheckDlc main window showing DLC list and ownership for a selected game" src="https://github.com/Lacro59/playnite-checkdlc-plugin/blob/main/forum/main_01.jpg?raw=true" height="200px">
  </picture>
</a>

### Settings panel

<a href="https://github.com/Lacro59/playnite-checkdlc-plugin/blob/main/forum/settings_01.jpg?raw=true">
  <picture>
    <img alt="CheckDlc plugin settings with general options and store configuration tabs" src="https://github.com/Lacro59/playnite-checkdlc-plugin/blob/main/forum/settings_01.jpg?raw=true" height="200px">
  </picture>
</a>

## 🔍 Global Search

Not implemented

## ⚙️ Configuration

Open **Settings → Extensions → CheckDlc**.

### General behavior

- Enable or disable auto-import when the library updates.
- Configure tags when all DLC for a game are owned (including optional “all DLC” variant).
- Select a Playnite game feature to store DLC metadata.
- Enable price-change notifications for DLC you do not own.
- Set GOG and Origin currency for regional price display.

### Store / library settings

- Configure Steam, Epic, and GOG sign-in and access mode (including Steam API vs. authenticated access).
- Origin, PlayStation, and Nintendo rely on store pages/APIs according to your library import (no extra CheckDlc sign-in).

### Integration

- Enable or disable theme controls: DLC button, full list, owned-only list, and not-owned-only list.

### Lists

- Maintain ignored DLC titles to exclude them from import/display.
- Flag manually owned DLC when store ownership cannot be confirmed.

> Store matching uses your installed Playnite library plugins automatically, but signing in on the Steam, Epic, and GOG tabs and choosing the right access mode usually gives the most accurate ownership results.

## 📥 Installation

### Install from Playnite Add-ons Browser (recommended)

1. Open Playnite.
2. Go to **Add-ons → Browse → Generic**.
3. Search for `CheckDlc` and install it.
4. Restart Playnite if requested.

Official Playnite guide: [Installing Extensions](https://api.playnite.link/docs/manual/features/extensionsSupport/installingExtensions.html)

### Manual installation (`.pext`)

1. Download the latest `.pext` file from [Releases](https://github.com/Lacro59/playnite-checkdlc-plugin/releases/latest).
2. In Playnite, open **Add-ons → Install from file**.
3. Select the downloaded `.pext`.
4. Restart Playnite, then configure store logins under **Settings → Extensions → CheckDlc**.

## 🤝 Contributing & Feedback

- **Bug reports**: [Open an issue](https://github.com/Lacro59/playnite-checkdlc-plugin/issues/new?template=bug_report.md)
- **Feature requests**: [Request an enhancement](https://github.com/Lacro59/playnite-checkdlc-plugin/issues/new?template=feature_request.md)
- **Pull requests**: [Submit a PR](https://github.com/Lacro59/playnite-checkdlc-plugin/pulls) targeting the `devel` branch
- **Translations**: [Contribute on Crowdin](https://crowdin.com/project/playnite-extensions)
- **Wiki & troubleshooting**: [Project wiki](https://github.com/Lacro59/playnite-checkdlc-plugin/wiki)

## 💝 Support

[![Ko-fi](https://img.shields.io/badge/Ko--fi-Support-FF5E5B?logo=ko-fi&logoColor=white)](https://ko-fi.com/lacro59)

If this plugin helps you, you can also support:

- [Playnite](https://www.patreon.com/playnite)
- [SteamDB](https://steamdb.info/donate)

## 📄 License

This project is licensed under the [MIT License](https://github.com/Lacro59/playnite-checkdlc-plugin/blob/main/LICENSE).
