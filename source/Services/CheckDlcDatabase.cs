﻿using CheckDlc.Clients;
using CheckDlc.Models;
using CommonPluginsShared;
using CommonPluginsShared.Collections;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using static CommonPluginsShared.PlayniteTools;

namespace CheckDlc.Services
{
    public class CheckDlcDatabase : PluginDatabaseObject<CheckDlcSettingsViewModel, CheckDlcCollection, GameDlc>
    {
        public bool SettingsOpen = false;


        public CheckDlcDatabase(IPlayniteAPI PlayniteApi, CheckDlcSettingsViewModel PluginSettings, string PluginUserDataPath) : base(PlayniteApi, PluginSettings, "CheckDlc", PluginUserDataPath)
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
                Database.SetGameInfo<Dlc>(PlayniteApi);

                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;
                logger.Info($"LoadDatabase with {Database.Count} items - {string.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10)}");
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
                Game game = PlayniteApi.Database.Games.Get(Id);
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
            Game game = PlayniteApi.Database.Games.Get(Id);
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
                }

                gameDlc.Items = dlcs;
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, "CheckDlc");
            }

            return gameDlc;
        }


        public override void Games_ItemUpdated(object sender, ItemUpdatedEventArgs<Game> e)
        {
            foreach (var GameUpdated in e.UpdatedItems)
            {
                Database.SetGameInfo<Dlc>(PlayniteApi, GameUpdated.NewData.Id);
            }
        }
    }
}
