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

            Game gameMenu = args.Games.First();
            List<Guid> ids = args.Games.Select(x => x.Id).ToList();
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
                        ResourceProvider.GetString("LOCCommonSelectGames")
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

            if ((CheckDlc.SupportedLibrary.Contains(gameMenu.PluginId) && PlayniteTools.IsEnabledPlaynitePlugin(gameMenu.PluginId)) || ids.Count > 1)
            {
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
            }

            if (gameDlc.HasData)
            {
                gameMenuItems.Add(new GameMenuItem
                {
                    MenuSection = ResourceProvider.GetString("LOCCheckDlc"),
                    Description = ResourceProvider.GetString("LOCCommonDeleteGameData"),
                    Action = (gameMenuItem) =>
                    {
                        if (ids.Count == 1)
                        {
                            Database.Remove(gameMenu);
                        }
                        else
                        {
                            Database.Remove(ids);
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
