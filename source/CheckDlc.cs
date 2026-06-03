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
            PlayniteTools.GetPluginId(PlayniteTools.ExternalPlugin.SteamLibrary),
            PlayniteTools.GetPluginId(PlayniteTools.ExternalPlugin.PSNLibrary),
            PlayniteTools.GetPluginId(PlayniteTools.ExternalPlugin.NintendoLibrary)
        };


        public CheckDlc(IPlayniteAPI api) : base(api, "CheckDlc")
        {
            PluginDatabase.Plugin = this;
            _menus = new CheckDlcMenus(PluginSettingsViewModel.Settings, PluginDatabase, this);

            EventManager.RegisterClassHandler(typeof(Window), Window.LoadedEvent, new RoutedEventHandler(WindowBase_LoadedEvent));
            EventManager.RegisterClassHandler(typeof(Button), Button.ClickEvent, new RoutedEventHandler(OnCustomThemeButtonClick));

            AddCustomElementSupport(new AddCustomElementSupportArgs
            {
                ElementList = new List<string> { "PluginButton", "PluginListDlcAll", "PluginListDlcOwned", "PluginListDlcNotOwned" },
                SourceName = "CheckDlc"
            });

            AddSettingsSupport(new AddSettingsSupportArgs
            {
                SourceName = "CheckDlc",
                SettingsRoot = $"{nameof(PluginSettingsViewModel)}.{nameof(PluginSettingsViewModel.Settings)}"
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
                string buttonName = ((Button)sender).Name;
                if (buttonName == "PART_CustomCheckDlcButton" || buttonName == "PART_CustomHowLongToBeatButton")
                {
                    Common.LogDebug(true, "OnCustomThemeButtonClick()");
                    PluginDatabase.PluginWindows.ShowPluginGameDataWindow(this);
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginDatabase.PluginName);
            }
        }
        #endregion


        #region Theme integration
        public override Control GetGameViewControl(GetGameViewControlArgs args)
        {
            if (args.Name == "PluginButton")
            {
                return new PluginButton();
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
        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            return _menus.GetGameMenuItems(args);
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            return _menus.GetMainMenuItems(args);
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

        public override void OnGameInstalled(OnGameInstalledEventArgs args)
        {
        }

        public override void OnGameUninstalled(OnGameUninstalledEventArgs args)
        {
        }

        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
        }

        public override void OnGameStarted(OnGameStartedEventArgs args)
        {
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
        }
        #endregion


        #region Application event
        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            if (PluginDatabase.PluginSettings.PluginState.SteamIsEnabled)
            {
                SteamApi = new SteamApi(PluginDatabase.PluginName, PlayniteTools.ExternalPlugin.CheckDlc);
                SteamApi.SetLanguage(API.Instance.ApplicationSettings.Language);
                SteamApi.SetForceAuth(true);
                SteamApi.StoreSettings = PluginDatabase.PluginSettings.SteamStoreSettings;
                _ = SteamApi.CurrentAccountInfos;
            }

            if (PluginDatabase.PluginSettings.PluginState.EpicIsEnabled)
            {
                EpicApi = new EpicApi(PluginDatabase.PluginName, PlayniteTools.ExternalPlugin.CheckDlc);
                EpicApi.SetLanguage(API.Instance.ApplicationSettings.Language);
                EpicApi.SetForceAuth(true);
                EpicApi.StoreSettings = PluginDatabase.PluginSettings.EpicStoreSettings;
                _ = EpicApi.CurrentAccountInfos;
            }

            if (PluginDatabase.PluginSettings.PluginState.GogIsEnabled)
            {
                GogApi = new GogApi(PluginDatabase.PluginName, PlayniteTools.ExternalPlugin.CheckDlc);
                GogApi.SetLanguage(API.Instance.ApplicationSettings.Language);
                GogApi.SetForceAuth(true);
                GogApi.StoreSettings = PluginDatabase.PluginSettings.GogStoreSettings;
                _ = GogApi.CurrentAccountInfos;
            }

            _ = Task.Run(() =>
            {
                Thread.Sleep(10000);
                PreventLibraryUpdatedOnStart = true;
            });

            if (PluginSettingsViewModel.Settings.PriceNotification)
            {
                _ = Task.Run(() =>
                {
                    PluginDatabase.GetAllCache().Where(x => x.PriceNotification && !x.IsManual).ForEach(x =>
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
                                    () => PluginDatabase.PluginWindows.ShowPluginGameDataWindow(x.Game)
                                ));
                            }
                        });
                    });
                });
            }

            if (!PluginDatabase.PluginSettings.IsConverted)
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
                    PluginSettingsViewModel.Settings.IgnoredList = PluginSettingsViewModel.Settings.IgnoredList.Distinct().ToObservable();
                    PluginDatabase.GetAllCache().ForEach(x =>
                    {
                        x.Items.ForEach(y =>
                        {
                            int found = PluginSettingsViewModel.Settings.IgnoredList.IndexOf(x.Name + "##" + y.Name);
                            if (found != -1)
                            {
                                PluginSettingsViewModel.Settings.IgnoredList[found] = y.Id;
                            }

                            found = PluginSettingsViewModel.Settings.IgnoredList.IndexOf(x.Name);
                            if (found != -1)
                            {
                                PluginSettingsViewModel.Settings.IgnoredList[found] = y.Id;
                            }
                        });
                    });

                    _ = Application.Current.Dispatcher?.BeginInvoke((Action)delegate
                    {
                        PluginSettingsViewModel.Settings.IsConverted = true;
                        SavePluginSettings(PluginSettingsViewModel.Settings);
                    });
                }, globalProgressOptions);
            }
        }

        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {
        }
        #endregion


        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            if (PreventLibraryUpdatedOnStart && PluginSettingsViewModel.Settings.AutoImport)
            {
                List<Guid> PlayniteDb = PlayniteApi.Database.Games
                        .Where(x => x.Added != null && x.Added > PluginSettingsViewModel.Settings.LastAutoLibUpdateAssetsDownload)
                        .Select(x => x.Id).ToList();

                PluginDatabase.Refresh(PlayniteDb);

                PluginSettingsViewModel.Settings.LastAutoLibUpdateAssetsDownload = DateTime.Now;
                SavePluginSettings(PluginSettingsViewModel.Settings);
            }
        }


        #region Settings
        public override ISettings GetSettings(bool firstRunSettings)
        {
            return PluginSettingsViewModel;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new CheckDlcSettingsView();
        }
        #endregion
    }
}
