# Core Architecture — CheckDlc integration

| Component | CheckDlc status | Notes |
|-----------|-----------------|-------|
| `PluginSettingsViewModel` | [x] Done | `CheckDlcSettingsViewModel` in `source/CheckDlcSettings.cs` |
| `PluginMenus` | [x] Done | `CheckDlcMenus` in `source/Services/CheckDlcMenus.cs` |
| `PluginWindows` | [x] Done | `CheckDlcWindows` in `source/Services/CheckDlcWindows.cs` |
| `PluginExportCsv` | [x] Done | `CheckDlcExport` in `source/Services/CheckDlcExport.cs` |
| `PluginDatabaseObject<TSettings, TItem, T>` | [x] Done | `CheckDlcDatabase` — `CheckDlcSettings`, `GameDlc`, `Dlc` |

## Remaining (runtime / optional)

- [ ] Playnite manual smoke test (menus, views, settings, stores)
- [ ] Optional: trim legacy NuGet entries in `packages.config` if no longer required
