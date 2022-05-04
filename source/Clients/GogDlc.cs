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

namespace CheckDlc.Clients
{
    class GogDlc : GenericDlc
    {
        private static bool _SettingsOpen = false;
        public static bool SettingsOpen
        {
            get => _SettingsOpen;

            set
            {
                _SettingsOpen = value;
                if (_SettingsOpen)
                {
                    GogAPI.ResetIsUserLoggedIn();
                }
            }
        }

        private static GogApi _GogAPI;
        private static GogApi GogAPI
        {
            get
            {
                if (_GogAPI == null)
                {
                    _GogAPI = new GogApi();
                }
                return _GogAPI;
            }

            set
            {
                _GogAPI = value;
            }
        }


        public GogDlc() : base("GOG", CodeLang.GetGogLang(PluginDatabase.PlayniteApi.ApplicationSettings.Language))
        {
            GogAPI.SetLanguage(PluginDatabase.PlayniteApi.ApplicationSettings.Language);
        }


        public override List<Dlc> GetGameDlc(Game game)
        {
            logger.Info($"Get dlc for {game.Name} with {ClientName}");
            List<Dlc> GameDlc = new List<Dlc>();

            try
            {
                if (GogAPI.IsUserLoggedIn)
                {
                    GogAPI.SetCurrency(PluginDatabase.PluginSettings.Settings.GogCurrency);

                    ObservableCollection<DlcInfos> dlcs = GogAPI.GetDlcInfos(game.GameId, GogAPI.CurrentAccountInfos);
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
                    ShowNotificationPluginNoAuthenticate(string.Format(resources.GetString("LOCCommonStoresNoAuthenticate"), ClientName), ExternalPlugin.GogLibrary);
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
