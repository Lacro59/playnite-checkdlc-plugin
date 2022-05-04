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
using CommonPluginsStores.Origin;

namespace CheckDlc.Clients
{
    class OriginDlc : GenericDlc
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
                    OriginAPI.ResetIsUserLoggedIn();
                }
            }
        }

        private static OriginApi _OriginAPI;
        private static OriginApi OriginAPI
        {
            get
            {
                if (_OriginAPI == null)
                {
                    _OriginAPI = new OriginApi();
                }
                return _OriginAPI;
            }

            set => _OriginAPI = value;
        }


        public OriginDlc() : base("Origin", CodeLang.GetOriginLang(PluginDatabase.PlayniteApi.ApplicationSettings.Language))
        {
            OriginAPI.SetLanguage(PluginDatabase.PlayniteApi.ApplicationSettings.Language);
        }


        public override List<Dlc> GetGameDlc(Game game)
        {
            logger.Info($"Get dlc for {game.Name} with {ClientName}");
            List<Dlc> GameDlc = new List<Dlc>();

            try
            {
                if (OriginAPI.IsUserLoggedIn)
                {
                    OriginAPI.SetCurrency(PluginDatabase.PluginSettings.Settings.OriginCurrency);

                    ObservableCollection<DlcInfos> dlcs = OriginAPI.GetDlcInfos(game.GameId, OriginAPI.CurrentAccountInfos);
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
