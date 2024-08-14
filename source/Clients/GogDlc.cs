using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CheckDlc.Models;
using Playnite.SDK.Models;
using CommonPluginsShared;
using static CommonPluginsShared.PlayniteTools;
using CommonPluginsStores.Gog;
using System.Collections.ObjectModel;
using CommonPluginsStores.Models;
using Playnite.SDK;

namespace CheckDlc.Clients
{
    public class GogDlc : GenericDlc
    {
        private static bool settingsOpen = false;
        public static bool SettingsOpen
        {
            get => settingsOpen;

            set
            {
                settingsOpen = value;
                if (settingsOpen)
                {
                    GogApi.ResetIsUserLoggedIn();
                }
            }
        }

        private static GogApi gogApi;
        private static GogApi GogApi
        {
            get
            {
                if (gogApi == null)
                {
                    gogApi = new GogApi(PluginDatabase.PluginName);
                }
                return gogApi;
            }

            set => gogApi = value;
        }


        public GogDlc() : base("GOG", CodeLang.GetGogLang(API.Instance.ApplicationSettings.Language))
        {
            GogApi.SetLanguage(API.Instance.ApplicationSettings.Language);
        }


        public override List<Dlc> GetGameDlc(Game game)
        {
            Logger.Info($"Get dlc for {game.Name} with {ClientName}");
            List<Dlc> GameDlc = new List<Dlc>();

            try
            {
                if (GogApi.IsUserLoggedIn)
                {
                    GogApi.SetCurrency(PluginDatabase.PluginSettings.Settings.GogCurrency);

                    ObservableCollection<DlcInfos> dlcs = GogApi.GetDlcInfos(game.GameId, GogApi.CurrentAccountInfos);
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
                    ShowNotificationPluginNoAuthenticate(string.Format(ResourceProvider.GetString("LOCCommonStoresNoAuthenticate"), ClientName), ExternalPlugin.GogLibrary);
                }
            }
            catch (Exception ex)
            {
                ShowNotificationPluginError(ex);
            }

            return null;
        }
    }
}
