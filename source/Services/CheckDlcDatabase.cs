using CheckDlc.Clients;
using CheckDlc.Models;
using CommonPluginsShared;
using CommonPluginsShared.Collections;
using CommonPluginsStores.Epic;
using CommonPluginsStores.Gog;
using CommonPluginsStores.Steam;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

using static CommonPluginsShared.PlayniteTools;

namespace CheckDlc.Services
{
    public class CheckDlcDatabase : PluginDatabaseObject<CheckDlcSettings, GameDlc, Dlc>
    {
        public bool SettingsOpen { get; set; } = false;

        public CheckDlc Plugin { get; set; }

        public CheckDlcDatabase(CheckDlcSettings pluginSettings, string pluginUserDataPath) : base(pluginSettings, "CheckDlc", pluginUserDataPath)
        {
            TagBefore = "[DLC]";
            PluginWindows = new CheckDlcWindows(PluginName, this);
            PluginExportCsv = new CheckDlcExport();
        }

        /// <summary>
        /// Creates store API clients for enabled library integrations and applies settings.
        /// Skips stores whose Playnite library plugin is not enabled.
        /// </summary>
        /// <param name="settings">Store settings to apply. Uses <see cref="PluginDatabaseObject{CheckDlcSettings, GameDlc, Dlc}.PluginSettings"/> when null.</param>
        /// <param name="reloadAccountInfos">When true, clears and reloads account information for each enabled store.</param>
        public void EnsureStoreApis(CheckDlcSettings settings, bool reloadAccountInfos)
        {
            CheckDlcSettings storeSettings = settings ?? PluginSettings;
            if (storeSettings == null)
            {
                return;
            }

            if (storeSettings.PluginState.SteamIsEnabled)
            {
                bool created = CheckDlc.SteamApi == null;
                if (created)
                {
                    CheckDlc.SteamApi = new SteamApi(PluginName, ExternalPlugin.CheckDlc);
                    CheckDlc.SteamApi.SetLanguage(API.Instance.ApplicationSettings.Language);
                }

                CheckDlc.SteamApi.SetForceAuth(true);
                CheckDlc.SteamApi.StoreSettings = storeSettings.SteamStoreSettings;

                if (reloadAccountInfos)
                {
                    CheckDlc.SteamApi.ReloadAccountInfos();
                    _ = CheckDlc.SteamApi.CurrentAccountInfos;
                }
                else if (created)
                {
                    _ = CheckDlc.SteamApi.CurrentAccountInfos;
                }
            }

            if (storeSettings.PluginState.EpicIsEnabled)
            {
                bool created = CheckDlc.EpicApi == null;
                if (created)
                {
                    CheckDlc.EpicApi = new EpicApi(PluginName, ExternalPlugin.CheckDlc);
                    CheckDlc.EpicApi.SetLanguage(API.Instance.ApplicationSettings.Language);
                }

                CheckDlc.EpicApi.SetForceAuth(true);
                CheckDlc.EpicApi.StoreSettings = storeSettings.EpicStoreSettings;

                if (reloadAccountInfos)
                {
                    CheckDlc.EpicApi.ReloadAccountInfos();
                    _ = CheckDlc.EpicApi.CurrentAccountInfos;
                }
                else if (created)
                {
                    _ = CheckDlc.EpicApi.CurrentAccountInfos;
                }
            }

            if (storeSettings.PluginState.GogIsEnabled)
            {
                bool created = CheckDlc.GogApi == null;
                if (created)
                {
                    CheckDlc.GogApi = new GogApi(PluginName, ExternalPlugin.CheckDlc);
                    CheckDlc.GogApi.SetLanguage(API.Instance.ApplicationSettings.Language);
                }

                CheckDlc.GogApi.SetForceAuth(true);
                CheckDlc.GogApi.StoreSettings = storeSettings.GogStoreSettings;

                if (reloadAccountInfos)
                {
                    CheckDlc.GogApi.ReloadAccountInfos();
                    _ = CheckDlc.GogApi.CurrentAccountInfos;
                }
                else if (created)
                {
                    _ = CheckDlc.GogApi.CurrentAccountInfos;
                }
            }
        }

        public override GameDlc Get(Guid id, bool onlyCache = false, bool force = false)
        {
            GameDlc gameDlc = base.GetOnlyCache(id);

            if ((gameDlc == null && !onlyCache) || force)
            {
                gameDlc = GetWeb(id);
                AddOrUpdate(gameDlc);
            }

            if (gameDlc == null)
            {
                Game game = API.Instance.Database.Games.Get(id);
                if (game != null)
                {
                    gameDlc = GetDefault(game);
                    AddOrUpdate(gameDlc);
                }
            }

            return gameDlc;
        }

        public override GameDlc GetWeb(Guid id)
        {
            Game game = API.Instance.Database.Games.Get(id);
            GameDlc gameDlc = GetDefault(game);
            try
            {
                List<Dlc> dlcs = new List<Dlc>();
                ExternalPlugin pluginType = PlayniteTools.GetPluginType(game.PluginId);
                switch (pluginType)
                {
                    case ExternalPlugin.SteamLibrary:
                        if (PluginSettings.PluginState.SteamIsEnabled)
                        {
                            SteamDlc steamDlc = new SteamDlc();
                            dlcs = steamDlc.GetGameDlc(game);
                        }
                        break;

                    case ExternalPlugin.GogLibrary:
                    case ExternalPlugin.GogOssLibrary:
                        if (PluginSettings.PluginState.GogIsEnabled)
                        {
                            GogDlc gogDlc = new GogDlc();
                            dlcs = gogDlc.GetGameDlc(game);
                        }
                        break;

                    case ExternalPlugin.EpicLibrary:
                    case ExternalPlugin.LegendaryLibrary:
                        if (PluginSettings.PluginState.EpicIsEnabled)
                        {
                            EpicDlc epicDlc = new EpicDlc();
                            dlcs = epicDlc.GetGameDlc(game);
                        }
                        break;

                    case ExternalPlugin.OriginLibrary:
                        if (PluginSettings.PluginState.OriginIsEnabled)
                        {
                            OriginDlc originDlc = new OriginDlc();
                            dlcs = originDlc.GetGameDlc(game);
                        }
                        break;

                    case ExternalPlugin.PSNLibrary:
                        if (PluginSettings.PluginState.PsnIsEnabled)
                        {
                            PsnDlc psnDlc = new PsnDlc();
                            dlcs = psnDlc.GetGameDlc(game);
                        }
                        break;

                    case ExternalPlugin.NintendoLibrary:
                        if (PluginSettings.PluginState.NintendoIsEnabled)
                        {
                            NintendoDlc nintendoDlc = new NintendoDlc();
                            dlcs = nintendoDlc.GetGameDlc(game);
                        }
                        break;

                    case ExternalPlugin.None:
                    case ExternalPlugin.BattleNetLibrary:
                    case ExternalPlugin.XboxLibrary:
                    case ExternalPlugin.IndiegalaLibrary:
                    case ExternalPlugin.AmazonGamesLibrary:
                    case ExternalPlugin.BethesdaLibrary:
                    case ExternalPlugin.HumbleLibrary:
                    case ExternalPlugin.ItchioLibrary:
                    case ExternalPlugin.RockstarLibrary:
                    case ExternalPlugin.TwitchLibrary:
                    case ExternalPlugin.OculusLibrary:
                    case ExternalPlugin.RiotLibrary:
                    case ExternalPlugin.UplayLibrary:
                    case ExternalPlugin.SuccessStory:
                    case ExternalPlugin.CheckDlc:
                    case ExternalPlugin.EmuLibrary:
                    default:
                        break;
                }

                gameDlc.Items = dlcs;
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginName);
            }

            return gameDlc;
        }

        private GameDlc GetManual(Guid id, uint appId)
        {
            Game game = API.Instance.Database.Games.Get(id);
            GameDlc gameDlc = GetDefault(game);

            try
            {
                SteamDlc steamDlc = new SteamDlc();
                gameDlc.IsManual = true;
                gameDlc.AppId = appId;
                gameDlc.Items = steamDlc.GetGameDlc(appId);
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, false, PluginName);
            }

            return gameDlc;
        }

        public override void SetThemesResources(Game game)
        {
            GameDlc gameDlc = Get(game, true);
            PluginSettings.HasData = gameDlc?.HasData ?? false;
            PluginSettings.ListDlcs = new List<Dlc>();

            if (PluginSettings.HasData)
            {
                PluginSettings.ListDlcs = gameDlc.Items;
            }
        }

        public override void RefreshNoLoader(Guid id, CancellationToken cancellationToken = default)
        {
            Game game = API.Instance.Database.Games.Get(id);
            Logger.Info(string.Format("RefreshNoLoader — {0} ({1} - {2})", game?.Name, id, game?.GameId));

            if (game == null)
            {
                return;
            }

            GameDlc loadedItem = Get(id, true);
            if (CheckDlc.SupportedLibrary.Contains(game.PluginId) && !loadedItem.IsManual)
            {
                GameDlc webItem = GetWeb(id);
                webItem.PriceNotification = loadedItem.PriceNotification;

                if (webItem != null && !ReferenceEquals(loadedItem, webItem))
                {
                    Update(webItem);
                }
                else
                {
                    webItem = loadedItem;
                }

                ActionAfterRefresh(webItem);
            }
            else if (loadedItem.IsManual)
            {
                GameDlc webItem = GetManual(id, loadedItem.AppId);

                if (webItem != null && !ReferenceEquals(loadedItem, webItem))
                {
                    Update(webItem);
                }
                else
                {
                    webItem = loadedItem;
                }

                ActionAfterRefresh(webItem);
            }
            else
            {
                Logger.Warn($"The plugin does not support the library {PlayniteTools.GetSourceByPluginId(game.PluginId)}");
            }
        }

        public override void ActionAfterRefresh(GameDlc item)
        {
            Game game = API.Instance.Database.Games.Get(item.Id);
            if ((item?.HasData ?? false) && PluginSettings.DlcFeature != null)
            {
                if (game.FeatureIds != null)
                {
                    _ = game.FeatureIds.AddMissing(PluginSettings.DlcFeature.Id);
                }
                else
                {
                    game.FeatureIds = new List<Guid> { PluginSettings.DlcFeature.Id };
                }
                API.Instance.Database.Games.Update(game);
            }
            else
            {
                if (PluginSettings.DlcFeature?.Id != null && game.FeatureIds?.Find(x => x == PluginSettings.DlcFeature?.Id) != null)
                {
                    _ = game.FeatureIds.Remove(PluginSettings.DlcFeature.Id);
                    API.Instance.Database.Games.Update(game);
                }
            }
        }

        protected override bool AppendPluginTag(Game game)
        {
            GameDlc item = Get(game, true);
            bool modified = base.AppendPluginTag(game);

            if (item?.HasData == true && PluginSettings.EnableTagAllDlc && item.HasAllDlc)
            {
                Guid? tagId = FindGoodPluginTags("100%");
                if (tagId != null)
                {
                    AppendTagId(game, tagId.Value);
                    modified = true;
                }
            }

            return modified;
        }
    }
}
