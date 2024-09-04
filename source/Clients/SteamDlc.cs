using System;
using System.Collections.Generic;
using CheckDlc.Models;
using Playnite.SDK.Models;
using CommonPluginsShared;
using static CommonPluginsShared.PlayniteTools;
using CommonPluginsStores.Steam;
using System.Collections.ObjectModel;
using CommonPluginsStores.Models;
using Playnite.SDK;

namespace CheckDlc.Clients
{
    public class SteamDlc : GenericDlc
    {
        private static SteamApi SteamApi => CheckDlc.SteamApi;

        private static bool settingsOpen = false;
        public static bool SettingsOpen
        {
            get => settingsOpen;

            set
            {
                settingsOpen = value;
                if (settingsOpen)
                {
                    SteamApi?.ResetIsUserLoggedIn();
                }
            }
        }


        public SteamDlc() : base("Steam", CodeLang.GetSteamLang(API.Instance.ApplicationSettings.Language))
        {

        }


        public override List<Dlc> GetGameDlc(Game game)
        {
            Logger.Info($"Get dlc for {game.Name} with {ClientName}");
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

                    Logger.Info($"Find {GameDlc?.Count} dlc");
                    return GameDlc;
                }
                else
                {
                    ShowNotificationPluginNoAuthenticate(string.Format(ResourceProvider.GetString("LOCCommonStoresNoAuthenticate"), ClientName), ExternalPlugin.CheckDlc);
                }
            }
            catch(Exception ex)
            {
                ShowNotificationPluginError(ex);
            }

            Logger.Info($"Find {GameDlc?.Count} dlc");
            return GameDlc;
        }
    }
}
