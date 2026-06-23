using CheckDlc.Models;
using CommonPlayniteShared.Common;
using CommonPluginsShared;
using CommonPluginsShared.Collections;
using CommonPluginsShared.Extensions;
using CommonPluginsShared.Interfaces;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CheckDlc.Services
{
    public class CheckDlcMenus : PluginMenus
    {
        private readonly CheckDlc _plugin;
        private CheckDlcDatabase Database => (CheckDlcDatabase)_database;

        public CheckDlcMenus(IPluginSettings settings, IPluginDatabase database, CheckDlc plugin) : base(settings, database)
        {
            _plugin = plugin;
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            if (args?.Games == null || !args.Games.Any())
            {
                return Enumerable.Empty<GameMenuItem>();
            }

            List<Game> includedGames = args.Games
                .Where(game => PlayniteTools.ShouldIncludeLibraryGame(game, _settings))
                .ToList();

            if (includedGames.Count == 0)
            {
                Common.LogDebug(true, string.Format(
                    "[LibraryFilter] CheckDlcMenus: game menu hidden — no eligible game in selection ({0} selected, IncludeEmulatedGames={1}, SourceFilter={2})",
                    args.Games.Count,
                    _settings.IncludeEmulatedGames,
                    PlayniteTools.FormatSourceFilterForLog(_settings)));

                return Enumerable.Empty<GameMenuItem>();
            }

            int excludedCount = args.Games.Count - includedGames.Count;
            if (excludedCount > 0)
            {
                Common.LogDebug(true, string.Format(
                    "[LibraryFilter] CheckDlcMenus: {0}/{1} selected game(s) excluded from menu actions (IncludeEmulatedGames={2}, SourceFilter={3})",
                    excludedCount,
                    args.Games.Count,
                    _settings.IncludeEmulatedGames,
                    PlayniteTools.FormatSourceFilterForLog(_settings)));
            }

            Game gameMenu = includedGames.First();
            List<Guid> ids = includedGames.Select(x => x.Id).ToList();
            GameDlc gameDlc = Database.Get(gameMenu, true);

            List<GameMenuItem> gameMenuItems = new List<GameMenuItem>();

            if (gameDlc.HasData)
            {
                gameMenuItems.Add(new GameMenuItem
                {
                    MenuSection = ResourceProvider.GetString("LOCCheckDlc"),
                    Description = ResourceProvider.GetString("LOCCheckDlcViewDlc"),
                    Action = (gameMenuItem) => Database.PluginWindows.ShowPluginGameDataWindow(gameMenu)
                });

                gameMenuItems.Add(new GameMenuItem
                {
                    MenuSection = ResourceProvider.GetString("LOCCheckDlc"),
                    Description = "-"
                });
            }

            gameMenuItems.Add(new GameMenuItem
            {
                MenuSection = ResourceProvider.GetString("LOCCheckDlc"),
                Description = ResourceProvider.GetString("LOCCommonAddManually"),
                Action = (gameMenuItem) =>
                {
                    GenericItemOption selectedGame = API.Instance.Dialogs.ChooseItemWithSearch(
                        new List<GenericItemOption>(),
                        (x) => CheckDlc.SteamApi.GetSearchGame(x),
                        gameMenu.Name.NormalizeGameName(),
                        string.Format(
                            ResourceProvider.GetString("LOCCheckDlcWindowTitleFormat"),
                            ResourceProvider.GetString("LOCCheckDlc"),
                            ResourceProvider.GetString("LOCCommonSelectGames"))
                    );

                    if (selectedGame != null)
                    {
                        uint appId = uint.Parse(selectedGame.Description.Split('-')[0].Trim());
                        gameDlc.IsManual = true;
                        gameDlc.AppId = appId;
                        Database.AddOrUpdate(gameDlc);
                        Database.Refresh(gameMenu.Id);
                    }
                }
            });

            gameMenuItems.Add(new GameMenuItem
            {
                MenuSection = ResourceProvider.GetString("LOCCheckDlc"),
                Description = ResourceProvider.GetString("LOCCommonRefreshGameData"),
                Action = (gameMenuItem) =>
                {
                    if (ids.Count == 1)
                    {
                        Database.Refresh(gameMenu.Id);
                    }
                    else
                    {
                        Database.Refresh(ids);
                    }
                }
            });

            if (gameDlc.HasData)
            {
                gameMenuItems.Add(new GameMenuItem
                {
                    MenuSection = ResourceProvider.GetString("LOCCheckDlc"),
                    Description = ResourceProvider.GetString("LOCCommonDeleteGameData"),
                    Action = (gameMenuItem) =>
                    {
                        List<Guid> idsWithData = ids
                            .Where(id => Database.Get(id, true).HasData)
                            .ToList();

                        if (idsWithData.Count == 0)
                        {
                            return;
                        }

                        if (idsWithData.Count == 1)
                        {
                            Database.Remove(idsWithData[0]);
                        }
                        else
                        {
                            Database.Remove(idsWithData);
                        }
                    }
                });
            }

#if DEBUG
            gameMenuItems.Add(new GameMenuItem
            {
                MenuSection = ResourceProvider.GetString("LOCCheckDlc"),
                Description = "-"
            });
            gameMenuItems.Add(new GameMenuItem
            {
                MenuSection = ResourceProvider.GetString("LOCCheckDlc"),
                Description = "Test",
                Action = (gameMenuItem) => { }
            });
#endif

            return gameMenuItems;
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            string menuInExtensions = string.Empty;
            if (_settings.MenuInExtensions)
            {
                menuInExtensions = "@";
            }

            string section = menuInExtensions + ResourceProvider.GetString("LOCCheckDlc");

            List<MainMenuItem> mainMenuItems = new List<MainMenuItem>
            {
                new MainMenuItem
                {
                    MenuSection = section,
                    Description = ResourceProvider.GetString("LOCCommonDownloadPluginData"),
                    Action = (mainMenuItem) => Database.GetSelectData()
                },
                new MainMenuItem
                {
                    MenuSection = section,
                    Description = ResourceProvider.GetString("LOCCheckDlcViewFreeDlcNoOwned"),
                    Action = (mainMenuItem) => ((CheckDlcWindows)Database.PluginWindows).ShowFreeDlcWindow(_plugin)
                }
            };

            if (_settings.EnableTag)
            {
                mainMenuItems.Add(new MainMenuItem
                {
                    MenuSection = section,
                    Description = "-"
                });

                mainMenuItems.Add(new MainMenuItem
                {
                    MenuSection = section,
                    Description = ResourceProvider.GetString("LOCCommonAddTPlugin"),
                    Action = (mainMenuItem) => Database.AddTagSelectData()
                });
                mainMenuItems.Add(new MainMenuItem
                {
                    MenuSection = section,
                    Description = ResourceProvider.GetString("LOCCommonAddAllTags"),
                    Action = mainMenuItem => _commands.CmdAddTag.Execute(null)
                });
                mainMenuItems.Add(new MainMenuItem
                {
                    MenuSection = section,
                    Description = ResourceProvider.GetString("LOCCommonRemoveAllTags"),
                    Action = mainMenuItem => _commands.CmdRemoveTag.Execute(null)
                });
            }

            mainMenuItems.Add(new MainMenuItem
            {
                MenuSection = section,
                Description = "-"
            });

            mainMenuItems.Add(new MainMenuItem
            {
                MenuSection = section,
                Description = ResourceProvider.GetString("LOCCommonExtractToCsv"),
                Action = (mainMenuItem) => Database.ExtractToCsv()
            });

            mainMenuItems.Add(new MainMenuItem
            {
                MenuSection = section,
                Description = ResourceProvider.GetString("LOCCommonDeletePluginData"),
                Action = mainMenuItem => _commands.CmdClearAll.Execute(null)
            });

#if DEBUG
            mainMenuItems.Add(new MainMenuItem
            {
                MenuSection = section,
                Description = "-"
            });
            mainMenuItems.Add(new MainMenuItem
            {
                MenuSection = section,
                Description = "Test",
                Action = (mainMenuItem) => { }
            });
#endif

            return mainMenuItems;
        }
    }
}
