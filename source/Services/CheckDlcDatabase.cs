using CheckDlc.Clients;
using CheckDlc.Models;
using CommonPluginsShared;
using CommonPluginsShared.Collections;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using static CommonPluginsShared.PlayniteTools;

namespace CheckDlc.Services
{
    public class CheckDlcDatabase : PluginDatabaseObject<CheckDlcSettingsViewModel, CheckDlcCollection, GameDlc, Dlc>
    {
        public bool SettingsOpen { get; set; } = false;


        public CheckDlcDatabase(CheckDlcSettingsViewModel PluginSettings, string PluginUserDataPath) : base(PluginSettings, "CheckDlc", PluginUserDataPath)
        {
            TagBefore = "[DLC]";
        }


        protected override bool LoadDatabase()
        {
            try
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                Database = new CheckDlcCollection(Paths.PluginDatabasePath);
                Database.SetGameInfo<Dlc>();

                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;
                Logger.Info($"LoadDatabase with {Database.Count} items - {string.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10)}");
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginName);
                return false;
            }

            return true;
        }


        public override GameDlc Get(Guid Id, bool OnlyCache = false, bool Force = false)
        {
            GameDlc gameDlc = base.GetOnlyCache(Id);

            // Get from web
            if ((gameDlc == null && !OnlyCache) || Force)
            {
                gameDlc = GetWeb(Id);
                AddOrUpdate(gameDlc);
            }

            if (gameDlc == null)
            {
                Game game = API.Instance.Database.Games.Get(Id);
                if (game != null)
                {
                    gameDlc = GetDefault(game);
                    AddOrUpdate(gameDlc);
                }
            }

            return gameDlc;
        }

        public override GameDlc GetWeb(Guid Id)
        {
            Game game = API.Instance.Database.Games.Get(Id);
            GameDlc gameDlc = GetDefault(game);
            try
            {
                //Thread.Sleep(100);
                List<Dlc> dlcs = new List<Dlc>();
                ExternalPlugin pluginType = PlayniteTools.GetPluginType(game.PluginId);
                switch (pluginType)
                {
                    case ExternalPlugin.SteamLibrary:
                        SteamDlc steamDlc = new SteamDlc();
                        dlcs = steamDlc.GetGameDlc(game);
                        break;

                    case ExternalPlugin.GogLibrary:
                        GogDlc gogDlc = new GogDlc();
                        dlcs = gogDlc.GetGameDlc(game);
                        break;

                    case ExternalPlugin.EpicLibrary:
                        EpicDlc epicDlc = new EpicDlc();
                        dlcs = epicDlc.GetGameDlc(game);
                        break;

                    case ExternalPlugin.OriginLibrary:
                        OriginDlc originDlc = new OriginDlc();
                        dlcs = originDlc.GetGameDlc(game);
                        break;
                }

                if (dlcs?.Count > 0)
                {
                    // Without game name
                    dlcs = dlcs.Where(x => PluginSettings.Settings.IgnoredList.All(y => !x.Name.Contains(y, StringComparison.InvariantCultureIgnoreCase))).ToList();

                    // With game name
                    dlcs = dlcs.Where(x => PluginSettings.Settings.IgnoredList.All(y => !(game.Name + "##" + x.Name).Contains(y, StringComparison.InvariantCultureIgnoreCase))).ToList();
                }
                else
                {
                    dlcs = new List<Dlc>();
                }

                gameDlc.Items = dlcs;
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginName);
            }

            return gameDlc;
        }


        public override void SetThemesResources(Game game)
        {
            GameDlc gameDlc = Get(game, true);

            if (gameDlc == null)
            {
                PluginSettings.Settings.HasData = false;

                return;
            }

            PluginSettings.Settings.HasData = gameDlc.HasData;
        }

        public override void RefreshNoLoader(Guid Id)
        {
            Game game = API.Instance.Database.Games.Get(Id);
            Logger.Info($"RefreshNoLoader({game?.Name} - {game?.Id})");

            GameDlc loadedItem = Get(Id, true);
            GameDlc webItem = GetWeb(Id);
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

        public override void ActionAfterRefresh(GameDlc item)
        {
            Game game = API.Instance.Database.Games.Get(item.Id);
            if ((item?.HasData ?? false) && PluginSettings.Settings.DlcFeature != null)
            {
                if (game.FeatureIds != null)
                {
                    _ = game.FeatureIds.AddMissing(PluginSettings.Settings.DlcFeature.Id);
                }
                else
                {
                    game.FeatureIds = new List<Guid> { PluginSettings.Settings.DlcFeature.Id };
                }
                API.Instance.Database.Games.Update(game);
            }
            else
            {
                if (PluginSettings.Settings.DlcFeature?.Id != null && game.FeatureIds?.Find(x => x == PluginSettings.Settings.DlcFeature?.Id) != null)
                {
                    _ = game.FeatureIds.Remove(PluginSettings.Settings.DlcFeature.Id);
                    API.Instance.Database.Games.Update(game);
                }
            }
        }
    }
}
