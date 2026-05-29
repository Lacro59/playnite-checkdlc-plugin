# Renaming Task — Plugin Database Classes

## Renames

| Old                                 | New                                              |
| ----------------------------------- | ------------------------------------------------ |
| `PluginDataBaseGameBase`            | `PluginGameEntry`                                |
| `PluginDataBaseGame<T>`             | `PluginGameCollection<T>`                        |
| `PluginDataBaseGameDetails<T, Y>`   | `PluginGameCollectionWithDetails<T, TDetails>`   |
| Paramètre de type `Y`               | `TDetails`                                       |

---

## Search & Replace

```text
Find    : PluginDataBaseGameBase
Replace : PluginGameEntry

Find    : PluginDataBaseGame<
Replace : PluginGameCollection<

Find    : PluginDataBaseGameDetails<
Replace : PluginGameCollectionWithDetails<
```

---

## Checklist — `playnite-plugincommon`

- [x] `PluginGameEntry.cs`
- [x] `PluginGameCollection.cs`
- [x] `PluginGameCollectionWithDetails.cs`

## Checklist — CheckDlc plugin

- [x] `source/Models/GameDlc.cs`
- [x] `source/Controls/PluginButton.xaml.cs`
- [x] `source/Controls/PluginListDlc.xaml.cs`
- [x] No remaining `PluginDataBaseGame*` in `source/` (excluding `playnite-plugincommon`)
- [x] Build solution (`Release`)
