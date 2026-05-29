# Checklist - CheckDlc alignment (PluginCommon / HowLongToBeat pattern)

> Scope: apply in this repository the same evolutions already made in  
> `playnite-howlongtobeat-plugin/source` (and SystemChecker / GameActivity where relevant),  
> including required wiring in `playnite-plugincommon` when the build depends on it.

## 1) Database class renaming (legacy -> new base)

- [x] Replace `PluginDataBaseGameBase` with `PluginGameEntry` in:
  - [x] `source/Controls/PluginButton.xaml.cs`
  - [x] `source/Controls/PluginListDlc.xaml.cs`
  - [N/A] `source/Controls/PluginProgressBar.xaml.cs` (control not used by CheckDlc)
  - [N/A] `source/Controls/PluginViewItem.xaml.cs` (control not used by CheckDlc)
- [x] Replace `PluginDataBaseGame<...>` with `PluginGameCollection<...>` in:
  - [x] `source/Models/GameDlc.cs`
- [x] Check whether any `PluginDataBaseGameDetails<..., ...>` references remain and migrate them to `PluginGameCollectionWithDetails<..., ...>` if present (none in CheckDlc plugin code)
- [x] Ensure there are no remaining `PluginDataBaseGame*` occurrences in `source` (excluding `playnite-plugincommon`)
- [x] Remove legacy custom collection `source/Models/CheckDlcCollection.cs` (handled by `PluginDatabaseObject` / `PluginItemCollection`)

## 2) Core architecture alignment

- [x] Fix `SettingsRoot` to point to `PluginSettingsViewModel.Settings`
  - [x] File: `source/CheckDlc.cs`
- [x] Migrate `CheckDlcSettingsViewModel` to `PluginSettingsViewModel` + `IPluginSettingsViewModel`
  - [x] File: `source/CheckDlcSettings.cs`
- [x] Pass plugin name to base constructor: `base(api, "CheckDlc")`
  - [x] File: `source/CheckDlc.cs`
- [x] Migrate database base signature: `PluginDatabaseObject<CheckDlcSettings, GameDlc, Dlc>`
  - [x] File: `source/Services/CheckDlcDatabase.cs`
- [x] Replace `PluginDatabase.PluginSettings.Settings` with `PluginDatabase.PluginSettings` (settings model directly on database)
  - [x] Views, clients, models, controls
- [x] Extract menu logic from `CheckDlc.cs` into `CheckDlcMenus`
  - [x] `GetGameMenuItems(...)`
  - [x] `GetMainMenuItems(...)`
  - [x] Initialize `_menus` in the plugin constructor
- [x] Introduce `CheckDlcWindows` and centralize plugin window opening
  - [x] Custom theme button (`OnCustomThemeButtonClick` → `PluginWindows.ShowPluginGameDataWindow`)
  - [x] Game menu “View DLC”
  - [x] Main menu “Free unowned DLC” (`ShowFreeDlcWindow`)
- [x] Align theme controls with shared control pattern (`OnLoaded`, `AttachStaticEvents`, `DatabaseItemUpdated`)
  - [x] `source/Controls/PluginButton.xaml.cs`
  - [x] `source/Controls/PluginListDlc.xaml.cs`

## 3) Export / shared tools

- [x] Add `CheckDlcExport` (`PluginExportCsv<GameDlc>`)
  - [x] File: `source/Services/CheckDlcExport.cs`
- [x] Wire `PluginExportCsv` in `CheckDlcDatabase` constructor
- [x] Expose CSV export in main menu via `Database.ExtractToCsv()` (shared dialog)
- [x] Add shared UI dependencies in `CheckDlc.csproj` (`ExportCsvView`, `DatabaseMaintenanceView`, `ListWithNoData`, `TransfertData`, …)

## 4) Technical migration cleanup

- [x] Migrate `OriginDlc` from removed `OriginApi` to `EaApi` (`CommonPluginsStores.Ea`)
- [x] Update Epic client: `GetNameSpace` → `GetNamespaceFromGame`
- [x] Replace `PluginDatabase.Database` usages with `GetAllCache()` where applicable
  - [x] `source/CheckDlc.cs`
  - [x] `source/Views/CheckDlcFreeView.xaml.cs`
- [x] Override `AppendPluginTag` instead of legacy `AddTag(Game)` override
- [x] Update `RefreshNoLoader(Guid, CancellationToken)` signature
- [x] Fix XAML command references (`Commands` → `CommandsNavigation` / `GlobalCommands`)
- [x] Add NuGet `Ardalis.GuardClauses` (required by updated `playnite-plugincommon`)
- [ ] Review `packages.config` vs SDK references (SteamKit2, protobuf-net, etc.) for consistency with other plugins — optional cleanup

## 5) Validation

- [x] Build the solution (`Release` — OK)
- [ ] Verify plugin menus and windows load correctly in Playnite runtime
- [ ] Verify quick non-regression:
  - [ ] Game view: `PluginButton`, `PluginListDlc*` controls
  - [ ] Game / main menus (refresh, tags, CSV export, clear data)
  - [ ] Settings + store panels (Steam, Epic, GOG)
  - [ ] Free unowned DLC view
  - [ ] Price change notifications (if enabled)
