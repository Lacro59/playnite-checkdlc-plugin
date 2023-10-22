using System;
using System.Collections.Generic;
using CheckDlc.Models;
using Playnite.SDK.Models;
using CommonPluginsShared;
using static CommonPluginsShared.PlayniteTools;
using CommonPluginsStores.Steam;
using System.Collections.ObjectModel;
using CommonPluginsStores.Models;

namespace CheckDlc.Clients
{
    class SteamDlc : GenericDlc
    {
        private readonly SteamApi SteamApi = CheckDlc.SteamApi;
        public static bool SettingsOpen { get; set; } = false;


        public SteamDlc() : base("Steam", CodeLang.GetSteamLang(PluginDatabase.PlayniteApi.ApplicationSettings.Language))
        {

        }


        public override List<Dlc> GetGameDlc(Game game)
        {
            logger.Info($"Get dlc for {game.Name} with {ClientName}");
            List<Dlc> GameDlc = new List<Dlc>();

            try
            {
                if (SteamApi.IsUserLoggedIn)
                {
                    ObservableCollection<DlcInfos> dlcs = SteamApi.GetDlcInfos(game.GameId, SteamApi.CurrentAccountInfos);
                    dlcs?.ForEach(x =>
                    {
                        Dlc dlc = new Dlc
                        {
                            DlcId = x.Id,
                            Name = x.Name,
                            Description = x.Description,
                            Image = x.Image,
                            Link = x.Link,
                            IsOwned = x.IsOwned,
                            Price = x.Price,
                            PriceBase = x.PriceBase
                        };

                        GameDlc.Add(dlc);
                    });

                    logger.Info($"Find {GameDlc?.Count} dlc");
                    return GameDlc;
                }
                else
                {
                    ShowNotificationPluginNoAuthenticate(string.Format(resources.GetString("LOCCommonStoresNoAuthenticate"), ClientName), ExternalPlugin.CheckDlc);
                }
            }
            catch(Exception ex)
            {
                ShowNotificationPluginError(ex);
            }

            logger.Info($"Find {GameDlc?.Count} dlc");
            return GameDlc;
        }
    }
}
