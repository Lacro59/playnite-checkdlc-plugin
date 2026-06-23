using CheckDlc.Models;
using CommonPluginsShared;
using CommonPluginsStores.Ea;
using CommonPluginsStores.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static CommonPluginsShared.PlayniteTools;

namespace CheckDlc.Clients
{
    public class OriginDlc : GenericDlc
    {
        protected static Lazy<EaApi> _eaApi = new Lazy<EaApi>(() => new EaApi(PluginDatabase.PluginName));
        internal static EaApi EaApi => _eaApi.Value;

        private static bool _settingsOpen = false;
        public static bool SettingsOpen
        {
            get => _settingsOpen;

            set
            {
                _settingsOpen = value;
                if (_settingsOpen)
                {
                    EaApi?.ResetIsUserLoggedIn();
                }
            }
        }


        public OriginDlc() : base("EA", CodeLang.GetEaLang(API.Instance.ApplicationSettings.Language))
        {
            EaApi.SetLanguage(API.Instance.ApplicationSettings.Language);
        }


        public override List<Dlc> GetGameDlc(Game game)
        {
            Logger.Info($"Get dlc for {game.Name} with {ClientName}");
            List<Dlc> gameDlcs = PluginDatabase.Get(game)?.Items ?? new List<Dlc>();

            try
            {
                if (EaApi.IsUserLoggedIn)
                {
                    List<Dlc> newDlcs = new List<Dlc>();
                    GameInfos gameInfos = EaApi.GetGameInfos(game.GameId, EaApi.CurrentAccountInfos);
                    ObservableCollection<DlcInfos> dlcs = gameInfos?.Dlcs;
                    dlcs?.ForEach(x =>
                    {
                        Dlc dlc = new Dlc
                        {
                            DlcId = x.Id2.IsNullOrEmpty() ? x.Id : x.Id2,
                            Name = x.Name,
                            Description = x.Description,
                            Image = x.Image,
                            Link = x.Link,
                            IsOwned = x.IsOwned,
                            Price = x.Price,
                            PriceBase = x.PriceBase
                        };

                        newDlcs.Add(dlc);
                    });

                    Logger.Info($"Find {newDlcs?.Count} dlc(s)");
                    return newDlcs?.Count > 0 ? newDlcs : gameDlcs;
                }
                else
                {
                    ShowNotificationPluginNoAuthenticate(string.Format(ResourceProvider.GetString("LOCCommonStoresNoAuthenticate"), ClientName), ExternalPlugin.OriginLibrary);
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
