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
using Playnite.SDK;

namespace CheckDlc.Clients
{
    public class OriginDlc : GenericDlc
    {
        protected static Lazy<OriginApi> _originApi = new Lazy<OriginApi>(() => new OriginApi(PluginDatabase.PluginName));
        internal static OriginApi OriginApi => _originApi.Value;

        private static bool _settingsOpen = false;
        public static bool SettingsOpen
        {
            get => _settingsOpen;

            set
            {
                _settingsOpen = value;
                if (_settingsOpen)
                {
                    OriginApi?.ResetIsUserLoggedIn();
                }
            }
        }


        public OriginDlc() : base("Origin", CodeLang.GetOriginLang(API.Instance.ApplicationSettings.Language))
        {
            OriginApi.SetLanguage(API.Instance.ApplicationSettings.Language);
        }


        public override List<Dlc> GetGameDlc(Game game)
        {
            Logger.Info($"Get dlc for {game.Name} with {ClientName}");
            List<Dlc> GameDlc = PluginDatabase.Get(game)?.Items ?? new List<Dlc>();

            try
            {
                if (OriginApi.IsUserLoggedIn)
                {
                    OriginApi.SetCurrency(PluginDatabase.PluginSettings.Settings.OriginCurrency);

                    ObservableCollection<DlcInfos> dlcs = OriginApi.GetDlcInfos(game.GameId, OriginApi.CurrentAccountInfos);
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
