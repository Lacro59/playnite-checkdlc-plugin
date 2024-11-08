using CheckDlc.Clients;
using CheckDlc.Controls;
using CheckDlc.Models;
using CheckDlc.Services;
using CheckDlc.Views;
using CommonPlayniteShared.Common;
using CommonPluginsShared;
using CommonPluginsShared.Extensions;
using CommonPluginsShared.PlayniteExtended;
using CommonPluginsStores.Epic;
using CommonPluginsStores.Gog;
using CommonPluginsStores.Steam;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;

namespace CheckDlc
{
    public class CheckDlc : PluginExtended<CheckDlcSettingsViewModel, CheckDlcDatabase>
    {
        public override Guid Id => Guid.Parse("bf78d9af-6e79-4c73-aca6-c23a11a485ae");

        private bool PreventLibraryUpdatedOnStart { get; set; } = false;

        public static SteamApi SteamApi { get; set; }
        public static EpicApi EpicApi { get; set; }
        public static GogApi GogApi { get; set; }

        public static List<Guid> SupportedLibrary => new List<Guid>
        {
            PlayniteTools.GetPluginId(PlayniteTools.ExternalPlugin.EpicLibrary),
            PlayniteTools.GetPluginId(PlayniteTools.ExternalPlugin.LegendaryLibrary),
            PlayniteTools.GetPluginId(PlayniteTools.ExternalPlugin.GogLibrary),
            PlayniteTools.GetPluginId(PlayniteTools.ExternalPlugin.GogOssLibrary),
            PlayniteTools.GetPluginId(PlayniteTools.ExternalPlugin.OriginLibrary),
            PlayniteTools.GetPluginId(PlayniteTools.ExternalPlugin.SteamLibrary)
        };


        public CheckDlc(IPlayniteAPI api) : base(api)
        {
            // Add Event for WindowBase for get the "WindowSettings".
            EventManager.RegisterClassHandler(typeof(Window), Window.LoadedEvent, new RoutedEventHandler(WindowBase_LoadedEvent));

            // Custom theme button
            EventManager.RegisterClassHandler(typeof(Button), Button.ClickEvent, new RoutedEventHandler(OnCustomThemeButtonClick));

            // Custom elements integration
            AddCustomElementSupport(new AddCustomElementSupportArgs
            {
                ElementList = new List<string> { "PluginButton", "PluginListDlcAll", "PluginListDlcOwned", "PluginListDlcNotOwned" },
                SourceName = "CheckDlc"
            });

            // Settings integration
            AddSettingsSupport(new AddSettingsSupportArgs
            {
                SourceName = "CheckDlc",
                SettingsRoot = $"{nameof(PluginSettings)}.{nameof(PluginSettings.Settings)}"
            });

            // TODO TEMP
            FileSystem.DeleteFile(Path.Combine(PluginDatabase.Paths.PluginUserDataPath, "SteamUserData.json"));
        }


        #region Custom event
        private void WindowBase_LoadedEvent(object sender, EventArgs e)
        {
            string WinIdProperty = string.Empty;
            try
            {
                WinIdProperty = ((Window)sender).GetValue(AutomationProperties.AutomationIdProperty).ToString();
                if (WinIdProperty == "WindowSettings" || WinIdProperty == "WindowExtensions" || WinIdProperty == "WindowLibraryIntegrations")
                {
                    GogDlc.SettingsOpen = true;
                    SteamDlc.SettingsOpen = true;
                    EpicDlc.SettingsOpen = true;
                    OriginDlc.SettingsOpen = true;
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, $"Error on WindowBase_LoadedEvent for {WinIdProperty}", true, PluginDatabase.PluginName);
            }
        }

        public void OnCustomThemeButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string ButtonName = ((Button)sender).Name;
                if (ButtonName == "PART_CustomHowLongToBeatButton")
                {
                    Common.LogDebug(true, $"OnCustomThemeButtonClick()");

                    WindowOptions windowOptions = new WindowOptions
                    {
                        CanBeResizable = false,
                        Height = 720,
                        Width = 1000,
                        ShowMaximizeButton = false
                    };
                    CheclDlcGameView ViewExtension = new CheclDlcGameView(this, PluginDatabase.GameContext);
                    Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(ResourceProvider.GetString("LOCCheckDlc"), ViewExtension, windowOptions);
                    _ = windowExtension.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginDatabase.PluginName);
            }
        }
        #endregion


        #region Theme integration
        // List custom controls
        public override Control GetGameViewControl(GetGameViewControlArgs args)
        {
            if (args.Name == "PluginButton")
            {
                return new PluginButton(this);
            }

            if (args.Name == "PluginListDlcAll")
            {
                return new PluginListDlc();
            }
            if (args.Name == "PluginListDlcOwned")
            {
                return new PluginListDlc { ListType = ListDlcType.Owned };
            }
            if (args.Name == "PluginListDlcNotOwned")
            {
                return new PluginListDlc { ListType = ListDlcType.NotOwned };
            }

            return null;
        }
        #endregion


        #region Menus
        // To add new game menu items override GetGameMenuItems
        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            Game gameMenu = args.Games.First();
            List<Guid> ids = args.Games.Select(x => x.Id).ToList();
            GameDlc gameDlc = PluginDatabase.Get(gameMenu, true);

            List<GameMenuItem> gameMenuItems = new List<GameMenuItem>();

            if (gameDlc.HasData)
            {
                gameMenuItems.Add(new GameMenuItem
                {
                    MenuSection = ResourceProvider.GetString("LOCCheckDlc"),
                    Description = ResourceProvider.GetString("LOCCheckDlcViewDlc"),
                    Action = (gameMenuItem) =>
                    {
                        WindowOptions windowOptions = new WindowOptions
                        {
                            CanBeResizable = false,
                            Height = 720,
                            Width = 1000,
                            ShowMaximizeButton = false
                        };
                        CheclDlcGameView ViewExtension = new CheclDlcGameView(this, gameMenu);
                        Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(ResourceProvider.GetString("LOCCheckDlc"), ViewExtension, windowOptions);
                        windowExtension.ShowDialog();
                    }
                });

                gameMenuItems.Add(new GameMenuItem
                {
                    MenuSection = ResourceProvider.GetString("LOCCheckDlc"),
                    Description = "-"
                });
            }

            if ((SupportedLibrary.Contains(gameMenu.PluginId) && PlayniteTools.IsEnabledPlaynitePlugin(gameMenu.PluginId)) || ids.Count > 1)
            {
                gameMenuItems.Add(new GameMenuItem
                {
                    MenuSection = ResourceProvider.GetString("LOCCheckDlc"),
                    Description = ResourceProvider.GetString("LOCCommonRefreshGameData"),
                    Action = (gameMenuItem) =>
                    {
                        if (ids.Count == 1)
                        {
                            PluginDatabase.Refresh(gameMenu.Id);
                        }
                        else
                        {
                            PluginDatabase.Refresh(ids);
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
                            PluginDatabase.Remove(gameMenu);
                        }
                        else
                        {
                            PluginDatabase.Remove(ids);
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
                Action = (gameMenuItem) =>
                {

                }
            });
#endif

            return gameMenuItems;
        }

        // To add new main menu items override GetMainMenuItems
        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            string MenuInExtensions = string.Empty;
            if (PluginSettings.Settings.MenuInExtensions)
            {
                MenuInExtensions = "@";
            }

            List<MainMenuItem> mainMenuItems = new List<MainMenuItem>
            {
                // Download missing data for all game in database
                new MainMenuItem
                {
                    MenuSection = MenuInExtensions + ResourceProvider.GetString("LOCCheckDlc"),
                    Description = ResourceProvider.GetString("LOCCommonDownloadPluginData"),
                    Action = (mainMenuItem) =>
                    {
                        PluginDatabase.GetSelectData();
                    }
                },

                new MainMenuItem
                {
                    MenuSection = MenuInExtensions + ResourceProvider.GetString("LOCCheckDlc"),
                    Description = ResourceProvider.GetString("LOCCheckDlcViewFreeDlcNoOwned"),
                    Action = (mainMenuItem) =>
                    {
                        CheckDlcFreeView ViewExtension = new CheckDlcFreeView(this);
                        Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(ResourceProvider.GetString("LOCCheckDlc"), ViewExtension);
                        windowExtension.ShowDialog();
                    }
                }
            };

            if (PluginDatabase.PluginSettings.Settings.EnableTag)
            {
                mainMenuItems.Add(new MainMenuItem
                {
                    MenuSection = MenuInExtensions + ResourceProvider.GetString("LOCCheckDlc"),
                    Description = "-"
                });

                // Add tag for selected game in database if data exists
                mainMenuItems.Add(new MainMenuItem
                {
                    MenuSection = MenuInExtensions + ResourceProvider.GetString("LOCCheckDlc"),
                    Description = ResourceProvider.GetString("LOCCommonAddTPlugin"),
                    Action = (mainMenuItem) =>
                    {
                        PluginDatabase.AddTagSelectData();
                    }
                });
                // Add tag for all games
                mainMenuItems.Add(new MainMenuItem
                {
                    MenuSection = MenuInExtensions + ResourceProvider.GetString("LOCCheckDlc"),
                    Description = ResourceProvider.GetString("LOCCommonAddAllTags"),
                    Action = (mainMenuItem) =>
                    {
                        PluginDatabase.AddTagAllGame();
                    }
                });
                // Remove tag for all game in database
                mainMenuItems.Add(new MainMenuItem
                {
                    MenuSection = MenuInExtensions + ResourceProvider.GetString("LOCCheckDlc"),
                    Description = ResourceProvider.GetString("LOCCommonRemoveAllTags"),
                    Action = (mainMenuItem) =>
                    {
                        PluginDatabase.RemoveTagAllGame();
                    }
                });
            }

            mainMenuItems.Add(new MainMenuItem
            {
                MenuSection = MenuInExtensions + ResourceProvider.GetString("LOCCheckDlc"),
                Description = "-"
            });


            mainMenuItems.Add(new MainMenuItem
            {
                MenuSection = MenuInExtensions + ResourceProvider.GetString("LOCCheckDlc"),
                Description = ResourceProvider.GetString("LOCCommonExtractToCsv"),
                Action = (mainMenuItem) =>
                {
                    string path = API.Instance.Dialogs.SelectFolder();
                    PluginDatabase.ExtractToCsv(path, false);
                }
            });

            // Delete database
            mainMenuItems.Add(new MainMenuItem
            {
                MenuSection = MenuInExtensions + ResourceProvider.GetString("LOCCheckDlc"),
                Description = ResourceProvider.GetString("LOCCommonDeletePluginData"),
                Action = (mainMenuItem) =>
                {
                    PluginDatabase.ClearDatabase();
                }
            });

#if DEBUG
            mainMenuItems.Add(new MainMenuItem
            {
                MenuSection = MenuInExtensions + ResourceProvider.GetString("LOCCheckDlc"),
                Description = "-"
            });
            mainMenuItems.Add(new MainMenuItem
            {
                MenuSection = MenuInExtensions + ResourceProvider.GetString("LOCCheckDlc"),
                Description = "Test",
                Action = (mainMenuItem) =>
                {

                }
            });
#endif

            return mainMenuItems;
        }
        #endregion


        #region Game event
        public override void OnGameSelected(OnGameSelectedEventArgs args)
        {
            try
            {
                if (args.NewValue?.Count == 1 && PluginDatabase.IsLoaded)
                {
                    PluginDatabase.GameContext = args.NewValue[0];
                    PluginDatabase.SetThemesResources(PluginDatabase.GameContext);
                }
                else
                {
                    _ = Task.Run(() =>
                    {
                        _ = SpinWait.SpinUntil(() => PluginDatabase.IsLoaded, -1);
                        _ = Application.Current.Dispatcher.BeginInvoke((Action)delegate
                        {
                            if (args.NewValue?.Count == 1)
                            {
                                PluginDatabase.GameContext = args.NewValue[0];
                                PluginDatabase.SetThemesResources(PluginDatabase.GameContext);
                            }
                        });
                    });
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginDatabase.PluginName);
            }
        }

        // Add code to be executed when game is finished installing.
        public override void OnGameInstalled(OnGameInstalledEventArgs args)
        {

        }

        // Add code to be executed when game is uninstalled.
        public override void OnGameUninstalled(OnGameUninstalledEventArgs args)
        {

        }

        // Add code to be executed when game is preparing to be started.
        public override void OnGameStarting(OnGameStartingEventArgs args)
        {

        }

        // Add code to be executed when game is started running.
        public override void OnGameStarted(OnGameStartedEventArgs args)
        {

        }

        // Add code to be executed when game is preparing to be started.
        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {

        }
        #endregion


        #region Application event
        // Add code to be executed when Playnite is initialized.
        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            // StoreAPI intialization
            if (PluginDatabase.PluginSettings.Settings.PluginState.SteamIsEnabled)
            {
                SteamApi = new SteamApi(PluginDatabase.PluginName, PlayniteTools.ExternalPlugin.CheckDlc);
                SteamApi.SetLanguage(API.Instance.ApplicationSettings.Language);
                SteamApi.StoreSettings = PluginDatabase.PluginSettings.Settings.SteamStoreSettings;
                _ = SteamApi.CurrentAccountInfos;
            }

            if (PluginDatabase.PluginSettings.Settings.PluginState.EpicIsEnabled)
            {
                EpicApi = new EpicApi(PluginDatabase.PluginName, PlayniteTools.ExternalPlugin.CheckDlc);
                EpicApi.SetLanguage(API.Instance.ApplicationSettings.Language);
                EpicApi.SetForceAuth(true);
                EpicApi.StoreSettings = PluginDatabase.PluginSettings.Settings.EpicStoreSettings;
                _ = EpicApi.CurrentAccountInfos;
            }

            if (PluginDatabase.PluginSettings.Settings.PluginState.GogIsEnabled)
            {
                GogApi = new GogApi(PluginDatabase.PluginName, PlayniteTools.ExternalPlugin.CheckDlc);
                GogApi.SetLanguage(API.Instance.ApplicationSettings.Language);
                GogApi.SetForceAuth(true);
                GogApi.StoreSettings = PluginDatabase.PluginSettings.Settings.GogStoreSettings;
                _ = GogApi.CurrentAccountInfos;
            }

            _ = Task.Run(() =>
            {
                Thread.Sleep(10000);
                PreventLibraryUpdatedOnStart = true;
            });

            if (PluginSettings.Settings.PriceNotification)
            {
                _ = Task.Run(() =>
                {
                    PluginDatabase.Database.Where(x => x.PriceNotification).ForEach(x =>
                    {
                        PluginDatabase.RefreshNoLoader(x.Id);
                        List<Dlc> newItems = PluginDatabase.GetOnlyCache(x.Id).Items;

                        newItems.ForEach(y =>
                        {
                            if (y.PriceNumeric != x.Items.Find(z => z.DlcId.IsEqual(y.DlcId)).PriceNumeric)
                            {
                                API.Instance.Notifications.Add(new NotificationMessage(
                                    $"{PluginDatabase.PluginName}-{x.Id}",
                                    $"{PluginDatabase.PluginName}" + Environment.NewLine + string.Format(ResourceProvider.GetString("LOCCheckDlcNewPrice"), x.Name),
                                    NotificationType.Info,
                                    () =>
                                    {
                                        WindowOptions windowOptions = new WindowOptions
                                        {
                                            CanBeResizable = false,
                                            Height = 720,
                                            Width = 1000,
                                            ShowMaximizeButton = false
                                        };
                                        CheclDlcGameView ViewExtension = new CheclDlcGameView(this, x.Game);
                                        Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(ResourceProvider.GetString("LOCCheckDlc"), ViewExtension, windowOptions);
                                        _ = windowExtension.ShowDialog();
                                    }
                                ));
                                return;
                            }
                        });
                    });
                });
            }

            // TODO TEMP
            if (!PluginDatabase.PluginSettings.Settings.IsConverted)
            {
                Logger.Info("Convert settings");

                GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                    $"{PluginDatabase.PluginName} - {ResourceProvider.GetString("LOCCommonConverting")}",
                    false
                );
                globalProgressOptions.IsIndeterminate = true;

                _ = API.Instance.Dialogs.ActivateGlobalProgress((activateGlobalProgress) =>
                {
                    _ = SpinWait.SpinUntil(() => PluginDatabase.IsLoaded, -1);
                    PluginSettings.Settings.IgnoredList = PluginSettings.Settings.IgnoredList.Distinct().ToObservable();
                    PluginDatabase.Database.ForEach(x =>
                    {
                        x.Items.ForEach(y =>
                        {
                            // With game name
                            int found = PluginSettings.Settings.IgnoredList.IndexOf(x.Name + "##" + y.Name);
                            if (found != -1)
                            {
                                PluginSettings.Settings.IgnoredList[found] = y.Id;
                            }
                            // Without game name
                            found = PluginSettings.Settings.IgnoredList.IndexOf(x.Name);
                            if (found != -1)
                            {
                                PluginSettings.Settings.IgnoredList[found] = y.Id;
                            }
                        });
                    });

                    _ = Application.Current.Dispatcher?.BeginInvoke((Action)delegate
                    {
                        PluginSettings.Settings.IsConverted = true;
                        SavePluginSettings(PluginSettings.Settings);
                    });
                }, globalProgressOptions);
            }
        }

        // Add code to be executed when Playnite is shutting down.
        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {

        }
        #endregion


        // Add code to be executed when library is updated.
        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            if (PreventLibraryUpdatedOnStart && PluginSettings.Settings.AutoImport)
            {
                List<Guid> PlayniteDb = PlayniteApi.Database.Games
                        .Where(x => x.Added != null && x.Added > PluginSettings.Settings.LastAutoLibUpdateAssetsDownload)
                        .Select(x => x.Id).ToList();

                PluginDatabase.Refresh(PlayniteDb);

                PluginSettings.Settings.LastAutoLibUpdateAssetsDownload = DateTime.Now;
                SavePluginSettings(PluginSettings.Settings);
            }
        }


        #region Settings
        public override ISettings GetSettings(bool firstRunSettings)
        {
            return PluginSettings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new CheckDlcSettingsView();
        }
        #endregion
    }
}
