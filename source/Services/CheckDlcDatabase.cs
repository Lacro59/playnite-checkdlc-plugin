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
using System.Threading;

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


        public override GameDlc Get(Guid id, bool onlyCache = false, bool force = false)
        {
            GameDlc gameDlc = base.GetOnlyCache(id);

            // Get from web
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
                //Thread.Sleep(100);
                List<Dlc> dlcs = new List<Dlc>();
                ExternalPlugin pluginType = PlayniteTools.GetPluginType(game.PluginId);
                switch (pluginType)
                {
                    case ExternalPlugin.SteamLibrary:
                        if (PluginSettings.Settings.PluginState.SteamIsEnabled)
                        {
                            SteamDlc steamDlc = new SteamDlc();
                            dlcs = steamDlc.GetGameDlc(game);
                        }
                        break;

                    case ExternalPlugin.GogLibrary:
                    case ExternalPlugin.GogOssLibrary:
                        if (PluginSettings.Settings.PluginState.GogIsEnabled)
                        {
                            GogDlc gogDlc = new GogDlc();
                            dlcs = gogDlc.GetGameDlc(game);
                        }
                        break;

                    case ExternalPlugin.EpicLibrary:
                    case ExternalPlugin.LegendaryLibrary:
                        if (PluginSettings.Settings.PluginState.EpicIsEnabled)
                        {
                            EpicDlc epicDlc = new EpicDlc();
                            dlcs = epicDlc.GetGameDlc(game);
                        }
                        break;


                    case ExternalPlugin.OriginLibrary:
                        if (PluginSettings.Settings.PluginState.OriginIsEnabled)
                        {
                            OriginDlc originDlc = new OriginDlc();
                            dlcs = originDlc.GetGameDlc(game);
                        }
                        break;

                    case ExternalPlugin.PSNLibrary:
                        if (PluginSettings.Settings.PluginState.PsnIsEnabled)
                        {
                            PsnDlc psnDlc = new PsnDlc();
                            dlcs = psnDlc.GetGameDlc(game);
                        }
                        break;

                    case ExternalPlugin.NintendoLibrary:
                        if (PluginSettings.Settings.PluginState.NintendosEnabled)
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
            PluginSettings.Settings.HasData = gameDlc?.HasData ?? false;
            PluginSettings.Settings.ListDlcs = new List<Dlc>();

            if (PluginSettings.Settings.HasData)
            {
                PluginSettings.Settings.ListDlcs = gameDlc.Items;
            }
        }

        public override void RefreshNoLoader(Guid id)
        {
            Game game = API.Instance.Database.Games.Get(id);
            Logger.Info($"RefreshNoLoader({game?.Name} - {game?.Id})");

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


        public override void AddTag(Game game)
        {
            GameDlc item = Get(game, true);
            if (item.HasData)
            {
                try
                {
                    Guid? TagId = FindGoodPluginTags(string.Empty);
                    if (TagId != null)
                    {
                        if (game.TagIds != null)
                        {
                            game.TagIds.Add((Guid)TagId);
                        }
                        else
                        {
                            game.TagIds = new List<Guid> { (Guid)TagId };
                        }
                    }

                    if (PluginSettings.Settings.EnableTagAllDlc && item.HasAllDlc)
                    {
                        TagId = FindGoodPluginTags("100%");
                        if (TagId != null)
                        {
                            game.TagIds.Add((Guid)TagId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, $"Tag insert error with {game.Name}", true, PluginName, string.Format(ResourceProvider.GetString("LOCCommonNotificationTagError"), game.Name));
                    return;
                }
            }
            else if (TagMissing)
            {
                if (game.TagIds != null)
                {
                    game.TagIds.Add((Guid)AddNoDataTag());
                }
                else
                {
                    game.TagIds = new List<Guid> { (Guid)AddNoDataTag() };
                }
            }

            API.Instance.MainView.UIDispatcher?.Invoke(() =>
            {
                API.Instance.Database.Games.Update(game);
                game.OnPropertyChanged();
            });
        }



        internal override string GetCsvData(GlobalProgressActionArgs a, bool minimum)
        {
            string csvData = string.Empty;
            Database.Items?.ForEach(x =>
            {
                // Header
                if (csvData.IsNullOrEmpty())
                {
                    csvData = "\"Game name\";\"Platform\";\"Dlc name\";\"Price\";\"Is owned\";\"Is owned manually\";\"Is hidden\";\"Dlc link\";\"Is manual added\";";
                }

                x.Value.Items.ForEach(y =>
                {
                    if (a.CancelToken.IsCancellationRequested)
                    {
                        return;
                    }

                    a.Text = $"{PluginName} - {ResourceProvider.GetString("LOCCommonExtracting")}"
                        + "\n\n" + $"{a.CurrentProgressValue}/{a.ProgressMaxValue}"
                        + "\n" + x.Value.Game?.Name + (x.Value.Game?.Source == null ? string.Empty : $" ({x.Value.Game?.Source.Name})");

                    csvData += Environment.NewLine;
                    csvData += $"\"{x.Value.Name}\";\"{x.Value.Source?.Name ?? x.Value.Platforms?.First()?.Name ?? "Playnite"}\";\"{y.Name}\";\"{y.Price}\";\"{(y.IsOwned ? "X" : string.Empty)}\";\"{(y.IsManualOwned ? "X" : string.Empty)}\";\"{(y.IsHidden ? "X" : string.Empty)}\";\"{y.Link}\";\"{(x.Value.IsManual ? "X" : string.Empty)}\";";

                    a.CurrentProgressValue++;
                });
            });
            return csvData;
        }
    }
}
